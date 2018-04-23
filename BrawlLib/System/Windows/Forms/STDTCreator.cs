namespace System.Windows.Forms
{
    public partial class STDTCreator : Form
    {
        public string title = "STDT Generation";
        public string lowerText = "Number of Entries:";

        public STDTCreator()
        {
            InitializeComponent();
        }

        public int NewValue { get { return (int)numNewCount.Value; } }

        public DialogResult ShowDialog()
        {
            this.Text = title;
            this.label2.Text = lowerText;
            return base.ShowDialog();
        }

        public DialogResult ShowDialog(string newTitle, string newLower)
        {
            this.Text = newTitle;
            this.label2.Text = newLower;
            return base.ShowDialog();
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            DialogResult = Forms.DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = Forms.DialogResult.Cancel;
            Close();
        }
    }
}
