using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CardMaker.Forms.Dialogs
{
    public partial class ExcelSheetSelectionDialog : Form
    {
        public ExcelSheetSelectionDialog()
        {
            InitializeComponent();
        }

        public ExcelSheetSelectionDialog(List<string> sheets)
            : this()
        {
            cmbSheetName.DataSource = sheets;
        }

        public string GetSelectedSheet()
        {
            return cmbSheetName.SelectedItem.ToString();
        }
    }
}
