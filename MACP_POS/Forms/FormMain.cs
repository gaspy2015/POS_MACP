using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MACP_POS.Forms
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void LoadFormIntoPanel(Form form)
        {
            if (form == null)
                return;

            form.TopLevel = false;
            form.Dock = DockStyle.Fill;

            pnlForms.Controls.Clear();
            pnlForms.Controls.Add(form);
            form.BringToFront();
            form.Show();
        }

        private void btnSignin_Click(object sender, EventArgs e)
        {
            FormLogin login = new FormLogin();

            LoadFormIntoPanel(login);
        }

        private void btnCreditCard_Click(object sender, EventArgs e)
        {
            FormCardDetails creditCard = new FormCardDetails();
            LoadFormIntoPanel(creditCard);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            FormChequeDetails cheque = new FormChequeDetails();
            LoadFormIntoPanel(cheque);
        }
    }
}
