using System;
using System.Windows.Forms;

namespace VMATTBI_optLoop
{
    public partial class confirmUI : Form
    {
        public bool confirm = false;
        public confirmUI()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            confirm = true;
            this.Close();
        }
    }
}
