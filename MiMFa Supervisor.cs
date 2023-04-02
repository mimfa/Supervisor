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
using MiMFa_Supervisor.Compute;
using MiMFa_Supervisor.Infra;
using System.Threading;
using MiMFa_Framework.General;
using MiMFa_Framework.Service;
using MiMFa_Framework.Support.Update;
using MiMFa_Framework.Log;

namespace MiMFa_Supervisor
{
    public partial class Supervisor : Form
    {
        Thread MainThread = new Thread(() => { });

        public Supervisor()
        {
            InitializeComponent();
            Connector.Logger += Connector_Logger;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue(Application.ProductName, Application.ExecutablePath);
            if (!File.Exists(API.ConfigurationPath)) WindowState = FormWindowState.Normal;
            Start();
            if (!API.Configuration.Enabled) WindowState = FormWindowState.Normal;
            SendLog();
            CheckUpdate();
        }

        private void Connector_Logger(int type, string message)
        {
            message += Environment.NewLine;
            switch (type)
            {
                case -1:
                    MiMFa_ControlService.SetControlThreadSafe(RTB, (arg) => Service.RichTextBoxAppendWithStyle(ref RTB, "ERROR:\t" + message, Color.Red));
                    break;
                case 0:
                    MiMFa_ControlService.SetControlThreadSafe(RTB, (arg) => Service.RichTextBoxAppendWithStyle(ref RTB, "PROCESS:\t" + message, Color.DimGray));
                    break;
                case 1:
                    MiMFa_ControlService.SetControlThreadSafe(RTB, (arg) => Service.RichTextBoxAppendWithStyle(ref RTB, "SUCCESS:\t" + message, Color.Green));
                    break;
                default:
                    MiMFa_ControlService.SetControlThreadSafe(RTB, (arg) => Service.RichTextBoxAppendWithStyle(ref RTB, "MESSAGE:\t"+ message, Color.Black));
                    break;
            }
            MiMFa_ControlService.SetControlThreadSafe(RTB, (arg) => { if (RTB.Lines.Length > 1000) RTB.Clear(); });
        }

        private void Start()
        {
            API.Start();
            LoadData(true);
            TMR.Interval = API.Configuration.Speed * 1000;
        }
        private string GetParameters=>
                    "key=6df5svdf1365165dav16&" +
                   "company=" + System.Windows.Forms.Application.CompanyName + "&" +
                   "product=" + System.Windows.Forms.Application.ProductName + "&" +
                   "version=" + System.Windows.Forms.Application.ProductVersion + "&" +
                   "ip=" + MiMFa_Framework.Network.MiMFa_Net.GetInternalIPv4() + "&" +
                    "mac=" + MiMFa_Framework.Network.MiMFa_Net.GetMAC() + "&" +
                    "stdate=" + "*" + "&" +
                    "exdate=" + "*" + "&" +
                    "username=" + MiMFa_Framework.Network.MiMFa_Net.GetHostName() + "&" +
                    "useraddress=" + "*" + "&" +
                    "userphone=" + "*" + "&" +
                    "userstatus=" + 0 + "&" +
                    "licensetype=" + 0 + "&" +
                    "haslog=" + 0 + "&" +
                    "date=" + MiMFa_Framework.Configuration.Default.Date + "&" +
                    "time=" + MiMFa_Framework.Configuration.Default.Time;
        private void CheckUpdate()
        {
            try
            {
                Updater updater = new Updater();
                if (updater.CheckUpdateByDialog(GetParameters, null, null)) Close();
            }
            catch { }
        }
        private void SendLog()
        {try
            {
                Logger logger = new Logger();
                logger.ExecuteLog(GetParameters, null);
            }
            catch { }
        }
        private void LoadData(bool speed = false)
        {
            try
            {
                API.Configuration.Load();
                tb_dir.Text = API.Configuration.SourceDirectory;
                tb_filter.Text = API.Configuration.Filter;
                tb_dest.Text = API.Configuration.DestinationDirectory;
                cb_Add.Checked = API.Configuration.AllowCreate;
                cb_Remove.Checked = API.Configuration.AllowDelete;
                cb_Modify.Checked = API.Configuration.AllowModify;
                cb_Update.Checked = API.Configuration.AllowUpdate;
                cb_Message.Checked = API.Configuration.AllowMessage;
                cb_Log.Checked = API.Configuration.AllowLog;
                nud_Number.Value = API.Configuration.Number;
                if (speed)
                {
                    cb_ShowMessage.Checked = API.Configuration.ShowMessage;
                    t_Speed.Value = API.Configuration.Speed;
                    NI.BalloonTipText = MiMFa_Convert.ToTimeString(t_Speed.Value, "") + " Backup from " + API.Configuration.SourceDirectory.TrimEnd('\\').Split('\\').Last();
                    Text = Application.ProductName + ": " + NI.BalloonTipText;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error 110" + (ex.Message.Length/10), MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        bool GetBackup = true;
        private void TMR_Tick(object sender, EventArgs e)
        {
            if (!GetBackup) return;
            if (GetBackup = API.Configuration.ShowMessage && MessageBox.Show("Do you want to start Backup by below detail?" + Environment.NewLine + API.Configuration.SourceDirectory + Environment.NewLine + "To" + Environment.NewLine + API.Configuration.DestinationDirectory, "Allowance Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            MainThread = new Thread(() =>
            {
                DateTime dt = DateTime.Now;
                string time = dt.ToLongDateString() + " " + dt.ToLongTimeString();
                try
                {
                    Connector.Log(0, time);
                    Connector.Log(0, "------START BACKUP------");
                    string destdir = API.Configuration.DestinationDirectory;
                    if (API.Configuration.Number > 1)
                    {
                        string name = API.Configuration.SourceDirectory.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last() + " ";
                        destdir = API.Configuration.DestinationDirectory + name + (dt.ToShortDateString() + " " + dt.ToLongTimeString()).Replace(":", "-").Replace("/", "-").Replace("\\", "-") + "\\";
                        string[] bcks = Directory.GetDirectories(API.Configuration.DestinationDirectory, name + "*", SearchOption.TopDirectoryOnly);
                        if (bcks.Length >= API.Configuration.Number && API.Configuration.AllowUpdate)
                            try { Directory.Move(bcks.First().TrimEnd('\\') + "\\", destdir.TrimEnd('\\') + "\\"); }
                            catch (Exception ex)
                            {
                                Connector.Log(-1, "[211" + (ex.Message.Length / 10)+ "]\tTaking the base of data was occured an Error! [" + ex.Message + "]");
                                destdir = bcks.First().TrimEnd('\\') + "\\";
                            }
                    }
                    MiMFa_ControlService.SetControlThreadSafe(p_PB, (arg) => p_PB.Visible = true);
                    PB.Value = 0;
                    MiMFa_ControlService.SetControlThreadSafe(btn_Save, (arg) => btn_Save.Enabled = false);
                    Connector.CreateDirectory(destdir);
                    string[] dirs = Directory.GetDirectories(API.Configuration.SourceDirectory, "*", SearchOption.AllDirectories);
                    string[] bdirs = API.Configuration.AllowDelete ? Directory.GetDirectories(destdir, "*", SearchOption.AllDirectories) : new string[0];
                    string[] files = Directory.GetFiles(API.Configuration.SourceDirectory, API.Configuration.Filter, SearchOption.AllDirectories);
                    string[] bfiles = API.Configuration.AllowDelete ? Directory.GetFiles(destdir, API.Configuration.Filter, SearchOption.AllDirectories) : new string[0];
                    PB.Maximum = dirs.Length + bdirs.Length + files.Length + bfiles.Length;
                    int snum = API.Configuration.SourceDirectory.Length;
                    int dnum = destdir.Length;
                    foreach (var sour in dirs)
                    {
                        string dest = destdir + sour.Substring(snum);
                        if (!Directory.Exists(dest)) Connector.CreateDirectory(dest);
                        PB.Value++;
                    }
                    foreach (var dest in bdirs)
                    {
                        string sour = API.Configuration.SourceDirectory + dest.Substring(dnum);
                        if (!Directory.Exists(sour)) Connector.DeleteDirectory(dest);
                        PB.Value++;
                    }
                    foreach (var sour in files)
                    {
                        string dest = destdir + sour.Substring(snum);
                        if (File.Exists(dest))
                        {
                            FileInfo fm = new FileInfo(sour);
                            FileInfo fb = new FileInfo(dest);
                            if (fm.LastWriteTime.Ticks > fb.CreationTime.Ticks || fb.LastWriteTime.Ticks > fb.CreationTime.Ticks) Connector.Modify(sour, dest);
                        }
                        else Connector.Create(sour, dest);
                        PB.Value++;
                    }
                    foreach (var dest in bfiles)
                    {
                        string sour = API.Configuration.SourceDirectory + dest.Substring(dnum);
                        if (!File.Exists(sour)) Connector.Delete(dest);
                        PB.Value++;
                    }
                    Connector.Log(1, "Taking Backup was Successful! Your Backup is up to " + time);
                }
                catch (Exception ex)
                {
                    Connector.Log(-1, "[210" + (ex.Message.Length / 10) + "]\tTaking Backup occured an Error! [" + ex.Message + "]");
                    if (API.Configuration.ShowMessage)
                        API.Configuration.ShowMessage = MessageBox.Show(ex.Message + Environment.NewLine + "Do you want to see again this message?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes;
                }
                finally
                {
                    MiMFa_ControlService.SetControlThreadSafe(p_PB, (arg) => p_PB.Visible = false);
                    PB.Value = 0;
                    Connector.Log(0, "------FINISH BACKUP------" + Environment.NewLine);
                    MiMFa_ControlService.SetControlThreadSafe(btn_Save, (arg) => btn_Save.Enabled = true);
                    GetBackup = true;
                }
            });
            MainThread.SetApartmentState(ApartmentState.STA);
            MainThread.IsBackground = true;
            btn_Start.Visible = false;
            btn_Pause.Visible = true;
            p_PB.Visible = true;
            MainThread.Start();
        }
        private void t_Speed_ValueChanged(object sender, EventArgs e)
        {
            NI.BalloonTipText = MiMFa_Convert.ToTimeString(t_Speed.Value, "") + " Backup from " + API.Configuration.SourceDirectory.TrimEnd('\\').Split('\\').Last();
            Text = Application.ProductName + ": " + NI.BalloonTipText;
            TMR.Interval = (API.Configuration.Speed = t_Speed.Value) * 1000;
        }
        private void btn_select_Click(object sender, EventArgs e)
        {
            try { FBD.SelectedPath = tb_dir.Text; } catch { }
            if (FBD.ShowDialog() == DialogResult.OK && !tb_dest.Text.StartsWith(FBD.SelectedPath)) tb_dir.Text = FBD.SelectedPath;
        }
        private void btn_select_Dest_Click(object sender, EventArgs e)
        {
            try { FBD.SelectedPath = tb_dest.Text; } catch { }
            if (FBD.ShowDialog() == DialogResult.OK && !FBD.SelectedPath.StartsWith(tb_dir.Text)) tb_dest.Text = FBD.SelectedPath;
        }
        private void label4_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(tb_dir.Text);
        }
        private void label2_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(tb_dest.Text);
        }
        private void btn_ofd_Click(object sender, EventArgs e)
        {
            try { OFD.InitialDirectory = tb_dir.Text; } catch { }
            try { OFD.FileName = tb_filter.Text; } catch { }
            if (OFD.ShowDialog() == DialogResult.OK) tb_filter.Text = Path.GetFileName(OFD.FileName);
        }
        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (!API.Configuration.ShowMessage || MessageBox.Show("Are you sure to save these configurations?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                try
                {
                    API.Configuration.SourceDirectory = Path.GetFullPath(tb_dir.Text).TrimEnd('\\') + "\\";
                    API.Configuration.DestinationDirectory = Path.GetFullPath(tb_dest.Text).TrimEnd('\\') + "\\";
                    API.Configuration.Filter = string.IsNullOrEmpty(tb_filter.Text)? tb_filter.Text = "*": tb_filter.Text;
                    API.Configuration.AllowCreate = cb_Add.Checked;
                    API.Configuration.AllowDelete = cb_Remove.Checked;
                    API.Configuration.AllowModify = cb_Modify.Checked;
                    API.Configuration.AllowMessage = cb_Message.Checked;
                    API.Configuration.AllowLog = cb_Log.Checked;
                    API.Configuration.AllowUpdate = cb_Update.Checked;
                    API.Configuration.ShowMessage = cb_ShowMessage.Checked;
                    API.Configuration.Number = Convert.ToInt32(nud_Number.Value);
                    API.Configuration.Speed = t_Speed.Value;
                    API.Configuration.Save();
                    Start();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message , "Error 120" + (ex.Message.Length / 10), MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void TC_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TC.SelectedTab == tp_Conf)
                LoadData();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://mimfa.net");
        }

        private void ExcelToWeb_FormClosing(object sender, FormClosingEventArgs e)
        {
            API.Configuration.Save();
            if (MessageBox.Show("Are you sure to disable this service!","Attention",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.No) e.Cancel = true;
        }

        public void Show(int index)
        {
            TopMost = true;
            try { TC.SelectedIndex = index; } catch { }
            WindowState = FormWindowState.Normal;
            TopMost = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show(0);
        }
        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show(1);
        }

        private void cts_Active_CheckedChanged(object sender, EventArgs e)
        {
            TMR.Enabled = cb_Enable.Checked = cts_Active.Checked;
        }
        private void cb_Enable_CheckedChanged(object sender, EventArgs e)
        {
            API.Configuration.Enabled = TMR.Enabled = cts_Active.Checked = cb_Enable.Checked;
            if (cb_Enable.Checked)
            {
                cb_Enable.Text = "Enabled";
                cb_Enable.ForeColor = Color.Green;
                cb_Enable.BackColor = Color.Transparent;
                TMR_Tick(TMR,EventArgs.Empty);
            }
            else
            {
                cb_Enable.Text = "Disabled";
                cb_Enable.ForeColor = Color.White;
                cb_Enable.BackColor = Color.Tomato;
            }
        }
        private void NI_DoubleClick(object sender, EventArgs e)
        {
            Show(9);
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (MainThread != null)
            {
                MainThread.Resume();
                MiMFa_ControlService.SetControlThreadSafe(btn_Start, (arg) => btn_Start.Visible = false);
                MiMFa_ControlService.SetControlThreadSafe(btn_Pause, (arg) => btn_Pause.Visible = true);
            }
        }
        private void btn_Pause_Click(object sender, EventArgs e)
        {
            if (MainThread != null)
            {
                MainThread.Suspend();
                MiMFa_ControlService.SetControlThreadSafe(btn_Start, (arg) => btn_Start.Visible = true);
                MiMFa_ControlService.SetControlThreadSafe(btn_Pause, (arg) => btn_Pause.Visible = false);
            }
        }
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            if (MainThread != null)
            {
                try { MainThread.Resume(); } catch { }
                MainThread.Abort();
                p_PB.Visible = false;
                PB.Value = 0;
                Connector.Log(-1, "Backuping is Stop!");
                Connector.Log(0, "------STOP BACKUP------" + Environment.NewLine);
                btn_Save.Enabled = GetBackup = true;
            }
        }

        private void cb_ShowMessage_CheckedChanged(object sender, EventArgs e)
        {
            API.Configuration.ShowMessage = cb_ShowMessage.Checked;
        }

    }
}
