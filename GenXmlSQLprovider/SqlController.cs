using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using NBright.GenXmlDB;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using System.Data.SqlTypes;
using System.Xml;

namespace GenXmlSQLprovider
{
    public class SqlController : DataBaseContainer, IDataBaseInterface
    {

        protected string ConnectionString { get; set; }

        protected string SqlTableName { get; set; }
        protected string databaseOwner { get; set; }
        protected string objectQualifier { get; set; }

        private BaseDataAccess BaseDA = null;


        public override void Connect(string XmlConfig)
        {
                        var nbi = new NBrightInfo();
            nbi.XmlString = XmlConfig;

            foreach (var nod in nbi.XMLDoc.XPathSelectElements("genxml/dependancy/*"))
            {
               // AssemblyLoadContext.Default.LoadFromAssemblyPath(nod.Value);
            }

            ConnectionString = nbi.GetXmlProperty("genxml/provider/connectionstring");

            SqlTableName = nbi.GetXmlProperty("genxml/provider/sqltablename");
            objectQualifier = nbi.GetXmlProperty("genxml/provider/objectQualifier");
            databaseOwner = nbi.GetXmlProperty("genxml/provider/databaseOwner");


            BaseDA = new BaseDataAccess(this.ConnectionString);

            CreateTable(SqlTableName);
        }

        #region "Base DB Methods"

        public override long Update(NBrightInfo nbInfo)
        {
            var lang = nbInfo.Lang;
            long rtnItemId = 0;

            var cmdText = "{databaseOwner}{objectQualifier}{TableName}_Update ";
            cmdText = replaceSqlTokens(cmdText);

            SqlCommand command = new SqlCommand(cmdText, BaseDA.connection);

            SqlXml newXml = new SqlXml(nbInfo.XMLDoc.CreateReader());

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.Int)).Value = nbInfo.ItemId;

            command.Parameters.Add(new SqlParameter("@PortalId", SqlDbType.Int)).Value = nbInfo.PortalId;
            command.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int)).Value = nbInfo.ModuleId;
            command.Parameters.Add(new SqlParameter("@TableCode", SqlDbType.NVarChar)).Value = nbInfo.TableCode;
            command.Parameters.Add(new SqlParameter("@KeyData", SqlDbType.NVarChar)).Value = nbInfo.KeyData;
            command.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime)).Value = nbInfo.ModifiedDate;
            command.Parameters.Add(new SqlParameter("@TextData", SqlDbType.NVarChar)).Value = nbInfo.TextData;
            command.Parameters.Add(new SqlParameter("@XrefItemId", SqlDbType.Int)).Value = nbInfo.XrefItemId;
            command.Parameters.Add(new SqlParameter("@ParentItemId", SqlDbType.Int)).Value = nbInfo.ParentItemId;
            command.Parameters.Add(new SqlParameter("@XmlString", SqlDbType.Xml)).Value = newXml;
            command.Parameters.Add(new SqlParameter("@Lang", SqlDbType.NVarChar)).Value = nbInfo.Lang;
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int)).Value = nbInfo.UserId;
            command.Parameters.Add(new SqlParameter("@LegacyItemId", SqlDbType.Int)).Value = nbInfo.LegacyItemId;

            command.ExecuteNonQuery();

            //var rtnObj = BaseDA.ExecuteScalar("SELECT count(Itemid) FROM [GenXmlDB].[dbo].[XMLDATA]");


            return rtnItemId;
        }

        public override long Update(NBrightData nbData)
        {
            try
            {

                return nbData.ItemId;
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
                return new NBrightInfo();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return null;
        }

        public override NBrightInfo GetDataById(string tableCode, long itemId, string lang = "")
        {
            try
            {
                return new NBrightInfo();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return null;
        }

        public override List<NBrightInfo> GetDataByFreeText(string tableCode, string text, string lang = "")
        {
            var rtnList = new List<NBrightInfo>();
            try
            {                

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rtnList;
        }

        public override List<NBrightInfo> GetListByUserId(string tableCode, long userId, string lang = "")
        {
            return new List<NBrightInfo>();
        }

        public override List<NBrightInfo> GetListByKey(string tableCode, long key, string lang = "")
        {
            return new List<NBrightInfo>();
        }

        public override List<NBrightInfo> GetListByParentItemId(string tableCode, long parentItemId, string lang = "")
        {
            return new List<NBrightInfo>();
        }

        public override List<NBrightInfo> GetListByXrefItemId(string tableCode, long xrefItemId, string lang = "")
        {
            return new List<NBrightInfo>();
        }

        public override List<NBrightInfo> GetListByModuleId(string tableCode, long moduleId, string lang = "")
        {
            return new List<NBrightInfo>();
        }

        public override List<NBrightInfo> GetListByPortalId(string tableCode, long portalId)
        {
            return GetListByPortalId(tableCode, portalId);
        }
        public override List<NBrightInfo> GetListByPortalId(string tableCode, long portalId, string lang = "")
        {
            return new List<NBrightInfo>();
        }

        private List<NBrightInfo> GetDataList(string tableCode,byte idxType, long keyValue, string lang = "")
        {
            var rtnList = new List<NBrightInfo>();
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rtnList;
        }


        public override void DeleteKey(string tableCode, long itemId)
        {
        }

        public override void DeleteTable(string tableCode)
        {

        }

        private void CreateTable(string tableName)
        {
            RunSqlFile(@"D:\Projects\GenXmlProject\GenXmlSQLprovider\sql\CreateDataTable.sql", tableName);
            //RunSqlFile(@"D:\Projects\GenXmlProject\GenXmlSQLprovider\sql\CreateTableFunctions.sql", tableName);
            //RunSqlFile(@"D:\Projects\GenXmlProject\GenXmlSQLprovider\sql\CreateLangTable.sql", tableName);
        }


        #endregion

        private void RunSqlFile(string filepathname,string tableName)
        {
            if (System.IO.File.Exists(filepathname))
            {
                var sqlquery = System.IO.File.ReadAllText(filepathname);
                sqlquery = sqlquery.Replace("{TableName}", tableName);
                sqlquery = sqlquery.Replace("{databaseOwner}", databaseOwner + ".");  // we need this for the merge functioons to work.
                sqlquery = sqlquery.Replace("{objectQualifier}", objectQualifier);

                var cmdList = sqlquery.Split("{GO}");
                foreach (var s in cmdList)
                {
                    if (s.Trim(' ') != "")
                    {
                        SqlCommand sqlQuery = new SqlCommand(s, BaseDA.connection);
                        sqlQuery.ExecuteNonQuery();
                    }
                }
            }

        }


        private string replaceSqlTokens(string sqlquery)
        {
            sqlquery = sqlquery.Replace("{TableName}", SqlTableName);
            sqlquery = sqlquery.Replace("{databaseOwner}", databaseOwner + ".");  // we need this for the merge functioons to work.
            sqlquery = sqlquery.Replace("{objectQualifier}", objectQualifier);
            return sqlquery;
        }



    }



    public class BaseDataAccess
    {
        protected string ConnectionString { get; set; }
        public SqlConnection connection = null;

        public BaseDataAccess(string connectionString)
        {
            this.ConnectionString = connectionString;
            GetConnection();
        }

        private SqlConnection GetConnection()
        {
            connection = new SqlConnection(this.ConnectionString);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }

        protected DbCommand GetCommand(DbConnection connection, string commandText, CommandType commandType)
        {
            SqlCommand command = new SqlCommand(commandText, connection as SqlConnection);
            command.CommandType = commandType;
            return command;
        }

        protected SqlParameter GetParameter(string parameter, object value)
        {
            SqlParameter parameterObject = new SqlParameter(parameter, value != null ? value : DBNull.Value);
            parameterObject.Direction = ParameterDirection.Input;
            return parameterObject;
        }

        protected SqlParameter GetParameterOut(string parameter, SqlDbType type, object value = null, ParameterDirection parameterDirection = ParameterDirection.InputOutput)
        {
            SqlParameter parameterObject = new SqlParameter(parameter, type); ;

            if (type == SqlDbType.NVarChar || type == SqlDbType.VarChar || type == SqlDbType.NText || type == SqlDbType.Text)
            {
                parameterObject.Size = -1;
            }

            parameterObject.Direction = parameterDirection;

            if (value != null)
            {
                parameterObject.Value = value;
            }
            else
            {
                parameterObject.Value = DBNull.Value;
            }

            return parameterObject;
        }

        protected int ExecuteNonQuery(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            int returnValue = -1;

            try
            {
                using (connection)
                {
                    DbCommand cmd = this.GetCommand(connection, procedureName, commandType);

                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    returnValue = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //LogException("Failed to ExecuteNonQuery for " + procedureName, ex, parameters);  
                Console.WriteLine(ex.Message);
                throw;
            }

            return returnValue;
        }

        public object ExecuteScalar(string commandText)
        {
            object returnValue = null;

            try
            {
                using (connection)
                {
                    SqlCommand cmd = new SqlCommand(commandText, connection);
                    returnValue = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //LogException("Failed to ExecuteScalar for " + procedureName, ex, parameters);  
                throw;
            }

            return returnValue;
        }

        protected DbDataReader GetDataReader(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            DbDataReader ds;

            try
            {
                using (connection)
                {
                    DbCommand cmd = this.GetCommand(connection, procedureName, commandType);
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //LogException("Failed to GetDataReader for " + procedureName, ex, parameters);  
                throw;
            }

            return ds;
        }

        public List<SqlParameter> AssignedSqlParams(NBrightInfo info)
        {
            var param = new List<SqlParameter>();
            foreach (var prop in info.GetType().GetProperties())
            {
                var sqlparam = new SqlParameter();
                sqlparam.TypeName = prop.Name;
                sqlparam.Value = prop.GetValue(info, null);
                param.Add(sqlparam);
            }
            return param;

        }
    }

}
