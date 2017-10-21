using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Xml.XPath;

namespace NBright.GenXmlDB
{
    public class DataBaseContainer : IDataBaseInterface
    {

        private IDataBaseInterface _objProvider = null;

        public virtual void Connect(string XmlConfig)
        {
            NBrightInfo nbi = new NBrightInfo();
            nbi.XmlString = XmlConfig;

            foreach (var nod in nbi.XMLDoc.XPathSelectElements("genxml/dependancy/*"))
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(nod.Value);
            }

            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(nbi.GetXmlProperty("genxml/provider/assembly"));

            var myType = myAssembly.GetType(nbi.GetXmlProperty("genxml/provider/namespaceclass"));
            _objProvider = (IDataBaseInterface)Activator.CreateInstance(myType);

            _objProvider.Connect(XmlConfig);

        }


        public virtual void Disconnect()
        {
            if(_objProvider != null)
            {
                _objProvider.Disconnect();
            }
        }

        #region "DB Interface Methods"

        public virtual void DeleteKey(string tableCode, long itemId)
        {
            _objProvider.DeleteKey(tableCode, itemId);
        }

        public virtual void DeleteTable(string tableCode)
        {
            _objProvider.DeleteTable(tableCode);
        }

        public List<NBrightInfo> GetDataByFreeText(string tableCode, string text)
        {
            return GetDataByFreeText(tableCode, text, string.Empty);
        }

        public virtual List<NBrightInfo> GetDataByFreeText(string tableCode, string text, string lang = "")
        {
            return _objProvider.GetDataByFreeText(tableCode, text, lang);
        }

        public virtual NBrightInfo GetDataById(string tableCode, long itemId)
        {
            return GetDataById(tableCode, itemId, string.Empty);
        }

        public virtual NBrightInfo GetDataById(string tableCode, long itemId, string lang = "")
        {
            return _objProvider.GetDataById(tableCode, itemId, lang);
        }

        public virtual List<NBrightInfo> GetListByKey(string tableCode, long key)
        {
            return GetListByKey(tableCode, key, string.Empty);
        }

        public virtual List<NBrightInfo> GetListByKey(string tableCode, long key, string lang = "")
        {
            return _objProvider.GetListByKey(tableCode, key, lang);
        }

        public virtual List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId)
        {
            return GetListByModuleId(tableCode, moduleId, string.Empty);
        }

        public virtual List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId, string lang = "")
        {
            return _objProvider.GetListByModuleId(tableCode, moduleId, lang);
        }

        public virtual List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId)
        {
            return GetListByParentItemId(tableCode, parentItemId, string.Empty);
        }

        public virtual List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId, string lang = "")
        {
            return _objProvider.GetListByParentItemId(tableCode, parentItemId, lang);
        }

        public virtual List<NBrightInfo> GetListByPortalId(string tableCode, long portalId)
        {
            return GetListByPortalId(tableCode, portalId, string.Empty);
        }

        public virtual List<NBrightInfo> GetListByPortalId(string tableCode, long portalId, string lang = "")
        {
            return _objProvider.GetListByPortalId(tableCode, portalId, lang);
        }

        public virtual List<NBrightInfo> GetListByUserId(string tableCode, long userId)
        {
            return GetListByUserId(tableCode, userId, string.Empty);
        }

        public virtual List<NBrightInfo> GetListByUserId(string tableCode, long userId, string lang = "")
        {
            return _objProvider.GetListByUserId(tableCode, userId, lang);
        }

        public virtual List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId)
        {
            return GetListByXrefItemId(tableCode, xrefItemId, string.Empty);
        }

        public virtual List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId, string lang = "")
        {
            return _objProvider.GetListByXrefItemId(tableCode, xrefItemId, lang);
        }

        public virtual long Update(NBrightInfo nbInfo)
        {
            return _objProvider.Update(nbInfo);
        }

        public virtual long Update(NBrightData nbData)
        {
            return _objProvider.Update(nbData);
        }

        #endregion

    }
}