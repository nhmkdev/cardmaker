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


using System.Linq;
using CardMaker.Card;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Events.Managers
{
    /// <summary>
    /// Handles Layout related communication between components
    /// </summary>
    public class LayoutManager
    {
        private static LayoutManager m_zInstance;
        
        public Deck ActiveDeck { get; private set; }

#warning TODO: consider not having this public so ActiveDeck.CardLayout is the only source
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

                ActiveDeck.SetAndLoadLayout(ActiveLayout, false);
            }

            if (null != LayoutLoaded)
            {
                LayoutLoaded(this, new LayoutEventArgs(ActiveLayout, ActiveDeck));
            }            
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
            FireLayoutRenderUpdatedEvent();
            Logger.AddLogLine("Cleared Image Cache");
        }
    }
}
