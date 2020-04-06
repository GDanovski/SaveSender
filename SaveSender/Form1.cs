using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaveSender
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosed += new FormClosedEventHandler(
                delegate (object o, FormClosedEventArgs a) 
            {
                System.Environment.Exit(1);
            });
        }

        private void btnHost_Click(object sender, EventArgs e)
        {
            Server ser = new Server();
            this.Hide();
            ser.Show();
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            Client cl = new Client();
            this.Hide();
            cl.Show();
        }
    }
}
