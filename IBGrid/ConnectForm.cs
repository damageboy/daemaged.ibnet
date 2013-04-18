using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IBGrid
{
    public partial class ConnectForm : Form
    {

        public System.Windows.Forms.TextBox HostTextBox
        {
            get { return hostTextBox; }
            set { hostTextBox = value; }
        }
        public System.Windows.Forms.TextBox PortTextBox
        {
            get { return portTextBox; }
            set { portTextBox = value; }
        }

        public ConnectForm()
        {
            InitializeComponent();
        }
    }
}
