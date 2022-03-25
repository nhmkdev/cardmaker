////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using System.Linq;
using System.Windows.Forms;
using CardMaker.Card;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.XML;
using Support.IO;
using Support.UI;
using LayoutEventArgs = CardMaker.Events.Args.LayoutEventArgs;

namespace CardMaker.Events.Managers
{
    /// <summary>
    /// Handles Layout related communication between components
    /// </summary>
    public class LayoutManager
    {
        private static LayoutManager m_zInstance;
        
        public Deck ActiveDeck { get; private set; }

        public ProjectLayout ActiveLayout { get; private set; }

        /// <summary>
        /// Fired when a layout select is requested
        /// </summary>
        public event LayoutSelectRequested LayoutSelectRequested;

        /// <summary>
        /// Fired when a layout is loaded (deck is loaded)
        /// </summary>
        public event LayoutLoaded LayoutLoaded;

        /// <summary>
        /// Fired when a layout is altered (including when a child element is altered)
        /// </summary>
        public event LayoutUpdated LayoutUpdated;

        /// <summary>
        /// Fired when a layout is altered in a way that only affects rendering (no persistence needed)
        /// </summary>
        public event LayoutUpdated LayoutRenderUpdated;

        /// <summary>
        /// Fired when the order of the elements is requested
        /// </summary>
        public event LayoutElementOrderRequest ElementOrderAdjustRequest;

        /// <summary>
        /// Fired when a selected element change is requested
        /// </summary>
        public event LayoutElementSelectRequest ElementSelectAdjustRequest;

        /// <summary>
        /// Fired when the deck index changes
        /// </summary>
        public event DeckIndexChanged DeckIndexChanged;

        /// <summary>
        /// Fired when there is a request to change the deck index
        /// </summary>
        public event DeckIndexChanged DeckIndexChangeRequested;

        public static LayoutManager Instance => m_zInstance ?? (m_zInstance = new LayoutManager());

        #region Event Triggers

        public void FireLayoutSelectRequested(ProjectLayout zLayout)
        {
            LayoutSelectRequested?.Invoke(this, new LayoutEventArgs(zLayout));
        }

        public void FireDeckIndexChangeRequested(int nIdx)
        {
            DeckIndexChangeRequested?.Invoke(this, new DeckChangeEventArgs(ActiveDeck, nIdx));
        }

        //TODO: (might even want to include the origin sender)
        /// <summary>
        /// This is the event for a modification to the layout/elements requiring save and re-draw
        /// </summary>
        public void FireLayoutUpdatedEvent(bool bDataChanged)
        {
            LayoutUpdated?.Invoke(this, new LayoutEventArgs(ActiveLayout, ActiveDeck, bDataChanged));
        }

        /// <summary>
        /// Fires the LayoutRenderUpdated event
        /// </summary>
        public void FireLayoutRenderUpdatedEvent()
        {
            LayoutRenderUpdated?.Invoke(this, new LayoutEventArgs(ActiveLayout));
        }

        /// <summary>
        /// This even is fired when the deck index has been changed
        /// </summary>
        public void FireDeckIndexChangedEvent()
        {
            DeckIndexChanged?.Invoke(this, new DeckChangeEventArgs(ActiveDeck));
        }

        // TODO: this "request" event should make the actual change and then fire the event (??)
        /// <summary>
        /// Fires the ElementSelectAdjustRequest event
        /// </summary>
        /// <param name="nIndexAdjust"></param>
        public void FireElementOrderAdjustRequest(int nIndexAdjust)
        {
            ElementOrderAdjustRequest?.Invoke(this, new LayoutElementNumericAdjustEventArgs(nIndexAdjust));
        }

        /// <summary>
        /// Fires the ElementSelectAdjustRequest event
        /// </summary>
        /// <param name="nAdjust"></param>
        public void FireElementSelectAdjustRequest(int nAdjust)
        {
            ElementSelectAdjustRequest?.Invoke(this, new LayoutElementNumericAdjustEventArgs(nAdjust));
        }

        #endregion

        /// <summary>
        /// Sets the active layout and initializes it
        /// </summary>
        /// <param name="zLayout"></param>
        public void SetActiveLayout(ProjectLayout zLayout)
        {
            ActiveLayout = zLayout;
            if (null == zLayout)
            {
                ActiveDeck = null;
            }
            InitializeActiveLayout();
        }

        /// <summary>
        /// Refreshes the Deck assocaited with the current Layout (and fires the LayoutLoaded event)
        /// </summary>
        public void InitializeActiveLayout()
        {
            if (null != ActiveLayout)
            {
                ActiveDeck = new Deck();

                ActiveDeck.SetAndLoadLayout(ActiveLayout, false, null);
            }

            LayoutLoaded?.Invoke(this, new LayoutEventArgs(ActiveLayout, ActiveDeck));
        }

        /// <summary>
        /// Initialize the cache for each element in the ProjectLayout
        /// </summary>
        /// <param name="zLayout">The layout to initialize the cache</param>
        public static void InitializeElementCache(ProjectLayout zLayout)
        {
            // mark all fields as specified
            if (null == zLayout.Element) return;
            foreach (var zElement in zLayout.Element)
            {
                zElement.InitializeTranslatedFields();
            }
        }

        /// <summary>
        /// Gets the element that has the matching name
        /// </summary>
        /// <param name="sName"></param>
        /// <returns>The ProjectLayoutElement or null if not found</returns>
        public ProjectLayoutElement GetElement(string sName)
        {
            if (ActiveLayout?.Element != null)
            {
                return ActiveLayout.Element.FirstOrDefault(zElement => zElement.name.Equals(sName));
            }
            return null;
        }

        /// <summary>
        /// Refreshes the active layout
        /// </summary>
        public void RefreshActiveLayout()
        {
            if (ActiveDeck != null)
            {
                CardMakerInstance.ForceDataCacheRefresh = true;
                InitializeActiveLayout();
                CardMakerInstance.ForceDataCacheRefresh = false;
            }
        }

        /// <summary>
        /// Resets the image cache(s)
        /// </summary>
        public void ClearImageCache()
        {
            ImageCache.ClearImageCaches();
            ActiveDeck?.ResetDeckCache();
            FireLayoutRenderUpdatedEvent();
            Logger.AddLogLine("Cleared Image Cache");
        }

        public void HandleLayoutElementNameChange(object sender, ElementRenamedEventArgs e)
        {
            ActiveDeck.CardLayout.ReInitializeElementLookup(e.Element, e.OldName);
        }

        /// <summary>
        /// Displays the adjust layout dialog (for scaling primarily)
        /// </summary>
        /// <param name="bCreateNew">Flag indicating that a copy of the specified layout should be created.</param>
        /// <param name="zLayout">The base layout to adjust or duplicate</param>
        public static void ShowAdjustLayoutSettingsDialog(bool bCreateNew, ProjectLayout zLayout, Form zParentForm)
        {
            var zQuery = new QueryPanelDialog(bCreateNew ? "Duplicate Layout (Custom)" : "Resize Layout", 450, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            const string LAYOUT_NAME = "layoutName";

            const string RESIZE_ADJUST_DIMENSIONS = "Dimensions";
            const string RESIZE_ADJUST_SCALE = "Scale";
            const string RESIZE_ADJUST_DPI = "DPI";
            const string RESIZE_ADJUST = "resizeAdjust";

            const string ELEMENTS_ADJUST_NOTHING = "Nothing";
            const string ELEMENTS_ADJUST_SCALE = "Scale";
            const string ELEMENTS_ADJUST_CENTER = "Center";
            const string ELEMENTS_ADJUST = "elementAdjust";
            const string DPI = "dpi";
            const string SCALE = "scale";
            const string WIDTH = "width";
            const string HEIGHT = "height";

            var listMeasureUnits = new List<string>();
            listMeasureUnits.AddRange(Enum.GetNames(typeof(MeasurementUnit)));
            listMeasureUnits.Add(MeasurementUtil.PIXEL);

            if (bCreateNew)
            {
                zQuery.AddTextBox("Layout Name", zLayout.Name + " copy", false, LAYOUT_NAME);
            }
            // intentionall start the index as non-zero
            var comboResizeType = zQuery.AddPullDownBox("Resize Type", new string[] { RESIZE_ADJUST_DIMENSIONS, RESIZE_ADJUST_SCALE, RESIZE_ADJUST_DPI }, 0, RESIZE_ADJUST);
            zQuery.AddLabel("Note: The DPI is only adjusted if DPI Resize Type is used.", 18);
            zQuery.AddVerticalSpace(10);
            var comboUnitMeasure = zQuery.AddPullDownBox("Unit of Measure",
                listMeasureUnits.ToArray(),
                (int)CardMakerSettings.PrintPageMeasurementUnit,
                IniSettings.PrintPageMeasurementUnit);

            // beware of the minimums as things like measurements definitely can be very small (<1)
            var numericWidth = zQuery.AddNumericBox("Width", 1, 0.001m, int.MaxValue, 1, 3, WIDTH);
            var numericHeight = zQuery.AddNumericBox("Height", 1, 0.001m, int.MaxValue, 1, 3, HEIGHT);
            var numericDPI = zQuery.AddNumericBox("Export DPI (Scales)", zLayout.dpi, 100, int.MaxValue, DPI);
            var numericScale = zQuery.AddNumericBox("Scale", 1, 0.01m, int.MaxValue, 0.01m, 3, SCALE);

            EventHandler zComboUnitMeasureAction = (sender, args) =>
            {
                decimal dWidth, dHeight;
                MeasurementUtil.GetMeasurement(zLayout.width, zLayout.height, zLayout.dpi, comboUnitMeasure.Text, out dWidth, out dHeight);
                numericWidth.Value = dWidth;
                numericHeight.Value = dHeight;
            };
            comboUnitMeasure.SelectedIndexChanged += zComboUnitMeasureAction;
            zComboUnitMeasureAction.Invoke(null, null);

            EventHandler zComboResizeTypeAction = (sender, args) =>
            {
                switch (comboResizeType.Text)
                {
                    case RESIZE_ADJUST_DIMENSIONS:
                        comboUnitMeasure.Enabled = numericWidth.Enabled = numericHeight.Enabled = true;
                        numericDPI.Enabled = numericScale.Enabled = false;
                        break;
                    case RESIZE_ADJUST_SCALE:
                        comboUnitMeasure.SelectedIndex = listMeasureUnits.Count - 1; // pixel index
                        numericScale.Enabled = true;
                        comboUnitMeasure.Enabled = numericDPI.Enabled = numericWidth.Enabled = numericHeight.Enabled = false;
                        break;
                    case RESIZE_ADJUST_DPI:
                        comboUnitMeasure.SelectedIndex = listMeasureUnits.Count - 1; // pixel index
                        numericDPI.Enabled = true;
                        comboUnitMeasure.Enabled = numericScale.Enabled = numericWidth.Enabled = numericHeight.Enabled = false;
                        break;
                }
            };
            comboResizeType.SelectedIndexChanged += zComboResizeTypeAction;
            zComboResizeTypeAction.Invoke(null, null);

            numericScale.ValueChanged += (sender, e) =>
            {
                var dScale = numericScale.Value / 1m;
                numericWidth.Value = (int)Math.Max(numericWidth.Minimum, ((decimal)zLayout.width * dScale));
                numericHeight.Value = (int)Math.Max(numericHeight.Minimum, ((decimal)zLayout.height * dScale));
                // do not adjust the DPI in any mode but the DPI mode
                //numericDPI.Value = (int) Math.Max(numericDPI.Minimum, ((decimal) zLayout.dpi * dScale));
            };

            numericDPI.ValueChanged += (sender, e) =>
            {
                var dScale = (decimal)numericDPI.Value / (decimal)zLayout.dpi;
                numericWidth.Value = (int)Math.Max(numericWidth.Minimum, ((decimal)zLayout.width * dScale));
                numericHeight.Value = (int)Math.Max(numericHeight.Minimum, ((decimal)zLayout.height * dScale));
                numericScale.Value = Math.Max(numericScale.Minimum, (1m * dScale));
            };

            zQuery.AddVerticalSpace(10);
            zQuery.AddPullDownBox("Element Adjustment", new string[] { ELEMENTS_ADJUST_NOTHING, ELEMENTS_ADJUST_SCALE, ELEMENTS_ADJUST_CENTER }, 0, ELEMENTS_ADJUST);

            // remove the accept button so the enter key does not automatically accept/close the dialog
            zQuery.Form.AcceptButton = null;

            if (DialogResult.OK == zQuery.ShowDialog(zParentForm))
            {
                var listUserActions = new List<Action<bool>>();
                var zLayoutAdjusted = bCreateNew ? new ProjectLayout(zQuery.GetString(LAYOUT_NAME)) : zLayout;

                var nOriginalWidth = zLayout.width;
                var nOriginalHeight = zLayout.height;
                var nOriginalDPI = zLayout.dpi;

                if (bCreateNew)
                {
                    zLayoutAdjusted.DeepCopy(zLayout);
                }

                int nNewWidth, nNewHeight;
                var nNewDPI = nOriginalDPI;

                switch (comboResizeType.Text)
                {
                    case RESIZE_ADJUST_DIMENSIONS:
                        MeasurementUtil.GetPixelMeasurement(zQuery.GetDecimal(WIDTH), zQuery.GetDecimal(HEIGHT), zLayout.dpi, comboUnitMeasure.Text, out nNewWidth, out nNewHeight);
                        break;
                    case RESIZE_ADJUST_DPI:
                        nNewWidth = (int)zQuery.GetDecimal(WIDTH);
                        nNewHeight = (int)zQuery.GetDecimal(HEIGHT);
                        // this is the only path where the DPI actually changes
                        nNewDPI = (int)zQuery.GetDecimal(DPI);
                        break;
                    default:
                        nNewWidth = (int)zQuery.GetDecimal(WIDTH);
                        nNewHeight = (int)zQuery.GetDecimal(HEIGHT);
                        break;
                }

                zLayoutAdjusted.width = nNewWidth;
                zLayoutAdjusted.height = nNewHeight;
                zLayoutAdjusted.dpi = nNewDPI;

                // create the user action for adjusting the layout settings
                listUserActions.Add((performAction) =>
                {
                    if (performAction)
                    {
                        zLayoutAdjusted.width = nNewWidth;
                        zLayoutAdjusted.height = nNewHeight;
                        zLayoutAdjusted.dpi = nNewDPI;
                    }
                    else
                    {
                        zLayoutAdjusted.width = nOriginalWidth;
                        zLayoutAdjusted.height = nOriginalHeight;
                        zLayoutAdjusted.dpi = nOriginalDPI;
                    }
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                });

                var bProcessedElementChange = false;
                // create the user action for adjusting the elements
                if (zLayout.Element != null)
                {
                    switch (zQuery.GetString(ELEMENTS_ADJUST))
                    {
                        case ELEMENTS_ADJUST_CENTER:
                            var nXAdjust = (int)(zLayoutAdjusted.width / 2) - (int)(nOriginalWidth / 2);
                            var nYAdjust = (int)(zLayoutAdjusted.height / 2) - (int)(nOriginalHeight / 2);
                            ElementManager.ProcessElementsChange(zLayoutAdjusted.Element, nXAdjust, nYAdjust, 0, 0, 1, 1, true, listUserActions);
                            bProcessedElementChange = true;
                            break;
                        case ELEMENTS_ADJUST_SCALE:
                            var dHorizontalScale = (decimal)zLayoutAdjusted.width / (decimal)nOriginalWidth;
                            var dVerticalScale = (decimal)zLayoutAdjusted.height / (decimal)nOriginalHeight;
                            ElementManager.ProcessElementsChange(zLayoutAdjusted.Element, 0, 0, 0, 0, dHorizontalScale, dVerticalScale, true, listUserActions);
                            bProcessedElementChange = true;
                            break;
                    }
                }
                if (!bProcessedElementChange)
                {
                    UserAction.PushActions(listUserActions);
                }

                if (bCreateNew)
                {
                    ProjectManager.Instance.AddLayout(zLayoutAdjusted);
                }
                else
                {
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                }
            }
        }
    }
}
