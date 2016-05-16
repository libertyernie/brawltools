namespace System.Windows.Forms
{
    public partial class TransformAttributesForm : Form
    {
        public TransformAttributesForm() { InitializeComponent(); }
        public TransformAttributesForm(Vector3 t, Vector3 r, Vector3 s)
        {
            InitializeComponent();
            _transformControl.ScaleVector = s;
            _transformControl.RotateVector = r;
            _transformControl.TranslateVector = t;
        }
        public bool TwoDimensional
        {
			get { return _transformControl.TwoDimensional; }
			set { _transformControl.TwoDimensional = value; }
		}
		public float this[int index]
        {
			get { return _transformControl[index]; }
			set { _transformControl[index] = value; }
		}
		public Vector3 ScaleVector
        {
			get { return _transformControl.ScaleVector; }
			set { _transformControl.ScaleVector = value; }
		}
		public Vector3 RotateVector
        {
			get { return _transformControl.RotateVector; }
			set { _transformControl.RotateVector = value; }
		}
		public Vector3 TranslateVector
        {
			get { return _transformControl.TranslateVector; }
			set { _transformControl.TranslateVector = value; }
		}
		public Matrix GetMatrix() { return _transformControl.GetMatrix(); }
	}
}
