using System;
using System.Collections.Generic;

namespace System.Windows.Forms {
	public class MultipleInterpretationAttributeGrid : AttributeGrid {
		private ComboBox chooser;

		public MultipleInterpretationAttributeGrid() : base() {
			chooser = new ComboBox() { Dock = DockStyle.Fill };
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
			throw new NotImplementedException();
		}

		private void chooser_SelectedIndexChanged(object sender, EventArgs e) {
			AttributeInfo[] item = (AttributeInfo[])chooser.SelectedItem;
			if (item != null) {
				base.AttributeArray = item;
				base.TargetChanged();
			}
		}

		public int Add(AttributeInfo[] arr) {
			int i = chooser.Items.Add(arr);
			if (base.AttributeArray == null) {
				chooser.SelectedIndex = i;
			}
			return i;
		}
		public void AddRange(IEnumerable<AttributeInfo[]> arrs) {
			foreach (var arr in arrs) Add(arr);
		}
		public void Remove(AttributeInfo[] arr) {
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
