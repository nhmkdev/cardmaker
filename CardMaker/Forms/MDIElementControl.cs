////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CardMaker.Card;
using CardMaker.Card.Shapes;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;
using Support.UI;
using Support.Util;

namespace CardMaker.Forms
{
    public partial class MDIElementControl : Form
    {
        private const string SAMPLE_TEXT = "ABCDEF abcdef 1234567890";
        private const float NORMAL_FONT_SIZE = 11f;
        private const float PREVIEW_FONT_SIZE = 14f;

        // mapping controls directly to special functions when a given control is adjusted
        private readonly Dictionary<Control, Action<ProjectLayoutElement>> m_dictionaryControlActions = new Dictionary<Control, Action<ProjectLayoutElement>>();
        
        // mapping controls directly to properties in the ProjectLayoutElement class
        private readonly Dictionary<Control, PropertyInfo> m_dictionaryControlField = new Dictionary<Control, PropertyInfo>();

        private readonly List<FontFamily> m_listFontFamilies = new List<FontFamily>();
        private readonly Dictionary<string, int> dictionaryShapeTypeIndex = new Dictionary<string, int>();

        private readonly ContextMenuStrip m_zContextMenu;

        private readonly Font m_zDefaultPreviewFont =
            FontLoader.GetFont(FontLoader.DefaultFont.FontFamily, PREVIEW_FONT_SIZE, FontStyle.Regular);

        private readonly Font m_zDefaultFont =
            FontLoader.GetFont(FontLoader.DefaultFont.FontFamily, NORMAL_FONT_SIZE, FontStyle.Regular);

        private Dictionary<string, Font> m_zFontDictionary;

        // used to toggle what is drawn in the combo (font name + (optional) sample)
        private bool m_bFontComboOpen = false;

        private bool m_bFireElementChangeEvents = true;
        private bool m_bFireFontChangeEvents = true;

        public MDIElementControl() 
        {
            InitializeComponent();

            btnElementBorderColor.Tag = panelBorderColor;
            btnElementFontColor.Tag = panelFontColor;
            btnElementShapeColor.Tag = panelShapeColor;
            btnElementGraphicColor.Tag = panelGraphicColor;
            btnElementOutlineColor.Tag = panelOutlineColor;
            btnElementBackgroundColor.Tag = panelBackgroundColor;

            // setup the font related items
            var arrayFontFamilies = GetFontFamilies();
            m_zFontDictionary = new Dictionary<string, Font>(arrayFontFamilies.Length);
            foreach (var zFontFamily in arrayFontFamilies)
            {
                if (m_zFontDictionary.ContainsKey(zFontFamily.Name))
                {
                    Logger.AddLogLine("Duplicate font family (ignoring): " + zFontFamily.Name);
                    continue;
                }

                m_listFontFamilies.Add(zFontFamily);
                comboFontName.Items.Add(zFontFamily.Name);
                m_zFontDictionary[zFontFamily.Name] = FontLoader.GetFont(zFontFamily, PREVIEW_FONT_SIZE, FontStyle.Regular);
            }

            // configure all event handling actions
            SetupControlActions();

            CreateControlFieldDictionary();

            comboFontName.SelectedIndex = 0;

            m_zContextMenu = new ContextMenuStrip
            {
                RenderMode = ToolStripRenderMode.System
            };

            LayoutManager.Instance.DeckIndexChanged += DeckIndex_Changed;
            ElementManager.Instance.ElementSelected += Element_Selected;
            ElementManager.Instance.ElementBoundsUpdated += ElementBounds_Updated;
            LayoutManager.Instance.LayoutUpdated += (sender, args) => HandleEnableStates();
        }

        #region overrides

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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.A:
                    txtElementVariable.SelectAll();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region manager events

        void DeckIndex_Changed(object sender, DeckChangeEventArgs args)
        {
            LayoutManager.Instance.ActiveDeck.PopulateListViewWithElementColumns(listViewElementColumns);
            if (LayoutManager.Instance.ActiveLayout.Element == null ||
                LayoutManager.Instance.ActiveLayout.Element.Length == 0)
            {
                HandleEnableStates();
            }
        }

        void ElementBounds_Updated(object sender, ElementEventArgs args)
        {
            if (null != args && args.Elements.Count > 0)
            {
                UpdateElementBoundsControlValues(args.Elements[0]);
            }
        }

        void Element_Selected(object sender, ElementEventArgs args)
        {
            HandleEnableStates();
            if (args.Elements != null && args.Elements.Count > 0)
            {
                UpdateElementValues(args.Elements[0]);
                HandleTypeEnableStates();
            }
            txtElementVariable.AcceptsTab = ProjectManager.Instance.LoadedProjectTranslatorType ==
                                            TranslatorType.JavaScript;
        }

        #endregion

        #region form events

        private void btnElementBrowseImage_Click(object sender, EventArgs e)
        {
            FormUtils.FileOpenHandler("All files (*.*)|*.*", txtElementVariable, true);
        }

        private void comboElementType_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleElementValueChange(sender, e);
            HandleTypeEnableStates();
        }

        private void btnColorMatrix_Click(object sender, EventArgs e)
        {
#warning TODO: create a custom dialog like the RGB dialog and allow for previewing of the color matrix (when valid)
            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (null == listSelectedElements)
            {
                MessageBox.Show(this, "Please select at least one enabled Element.");
                return;
            }

            var zElement = listSelectedElements[0];
            var arrayCurrentMatrixValues = zElement.colormatrix.Split(new char[] { ';' }, StringSplitOptions.None);
            if (arrayCurrentMatrixValues.Length != 25)
            {
                arrayCurrentMatrixValues = Enumerable.Repeat((0.0f).ToString(), 25).ToArray();
            }

            var zQuery =
                FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Configure ColorMatrix", 450, false));
            const string MATRIX = "matrix";
            var zFont = FontLoader.GetFont(FontFamily.GenericMonospace, 12, FontStyle.Regular);
            var zBuilder = new StringBuilder();
            for (var y = 0; y < 5; y++)
            {
                zBuilder.AppendLine(string.Join(";", arrayCurrentMatrixValues, y * 5, 5));
            }
            zQuery.AddMultiLineTextBox("Color Matrix", zBuilder.ToString(), 150, MATRIX).Font = zFont;
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var arrayEntries = zQuery.GetString(MATRIX).Split(new char[] {';','\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
                if (arrayEntries.Length != 25)
                {
                    Logger.AddLogLine("Failed to parse valid color matrix");
                    return;
                }
                else
                {
                    foreach (var sEntry in arrayEntries)
                    {
                        if (!ParseUtil.ParseFloat(sEntry, out var fValue))
                        {
                            Logger.AddLogLine($"Failed to parse float value from color matrix: {sEntry}");
                            return;
                        }
                    }
                }

                // this will trigger a call to HandleElementValueChanged
                txtColorMatrix.Text = string.Join(";", arrayEntries);
            }
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (null == listSelectedElements)
            {
                MessageBox.Show(this, "Please select at least one enabled Element.");
                return;
            }

            var zRGB = new RGBColorSelectDialog();
            var btnClicked = (Button)sender;
            var zPanel = (Panel)btnClicked.Tag;
            zRGB.UpdateColorBox(zPanel.BackColor);

            // create the undo dictionary per element
            var dictionaryElementUndoColors = new Dictionary<ProjectLayoutElement, Color>(listSelectedElements.Count);
            foreach (var zElement in listSelectedElements)
            {
                var zElementToChange = zElement;
                var colorUndo = Color.White;
                if (btnClicked == btnElementBorderColor)
                {
                    colorUndo = zElement.GetElementBorderColor();
                }
                else if (btnClicked == btnElementOutlineColor)
                {
                    colorUndo = zElement.GetElementOutlineColor();
                }
                else if (btnClicked == btnElementBackgroundColor)
                {
                    colorUndo = zElement.GetElementBackgroundColor();
                }
                else if (btnClicked == btnElementFontColor || btnClicked == btnElementShapeColor || btnClicked == btnElementGraphicColor)
                {
                    colorUndo = zElement.GetElementColor();
                }
                dictionaryElementUndoColors.Add(zElementToChange, colorUndo);
            }

            zRGB.PreviewEvent += delegate(object o, Color color)
            {
                foreach (var zElement in listSelectedElements)
                {
                    SetColorValue(btnClicked, color, zElement);
                }
                LayoutManager.Instance.FireLayoutUpdatedEvent(false);
            };

            if (DialogResult.OK != zRGB.ShowDialog())
            {
                // restore the original colors on the elements
                foreach (var zElement in listSelectedElements)
                {
                    if (dictionaryElementUndoColors.TryGetValue(zElement, out var colorOriginal))
                    {
                        SetColorValue(btnClicked, colorOriginal, zElement);
                    }
                }
                LayoutManager.Instance.FireLayoutUpdatedEvent(false);
                return;
            }
            var colorRedo = zRGB.Color;

            var listActions = UserAction.CreateActionList();

            foreach (var zElement in listSelectedElements)
            {
                var zElementToChange = zElement;

                listActions.Add(bRedo =>
                    {
                        if (null != LayoutManager.Instance.ActiveDeck)
                        {
                            LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElementToChange.name);
                        }

                        var colorUndo = Color.White;
                        if (dictionaryElementUndoColors.TryGetValue(zElementToChange, out var colorOriginal))
                        {
                            colorUndo = colorOriginal;
                        }
                        SetColorValue(btnClicked, bRedo ? colorRedo : colorUndo, zElementToChange);
                        UpdatePanelColors(zElementToChange);
                    });
            }

            Action<bool> actionChangeColor = bRedo =>
            {
                listActions.ForEach(action => action(bRedo));
                LayoutManager.Instance.FireLayoutUpdatedEvent(true);
            };
            UserAction.PushAction(actionChangeColor);

            // perform the action as a redo now
            actionChangeColor(true);
        }

        private void btnNullBackgroundColor_Click(object sender, EventArgs e)
        {
            // TODO: this and the above method are 80% the same...
            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (null == listSelectedElements)
            {
                MessageBox.Show(this, "Please select at least one enabled Element.");
                return;
            }

            var btnClicked = (Button)sender;
            var colorRedo = CardMakerConstants.NoColor;

            var listActions = UserAction.CreateActionList();

            foreach (var zElement in listSelectedElements)
            {
                var zElementToChange = zElement;
                var colorUndo = zElementToChange.GetElementBackgroundColor();

                listActions.Add(bRedo =>
                {
                    if (null != LayoutManager.Instance.ActiveDeck)
                    {
                        LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElementToChange.name);
                    }
                    SetColorValue(btnClicked, bRedo ? colorRedo : colorUndo, zElementToChange);
                    UpdatePanelColors(zElementToChange);
                });
            }

            Action<bool> actionChangeColor = bRedo =>
            {
                listActions.ForEach(action => action(bRedo));
                LayoutManager.Instance.FireLayoutUpdatedEvent(true);
            };
            UserAction.PushAction(actionChangeColor);

            // perform the action as a redo now
            actionChangeColor(true);
        }

        private void HandleElementValueChange(object sender, EventArgs e)
        {
            if (null == ElementManager.Instance.GetSelectedElement() ||
                !m_bFireElementChangeEvents ||
                CardMakerInstance.ProcessingUserAction)
            {
                return;
            }

            var listSelectedElements = ElementManager.Instance.SelectedElements;
            if (null != sender && null != listSelectedElements)
            {
                var zControl = (Control)sender;

                var listActions = UserAction.CreateActionList();

                foreach (var zElement in listSelectedElements)
                {
                    object zUndoValue = null;
                    object zRedoValue = null;
                    var zElementToChange = zElement;

                    PopulateUndoRedoValues(zControl, zElementToChange, ref zRedoValue, ref zUndoValue);

                    listActions.Add(bRedo =>
                            AssignValueByControl(zControl, zElementToChange, bRedo ? zRedoValue : zUndoValue, false));
                }

                Action<bool> actionElementChange = bRedo =>
                {
                    CardMakerInstance.ProcessingUserAction = true;
                    listActions.ForEach(action => action(bRedo));
                    CardMakerInstance.ProcessingUserAction = false;
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                };

                UserAction.PushAction(actionElementChange);

                // perform the action as a redo now
                actionElementChange(true);
            }
        }

        private void MDIElementControl_Load(object sender, EventArgs e)
        {
            foreach (var zShape in ShapeManager.ShapeDictionary.Values)
            {
                comboShapeType.Items.Add(zShape);
                dictionaryShapeTypeIndex.Add(zShape.Name, comboShapeType.Items.Count - 1);
            }

            comboShapeType.SelectedIndex = 0;

            for (var nIdx = 0; nIdx < (int)ElementType.End; nIdx++)
            {
                comboElementType.Items.Add(((ElementType)nIdx).ToString());
            }

            for (var nIdx = 0; nIdx < (int)ElementColorType.End; nIdx++)
            {
                comboColorType.Items.Add(((ElementColorType)nIdx).ToString());
            }

            for (var nIdx = 0; nIdx < (int)MirrorType.End; nIdx++)
            {
                comboElementMirror.Items.Add(((MirrorType)nIdx).ToString());
            }
            tabControl.Visible = false;
        }

        private void HandleFontSettingChange(object sender, EventArgs e)
        {
            if (!m_bFireElementChangeEvents ||
                !m_bFireFontChangeEvents ||
                CardMakerInstance.ProcessingUserAction)
            {
                return;
            }

            var listElements = ElementManager.Instance.SelectedElements;
            if (null != listElements)
            {
                var listActions = UserAction.CreateActionList();

                var zControl = (Control)sender;

                foreach (var zElement in listElements)
                {
                    var zElementToChange = zElement;
                    if (!CardMakerInstance.ProcessingUserAction && null != sender)
                    {
                        object zRedoValue = null;
                        object zUndoValue = null;
                        // The current value on the element can be used for an undo
                        if (PopulateUndoRedoValues(zControl, zElementToChange, ref zRedoValue, ref zUndoValue))
                        {
                            listActions.Add(bRedo =>
                            {
                                AssignValueByControl(zControl, zElementToChange, bRedo ? zRedoValue : zUndoValue, false);
                                if (null != LayoutManager.Instance.ActiveDeck)
                                {
                                    LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElementToChange.name);
                                }
                            });
                        }
                        else
                        {
                            FontStyle eFontStyle =
                                (checkBoxBold.Checked ? FontStyle.Bold : FontStyle.Regular) |
                                (checkBoxItalic.Checked ? FontStyle.Italic : FontStyle.Regular) |
                                (checkBoxStrikeout.Checked ? FontStyle.Strikeout : FontStyle.Regular) |
                                (checkBoxUnderline.Checked ? FontStyle.Underline : FontStyle.Regular);

                            // even if this font load fails due to style it should convert over to the valid one
                            var zFont = FontLoader.GetFont(m_listFontFamilies[comboFontName.SelectedIndex], (int)numericFontSize.Value, eFontStyle);

                            var fontRedo = zFont;
                            var fontUndo = zElementToChange.GetElementFont();
                            fontUndo = fontUndo ?? FontLoader.DefaultFont;

                            listActions.Add(bRedo =>
                            {
                                var fontValue = bRedo ? fontRedo : fontUndo;

                                zElementToChange.SetElementFont(fontValue);

                                // only affect the controls if the current selected element matches that of the element to change
                                if (zElementToChange == ElementManager.Instance.GetSelectedElement())
                                {
                                    comboFontName.Text = fontValue.Name;
                                    SetupElementFont(fontValue);
                                }
                                if (null != LayoutManager.Instance.ActiveDeck)
                                {
                                    LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElementToChange.name);
                                }
                            });
                        }
                    }
                }

                Action<bool> actionChangeFont = bRedo =>
                {
                    CardMakerInstance.ProcessingUserAction = true;
                    listActions.ForEach(action => action(bRedo));
                    CardMakerInstance.ProcessingUserAction = false;
                    if (0 < numericLineSpace.Value)
                    {
                        checkFontAutoScale.Checked = false;
                    }
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                };

                UserAction.PushAction(actionChangeFont);

                // perform the action as a redo now
                actionChangeFont(true);

            }
        }

        private void comboFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupElementFont(null);
            HandleFontSettingChange(sender, e);
        }

        private void comboColorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColorMatrix.Enabled = comboColorType.SelectedIndex == (int)ElementColorType.Matrix;
            HandleElementValueChange(sender, e);
        }

        private void comboFontName_DropDownClosed(object sender, EventArgs e)
        {
            m_bFontComboOpen = false;
            comboFontName.Invalidate();
        }

        private void comboFontName_DropDown(object sender, EventArgs e)
        {
            m_bFontComboOpen = true;
        }

        private void comboFontName_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            var sFontName = ((ComboBox)sender).Items[e.Index].ToString();

            // the font color needs to constrast from the background... (critical if selected)
            var zFontBrush = ((e.State & DrawItemState.Selected) == 0)
                ? new SolidBrush(SystemColors.WindowText) 
                : new SolidBrush(SystemColors.Window);

            var zGraphics = e.Graphics;

            var zFontNameFont = m_bFontComboOpen ? m_zDefaultPreviewFont : m_zDefaultFont;
            var zMeasure = zGraphics.MeasureString(sFontName, zFontNameFont);
            var fNameRenderOffsetY = (e.Bounds.Height - zMeasure.Height) / 2f;
            zGraphics.DrawString(sFontName, zFontNameFont, zFontBrush, e.Bounds.X, e.Bounds.Y + fNameRenderOffsetY);

            if (m_bFontComboOpen)
            {
                var zFont = m_zFontDictionary[sFontName] ?? FontLoader.GetFont(FontLoader.DefaultFont.FontFamily, PREVIEW_FONT_SIZE, FontStyle.Regular);
                var fSampleRenderOffsetX = e.Bounds.X + zMeasure.Width;

                zMeasure = zGraphics.MeasureString(SAMPLE_TEXT, zFont);

                var fSampleRenderOffsetY = (e.Bounds.Height - zMeasure.Height) / 2f;
                zGraphics.DrawString(SAMPLE_TEXT, zFont, zFontBrush, fSampleRenderOffsetX, e.Bounds.Y + fSampleRenderOffsetY);
            }
        }


        private void comboShapeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGridShape.SelectedObject = comboShapeType.SelectedItem;
        }

        private void propertyGridShape_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var zShape = (AbstractShape)propertyGridShape.SelectedObject;
            txtElementVariable.Text = zShape.ToCardMakerString();
        }

        private void txtElementVariable_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                        e.SuppressKeyPress = ProjectManager.Instance.LoadedProjectTranslatorType == TranslatorType.Incept;
                    break;
            }
        }

        private void btnSetSizeToImage_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtElementVariable.Text))
            {
                var zElement = ElementManager.Instance.GetSelectedElement();
                if (null == zElement)
                {
                    Logger.AddLogLine("Unable to set element size. Is an element selected?");
                    return;
                }
                var zBmp = ImageCache.LoadCustomImageFromCache(CardPathUtil.getPath(txtElementVariable.Text), zElement);
                if (null == zBmp)
                {
                    // attempt a translated version of the element variable
                    var zElementString = LayoutManager.Instance.ActiveDeck.GetStringFromTranslationCache(zElement.name);
                    if (null != zElementString.String)
                    {
                        zBmp = ImageCache.LoadCustomImageFromCache(zElementString.String, zElement);
                    }
                }
                if (null != zBmp)
                {
                    var nWidth = zElement.width;
                    var nHeight = zElement.height;
                    Action<bool> actionResizeImage = bRedo =>
                    {
                        zElement.width = bRedo ? zBmp.Width : nWidth;
                        zElement.height = bRedo ? zBmp.Height : nHeight;
                        LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                    };
                    UserAction.PushAction(actionResizeImage, true);
                }
            }
        }

        private void listViewElementColumns_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right == e.Button)
            {
                var info = listViewElementColumns.HitTest(e.Location);
                if (null != info.Item)
                {
                    int nColumn = info.Item.SubItems.IndexOf(info.SubItem);
                    if (-1 != nColumn)
                    {
                        string columnText = listViewElementColumns.Columns[nColumn].Text;
                        m_zContextMenu.Items.Clear();
                        m_zContextMenu.Items.Add("Add Reference to [" + columnText + "] column", null,
                            (osender, ea) =>
                            {
                                // TODO: move this logic into the translator classes and get the current translator from the current deck object
                                if (TranslatorType.Incept == ProjectManager.Instance.LoadedProjectTranslatorType)
                                {
                                    InsertVariableText("@[" + columnText + "]");
                                }
                                else if(TranslatorType.JavaScript == ProjectManager.Instance.LoadedProjectTranslatorType)
                                {
                                    InsertVariableText(columnText.StartsWith("~") ? columnText.Substring(1) : columnText);
                                }
                            });
                        m_zContextMenu.Show(listViewElementColumns.PointToScreen(e.Location));
                    }
                }
            }
        }

        private void btnAssist_Click(object sender, EventArgs e)
        {
            contextMenuStripAssist.Items.Clear();
            // NOTE: if there is ever a third scripting language (hopefully not) break this kind of logic out into the Translator classes
#warning this is a bit of a hack, this kind of logic should be handled by the translator
            if (TranslatorType.Incept == ProjectManager.Instance.LoadedProjectTranslatorType)
            {
                contextMenuStripAssist.Items.Add("Add Empty", null, (os, ea) => InsertVariableText("#empty"));
                contextMenuStripAssist.Items.Add("No Draw", null, (os, ea) => InsertVariableText("#nodraw"));
                contextMenuStripAssist.Items.Add("Add If Statement", null,
                    (os, ea) => InsertVariableText("#(if x == y then a)#"));
                contextMenuStripAssist.Items.Add("Add If Else Statement", null,
                    (os, ea) => InsertVariableText("#(if x == y then a else b)#"));
                contextMenuStripAssist.Items.Add("Add Switch Statement", null,
                    (os, ea) =>
                        InsertVariableText("#(switch;key;keytocheck1;value1;keytocheck2;value2;#default;#empty)#"));
                contextMenuStripAssist.Items.Add("Add Background Shape (basic)", null,
                    (os, ea) =>
                        InsertVariableText("#bgshape::#roundedrect;0;-;-;10#::0xff0000#"));
                contextMenuStripAssist.Items.Add("Add Background Shape (advanced)", null,
                    (os, ea) =>
                        InsertVariableText("#bgshape::#roundedrect;0;-;-;10#::0xff0000::0::0::0::0::0::0xffffff#"));
                contextMenuStripAssist.Items.Add("Add Background Graphic (basic)", null,
                    (os, ea) =>
                        InsertVariableText("#bggraphic::images/Faction_empire.bmp#"));
                contextMenuStripAssist.Items.Add("Add Background Graphic (advanced)", null,
                    (os, ea) =>
                        InsertVariableText("#bggraphic::images/Faction_empire.bmp::0::0::0::0::true::-::0::0#"));
                switch ((ElementType) comboElementType.SelectedIndex)
                {
                    case ElementType.FormattedText:
                        contextMenuStripAssist.Items.Add("Add New Line", null, (os, ea) => InsertVariableText("<br>"));
                        contextMenuStripAssist.Items.Add("Add Quote (\")", null, (os, ea) => InsertVariableText("<q>"));
                        contextMenuStripAssist.Items.Add("Add Comma", null, (os, ea) => InsertVariableText("<c>"));
                        break;
                    case ElementType.Text:
                        contextMenuStripAssist.Items.Add("Add Counter...", null, (os, ea) =>
                        {
                            var zQuery = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Add Counter", 450, false));
                            const string INIT_VALUE = "initialValue";
                            const string MULT_VALUE = "multValue";
                            const string PAD_VALUE = "padValue";
                            zQuery.AddNumericBox("Initial Value", 1, int.MinValue, int.MaxValue, INIT_VALUE);
                            zQuery.AddNumericBox("Multiplier Value", 1, int.MinValue, int.MaxValue, MULT_VALUE);
                            zQuery.AddNumericBox("Left 0 Padding", 0, int.MinValue, int.MaxValue, PAD_VALUE);
                            if (DialogResult.OK == zQuery.ShowDialog(this))
                            {
                                InsertVariableText("##" +
                                                   zQuery.GetDecimal(INIT_VALUE) + ";" +
                                                   zQuery.GetDecimal(MULT_VALUE) + ";" +
                                                   zQuery.GetDecimal(PAD_VALUE) + "#");
                            }
                        });
                        contextMenuStripAssist.Items.Add("Add New Line", null, (os, ea) => InsertVariableText("\\n"));
                        contextMenuStripAssist.Items.Add("Add Quote (\")", null, (os, ea) => InsertVariableText("\\q"));
                        contextMenuStripAssist.Items.Add("Add Comma", null, (os, ea) => InsertVariableText("\\c"));
                        break;
                    case ElementType.Graphic:
                        contextMenuStripAssist.Items.Add("Add Draw No Image", null,
                            (os, ea) => InsertVariableText("none"));
                        break;
                    case ElementType.Shape:
                        break;
                    case ElementType.SubLayout:
                        //TODO: add all the layout names?
                        break;
                }
            }
            else if (TranslatorType.JavaScript == ProjectManager.Instance.LoadedProjectTranslatorType)
            {
                contextMenuStripAssist.Items.Add("Add Empty", null, (os, ea) => InsertVariableText("'#empty'"));
                contextMenuStripAssist.Items.Add("No Draw", null, (os, ea) => InsertVariableText("'#nodraw'"));
                contextMenuStripAssist.Items.Add("Add If Statement", null,
                    (os, ea) => InsertVariableText(string.Format("if(x == y){0}{{{0}{0}}}", Environment.NewLine)));
                contextMenuStripAssist.Items.Add("Add If Else Statement", null,
                    (os, ea) => InsertVariableText(string.Format("if(x == y){0}{{{0}{0}}}{0}else{0}{{{0}{0}}}", Environment.NewLine)));
                contextMenuStripAssist.Items.Add("Add Switch Statement", null,
                    (os, ea) =>
                        InsertVariableText(string.Format("switch(key){0}{{{0}case keytocheck1:{0}\tvalue1;{0}}}", Environment.NewLine)));
                contextMenuStripAssist.Items.Add("Add Background Shape (basic)", null,
                    (os, ea) =>
                        InsertVariableText("'#bgshape::#roundedrect;0;-;-;10#::0xff0000#'"));
                contextMenuStripAssist.Items.Add("Add Background Shape (advanced)", null,
                    (os, ea) =>
                        InsertVariableText("'#bgshape::#roundedrect;0;-;-;10#::0xff0000::0::0::0::0::0::0xffffff#'"));
                contextMenuStripAssist.Items.Add("Add Background Graphic (basic)", null,
                    (os, ea) =>
                        InsertVariableText("'#bggraphic::images/Faction_empire.bmp#'"));
                contextMenuStripAssist.Items.Add("Add Background Graphic (advanced)", null,
                    (os, ea) =>
                        InsertVariableText("'#bggraphic::images/Faction_empire.bmp::0::0::0::0::true::-::0::0#'"));
                switch ((ElementType)comboElementType.SelectedIndex)
                {
                    case ElementType.FormattedText:
                        contextMenuStripAssist.Items.Add("Add New Line", null, (os, ea) => InsertVariableText("<br>"));
                        contextMenuStripAssist.Items.Add("Add Quote (\")", null, (os, ea) => InsertVariableText("<q>"));
                        contextMenuStripAssist.Items.Add("Add Comma", null, (os, ea) => InsertVariableText("<c>"));
                        break;
                    case ElementType.Text:
                        contextMenuStripAssist.Items.Add("Add New Line", null, (os, ea) => InsertVariableText("\\\\n"));
                        contextMenuStripAssist.Items.Add("Add Quote (\")", null, (os, ea) => InsertVariableText("\\\\q"));
                        contextMenuStripAssist.Items.Add("Add Comma", null, (os, ea) => InsertVariableText("\\\\c"));
                        break;
                    case ElementType.Graphic:
                        contextMenuStripAssist.Items.Add("Add Draw No Image", null,
                            (os, ea) => InsertVariableText("none"));
                        break;
                    case ElementType.Shape:
                        break;
                }
            }
            contextMenuStripAssist.Show(btnAssist, new Point(btnAssist.Width, 0), ToolStripDropDownDirection.AboveLeft);
        }

#endregion

        private void SetColorValue(Button btnClicked, Color color, ProjectLayoutElement zElement)
        {
            if (btnClicked == btnElementBorderColor)
            {
                zElement.SetElementBorderColor(color);
            }
            else if (btnClicked == btnElementOutlineColor)
            {
                zElement.SetElementOutlineColor(color);
            }
            else if (btnClicked == btnElementFontColor || btnClicked == btnElementShapeColor || btnClicked == btnElementGraphicColor)
            {
                zElement.SetElementColor(color);
            }
            if (btnClicked == btnElementBackgroundColor || btnClicked == btnNullBackgroundColor)
            {
                zElement.SetElementBackgroundColor(color);
            }
        }

        private void HandleTypeEnableStates()
        {
            tabControl.Visible = true;

            // update the graphic and text alignment combo boxes
            ProjectLayoutElement zElement = ElementManager.Instance.GetSelectedElement();
            comboTextHorizontalAlign.SelectedIndex = zElement.horizontalalign;
            comboTextVerticalAlign.SelectedIndex = zElement.verticalalign;
            comboGraphicHorizontalAlign.SelectedIndex = zElement.horizontalalign;
            comboGraphicVerticalAlign.SelectedIndex = zElement.verticalalign;
            comboColorType.SelectedIndex = zElement.colortype;

            tabControl.Enabled = false;
#if MONO_BUILD
            switch ((ElementType)comboElementType.SelectedIndex)
            {
                case ElementType.Graphic:
                    tabControl.SelectedTab = tabPageGraphic;
                    break;
                case ElementType.Shape:
                    tabControl.SelectedTab = tabPageShape;
                    break;
                case ElementType.SubLayout:
                    tabControl.SelectedTab = null;
                    break;
                case ElementType.Text:
                    tabControl.SelectedTab = tabPageFont;
                    checkFontAutoScale.Visible = true;
                    lblWordSpacing.Visible = false;
                    numericWordSpace.Visible = false;
                    lblLineSpace.Visible = false;
                    numericLineSpace.Visible = false;
                    break;
                case ElementType.FormattedText:
                    tabControl.SelectedTab = tabPageFont;
                    checkFontAutoScale.Visible = false;
                    lblWordSpacing.Visible = true;
                    numericWordSpace.Visible = true;
                    numericLineSpace.Visible = true;
                    lblLineSpace.Visible = true;
                    break;
            }
#else
            tabControl.TabPages.Clear();
            switch ((ElementType)comboElementType.SelectedIndex)
            {
                case ElementType.Graphic:
                    tabControl.TabPages.Add(tabPageGraphic);
                    break;
                case ElementType.Shape:
                    tabControl.TabPages.Add(tabPageShape);
                    break;
                case ElementType.Text:
                    tabControl.TabPages.Add(tabPageFont);
                    checkFontAutoScale.Visible = true;
                    lblWordSpacing.Visible = false;
                    numericWordSpace.Visible = false;
                    lblLineSpace.Visible = false;
                    numericLineSpace.Visible = false;
                    checkJustifiedText.Visible = false;
                    break;
                case ElementType.FormattedText:
                    tabControl.TabPages.Add(tabPageFont);
                    checkFontAutoScale.Visible = true;
                    lblWordSpacing.Visible = true;
                    numericWordSpace.Visible = true;
                    numericLineSpace.Visible = true;
                    lblLineSpace.Visible = true;
                    checkJustifiedText.Visible = true;
                    break;
            }
#endif
            tabControl.Enabled = true;
            btnElementBrowseImage.Enabled = (comboElementType.SelectedIndex == (int)ElementType.Graphic);
        }

        private void HandleEnableStates()
        {
            groupBoxElement.Enabled = null != ElementManager.Instance.GetSelectedElement() &&
                                       ElementManager.Instance.SelectedElements.TrueForAll(e => e.enabled);
        }

        private void CreateControlFieldDictionary()
        {
            var zType = typeof(ProjectLayoutElement);

            // HandleElementValueChange related
            m_dictionaryControlField.Add(txtElementVariable, zType.GetProperty("variable"));
            m_dictionaryControlField.Add(comboElementType, zType.GetProperty("type"));
            m_dictionaryControlField.Add(numericElementX, zType.GetProperty("x"));
            m_dictionaryControlField.Add(numericElementY, zType.GetProperty("y"));
            m_dictionaryControlField.Add(numericElementW, zType.GetProperty("width"));
            m_dictionaryControlField.Add(numericElementH, zType.GetProperty("height"));
            m_dictionaryControlField.Add(numericElementBorderThickness, zType.GetProperty("borderthickness"));
            m_dictionaryControlField.Add(checkFontAutoScale, zType.GetProperty("autoscalefont"));
            m_dictionaryControlField.Add(checkLockAspect, zType.GetProperty("lockaspect"));
            m_dictionaryControlField.Add(checkKeepOriginalSize, zType.GetProperty("keeporiginalsize"));
            m_dictionaryControlField.Add(checkCenterOnOrigin, zType.GetProperty("centerimageonorigin"));
            m_dictionaryControlField.Add(numericElementOutLineThickness, zType.GetProperty("outlinethickness"));
            m_dictionaryControlField.Add(numericElementRotation, zType.GetProperty("rotation"));
            m_dictionaryControlField.Add(comboGraphicHorizontalAlign, zType.GetProperty("horizontalalign"));
            m_dictionaryControlField.Add(comboGraphicVerticalAlign, zType.GetProperty("verticalalign"));
            m_dictionaryControlField.Add(comboColorType, zType.GetProperty("colortype"));
            m_dictionaryControlField.Add(txtColorMatrix, zType.GetProperty("colormatrix"));
            m_dictionaryControlField.Add(comboTextHorizontalAlign, zType.GetProperty("horizontalalign"));
            m_dictionaryControlField.Add(comboTextVerticalAlign, zType.GetProperty("verticalalign"));
            m_dictionaryControlField.Add(numericElementOpacity, zType.GetProperty("opacity"));
            m_dictionaryControlField.Add(txtTileSize, zType.GetProperty("tilesize"));
            m_dictionaryControlField.Add(comboElementMirror, zType.GetProperty("mirrortype"));

            // HandleFontSettingChange related 
            m_dictionaryControlField.Add(numericLineSpace, zType.GetProperty("lineheight"));
            m_dictionaryControlField.Add(numericWordSpace, zType.GetProperty("wordspace"));
            m_dictionaryControlField.Add(checkJustifiedText, zType.GetProperty("justifiedtext"));
        }

        private void SetupControlActions()
        {
            m_dictionaryControlActions.Add(txtElementVariable, zElement =>
            {
                LayoutManager.Instance.ActiveDeck.ResetTranslationCache(zElement);
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name);
            });

            m_dictionaryControlActions.Add(numericElementX, zElement =>
            {
                LayoutManager.Instance.ActiveDeck.ResetTranslationCache(zElement);
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name);
            });
            m_dictionaryControlActions.Add(numericElementY, zElement =>
            {
                LayoutManager.Instance.ActiveDeck.ResetTranslationCache(zElement);
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name);
            });
            m_dictionaryControlActions.Add(numericElementW, zElement =>
            {
                LayoutManager.Instance.ActiveDeck.ResetTranslationCache(zElement);
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name);
            });
            m_dictionaryControlActions.Add(numericElementH, zElement =>
            {
                LayoutManager.Instance.ActiveDeck.ResetTranslationCache(zElement);
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name);
            });

            m_dictionaryControlActions.Add(numericElementOutLineThickness, zElement =>
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(comboTextHorizontalAlign, zElement =>
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(comboTextVerticalAlign, zElement =>
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(numericElementOpacity, zElement =>
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(checkJustifiedText, zElement =>
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(checkFontAutoScale, zElement =>
                LayoutManager.Instance.ActiveDeck.ResetMarkupCache(zElement.name));
        }

        private bool PopulateUndoRedoValues(Control zControl, ProjectLayoutElement zElement, ref object zRedoValue, ref object zUndoValue)
        {
            PropertyInfo zInfo;
            if (!m_dictionaryControlField.TryGetValue(zControl, out zInfo))
            {
                return false;
            }

            MethodInfo zGetMethodInfo = zInfo.GetGetMethod();
            if (zInfo.PropertyType == typeof(int))
            {
                zUndoValue = (int)zGetMethodInfo.Invoke(zElement, null);
                if (zControl is ComboBox)
                {
                    zRedoValue = ((ComboBox)zControl).SelectedIndex;
                }
                else // assumes numericupdown
                {
                    zRedoValue = (int)((NumericUpDown)zControl).Value;
                }
            }
            else if (zInfo.PropertyType == typeof(float))
            {
                zUndoValue = (float)zGetMethodInfo.Invoke(zElement, null);
                zRedoValue = (float)((NumericUpDown)zControl).Value;
            }
            else if (zInfo.PropertyType == typeof(string))
            {
                zUndoValue = (string)zGetMethodInfo.Invoke(zElement, null);
                if (zControl is ComboBox)
                {
                    zRedoValue = ((ComboBox)zControl).Text;
                }
                else // assumes textbox
                {
                    zRedoValue = ((TextBox)zControl).Text;
                }
            }
            else if (zInfo.PropertyType == typeof(bool))
            {
                zUndoValue = (bool)zGetMethodInfo.Invoke(zElement, null);
                zRedoValue = ((CheckBox)zControl).Checked;
            }
            return true;
        }

        private void AssignValueByControl(Control zControl, ProjectLayoutElement zElement, object zNewValue, bool bSkipControlUpdate)
        {
            // Assign the element property the new value
            PropertyInfo zInfo = m_dictionaryControlField[zControl];
            // TODO: would caching this call speed anything up?
            MethodInfo zSetMethodInfo = zInfo.GetSetMethod();
            zSetMethodInfo.Invoke(zElement, new object[] { zNewValue });

            // execute any control/element property specific functionality
            PerformControlChangeActions(zElement, zControl);

            if (ElementManager.Instance.GetSelectedElement() != zElement || bSkipControlUpdate)
            {
                // don't update the controls if they are for a non-selected item
                return;
            }

            // update the element controls
            if (zInfo.PropertyType == typeof(int))
            {
                if (zControl is ComboBox)
                {
                    ((ComboBox)zControl).SelectedIndex = (int)zNewValue;
                }
                else
                {
                    ((NumericUpDown)zControl).Value = (int)zNewValue;
                }
            }
            else if (zInfo.PropertyType == typeof(float))
            {
                ((NumericUpDown)zControl).Value = (decimal)(float)zNewValue;
            }
            else if (zInfo.PropertyType == typeof(string))
            {
                if (zControl is ComboBox)
                {
                    ((ComboBox)zControl).Text = (string)zNewValue;
                }
                else // assumes textbox
                {
                    ((TextBox)zControl).Text = (string)zNewValue;
                }
            }
            else if (zInfo.PropertyType == typeof(bool))
            {
                ((CheckBox)zControl).Checked = (bool)zNewValue;
            }

        }

        /// <summary>
        /// Method for updating the selected element bounds control values externally (no undo/redo processing)
        /// </summary>
        /// <param name="zElement"></param>
        private void UpdateElementBoundsControlValues(ProjectLayoutElement zElement)
        {
            if (null != zElement)
            {
                var listControlsChanges = new List<Control>();
                m_bFireElementChangeEvents = false;
                if (AssignNumericIfDifferent(numericElementX, zElement.x)) listControlsChanges.Add(numericElementX);
                if (AssignNumericIfDifferent(numericElementY, zElement.y)) listControlsChanges.Add(numericElementY);
                if (AssignNumericIfDifferent(numericElementW, zElement.width)) listControlsChanges.Add(numericElementW);
                if (AssignNumericIfDifferent(numericElementH, zElement.height)) listControlsChanges.Add(numericElementH);
                if (AssignNumericIfDifferent(numericElementRotation, (decimal)zElement.rotation)) listControlsChanges.Add(numericElementRotation);
                m_bFireElementChangeEvents = true;

                // TODO: if the value does not actually change this applies an update for no specific reason... (tbd)
                PerformControlChangeActions(zElement, listControlsChanges.ToArray());
            }
            LayoutManager.Instance.FireLayoutUpdatedEvent(true);
        }

        /// <summary>
        /// Assigns the specified value to the numeric if the numeric does not already have the value
        /// </summary>
        /// <param name="numericControl">The numeric to update</param>
        /// <param name="nValue">The value to set on the numeric</param>
        /// <returns>true if the value changed, false otherwise</returns>
        private bool AssignNumericIfDifferent(NumericUpDown numericControl, decimal nValue)
        {
            if (numericControl.Value != nValue)
            {
                numericControl.Value = nValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Perform any actions associated with the specified control using the element as a parameter
        /// </summary>
        /// <param name="zElement">The element to act with</param>
        /// <param name="arraycontrols">The controls to check for associated actions</param>
        private void PerformControlChangeActions(ProjectLayoutElement zElement, params Control[] arraycontrols)
        {
            Action<ProjectLayoutElement> action;
            foreach(var zControl in arraycontrols)
            {
                if (m_dictionaryControlActions.TryGetValue(zControl, out action))
                {
                    action(zElement);
                }                
            }
        }

        private void UpdateElementValues(ProjectLayoutElement zElement)
        {
            if (null != zElement)
            {
                m_bFireElementChangeEvents = false;
                numericElementX.Value = zElement.x;
                numericElementY.Value = zElement.y;
                numericElementW.Value = zElement.width;
                numericElementH.Value = zElement.height;
                numericElementRotation.Value = (decimal)zElement.rotation;
                numericElementBorderThickness.Value = (decimal)zElement.borderthickness;
                numericElementOutLineThickness.Value = (decimal)zElement.outlinethickness;
                numericLineSpace.Value = (decimal)zElement.lineheight;
                numericWordSpace.Value = (decimal)zElement.wordspace;
                checkFontAutoScale.Checked = zElement.autoscalefont;
                checkLockAspect.Checked = zElement.lockaspect;
                checkKeepOriginalSize.Checked = zElement.keeporiginalsize;
                checkCenterOnOrigin.Checked = zElement.centerimageonorigin;
                numericElementOpacity.Value = (decimal)zElement.opacity;
                comboTextHorizontalAlign.SelectedIndex = zElement.horizontalalign;
                comboTextVerticalAlign.SelectedIndex = zElement.verticalalign;
                comboGraphicHorizontalAlign.SelectedIndex = zElement.horizontalalign;
                comboGraphicVerticalAlign.SelectedIndex = zElement.verticalalign;
                comboColorType.SelectedIndex = zElement.colortype;
                txtTileSize.Text = zElement.tilesize;
                checkJustifiedText.Checked = zElement.justifiedtext;
                txtElementVariable.Text = zElement.variable;
                txtElementVariable.SelectionStart = zElement.variable.Length;
                txtElementVariable.SelectionLength = 0;
                comboElementMirror.SelectedIndex = zElement.mirrortype;
                ElementType eType = EnumUtil.GetElementType(zElement.type);
                switch (eType)
                {
                    case ElementType.Shape:
                        // configure the combo box and property grid for the shap
                        string sType = AbstractShape.GetShapeType(zElement.variable);
                        if (null != sType)
                        {
                            int nSelectedIndex;
                            if (dictionaryShapeTypeIndex.TryGetValue(sType, out nSelectedIndex))
                            {
                                comboShapeType.SelectedIndex = nSelectedIndex;
                                // always make a copy of the shape object for properties editing
                                var zShape = (AbstractShape)Activator.CreateInstance(comboShapeType.SelectedItem.GetType());
                                zShape.InitializeItem(zElement.variable);
                                // associated the prop grid with this shape object
                                propertyGridShape.SelectedObject = zShape;
                            }
                        }
                        break;
                    case ElementType.Text:
                        checkFontAutoScale.Visible = true;
                        lblWordSpacing.Visible = false;
                        numericWordSpace.Visible = false;
                        checkJustifiedText.Visible = false;
                        break;
                    case ElementType.FormattedText:
                        checkFontAutoScale.Visible = true;
                        lblWordSpacing.Visible = true;
                        numericWordSpace.Visible = true;
                        checkJustifiedText.Visible = true;
                        break;
                }
                // this fires a change event on the combo box... seems like it might be wrong?
                comboElementType.SelectedIndex = (int)eType;
                UpdatePanelColors(zElement);
                Font zFont = zElement.GetElementFont();
                zFont = zFont ?? FontLoader.DefaultFont;
                for (int nFontIndex = 0; nFontIndex < comboFontName.Items.Count; nFontIndex++)
                {
                    if (zFont.Name.Equals((string)comboFontName.Items[nFontIndex], StringComparison.CurrentCultureIgnoreCase))
                    {
                        comboFontName.SelectedIndex = nFontIndex;
                        break;
                    }
                }

                SetupElementFont(zFont);
                m_bFireElementChangeEvents = true;
            }
            HandleEnableStates();
        }

        private void SetupElementFont(Font zElementFont)
        {
            m_bFireFontChangeEvents = false;
            // setup the font settings (unsupported types cause exceptions)
            var zFamily = m_listFontFamilies[comboFontName.SelectedIndex];

            // configure font style controls
            ConfigureFontStyleControl(zFamily, FontStyle.Bold, checkBoxBold);
            ConfigureFontStyleControl(zFamily, FontStyle.Italic, checkBoxItalic);
            ConfigureFontStyleControl(zFamily, FontStyle.Strikeout, checkBoxStrikeout);
            ConfigureFontStyleControl(zFamily, FontStyle.Underline, checkBoxUnderline);

            if (null != zElementFont)
            {
                checkBoxBold.Checked = zElementFont.Bold;
                checkBoxItalic.Checked = zElementFont.Italic;
                checkBoxStrikeout.Checked = zElementFont.Strikeout;
                checkBoxUnderline.Checked = zElementFont.Underline;
                numericFontSize.Value = (decimal)zElementFont.SizeInPoints;
            }

            m_bFireFontChangeEvents = true;
        }

        private void ConfigureFontStyleControl(FontFamily zFamily, FontStyle eStyle, CheckBox checkBox)
        {
            checkBox.Enabled = zFamily.IsStyleAvailable(eStyle);
            if (!checkBox.Enabled)
            {
                checkBox.Checked = false;
            }
        }

        private void UpdatePanelColors(ProjectLayoutElement zElement)
        {
            if (zElement != ElementManager.Instance.GetSelectedElement())
            {
                return;
            }
            panelBorderColor.BackColor = zElement.GetElementBorderColor();
            panelOutlineColor.BackColor = zElement.GetElementOutlineColor();
            panelShapeColor.BackColor = zElement.GetElementColor();
            panelFontColor.BackColor = panelShapeColor.BackColor;
            panelGraphicColor.BackColor = panelShapeColor.BackColor;
            panelBackgroundColor.BackColor = zElement.GetElementBackgroundColor() == CardMakerConstants.NoColor
                ? Control.DefaultBackColor
                : zElement.GetElementBackgroundColor();
        }

        private void InsertVariableText(string textToInsert)
        {
            int previousSelectionStart = txtElementVariable.SelectionStart;
            txtElementVariable.Text =
                txtElementVariable.Text.Remove(previousSelectionStart, txtElementVariable.SelectionLength).Insert(txtElementVariable.SelectionStart, textToInsert);
            txtElementVariable.SelectionStart = previousSelectionStart + textToInsert.Length;
            txtElementVariable.SelectionLength = 0;
        }

        private FontFamily[] GetFontFamilies()
        {
            var zFonts = new InstalledFontCollection();
            var arrayFontsCopy = new FontFamily[zFonts.Families.Length];
            Array.Copy(zFonts.Families, arrayFontsCopy, zFonts.Families.Length);
#if MONO_BUILD // the collection is unsorted in mono
            Array.Sort(arrayFontsCopy, (a, b) => a.Name.CompareTo(b.Name));
#endif
            return arrayFontsCopy;
        }
    }
}