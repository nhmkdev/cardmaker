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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Support.UI
{
    public partial class RGBColorSelectDialog : Form
    {
        private int m_nPreviousColorIndex = -1;
        bool m_bEventsEnabled = true;
        private Color m_lastColor = Color.Black;

        readonly Bitmap m_BitmapHue;

        private const int PREVIOUS_COLOR_WIDTH = 16;

        private static readonly List<Color> s_listPreviousColors = new List<Color>();
        private static bool s_bZeroXChecked = true;

        // A delegate type for hooking up change notifications.
        public delegate void ColorPreviewEvent(object sender, Color zColor);

        public event ColorPreviewEvent PreviewEvent;

        private readonly PictureBoxSelectable[] m_arrayPictureBoxes;

        public RGBColorSelectDialog()
        {
            InitializeComponent();
            m_BitmapHue = new Bitmap(pictureColorHue.ClientSize.Width, pictureColorHue.ClientSize.Height);
            pictureColorHue.Image = m_BitmapHue;
            pictureColorHue.SelectionIndicator = PictureBoxSelectable.IndicatorType.Both;
            
            var m_bitmapRedToPurple = new Bitmap(pictureRedToPurple.ClientSize.Width, 255);
            var m_bitmapPurpleToBlue = new Bitmap(pictureRedToPurple.ClientSize.Width, 255);
            var m_bitmapBlueToTeal = new Bitmap(pictureRedToPurple.ClientSize.Width, 255);
            var m_bitmapTealToGreen = new Bitmap(pictureRedToPurple.ClientSize.Width, 255);
            var m_bitmapGreenToYellow = new Bitmap(pictureRedToPurple.ClientSize.Width, 255);
            var m_bitmapYellowToRed = new Bitmap(pictureRedToPurple.ClientSize.Width, 255);

            GenerateColorBar(m_bitmapRedToPurple, Color.FromArgb(255, 0, 0), Color.FromArgb(255, 0, 255));
            GenerateColorBar(m_bitmapPurpleToBlue, Color.FromArgb(255, 0, 255), Color.FromArgb(0, 0, 255));
            GenerateColorBar(m_bitmapBlueToTeal, Color.FromArgb(0, 0, 255), Color.FromArgb(0, 255, 255));
            GenerateColorBar(m_bitmapTealToGreen, Color.FromArgb(0, 255, 255), Color.FromArgb(0, 255, 0));
            GenerateColorBar(m_bitmapGreenToYellow, Color.FromArgb(0, 255, 0), Color.FromArgb(255, 255, 0));
            GenerateColorBar(m_bitmapYellowToRed, Color.FromArgb(255, 255, 0), Color.FromArgb(255, 0, 0));

            pictureRedToPurple.Image = m_bitmapRedToPurple;
            picturePurpleToBlue.Image = m_bitmapPurpleToBlue;
            pictureBlueToTeal.Image = m_bitmapBlueToTeal;
            pictureTealToGreen.Image = m_bitmapTealToGreen;
            pictureGreenToYellow.Image = m_bitmapGreenToYellow;
            pictureYellowToRed.Image = m_bitmapYellowToRed;

            pictureRedToPurple.Tag = m_bitmapRedToPurple;
            picturePurpleToBlue.Tag = m_bitmapPurpleToBlue;
            pictureBlueToTeal.Tag = m_bitmapBlueToTeal;
            pictureTealToGreen.Tag = m_bitmapTealToGreen;
            pictureGreenToYellow.Tag = m_bitmapGreenToYellow;
            pictureYellowToRed.Tag = m_bitmapYellowToRed;

            pictureRedToPurple.SelectionIndicator = PictureBoxSelectable.IndicatorType.HLine;
            picturePurpleToBlue.SelectionIndicator = PictureBoxSelectable.IndicatorType.HLine;
            pictureBlueToTeal.SelectionIndicator = PictureBoxSelectable.IndicatorType.HLine;
            pictureTealToGreen.SelectionIndicator = PictureBoxSelectable.IndicatorType.HLine;
            pictureGreenToYellow.SelectionIndicator = PictureBoxSelectable.IndicatorType.HLine;
            pictureYellowToRed.SelectionIndicator = PictureBoxSelectable.IndicatorType.HLine;

            m_arrayPictureBoxes = new [] {
                pictureRedToPurple,
                picturePurpleToBlue,
                pictureBlueToTeal,
                pictureTealToGreen,
                pictureGreenToYellow,
                pictureYellowToRed };

            // populate the previous colors

            if (0 < s_listPreviousColors.Count)
            {
                var zBmp = new Bitmap(panelPreviousColors.ClientRectangle.Width, panelPreviousColors.ClientRectangle.Height);
                var zGraphics = Graphics.FromImage(zBmp);
                var nXOffset = 0;
                foreach (var prevColor in s_listPreviousColors)
                {
                    zGraphics.FillRectangle(new SolidBrush(prevColor), nXOffset, 0, PREVIOUS_COLOR_WIDTH, panelPreviousColors.Height);
                    nXOffset += PREVIOUS_COLOR_WIDTH;
                }
                panelPreviousColors.BackgroundImage = zBmp;
            }

            UpdateHue(Color.Red);
        }

#if false
        public void SetColor(Color zColor)
        {
            UpdateColorBox(zColor);
            panelColor.BackColor = zColor;
            int nRed = 0;
            int nGreen = 0;
            int nBlue = 0;
            if (zColor.R > zColor.B)
            {
                nRed = zColor.R;
                if (zColor.G > zColor.B)
                {
                    nGreen = zColor.G;
                }
                else
                {
                    nBlue = zColor.B;
                }
            }
            else
            {
                nBlue = zColor.B;
                if (zColor.G > zColor.B)
                {
                    nGreen = zColor.G;
                }
                else
                {
                    nBlue = zColor.B;
                }
            }
            //Color colorSeek = Color.FromArgb(255, nRed, nGreen, nBlue);
            //for (int nIdx = 0; nIdx < m_BitmapColors.Height; nIdx++)
            //{
            //    Color pixelColor = m_BitmapColors.GetPixel(0, nIdx);
            //    Console.WriteLine(pixelColor + "::" + colorSeek);
            //    if (colorSeek == pixelColor)
            //    {
            //        pictureRedToPurple.Yposition = nIdx;
            //        break;
            //    }
            //}

        }
#endif

        public void UpdateColorBox(Color colorCurrent)
        {
            m_bEventsEnabled = false;
            numericRed.Value = colorCurrent.R;
            numericGreen.Value = colorCurrent.G;
            numericBlue.Value = colorCurrent.B;
            m_lastColor = colorCurrent;
            UpdateColorHexText();
            panelColor.BackColor = colorCurrent;

            m_bEventsEnabled = true;
            PreviewEvent?.Invoke(this, colorCurrent);
        }

        private void UpdateColorHexText()
        {
            txtHexColor.Text =
                (checkBoxAddZeroX.Checked ? "0x" : string.Empty) +
                m_lastColor.R.ToString("X").PadLeft(2, '0') +
                m_lastColor.G.ToString("X").PadLeft(2, '0') +
                m_lastColor.B.ToString("X").PadLeft(2, '0');            
        }

        public Color Color => Color.FromArgb((int)numericRed.Value, (int)numericGreen.Value, (int)numericBlue.Value);

        public void SetHueColor()
        {
            UpdateColorBox(m_BitmapHue.GetPixel(pictureColorHue.Xposition, pictureColorHue.Yposition));
        }


        private void GenerateColorBar(Bitmap zBmp, Color colorFrom, Color colorTo)
        {
            var zGraphics = Graphics.FromImage(zBmp);
            var zLinear = new LinearGradientBrush(new Rectangle(0, 0, zBmp.Width, zBmp.Height), colorFrom, colorTo, 90);
            zGraphics.FillRectangle(zLinear, new Rectangle(0, 0, zBmp.Width, zBmp.Height));
        }

        private void UpdateHue(Color zColor)
        {
            var zGraphics = Graphics.FromImage(m_BitmapHue);

            int nRedMax = zColor.R;
            int nGreenMax = zColor.G;
            int nBlueMax = zColor.B;

            var fRedInterval = (float)(255 - nRedMax) / (float)255;
            var fGreenInterval = (float)(255 - nGreenMax) / (float)255;
            var fBlueInterval = (float)(255 - nBlueMax) / (float)255;

            for (int nRow = 0; nRow < m_BitmapHue.Height; nRow++)
            {
                var nRed = (int)((float)nRedMax + (float)nRow * fRedInterval);
                var nGreen = (int)((float)nGreenMax + (float)nRow * fGreenInterval);
                var nBlue = (int)((float)nBlueMax + (float)nRow * fBlueInterval);

                if ((m_BitmapHue.Height - 1) == nRow)
                {
                    nRed = 255;
                    nGreen = 255;
                    nBlue = 255;
                }

                nRed = GetWithinRange(nRed, 0, 255);
                nGreen = GetWithinRange(nGreen, 0, 255);
                nBlue = GetWithinRange(nBlue, 0, 255);

                zGraphics.FillRectangle(
                    new LinearGradientBrush(new Point(0, 0), new Point(m_BitmapHue.Width -1, 0),
                        Color.Black, Color.FromArgb(nRed, nGreen, nBlue)),
                    new Rectangle(0, nRow, m_BitmapHue.Width, 1));
            }

            pictureColorHue.Invalidate();
        }

        public int GetWithinRange(int nValue, int nMin, int nMax)
        {
            if (nValue > nMax)
            {
                return nMax;
            }
            if (nValue < nMin)
            {
                return nMin;
            }
            return nValue;
        }

        private void numeric_ValueChanged(object sender, EventArgs e)
        {
            if(m_bEventsEnabled)
            {
                Color zColor = Color.FromArgb((int)numericRed.Value, (int)numericGreen.Value, (int)numericBlue.Value);
                UpdateColorBox(zColor);
            }
        }

        private void pictureColor_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
        }

        private void pictureColor_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        public void HandleMouseHueColor(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    UpdateColorBox(m_BitmapHue.GetPixel(pictureColorHue.Xposition, pictureColorHue.Yposition));
                    break;
            }
        }

        public void HandleMouseColor(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    var pictureBox = (PictureBoxSelectable)sender;
                    if ((-1 < e.Y) && (pictureBox.Image.Height > e.Y))
                    {
                        

                        Color colorSelected = ((Bitmap)pictureBox.Tag).GetPixel(0, e.Y);
                        UpdateHue(colorSelected);
                        SetHueColor();
                        foreach (PictureBoxSelectable picBox in m_arrayPictureBoxes)
                        {
                            if (picBox != pictureBox)
                            {
                                picBox.Yposition = 0;
                                picBox.Invalidate();
                            }
                        }
                    }
                    break;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Color colorSelected = Color.FromArgb((int)numericRed.Value, (int)numericGreen.Value, (int)numericBlue.Value);
            int nMaxPreviousColors = panelPreviousColors.Width / PREVIOUS_COLOR_WIDTH;

            if (0 == s_listPreviousColors.Count || 
                (0 < s_listPreviousColors.Count && s_listPreviousColors[0] != colorSelected))
            {

                // clean up any existing copies of the selected color
                int nIdx = 0;
                while (nIdx < s_listPreviousColors.Count)
                {
                    if (s_listPreviousColors[nIdx] != colorSelected)
                        nIdx++;
                    else
                        s_listPreviousColors.RemoveAt(nIdx);
                }

                // make room for the new "previous" color
                while (s_listPreviousColors.Count >= nMaxPreviousColors)
                    s_listPreviousColors.RemoveAt(s_listPreviousColors.Count - 1);

                s_listPreviousColors.Insert(0, colorSelected);
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void panelPreviousColors_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_nPreviousColorIndex < s_listPreviousColors.Count)
            {
                UpdateColorBox(Color.FromArgb(
                    s_listPreviousColors[m_nPreviousColorIndex].R,
                    s_listPreviousColors[m_nPreviousColorIndex].G,
                    s_listPreviousColors[m_nPreviousColorIndex].B));
            }
        }

        private void panelPreviousColors_MouseMove(object sender, MouseEventArgs e)
        {
            int nColorIndex = e.X / PREVIOUS_COLOR_WIDTH;
            
            if (m_nPreviousColorIndex == nColorIndex)
                return;

            m_nPreviousColorIndex = nColorIndex;
            if (m_nPreviousColorIndex < s_listPreviousColors.Count)
            {
                toolTipPreviouscolor.SetToolTip(panelPreviousColors,
                    "R:" + s_listPreviousColors[m_nPreviousColorIndex].R.ToString("000") + " " +
                    "G:" + s_listPreviousColors[m_nPreviousColorIndex].G.ToString("000") + " " +
                    "B:" + s_listPreviousColors[m_nPreviousColorIndex].B.ToString("000"));
            }
        }

        private void checkBoxAddZeroX_CheckedChanged(object sender, EventArgs e)
        {
            UpdateColorHexText();
        }

        private void RGBColorSelectDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            s_bZeroXChecked = checkBoxAddZeroX.Checked;
        }

        private void RGBColorSelectDialog_Load(object sender, EventArgs e)
        {
            checkBoxAddZeroX.Checked = s_bZeroXChecked;
        }

    }

    class PictureBoxSelectable : PictureBox
    {
        private int m_nPosX;
        private int m_nPosY;

        private IndicatorType m_eIndicatorType = IndicatorType.None;

        public IndicatorType SelectionIndicator 
        {
            get
            {
                return m_eIndicatorType;
            }
            set
            {
                m_eIndicatorType = value;
            }
        }

        public int Xposition
        {
            get
            {
                return m_nPosX;
            }
            set
            {
                m_nPosX = value;
            }
        }

        public int Yposition
        {
            get
            {
                return m_nPosY;
            }
            set
            {
                m_nPosY = value;
            }
        }

        public enum IndicatorType
        {
            VLine,
            HLine,
            Both,
            None
        }

        public PictureBoxSelectable()
        {
            MouseMove += PictureBoxSelectable_MouseMove;
        }

        void PictureBoxSelectable_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // clamp the selection to be inside the client
                if (ClientRectangle.Contains(e.X, 0))
                {
                    m_nPosX = e.X;
                }
                else if (e.X < ClientRectangle.Left)
                {
                    m_nPosX = ClientRectangle.Left;
                }
                else if (e.X > ClientRectangle.Right - 1)
                {
                    m_nPosX = ClientRectangle.Right - 1;
                }

                if (ClientRectangle.Contains(0, e.Y))
                {
                    m_nPosY = e.Y;
                }
                else if (e.Y < ClientRectangle.Top)
                {
                    m_nPosY = ClientRectangle.Top;
                }
                else if (e.Y > ClientRectangle.Bottom - 1)
                {
                    m_nPosY = ClientRectangle.Bottom - 1;
                }

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            switch (m_eIndicatorType)
            {
                case IndicatorType.HLine:
                    pe.Graphics.DrawLine(new Pen(Color.FromArgb(255, 255, 255, 255)), new Point(0, m_nPosY), new Point(ClientSize.Width, m_nPosY));
                    break;
                case IndicatorType.VLine:
                    pe.Graphics.DrawLine(new Pen(Color.FromArgb(255, 255, 255, 255)), new Point(m_nPosX, 0), new Point(m_nPosX, ClientSize.Height));
                    break;
                case IndicatorType.Both:
                    pe.Graphics.DrawLine(new Pen(Color.FromArgb(255, 255, 255, 255)), new Point(0, m_nPosY), new Point(ClientSize.Width, m_nPosY));
                    pe.Graphics.DrawLine(new Pen(Color.FromArgb(255, 255, 255, 255)), new Point(m_nPosX, 0), new Point(m_nPosX, ClientSize.Height));
                    break;
            }
        }
    }
}