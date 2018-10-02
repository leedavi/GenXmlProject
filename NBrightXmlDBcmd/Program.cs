using System;
using NBright.GenXmlDB;
using System.Reflection;
using System.Linq;

namespace NBrightXmlDBcmd
{
    class Program
    {
        //private static string project_root = @"D:\Projects\";
        private static string project_root = @"D:\Projects\GenXmlProject\";
        //private static string test_sql_conn = @"Data Source=DCL-PC\SQLEXPRESS01;Initial Catalog=GenXmlDB;User ID=sa;Password=HsQ2A5pn";
        private static string test_sql_conn = @"Data Source=.\SQLEXPRESS02;Initial Catalog=GenXmlDB;User ID=sa;Password=HsQ2A5pn";

        static void Main(string[] args)
        {
            Console.WriteLine("S => SQL | D => DBreeze | O => Other");
            ConsoleKeyInfo cki;
            bool outloop = false;
            do
            {
                outloop = false;
                cki = Console.ReadKey(true);
                switch (cki.KeyChar)
                {
                    case 'S':
                    case 's':
                        Console.WriteLine("SQL serveur");
                        RunSQLDB(args);
                        break;
                    case 'D':
                    case 'd':
                        Console.WriteLine("DBreeze");
                        MainDBreeze(args);
                        break;
                    case 'O':
                    case 'o':
                        Console.WriteLine("Other");
                        break;
                    default:
                        Console.WriteLine("Unknown");
                        outloop = true;
                        break;
                }
            }
            while (!outloop);
        }
        static void RunSQLDB(string[] args)
        {
            NBrightInfo nbiConfig = new NBrightInfo();
            nbiConfig.SetXmlProperty("genxml/provider/assembly", string.Format(@"{0}GenXmlSQLprovider\bin\Debug\netcoreapp2.0\GenXmlSQLprovider.dll", project_root));
            //nbiConfig.SetXmlProperty("genxml/provider/assembly", string.Format(@"{0}GenXmlSQLprovider\bin\Debug\netcoreapp1.1\GenXmlSQLprovider.dll", project_root));
            nbiConfig.SetXmlProperty("genxml/provider/namespaceclass", "GenXmlSQLprovider.SqlController");
            nbiConfig.SetXmlProperty("genxml/provider/connectionstring", test_sql_conn);
            nbiConfig.SetXmlProperty("genxml/provider/sqltablename", "XMLDATA");
            nbiConfig.SetXmlProperty("genxml/provider/objectQualifier", "");
            nbiConfig.SetXmlProperty("genxml/provider/databaseOwner", "dbo");

            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly1", @"System.Xml.XmlSerializer.dll");
            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly2", @"System.Data.SqlClient.dll");
            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly3", @"System.Text.Encoding.CodePages.dll");
            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly4", @"sni.dll");
            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly5", @"C:\Users\leedavi\.nuget\packages\runtime.win-x64.runtime.native.system.data.sqlclient.sni\4.4.0\runtimes\win-x64\native\sni.dll");
            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly6", @"C:\Users\leedavi\.nuget\packages\runtime.win-x86.runtime.native.system.data.sqlclient.sni\4.4.0\runtimes\win-x86\native\sni.dll");
            //nbiConfig.SetXmlProperty("genxml/dependancy/assembly4", @"");


            DataBaseContainer dbCtrl = new DataBaseContainer();
            dbCtrl.Connect(nbiConfig.XmlString);
            Console.WriteLine("SQL server is now connected");
            PerformTests(dbCtrl);
            dbCtrl.Disconnect();

            Console.WriteLine("Exit");
            Console.ReadLine();
        }

        static void MainDBreeze(string[] args)
        {
            // --------------------------------------------------------------
            // this is NOT used, method name changed to keep code in file.
            // --------------------------------------------------------------

            NBrightInfo nbiConfig = new NBrightInfo();
            nbiConfig.SetXmlProperty("genxml/provider/assembly", string.Format(@"{0}NBrightDBreezeProvider\bin\Debug\netcoreapp2.0\GenXmlDBreezeProvider.dll", project_root));
            //nbiConfig.SetXmlProperty("genxml/provider/assembly", string.Format(@"{0}NBrightDBreezeProvider\bin\Debug\netcoreapp1.1\NBrightDBreezeProvider.dll", project_root));
            nbiConfig.SetXmlProperty("genxml/provider/namespaceclass", "GenXmlDBreezeProvider.DBreezeController");
            nbiConfig.SetXmlProperty("genxml/provider/dbpath", string.Format(@"{0}NBrightXmlDBcmd\App_Data\db1", project_root));

            nbiConfig.SetXmlProperty("genxml/dependancy/assembly1", string.Format(@"{0}lib\DBreeze.dll", project_root));
            nbiConfig.SetXmlProperty("genxml/dependancy/assembly2", string.Format(@"{0}lib\NetJSON.Core.dll", project_root));
            nbiConfig.SetXmlProperty("genxml/dependancy/assembly3", string.Format(@"{0}lib\System.Xml.XmlSerializer.dll", project_root));

            DataBaseContainer dbCtrl = new DataBaseContainer();
            dbCtrl.Connect(nbiConfig.XmlString);
            Console.WriteLine("DBreeze is now connected");
            PerformTests(dbCtrl);
            dbCtrl.Disconnect();

            Console.WriteLine("Exit");
            Console.ReadLine();
        }

        private static void PerformTests(IDataBaseInterface dbCtrl)
        {
            string l1 = "temp";
            while (l1 != "exit" && l1 != string.Empty)
            {
                Console.Write("> ");
                l1 = Console.ReadLine();
                Console.WriteLine("INPUT:" + l1);
                switch (l1)
                {
                    case "i":
                        Insert(dbCtrl);
                        break;
                    case "r":
                        Read(dbCtrl);
                        break;
                    case "dt":
                        dbCtrl.DeleteTable("DATA");
                        break;
                    case "xml":
                        XMLFeat();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void Insert(IDataBaseInterface dbCtrl)
        {
            string[] lines = System.IO.File.ReadAllLines(string.Format(@"{0}NBrightXmlDBcmd\App_Data\testdata.txt", project_root));
            foreach (string line in lines)
            {
                Console.WriteLine(line);
                if (line != "")
                {
                    string[] sl = line.Split(',');
                    NBrightInfo nbd = new NBrightInfo();
                    nbd.ItemId = Convert.ToInt32(sl[0]);
                    nbd.Lang = sl[1];
                    nbd.ModifiedDate = Convert.ToDateTime(sl[2]);
                    nbd.ModuleId = Convert.ToInt32(sl[3]);
                    nbd.ParentItemId = Convert.ToInt32(sl[4]);
                    nbd.PortalId = Convert.ToInt32(sl[5]);
                    nbd.KeyData = sl[6];
                    nbd.TableCode = sl[7];
                    nbd.TextData = sl[8];
                    nbd.UserId = Convert.ToInt32(sl[9]);
                    nbd.XmlString = sl[10];
                    nbd.XrefItemId = Convert.ToInt32(sl[11]);
                    dbCtrl.Update(nbd);
                }
            }
        }

        private static void Read(IDataBaseInterface dbCtrl)
        {
            Console.WriteLine("Get List DATA ----------------------------------- ");
            var rtnList = dbCtrl.GetListByPortalId("DATA", 1);
            foreach (var nbi1 in rtnList)
            {
                Console.WriteLine(nbi1.ItemId + " " + nbi1.Lang + " " + nbi1.ModifiedDate + " " + nbi1.ModuleId + " " + nbi1.ParentItemId + " " + nbi1.PortalId + " " + nbi1.KeyData + " " + nbi1.TableCode + " " + nbi1.TextData + " " + nbi1.UserId + " " + nbi1.XmlString.Replace(" ", "").Replace(Environment.NewLine, "") + " " + nbi1.XrefItemId + " genxml/textbox/name:" + nbi1.GetXmlProperty("genxml/textbox/name") + Environment.NewLine);
            }

            Console.WriteLine("Get List DATALANG ----------------------------------- ");
            rtnList = dbCtrl.GetListByPortalId("DATALANG", 1);
            foreach (var nbi1 in rtnList)
            {
                Console.WriteLine(nbi1.ItemId + " " + nbi1.Lang + " " + nbi1.ModifiedDate + " " + nbi1.ModuleId + " " + nbi1.ParentItemId + " " + nbi1.PortalId + " " + nbi1.KeyData + " " + nbi1.TableCode + " " + nbi1.TextData + " " + nbi1.UserId + " " + nbi1.XmlString.Replace(" ", "").Replace(Environment.NewLine, "") + " " + nbi1.XrefItemId + " genxml/textbox/name:" + nbi1.GetXmlProperty("genxml/textbox/lname") + Environment.NewLine);
            }

            Console.WriteLine("Get List fr-FR ----------------------------------- ");
            rtnList = dbCtrl.GetListByPortalId("DATA", 1, "fr-FR");
            foreach (var nbi1 in rtnList)
            {
                Console.WriteLine(nbi1.ItemId + " " + nbi1.Lang + " " + nbi1.ModifiedDate + " " + nbi1.ModuleId + " " + nbi1.ParentItemId + " " + nbi1.PortalId + " " + nbi1.KeyData + " " + nbi1.TableCode + " " + nbi1.TextData + " " + nbi1.UserId + " " + nbi1.XmlString.Replace(" ", "").Replace(Environment.NewLine, "") + " " + nbi1.XrefItemId + " genxml/textbox/name:" + nbi1.GetXmlProperty("genxml/textbox/name") + Environment.NewLine);
            }

            Console.WriteLine("Get List en-US ----------------------------------- ");
            rtnList = dbCtrl.GetListByPortalId("DATA", 1, "en-US");
            foreach (var nbi1 in rtnList)
            {
                Console.WriteLine(nbi1.ItemId + " " + nbi1.Lang + " " + nbi1.ModifiedDate + " " + nbi1.ModuleId + " " + nbi1.ParentItemId + " " + nbi1.PortalId + " " + nbi1.KeyData + " " + nbi1.TableCode + " " + nbi1.TextData + " " + nbi1.UserId + " " + nbi1.XmlString.Replace(" ", "").Replace(Environment.NewLine, "") + " " + nbi1.XrefItemId + " genxml/textbox/name:" + nbi1.GetXmlProperty("genxml/textbox/name") + Environment.NewLine);
            }

            Console.WriteLine("----------------------------------- ");
            Console.WriteLine("DELETE 3 ----------------------------------- ");
            Console.WriteLine("----------------------------------- ");

            dbCtrl.DeleteKey("DATA", 3);

            Console.WriteLine("Get List DATA ----------------------------------- ");
            rtnList = dbCtrl.GetListByPortalId("DATA", 1);
            foreach (var nbi1 in rtnList)
            {
                Console.WriteLine(nbi1.ItemId + " " + nbi1.Lang + " " + nbi1.ModifiedDate + " " + nbi1.ModuleId + " " + nbi1.ParentItemId + " " + nbi1.PortalId + " " + nbi1.KeyData + " " + nbi1.TableCode + " " + nbi1.TextData + " " + nbi1.UserId + " " + nbi1.XmlString.Replace(" ", "").Replace(Environment.NewLine, "") + " " + nbi1.XrefItemId + " genxml/textbox/name:" + nbi1.GetXmlProperty("genxml/textbox/name") + Environment.NewLine);
            }
            Console.WriteLine("Get List DATALANG ----------------------------------- ");
            rtnList = dbCtrl.GetListByPortalId("DATALANG", 1);
            foreach (var nbi1 in rtnList)
            {
                Console.WriteLine(nbi1.ItemId + " " + nbi1.Lang + " " + nbi1.ModifiedDate + " " + nbi1.ModuleId + " " + nbi1.ParentItemId + " " + nbi1.PortalId + " " + nbi1.KeyData + " " + nbi1.TableCode + " " + nbi1.TextData + " " + nbi1.UserId + " " + nbi1.XmlString.Replace(" ", "").Replace(Environment.NewLine, "") + " " + nbi1.XrefItemId + " genxml/textbox/name:" + nbi1.GetXmlProperty("genxml/textbox/lname") + Environment.NewLine);
            }


            //Console.WriteLine("get Free Text ");
            //rtnList = dbCtrl.GetDataByFreeText("DATA", "free");
            //foreach (var nbi2 in rtnList)
            //{
            //    Console.WriteLine(nbi2.GetXmlProperty("genxml/textbox/name") + " : " + nbi2.XmlString + " : " + nbi2.PortalId);

            //}
            //Console.WriteLine("get Lang by ParentItemId -------------");
            //var nbi = dbCtrl.GetDataByParentIdLang("DATA",1,"fr-FR");
            //if (nbi != null)
            //{
            //    Console.WriteLine(nbi.GetXmlProperty("genxml/textbox/lname") + " : " + nbi.XmlString + " : " + nbi.PortalId);
            //}
            //Console.WriteLine("get Data with Lang -------------");
            //var nbi3 = dbCtrl.GetLangData("DATA", 1, "fr-FR");
            //if (nbi3 != null)
            //{
            //    Console.WriteLine(nbi3.GetXmlProperty("genxml/lang/genxml/textbox/lname") + " : " + nbi3.XmlString + " : " + nbi3.PortalId);
            //}
        }

        private static void XMLFeat()
        {
            NBrightInfo nbi = new NBrightInfo
            {
                XmlString = "<root><test></test><int>22</int></root>"
            };

            Console.WriteLine("nbi.XMLData: " + nbi.XmlString);
            Console.WriteLine("GetXmlProperty root/test: " + nbi.GetXmlProperty("root/test"));

            Console.WriteLine("GetXmlPropertyInt root/test: " + nbi.GetXmlPropertyInt("root/test"));
            Console.WriteLine("GetXmlPropertyInt root/int: " + nbi.GetXmlPropertyInt("root/int"));

            nbi.SetXmlPropertyDouble("root/test/never", 99.97);
            nbi.SetXmlProperty("root/test/never/again", "NEW VALUE");
            Console.WriteLine("NEW GetXmlProperty root/test/never/again: " + nbi.GetXmlProperty("root/test/never/again"));

            nbi.SetXmlPropertyDouble("root/test/never", 199.97);
            nbi.SetXmlProperty("root/test/never/date", DateTime.Now.ToString(), TypeCode.DateTime);

            Console.WriteLine(nbi.GetXmlProperty("root/test/never/date"));
            Console.WriteLine(nbi.GetXmlPropertyFormat("root/test/never/date"));
            Console.WriteLine(nbi.GetXmlPropertyFormat("root/test/never/date", "yyyy-MM-dd"));

            nbi.ReplaceXmlSection("<lang><ass>ASS</ass></lang>");
            nbi.ReplaceXmlSection("<ass2><ass3>ASS3</ass3></ass2>", "root/lang/ass");
            nbi.ReplaceXmlSection("<ass2><ass3>ASS4</ass3></ass2>", "root/lang/ass");
            nbi.ReplaceXmlSection("<ass2><ass3>ASS5</ass3></ass2>", "root/lang/ass");
            nbi.AddXmlLang("<genxml><test1></test1><test2></test2></genxml>");

            Console.WriteLine("nbi.XmlString: " + nbi.XmlString);
            Console.WriteLine("Export: " + Utils.ConvertToXmlItem(nbi));
        }
    }
}