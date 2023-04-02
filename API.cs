using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MiMFa_Supervisor.Model;

namespace MiMFa_Supervisor
{
    public static class API
    {
        public static Configuration Configuration { get; set; } = new Configuration();
        public static string ConfigurationPath { get; set; } = "Supervisor.cnf";
        public static string[] ConfigurationDelimited { get; set; } = new string[] { "->" };

        public static void Start()
        {
            MiMFa_Framework.Config.ProductSignature = "fg/sup";
            Configuration = new Configuration();
            Configuration.Load();
        }
    }
}
