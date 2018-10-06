using System.Collections.Generic;

namespace NBright.GenXmlDB
{
    public interface IDataBaseInterface
    {

        void Connect(string XmlConfig);

        void Disconnect();

        long Update(NBrightInfo nbInfo);

        long Update(NBrightRecord nbData);

        NBrightInfo GetDataById(long itemId, string lang = "", string tableCode = "");

        List<NBrightInfo> GetList(NBrightSearchParams searchParams);

        void DeleteKey(long itemId, string tableCode = "");

        void DeleteTableCode(string tableCode = "");

    }
}
