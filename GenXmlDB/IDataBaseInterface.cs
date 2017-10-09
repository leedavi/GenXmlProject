using System;
using System.Collections.Generic;
using System.Text;

namespace NBright.GenXmlDB
{
    public abstract class IDataBaseInterface
    {

        public abstract void Connect(string XmlConfig);

        public abstract void Disconnect();

        public abstract long Update(NBrightInfo nbInfo);

        public abstract long Update(NBrightData nbData);

        public abstract NBrightInfo GetDataById(string tableCode, long itemId, string lang = "");

        public abstract List<NBrightInfo> GetDataByFreeText(string tableCode, string text, string lang = "");

        public abstract List<NBrightInfo> GetListByUserId(string tableCode, long userId, string lang = "");

        public abstract List<NBrightInfo> GetListByKey(string tableCode, long key, string lang = "");

        public abstract List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId, string lang = "");

        public abstract List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId, string lang = "");

        public abstract List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId, string lang = "");

        public abstract List<NBrightInfo> GetListByPortalId(string tableCode, long portalId, string lang = "");

        public abstract void DeleteKey(string tableCode, long itemId);

        public abstract void DeleteTable(string tableCode);


    }
}
