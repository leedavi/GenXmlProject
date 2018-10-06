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


        public virtual NBrightInfo GetDataById(long itemId, string lang = "", string tableCode = "")
        {
            return _objProvider.GetDataById(itemId, lang,  tableCode);
        }

        public virtual List<NBrightInfo> GetList(NBrightSearchParams searchParams)
        {
            return _objProvider.GetList(searchParams);
        }

        public virtual void DeleteKey(long itemId, string tableCode = "")
        {
            _objProvider.DeleteKey(itemId, tableCode);
        }

        public virtual void DeleteTableCode(string tableCode = "")
        {
            _objProvider.DeleteTableCode(tableCode);
        }

        public virtual long Update(NBrightInfo nbInfo)
        {
            return _objProvider.Update(nbInfo);
        }

        public virtual long Update(NBrightRecord nbData)
        {
            return _objProvider.Update(nbData);
        }

        #endregion

    }
}