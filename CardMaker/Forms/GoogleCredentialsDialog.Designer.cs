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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CardMaker.Forms
{
    partial class GoogleCredentialsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.txtAccessToken = new TextBox();
            this.btnOK = new Button();
            this.btnPaste = new Button();
            this.label2 = new Label();
            this.txtAuthURL = new TextBox();
            this.btnBrowse = new Button();
            this.label3 = new Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new Size(90, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Access Token:";
            this.label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtAccessToken
            // 
            this.txtAccessToken.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.txtAccessToken.Location = new Point(111, 48);
            this.txtAccessToken.Name = "txtAccessToken";
            this.txtAccessToken.Size = new Size(393, 20);
            this.txtAccessToken.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnOK.Location = new Point(553, 86);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnPaste.Location = new Point(510, 47);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new Size(118, 23);
            this.btnPaste.TabIndex = 4;
            this.btnPaste.Text = "Paste Token";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new EventHandler(this.btnPaste_Click);
            // 
            // label2
            // 
            this.label2.Location = new Point(12, 12);
            this.label2.Name = "label2";
            this.label2.Size = new Size(90, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Auth URL:";
            this.label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtAuthURL
            // 
            this.txtAuthURL.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.txtAuthURL.Location = new Point(111, 12);
            this.txtAuthURL.Name = "txtAuthURL";
            this.txtAuthURL.ReadOnly = true;
            this.txtAuthURL.Size = new Size(393, 20);
            this.txtAuthURL.TabIndex = 6;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnBrowse.Location = new Point(510, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(118, 23);
            this.btnBrowse.TabIndex = 8;
            this.btnBrowse.Text = "Browse To URL";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.label3.Location = new Point(12, 86);
            this.label3.Name = "label3";
            this.label3.Size = new Size(535, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Note: CardMaker only stores the access token in the current running instance.";
            this.label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // GoogleCredentialsDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(640, 115);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtAuthURL);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnPaste);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtAccessToken);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MaximumSize = new Size(648, 142);
            this.MinimizeBox = false;
            this.MinimumSize = new Size(648, 142);
            this.Name = "GoogleCredentialsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.Text = "Google Credentials";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox txtAccessToken;
        private Button btnOK;
        private Button btnPaste;
        private Label label2;
        private TextBox txtAuthURL;
        private Button btnBrowse;
        private Label label3;
    }
}