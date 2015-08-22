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

using Support.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CardMaker.Forms
{
    public partial class MDIPreview : Form
    {
        private static MDIPreview s_zInstance;
        private Point m_zLocation;

        private MDIPreview()
        {
            InitializeComponent();
        }

        public static MDIPreview Instance
        {
            get
            {
                s_zInstance = s_zInstance ?? new MDIPreview();
                return s_zInstance;
            }
        }

        public void SetImage(Bitmap zBitmap)
        {
            DrawScaledImage(zBitmap);
        }

        public void DrawScaledImage(Bitmap zBitmap)
        {
            var zGraphics = Graphics.FromImage(zBitmap);
            var nWidth = (int)((float)numericZoom.Value * zBitmap.Width);
            var nHeight = (int)((float)numericZoom.Value * zBitmap.Height);
            zGraphics.ScaleTransform((float)numericZoom.Value, (float)numericZoom.Value);
            if (0 < nWidth && 0 < nHeight)
            {
                var zScaledBitmap = new Bitmap(zBitmap, nWidth, nHeight);
                zGraphics.DrawImageUnscaled(zScaledBitmap, new Point(0, 0));
                picturePreview.Size = zScaledBitmap.Size;
                Text = "Preview [" + picturePreview.Width + "," + picturePreview.Height + "]";
                picturePreview.Image = zScaledBitmap;
            }
            zGraphics.Dispose();
        }

        private void MDIPreview_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseReason.UserClosing == e.CloseReason)
            {
                m_zLocation = Location;
                e.Cancel = true;
                Hide();
            }
        }

        private void panel_Resize(object sender, EventArgs e)
        {
            PanelEx.SetupScrollState(panel);
        }

        private void MDIPreview_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && !m_zLocation.IsEmpty)
            {
                Location = m_zLocation;
            }
        }

        private void numericZoom_ValueChanged(object sender, EventArgs e)
        {
            CardMakerMDI.Instance.DrawCardCanvas.UpdatePreview();
        }
    }
}