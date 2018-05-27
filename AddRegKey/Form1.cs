using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChangeEnvironmentVariables.Properties;
using Microsoft.Win32;

namespace AddRegKey
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string fileVarNames = "savedVariableNames.txt";
        private string fileVarPath = "savedVariablePath.txt";

        private string savedVarName
        {
            get
            {
                var _file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + fileVarNames;
                return _file;
            }
        }

        private string savedVarPath
        {
            get
            {
                var file_ = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + fileVarPath;
                return file_;
            }
        }

        private void SaveVarNameToFile()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");

            using (var writer = File.AppendText(savedVarName))
            {
                writer.Write(cbVarName.Text);
            }
            RemoveDuplicatesFromText(savedVarName);
            RemoveEmptyLinesFromText(savedVarName);
        }

        private void SaveVarPathToFile()
        {
            using (var writer = File.AppendText(savedVarPath))
            {
                writer.Write(cbVarPath.Text);
            }
            RemoveDuplicatesFromText(savedVarPath);
            RemoveEmptyLinesFromText(savedVarPath);
        }

        private void RemoveEmptyLinesFromText(string fileName)
        {
            var lines = File.ReadAllLines(fileName).Where(arg => !string.IsNullOrWhiteSpace(arg));
            File.WriteAllLines(fileName, lines);
        }

        private void RemoveDuplicatesFromText(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            File.WriteAllLines(fileName, lines.Distinct().ToArray());
        }

        private void LoadStrTextFromFile(string fileName, ComboBox cb)
        {
            if(File.Exists(fileName))
                foreach (string line in File.ReadLines(fileName))
                    cb.Items.Add(line);
        }

        private void btnAddRegKey_Click(object sender, EventArgs e)
        {
            try
            {
                SaveVarNameToFile();
                SaveVarPathToFile();

                cbVarPath.Items.Clear();

                LoadStrTextFromFile(savedVarName, cbVarName);
                LoadStrTextFromFile(savedVarPath, cbVarPath);

                AddUserVariable();
                AddSystemVariable();

                string btnText = btnAddRegKey.Text;

                if (btnAddRegKey.Text == "Add")
                {
                    MessageBox.Show("Successfully added BOOST_ROOT User and System Environment Variables.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Successfully replaced BOOST_ROOT User and System Environment Variables.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                txtStatus.Clear();
                // txtStatus.AppendText("Successfully added or replaced BOOST_ROOT User and System Environment Variables." + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddUserVariable()
        {
            RegistryKey 
 
            userVar = Registry.CurrentUser.CreateSubKey("Environment");

            userVar.SetValue(cbVarName.Text, cbVarPath.Text);
            userVar.Close();
        }

        private void AddSystemVariable()
        {
            RegistryKey 

            //systemVar = Registry.LocalMachine.CreateSubKey(@"SYSTEM\ControlSet001\Control\Session Manager\Environment");
            systemVar = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");

            systemVar.SetValue(cbVarName.Text, cbVarPath.Text);
            systemVar.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveVarNameToFile();
            SaveVarPathToFile();

            cbVarPath.Items.Clear();

            LoadStrTextFromFile(savedVarName, cbVarName);
            LoadStrTextFromFile(savedVarPath, cbVarPath);

            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                cbVarPath.Text = folderBrowserDialog1.SelectedPath;
        }

        private string CheckRegSystemStr()
        {
            string noSuch = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment",
            "BOOST_ROOT", string.Empty);
            return noSuch;
        }

        private string CheckRegUserStr()
        {
            string noSuch = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Environment",
            "BOOST_ROOT", string.Empty);
            return noSuch;
        }

        // Check Button
        private void button1_Click(object sender, EventArgs e)
        {
            CheckRegSystemStr();
        }

        private void GetRegData()
        {
            // user
            RegistryKey regKeyUser = Registry.CurrentUser.OpenSubKey("Environment");
            if (regKeyUser != null)
            {
                if (regKeyUser.GetValue(cbVarName.Text) != null)
                {
                    // MessageBox.Show("USER Variable - " + cbVarName.Text + " does exist");
                    txtStatus.AppendText("User Variable " + cbVarName.Text + ", Path: \"" + CheckRegUserStr() + "\" exists on this computer." + Environment.NewLine);
                    btnAddRegKey.Text = "Replace";
                }

                else
                {
                    //   MessageBox.Show("USER Variable - " + cbVarName.Text + " doesn't exist");
                    txtStatus.AppendText("User Variable " + cbVarName.Text + " does NOT exist on this computer." + Environment.NewLine);
                    btnAddRegKey.Text = "Add";
                }
            }

            //system
            RegistryKey regKeySystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            if (regKeySystem != null)
            {
                if (regKeySystem.GetValue(cbVarName.Text) != null)
                {
                    // MessageBox.Show("System Variable - " + cbVarName.Text + " does exist");
                    txtStatus.AppendText("System Variable " + cbVarName.Text + ", Path: \"" + CheckRegSystemStr() + "\" exists on this computer." + Environment.NewLine);

                }

                else
                {
                    //MessageBox.Show("System Variable - " + cbVarName.Text + " doesn't exist");
                    txtStatus.AppendText("System Variable " + cbVarName.Text + " does NOT exist on this computer." + Environment.NewLine);

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStrTextFromFile(savedVarName, cbVarName);
            LoadStrTextFromFile(savedVarPath, cbVarPath);

            GetRegData();

            if (Settings.Default.FormSize != null)
            {
                this.Size = Settings.Default.FormSize;
            }
            CenterToScreen();
        }

        private void ChangeBackColor_Button(Button button, Color color)
        {
            button.BackColor = color;
        }

        private void btnAddRegKey_MouseEnter(object sender, EventArgs e)
        {
           btnAddRegKey.BackColor = Color.DodgerBlue;
            btnAddRegKey.ForeColor = Color.White;
        }

        private void btnAddRegKey_MouseLeave(object sender, EventArgs e)
        {
            btnAddRegKey.BackColor = Color.SteelBlue;
            btnAddRegKey.ForeColor = Color.White;
        }

        private void btnBrowse_MouseEnter(object sender, EventArgs e)
        {
            btnBrowse.BackColor = Color.DodgerBlue;
            btnBrowse.ForeColor = Color.White;
        }

        private void btnBrowse_MouseLeave(object sender, EventArgs e)
        {
            btnBrowse.BackColor = Color.SteelBlue;
            btnBrowse.ForeColor = Color.White;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveVarNameToFile();
            SaveVarPathToFile();

            // Copy window size to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.FormSize = this.Size;
            }
            else
            {
                Settings.Default.FormSize = this.RestoreBounds.Size;
            }

            // Save settings
            Settings.Default.Save();
        }
    }
}
