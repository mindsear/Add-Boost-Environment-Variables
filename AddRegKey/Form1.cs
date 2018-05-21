using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AddRegKey
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAddRegKey_Click(object sender, EventArgs e)
        {
            try
            {
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

            userVar.SetValue(variableName.Text, txtBoostPath.Text);
            userVar.Close();
        }

        private void AddSystemVariable()
        {
            RegistryKey 

            //systemVar = Registry.LocalMachine.CreateSubKey(@"SYSTEM\ControlSet001\Control\Session Manager\Environment");
            systemVar = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");

            systemVar.SetValue(variableName.Text, txtBoostPath.Text);
            systemVar.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                txtBoostPath.Text = folderBrowserDialog1.SelectedPath;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                checkBox1.Text = "Locked";
                variableName.ReadOnly = true;
            }
            else
            {
                checkBox1.Text = "Unlocked";
                variableName.ReadOnly = false;
            }
        }

        private void GetRegData()
        {
            // user
            RegistryKey regKeyUser = Registry.CurrentUser.OpenSubKey("Environment");
            if (regKeyUser != null)
            {
                if (regKeyUser.GetValue(variableName.Text) != null)
                {
                    // MessageBox.Show("USER Variable - " + variableName.Text + " does exist");
                    txtStatus.AppendText("User Variable " + variableName.Text + ", Path: \"" + CheckRegUserStr() + "\" exists on this computer." + Environment.NewLine);
                    btnAddRegKey.Text = "Replace";
                }

                else
                {
                    //   MessageBox.Show("USER Variable - " + variableName.Text + " doesn't exist");
                    txtStatus.AppendText("User Variable " + variableName.Text + " does NOT exist on this computer." + Environment.NewLine);
                    btnAddRegKey.Text = "Add";
                }
            }

            //system
            RegistryKey regKeySystem = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            if (regKeySystem != null)
            {
                if (regKeySystem.GetValue(variableName.Text) != null)
                {
                    // MessageBox.Show("System Variable - " + variableName.Text + " does exist");
                    txtStatus.AppendText("System Variable " + variableName.Text + ", Path: \"" + CheckRegSystemStr() + "\" exists on this computer." + Environment.NewLine);

                }

                else
                {
                    //MessageBox.Show("System Variable - " + variableName.Text + " doesn't exist");
                    txtStatus.AppendText("System Variable " + variableName.Text + " does NOT exist on this computer." + Environment.NewLine);

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = true;

            GetRegData();
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
    }
}
