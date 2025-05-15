using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MACP_POS
{
    public partial class FormMain1 : Form
    {
        public FormMain1()
        {
            InitializeComponent();
        }

        private void LoadFormIntoPanel(Form form)
        {
            if (form == null)
                return;

            form.TopLevel = false;
            form.Dock = DockStyle.Fill;

            PanelFormInput.Controls.Clear();
            PanelFormInput.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void btnSignin_Click(object sender, EventArgs e)
        {
            FormLogin login = new FormLogin();
            
                LoadFormIntoPanel(login);
            
        }
    }
}
