using BrawlLib.SSBB.ResourceNodes;
using System.Drawing;
using System.Runtime.ExceptionServices;

namespace System.Windows.Forms {
	public partial class DataEditor4B : UserControl {
		private ResourceNode _openNode;

		public DataEditor4B() {
			InitializeComponent();

			dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
			dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
		}

		void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e) {
			foreach (DataGridViewRow row in dataGridView1.Rows) {
				row.HeaderCell.Value = "0x" + (0x14 + row.Index * 4).ToString("X");
			}
			dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
			dataGridView1.Columns[2].DefaultCellStyle.Font = new Font(FontFamily.GenericMonospace, 12);
		}

		void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
			//if (_openNode != null) _openNode.IsDirty = true;
		}

		public unsafe void SetSource(STDTNode src) {
			dataGridView1.DataSource = src.Entries;
			_openNode = src;
			_openNode.Disposing += _openNode_Disposing;
		}

		void _openNode_Disposing(ResourceNode node) {
			if (node == _openNode) {
				dataGridView1.DataSource = null;
			}
		}

		public void ClearAll() {
			dataGridView1.DataSource = null;
		}
	}
}
