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


using System;
using CardMaker.Card;
using CardMaker.Events.Args;
using CardMaker.XML;

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

        public static LayoutManager Instance
        {
            get
            {
                if (m_zInstance == null)
                {
                    m_zInstance = new LayoutManager();
                }
                return m_zInstance;
            }
        }

        #region Event Triggers

        public void FireLayoutSelectRequested(ProjectLayout zLayout)
        {
            if (null != LayoutSelectRequested)
            {
                LayoutSelectRequested(this, new LayoutEventArgs(zLayout));
            }
        }

        public void FireDeckIndexChangeRequested(int nIdx)
        {
            if (null != DeckIndexChangeRequested)
            {
                DeckIndexChangeRequested(this, new DeckChangeEventArgs(ActiveDeck, nIdx));
            }
        }

        //TODO: (might even want to include the origin sender)
        /// <summary>
        /// This is the event for a modification to the layout/elements requiring save and re-draw
        /// </summary>
        public void FireLayoutUpdatedEvent(bool bDataChanged)
        {
            if (null != LayoutUpdated)
            {
                LayoutUpdated(this, new LayoutEventArgs(ActiveLayout, ActiveDeck, bDataChanged));
            }
        }

        /// <summary>
        /// Fires the LayoutRenderUpdated event
        /// </summary>
        public void FireLayoutRenderUpdatedEvent()
        {
            if (null != LayoutRenderUpdated)
            {
                LayoutRenderUpdated(this, new LayoutEventArgs(ActiveLayout));
            }
        }

        /// <summary>
        /// This even is fired when the deck index has been changed
        /// </summary>
        public void FireDeckIndexChangedEvent()
        {
            if (null != DeckIndexChanged)
            {
                DeckIndexChanged(this, new DeckChangeEventArgs(ActiveDeck));
            }
        }

        // TODO: this "request" event should make the actual change and then fire the event (??)
        /// <summary>
        /// Fires the ElementSelectAdjustRequest event
        /// </summary>
        /// <param name="nIndexAdjust"></param>
        public void FireElementOrderAdjustRequest(int nIndexAdjust)
        {
            if (null != ElementOrderAdjustRequest)
            {
                ElementOrderAdjustRequest(this, new LayoutElementNumericAdjustEventArgs(nIndexAdjust));
            }
        }

        /// <summary>
        /// Fires the ElementSelectAdjustRequest event
        /// </summary>
        /// <param name="nAdjust"></param>
        public void FireElementSelectAdjustRequest(int nAdjust)
        {
            if (null != ElementSelectAdjustRequest)
            {
                ElementSelectAdjustRequest(this, new LayoutElementNumericAdjustEventArgs(nAdjust));
            }
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
            if (null != zLayout.Element)
            {
                foreach (var zElement in zLayout.Element)
                {
                    zElement.InitializeCache();
                }
            }
        }

        /// <summary>
        /// Gets the element that has the matching name
        /// </summary>
        /// <param name="sName"></param>
        /// <returns>The ProjectLayoutElement or null if not found</returns>
        public ProjectLayoutElement GetElement(string sName)
        {
            if (null != ActiveLayout && null != ActiveLayout.Element)
            {
                foreach (var zElement in ActiveLayout.Element)
                {
                    if (zElement.name.Equals(sName))
                    {
                        return zElement;
                    }
                }
            }
            return null;
        }
    }
}
