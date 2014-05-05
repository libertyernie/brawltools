using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Windows.Forms {
	public unsafe partial class FourByteEditor : Form {
		public float[] DataBuffer;

		public FourByteEditor() {
			InitializeComponent();
			Shown += (o, e) => {
				foreach (float f in DataBuffer) Console.WriteLine(f.ToString() + " ");
			};
		}
	}

	public class FourByteTypeEditor : UITypeEditor {
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null) return UITypeEditorEditStyle.Modal;
			return base.GetEditStyle(context);
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if ((context == null) || (provider == null)) return base.EditValue(context, provider, value);
			
			var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

			if (editorService != null) {
				var modalEditor = new FourByteEditor();

				modalEditor.DataBuffer = ((List<float>)value).ToArray();

				if (editorService.ShowDialog(modalEditor) == System.Windows.Forms.DialogResult.OK) {
					return new List<float>(modalEditor.DataBuffer);
				}
			}
			return value;
		}
	}
}
