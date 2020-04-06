using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace SaveSender
{
    public partial class Client : Form
    {
        private bool send = false;
        private bool wait = false;
        //буфер за входящи данни
        private byte[] bytes = new byte[1024 * 1024 * 10];
        public Client()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            tbIP.Text = Properties.Settings.Default.oldIP;

            this.FormClosed += new FormClosedEventHandler(
                delegate (object o, FormClosedEventArgs a)
                {
                    try
                    {
                        Communicate(tbIP.Text, 8888, "<TheEnd>");
                    }
                    catch
                    {
                    }
                    System.Environment.Exit(1);
                });
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (btnSend.Text == "Connect")
            {
                try
                {
                    Communicate(tbIP.Text, 8888);
                    btnSend.Text = "Send";

                    lbLog.Items.Add(DateTime.Now.ToString() +
                               " - connection to: " + tbIP.Text);

                    Properties.Settings.Default.oldIP = tbIP.Text;
                    Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    send = true;
                    wait = true;
                    CheckForResponde();
                }
                catch (Exception)
                {
                    send = false;
                    wait = false;
                    lbLog.Items.Add(DateTime.Now.ToString() +
                               " - connection is terminated!");
                    throw;
                }
            }
        }
        private void CheckForResponde()
        {
            while(wait)
            {
                Communicate(tbIP.Text, 8888);
                Thread.Sleep(1000);
                Application.DoEvents();
            }
        }
        private void Communicate(string hostname, int port, string message = "")
        {
            //свързваме се с отдалечен сървър

            //определяме отдалечена точка (сървър) за сокета
            IPHostEntry ipHost = Dns.GetHostByName(hostname);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sock = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //свързваме се със сървъра
            sock.Connect(ipEndPoint);

            byte[] data = null;

            if (message != "")
            {
                data = Encoding.UTF8.GetBytes(message);
            }
            else if (send)
            {
                send = false;
                //кодираме отговора на сървара
                data = File.ReadAllBytes(Environment.CurrentDirectory + "\\Quicksave.sav");
                //изпращане на отговора
                lbLog.Items.Add(DateTime.Now.ToString() +
                    " - file sent");
            }
            else
            {
                data = Encoding.UTF8.GetBytes("<Ping>");
            }

            int byteSend = sock.Send(data);

            if (message == "<TheEnd>")
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
                System.Environment.Exit(1);
            }

            int byteRec = sock.Receive(bytes);
            
            string str = Encoding.UTF8.GetString(bytes, 0, byteRec);
            if (str.IndexOf("<Ping>") == -1)
            {
                wait = false;
                byte[] wBytes = new byte[byteRec];
                Array.Copy(bytes, wBytes, byteRec);
                
                File.WriteAllBytes(Environment.CurrentDirectory + "\\Quicksave.sav", wBytes);
                lbLog.Items.Add(DateTime.Now.ToString() +
                    " - file recieved");
            }

            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }
    }
}
