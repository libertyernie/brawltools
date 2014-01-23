using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using BrawlLib;

namespace System.Windows.Forms
{
    public partial class ExportAllFormatDialog : Form
    {
        public ExportAllFormatDialog()
        {
            InitializeComponent();
            string[] source = FileFilters.TEX0.Split('|');
            for (int i = 0; i < source.Length; i += 2)
                if (!source[i].StartsWith("All"))
                    comboBox1.Items.Add(new FormatForExportAllDialog(source[i], source[i + 1]));
            comboBox1.SelectedIndex = 0;
        }

        public string SelectedExtension
        {
            get
            {
                return ((FormatForExportAllDialog)comboBox1.SelectedItem).extension.Replace("*", "");
            }
        }
    }

    public class FormatForExportAllDialog
    {
        public string description { get; set; }
        public string extension { get; set; }

        public FormatForExportAllDialog(string description, string extension)
        {
            this.description = description;
            int locationOfSemicolon = extension.IndexOf(';');
            if (locationOfSemicolon < 0)
                this.extension = extension;
            else
                this.extension = extension.Substring(0, locationOfSemicolon);
        }

        public override string ToString() { return description; }
    }
}
