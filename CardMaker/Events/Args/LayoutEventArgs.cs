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


using CardMaker.Card;
using CardMaker.XML;

namespace CardMaker.Events.Args
{
    public delegate void LayoutSelectRequested(object sender, LayoutEventArgs args);

    public delegate void LayoutSelected(object sender, LayoutEventArgs args);
    
    public delegate void LayoutLoaded(object sender, LayoutEventArgs args);
    
    public delegate void LayoutUpdated(object sender, LayoutEventArgs args);

    public delegate void LayoutAdded(object sender, LayoutEventArgs args);

    public class LayoutEventArgs
    {
        public ProjectLayout Layout { get; private set; }
        public Deck Deck { get; private set; }
        public bool DataChange { get; private set; }

        public LayoutEventArgs(ProjectLayout zLayout, Deck zDeck, bool bDataChange)
        {
            Layout = zLayout;
            Deck = zDeck;
            DataChange = bDataChange;
        }

        public LayoutEventArgs(ProjectLayout zLayout, Deck zDeck)
            : this(zLayout, zDeck, false)
        {

        }

        public LayoutEventArgs(ProjectLayout zLayout)
            : this(zLayout, null, false)
        {

        }
    }
}
