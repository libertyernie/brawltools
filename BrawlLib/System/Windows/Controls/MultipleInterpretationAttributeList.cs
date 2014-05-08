using System;
using System.Collections.Generic;

namespace System.Windows.Forms {
	public class MultipleInterpretationAttributeGrid : AttributeGrid {
		private ComboBox chooser;

		public MultipleInterpretationAttributeGrid()
			: base() {
			chooser = new ComboBox() {
				Dock = DockStyle.Top,
			};
			this.Controls.Add(chooser);
			chooser.SelectedIndexChanged += chooser_SelectedIndexChanged;
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
