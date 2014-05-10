using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO;

namespace System.Windows.Forms {
	public class AttributeInterpretation {
		public AttributeInfo[] Array { get; private set; }
		public string Filename { get; private set; }
		public int NumEntries {
			get {
				return Array.Length;
			}
		}

		public AttributeInterpretation(AttributeInfo[] array, string saveToFile) {
			this.Array = array;
			this.Filename = saveToFile;
		}

		public AttributeInterpretation(string filename) {
			this.Filename = filename;

			List<AttributeInfo> list = new List<AttributeInfo>();
			int index = 0x14;
			if (filename != null && File.Exists(filename)) {
				using (var sr = new StreamReader(filename)) {
					for (int i = 0; !sr.EndOfStream /*&& i < NumEntries*/; i++) {
						AttributeInfo attr = new AttributeInfo();
						attr._name = sr.ReadLine();
						attr._description = sr.ReadLine();
						string num = sr.ReadLine();
						try {
							attr._type = int.Parse(num);
						} catch (FormatException ex) {
							throw new FormatException("Invalid type \"" + num + "\" in " + Path.GetFileName(filename) + ".", ex);
						}

						if (attr._description == "") attr._description = "No Description Available.";

						list.Add(attr);
						sr.ReadLine();
						index++;
					}
				}
			}
			this.Array = list.ToArray();
		}

		public override string ToString() {
			return Path.GetFileNameWithoutExtension(this.Filename);
		}

		public void Save() {
			string dir = Path.GetDirectoryName(Filename);
			if (!Directory.Exists(dir)) {
				MessageBox.Show("The directory " + dir + " does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (File.Exists(Filename)) {
				if (DialogResult.Yes != MessageBox.Show("Overwrite " + Filename + "?", "Overwrite",
					MessageBoxButtons.YesNo)) return;
			}
			using (var sw = new StreamWriter(Filename)) {
				foreach (AttributeInfo attr in Array) {
					sw.WriteLine(attr._name);
					sw.WriteLine(attr._description);
					sw.WriteLine(attr._type);
					sw.WriteLine();
				}
			}
		}
	}

	public class MultipleInterpretationAttributeGrid : AttributeGrid {
		private ComboBox chooser;

		public MultipleInterpretationAttributeGrid() : base() {
			chooser = new ComboBox() {
				Dock = DockStyle.Fill,
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			chooser.SelectedIndexChanged += chooser_SelectedIndexChanged;

			Button save = new Button() {
				Dock = DockStyle.Right,
				Text = "Save"
			};
			save.Click += save_Click;

			Panel p = new Panel() {
				Dock = DockStyle.Top,
				Size = chooser.PreferredSize
			};
			p.Controls.Add(chooser);
			p.Controls.Add(save);
			this.Controls.Add(p);

			foreach (Control c in new Control[] { chooser, save, p }) {
				c.Margin = new Padding(0);
			}
		}

		void save_Click(object sender, EventArgs e) {
			AttributeInterpretation item = (AttributeInterpretation)chooser.SelectedItem;
			if (item != null) {
				item.Save();
			}
		}

		private void chooser_SelectedIndexChanged(object sender, EventArgs e) {
			AttributeInterpretation item = (AttributeInterpretation)chooser.SelectedItem;
			if (item != null) {
				base.AttributeArray = item.Array;
				base.TargetChanged();
			}
		}

		public int Add(AttributeInterpretation arr) {
			int i = chooser.Items.Add(arr);
			if (base.AttributeArray == null) {
				chooser.SelectedIndex = i;
			}
			return i;
		}
		public void AddRange(IEnumerable<AttributeInterpretation> arrs) {
			foreach (var arr in arrs) Add(arr);
		}
		public void Remove(AttributeInterpretation arr) {
			chooser.Items.Remove(arr);
			if (base.AttributeArray == null) {
				base.AttributeArray = null;
			}
		}
		public void Clear() {
			chooser.Items.Clear();
			base.AttributeArray = null;
		}
	}
}
