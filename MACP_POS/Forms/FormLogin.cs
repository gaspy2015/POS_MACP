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
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            FormMain3 main = new FormMain3();
            main.Show();

            FormSecondScreen formSecondScreen = new FormSecondScreen();

            //check if there is more than 1 screen
            if(Screen.AllScreens.Length > 1)
            {
                //get the second screen
                Screen secondScreen = Screen.AllScreens[1];

                //move the form to the second screen
                formSecondScreen.StartPosition = FormStartPosition.Manual;
                formSecondScreen.Location = secondScreen.WorkingArea.Location;
            }
            formSecondScreen.Show();
        }
    }
}
