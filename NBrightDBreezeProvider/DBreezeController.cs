using System;
using System.Collections.Generic;
using DBreeze;
using DBreeze.Utils;
using System.Linq;
using System.Xml.XPath;
using NBright.GenXmlDB;

namespace GenXmlDBreezeProvider
{
    public class DBreezeController : DataBaseContainer, IDataBaseInterface
    {
        DBreezeEngine engine = null;

        static class DBreezeIdx
        {
            public const byte idx_ItemId = 1;
            public const byte idx_PortalId = 2;
            public const byte idx_ModuleId = 3;
            public const byte idx_Key = 4;
            public const byte idx_XrefItemId = 5;
            public const byte idx_ParentItemId = 6;
            public const byte idx_Lang = 7;
            public const byte idx_UserId = 8;

            public const byte idx_ParentItemId_Lang = 20;

        }


        public override void Connect(string XmlConfig)
        {
            NBrightInfo nbi = new NBrightInfo();
            nbi.XmlString = XmlConfig;

            engine = new DBreezeEngine(nbi.GetXmlProperty("genxml/provider/dbpath"));

            //Setting up NetJSON serializer (from NuGet) to be used by DBreeze
            DBreeze.Utils.CustomSerializator.ByteArraySerializator = (object o) => { return NetJSON.NetJSON.Serialize(o).To_UTF8Bytes(); };
            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t) => { return NetJSON.NetJSON.Deserialize(t, bt.UTF8_GetString()); };
        }

        public override void Disconnect()
        {
            engine.Dispose();
        }

        #region "Base DB Methods"

        public override long Update(NBrightInfo nbInfo)
        {
            var lang = nbInfo.Lang;
            long rtnItemId = 0;
            if (lang == "")
            {
                // no lang record required
                var nbd = Utils.ConvertToNBrightData(nbInfo);
                rtnItemId = Update(nbd);
            }
            else
            {
                // create base and lang records
                var baseXml = nbInfo.XmlString; // make sure we have the orginal XML before changing anything.

                nbInfo.XMLDoc.XPathSelectElement("genxml/lang").Remove();
                var nbd = Utils.ConvertToNBrightData(nbInfo);
                nbd.Lang = "";
                nbd.ParentItemId = 0;
                var parentItemId = Update(nbd);
                
                nbInfo.XmlString = baseXml;
                var nodLang = nbInfo.XMLDoc.XPathSelectElement("genxml/lang/genxml");
                nbInfo.XmlString = nodLang.ToString();
                var nbdl = Utils.ConvertToNBrightData(nbInfo);
                nbdl.ParentItemId = parentItemId;
                var recordexists = GetDataByParentIdLang(nbInfo.TableCode + "LANG", nbInfo.ItemId, lang);
                nbdl.ItemId = 0;
                if (recordexists != null)
                {
                    nbdl.ItemId = recordexists.ItemId;
                }
                Update(nbdl);
                rtnItemId = parentItemId;
            }

            return rtnItemId;
        }

        public override long Update(NBrightData nbData)
        {
            try
            {

                using (var t = engine.GetTransaction())
                {
                    var tableCode = nbData.TableCode;

                    var indexId = nbData.ItemId;
                    if (nbData.Lang != "")
                    {
                        tableCode += "LANG"; // use language tablecode if we have a language record.
                        nbData.TableCode = tableCode;
                        indexId = nbData.ParentItemId;
                    }

                    //Documentation https://goo.gl/Kwm9aq
                    //This line with a list of tables we need in case if we modify morethen 1 table inside of transaction
                    t.SynchronizeTables(tableCode);

                    bool newEntity = nbData.ItemId == 0;
                    if (newEntity) nbData.ItemId = t.ObjectGetNewIdentity<long>(tableCode);


                    // build Indexes
                    var indexes = new List<DBreeze.Objects.DBreezeIndex>();
                    var idIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_ItemId, nbData.ItemId) { PrimaryIndex = true };
                    indexes.Add(idIndex);

                    if (nbData.ParentItemId > 0)
                    {
                        var parentidIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_ParentItemId, nbData.ParentItemId);
                        indexes.Add(parentidIndex);
                        if (nbData.Lang != null && nbData.Lang != "")
                        {
                            parentidIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_ParentItemId_Lang, nbData.ParentItemId.ToBytes(nbData.Lang));
                            indexes.Add(parentidIndex);
                        }
                    }
                    if (nbData.XrefItemId > 0)
                    {
                        var XrefItemIdIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_XrefItemId, nbData.XrefItemId);
                        indexes.Add(XrefItemIdIndex);
                    }
                    if (nbData.PortalId > 0)
                    {
                        var PortalIdIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_PortalId, nbData.PortalId);
                        indexes.Add(PortalIdIndex);
                    }
                    if (nbData.ModuleId > 0)
                    {
                        var ModuleIdIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_ModuleId, nbData.ModuleId);
                        indexes.Add(ModuleIdIndex);
                    }
                    if (nbData.UserId > 0)
                    {
                        var UserIdIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_UserId, nbData.UserId);
                        indexes.Add(UserIdIndex);
                    }
                    if (nbData.Key != null && nbData.Key != "")
                    {
                        var KeyIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_Key, nbData.Key);
                        indexes.Add(KeyIndex);
                    }
                    if (nbData.Lang != null && nbData.Lang != "")
                    {
                        var LangIndex = new DBreeze.Objects.DBreezeIndex(DBreezeIdx.idx_Lang, nbData.Lang);
                        indexes.Add(LangIndex);
                    }

                    //Documentation https://goo.gl/YtWnAJ
                    t.ObjectInsert(tableCode, new DBreeze.Objects.DBreezeObject <NBrightData>
                    {
                        NewEntity = newEntity,
                        Entity = nbData,
                        Indexes = indexes
                    }, false);

                    //Documentation https://goo.gl/s8vtRG
                    //Setting text search index. We will store text-search 
                    //indexes concerning customers in table "TS_".
                    //Second parameter is a reference to the customer ID.
                    if (nbData?.FreeTextIndexData != "")
                    {
                        t.TextInsert("TS_" + nbData.TableCode, indexId.ToBytes(), nbData.FreeTextIndexData);
                    }


                    //Committing entry
                    t.Commit();
                    return nbData.ItemId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private NBrightInfo GetDataByParentIdLang(string tableCode, long parentItemId, string lang)
        {
            try
            {
                using (var t = engine.GetTransaction())
                {
                    foreach (var el in t.SelectForwardFromTo<byte[], byte[]>(tableCode, DBreezeIdx.idx_ParentItemId_Lang.ToIndex(parentItemId,lang, long.MinValue), true, DBreezeIdx.idx_ParentItemId_Lang.ToIndex(parentItemId,lang, long.MaxValue), true))
                    {
                        return new NBrightInfo(el.ObjectGet<NBrightData>().Entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public override NBrightInfo GetDataById(string tableCode, long itemId, string lang = "")
        {
            try
            {
                using (var t = engine.GetTransaction())
                {
                    var obj = t.Select<byte[], byte[]>(tableCode, DBreezeIdx.idx_ItemId.ToIndex(itemId)).ObjectGet<NBrightData>();
                    if (obj != null)
                    {
                        if (lang == "")
                        {
                            return new NBrightInfo(obj.Entity);
                        }
                        else
                        {
                            var nbi1 = new NBrightInfo(obj.Entity);
                            foreach (var el2 in t.SelectForwardFromTo<byte[], byte[]>(tableCode + "LANG", DBreezeIdx.idx_ParentItemId_Lang.ToIndex(nbi1.ItemId, lang, long.MinValue), true, DBreezeIdx.idx_ParentItemId_Lang.ToIndex(nbi1.ItemId, lang, long.MaxValue), true))
                            {
                                var nbi2 = new NBrightInfo(el2.ObjectGet<NBrightData>().Entity);
                                nbi1.AddXmlLang(nbi2.XmlString);
                                nbi1.Lang = nbi2.Lang;
                            }
                            return nbi1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public override List<NBrightInfo> GetDataByFreeText(string tableCode, string text, string lang = "")
        {
            var rtnList = new List<NBrightInfo>();
            try
            {                
                using (var t = engine.GetTransaction())
                {
                    foreach (var doc in t.TextSearch("TS_" + tableCode).BlockAnd(text).GetDocumentIDs())
                    {
                        var obj = t.Select<byte[], byte[]>(tableCode, DBreezeIdx.idx_ItemId.ToIndex(doc)).ObjectGet<NBrightData>();
                        if (lang == "")
                        {
                            if (obj != null) rtnList.Add(new NBrightInfo(obj.Entity));
                        }
                        else
                        {
                            var nbi1 = new NBrightInfo(obj.Entity);
                            foreach (var el2 in t.SelectForwardFromTo<byte[], byte[]>(tableCode + "LANG", DBreezeIdx.idx_ParentItemId_Lang.ToIndex(nbi1.ItemId, lang, long.MinValue), true, DBreezeIdx.idx_ParentItemId_Lang.ToIndex(nbi1.ItemId, lang, long.MaxValue), true))
                            {
                                var nbi2 = new NBrightInfo(el2.ObjectGet<NBrightData>().Entity);
                                nbi1.AddXmlLang(nbi2.XmlString);
                                nbi1.Lang = nbi2.Lang;
                            }
                            rtnList.Add(nbi1);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rtnList;
        }

        public override List<NBrightInfo> GetListByUserId(string tableCode, long userId, string lang = "")
        {
            return GetDataList(tableCode, DBreezeIdx.idx_UserId, userId, lang);
        }

        public override List<NBrightInfo> GetListByKey(string tableCode, long key, string lang = "")
        {
            return GetDataList(tableCode, DBreezeIdx.idx_Key, key, lang);
        }

        public override List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId, string lang = "")
        {
            return GetDataList(tableCode, DBreezeIdx.idx_ParentItemId, parentItemId, lang);
        }

        public override List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId, string lang = "")
        {
            return GetDataList(tableCode, DBreezeIdx.idx_XrefItemId, xrefItemId, lang);
        }

        public override List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId, string lang = "")
        {
            return GetDataList(tableCode, DBreezeIdx.idx_ModuleId, moduleId, lang);
        }

        public override List<NBrightInfo> GetListByPortalId(string tableCode, long portalId)
        {
            return GetListByPortalId(tableCode, portalId);
        }

        public override List<NBrightInfo> GetListByPortalId(string tableCode, long portalId, string lang = "")
        {
            return GetDataList(tableCode, DBreezeIdx.idx_PortalId, portalId, lang);
        }

        private List<NBrightInfo> GetDataList(string tableCode,byte idxType, long keyValue, string lang = "")
        {
            var rtnList = new List<NBrightInfo>();
            try
            {
                using (var t = engine.GetTransaction())
                {

                    if (lang == "")
                    {
                        foreach (var el in t.SelectForwardFromTo<byte[], byte[]>(tableCode, idxType.ToIndex(keyValue, long.MinValue), true, idxType.ToIndex(keyValue, long.MaxValue), true))
                        {
                            var nbd = el.ObjectGet<NBrightData>().Entity;
                            if (nbd != null) rtnList.Add(new NBrightInfo(nbd));
                        }
                    }
                    else
                    {

                        foreach (var el in t.SelectForwardFromTo<byte[], byte[]>(tableCode, idxType.ToIndex(keyValue, long.MinValue), true, idxType.ToIndex(keyValue, long.MaxValue), true))
                        {
                            var nbi1 = new NBrightInfo(el.ObjectGet<NBrightData>().Entity);
                            foreach (var el2 in t.SelectForwardFromTo<byte[], byte[]>(tableCode + "LANG", DBreezeIdx.idx_ParentItemId_Lang.ToIndex(nbi1.ItemId, lang, long.MinValue), true, DBreezeIdx.idx_ParentItemId_Lang.ToIndex(nbi1.ItemId, lang, long.MaxValue), true))
                            {
                                var nbi2 = new NBrightInfo(el2.ObjectGet<NBrightData>().Entity);
                                nbi1.AddXmlLang(nbi2.XmlString);
                                nbi1.Lang = nbi2.Lang;
                            }

                            rtnList.Add(nbi1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rtnList;
        }


        public override void DeleteKey(string tableCode, long itemId)
        {
            using (var t = engine.GetTransaction())
            {

                t.ObjectRemove(tableCode, DBreezeIdx.idx_ItemId.ToIndex(itemId));
                t.ObjectRemove("TS_" + tableCode, DBreezeIdx.idx_ItemId.ToIndex(itemId));

                foreach (var el in t.SelectForwardFromTo<byte[], byte[]>(tableCode + "LANG", DBreezeIdx.idx_ParentItemId.ToIndex(itemId, long.MinValue), true, DBreezeIdx.idx_ParentItemId.ToIndex(itemId, long.MaxValue), true))
                {
                    t.ObjectRemove(tableCode + "LANG", DBreezeIdx.idx_ItemId.ToIndex((long)el.ObjectGet<NBrightData>().Entity.ItemId));
                }

                t.Commit();
            }
        }

        public override void DeleteTable(string tableCode)
        {
            using (var t = engine.GetTransaction())
            {
                t.RemoveAllKeys(tableCode, true);
                t.RemoveAllKeys(tableCode + "LANG", true);
                t.RemoveAllKeys("TS_" + tableCode, true);
            }
        }
        #endregion
    }
}