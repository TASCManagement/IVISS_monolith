
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigureSettingsAfterSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configruing Settings for IVISS..Do not close this window");
            Console.WriteLine("Configruing database setup for IVISS");
            Console.WriteLine("--------------------------------------");



            string script = null;
            script = File.ReadAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\sql.txt");
            string[] ScriptSplitter = script.Split(new string[] { "GO" }, StringSplitOptions.None);
            try
            {
                using (var cn = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True"))
                {
                    cn.Open();
                    foreach (string str in ScriptSplitter)
                    {
                        using (var cm = cn.CreateCommand())
                        {
                            cm.CommandText = str;
                            cm.ExecuteNonQuery();
                        }
                    }
                }
                Console.WriteLine("Database Created successfully");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to close--------------------------------------");
                Console.ReadLine();
            }
           

            //Process process = new Process();
            //process.StartInfo.FileName = "msiexec";


            //process.StartInfo.Arguments = string.Format(" /i \"{0}\" ALLUSERS=1", @"CR13SP27MSI64_0-10010309.msi");

            //process.StartInfo.Verb = "runas";
            //process.Start();
            //process.WaitForExit();



            //string strScript = GetSql(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\sql.txt");

           // string result = ExecuteSql(@".\sqlexpress", "master", strScript);
           

        }
       
    }
}
