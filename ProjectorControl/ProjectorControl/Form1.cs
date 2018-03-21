#define USE_RJ45
//#define USE_RS232
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace ProjectorControl
{
    public partial class Form1 : Form
    {
#if USE_RS232
        SerialPort sp;
#elif USE_RJ45
        Socket _socketCliect;
        string[]      ipArray;
        Socket[]    socketArray;
        Thread[]   threadArray;
        int[]           statusArray;
        bool[]        stopflagArray;
        Queue<string> consoleTextQueue = new Queue<string>();
#endif
        

        public Form1()
        {
            InitializeComponent();
#if USE_RS232
            string[] ports = SerialPort.GetPortNames();
            portsComboBox.Items.Clear();
            foreach(string p in ports)
            {
                portsComboBox.Items.Add(p);
                Console.WriteLine("Get: " + p);
            }

            // LCT100 a03
            string com = "COM3";
            int baudrate = 19200;
            var parity = Parity.None;
            int databits = 8;
            var stopbits = StopBits.One;

            sp = new SerialPort(com, baudrate, parity, databits, stopbits)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
            sp.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
#elif USE_RJ45
            

#endif
        }
      
#if USE_RS232
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //讀入字串
            try
            {
                string data = sp.ReadExisting();
                Console.WriteLine("Receive: " + data);
                if (data.Contains("0"))
                {
                    statusStr = "Status: Off";
                }
                if (data.Contains("1"))
                {
                    statusStr = "Status: On";
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
#endif
        private void turnOnButton_Click(object sender, EventArgs e)
        {
#if USE_RS232
            try
            {
                if (!sp.IsOpen)
                    sp.Open();
                if (sp.IsOpen)
                {
                    Console.WriteLine("SerialPort is Open");
                    //string cmdOn = "0x23, 0x30, 0x30, 0x30, 0x30, 0x20, 0x31, 0x0D";//z28
                    string cmdOn = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x31, 0x0D";//optoma
                    var cmd = CommandBytes(cmdOn);
                   foreach(Byte b in cmd)
                        Console.Write("[{0}]", b);
                    Console.WriteLine("---");
                    ASCIIEncoding asen = new ASCIIEncoding();
                    var cmd2 = asen.GetBytes("#0000 1\r");
                    foreach (Byte b in cmd2)
                        Console.Write("[{0}]", b);
                    Console.WriteLine("===");
                    sp.Write(cmd2, 0, cmd2.Count());
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message);
                sp.Close();
            }
#elif USE_RJ45

            int ipNum = ipArray.Length;
            for (int i = 0; i < ipNum; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(turnOnThread));
                t.Start(i);
            }

            //try
            //{
            //    IPAddress ipAddress = IPAddress.Parse(ipInput.Text);

            //    //IPAddress ipAddress = IPAddress.Parse("169.254.107.113");
            //    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 4352);

            //    //Create a TCP/IP  socket.
            //    if (this._socketCliect == null)
            //    {
            //        this._socketCliect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    }

            //    EndPoint endPoint = (EndPoint)ipEndPoint;
            //    if (!_socketCliect.Connected)
            //    {
            //        this._socketCliect.Connect(endPoint);
            //    }
            //    if (this._socketCliect.Connected)
            //    {
            //        Console.WriteLine("Socket is Connected");
            //        errorLabel.Text = "Socket is Connected";
            //        //string cmdOn = "0x23, 0x30, 0x30, 0x30, 0x30, 0x20, 0x31, 0x0D";//z28
            //        string cmdOn = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x31, 0x0D";//optoma
            //        var cmd = CommandBytes(cmdOn);

            //        this._socketCliect.Send(cmd, cmd.Length, 0);
            //        Console.WriteLine("Socket send command");
            //        errorLabel.Text = "Socket send command";

            //        byte[] bytes = new byte[256];
            //        this._socketCliect.Receive(bytes);
            //        Console.WriteLine(Encoding.UTF8.GetString(bytes));
            //        errorLabel.Text = Encoding.UTF8.GetString(bytes);
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    errorLabel.Text = ex.Message;
            //    //turnOnButton_Click(sender, e);
            //}
#endif
        }

        private void turnOffButton_Click(object sender, EventArgs e)
        {
#if USE_RS232
            try
            {
                if (!sp.IsOpen)
                    sp.Open();
                if (sp.IsOpen)
                {
                    Console.WriteLine("SerialPort is Open");
                    //string cmdOff = "0x23, 0x30, 0x30, 0x30, 0x30, 0x20, 0x30, 0x0D";
                    string cmdOff = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x30, 0x0D";//optoma
                    var cmd = CommandBytes(cmdOff);
                    sp.Write(cmd, 0, cmd.Count());
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                sp.Close();
            }
#elif USE_RJ45
            int ipNum = ipArray.Length;
            for (int i = 0; i < ipNum; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(turnOffThread));
                t.Start(i);
            }
            //try
            //{
            //    IPAddress ipAddress = IPAddress.Parse(ipInput.Text);
            //    //IPAddress ipAddress = IPAddress.Parse("169.254.177.20");
            //    //IPAddress ipAddress = IPAddress.Parse("169.254.107.113");
            //    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 4352);

            //    //Create a TCP/IP  socket.
            //    if (this._socketCliect == null)
            //    {
            //        this._socketCliect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    }

            //    EndPoint endPoint = (EndPoint)ipEndPoint;
            //    if (!_socketCliect.Connected)
            //    {
            //        this._socketCliect.Connect(endPoint);
            //    }
            //    if (this._socketCliect.Connected)
            //    {
            //        Console.WriteLine("Socket is Connected");
            //        errorLabel.Text = "Socket is Connected";
            //        string cmdOff = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x30, 0x0D";//optoma
            //        var cmd = CommandBytes(cmdOff);

            //        this._socketCliect.Send(cmd, cmd.Length, 0);
            //        Console.WriteLine("Socket send command");
            //        errorLabel.Text = "Socket send command";

            //        byte[] bytes = new byte[256];
            //        this._socketCliect.Receive(bytes);
            //        Console.WriteLine(Encoding.UTF8.GetString(bytes));
            //        errorLabel.Text = Encoding.UTF8.GetString(bytes);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    errorLabel.Text = ex.Message;
            //}
#endif

        }
        
       
        private void timer1_Tick(object sender, EventArgs e)
        {
            refreshStausTable();
           // statusLabel.Text = statusStr;
           
        }

        static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public byte[] CommandBytes(string cmdString)
        {
            var command = System.Text.RegularExpressions.Regex.Replace(cmdString, @"\s+", "");
            command = System.Text.RegularExpressions.Regex.Replace(command, @"0x|0X|,", "");
            return StringToByteArray(command);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadIpArray();
            int ipNum = ipArray.Length;
            socketArray = new Socket[ipNum];
            statusArray = new int[ipNum];
            stopflagArray = new bool[ipNum] ;
            ipNumInput.Text = ipNum + "";
    
            ipNumInput.KeyPress += new KeyPressEventHandler(ipNumInput_KeyPress);
            tabControl1.ImageList = imageList1;
            tabControl1.TabPages[0].ImageIndex = 0;
            tabControl1.TabPages[0].Text = "投影机状态";
            tabControl1.TabPages[1].ImageIndex =1;
            tabControl1.TabPages[1].Text = "连线设定";
            tabControl1.TabPages[2].ImageIndex = 2;
            tabControl1.TabPages[2].Text = "错误信息";
            refreshStausTable();

            threadArray = new Thread[ipNum];
            for(int i=0;i< ipNum;i++)
            {
                threadArray[i] = new Thread(new ParameterizedThreadStart(checkStatusThread));
                threadArray[i].Start(i);
            }
            
        }

        private void turnOnThread(object value)
        {
            int idx = Convert.ToInt32(value);
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ipArray[idx]);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 4352);

                //Create a TCP/IP  socket.
                Socket socket  = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                EndPoint endPoint = (EndPoint)ipEndPoint;
                if (!socket.Connected)
                {
                    socket.Connect(endPoint);
                }
                if (socket.Connected)
                {
                    Console.WriteLine("Socket is Connected");
                    errorLabel.Text = "Socket is Connected";
                    string cmdStatus = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x31, 0x0D";//optoma
                    var cmd = CommandBytes(cmdStatus);

                    socket.Send(cmd, cmd.Length, 0);
                    Console.WriteLine("Socket send command");
                    errorLabel.Text = "Socket send command";
                    
                }
            }
            catch (Exception ex)
            {
                if (idx < statusArray.Length)
                {
                    statusArray[idx] = -1;
                }
                Console.WriteLine("no." + (idx + 1) + ":  " + ex.Message);
                consoleTextQueue.Enqueue("投影机 no." + (idx + 1) + ":  " + ex.Message + "\n");
            }
        }

        private void turnOffThread(object value)
        {
            int idx = Convert.ToInt32(value);
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ipArray[idx]);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 4352);

                //Create a TCP/IP  socket.
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                EndPoint endPoint = (EndPoint)ipEndPoint;
                if (!socket.Connected)
                {
                    socket.Connect(endPoint);
                }
                if (socket.Connected)
                {
                    Console.WriteLine("Socket is Connected");
                    errorLabel.Text = "Socket is Connected";
                    string cmdStatus = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x30, 0x0D";//optoma
                    var cmd = CommandBytes(cmdStatus);

                    socket.Send(cmd, cmd.Length, 0);
                    Console.WriteLine("Socket send command");
                    errorLabel.Text = "Socket send command";

                }
            }
            catch (Exception ex)
            {
                if (idx < statusArray.Length)
                {
                    statusArray[idx] = -1;
                }
                Console.WriteLine("no." + (idx + 1) + ":  " + ex.Message);
                consoleTextQueue.Enqueue("投影机 no." + (idx + 1) + ":  " + ex.Message + "\n");
            }
        }

        private void checkStatusThread(object value)
        {
            int idx = Convert.ToInt32(value);
            while(!stopflagArray[idx])
            {
                try
                {
                    IPAddress ipAddress = IPAddress.Parse(ipArray[idx]);
                    
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 4352);

                    //Create a TCP/IP  socket.
                    if (this.socketArray[idx] == null)
                    {
                        this.socketArray[idx] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }

                    EndPoint endPoint = (EndPoint)ipEndPoint;
                    if (!socketArray[idx].Connected)
                    {
                        this.socketArray[idx].Connect(endPoint);
                    }
                    if (this.socketArray[idx].Connected)
                    {
                        Console.WriteLine("Socket is Connected");
                        errorLabel.Text = "Socket is Connected";
                        string cmdStatus = "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x3F, 0x0D";//optoma
                        var cmd = CommandBytes(cmdStatus);

                        this.socketArray[idx].Send(cmd, cmd.Length, 0);
                        Console.WriteLine("Socket send command");
                        errorLabel.Text = "Socket send command";

                        byte[] bytes = new byte[256];
                        this.socketArray[idx].Receive(bytes);
                        Console.WriteLine(Encoding.UTF8.GetString(bytes));
                        errorLabel.Text = Encoding.UTF8.GetString(bytes);
                        if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=0"))
                        {
                           // statusStr = "Status: Power-Off";
                            statusArray[idx] = 0;
                        }
                        else if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=1"))
                        {
                            //statusStr = "Status: Power-On";
                            statusArray[idx] = 1;
                        }
                        else if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=2"))
                        {
                           // statusStr = "Status: Cooling";
                        }
                        else if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=3"))
                        {
                          //  statusStr = "Status: Warm-up";
                        }
                        Thread.Sleep(10000);
                    }
                }
                catch (Exception ex)
                {
                    if (idx < statusArray.Length)
                    {
                        statusArray[idx] = -1;
                    }
                    Console.WriteLine("no." + (idx+1) +":  "+  ex.Message);
                    consoleTextQueue.Enqueue("投影机 no." + (idx + 1) + ":  " + ex.Message + "\n");
                    if (ex.GetType() == typeof(FormatException))
                    {
                        return;
                    }

                    Thread.Sleep(10000);
                }
            }
           
        }

        private void loadIpArray()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = ConfigurationManager.AppSettings;
            ipArray = config.AppSettings.Settings["IPAddrArray"].Value.Split(',');//ConfigurationManager.AppSettings["IPAddrArray"].Split(',');
        }

        private void ipNumInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar >= '0' && e.KeyChar <= '9' || e.KeyChar == (char)Keys.Back) 
            {
                e.Handled = false; //Do not reject the input
            }
            else
            {
                e.Handled = true; //Reject the input
            }
        }

        private void ipNumInput_TextChanged(object sender, EventArgs e)
        {
            tableLayoutPanel1.Controls.Clear();
            int ipNum = 0;
            bool res = int.TryParse(ipNumInput.Text, out ipNum);
            if (res && ipNum > 0)
            {
                if (ipNum > 30) { ipNum = 30; }
                tableLayoutPanel1.RowCount = ipNum;
                tableLayoutPanel1.Height = ((ipNum-1)/2 +1) * 29;
                for(int i = 0; i < ipNum; i++)
                {
                    Label idLabel = new Label();
                    idLabel.Text = "投影机 no." + (i + 1);
                    idLabel.AutoSize = false;
                    idLabel.Size = new Size(103, 28);
                    idLabel.Font = new Font("arial", 12);
                    tableLayoutPanel1.Controls.Add(idLabel, 2 * (i % 2), i / 2);

                    TextBox ipInputBx = new TextBox();
                    ipInputBx.Text = (i < ipArray.Count()) ? ipArray[i] : "";
                    ipInputBx.Font = new Font("arial", 10);
                    ipInputBx.Size = new Size(140, 23);
                    tableLayoutPanel1.Controls.Add(ipInputBx, 2 * (i % 2) + 1, i / 2);
                    
                }
            }
        }

        private void ipSaveButton_Click(object sender, EventArgs e)
        {
            int ipNum = tableLayoutPanel1.RowCount;
            string ipOutputStr = "";
            for (int i = 0; i < ipNum; i++)
            {
                TextBox ipInputBx = (TextBox) tableLayoutPanel1.GetControlFromPosition(2 * (i % 2) + 1, i / 2);
                ipOutputStr = ipOutputStr + ipInputBx.Text + ",";
                Console.WriteLine(ipInputBx.Text);
            }
            ipOutputStr = ipOutputStr.Substring(0, ipOutputStr.Length - 1);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("IPAddrArray");
            config.AppSettings.Settings.Add("IPAddrArray", ipOutputStr);
            config.Save();

            System.Diagnostics.Process.Start(Application.ExecutablePath);
            this.Close();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedIndex == 0)
            {
                refreshStausTable();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
            }
        }

        private void refreshStausTable()
        {
            tableLayoutPanel2.Controls.Clear();
            int ipNum = ipArray.Count();
            if (ipNum > 0)
            {
                tableLayoutPanel2.RowCount = ipNum;
                tableLayoutPanel2.Height = ((ipNum - 1) / 2 + 1) * 28;
                for (int i = 0; i < ipNum; i++)
                {
                    Label idLabel = new Label();
                    idLabel.Text = "投影机 no." + (i + 1);
                    idLabel.AutoSize = false;
                    idLabel.Size = new Size(103, 28);
                    idLabel.Font = new Font("arial", 12);
                    tableLayoutPanel2.Controls.Add(idLabel, 4 * (i % 2), i / 2);

                    PictureBox statusImg = new PictureBox();
                    statusImg.SizeMode = PictureBoxSizeMode.Zoom;
                    switch (statusArray[i])
                    {
                        case 0:
                            statusImg.Image = imageList2.Images[2];  //0: disconect 1: on 2: off
                            break;
                        case 1:
                            statusImg.Image = imageList2.Images[1];  //0: disconect 1: on 2: off
                            break;
                        default:
                            statusImg.Image = imageList2.Images[0];  //0: disconect 1: on 2: off
                            break;
                    }
                    
                    statusImg.Size = new Size(32, 16);
                    tableLayoutPanel2.Controls.Add(statusImg, 4 * (i % 2) + 1, i / 2);

                    Button onButton = new Button();
                    onButton.Text = "开";
                    onButton.Size = new Size(32, 18);
                    onButton.Tag = i;
                    onButton.Click += new EventHandler(onButton_Click);
                    tableLayoutPanel2.Controls.Add(onButton, 4 * (i % 2) + 2, i / 2);

                    Button offButton = new Button();
                    offButton.Text = "关";
                    offButton.Size = new Size(32, 18);
                    offButton.Tag = i;
                    offButton.Click += new EventHandler(offButton_Click);
                    tableLayoutPanel2.Controls.Add(offButton, 4 * (i % 2) + 3, i / 2);
                }
            }
        }

        private void onButton_Click(object sender, EventArgs e)
        {
            Button onButton = sender as Button;
            Console.WriteLine(onButton.Tag+" on");
            Thread t = new Thread(new ParameterizedThreadStart(turnOnThread));
            t.Start(onButton.Tag);
        }

        private void offButton_Click(object sender, EventArgs e)
        {
            Button offButton = sender as Button;
            Console.WriteLine(offButton.Tag + " off");
            Thread t = new Thread(new ParameterizedThreadStart(turnOffThread));
            t.Start(offButton.Tag);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("Form1_FormClosing");
            Environment.Exit(0);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            while (consoleTextQueue.Count > 0)
            {
                consoleOutput.AppendText(DateTime.Now.ToString("hh:mm:ss    ")+consoleTextQueue.Dequeue());
            }

            if(consoleOutput.Lines.Count() > 100)
            {
                int start_index = consoleOutput.GetFirstCharIndexFromLine(0);
                int count = consoleOutput.Lines[0].Length;

                // Eat new line chars
                if (0 < consoleOutput.Lines.Length - 1)
                {
                    count += consoleOutput.GetFirstCharIndexFromLine(0 + 1) -
                        ((start_index + count - 1) + 1);
                }

                consoleOutput.Text = consoleOutput.Text.Remove(start_index, count);
            }
           
        }
    }
    
}
