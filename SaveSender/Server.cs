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
    public partial class Server : Form
    {
        bool send = false;
        string ConnectedDevice = "";
        public Server()
        {
            InitializeComponent();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            this.FormClosed += new FormClosedEventHandler(
                delegate (object o, FormClosedEventArgs a)
                {
                    System.Environment.Exit(1);
                });

            lbLog.Items.Add(DateTime.Now.ToString() + 
                " - server started!");

            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            lbMyIP.Text = "My IP address is " + IPHost.AddressList[0].ToString();
            
            Thread thdListener = new Thread(new ThreadStart(StartServer));
            thdListener.Start();
        }

        private void StartServer()
        {
            //Подготвяме крайна точка за сокета
            //string externalip = new WebClient().DownloadString("http://icanhazip.com");
            IPHostEntry ipHost = Dns.GetHostByName(Dns.GetHostName());
            //IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];

            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 8888);
            //Създаваме потоков сокет, протокол
            //TCP/IP
            Socket sock = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //свързваме сокета с крайна точка
                sock.Bind(ipEndPoint);
                //започваме да слушаме
                sock.Listen(10);
                //започваме да слушаме за повикване
                while (true)
                {
                    //Програмата спира в очакване на входящо повикване

                    //Сокет за обмяна на данни с клиента
                    Socket s = sock.Accept();
                    if (s.Connected)
                    {
                        Control.CheckForIllegalCrossThreadCalls = false;

                        string ClientName = s.RemoteEndPoint.ToString().Split(new string[] { ":" },
                                StringSplitOptions.None)[0];
                        if (ConnectedDevice != ClientName)
                        {
                            lbLog.Items.Add(DateTime.Now.ToString() +
                                   " - " + ClientName + " - connected");
                            ConnectedDevice = ClientName;
                        }
                        //има клиент
                        //започваме да четем заявката от него
                        //масив с получени данни
                        byte[] bytes = new byte[1024*1024*10];
                        //дължина на получените данни
                        int bytesCount = s.Receive(bytes);

                        string data = Encoding.UTF8.GetString(bytes, 0, bytesCount);
                        if (data.IndexOf("<TheEnd>") > -1)
                        {
                            lbLog.Items.Add(DateTime.Now.ToString() +
                                " - connection is terminated!");

                            ConnectedDevice = "";

                            lbLog.Invalidate();
                            lbLog.Update();
                            lbLog.Refresh();
                            Application.DoEvents();
                            break;
                        }
                        else if (data.IndexOf("<Ping>") == -1)
                        {
                            byte[] wBytes = new byte[bytesCount];
                            Array.Copy(bytes, wBytes, bytesCount);

                            File.WriteAllBytes(Environment.CurrentDirectory + "\\Quicksave.sav", wBytes);
                            lbLog.Items.Add(DateTime.Now.ToString() +
                                " - file recieved");
                            
                            lbLog.Invalidate();
                            lbLog.Update();
                            lbLog.Refresh();
                            Application.DoEvents();
                        }

                        if (send)
                        {
                            send = false;
                            //кодираме отговора на сървара
                            byte[] msg = File.ReadAllBytes(Environment.CurrentDirectory + "\\Quicksave.sav");
                            //изпращане на отговора
                            s.Send(msg);
                            lbLog.Items.Add(DateTime.Now.ToString() +
                                " - file sent");
                            lbLog.Invalidate();
                            lbLog.Update();
                            lbLog.Refresh();
                            Application.DoEvents();
                        }
                        else
                        {
                            s.Send(Encoding.UTF8.GetBytes("<Ping>"));
                        }

                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
        private void btnSend_Click(object sender, EventArgs e)
        {
            send = true;
        }
    }
}
