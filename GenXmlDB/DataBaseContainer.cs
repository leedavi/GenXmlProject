using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Reflection;
using System.Xml.XPath;
using System.Linq;

namespace NBright.GenXmlDB
{
    public class DataBaseContainer : IDataBaseInterface
    {

        private IDataBaseInterface _objProvider = null;

        public override void Connect(string XmlConfig)
        {
            var nbi = new NBrightInfo();
            nbi.XmlString = XmlConfig;

            foreach (var nod in nbi.XMLDoc.XPathSelectElements("genxml/dependancy/*"))
            {
                //AssemblyLoadContext.Default.LoadFromAssemblyPath(nod.Value);
            }            

            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(nbi.GetXmlProperty("genxml/provider/assembly"));

            var myType = myAssembly.GetType(nbi.GetXmlProperty("genxml/provider/namespaceclass"));
            _objProvider = (IDataBaseInterface)Activator.CreateInstance(myType);

            _objProvider.Connect(XmlConfig);

        }


        public override void Disconnect()
        {
            _objProvider.Disconnect();
        }

        #region "DB Interface Methods"

        public override void DeleteKey(string tableCode, long itemId)
        {
            _objProvider.DeleteKey(tableCode, itemId);
        }

        public override void DeleteTable(string tableCode)
        {
            _objProvider.DeleteTable(tableCode);
        }

        public override List<NBrightInfo> GetDataByFreeText(string tableCode, string text, string lang = "")
        {
            return _objProvider.GetDataByFreeText(tableCode, text, lang);
        }

        public override NBrightInfo GetDataById(string tableCode, long itemId, string lang = "")
        {
            return _objProvider.GetDataById(tableCode, itemId, lang);
        }

        public override List<NBrightInfo> GetListByKey(string tableCode, long key, string lang = "")
        {
            return _objProvider.GetListByKey(tableCode, key, lang);
        }

        public override List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId, string lang = "")
        {
            return _objProvider.GetListByModuleId(tableCode, moduleId, lang);
        }

        public override List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId, string lang = "")
        {
            return _objProvider.GetListByParentItemId(tableCode, parentItemId, lang);
        }

        public override List<NBrightInfo> GetListByPortalId(string tableCode, long portalId, string lang = "")
        {
            return _objProvider.GetListByPortalId(tableCode, portalId, lang);
        }

        public override List<NBrightInfo> GetListByUserId(string tableCode, long userId, string lang = "")
        {
            return _objProvider.GetListByUserId(tableCode, userId, lang);
        }

        public override List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId, string lang = "")
        {
            return _objProvider.GetListByXrefItemId(tableCode, xrefItemId, lang);
        }

        public override long Update(NBrightInfo nbInfo)
        {
            return _objProvider.Update(nbInfo);
        }

        public override long Update(NBrightData nbData)
        {
            return _objProvider.Update(nbData);
        }

        #endregion

    }



}
