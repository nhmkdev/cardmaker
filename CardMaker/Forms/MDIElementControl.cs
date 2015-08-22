////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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

using CardMaker.Card;
using CardMaker.Card.Shapes;
using CardMaker.XML;
using Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

namespace CardMaker.Forms
{
    public partial class MDIElementControl : Form
    {
        private static MDIElementControl s_zInstance;

        private readonly Dictionary<string, ElementType> m_dictionaryElementTypes = new Dictionary<string, ElementType>();

        // mapping controls directly to special functions when a given control is adjusted
        private readonly Dictionary<Control, Action<ProjectLayoutElement>> m_dictionaryControlActions = new Dictionary<Control, Action<ProjectLayoutElement>>();
        
        // mapping controls directly to properties in the ProjectLayoutElement class
        private readonly Dictionary<Control, PropertyInfo> m_dictionaryControlField = new Dictionary<Control, PropertyInfo>();

        private readonly List<FontFamily> m_listFontFamilies = new List<FontFamily>();
        private readonly Dictionary<string, int> dictionaryShapeTypeIndex = new Dictionary<string, int>();

        private readonly ContextMenuStrip m_zContextMenu;

        bool m_bFireFontChangeEvents = true;

        private MDIElementControl() 
        {
            InitializeComponent();

            btnElementBorderColor.Tag = panelBorderColor;
            btnElementFontColor.Tag = panelFontColor;
            btnElementShapeColor.Tag = panelShapeColor;
            btnElementOutlineColor.Tag = panelOutlineColor;

            // setup the font related items
            var fonts = new InstalledFontCollection();
            foreach (FontFamily zFontFamily in fonts.Families)
            {
                m_listFontFamilies.Add(zFontFamily);
                comboFontName.Items.Add(zFontFamily.Name);
            }

            // configure all event handling actions
            SetupControlActions();

            CreateControlFieldDictionary();

            comboFontName.SelectedIndex = 0;

            m_zContextMenu = new ContextMenuStrip
            {
                RenderMode = ToolStripRenderMode.System
            };
        }

        public static MDIElementControl Instance
        {
            get
            {
                if (null == s_zInstance)
                    s_zInstance = new MDIElementControl();
                return s_zInstance;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CP_NOCLOSE_BUTTON = 0x200;
                CreateParams mdiCp = base.CreateParams;
                mdiCp.ClassStyle = mdiCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return mdiCp;
            }
        }

        private void btnElementBrowseImage_Click(object sender, EventArgs e)
        {
            CardMakerMDI.FileOpenHandler("All files (*.*)|*.*", txtElementVariable, true);
        }

        private void comboElementType_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleElementValueChange(sender, e);
            MDILayoutControl.Instance.RefreshElementTypes();
            HandleTypeEnableStates();
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            var listSelectedElements = MDILayoutControl.Instance.GetSelectedLayoutElements();
            if (null == listSelectedElements)
            {
                MessageBox.Show(this, "Please select at least one enabled Element.");
                return;
            }

            var zRGB = new RGBColorSelectDialog();
            var btnClicked = (Button)sender;
            var zPanel = (Panel)btnClicked.Tag;
            zRGB.UpdateColorBox(zPanel.BackColor);

            if (DialogResult.OK != zRGB.ShowDialog())
            {
                return;
            }
            var colorRedo = zRGB.Color;

            var listActions = UserAction.CreateActionList();

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
                else if (btnClicked == btnElementFontColor || btnClicked == btnElementShapeColor)
                {
                    colorUndo = zElement.GetElementColor();
                }

                listActions.Add(bRedo =>
                    {
                        SetColorValue(btnClicked, bRedo ? colorRedo : colorUndo, zElementToChange);
                        UpdatePanelColors(zElementToChange);
                    });
            }

            Action<bool> actionChangeColor = bRedo =>
            {
                listActions.ForEach(action => action(bRedo));
                CardMakerMDI.Instance.DrawCurrentCardIndex();
                CardMakerMDI.Instance.MarkDirty();
            };
            UserAction.PushAction(actionChangeColor);

            // perform the action as a redo now
            actionChangeColor(true);
        }

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
            else if (btnClicked == btnElementFontColor || btnClicked == btnElementShapeColor)
            {
                zElement.SetElementColor(color);
            }
        }

        public void HandleTypeEnableStates()
        {
            tabControl.Visible = true;

            // update the graphic and text alignment combo boxes
            ProjectLayoutElement zElement = MDILayoutControl.Instance.GetSelectedLayoutElement();
            comboTextHorizontalAlign.SelectedIndex = zElement.horizontalalign;
            comboTextVerticalAlign.SelectedIndex = zElement.verticalalign;
            comboGraphicHorizontalAlign.SelectedIndex = zElement.horizontalalign;
            comboGraphicVerticalAlign.SelectedIndex = zElement.verticalalign;

            tabControl.Enabled = false;
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
                    break;
                case ElementType.FormattedText:
                    tabControl.TabPages.Add(tabPageFont);
                    checkFontAutoScale.Visible = false;
                    lblWordSpacing.Visible = true;
                    numericWordSpace.Visible = true;
                    numericLineSpace.Visible = true;
                    lblLineSpace.Visible = true;
                    break;
            }

            tabControl.Enabled = true;
            btnElementBrowseImage.Enabled = (comboElementType.SelectedIndex == (int)ElementType.Graphic);
        }

        public void HandleEnableStates()
        {
            groupBoxElement.Enabled = (null != MDILayoutControl.Instance.GetSelectedLayoutElement());
        }

        public void CreateControlFieldDictionary()
        {
            Type zType = typeof(ProjectLayoutElement);

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
            m_dictionaryControlField.Add(numericElementOutLineThickness, zType.GetProperty("outlinethickness"));
            m_dictionaryControlField.Add(numericElementRotation, zType.GetProperty("rotation"));
            m_dictionaryControlField.Add(comboGraphicHorizontalAlign, zType.GetProperty("horizontalalign"));
            m_dictionaryControlField.Add(comboGraphicVerticalAlign, zType.GetProperty("verticalalign"));
            m_dictionaryControlField.Add(comboTextHorizontalAlign, zType.GetProperty("horizontalalign"));
            m_dictionaryControlField.Add(comboTextVerticalAlign, zType.GetProperty("verticalalign"));
            m_dictionaryControlField.Add(numericElementOpacity, zType.GetProperty("opacity"));

            // HandleFontSettingChange related 
            m_dictionaryControlField.Add(numericLineSpace, zType.GetProperty("lineheight"));
            m_dictionaryControlField.Add(numericWordSpace, zType.GetProperty("wordspace"));
        }

        public void SetupControlActions()
        {
            m_dictionaryControlActions.Add(txtElementVariable, zElement =>
            {
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetTranslationCache(zElement);
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name);
            });

            m_dictionaryControlActions.Add(numericElementW, zElement =>
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(numericElementH, zElement =>
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(numericElementOutLineThickness, zElement =>
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(comboTextHorizontalAlign, zElement =>
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(comboTextVerticalAlign, zElement =>
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name));

            m_dictionaryControlActions.Add(numericElementOpacity, zElement =>
                CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElement.name));
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
            Action<ProjectLayoutElement> actionElement;
            if(m_dictionaryControlActions.TryGetValue(zControl, out actionElement))
            {
                actionElement(zElement);
            }

            if (MDILayoutControl.Instance.GetSelectedLayoutElement() != zElement || bSkipControlUpdate)
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

        private void HandleElementValueChange(object sender, EventArgs e)
        {
            if (null == MDILayoutControl.Instance.GetSelectedLayoutElement() || 
                !MDILayoutControl.Instance.FireElementChangeEvents || 
                CardMakerMDI.ProcessingUserAction)
            {
                return;
            }

            // special case handling for the user dragging in the Canvas (manages its own undo/redo)
            if (MDICanvas.Instance.CanvasUserAction)
            {
                Action<ProjectLayoutElement> action;
                if(m_dictionaryControlActions.TryGetValue((Control)sender, out action))
                {
                    // see SetupEventHandlerActions
                    action(MDILayoutControl.Instance.GetSelectedLayoutElement());
                    CardMakerMDI.Instance.MarkDirty();
                    CardMakerMDI.Instance.DrawCurrentCardIndex();
                }
                return;
            }

            var listSelectedElements = MDILayoutControl.Instance.GetSelectedLayoutElements();
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
                    CardMakerMDI.ProcessingUserAction = true;
                    listActions.ForEach(action => action(bRedo));
                    CardMakerMDI.ProcessingUserAction = false;
                    CardMakerMDI.Instance.MarkDirty();
                    CardMakerMDI.Instance.DrawCurrentCardIndex();
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

            for (int nIdx = 0; nIdx < (int)ElementType.End; nIdx++)
            {
                var sType = ((ElementType)nIdx).ToString();
                comboElementType.Items.Add(sType);
                m_dictionaryElementTypes.Add(sType, (ElementType)nIdx);
            }

            tabControl.Visible = false;
        }

        public int ElementX
        {
            set
            {
                numericElementX.Value = value;
            }
            get
            {
                return (int)numericElementX.Value;
            }
        }

        public int ElementY
        {
            set
            {
                numericElementY.Value = value;
            }
            get
            {
                return (int)numericElementY.Value;
            }
        }

        public int ElementH
        {
            set
            {
                numericElementH.Value = value;
            }
        }

        public int ElementW
        {
            set
            {
                numericElementW.Value = value;
            }
        }

        public void UpdateElementColumnValues()
        {
            CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.PopulateListViewWithElementColumns(listViewElementColumns);
        }

        public void UpdateCurrentElementExtents()
        {
            ProjectLayoutElement zElement = MDILayoutControl.Instance.GetSelectedLayoutElement();
            if (null != zElement)
            {
                MDILayoutControl.Instance.FireElementChangeEvents = false;
                numericElementX.Value = zElement.x;
                numericElementY.Value = zElement.y;
                numericElementW.Value = zElement.width;
                numericElementH.Value = zElement.height;
                MDILayoutControl.Instance.FireElementChangeEvents = true;
            }
            CardMakerMDI.Instance.MarkDirty();
            CardMakerMDI.Instance.DrawCurrentCardIndex();
        }

        public void UpdateElementValues(ProjectLayoutElement zElement)
        {
            if (null != zElement)
            {
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
                numericElementOpacity.Value = (decimal)zElement.opacity;
                comboTextHorizontalAlign.SelectedIndex = zElement.horizontalalign;
                comboTextVerticalAlign.SelectedIndex = zElement.verticalalign;
                comboGraphicHorizontalAlign.SelectedIndex = zElement.horizontalalign;
                comboGraphicVerticalAlign.SelectedIndex = zElement.verticalalign;
                txtElementVariable.Text = zElement.variable;
                txtElementVariable.SelectionStart = zElement.variable.Length;
                txtElementVariable.SelectionLength = 0;
                ElementType eType = m_dictionaryElementTypes[zElement.type];
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
                                var zShape = (AbstractShape)comboShapeType.SelectedItem;
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
                        break;
                    case ElementType.FormattedText:
                        checkFontAutoScale.Visible = false;
                        lblWordSpacing.Visible = true;
                        numericWordSpace.Visible = true;
                        break;
                }
                comboElementType.SelectedIndex = (int)eType;
                UpdatePanelColors(zElement);
                groupBoxElement.Enabled = true;
                Font zFont = zElement.GetElementFont();
                zFont = zFont ?? DrawItem.DefaultFont;
                for (int nFontIndex = 0; nFontIndex < comboFontName.Items.Count; nFontIndex++)
                {
                    if (zFont.Name.Equals((string)comboFontName.Items[nFontIndex], StringComparison.CurrentCultureIgnoreCase))
                    {
                        comboFontName.SelectedIndex = nFontIndex;
                        break;
                    }
                }

                SetupElementFont(zFont);
            }
            else
            {
                groupBoxElement.Enabled = false;
            }
        }

        private void HandleFontSettingChange(object sender, EventArgs e)
        {
            if (null == MDILayoutControl.Instance || 
                !MDILayoutControl.Instance.FireElementChangeEvents || 
                !m_bFireFontChangeEvents || 
                CardMakerMDI.ProcessingUserAction)
            {
                return;
            }

            var listElements = MDILayoutControl.Instance.GetSelectedLayoutElements();
            if (null != listElements)
            {
                var listActions = UserAction.CreateActionList();

                var zControl = (Control)sender;

                foreach (var zElement in listElements)
                {
                    var zElementToChange = zElement;
                    if (!CardMakerMDI.ProcessingUserAction && null != sender)
                    {
                        object zRedoValue = null;
                        object zUndoValue = null;
                        // The current value on the element can be used for an undo
                        if (PopulateUndoRedoValues(zControl, zElementToChange, ref zRedoValue, ref zUndoValue))
                        {
                            listActions.Add(bRedo =>
                            {
                                AssignValueByControl(zControl, zElementToChange, bRedo ? zRedoValue : zUndoValue, false);
                                if (null != CardMakerMDI.Instance.DrawCardCanvas)
                                {
                                    CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElementToChange.name);
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

                            var zFont = new Font(m_listFontFamilies[comboFontName.SelectedIndex], (int)numericFontSize.Value, eFontStyle);

                            var fontRedo = zFont;
                            var fontUndo = zElementToChange.GetElementFont();
                            fontUndo = fontUndo ?? DrawItem.DefaultFont;

                            listActions.Add(bRedo =>
                            {
                                var fontValue = bRedo ? fontRedo : fontUndo;

                                zElementToChange.SetElementFont(fontValue);

                                // only affect the controls if the current selected element matches that of the element to change
                                if (zElementToChange == MDILayoutControl.Instance.GetSelectedLayoutElement())
                                {
                                    comboFontName.Text = fontValue.Name;
                                    SetupElementFont(fontValue);
                                }
                                if (null != CardMakerMDI.Instance.DrawCardCanvas)
                                {
                                    CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.ResetMarkupCache(zElementToChange.name);
                                }
                            });
                        }
                    }
                }

                Action<bool> actionChangeFont = bRedo =>
                {
                    CardMakerMDI.ProcessingUserAction = true;
                    listActions.ForEach(action => action(bRedo));
                    CardMakerMDI.ProcessingUserAction = false;
                    if (0 < numericLineSpace.Value)
                    {
                        checkFontAutoScale.Checked = false;
                    }
                    CardMakerMDI.Instance.DrawCurrentCardIndex();
                    CardMakerMDI.Instance.MarkDirty();
                };

                UserAction.PushAction(actionChangeFont);

                // perform the action as a redo now
                actionChangeFont(true);
                    
            }
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

        private void comboFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupElementFont(null);
            HandleFontSettingChange(sender, e);
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
            switch(e.KeyCode)
            {
                case Keys.Enter:
                    e.SuppressKeyPress = true;
                    break;
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

        private void btnSetSizeToImage_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(txtElementVariable.Text))
            {
                var zBmp = DrawItem.LoadImageFromCache(txtElementVariable.Text);
                if (null == zBmp)
                {
                    var zElement = MDILayoutControl.Instance.GetSelectedLayoutElement();
                    if (null != zElement)
                    {
                        var zElementString = CardMakerMDI.Instance.DrawCardCanvas.ActiveDeck.GetStringFromTranslationCache(zElement.name);
                        if (null != zElementString.String)
                        {
                            zBmp = DrawItem.LoadImageFromCache(zElementString.String);
                        }
                    }
                }
                if(null != zBmp)
                {
                    numericElementW.Value = zBmp.Width;
                    numericElementH.Value = zBmp.Height;
                }
            }
        }

        private void UpdatePanelColors(ProjectLayoutElement zElement)
        {
            if (zElement != MDILayoutControl.Instance.GetSelectedLayoutElement()) return;
            panelBorderColor.BackColor = zElement.GetElementBorderColor();
            panelOutlineColor.BackColor = zElement.GetElementOutlineColor();
            panelShapeColor.BackColor = zElement.GetElementColor();
            panelFontColor.BackColor = panelShapeColor.BackColor;
        }

        private void listViewElementColumns_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right == e.Button)
            {
                var info = listViewElementColumns.HitTest(e.Location);
                if(null != info.Item)
                {
                    int nColumn = info.Item.SubItems.IndexOf(info.SubItem);
                    if(-1 != nColumn)
                    {
                        string columnText = listViewElementColumns.Columns[nColumn].Text;
                        m_zContextMenu.Items.Clear();
                        m_zContextMenu.Items.Add("Add Reference to [" + columnText + "] column", null, 
                            (osender, ea) =>
                                InsertVariableText("@[" + columnText + "]"));
                        m_zContextMenu.Show(listViewElementColumns.PointToScreen(e.Location));
                    }
                }
            }
        }

        private void InsertVariableText(string textToInsert)
        {
            int previousSelectionStart = txtElementVariable.SelectionStart;
            txtElementVariable.Text =
                txtElementVariable.Text.Remove(previousSelectionStart, txtElementVariable.SelectionLength).Insert(txtElementVariable.SelectionStart, textToInsert);
            txtElementVariable.SelectionStart = previousSelectionStart + textToInsert.Length;
            txtElementVariable.SelectionLength = 0;
        }

        private void btnAssist_Click(object sender, EventArgs e)
        {
            //ProjectLayoutElement zElement = MDILayoutControl.Instance.GetSelectedLayoutElement();
            contextMenuStripAssist.Items.Clear();
            contextMenuStripAssist.Items.Add("Add Empty", null, (os, ea) => InsertVariableText("#empty"));
            contextMenuStripAssist.Items.Add("Add If Statement", null, (os, ea) => InsertVariableText("#(if x == y then a)#"));
            contextMenuStripAssist.Items.Add("Add If Else Statement", null, (os, ea) => InsertVariableText("#(if x == y then a else b)#"));
            contextMenuStripAssist.Items.Add("Add Switch Statement", null, (os, ea) => InsertVariableText("#(switch;key;keytocheck1;value1;keytocheck2;value2;#default;#empty)#"));
            switch ((ElementType)comboElementType.SelectedIndex)
            {
                case ElementType.FormattedText:
                    contextMenuStripAssist.Items.Add("Add New Line", null, (os, ea) => InsertVariableText("<br>"));
                    contextMenuStripAssist.Items.Add("Add Quote (\")", null, (os, ea) => InsertVariableText("<q>"));
                    contextMenuStripAssist.Items.Add("Add Comma", null, (os, ea) => InsertVariableText("<c>"));
                    break;
                case ElementType.Text:
                    contextMenuStripAssist.Items.Add("Add Counter...", null, (os, ea) =>
                        {
                            var zQuery = new QueryPanelDialog("Add Counter", 450, false);
                            zQuery.SetIcon(CardMakerMDI.Instance.Icon);
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
                    contextMenuStripAssist.Items.Add("Add Draw No Image", null, (os, ea) => InsertVariableText("none"));
                    break;
                case ElementType.Shape:
                    break;
            }
            contextMenuStripAssist.Show(btnAssist, new Point(btnAssist.Width, 0), ToolStripDropDownDirection.AboveLeft);
        }
    }
}