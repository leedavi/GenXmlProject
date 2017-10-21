using System.Collections.Generic;

namespace NBright.GenXmlDB
{
    public interface IDataBaseInterface
    {

        void Connect(string XmlConfig);

        void Disconnect();

        long Update(NBrightInfo nbInfo);

        long Update(NBrightData nbData);

        NBrightInfo GetDataById(string tableCode, long itemId, string lang);

        List<NBrightInfo> GetDataByFreeText(string tableCode, string text, string lang);

        List<NBrightInfo> GetListByUserId(string tableCode, long userId, string lang);

        List<NBrightInfo> GetListByKey(string tableCode, long key, string lang);

        List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId, string lang);

        List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId, string lang);

        List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId, string lang);

        List<NBrightInfo> GetListByPortalId(string tableCode, long portalId);
        List<NBrightInfo> GetListByPortalId(string tableCode, long portalId, string lang);

        void DeleteKey(string tableCode, long itemId);

        void DeleteTable(string tableCode);


    }
}
