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

namespace GenXmlSQLprovider
{
    public class SqlController : DataBaseContainer, IDataBaseInterface
    {

        protected string ConnectionString { get; set; }
        SqlConnection connection = null;


        public override void Connect(string XmlConfig)
        {
            var nbi = new NBrightInfo();
            nbi.XmlString = XmlConfig;

            foreach (var nod in nbi.XMLDoc.XPathSelectElements("genxml/dependancy/*"))
            {
               // AssemblyLoadContext.Default.LoadFromAssemblyPath(nod.Value);
            }

            ConnectionString = nbi.GetXmlProperty("genxml/provider/connectionstring");

            connection = new SqlConnection(this.ConnectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            CreateTable("TEST1");
        }

        #region "Base DB Methods"

        public override long Update(NBrightInfo nbInfo)
        {
            var lang = nbInfo.Lang;
            long rtnItemId = 0;
      
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
            //RunSqlFile(@"D:\Projects\GenXmlProject\GenXmlSQLprovider\sql\CreateIdxTable.sql", tableName);
            //RunSqlFile(@"D:\Projects\GenXmlProject\GenXmlSQLprovider\sql\CreateLangTable.sql", tableName);
        }


        #endregion

        private void RunSqlFile(string filepathname,string tableName,string databaseOwner = "", string objectQualifier = "")
        {
            if (System.IO.File.Exists(filepathname))
            {
                var sqlquery = System.IO.File.ReadAllText(@"D:\Projects\GenXmlProject\GenXmlSQLprovider\sql\CreateDataTable.sql");
                sqlquery = sqlquery.Replace("{TableName}", tableName);
                sqlquery = sqlquery.Replace("{databaseOwner}", "");
                sqlquery = sqlquery.Replace("{objectQualifier}", "");
                SqlCommand sqlQuery = new SqlCommand(sqlquery, connection);
                sqlQuery.ExecuteNonQuery();
            }

        }





    }



    public class BaseDataAccess
    {
        protected string ConnectionString { get; set; }

        public BaseDataAccess()
        {
        }

        public BaseDataAccess(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(this.ConnectionString);
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
                using (SqlConnection connection = this.GetConnection())
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

        protected object ExecuteScalar(string procedureName, List<SqlParameter> parameters)
        {
            object returnValue = null;

            try
            {
                using (DbConnection connection = this.GetConnection())
                {
                    DbCommand cmd = this.GetCommand(connection, procedureName, CommandType.StoredProcedure);

                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

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
                DbConnection connection = this.GetConnection();
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
    }

}
