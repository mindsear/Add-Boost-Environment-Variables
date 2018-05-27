using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChangeEnvironmentVariables;
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

        private string fileVarNames = "savedSystemVarNames.txt";

        private string savedVarName
        {
            get
            {
                var _file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + fileVarNames;
                return _file;
            }
        }

        private async void SaveVarNameToFile()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");

            if (File.Exists(savedVarName))
                return;

            using (var writer = File.AppendText(savedVarName))
            {
                foreach (string subKeyName in registryKey.GetValueNames())
                   await writer.WriteAsync(subKeyName + Environment.NewLine);
            }
            RemoveDuplicatesFromText(savedVarName);
            RemoveEmptyLinesFromText(savedVarName);
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
            foreach (string line in File.ReadLines(fileName))
                cb.Items.Add(line);
        }

        private void T()
        {
            if (cbVariableName.Text == "BOOST_ROOT")
            {
                AddUserVariable();
                AddSystemVariable();
            }
            else if (cbVariableName.Text == "Path" || cbVariableName.Text == "PATHEXT" || cbVariableName.Text == "PATH")
                AddSystemVariable();
            else
                AddSystemVariable();
        }

        private void btnAddRegKey_Click(object sender, EventArgs e)
        {
            try
            {
                //==== Backup USER AND SYSTEM Environment Variables

                if (!File.Exists("Backup-UserEnvironment.reg") || !File.Exists("Backup-SystemEnvironment.reg"))
                {
                    txtStatus.AppendText(Environment.NewLine + "> Backing up User and System Environment Registry Data... " + Environment.NewLine);

                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    string arg = @"/C REG EXPORT HKCU\Environment Backup-UserEnvironment.reg && REG EXPORT ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"" Backup-SystemEnvironment.reg";
                    startInfo.Arguments = arg;
                    process.StartInfo = startInfo;
                    process.Start();

                    process.WaitForExit();

                    // Inform the user if backing up completed successfully
                    txtStatus.AppendText("> Successfully backed up User and System Environment Registry Data." + Environment.NewLine);
                }

                //==== End of Backup

                SaveVarNameToFile();
                //SaveVarPathToFile();
                

                T();
                
                if (/*btnAddRegKey.Text == "Add"*/ cbVariableName.Text == "BOOST_ROOT")
                {
                    //MessageBox.Show("Successfully added BOOST_ROOT User and System Environment Variables.",
                    //Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtStatus.AppendText("> Successfully added/replaced " + cbVariableName.Text + " User and System Environment Variables." + Environment.NewLine);
                }
                else
                {
                    //MessageBox.Show("Successfully replaced BOOST_ROOT User and System Environment Variables.",
                    //Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtStatus.AppendText("> Successfully replaced " + cbVariableName.Text + " System Environment Variable." + Environment.NewLine);
                }
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
            
            userVar.SetValue(cbVariableName.Text, txtVarData.Text);
            userVar.Close();
        }

        private void AddSystemVariable()
        {
            RegistryKey 

            //systemVar = Registry.LocalMachine.CreateSubKey(@"SYSTEM\ControlSet001\Control\Session Manager\Environment"); // Win 7 ??
            systemVar = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");

            systemVar.SetValue(cbVariableName.Text, txtVarData.Text);
            systemVar.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (cbVariableName.Text == "Path" || cbVariableName.Text == "PATH")
            {
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    txtVarData.AppendText(folderBrowserDialog1.SelectedPath + ";");

                    SaveVarNameToFile();
                    //SaveVarPathToFile();
                }
            }

            else
            {
                FolderBrowserDialog folderBrowserDialog2 = new FolderBrowserDialog();
                if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
                {
                    txtVarData.Text = folderBrowserDialog2.SelectedPath;

                    SaveVarNameToFile();
                    //SaveVarPathToFile();
                }
            }
        }

        private string CheckRegSystemStr()
        {
            string noSuch = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment",
            cbVariableName.Text, string.Empty);
            return noSuch;
        }

        private string CheckRegUserStr()
        {
            string noSuch = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Environment",
            cbVariableName.Text, string.Empty);
            return noSuch;
        }

        private void GetRegData()
        {
            // user
            RegistryKey regKeyUser = Registry.CurrentUser.OpenSubKey("Environment");
            if (regKeyUser != null)
            {
                if (regKeyUser.GetValue("BOOST_ROOT") != null)
                {
                    cbVariableName.Text = "BOOST_ROOT";
                    // MessageBox.Show("USER Variable - " + variableName.Text + " does exist");
                    txtStatus.AppendText("> User Variable: " + cbVariableName.Text + ", Data: \"" + CheckRegUserStr() + "\" exists on this computer." + Environment.NewLine);
                    btnAddRegKey.Text = "Replace";
                }

                else
                {
                    //   MessageBox.Show("USER Variable - " + variableName.Text + " doesn't exist");
                    txtStatus.AppendText("> User Variable: " + cbVariableName.Text + " doesn't exist on this computer." + Environment.NewLine);
                    btnAddRegKey.Text = "Add";
                }
            }

            //system
            RegistryKey regKeySystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            if (regKeySystem != null)
            {
                if (regKeySystem.GetValue("BOOST_ROOT") != null)
                {
                    cbVariableName.Text = "BOOST_ROOT";
                    // MessageBox.Show("System Variable - " + variableName.Text + " does exist");
                    txtStatus.AppendText("> System Variable: " + cbVariableName.Text + ", Data: \"" + CheckRegSystemStr() + "\" exists on this computer." + Environment.NewLine);
                    
                }

                else
                {
                    //MessageBox.Show("System Variable - " + variableName.Text + " doesn't exist");
                    txtStatus.AppendText("> System Variable: " + cbVariableName.Text + " doesn't exist on this computer." + Environment.NewLine);

                }
            }

            regKeyUser.Close();
            regKeySystem.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SaveVarNameToFile();
            
            LoadStrTextFromFile(savedVarName, cbVariableName);

            //GetRegData();

            // Set window size
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

        private void CheckRegSystemData()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", false);
                string valueName = cbVariableName.Text;

                object obj = rk.GetValue(valueName);

                switch (rk.GetValueKind(valueName))
                {
                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                        txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + ", Data: " + obj + Environment.NewLine);

                        txtVarData.AppendText(obj.ToString());
                        
                        break;
                    case RegistryValueKind.Binary:
                        foreach (byte b in (byte[])obj)
                        {
                            txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + ", Data: " + b + Environment.NewLine);
                        }
                        break;
                    case RegistryValueKind.DWord:
                        //Console.WriteLine("Value = " + Convert.ToString((Int32)o));
                        txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + ", Data: " + Convert.ToString((Int32)obj) + Environment.NewLine);
                        break;
                    case RegistryValueKind.QWord:
                        //Console.WriteLine("Value = " + Convert.ToString((Int64)o));
                        txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + ", Data: " + Convert.ToString((Int64)obj) + Environment.NewLine);
                        break;
                    case RegistryValueKind.MultiString:
                        foreach (string s in (string[])obj)
                        {
                            //Console.Write("[{0:s}], ", s);
                            txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + ", Data: " + s + Environment.NewLine);
                        }
                        break;
                    default:
                        txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + " UNKNOWN " + Environment.NewLine);
                        break;
                }
                
                string def = (string)rk.GetValue("notavalue", "");
                txtStatus.AppendText(def /*Environment.NewLine + "System Variable: " + cbVariableName.Text + " doesn't exist on this computer." + Environment.NewLine*/);
                rk.Close();
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtStatus.AppendText(Environment.NewLine + "> System Variable: " + cbVariableName.Text + " doesn't exist on this computer." + Environment.NewLine);
            }
        }

        private void CheckRegUserData()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("Environment", false);
                string valueName = cbVariableName.Text;

                object obj = rk.GetValue(valueName);

                switch (rk.GetValueKind(valueName))
                {
                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                        txtStatus.AppendText("> User Variable: " + cbVariableName.Text + ", Data: " + obj + Environment.NewLine);
                        break;
                    case RegistryValueKind.Binary:
                        foreach (byte b in (byte[])obj)
                        {
                            txtStatus.AppendText(Environment.NewLine + "> User Variable: " + cbVariableName.Text + ", Data: " + b + Environment.NewLine);
                        }
                        break;
                    case RegistryValueKind.DWord:
                        //Console.WriteLine("Value = " + Convert.ToString((Int32)o));
                        txtStatus.AppendText(Environment.NewLine + "> User Variable: " + cbVariableName.Text + ", Data: " + Convert.ToString((Int32)obj) + Environment.NewLine);
                        break;
                    case RegistryValueKind.QWord:
                        //Console.WriteLine("Value = " + Convert.ToString((Int64)o));
                        txtStatus.AppendText(Environment.NewLine + "> User Variable: " + cbVariableName.Text + ", Data: " + Convert.ToString((Int64)obj) + Environment.NewLine);
                        break;
                    case RegistryValueKind.MultiString:
                        foreach (string s in (string[])obj)
                        {
                            //Console.Write("[{0:s}], ", s);
                            txtStatus.AppendText(Environment.NewLine + "> User Variable: " + cbVariableName.Text + ", Data: " + s + Environment.NewLine);
                        }
                        break;
                    default:
                        txtStatus.AppendText(Environment.NewLine + "> User Variable: " + cbVariableName.Text + " UNKNOWN " + Environment.NewLine);
                        break;
                }
                
                string def = (string)rk.GetValue("notavalue", "");
                txtStatus.AppendText(def /*Environment.NewLine + "User Variable: " + cbVariableName.Text + " doesn't exist on this computer." + Environment.NewLine*/);
                rk.Close();
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtStatus.AppendText("> User Variable: " + cbVariableName.Text + " doesn't exist on this computer." + Environment.NewLine);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
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

        private void cbVariableName_TextChanged(object sender, EventArgs e)
        {
            txtVarData.Clear();

            CheckRegSystemData();
            CheckRegUserData();
        }
    }
}
