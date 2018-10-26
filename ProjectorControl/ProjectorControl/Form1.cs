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
using Microsoft.Win32;

namespace ProjectorControl
{
    public partial class Form1 : Form
    {
#if USE_RS232
        SerialPort sp;
#elif USE_RJ45
        Socket _socketCliect;
        string[]      ipArray;
        string[]      typeArray;
        Socket[]    socketArray;
        Thread[]   threadArray;
        int[]           statusArray;
        bool[]        stopflagArray;
        int[]           cmdArray;
        Queue<string> consoleTextQueue = new Queue<string>();
        bool waitmsgIsVisible = false;
#endif
        

        public Form1()
        {
            if (VerifyOptimotion())
                InitializeComponent();
            else
            {
                MessageBox.Show("请先激活Optimotion后方能使用本工具");
                Environment.Exit(0);
            }
                
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

        private bool VerifyOptimotion()
        {
            using (var userKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView
                .Registry32))
            {
                var optimotionPath = userKey.OpenSubKey(@"SOFTWARE\Uniigym\Optimotion");
                if (optimotionPath == null)
                    return false;
                var secretKey = optimotionPath.GetValue("secretKey"); 
                if (secretKey != null && (string)secretKey != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

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
            waitmsgIsVisible = true;
            int ipNum = ipArray.Length;
            for (int i = 0; i < ipNum; i++)
            {
                cmdArray[i] = 1;
                //Thread t = new Thread(new ParameterizedThreadStart(turnOnThread));
                //t.Start(i);
            }
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
            waitmsgIsVisible = true;
            int ipNum = ipArray.Length;
            for (int i = 0; i < ipNum; i++)
            {
                cmdArray[i] = 0;
                //Thread t = new Thread(new ParameterizedThreadStart(turnOffThread));
                //t.Start(i);
            }
#endif

        }
        
       
        private void timer1_Tick(object sender, EventArgs e)
        {
            refreshStausTable();
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
            cmdArray = new int[ipNum];
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
            createStausTable();

            threadArray = new Thread[ipNum];
            for(int i=0;i< ipNum;i++)
            {
                cmdArray[i] = -1;
                threadArray[i] = new Thread(new ParameterizedThreadStart(checkStatusThread));
                threadArray[i].Start(i);
            }
            timer1.Enabled = true;
            timer3.Enabled = true;
        }

        private void turnOnThread(object value)
        {
            int idx = Convert.ToInt32(value);
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ipArray[idx]);
                int port = CommandTable.getPort(typeArray[idx]);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
                Console.WriteLine("IP: "+ ipAddress+" port: " + port);
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
                    string cmdStatus = CommandTable.getPowerOnCommand(typeArray[idx]); //"0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x31, 0x0D";//optoma
                    var cmd = CommandBytes(cmdStatus);

                    socket.Send(cmd, cmd.Length, 0);
                    Console.WriteLine("Socket send command");
                    
                }
            }
            catch (Exception ex)
            {
                if (idx < statusArray.Length)
                {
                    statusArray[idx] = -1;
                }
                Console.WriteLine("no." + (idx + 1) + ":  " + ex.Message);
                consoleTextQueue.Enqueue("(开机) 投影机 no." + (idx + 1) + ":  " + ex.Message + "\n");
            }
        }

        private void turnOffThread(object value)
        {
            int idx = Convert.ToInt32(value);
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ipArray[idx]);
                int port = CommandTable.getPort(typeArray[idx]);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

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
                    string cmdStatus = CommandTable.getPowerOffCommand(typeArray[idx]); //"0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x30, 0x0D";//optoma
                    var cmd = CommandBytes(cmdStatus);

                    socket.Send(cmd, cmd.Length, 0);
                    Console.WriteLine("Socket send command");

                }
            }
            catch (Exception ex)
            {
                if (idx < statusArray.Length)
                {
                    statusArray[idx] = -1;
                }
                Console.WriteLine("no." + (idx + 1) + ":  " + ex.Message);
                consoleTextQueue.Enqueue("(关机) 投影机 no." + (idx + 1) + ":  " + ex.Message + "\n");
            }
        }

        private void checkStatusThread(object value)
        {
            int idx = Convert.ToInt32(value);
            while(!stopflagArray[idx])
            {
                try
                {
                    waitmsgIsVisible = false;
                    IPAddress ipAddress = IPAddress.Parse(ipArray[idx]);
                    int port = CommandTable.getPort(typeArray[idx]);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

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
                        string cmdStatus="";
                        if (cmdArray[idx] == -1)
                        {
                            cmdStatus = CommandTable.getPowerStateCommand(typeArray[idx]);//"0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x3F, 0x0D";//optoma

                        }
                        else if (cmdArray[idx] == 1)
                        {
                            cmdStatus = CommandTable.getPowerOnCommand(typeArray[idx]);
                            cmdArray[idx] = -1;
                        }
                        else if (cmdArray[idx] == 0)
                        {
                            cmdStatus = CommandTable.getPowerOffCommand(typeArray[idx]);
                            cmdArray[idx] = -1;
                        }
                        var cmd = CommandBytes(cmdStatus);

                        this.socketArray[idx].Send(cmd, cmd.Length, 0);
                        Console.WriteLine("Socket send command");

                        byte[] bytes = new byte[256];
                        this.socketArray[idx].Receive(bytes);
                        Console.WriteLine(Encoding.UTF8.GetString(bytes));
                        if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=0") || Encoding.UTF8.GetString(bytes).ToUpper().Contains("OK0") || Encoding.UTF8.GetString(bytes).Contains("Ok0"))
                        {
                           // statusStr = "Status: Power-Off";
                            statusArray[idx] = 0;
                        }
                        else if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=1") || Encoding.UTF8.GetString(bytes).ToUpper().Contains("OK1") || Encoding.UTF8.GetString(bytes).Contains("Ok1"))
                        {
                            //statusStr = "Status: Power-On";
                            statusArray[idx] = 1;
                        }
                        else if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=2"))
                        {
                            statusArray[idx] = 2;
                            // statusStr = "Status: Cooling";
                        }
                        else if (Encoding.UTF8.GetString(bytes).Contains("%1POWR=3"))
                        {
                            statusArray[idx] = 3;
                            //  statusStr = "Status: Warm-up";
                        }
                        Thread.Sleep(15000);
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
                    socketArray[idx] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            }
           
        }

        private void loadIpArray()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = ConfigurationManager.AppSettings;
            ipArray = config.AppSettings.Settings["IPAddrArray"].Value.Split(',');//ConfigurationManager.AppSettings["IPAddrArray"].Split(',');
            typeArray = config.AppSettings.Settings["TypeArray"].Value.Split(',');
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
                tableLayoutPanel1.Height = ipNum  * 29;
                for (int i = 0; i < ipNum; i++)
                {
                    Label idLabel = new Label();
                    idLabel.Text = "投影机 no." + (i + 1);
                    idLabel.AutoSize = false;
                    idLabel.Size = new Size(103, 28);
                    idLabel.Font = new Font("arial", 12);
                    tableLayoutPanel1.Controls.Add(idLabel, 0 , i );

                    TextBox ipInputBx = new TextBox();
                    ipInputBx.Text = (i < ipArray.Count()) ? ipArray[i] : "";
                    ipInputBx.Font = new Font("arial", 10);
                    ipInputBx.Size = new Size(140, 23);
                    tableLayoutPanel1.Controls.Add(ipInputBx,  1, i );

                    ComboBox comboBox = new ComboBox();
                    comboBox.Text = (i < ipArray.Count()) ? typeArray[i] : "";
                    comboBox.Items.Add("Z15WST");
                    comboBox.Items.Add("EH400+");
                    comboBox.Items.Add("ZX310ST");
                    comboBox.Items.Add("ZW310ST");
                    tableLayoutPanel1.Controls.Add(comboBox, 2, i);
                }
            }
        }

        private void ipSaveButton_Click(object sender, EventArgs e)
        {
            int ipNum = tableLayoutPanel1.RowCount;
            string ipOutputStr = "";
            for (int i = 0; i < ipNum; i++)
            {
                TextBox ipInputBx = (TextBox)tableLayoutPanel1.GetControlFromPosition(1, i);
                ipOutputStr = ipOutputStr + ipInputBx.Text + ",";
                Console.WriteLine(ipInputBx.Text);
            }
            ipOutputStr = ipOutputStr.Substring(0, ipOutputStr.Length - 1);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("IPAddrArray");
            config.AppSettings.Settings.Add("IPAddrArray", ipOutputStr);
            config.Save();
            //
            string typeOutputStr = "";
            for (int i = 0; i < ipNum; i++)
            {
                ComboBox comboBox = (ComboBox)tableLayoutPanel1.GetControlFromPosition(2, i);
                typeOutputStr = typeOutputStr + comboBox.Text + ",";
                Console.WriteLine(comboBox.Text);
            }
            typeOutputStr = typeOutputStr.Substring(0, typeOutputStr.Length - 1);
            config.AppSettings.Settings.Remove("TypeArray");
            config.AppSettings.Settings.Add("TypeArray", typeOutputStr);
            config.Save();
            //
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

        private void createStausTable()
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
                        case 0:     // Power-On
                            statusImg.Image = imageList2.Images[2];  
                            break;
                        case 1:     // Power-Off
                            statusImg.Image = imageList2.Images[1];  
                            break;
                        case 2:     // Cooling
                            statusImg.Image = imageList2.Images[3]; 
                            break;
                        case 3:     // Warm-up
                            statusImg.Image = imageList2.Images[4]; 
                            break;
                        default:     // disconect
                            statusImg.Image = imageList2.Images[0];  
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

        private void refreshStausTable()
        {
            int ipNum = ipArray.Count();
            if (ipNum > 0)
            {
                for (int i = 0; i < ipNum; i++)
                {
                    PictureBox statusImg = tableLayoutPanel2.GetControlFromPosition(4 * (i % 2) + 1, i / 2) as PictureBox;
                    switch (statusArray[i])
                    {
                        case 0:     // Power-On
                            statusImg.Image = imageList2.Images[2];
                            break;
                        case 1:     // Power-Off
                            statusImg.Image = imageList2.Images[1];
                            break;
                        case 2:     // Cooling
                            statusImg.Image = imageList2.Images[3];
                            break;
                        case 3:     // Warm-up
                            statusImg.Image = imageList2.Images[4];
                            break;
                        default:     // disconect
                            statusImg.Image = imageList2.Images[0];
                            break;
                    }
                }
            }
         }

        private void onButton_Click(object sender, EventArgs e)
        {
            waitmsgIsVisible = true;
            Button onButton = sender as Button;
            Console.WriteLine(onButton.Tag+" on");
            //Thread t = new Thread(new ParameterizedThreadStart(turnOnThread));
            //t.Start(onButton.Tag);
            cmdArray[(int)onButton.Tag] = 1;
        }

        private void offButton_Click(object sender, EventArgs e)
        {
            waitmsgIsVisible = true;
            Button offButton = sender as Button;
            Console.WriteLine(offButton.Tag + " off");
            //Thread t = new Thread(new ParameterizedThreadStart(turnOffThread));
            //t.Start(offButton.Tag);
            cmdArray[(int)offButton.Tag] = 0;
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

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (waitmsgIsVisible)
            {
                waitMessage.Visible = true;
            }
            else
            {
                waitMessage.Visible = false;
            }
        }
    }
    
}
