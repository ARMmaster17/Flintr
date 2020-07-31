using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Flintr_Administration
{
    public partial class LoginForm : Form
    {
        public IPAddress address;
        public int port;
        
        public LoginForm()
        {
            InitializeComponent();
        }

        private void ipTextBox_Validating(object sender, CancelEventArgs e)
        {
            validateAll();
        }

        private void validateAll()
        {
            try
            {
                IPAddress.Parse(ipTextBox.Text);
                errorProvider.SetError(ipTextBox, "");
            }
            catch (FormatException)
            {
                errorProvider.SetError(ipTextBox, "Enter a valid IPv4 IP address.");
            }

            try
            {
                int temp = Convert.ToInt32(portTextBox.Text);
                if (temp < 1 || temp > 65535) throw new FormatException();
                errorProvider.SetError(portTextBox, "");
            }
            catch (FormatException)
            {
                errorProvider.SetError(portTextBox, "Enter a valid port number (default 3999).");
            }
        }

        private void portTextBox_Validating(object sender, CancelEventArgs e)
        {
            validateAll();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (errorProvider.GetError(ipTextBox) != "" && errorProvider.GetError(portTextBox) != "") return;
            address = IPAddress.Parse(ipTextBox.Text);
            port = Convert.ToInt32(portTextBox.Text);
            this.Close();
        }
    }
}
