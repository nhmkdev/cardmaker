////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CardMaker.Card;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;
using Support.UI;
using LayoutEventArgs = CardMaker.Events.Args.LayoutEventArgs;

namespace CardMaker.Forms
{
    public partial class MDICanvas : Form
    {
        private const int SELECTION_BUFFER = 3;
        private const int SELECTION_BUFFER_SPACE = SELECTION_BUFFER*2;
        private bool m_bElementSelected;
        private MouseMode m_eMouseMode = MouseMode.Unknown;
        private ResizeDirection m_eResizeDirection = ResizeDirection.Up;
        private ProjectLayoutElement m_zSelectedElement;
        private List<ProjectLayoutElement> m_listSelectedElements;
        private List<Point> m_listSelectedOriginalPosition;
        private List<float> m_listSelectedOriginalRotation;
        private Dictionary<ProjectLayoutElement, ElementPosition> m_dictionarySelectedUndo;
        private Point m_pointOriginalMouseDown = Point.Empty;
        private TranslationLock m_eTranslationLockState = TranslationLock.Unset;
        private readonly ContextMenuStrip m_zContextMenu = new ContextMenuStrip();

        private readonly CardCanvas m_zCardCanvas;
        private float m_fZoom = 1.0f;
        private float m_fZoomRatio = 1.0f;

        private const int TRANSLATION_LOCK_DEAD_ZONE = 3; // 3 pixels of movement

        private enum MouseMode
        {
            Unknown,
            MoveResize,
            Move,
            Rotate,
        }

        private enum TranslationLock
        {
            Unset,
            WaitingToSet,
            Vertical, // lock to vertical only
            Horizontal, // lock to horizontal only
        }

        private enum ElementCenterAlign
        {
            None,
            Layout,
            FirstElement
        }

        private TranslationLock TranslationLockState
        {
            get { return m_eTranslationLockState; }
            set
            {
                // Logger.AddLogLine(m_eTranslationLockState + "=>" + value);
                m_eTranslationLockState = value;
            }
        }

        [Flags]
        private enum ResizeDirection
        {
            None = 0,
            Up = 1 << 0,
            Down = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
            UpLeft = Up | Left,
            UpRight = Up | Right,
            DownLeft = Down | Left,
            DownRight = Down | Right,
            Move = 1 << 8,
        }

        public MDICanvas()
        {
            InitializeComponent();
            var zBitmap = new Bitmap(32, 32);
            Graphics zGraphics = Graphics.FromImage(zBitmap);
            Brush zBlack = new SolidBrush(Color.LightGray);
            Brush zWhite = new SolidBrush(Color.White);
            zGraphics.FillRectangle(zBlack, 0, 0, 16, 16);
            zGraphics.FillRectangle(zWhite, 16, 0, 16, 16);
            zGraphics.FillRectangle(zBlack, 16, 16, 16, 16);
            zGraphics.FillRectangle(zWhite, 0, 16, 16, 16);
            panelCardCanvas.BackgroundImage = zBitmap;
            CardMakerInstance.CanvasUserAction = false;
            // m_zCardCanvas is a panel within the panelCardCanvas
            m_zCardCanvas = new CardCanvas
            {
                Location = new Point(0, 0),
            };
            ChangeMouseMode(MouseMode.MoveResize);
            m_zCardCanvas.MouseMove += cardCanvas_MouseMove;
            m_zCardCanvas.MouseDown += cardCanvas_MouseDown;
            m_zCardCanvas.MouseUp += cardCanvas_MouseUp;
            m_zCardCanvas.ContextMenuStrip = m_zContextMenu;
            m_zContextMenu.RenderMode = ToolStripRenderMode.System;
            m_zContextMenu.Opening += contextmenuOpening_Handler;

            panelCardCanvas.Controls.Add(m_zCardCanvas);

            LayoutManager.Instance.LayoutUpdated += Layout_Updated;
            LayoutManager.Instance.LayoutLoaded += Layout_Loaded;
            LayoutManager.Instance.LayoutRenderUpdated += LayoutRender_Updated;
            LayoutManager.Instance.DeckIndexChanged += DeckIndex_Changed;
            ElementManager.Instance.ElementSelected += Element_Selected;
            ProjectManager.Instance.ProjectOpened += Project_Opened;

            verticalCenterButton.Image = Properties.Resources.VerticalAlign.ToBitmap();
            customVerticalAlignButton.Image = Properties.Resources.VerticalCustomAlign.ToBitmap();
            horizontalCenterButton.Image = Properties.Resources.HorizontalAlign.ToBitmap();
            customHoritonalAlignButton.Image = Properties.Resources.HorizontalCustomAlign.ToBitmap();
            customAlignButton.Image = Properties.Resources.CustomAlign.ToBitmap();

        }

        #region Form Overrides

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!numericUpDownZoom.Focused)
            {
                int nHChange = 0;
                int nVChange = 0;
                // NOTE: this method only detects keydown events
                switch (keyData)
                {
                    case Keys.Control | Keys.Add:
                        numericUpDownZoom.Value = Math.Min(numericUpDownZoom.Maximum,
                            numericUpDownZoom.Value + numericUpDownZoom.Increment);
                        break;
                    case Keys.Control | Keys.Subtract:
                        numericUpDownZoom.Value = Math.Max(numericUpDownZoom.Minimum,
                            numericUpDownZoom.Value - numericUpDownZoom.Increment);
                        break;
                    // focus is taken by the MDICanvas, reset it after each change below & reset the translation lock
                    case Keys.Shift | Keys.Up:
                        LayoutManager.Instance.FireElementOrderAdjustRequest(-1);
                        TranslationLockState = TranslationLock.Unset;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.Shift | Keys.Down:
                        LayoutManager.Instance.FireElementOrderAdjustRequest(1);
                        TranslationLockState = TranslationLock.Unset;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.Control | Keys.Up:
                        LayoutManager.Instance.FireElementSelectAdjustRequest(-1);
                        TranslationLockState = TranslationLock.Unset;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.Control | Keys.Down:
                        LayoutManager.Instance.FireElementSelectAdjustRequest(1);
                        TranslationLockState = TranslationLock.Unset;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.ShiftKey | Keys.Shift:
                        if (TranslationLock.Unset == TranslationLockState)
                        {
                            TranslationLockState = TranslationLock.WaitingToSet;
                        }
                        break;
                    case Keys.Up:
                        nVChange = -1;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.Down:
                        nVChange = 1;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.Left:
                        nHChange = -1;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.Right:
                        nHChange = 1;
                        m_zCardCanvas.Focus();
                        break;
                    case Keys.M:
                        ChangeMouseMode(MouseMode.Move == m_eMouseMode
                            ? MouseMode.MoveResize
                            : MouseMode.Move);
                        break;
                    case Keys.R:
                        ChangeMouseMode(MouseMode.Rotate == m_eMouseMode
                            ? MouseMode.MoveResize
                            : MouseMode.Rotate);
                        break;
                }
                if (CheckAllSelectedElementsEnabled(false))
                {
                    ElementManager.Instance.ProcessSelectedElementsChange(nHChange, nVChange, 0, 0);
                }
                else
                {
                    Logger.AddLogLine("You cannot adjust disabled elements!");
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CP_NOCLOSE_BUTTON = 0x200;
                CreateParams zParams = base.CreateParams;
                zParams.ClassStyle = zParams.ClassStyle | CP_NOCLOSE_BUTTON;
                return zParams;
            }
        }

        #endregion

        #region Manager Events

        void Element_Selected(object sender, ElementEventArgs args)
        {
            Redraw();
        }

        void DeckIndex_Changed(object sender, DeckChangeEventArgs args)
        {
            Redraw();
        }

        void Layout_Updated(object sender, LayoutEventArgs args)
        {
            // pass the loaded deck into the renderer
            m_zCardCanvas.Reset(args.Deck);
            Redraw();
        }

        void Layout_Loaded(object sender, LayoutEventArgs args)
        {
            // pass the loaded deck into the renderer
            m_zCardCanvas.Reset(args.Deck);
            Redraw();
        }

        void LayoutRender_Updated(object sender, LayoutEventArgs args)
        {
            Redraw();
        }

        void Project_Opened(object sender, ProjectEventArgs e)
        {
            m_zCardCanvas.Reset(null);
            Redraw();
        }

        #endregion

        #region Form Events

        private void contextmenuOpening_Handler(object sender, CancelEventArgs e)
        {
            m_zContextMenu.Items.Clear();
            if (null != LayoutManager.Instance.ActiveDeck.CardLayout.Element)
            {
                Point pointMouse = m_zCardCanvas.PointToClient(MousePosition);
                // add only the items that the mouse is within the rectangle of
                foreach (ProjectLayoutElement zElement in LayoutManager.Instance.ActiveDeck.CardLayout.Element)
                {
                    if (!zElement.enabled)
                    {
                        continue;
                    }
                    var zRect = new RectangleF(zElement.x*m_fZoom, zElement.y*m_fZoom, zElement.width*m_fZoom,
                        zElement.height*m_fZoom);
                    if (zRect.Contains(pointMouse))
                    {
                        var zItem = m_zContextMenu.Items.Add(zElement.name, null, contextmenuClick_Handler);
                        zItem.Tag = zElement;
                    }
                }
            }
            if (1 == m_zContextMenu.Items.Count)
            {
                // auto-select if there's only one!
                contextmenuClick_Handler(m_zContextMenu.Items[0], new EventArgs());
                e.Cancel = true;
            }
            if (0 == m_zContextMenu.Items.Count)
            {
                e.Cancel = true;
            }
        }

        private void contextmenuClick_Handler(object sender, EventArgs e)
        {
            ElementManager.Instance.FireElementSelectRequestedEvent((ProjectLayoutElement)((ToolStripItem) sender).Tag);
        }

        private void cardCanvas_MouseMove(object sender, MouseEventArgs e)
        {
#warning this would be better with event handler subscribing/unsubscribing (didn't work so well with the 60 seconds I invested in trying it)
            switch (m_eMouseMode)
            {
                case MouseMode.Rotate:
                    cardCanvas_MouseMoveRotateMode(sender, e);
                    break;
                case MouseMode.Move:
                case MouseMode.MoveResize:
                    cardCanvas_MouseMoveGeneralMode(sender, e);
                    break;
            }
        }

        private void cardCanvas_MouseMoveRotateMode(object sender, MouseEventArgs e)
        {
#warning this is a really bad place for this... maybe switch the mouse handler if in rotate only mode
            if (m_eMouseMode == MouseMode.Rotate)
            {
                if (MouseButtons.Left == e.Button)
                {
                    int nYDiff = e.Y - m_pointOriginalMouseDown.Y;
                    var nNewRotation = (int) ((float) nYDiff/m_fZoom);
                    if (null != m_listSelectedElements)
                    {
                        int nIdx = 0;
                        foreach (var selectedElement in m_listSelectedElements)
                        {
                            selectedElement.rotation = nNewRotation + m_listSelectedOriginalRotation[nIdx];
                            nIdx++;
                        }
                    }

                    if (nNewRotation != 0)
                    {
                        ElementManager.Instance.FireElementBoundsUpdateEvent();
                    }

                }
            }
        }

        private void cardCanvas_MouseMoveGeneralMode(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                if (m_bElementSelected)
                {
                    int nX = e.X;
                    int nY = e.Y;
                    var bElementBoundsChanged = false;
                    if (Cursor == Cursors.SizeAll)
                    {
                        if (TranslationLock.WaitingToSet == TranslationLockState)
                        {
                            // setup the lock (if outside the dead zone)
                            int nXDiff = Math.Abs(nX - m_pointOriginalMouseDown.X);
                            int nYDiff = Math.Abs(nY - m_pointOriginalMouseDown.Y);
                            if (nXDiff > TRANSLATION_LOCK_DEAD_ZONE || nYDiff > TRANSLATION_LOCK_DEAD_ZONE)
                            {
                                TranslationLockState = nXDiff > nYDiff
                                    ? TranslationLock.Horizontal
                                    : TranslationLock.Vertical;
                            }
                        }

                        switch (TranslationLockState)
                        {
                            case TranslationLock.Horizontal:
                                nY = m_pointOriginalMouseDown.Y;
                                break;
                            case TranslationLock.Vertical:
                                nX = m_pointOriginalMouseDown.X;
                                break;
                        }

                        var nXUnzoomed = (int) ((float) nX*m_fZoomRatio);
                        var nYUnzoomed = (int) ((float) nY*m_fZoomRatio);

                        if (null != m_listSelectedElements)
                        {
                            int idx = 0;
                            foreach (var selectedElement in m_listSelectedElements)
                            {
                                selectedElement.x = nXUnzoomed - m_listSelectedOriginalPosition[idx].X;
                                selectedElement.y = nYUnzoomed - m_listSelectedOriginalPosition[idx].Y;
                                idx++;
                            }
                        }

                        bElementBoundsChanged = true;
                    }
                    else if (Cursor == Cursors.SizeNS)
                    {
                        switch (m_eResizeDirection)
                        {
                            case ResizeDirection.Up:
                                bElementBoundsChanged |= ResizeUp(nY);
                                break;
                            case ResizeDirection.Down:
                                bElementBoundsChanged |= ResizeDown(nY);
                                break;
                        }
                    }
                    else if (Cursor == Cursors.SizeWE)
                    {
                        switch (m_eResizeDirection)
                        {
                            case ResizeDirection.Left:
                                bElementBoundsChanged |= ResizeLeft(nX);
                                break;
                            case ResizeDirection.Right:
                                bElementBoundsChanged |= ResizeRight(nX);
                                break;
                        }
                    }
                    else if (Cursor == Cursors.SizeNESW)
                    {
                        switch (m_eResizeDirection)
                        {
                            case ResizeDirection.UpRight:
                                bElementBoundsChanged |= ResizeUp(nY);
                                bElementBoundsChanged |= ResizeRight(nX);
                                break;
                            case ResizeDirection.DownLeft:
                                bElementBoundsChanged |= ResizeDown(nY);
                                bElementBoundsChanged |= ResizeLeft(nX);
                                break;
                        }
                    }
                    else if (Cursor == Cursors.SizeNWSE)
                    {
                        switch (m_eResizeDirection)
                        {
                            case ResizeDirection.UpLeft:
                                bElementBoundsChanged |= ResizeUp(nY);
                                bElementBoundsChanged |= ResizeLeft(nX);
                                break;
                            case ResizeDirection.DownRight:
                                bElementBoundsChanged |= ResizeDown(nY);
                                bElementBoundsChanged |= ResizeRight(nX);
                                break;
                        }
                    }

                    if (bElementBoundsChanged)
                    {
                        ElementManager.Instance.FireElementBoundsUpdateEvent();
                    }

                }
            }
            if (MouseButtons.None == e.Button)
            {
                var zElement = ElementManager.Instance.GetSelectedElement();
                if (null != zElement)
                {
                    m_eResizeDirection = ResizeDirection.None;

                    var nX = e.X;
                    var nY = e.Y;

                    var nEdgeRight = (int) (zElement.x*m_fZoom + zElement.width*m_fZoom + SELECTION_BUFFER);
                    var nEdgeLeft = (int) (zElement.x*m_fZoom - SELECTION_BUFFER);
                    var nEdgeTop = (int) (zElement.y*m_fZoom - SELECTION_BUFFER);
                    var nEdgeBottom = (int) (zElement.y*m_fZoom + zElement.height*m_fZoom + SELECTION_BUFFER);

                    // first verify the cursor is within the SELECTION_BUFFER sized area
                    if ((nX >= (nEdgeLeft)) && (nX <= (nEdgeRight)) &&
                        (nY >= (nEdgeTop)) && (nY <= (nEdgeBottom)))
                    {

                        if (SELECTION_BUFFER_SPACE >= (nX - nEdgeLeft))
                        {
                            m_eResizeDirection |= ResizeDirection.Left;
                        }

                        if (SELECTION_BUFFER_SPACE >= (nEdgeRight - nX))
                        {
                            m_eResizeDirection |= ResizeDirection.Right;
                        }

                        if (SELECTION_BUFFER_SPACE >= (nY - nEdgeTop))
                        {
                            m_eResizeDirection |= ResizeDirection.Up;
                        }

                        if (SELECTION_BUFFER_SPACE >= (nEdgeBottom - nY))
                        {
                            m_eResizeDirection |= ResizeDirection.Down;
                        }

                        if (MouseMode.Move == m_eMouseMode)
                            m_eResizeDirection = ResizeDirection.None;

                        switch (m_eResizeDirection)
                        {
                            case ResizeDirection.Down:
                            case ResizeDirection.Up:
                                Cursor = Cursors.SizeNS;
                                break;
                            case ResizeDirection.Left:
                            case ResizeDirection.Right:
                                Cursor = Cursors.SizeWE;
                                break;
                            case ResizeDirection.UpLeft:
                            case ResizeDirection.DownRight:
                                Cursor = Cursors.SizeNWSE;
                                break;
                            case ResizeDirection.UpRight:
                            case ResizeDirection.DownLeft:
                                Cursor = Cursors.SizeNESW;
                                break;
                            case ResizeDirection.None:
                                // if no edge was found to be selected it's a move
                                Cursor = Cursors.SizeAll;
                                m_eResizeDirection = ResizeDirection.Move;
                                break;
                        }
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }
            if (MouseButtons.Middle == e.Button)
            {
                var nXDiff = m_pointOriginalMouseDown.X - e.X;
                var nYDiff = m_pointOriginalMouseDown.Y - e.Y;
                panelCardCanvas.ScrollToXY(
                    panelCardCanvas.AutoScrollPosition.X - nXDiff,
                    panelCardCanvas.AutoScrollPosition.Y - nYDiff);
                // after the panel adjust the original mouse down has to be adjusted! Get its position based on the canvas itself
                m_pointOriginalMouseDown = m_zCardCanvas.PointToClient(Cursor.Position);
            }
        }

        private void cardCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            CardMakerInstance.CanvasUserAction = false;
            if (null != m_listSelectedElements && m_bElementSelected)
            {
               ElementManager.Instance.ConfigureUserAction(m_dictionarySelectedUndo, ElementManager.Instance.GetUndoRedoPoints());
            }

            m_dictionarySelectedUndo = null;
            m_listSelectedElements = null;
            m_zSelectedElement = null;
            m_bElementSelected = false;
            TranslationLockState = TranslationLock.Unset;
        }

        private void cardCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                if (!CheckAllSelectedElementsEnabled(true)) return;

                ProjectLayoutElement zElement = ElementManager.Instance.GetSelectedElement();
                if (null != zElement)
                {
                    CardMakerInstance.CanvasUserAction = true;

                    m_pointOriginalMouseDown = e.Location;
                    if (TranslationLock.WaitingToSet != TranslationLockState)
                    {
                        TranslationLockState = TranslationLock.Unset;
                    }

                    int nX = e.X;
                    int nY = e.Y;

                    if ((nX >= (int) (zElement.x*m_fZoom - SELECTION_BUFFER) &&
                         nX <= (int) (zElement.x*m_fZoom + zElement.width*m_fZoom + SELECTION_BUFFER)) &&
                        (nY >= (int) (zElement.y*m_fZoom - SELECTION_BUFFER) &&
                         nY <= (int) (zElement.y*m_fZoom + zElement.height*m_fZoom + SELECTION_BUFFER))
                        || m_eMouseMode == MouseMode.Rotate)
                    {
                        // Setup the start position and allow movement
                        var nXUnzoomed = (int) ((float) nX*m_fZoomRatio);
                        var nYUnzoomed = (int) ((float) nY*m_fZoomRatio);
                        // store the offset in "normal" size
                        m_bElementSelected = true;
                        m_zSelectedElement = zElement;

                        m_listSelectedElements = ElementManager.Instance.SelectedElements;
                        if (null != m_listSelectedElements)
                        {
                            m_listSelectedOriginalPosition = new List<Point>();
                            m_listSelectedOriginalRotation = new List<float>();
                            foreach (var zSelectedElement in m_listSelectedElements)
                            {
                                m_listSelectedOriginalPosition.Add(new Point(
                                    nXUnzoomed - zSelectedElement.x,
                                    nYUnzoomed - zSelectedElement.y));
                                m_listSelectedOriginalRotation.Add(zSelectedElement.rotation);
                            }
                            // setup the undo dictionary (covers all types of changes allowed with canvas mouse movement)
                            m_dictionarySelectedUndo = ElementManager.Instance.GetUndoRedoPoints();
                        }
                    }
                }
            }
            if (MouseButtons.Middle == e.Button)
            {
                //initiate panning
                m_pointOriginalMouseDown = e.Location;
            }
        }

        private void btnFitHorizontalZoom_Click(object sender, EventArgs e)
        {
            if (LayoutManager.Instance.ActiveLayout != null)
            {
                var widthRatio = (decimal)(panelCardCanvas.Width -
                                           20) / (decimal)LayoutManager.Instance.ActiveLayout.width;
                numericUpDownZoom.Value = widthRatio;
            }
        }

        private void btnFitZoom_Click(object sender, EventArgs e)
        {
            if (LayoutManager.Instance.ActiveLayout != null)
            {
                var widthRatio = (decimal) (panelCardCanvas.Width -
                                 20) / (decimal) LayoutManager.Instance.ActiveLayout.width;
                var heightRatio = (decimal)(panelCardCanvas.Height -
                                           20) / (decimal)LayoutManager.Instance.ActiveLayout.height;
                numericUpDownZoom.Value = Math.Min(widthRatio, heightRatio);
            }
        }

        private void numericUpDownZoom_ValueChanged(object sender, EventArgs e)
        {
            if (LayoutManager.Instance.ActiveDeck == null)
            {
                return;
            }
            m_fZoom = (float)numericUpDownZoom.Value;
            m_fZoomRatio = 1.0f / m_fZoom;
            TranslationLockState = TranslationLock.Unset;
            m_zCardCanvas.CardRenderer.ZoomLevel = m_fZoom;
            LayoutManager.Instance.ActiveDeck.ResetDeckCache();
            m_zCardCanvas.UpdateSize();
            m_zCardCanvas.Invalidate();
        }

        private void verticalCenterButton_Click(object sender, EventArgs e)
        {
            if (!CheckAllSelectedElementsEnabled(true)) return;

            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (listSelectedElements == null || listSelectedElements.Count == 0)
            {
                return;
            }
            var zLayout = LayoutManager.Instance.ActiveLayout;
            var dictionaryOriginalPositions = ElementManager.Instance.GetUndoRedoPoints();
            foreach (var zElement in ElementManager.Instance.SelectedElements)
            {
                zElement.y = (zLayout.height - zElement.height) / 2;
            }
            ElementManager.Instance.ConfigureUserAction(dictionaryOriginalPositions,
                ElementManager.Instance.GetUndoRedoPoints());
            ElementManager.Instance.FireElementBoundsUpdateEvent();
        }

        private void horizontalCenterButton_Click(object sender, EventArgs e)
        {
            if (!CheckAllSelectedElementsEnabled(true)) return;

            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (listSelectedElements == null || listSelectedElements.Count == 0)
            {
                return;
            }
            var zLayout = LayoutManager.Instance.ActiveLayout;
            var dictionaryOriginalPositions = ElementManager.Instance.GetUndoRedoPoints();
            foreach (var zElement in ElementManager.Instance.SelectedElements)
            {
                zElement.x = (zLayout.width - zElement.width) / 2;
            }

            ElementManager.Instance.ConfigureUserAction(dictionaryOriginalPositions,
                ElementManager.Instance.GetUndoRedoPoints());
            ElementManager.Instance.FireElementBoundsUpdateEvent();
        }

        private void customAlignElementButton_Click(object sender, EventArgs e)
        {
            if (!CheckAllSelectedElementsEnabled(true)) return;

            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (listSelectedElements == null || listSelectedElements.Count == 0)
            {
                return;
            }

            const string VERTICAL_SPACING = "vertical_spacing";
            const string APPLY_ELEMENT_WIDTHS = "apply_element_widths";
            const string HORIZONTAL_SPACING = "horizontal_spacing";
            const string APPLY_ELEMENT_HEIGHTS = "apply_element_heights";

            var zQuery = new QueryPanelDialog("Custom Align Elements", 450, 150, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);

            zQuery.AddNumericBox("Vertical Pixel Spacing", 0, int.MinValue, int.MaxValue, VERTICAL_SPACING);
            zQuery.AddCheckBox("Include Element Heights", false, APPLY_ELEMENT_HEIGHTS);
            zQuery.AddNumericBox("Horizontal Pixel Spacing", 0, int.MinValue, int.MaxValue, HORIZONTAL_SPACING);
            zQuery.AddCheckBox("Include Element Widths", false, APPLY_ELEMENT_WIDTHS);

            if (DialogResult.OK != zQuery.ShowDialog(CardMakerInstance.ApplicationForm))
            {
                return;
            }

            var nVerticalSpace = (int)zQuery.GetDecimal(VERTICAL_SPACING);
            var nHorizontalSpace = (int)zQuery.GetDecimal(HORIZONTAL_SPACING);
            var bApplyElementWidths = zQuery.GetBool(APPLY_ELEMENT_WIDTHS);
            var bApplyElementHeights = zQuery.GetBool(APPLY_ELEMENT_HEIGHTS);

            var dictionaryOriginalPositions = ElementManager.Instance.GetUndoRedoPoints();

            // apply the translation
            var nX = listSelectedElements[0].x;
            var nY = listSelectedElements[0].y;
            foreach (var zElement in listSelectedElements)
            {
                zElement.x = nX;
                zElement.y = nY;
                nX += bApplyElementWidths ? zElement.width : 0;
                nY += bApplyElementHeights ? zElement.height : 0;
                nX += nHorizontalSpace;
                nY += nVerticalSpace;
            }
            ElementManager.Instance.ConfigureUserAction(dictionaryOriginalPositions,
                ElementManager.Instance.GetUndoRedoPoints());

            ElementManager.Instance.FireElementBoundsUpdateEvent();
        }

        private void customVerticalAlignButton_Click(object sender, EventArgs e)
        {
            if (!CheckAllSelectedElementsEnabled(true)) return;

            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (listSelectedElements == null || listSelectedElements.Count == 0)
            {
                return;
            }

            const string VERTICAL_SPACING = "vertical_spacing";
            const string APPLY_ELEMENT_HEIGHTS = "apply_element_heights";
            const string ELEMENT_CENTERING = "element_centering";

            var zQuery = new QueryPanelDialog("Custom Vertical Align Elements", 450, 150, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);

            zQuery.AddNumericBox("Vertical Pixel Spacing", 0, int.MinValue, int.MaxValue, VERTICAL_SPACING);
            zQuery.AddCheckBox("Include Element Heights", false, APPLY_ELEMENT_HEIGHTS);
            zQuery.AddPullDownBox("Horizontal Centering", Enum.GetNames(typeof(ElementCenterAlign)), 0,
                ELEMENT_CENTERING);

            if (DialogResult.OK != zQuery.ShowDialog(CardMakerInstance.ApplicationForm))
            {
                return;
            }

            var nVerticalSpace = (int)zQuery.GetDecimal(VERTICAL_SPACING);
            var bApplyElementHeights = zQuery.GetBool(APPLY_ELEMENT_HEIGHTS);
            var eCenterAlignment = (ElementCenterAlign)zQuery.GetIndex(ELEMENT_CENTERING);

            // determine the centering
            var nCenterX = 0;
            switch (eCenterAlignment)
            {
                case ElementCenterAlign.FirstElement:
                    nCenterX = listSelectedElements[0].x + (listSelectedElements[0].width / 2);
                    break;
                case ElementCenterAlign.Layout:
                    nCenterX = LayoutManager.Instance.ActiveLayout.width / 2;
                    break;
            }

            // apply the translation
            var dictionaryOriginalPositions = ElementManager.Instance.GetUndoRedoPoints();
            var nY = listSelectedElements[0].y;
            for (var nIdx = 0; nIdx < listSelectedElements.Count; nIdx++)
            {
                var zElement = listSelectedElements[nIdx];
                zElement.y = nY;
                nY += bApplyElementHeights ? zElement.height : 0;
                nY += nVerticalSpace;
                switch (eCenterAlignment)
                {
                    case ElementCenterAlign.FirstElement:
                        if (0 < nIdx)
                        {
                            zElement.x = nCenterX - (zElement.width / 2);
                        }
                        break;
                    case ElementCenterAlign.Layout:
                        zElement.x = nCenterX - (zElement.width / 2);
                        break;
                }
            }
            ElementManager.Instance.ConfigureUserAction(dictionaryOriginalPositions,
                ElementManager.Instance.GetUndoRedoPoints());

            ElementManager.Instance.FireElementBoundsUpdateEvent();
        }

        private void customHoritonalAlignButton_Click(object sender, EventArgs e)
        {
            if (!CheckAllSelectedElementsEnabled(true)) return;

            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (listSelectedElements == null || listSelectedElements.Count == 0)
            {
                return;
            }

            const string HORIZONTAL_SPACING = "horizontal_spacing";
            const string APPLY_ELEMENT_WIDTHS = "apply_element_widths";
            const string ELEMENT_CENTERING = "element_centering";

            var zQuery = new QueryPanelDialog("Custom Horizontal Align Elements", 450, 150, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);

            zQuery.AddNumericBox("Horizontal Pixel Spacing", 0, int.MinValue, int.MaxValue, HORIZONTAL_SPACING);
            zQuery.AddCheckBox("Include Element Widths", false, APPLY_ELEMENT_WIDTHS);
            zQuery.AddPullDownBox("Vertical Centering", Enum.GetNames(typeof(ElementCenterAlign)), 0,
                ELEMENT_CENTERING);


            if (DialogResult.OK != zQuery.ShowDialog(CardMakerInstance.ApplicationForm))
            {
                return;
            }

            var nHorizontalSpace = (int)zQuery.GetDecimal(HORIZONTAL_SPACING);
            var bApplyElementWidths = zQuery.GetBool(APPLY_ELEMENT_WIDTHS);
            var eCenterAlignment = (ElementCenterAlign)zQuery.GetIndex(ELEMENT_CENTERING);

            // determine the centering
            var nCenterY = 0;
            switch (eCenterAlignment)
            {
                case ElementCenterAlign.FirstElement:
                    nCenterY = listSelectedElements[0].y + (listSelectedElements[0].height / 2);
                    break;
                case ElementCenterAlign.Layout:
                    nCenterY = LayoutManager.Instance.ActiveLayout.height / 2;
                    break;
            }

            var dictionaryOriginalPositions = ElementManager.Instance.GetUndoRedoPoints();

            // apply the translation
            var nX = listSelectedElements[0].x;
            for (var nIdx = 0; nIdx < listSelectedElements.Count; nIdx++)
            {
                var zElement = listSelectedElements[nIdx];
                zElement.x = nX;
                nX += bApplyElementWidths ? zElement.width : 0;
                nX += nHorizontalSpace;
                switch (eCenterAlignment)
                {
                    case ElementCenterAlign.FirstElement:
                        if (0 < nIdx)
                        {
                            zElement.y = nCenterY - (zElement.width / 2);
                        }
                        break;
                    case ElementCenterAlign.Layout:
                        zElement.y = nCenterY - (zElement.width / 2);
                        break;
                }
            }
            ElementManager.Instance.ConfigureUserAction(dictionaryOriginalPositions,
                ElementManager.Instance.GetUndoRedoPoints());

            ElementManager.Instance.FireElementBoundsUpdateEvent();
        }

        private void btnToggleGuides_CheckStateChanged(object sender, EventArgs e)
        {
            CardMakerInstance.DrawLayoutDividers = btnToggleDividers.Checked;
            LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
        }

        private void txtHorizontalDividers_TextChanged(object sender, EventArgs e)
        {
            var nValue = 0;
            int.TryParse(txtHorizontalDividers.Text, out nValue);
            CardMakerInstance.LayoutDividerHorizontalCount = nValue;
            if (CardMakerInstance.DrawLayoutDividers)
            {
                LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
            }
        }

        private void txtVerticalDividers_TextChanged(object sender, EventArgs e)
        {
            var nValue = 0;
            int.TryParse(txtVerticalDividers.Text, out nValue);
            CardMakerInstance.LayoutDividerVerticalCount = nValue;
            if (CardMakerInstance.DrawLayoutDividers)
            {
                LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
            }
        }

        private void toolStripButtonReloadReferences_Click(object sender, EventArgs e)
        {
            LayoutManager.Instance.RefreshActiveLayout();
        }

        private void toolStripButtonClearImageCache_Click(object sender, EventArgs e)
        {
            LayoutManager.Instance.ClearImageCache();
        }

        #endregion

        /// <summary>
        /// Invalidates the panel and card canvas
        /// </summary>
        private void Redraw()
        {
            panelCardCanvas.Invalidate();
            m_zCardCanvas.Invalidate();
        }

        private bool ResizeRight(int nX)
        {
            int nWidth = (int)((float)nX * m_fZoomRatio) - m_zSelectedElement.x;
            if (1 <= nWidth)
            {
                m_zSelectedElement.width = nWidth;
                return true;
            }
            return false;
        }

        private bool ResizeLeft(int nX)
        {
            int nWidth = m_zSelectedElement.width + (m_zSelectedElement.x - (int)((float)nX * m_fZoomRatio));
            if ((0 <= nX) && (1 <= nWidth))
            {
                m_zSelectedElement.width = nWidth;
                m_zSelectedElement.x = (int)((float)nX * m_fZoomRatio);
                return true;
            }
            return false;
        }

        private bool ResizeUp(int nY)
        {
            int nHeight = m_zSelectedElement.height + (m_zSelectedElement.y - (int)((float)nY * m_fZoomRatio));
            if ((0 <= nY) && (1 <= nHeight))
            {
                m_zSelectedElement.height = nHeight;
                m_zSelectedElement.y = (int)((float)nY * m_fZoomRatio);
                return true;
            }
            return false;
        }

        private bool ResizeDown(int nY)
        {
            int nHeight = (int)((float)nY * m_fZoomRatio) - m_zSelectedElement.y;
            if (1 <= nHeight)
            {
                m_zSelectedElement.height = nHeight;
                return true;
            }
            return false;
        }

        private void TriggerMouseMoveAtMouseLocation()
        {
            var pLocation = panelCardCanvas.PointToClient(Cursor.Position);
            cardCanvas_MouseMoveGeneralMode(null, new MouseEventArgs(MouseButtons.None, 0, pLocation.X, pLocation.Y, 0));
        }

        private void ChangeMouseMode(MouseMode eDestinationMode)
        {
            if (eDestinationMode == m_eMouseMode)
            {
                return;
            }
            m_eMouseMode = eDestinationMode;

            switch (m_eMouseMode)
            {
                case MouseMode.Move:
                    Text = "Canvas [Mode: Move-only]";
                    TriggerMouseMoveAtMouseLocation();
                    break;
                case MouseMode.MoveResize:
                    Text = "Canvas [Mode: Normal]";
                    TriggerMouseMoveAtMouseLocation();
                    break;
                case MouseMode.Rotate:
                    Text = "Canvas [Mode: Rotate-only]";
                    Cursor = new Cursor(Properties.Resources.RotateCursor.Handle);
                    break;
            }
        }

        private bool CheckAllSelectedElementsEnabled(bool bShowWarning)
        {
            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (null != listSelectedElements)
            {
                if (listSelectedElements.TrueForAll(x => x.enabled))
                {
                    return true;
                }
            }
            if (bShowWarning)
            {
                MessageBox.Show(this, "All element(s) must be enabled.", "Enabled Elements Only", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }
    }
}