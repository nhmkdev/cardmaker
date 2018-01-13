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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CardMaker.Card;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.UI;
using LayoutEventArgs = CardMaker.Events.Args.LayoutEventArgs;

namespace CardMaker.Forms
{
    public partial class MDILayoutControl : Form
    {
        private bool m_bFireLayoutChangeEvents = true;
        private readonly List<ProjectLayoutElement> m_listClipboardElements = new List<ProjectLayoutElement>();
        private readonly Dictionary<string, ListViewItem> m_dictionaryItems = new Dictionary<string, ListViewItem>();
        private int[] m_arrayRowToIndex;
        private int[] m_arrayIndexToRow;

        private ProjectLayout m_zLastProjectLayout;
        private int m_nDestinationCardIndex = -1;

        public MDILayoutControl() 
        {
            InitializeComponent();
            LayoutManager.Instance.LayoutLoaded += ProjectLayout_Loaded;
            LayoutManager.Instance.ElementOrderAdjustRequest += ElementOrderAdjust_Request;
            LayoutManager.Instance.ElementSelectAdjustRequest += ElementSelectAdjust_Request;
            LayoutManager.Instance.LayoutUpdated += Layout_Updated;
            LayoutManager.Instance.DeckIndexChangeRequested += DeckIndexChange_Requested;
            ElementManager.Instance.ElementSelectRequested += Element_Selected;
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

        #endregion

        #region manager events

        void DeckIndexChange_Requested(object sender, DeckChangeEventArgs args)
        {
            int adjustedValue = args.Index + 1;
            if (adjustedValue >= numericCardIndex.Minimum &&
                adjustedValue <= numericCardIndex.Maximum)
            {
                numericCardIndex.Value = adjustedValue;
            }
        }

        void ElementSelectAdjust_Request(object sender, LayoutElementNumericAdjustEventArgs args)
        {
            ChangeSelectedElement(args.Adjustment);
        }

        void ElementOrderAdjust_Request(object sender, LayoutElementNumericAdjustEventArgs args)
        {
            ChangeElementOrder(args.Adjustment);
        }

        void Layout_Updated(object sender, LayoutEventArgs args)
        {
            RefreshElementInformation();
        }

        void Element_Selected(object sender, ElementEventArgs args)
        {
            if (null != args.Elements && args.Elements.Count == 1)
            {
                ChangeSelectedElement(args.Elements[0].name);
            }
        }

        void ProjectLayout_Loaded(object sender, LayoutEventArgs args)
        {
            UpdateLayoutInfo(args.Layout);
        }

        #endregion

        #region form events

        private void btnAddElement_Click(object sender, EventArgs e)
        {
            const string ELEMNAME = "ELEMNAME";
            var zQuery = new QueryPanelDialog("Add Element", 400, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddLabel("Element Names are broken up by a line.", 24);
            zQuery.AddMultiLineTextBox("Element Name(s)", string.Empty, 200, ELEMNAME);

            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var arrayNames = zQuery.GetString(ELEMNAME).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (0 < arrayNames.Length)
                {
                    AddElements(arrayNames, null);
                    ChangeSelectedElement(arrayNames[0]);
                }
            }
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (0 == listViewElements.SelectedItems.Count)
            {
                return;
            }

            const string ELEMNAME = "ELEMNAME";
            var zQuery = new QueryPanelDialog("Duplicate Element", 400, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddLabel("Duplicate Element Names are broken up by a line.", 24);
            zQuery.AddMultiLineTextBox("Element Name(s)", string.Empty, 200, ELEMNAME);
            if (1 < listViewElements.SelectedItems.Count)
            {
                zQuery.Form.Closing += (o, args) =>
                {
                    if (zQuery.Form.DialogResult == DialogResult.Cancel)
                    {
                        return;
                    }
                    var arrayNames = zQuery.GetString(ELEMNAME)
                                        .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (arrayNames.Length != listViewElements.SelectedItems.Count)
                    {
                        MessageBox.Show(zQuery.Form,
                            $"Please specify {listViewElements.SelectedItems.Count} element names.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        args.Cancel = true;
                    }
                };
            }

            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                string[] arrayNames = zQuery.GetString(ELEMNAME)
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (1 == listViewElements.SelectedItems.Count)
                {
                    AddElements(arrayNames, (ProjectLayoutElement) listViewElements.SelectedItems[0].Tag);
                }
                else if (arrayNames.Length == listViewElements.SelectedIndices.Count)
                {
                    var listIndicies = new List<int>();
                    foreach (int nIdx in listViewElements.SelectedIndices)
                    {
                        listIndicies.Add(nIdx);
                    }
                    listIndicies.Sort();
                    for (var nIdx = 0; nIdx < arrayNames.Length; nIdx++)
                    {
                        AddElements(new string[] { arrayNames[nIdx] }, (ProjectLayoutElement)listViewElements.Items[listIndicies[nIdx]].Tag);
                    }
                }
            }
        }

        private void btnRemoveElement_Click(object sender, EventArgs e)
        {
            if (0 == listViewElements.SelectedItems.Count)
            {
                return;
            }

            if (DialogResult.Yes == MessageBox.Show(this, "Are you sure you want to remove the selected elements?", "Remove Elements", MessageBoxButtons.YesNo))
            {
                var listToDelete = new List<ListViewItem>();
                var listToKeep = new List<ProjectLayoutElement>();
#if MONO_BUILD
                var listItemsToKeep = new List<ListViewItem>();
#endif
                foreach (ListViewItem zLvi in listViewElements.Items)
                {
                    var zElement = (ProjectLayoutElement)zLvi.Tag;
                    if (!zLvi.Selected)
                    {
                        listToKeep.Add(zElement);
#if MONO_BUILD
                        listItemsToKeep.Add(zLvi);
#endif
                    }
                    else
                    {
                        m_dictionaryItems.Remove(zElement.name);
                        listToDelete.Add(zLvi);
                    }
                }

#if MONO_BUILD  // HACK: mono has had a bug for years with .remove :(
                listViewElements.Items.Clear();
                listViewElements.Items.AddRange(listItemsToKeep.ToArray());
#else
                foreach (var zLvi in listToDelete)
                {
                    listViewElements.Items.Remove(zLvi);
                }
#endif
                SetupLayoutUndo(listToKeep);

                LayoutManager.Instance.ActiveLayout.Element = listToKeep.ToArray();
                LayoutManager.Instance.FireLayoutUpdatedEvent(true);
            }

        }

        private void btnElementRename_Click(object sender, EventArgs e)
        {
            if (1 != listViewElements.SelectedItems.Count)
            {
                return;
            }
            const string NAME = "NAME";
            var zElement = (ProjectLayoutElement)listViewElements.SelectedItems[0].Tag;
            var zQuery = new QueryPanelDialog("Rename", 350, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddTextBox("Name: ", zElement.name, false, NAME);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                string sName = zQuery.GetString(NAME).Trim();
                if (!m_dictionaryItems.ContainsKey(sName))
                {
                    // UserAction
                    var lvItem = listViewElements.SelectedItems[0];
                    var sRedoName = sName;
                    var sUndoName = zElement.name;
                    UserAction.PushAction(bRedo =>
                    {
                        string sOldName = bRedo ? sUndoName : sRedoName;
                        string sNewName = bRedo ? sRedoName : sUndoName;

                        RenameElement(zElement, lvItem, sOldName, sNewName);
                    });

                    RenameElement(zElement, lvItem, zElement.name, sName);

                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                }
                else
                {
                    MessageBox.Show(this, "The new name already exists!", "Duplicate Name", MessageBoxButtons.OK);
                }
            }
        }

        private void btnElementChangeOrder_Click(object sender, EventArgs e)
        {
            int nChange = (sender == btnElementDown) ? 1 : -1;
            ChangeElementOrder(nChange);
        }

        private void numericCardIndex_ValueChanged(object sender, EventArgs e)
        {
            var nTargetIndex = (int)numericCardIndex.Value - 1;
            m_nDestinationCardIndex = nTargetIndex;
            m_zLastProjectLayout = LayoutManager.Instance.ActiveLayout;
            ChangeCardIndex(nTargetIndex);
            m_bFireLayoutChangeEvents = false;
            numericRowIndex.Value = m_arrayIndexToRow[nTargetIndex] + 1;
            m_bFireLayoutChangeEvents = true;
        }


        private void numericRowIndex_ValueChanged(object sender, EventArgs e)
        {
            // use the numeric card index to control this
            if (m_bFireLayoutChangeEvents)
            {
                numericCardIndex.Value = m_arrayRowToIndex[(int)numericRowIndex.Value - 1] + 1;
            }
        }

        private void btnGenCards_Click(object sender, EventArgs e)
        {
            if (LayoutManager.Instance.ActiveDeck.CardLayout.Reference != null &&
                LayoutManager.Instance.ActiveDeck.CardLayout.Reference.Length > 0)
            {
                FormUtils.ShowErrorMessage("You cannot assign a default card count to a layout with an associated reference.");
                return;
            }
            const string CARD_COUNT = "CARD_COUNT";
            var zQuery = new QueryPanelDialog("Default Card Count", 240, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            zQuery.AddNumericBox("Card Count", LayoutManager.Instance.ActiveDeck.CardLayout.defaultCount, 1, int.MaxValue, CARD_COUNT);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                LayoutManager.Instance.ActiveLayout.defaultCount = (int)zQuery.GetDecimal(CARD_COUNT);
                LayoutManager.Instance.InitializeActiveLayout();
                LayoutManager.Instance.FireLayoutUpdatedEvent(true);
            }
        }

        private void listViewElements_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        ChangeElementOrder(-1);
                        e.Handled = true; // block the up action
                        break;
                    case Keys.Down:
                        ChangeElementOrder(1);
                        e.Handled = true; // block the down action
                        break;
                }
            }
        }

        private void listViewElements_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewElements);
        }

        private void listViewElements_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listSelectedElements = new List<ProjectLayoutElement>(listViewElements.SelectedItems.Count);
            for (var nIdx = 0; nIdx < listViewElements.SelectedItems.Count; nIdx++)
            {
                listSelectedElements.Add((ProjectLayoutElement)listViewElements.SelectedItems[nIdx].Tag);
            }

            // todo: multi layout selection is broken! order is messed up causing elements to overwrite one another

            ElementManager.Instance.FireElementSelectedEvent(listSelectedElements);
        }

        private void listViewElements_DoubleClick(object sender, EventArgs e)
        {
            if (1 == listViewElements.SelectedItems.Count)
            {
                ToggleEnableState();
            }
        }

        private void listViewElements_KeyPress(object sender, KeyPressEventArgs e)
        {

            switch (e.KeyChar)
            {
                case ' ':
                    ToggleEnableState();
                    break;
            }
        }

        private void MDILayoutControl_Load(object sender, EventArgs e)
        {
            listViewElements_Resize(sender, new EventArgs());
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (0 < listViewElements.SelectedItems.Count)
            {
                m_listClipboardElements.Clear();
                for (int nIdx = 0; nIdx < listViewElements.SelectedItems.Count; nIdx++)
                {
                    m_listClipboardElements.Add((ProjectLayoutElement)listViewElements.SelectedItems[nIdx].Tag);
                }
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (0 < m_listClipboardElements.Count)
            {
                m_listClipboardElements.ForEach(x => AddElements(new string[] { x.name }, x));
            }
        }

        private void pasteSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 != m_listClipboardElements.Count)
            {
                return;
            }
            var zSourceElement = m_listClipboardElements[0];
            var zQuery = new QueryPanelDialog("Apply Element Settings", 400, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            const string SETTINGS_TO_COPY = "settings_to_copy";

            // TODO: if this ever expands to more fields just use a dictionary.contains
            var listProperties = ProjectLayoutElement.SortedPropertyInfos.Where(x => !x.Name.Equals("name")).ToList();

            zQuery.AddLabel("Select the settings to apply to the selected Elements.", 40);
            zQuery.AddListBox("Settings to apply", listProperties.Select(x => x.Name).ToArray(), null, true, 400, SETTINGS_TO_COPY);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var dictionaryNewElementValues = new Dictionary<PropertyInfo, object>();
                var dictionaryOldElementsValues = new Dictionary<ProjectLayoutElement, Dictionary<PropertyInfo, object>>();
                // construct the set of values from the source element
                foreach (var nIdx in zQuery.GetIndices(SETTINGS_TO_COPY))
                {
                    dictionaryNewElementValues.Add(listProperties[nIdx], listProperties[nIdx].GetValue(zSourceElement, null));
                }
                // construct the set of values from the destination element(s)
                foreach (var zElement in ElementManager.Instance.SelectedElements)
                {
                    var dictionaryOldValues = new Dictionary<PropertyInfo, object>();
                    dictionaryOldElementsValues.Add(zElement, dictionaryOldValues);
                    foreach (var zEntry in dictionaryNewElementValues)
                    {
                        dictionaryOldValues.Add(zEntry.Key, zEntry.Key.GetValue(zElement, null));
                    }
                }

                UserAction.PushAction(bRedo =>
                {
                    CardMakerInstance.ProcessingUserAction = true;

                    foreach (var zElementToValue in dictionaryOldElementsValues)
                    {
                        foreach (var zEntry in zElementToValue.Value)
                        {
                            // pull the value from the old element dictionary or the source element
                            var zValue = bRedo ? dictionaryNewElementValues[zEntry.Key] : zEntry.Value;
                            zEntry.Key.SetValue(zElementToValue.Key, zValue, null);
                        }
                        // re-init any translated string values (colors/fonts) 
                        // TODO: consider using an event for this kind of thing...
                        zElementToValue.Key.InitializeTranslatedFields();
                    }

                    CardMakerInstance.ProcessingUserAction = false;

                    // clear the deck cache so element translations are re-processed
                    LayoutManager.Instance.ActiveDeck.ResetDeckCache();
                    // trigger a re-select (this will cause the element window to update)
                    listViewElements_SelectedIndexChanged(null, null);
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                }, true);
            }
        }

        private void contextMenuElements_Opening(object sender, CancelEventArgs e)
        {
            if (0 == m_listClipboardElements.Count && 0 == listViewElements.SelectedItems.Count)
                e.Cancel = true;

            copyToolStripMenuItem.Enabled = 0 != listViewElements.SelectedItems.Count;
            pasteToolStripMenuItem.Enabled = 0 < m_listClipboardElements.Count;
            pasteSettingsToolStripMenuItem.Enabled = 1 == m_listClipboardElements.Count;
        }

        private void resize_Click(object sender, EventArgs e)
        {
            var zQuery = new QueryPanelDialog("Resize Elements", 500, false);
            const string WIDTH_ADJUST = "widthadjust";
            const string HEIGHT_ADJUST = "heightadjust";
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            zQuery.AddNumericBox("Width Adjust", 0, -65536, 65536, WIDTH_ADJUST);
            zQuery.AddNumericBox("Height Adjust", 0, -65536, 65536, HEIGHT_ADJUST);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var nWidthAdjust = (int)zQuery.GetDecimal(WIDTH_ADJUST);
                var nHeightAdjust = (int)zQuery.GetDecimal(HEIGHT_ADJUST);
                ElementManager.Instance.ProcessSelectedElementsChange(0, 0, nWidthAdjust, nHeightAdjust);
            }
        }

        private void btnScale_Click(object sender, EventArgs e)
        {
            var zQuery = new QueryPanelDialog("Resize Elements", 500, false);
            const string WIDTH_ADJUST = "widthadjust";
            const string HEIGHT_ADJUST = "heightadjust";
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            zQuery.AddNumericBox("Width Scale", 1, 0.001m, 1000, 0.001m, 3, WIDTH_ADJUST);
            zQuery.AddNumericBox("Height Scale", 1, 0.001m, 1000, 0.001m, 3, HEIGHT_ADJUST);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var dWidthAdjust = zQuery.GetDecimal(WIDTH_ADJUST);
                var dHeightAdjust = zQuery.GetDecimal(HEIGHT_ADJUST);
                ElementManager.Instance.ProcessSelectedElementsChange(0, 0, 0, 0, dWidthAdjust, dHeightAdjust);
            }
        }

        private void btnConfigureSize_Click(object sender, EventArgs e)
        {
            const string CARD_WIDTH = "cardWidth";
            const string CARD_HEIGHT = "cardHeight";
            var zQuery = new QueryPanelDialog("CardMaker Settings", 450, 250, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            zQuery.SetMaxHeight(600);
            zQuery.AddPullDownBox("Unit of Measure", Enum.GetNames(typeof(MeasurementUnit)), (int)CardMakerSettings.PrintPageMeasurementUnit, IniSettings.PrintPageMeasurementUnit);
            zQuery.AddNumericBox("Width", 10, 0, int.MaxValue, 1, 2, CARD_WIDTH);
            zQuery.AddNumericBox("Height", 10, 0, int.MaxValue, 1, 2, CARD_HEIGHT);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                switch ((MeasurementUnit) zQuery.GetIndex(IniSettings.PrintPageMeasurementUnit))
                {
                    case MeasurementUnit.Inch:
                        numericCardSetWidth.Value = zQuery.GetDecimal(CARD_WIDTH) * numericCardSetDPI.Value;
                        numericCardSetHeight.Value = zQuery.GetDecimal(CARD_HEIGHT) * numericCardSetDPI.Value;
                        break;
                    case MeasurementUnit.Millimeter:
                        numericCardSetWidth.Value = (decimal)(MeasurementUtil.GetInchesFromMillimeter((double)zQuery.GetDecimal(CARD_WIDTH)) * (double)numericCardSetDPI.Value);
                        numericCardSetHeight.Value = (decimal)(MeasurementUtil.GetInchesFromMillimeter((double)zQuery.GetDecimal(CARD_HEIGHT)) * (double)numericCardSetDPI.Value);
                        break;
                    case MeasurementUnit.Centimeter:
                        numericCardSetWidth.Value = (decimal)(MeasurementUtil.GetInchesFromCentimeter((double)zQuery.GetDecimal(CARD_WIDTH)) * (double)numericCardSetDPI.Value);
                        numericCardSetHeight.Value = (decimal)(MeasurementUtil.GetInchesFromCentimeter((double)zQuery.GetDecimal(CARD_HEIGHT)) * (double)numericCardSetDPI.Value);
                        break;
                }
            }
        }

        #endregion

        private ListViewItem CreateListViewItem(ProjectLayoutElement zElement)
        {
            var zLvi = new ListViewItem(new string[] { zElement.enabled.ToString(), zElement.name, zElement.type })
            {
                Tag = zElement
            };
            UpdateListViewItemText(zLvi, zElement);
            m_dictionaryItems.Add(zElement.name, zLvi);
            return zLvi;
        }

        private void ClearSelection()
        {
            while (0 < listViewElements.SelectedItems.Count)
            {
                listViewElements.SelectedItems[0].Selected = false;
            }
        }

        private void ChangeSelectedElement(int nChange)
        {
            if (1 == listViewElements.SelectedIndices.Count)
            {
                if ((nChange == -1 && 0 == listViewElements.SelectedIndices[0]) ||
                    (nChange == 1 && (listViewElements.SelectedIndices[0] == listViewElements.Items.Count - 1)))
                {
                    return;
                }
                int nSelectedIndex = listViewElements.SelectedItems[0].Index;
                listViewElements.SelectedItems[0].Selected = false;
                ListViewItem zItem = listViewElements.Items[nSelectedIndex + nChange];
                zItem.Selected = true;
                zItem.EnsureVisible();
            }
        }

        private void ChangeSelectedElement(string sName)
        {
            ClearSelection();
            ListViewItem zItem;
            if (m_dictionaryItems.TryGetValue(sName, out zItem))
            {
                zItem.Selected = true;
                zItem.EnsureVisible();
            }
        }

        private void AddElements(IEnumerable<string> collectionNames, ProjectLayoutElement zBaseElement)
        {
            // construct a new list of elements
            var listElements = new List<ProjectLayoutElement>();
            if (null != LayoutManager.Instance.ActiveLayout.Element)
            {
                listElements.AddRange(LayoutManager.Instance.ActiveLayout.Element);
            }

            foreach (string sName in collectionNames)
            {
                string sTrimmed = sName.Trim();
                if (m_dictionaryItems.ContainsKey(sTrimmed)) // no duplicates!
                {
                    continue;
                }
                var zCardElement = new ProjectLayoutElement(sTrimmed);

                if (null != zBaseElement)
                {
                    zCardElement.DeepCopy(zBaseElement, true);
                }
                else
                {
                    zCardElement.lineheight = 14;
                    zCardElement.SetElementColor(Color.Black);
                    zCardElement.SetElementFont(FontLoader.DefaultFont);
                }
                listElements.Add(zCardElement);
                ListViewItem zLvi = CreateListViewItem(zCardElement);
                listViewElements.Items.Add(zLvi);
            }

            var zLayout = LayoutManager.Instance.ActiveLayout;
            if (null == zLayout.Element ||
                // it is possible nothing was added if all names were duplicates (skip in that case)
                zLayout.Element.Length < listElements.Count)
            {
                // UserAction
                SetupLayoutUndo(listElements);

                // assign the new list to the actual project layout
                LayoutManager.Instance.ActiveLayout.Element = listElements.ToArray();
                LayoutManager.Instance.FireLayoutUpdatedEvent(true);
            }
        }

        private void HandleCardSetValueChange(object sender, EventArgs e)
        {
            if (!m_bFireLayoutChangeEvents)
            {
                return;
            }
            if (sender == numericCardSetWidth)
            {
                LayoutManager.Instance.ActiveLayout.width = (int)numericCardSetWidth.Value;
            }
            else if (sender == numericCardSetHeight)
            {
                LayoutManager.Instance.ActiveLayout.height = (int)numericCardSetHeight.Value;
            }
            else if (sender == numericCardSetBuffer)
            {
                LayoutManager.Instance.ActiveLayout.buffer = (int)numericCardSetBuffer.Value;
            }
            else if (sender == numericCardSetDPI)
            {
                LayoutManager.Instance.ActiveLayout.dpi = (int)numericCardSetDPI.Value;
            }
            else if (sender == checkCardSetDrawBorder)
            {
                LayoutManager.Instance.ActiveLayout.drawBorder = checkCardSetDrawBorder.Checked;
            }
            else if (sender == checkLoadAllReferences)
            {
                LayoutManager.Instance.ActiveLayout.combineReferences = checkLoadAllReferences.Checked;
            }
            LayoutManager.Instance.FireLayoutUpdatedEvent(true);
        }

        private void ChangeElementOrder(int nChange)
        {
            if (0 == listViewElements.SelectedItems.Count)
            {
                return;
            }
#if !MONO_BUILD
            Win32.SetRedraw(listViewElements.Handle, false);
#endif
            ListViewAssist.MoveListViewItems(listViewElements, nChange);
#if !MONO_BUILD
            Win32.SetRedraw(listViewElements.Handle, true);
#endif
            listViewElements.Invalidate();

            listViewElements.SelectedItems[0].EnsureVisible();

            var listElements = new List<ProjectLayoutElement>();
            foreach (ListViewItem zLvi in listViewElements.Items)
            {
                listElements.Add((ProjectLayoutElement)zLvi.Tag);
            }

            // UserAction
            SetupLayoutUndo(listElements);

            LayoutManager.Instance.ActiveLayout.Element = listElements.ToArray();
            LayoutManager.Instance.FireLayoutUpdatedEvent(true);
        }

        private void RenameElement(ProjectLayoutElement zElement, ListViewItem lvItem, string sOldName, string sNewName)
        {
            m_dictionaryItems.Remove(sOldName);
            zElement.name = sNewName;
            lvItem.SubItems[1].Text = zElement.name;
            // update dictionary
            m_dictionaryItems.Add(zElement.name, listViewElements.SelectedItems[0]);
        }

        private void ChangeCardIndex(int nDesiredIndex)
        {
            LayoutManager.Instance.ActiveDeck.CardIndex = nDesiredIndex;
            LayoutManager.Instance.FireDeckIndexChangedEvent();
        }

        private void SetupCardIndices(int nMaxIndex)
        {
            // TODO: this is only used in one place
            var listDeckLines = LayoutManager.Instance.ActiveDeck.ValidLines;
            m_arrayIndexToRow = new int[listDeckLines.Count];
            var listRowToIndex = new List<int> {0};
            var nCurrentRow = 0;
            for (var nIdx = 0; nIdx < listDeckLines.Count; nIdx++)
            {
                if (listDeckLines[nIdx].RowSubIndex == 0 && nIdx != 0)
                {
                    nCurrentRow++;
                    listRowToIndex.Add(nIdx);
                }
                m_arrayIndexToRow[nIdx] = nCurrentRow;
            }
            m_arrayRowToIndex = listRowToIndex.ToArray();

            // be sure to set the index back to 1 before changing the max!
            numericCardIndex.Value = 1;
            numericRowIndex.Value = 1;
            
            numericCardIndex.Minimum = 1;
            numericCardIndex.Maximum = nMaxIndex;
            lblIndex.Text = "/" + nMaxIndex;

            numericRowIndex.Minimum = 1;
            numericRowIndex.Maximum = m_arrayRowToIndex.Length;
        }

        /// <summary>
        /// Configures the controls to match the Layout settings
        /// </summary>
        /// <param name="zLayout"></param>
        private void UpdateLayoutInfo(ProjectLayout zLayout)
        {
            if (null != zLayout)
            {
                // don't trigger any events (this is just setup)
                m_bFireLayoutChangeEvents = false;

                // get the destination index before changing the controls
                // (the value is lost when the controls are adjusted)
                int nDestinationCardIndex = m_nDestinationCardIndex;

                // configure the UI based on the newly loaded item
                numericCardSetBuffer.Value = zLayout.buffer;
                numericCardSetWidth.Value = zLayout.width;
                numericCardSetHeight.Value = zLayout.height;
                numericCardSetDPI.Value = zLayout.dpi;
                checkCardSetDrawBorder.Checked = zLayout.drawBorder;
                checkLoadAllReferences.Checked = zLayout.combineReferences;
                SetupCardIndices(LayoutManager.Instance.ActiveDeck.CardCount);
                groupBoxCardCount.Enabled = true;
                groupBoxCardSet.Enabled = true;

                // update the list of elements
                listViewElements.Items.Clear();
                m_dictionaryItems.Clear();
                if (null != zLayout.Element)
                {
                    foreach (ProjectLayoutElement zElement in zLayout.Element)
                    {
                        ListViewItem zLvi = CreateListViewItem(zElement);
                        UpdateListViewItemText(zLvi, zElement);
                        listViewElements.Items.Add(zLvi);
                    }
                    if (0 < listViewElements.Items.Count)
                    {
                        listViewElements.Items[0].Selected = true;
                    }
                }
                else
                {
                    ElementManager.Instance.FireElementSelectedEvent(null);
                }
                m_bFireLayoutChangeEvents = true;

                // these adjustments will trigger the events necessary to adjust to the given index
                if (LayoutManager.Instance.ActiveLayout == m_zLastProjectLayout && -1 != nDestinationCardIndex &&
                    LayoutManager.Instance.ActiveDeck.CardCount > nDestinationCardIndex)
                {
                    numericCardIndex.Value = nDestinationCardIndex + 1;
                }
                else
                {
                    numericCardIndex.Value = 1;
                }
                // just in case the value is considered unchanged, fire off the event
                ChangeCardIndex((int)numericCardIndex.Value - 1);
            }
            else
            {
                groupBoxCardCount.Enabled = false;
                groupBoxCardSet.Enabled = false;
            }
        }

        private void UpdateListViewItemText(ListViewItem zLvi, ProjectLayoutElement zElement)
        {
            //zLvi.BackColor = zElement.enabled ? Color.White : Color.Tomato;
            zLvi.SubItems[0].Text = zElement.enabled.ToString();
        }

        private void ToggleEnableState()
        {
            ElementManager.Instance.ProcessSelectedElementsEnableToggle();
        }

        private void RefreshElementInformation()
        {
            foreach (var zLvi in m_dictionaryItems.Values)
            {
                var zElement = (ProjectLayoutElement) zLvi.Tag;
                zLvi.SubItems[2].Text = zElement.type;
                UpdateListViewItemText(zLvi, zElement);
            }
        }

        private void SetupLayoutUndo(List<ProjectLayoutElement> listNewLayoutElements)
        {
            if (!CardMakerInstance.ProcessingUserAction)
            {
                var arrayUndo = LayoutManager.Instance.ActiveLayout.Element;
                var arrayRedo = listNewLayoutElements.ToArray();

                UserAction.PushAction(bRedo =>
                {
                    CardMakerInstance.ProcessingUserAction = true;

                    // restore items
                    m_dictionaryItems.Clear();
                    listViewElements.Items.Clear();
                    ProjectLayoutElement[] arrayChange = bRedo ? arrayRedo : arrayUndo;
                    LayoutManager.Instance.ActiveLayout.Element = arrayChange;
                    if (arrayChange != null)
                    {
                        foreach (ProjectLayoutElement zElement in arrayChange)
                        {
                            var zLvi = CreateListViewItem(zElement);
                            listViewElements.Items.Add(zLvi);
                        }
                    }
                    else
                    {
                        // TODO: force redraw
                    }

                    CardMakerInstance.ProcessingUserAction = false;

                    listViewElements_SelectedIndexChanged(null, null);
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                });
            }
        }
    }
}