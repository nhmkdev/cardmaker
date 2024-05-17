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
using System.Drawing;
using System.Windows.Forms;
using CardMaker.Data;
using CardMaker.Data.Serialization;

namespace Support.UI
{
    public partial class GradientDialog : Form
    {
        private bool m_bSkipEvents = false;

        public delegate void GradientStringPreviewEvent(object sender, string sSerializedGradient, Color colorSource);

        public event GradientStringPreviewEvent PreviewEvent;
        public Color OriginalElementColor { get; }
        public string OriginalElementGradient { get; }
        
        public Color ElementColor { get; private set; }
        public string ElementGradient { get; private set; }

        public GradientDialog(Color colorElement, string sGradient)
        {
            InitializeComponent();

            tabLeftToRight.Tag = GradientStyle.lefttoright;
            tabPoints.Tag = GradientStyle.points;
            tabPointsNormalized.Tag = GradientStyle.pointsnormalized;

            numericPSourceX.Maximum = int.MaxValue;
            numericPSourceY.Maximum = int.MaxValue;
            numericPDestinationX.Maximum = int.MaxValue;
            numericPDestinationY.Maximum = int.MaxValue;

            OriginalElementColor = colorElement;
            OriginalElementGradient = sGradient;
        }

        public GradientDialog() : this(Color.Black, string.Empty)
        {
        }

        #region Form Events

        private void GradientDialog_Load(object sender, EventArgs e)
        {
            m_bSkipEvents = true;
            LoadGradient();
            m_bSkipEvents = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
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
            m_bSkipEvents = true;
            LoadGradient();
            m_bSkipEvents = false;
            OnConfigurationChange(sender, e);
        }

        private void btnSourceColor_Click(object sender, EventArgs e)
        {
            ShowColorSelectionDialog(panelSourceColor);
        }

        private void btnDestinationColor_Click(object sender, EventArgs e)
        {
            ShowColorSelectionDialog(panelDestinationColor);
        }

        private void btnRemoveGradient_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ElementColor = OriginalElementColor;
            ElementGradient = string.Empty;
            Close();
        }

        #endregion

        private void LoadGradient()
        {
            panelSourceColor.BackColor = OriginalElementColor;
            var eGradientStyle = GradientSerializer.GetGradientStyle(OriginalElementGradient);

            switch (eGradientStyle)
            {
                case GradientStyle.lefttoright:
                    var zAngleGradient = GradientSerializer.GetAngleGradient(OriginalElementGradient);
                    if (null != zAngleGradient)
                    {
                        numericLTRAngle.Value = (decimal)zAngleGradient.Angle;
                        panelDestinationColor.BackColor = zAngleGradient.DestinationColor;
                        tabGradients.SelectedTab = tabLeftToRight;
                    }
                    break;
                case GradientStyle.points:
                {
                    var zPointGradient = GradientSerializer.GetPointGradient(OriginalElementGradient);
                    if (null != zPointGradient)
                    {
                        panelDestinationColor.BackColor = zPointGradient.DestinationColor;
                        numericPSourceX.Value = (decimal)zPointGradient.Source.X;
                        numericPSourceY.Value = (decimal)zPointGradient.Source.Y;
                        numericPDestinationX.Value = (decimal)zPointGradient.Destination.X;
                        numericPDestinationY.Value = (decimal)zPointGradient.Destination.Y;
                        tabGradients.SelectedTab = tabPoints;
                    }
                }
                    break;
                case GradientStyle.pointsnormalized:
                {
                    var zPointGradient = GradientSerializer.GetPointGradient(OriginalElementGradient);
                    if (null != zPointGradient)
                    {
                        panelDestinationColor.BackColor = zPointGradient.DestinationColor;
                        numericPNSourceX.Value = (decimal)zPointGradient.Source.X;
                        numericPNSourceY.Value = (decimal)zPointGradient.Source.Y;
                        numericPNDestinationX.Value = (decimal)zPointGradient.Destination.X;
                        numericPNDestinationY.Value = (decimal)zPointGradient.Destination.Y;
                        tabGradients.SelectedTab = tabPointsNormalized;
                    }
                }
                    break;
            }
        }

        private void OnConfigurationChange(object sender, EventArgs e)
        {
            if (m_bSkipEvents)
            {
                return;
            }

            ElementColor = panelSourceColor.BackColor;

            var sSerializedGradient = GenerateGradientString();
            if (!string.IsNullOrEmpty(sSerializedGradient))
            {
                ElementGradient = sSerializedGradient;
                PreviewEvent?.Invoke(this, sSerializedGradient, panelSourceColor.BackColor);
            }
        }

        private string GenerateGradientString()
        {
            switch ((GradientStyle)tabGradients.SelectedTab.Tag)
            {
                case GradientStyle.lefttoright:
                    return GradientSerializer.SerializeToString(
                        new GradientSettingsAngle()
                        {
                            DestinationColor = panelDestinationColor.BackColor,
                            Angle = (float)numericLTRAngle.Value,
                        });
                case GradientStyle.points:
                    return GradientSerializer.SerializeToString(
                        new GradientSettingsPoints()
                        {
                            DestinationColor = panelDestinationColor.BackColor,
                            Source = new PointF((float)numericPSourceX.Value, (float)numericPSourceY.Value),
                            Destination = new PointF((float)numericPDestinationX.Value, (float)numericPDestinationY.Value),
                        }, GradientStyle.points);
                case GradientStyle.pointsnormalized:
                    return GradientSerializer.SerializeToString(
                        new GradientSettingsPoints()
                        {
                            DestinationColor = panelDestinationColor.BackColor,
                            Source = new PointF((float)numericPNSourceX.Value, (float)numericPNSourceY.Value),
                            Destination = new PointF((float)numericPNDestinationX.Value, (float)numericPNDestinationY.Value),
                        }, GradientStyle.pointsnormalized);
            }

            return string.Empty;
        }

        private void ShowColorSelectionDialog(Panel zPanel)
        {
            var zRGBDialog = new RGBColorSelectDialog();
            zRGBDialog.PreviewEvent += (obj, colorPreview) =>
            {
                zPanel.BackColor = colorPreview;
                OnConfigurationChange(zPanel, EventArgs.Empty);
            };
            zRGBDialog.UpdateColorBox(zPanel.BackColor);
            if (DialogResult.OK == zRGBDialog.ShowDialog(this))
            {
                zPanel.BackColor = zRGBDialog.Color;
            }
        }
    }
}
