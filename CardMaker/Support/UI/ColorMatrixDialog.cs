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
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CardMaker.Data.Serialization;
using CardMaker.Forms;

namespace Support.UI
{
    public partial class ColorMatrixDialog : Form
    {
        private bool m_bSkipTextEvents = false;

        public delegate void ColorMatrixPreviewEvent(object sender, ColorMatrix zColorMatrix);
        public event ColorMatrixPreviewEvent PreviewEvent;
        public ColorMatrix OriginalColorMatrix { get; }
        public ColorMatrix ResultColorMatrix { get; private set; }

        public ColorMatrixDialog(ColorMatrix zColorMatrix)
        {
            InitializeComponent();
            OriginalColorMatrix = zColorMatrix;
        }

        private void PopulateColorMatrixText(ColorMatrix zColorMatrix, bool bSkipEvents = true)
        {
            if (null == zColorMatrix)
            {
                zColorMatrix = ColorMatrixSerializer.GetIdentityColorMatrix();
            }

            var sInput = ColorMatrixSerializer.SerializeToString(zColorMatrix);
            var arrayCurrentMatrixValues = sInput.Split(new char[] { ';', '\n', '\r' }, StringSplitOptions.None);
            // this should never happen but.... just in case.
            if (arrayCurrentMatrixValues.Length != 25)
            {
                arrayCurrentMatrixValues = Enumerable.Repeat((0.0f).ToString(), 25).ToArray();
            }

            var zBuilder = new StringBuilder();
            for (var y = 0; y < 5; y++)
            {
                zBuilder.AppendLine(string.Join(";", arrayCurrentMatrixValues, y * 5, 5));
            }

            m_bSkipTextEvents = bSkipEvents & true;
            txtColorMatrix.Text = zBuilder.ToString();
            m_bSkipTextEvents = false;
        }

        private void txtColorMatrix_TextChanged(object sender, EventArgs e)
        {
            if (m_bSkipTextEvents)
            {
                return;
            }
            var zColorMatrix = ColorMatrixSerializer.DeserializeFromString(txtColorMatrix.Text);
            if (null != zColorMatrix)
            {
                PreviewEvent?.Invoke(this, zColorMatrix);
            }
        }

        private void ColorMatrixDialog_Load(object sender, EventArgs e)
        {
            PopulateColorMatrixText(OriginalColorMatrix);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var zColorMatrix = ColorMatrixSerializer.DeserializeFromString(txtColorMatrix.Text);
            if (null == zColorMatrix)
            {
                FormUtils.ShowErrorMessage("Invalid Color Matrix specified");
                return;
            }

            ResultColorMatrix = zColorMatrix;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            PopulateColorMatrixText(OriginalColorMatrix, false);
        }

        private void btnIdentity_Click(object sender, EventArgs e)
        {
            PopulateColorMatrixText(ColorMatrixSerializer.GetIdentityColorMatrix(), false);
        }

        private void btnClipboard_Click(object sender, EventArgs e)
        {
            var zColorMatrix = ColorMatrixSerializer.DeserializeFromString(txtColorMatrix.Text);
            if (null == zColorMatrix)
            {
                FormUtils.ShowErrorMessage("Invalid Color Matrix specified");
                return;
            }

            Clipboard.SetText(ColorMatrixSerializer.SerializeToString(zColorMatrix));
        }
    }
}
