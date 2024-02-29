using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boombox
{
    public partial class Form_Main_Menu : Form
    {
        public Form_Main_Menu()
        {
            InitializeComponent();
        }

        private void btn_Raw_Metadata_Click(object sender, EventArgs e)
        {
            Form_Raw_Metadata_Processing Raw_Metadata_Processing = new Form_Raw_Metadata_Processing();
            Raw_Metadata_Processing.Tag = this;
            Raw_Metadata_Processing.Show(this);
            this.Hide();
        }
    }
}
