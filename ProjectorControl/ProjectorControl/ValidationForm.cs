//#define KEYGEN_MODE

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ProjectorControl
{
    public partial class ValidationForm : Form
    {
        public ValidationForm()
        {
            InitializeComponent();
        }

        private void ValidationForm_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "CiCS";
#if KEYGEN_MODE

            validButton.Text = "Generate";
            this.FindForm().Text = "KeyGen";

#else
            validButton.Text = "Verify";
            using (var userKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView
               .Registry32))
            {
                var path = userKey.OpenSubKey(@"SOFTWARE\CiCS\ProjectorControl");
                if (path == null) return;
                comboBox1.Text = (String)path.GetValue("Organization");
                validKey.Text = (String)path.GetValue("sn");
                verify();
            }
#endif

        }

        string getEncryptedCode()
        {
            using (var localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView
               .Registry32))
            {
                var cryptography = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                if (cryptography == null) return "errorerrorerrorerrorerrorerror";
                var guid = (string)cryptography.GetValue("MachineGuid");
              
                if (comboBox1.Text == "CiCS")
                {
                    return sha256(guid + "alpha");
                }
                else if (comboBox1.Text == "Coretronic")
                {
                    return sha256(guid + "beta");
                }
                else if (comboBox1.Text == "Optoma")
                {
                    return sha256(guid + "gamma");
                }
                else
                {
                    return sha256(guid + "delta");
                }
            }
        }

        static string sha256(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        private void validButton_Click(object sender, EventArgs e)
        {
#if KEYGEN_MODE
            validKey.Text = getEncryptedCode();
#else
            verify();
#endif
        }

        private void verify()
        {
            using (var localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView
               .Registry32))
            {
                var cryptography = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                if (cryptography == null) return;
                var guid = (string)cryptography.GetValue("MachineGuid");
                string ans = getEncryptedCode();
                if (validKey.Text == ans)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\CiCS\ProjectorControl", "Organization", comboBox1.Text);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\CiCS\ProjectorControl", "sn", validKey.Text);
                    Form1 form1 = new Form1();
                    this.Hide();
                    form1.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to verify product key.");
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "CiCS")
            {
                pictureBox1.Image = imageList1.Images[0];
            }
            else if (comboBox1.Text == "Coretronic")
            {
                pictureBox1.Image = imageList1.Images[1];
            }
            else if (comboBox1.Text == "Optoma")
            {
                pictureBox1.Image = imageList1.Images[2];
            } else
            {
                pictureBox1.Image = null;
            }
        }
    }
}
