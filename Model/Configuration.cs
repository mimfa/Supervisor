using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMFa_Supervisor.Model
{
    public class Configuration
    {
        public bool Enabled { get; set; } = false;
        public string SourceDirectory { get; set; } = "";
        public string DestinationDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"\\Backup\\";
        public string Filter { get; set; } = "*";
        public bool AllowCreate { get; set; } = true;
        public bool AllowDelete { get; set; } = true;
        public bool AllowModify { get; set; } = true;
        public bool ShowMessage { get; set; } = true;
        public bool AllowUpdate { get; set; } = true;
        public bool AllowLog { get; set; } = true;
        public bool AllowMessage { get; set; } = false;
        public int Number { get; set; } = 1;
        public int Speed { get; set; } = 100;

        public void Load()
        {
            if (!File.Exists(API.ConfigurationPath)) File.WriteAllText(API.ConfigurationPath,"");
            string[] configs = File.ReadAllLines(API.ConfigurationPath,Encoding.UTF8);
            foreach (var item in configs)
            {
                string[] kvp = item.Split(API.ConfigurationDelimited, StringSplitOptions.None);
                if (kvp.Length > 1)
                    switch (kvp.First().Trim().ToLower().Replace(" ", ""))
                    {
                        case "e":
                        case "enable":
                        case "enabled":
                        case "active":
                        case "actived":
                            try { Enabled = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "f":
                        case "fn":
                        case "filename":
                        case "filesname":
                        case "filter":
                        case "filters":
                            Filter = kvp.Last().Trim();
                            break;
                        case "dd":
                        case "dest":
                        case "destination":
                        case "destinationdirectory":
                            DestinationDirectory = kvp.Last().Trim().TrimEnd('\\') + "\\";
                            break;
                        case "sd":
                        case "sour":
                        case "source":
                        case "sourcedirectory":
                            SourceDirectory = kvp.Last().Trim().TrimEnd('\\') + "\\";
                            break;
                        case "aa":
                        case "allowadd":
                        case "add":
                        case "ac":
                        case "allowcreate":
                        case "create":
                            try { AllowCreate = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "ar":
                        case "allowremove":
                        case "remove":
                        case "ad":
                        case "allowdelete":
                        case "delete":
                            try { AllowDelete = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "am":
                        case "allowmodify":
                        case "modify":
                            try { AllowModify = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "allowupdate":
                        case "update":
                            try { AllowUpdate = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "allowlog":
                        case "log":
                            try { AllowLog = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "allowmessage":
                        case "message":
                            try { AllowMessage = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "s":
                        case "d":
                        case "speed":
                        case "duration":
                            try { Speed = Convert.ToInt32(kvp.Last().Trim()); }catch{ }
                            break;
                        case "n":
                        case "number":
                            try { Number = Convert.ToInt32(kvp.Last().Trim()); }catch{ }
                            break;
                        case "sm":
                        case "showmessage":
                        case "showmessages":
                            try { ShowMessage = Convert.ToBoolean(kvp.Last().Trim()); }catch{ }
                            break;
                        default:
                            break;
                    }
            }
        }
        public void Save()
        {
            List<string> configs = new List<string>()
            {
                "Enabled" + API.ConfigurationDelimited.First() + Enabled,
               "SourceDirectory" + API.ConfigurationDelimited.First() + SourceDirectory,
                "DestinationDirectory" + API.ConfigurationDelimited.First() + DestinationDirectory ,
                "Filter" + API.ConfigurationDelimited.First() + Filter,
               "AllowCreate" + API.ConfigurationDelimited.First() + AllowCreate,
               "AllowDelete" + API.ConfigurationDelimited.First() + AllowDelete,
               "AllowModify" + API.ConfigurationDelimited.First() + AllowModify,
               "AllowUpdate" + API.ConfigurationDelimited.First() + AllowUpdate,
               "AllowLog" + API.ConfigurationDelimited.First() + AllowLog,
               "AllowMessage" + API.ConfigurationDelimited.First() + AllowMessage,
               "Number" + API.ConfigurationDelimited.First() + Number,
               "Speed" + API.ConfigurationDelimited.First() + Speed,
               "ShowMessage" + API.ConfigurationDelimited.First() + ShowMessage
            };
            File.WriteAllLines(API.ConfigurationPath, configs);
        }
    }
}
