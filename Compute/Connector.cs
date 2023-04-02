using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using MiMFa_Supervisor.Infra;
using System.Windows.Forms;

namespace MiMFa_Supervisor.Compute
{
    delegate void LogHandler(int type, string message);
    static class Connector
    {
        public static event LogHandler Logger = (t, m) => { };
        public static void Log(int type, string message) => Logger( type,  message);

        internal static void Create(string mainFile, string backupFile)
        {
            if (!API.Configuration.AllowCreate) return;
            try
            {
                if (API.Configuration.AllowMessage)
                {
                    if (MessageBox.Show(mainFile + (API.Configuration.AllowUpdate ? Environment.NewLine + "Do you want to take a Backup from this file?":""), "Created File", API.Configuration.AllowUpdate ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.Yes)
                        File.Copy(mainFile, backupFile, true);
                }
                else if (API.Configuration.AllowUpdate) File.Copy(mainFile, backupFile, true);
                if (API.Configuration.AllowLog) Logger(1, "Backup was successfull Created in '" + backupFile + "'");
            }
            catch (Exception ex)
            {
                if (API.Configuration.AllowLog) Logger(-1, "[310" + (ex.Message.Length / 10) + "]\t" + ex.Message);
                if (API.Configuration.ShowMessage && MessageBox.Show("Backup was not successfull Created from'" + mainFile + "'!" + Environment.NewLine + "Do you want to try it again?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Create(mainFile, backupFile);
            }
        }
        internal static void Delete(string backupFile)
        {
            if (!API.Configuration.AllowDelete) return;
            try
            {
                if (API.Configuration.AllowMessage)
                {
                    if (MessageBox.Show(backupFile + (API.Configuration.AllowUpdate ? Environment.NewLine + "Do you want to Delete this file?" : ""), "Deleted File", API.Configuration.AllowUpdate ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.Yes)
                        File.Delete(backupFile);
                }
                else if (API.Configuration.AllowUpdate) File.Delete(backupFile);
                if (API.Configuration.AllowLog) Logger(1, "Backup was successfull Deleted from '" + backupFile + "'");
            }
            catch (Exception ex)
            {
                if (API.Configuration.AllowLog) Logger(-1, "[320" + (ex.Message.Length / 10) + "]\t" + ex.Message);
                if (API.Configuration.ShowMessage && MessageBox.Show("Backup was not successfull Deleted from'" + backupFile + "'!" + Environment.NewLine + "Do you want to try it again?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Delete(backupFile);
            }
        }
        internal static void Modify(string mainFile, string backupFile)
        {
            if (!API.Configuration.AllowModify) return;
            try
            {
                if (API.Configuration.AllowMessage)
                {
                    if (MessageBox.Show(mainFile + (API.Configuration.AllowUpdate ? Environment.NewLine + "Do you want to take a Backup from this file?" : ""), "Modified File", API.Configuration.AllowUpdate ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.Yes)
                        File.Copy(mainFile, backupFile, true);
                }
                else if (API.Configuration.AllowUpdate) File.Copy(mainFile, backupFile, true);
                if (API.Configuration.AllowLog) Logger(1, "Backup was successfull Modified in '" + backupFile + "'");
            }
            catch (Exception ex)
            {
                if (API.Configuration.AllowLog) Logger(-1, "[330" + (ex.Message.Length / 10) + "]\t" + ex.Message);
                if (API.Configuration.ShowMessage && MessageBox.Show("Backup was not successfull Modified from '" + mainFile + "'!" + Environment.NewLine + "Do you want to try it again?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Modify(mainFile, backupFile);
            }
        }

        internal static void CreateDirectory(string backupDirectory)
        {
            if (!API.Configuration.AllowCreate) return;
            try
            {
                if (!Directory.GetParent(backupDirectory.TrimEnd('\\')).Exists) CreateDirectory(Directory.GetParent(backupDirectory).FullName);
                if (API.Configuration.AllowMessage)
                {
                    if (MessageBox.Show(backupDirectory + (API.Configuration.AllowUpdate ? Environment.NewLine + "Do you want to Create this Directory?" : ""), "Created Directory", API.Configuration.AllowUpdate ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.Yes)
                        Directory.CreateDirectory(backupDirectory);
                }
                else if (API.Configuration.AllowUpdate) Directory.CreateDirectory(backupDirectory);
                if (API.Configuration.AllowLog) Logger(1, "Directory was successfull created in '" + backupDirectory + "'");
            }
            catch (Exception ex)
            {
                if (API.Configuration.AllowLog) Logger(-1, "[340" + (ex.Message.Length / 10) + "]\t" + ex.Message);
                if (API.Configuration.ShowMessage && MessageBox.Show("Directory was not created in '" + backupDirectory + "'!" + Environment.NewLine + "Do you want to try it again?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    CreateDirectory(backupDirectory);
            }
        }
        internal static void DeleteDirectory(string backupDirectory)
        {
            if (!API.Configuration.AllowDelete) return;
            try
            {
                if (Directory.GetParent(backupDirectory).Exists)
                    if (API.Configuration.AllowMessage)
                    {
                        if (MessageBox.Show(backupDirectory + (API.Configuration.AllowUpdate ? Environment.NewLine + "Do you want to Delete this Directory?" : ""), "Deleted Directory", API.Configuration.AllowUpdate ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.Yes)
                            Directory.Delete(backupDirectory, true);
                    }
                    else if (API.Configuration.AllowUpdate) Directory.Delete(backupDirectory,true);
                if (API.Configuration.AllowLog) Logger(1, "Directory was successfull deleted from '" + backupDirectory + "'");
            }
            catch (Exception ex)
            {
                if (API.Configuration.AllowLog) Logger(-1, "[350" + (ex.Message.Length / 10) + "]\t" + ex.Message);
                if (API.Configuration.ShowMessage && MessageBox.Show("Directory was not deleted from '" + backupDirectory + "'!" + Environment.NewLine + "Do you want to try it again?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    DeleteDirectory(backupDirectory);
            }
        }

    }
}
