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
            try
            {


                var lang = nbInfo.Lang;
                long rtnItemId = 0;
                if (lang == "")
                {
                    // no lang record required
                    var nbd = Utils.ConvertToNBrightRecord(nbInfo);
                    rtnItemId = Update(nbd);
                }
                else
                {
                    // create base and lang records
                    var baseXml = nbInfo.XmlString; // make sure we have the orginal XML before changing anything.

                    nbInfo.XMLDoc.XPathSelectElement("genxml/lang").Remove();
                    var nbd = Utils.ConvertToNBrightRecord(nbInfo);
                    nbd.Lang = "";
                    nbd.ParentItemId = 0;
                    var parentItemId = Update(nbd);

                    nbInfo.XmlString = baseXml;
                    var nodLang = nbInfo.XMLDoc.XPathSelectElement("genxml/lang/genxml");
                    nbInfo.XmlString = nodLang.ToString();
                    var nbdl = Utils.ConvertToNBrightRecord(nbInfo);
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override long Update(NBrightRecord nbData)
        {
            try
            {
                var nbInfo = new NBrightInfo(nbData);

                int rtnItemId = 0;

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

                var rtbObj = command.ExecuteScalar();

                rtnItemId = (int)rtbObj;

                return rtnItemId;
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
                var cmdText = "{databaseOwner}{objectQualifier}{TableName}_GetDataLang ";
                cmdText = replaceSqlTokens(cmdText);

                SqlCommand command = new SqlCommand(cmdText, BaseDA.connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ParentItemId", SqlDbType.Int)).Value = parentItemId;
                command.Parameters.Add(new SqlParameter("@Lang", SqlDbType.NVarChar)).Value = lang;

                return ReadSqlCommand(command);               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override NBrightInfo GetDataById(long itemId, string lang = "", string tableCode = "")
        {
            try
            {
                var cmdText = "{databaseOwner}{objectQualifier}{TableName}_Get ";
                cmdText = replaceSqlTokens(cmdText);

                SqlCommand command = new SqlCommand(cmdText, BaseDA.connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.Int)).Value = itemId;
                command.Parameters.Add(new SqlParameter("@Lang", SqlDbType.NVarChar)).Value = lang;

                return ReadSqlCommand(command);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public override List<NBrightInfo> GetList(NBrightSearchParams searchParams)
        {

            var cmdText = "{databaseOwner}{objectQualifier}{TableName}_GetList ";
            cmdText = replaceSqlTokens(cmdText);

            SqlCommand command = new SqlCommand(cmdText, BaseDA.connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PortalId", SqlDbType.Int)).Value = searchParams.PortalId;
            command.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int)).Value = searchParams.ModuleId;
            command.Parameters.Add(new SqlParameter("@TableCode", SqlDbType.NVarChar)).Value = searchParams.TableCode;
            command.Parameters.Add(new SqlParameter("@OrderBy", SqlDbType.NVarChar)).Value = searchParams.SqlOrderBy;
            command.Parameters.Add(new SqlParameter("@ReturnLimit", SqlDbType.Int)).Value = searchParams.ReturnLimit;
            command.Parameters.Add(new SqlParameter("@pageNum", SqlDbType.Int)).Value = searchParams.pageNum;
            command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int)).Value = searchParams.pageSize;
            command.Parameters.Add(new SqlParameter("@RecordCount", SqlDbType.Int)).Value = searchParams.RecordCount;
            command.Parameters.Add(new SqlParameter("@Lang", SqlDbType.NVarChar)).Value = searchParams.Lang;


            switch (searchParams.SearchType.ToLower())
            {
                case "getlist":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = searchParams.SqlFilter;
                    break;
                case "byuserid":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = " and nb1.userid = " + searchParams.UserId + " ";
                    break;
                case "bykey":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = " and nb1.keydata = '" + searchParams.KeyData + "' ";
                    break;
                case "byparentitemid":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = " and nb1.ParentItemId = " + searchParams.ParentItemId + " ";
                    break;
                case "byxrefitemid":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = " and nb1.xrefitemid = " + searchParams.XrefItemId + " ";
                    break;
                case "bymoduleid":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = " and nb1.moduleid = " + searchParams.ModuleId  + " ";
                    break;
                case "byportalid":
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = " and nb1.PortalId = " + searchParams.PortalId + " ";
                    break;

                case "ALLEXAMPLE":
                    command.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.Int)).Value = searchParams.ItemId;
                    command.Parameters.Add(new SqlParameter("@PortalId", SqlDbType.Int)).Value = searchParams.PortalId;
                    command.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int)).Value = searchParams.ModuleId;
                    command.Parameters.Add(new SqlParameter("@TableCode", SqlDbType.NVarChar)).Value = searchParams.TableCode;
                    command.Parameters.Add(new SqlParameter("@KeyData", SqlDbType.NVarChar)).Value = searchParams.KeyData;
                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime)).Value = searchParams.StartDate;
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime)).Value = searchParams.EndDate;
                    command.Parameters.Add(new SqlParameter("@XrefItemId", SqlDbType.Int)).Value = searchParams.XrefItemId;
                    command.Parameters.Add(new SqlParameter("@ParentItemId", SqlDbType.Int)).Value = searchParams.ParentItemId;
                    command.Parameters.Add(new SqlParameter("@Lang", SqlDbType.NVarChar)).Value = searchParams.Lang;
                    command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int)).Value = searchParams.UserId;
                    command.Parameters.Add(new SqlParameter("@Filter", SqlDbType.NVarChar)).Value = searchParams.SqlFilter;
                    command.Parameters.Add(new SqlParameter("@OrderBy", SqlDbType.NVarChar)).Value = searchParams.SqlOrderBy;
                    command.Parameters.Add(new SqlParameter("@SearchType", SqlDbType.NVarChar)).Value = searchParams.SearchType;
                    command.Parameters.Add(new SqlParameter("@ReturnLimit", SqlDbType.Int)).Value = searchParams.ReturnLimit;
                    command.Parameters.Add(new SqlParameter("@pageNum", SqlDbType.Int)).Value = searchParams.pageNum;
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int)).Value = searchParams.pageSize;
                    command.Parameters.Add(new SqlParameter("@RecordCount", SqlDbType.Int)).Value = searchParams.RecordCount;

                    foreach (var ed in searchParams.ExtraData)
                    {
                        command.Parameters.Add(new SqlParameter("@" + ed.Key, SqlDbType.NVarChar)).Value = ed.Value;
                    }

                    break;
            }

            if (cmdText == "")
            {
                var rtnErr = new List<NBrightInfo>();
                var nbi = new NBrightInfo();
                nbi.XmlString = "<genxml><error>No Command Text Found. SearchType not set to a valid value</error></genxml>";
                rtnErr.Add(nbi);
                return rtnErr;
            }
            else
            {
                return ReadSqlListCommand(command);

            }

        }


        public override void DeleteKey(long itemId, string tableCode = "")
        {
            var cmdText = "{databaseOwner}{objectQualifier}{TableName}_DeleteKey ";
            cmdText = replaceSqlTokens(cmdText);

            SqlCommand command = new SqlCommand(cmdText, BaseDA.connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.Int)).Value = itemId;

            command.ExecuteNonQuery();

        }

        public override void DeleteTableCode(string tableCode)
        {
            var cmdText = "{databaseOwner}{objectQualifier}{TableName}_DeleteTableCode ";
            cmdText = replaceSqlTokens(cmdText);

            SqlCommand command = new SqlCommand(cmdText, BaseDA.connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@TableCode", SqlDbType.NVarChar)).Value = tableCode;

            command.ExecuteNonQuery();
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

        private NBrightInfo ReadSqlCommand(SqlCommand command)
        {
            var nbi = new NBrightInfo();

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    nbi.ItemId = (int)reader[0];
                    nbi.PortalId = (int)reader[1];
                    nbi.ModuleId = (int)reader[2];
                    nbi.TableCode = (string)reader[3];
                    nbi.KeyData = (string)reader[4];
                    nbi.ModifiedDate = (DateTime)reader[5];
                    nbi.TextData = (string)reader[6];
                    nbi.XrefItemId = (int)reader[7];
                    nbi.ParentItemId = (int)reader[8];
                    nbi.XmlString = (string)reader[9];
                    nbi.Lang = (string)reader[10];
                    nbi.UserId = (int)reader[11];
                }
            }

            return nbi;
        }


        private List<NBrightInfo> ReadSqlListCommand(SqlCommand command)
        {
            var nbiList = new List<NBrightInfo>();


            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var nbi = new NBrightInfo();
                    nbi.ItemId = (int)reader[0];
                    nbi.PortalId = (int)reader[3];
                    nbi.ModuleId = (int)reader[4];
                    nbi.TableCode = (string)reader[5];
                    nbi.KeyData = (string)reader[6];
                    nbi.ModifiedDate = (DateTime)reader[7];
                    nbi.TextData = (string)reader[8];
                    nbi.XrefItemId = (int)reader[9];
                    nbi.ParentItemId = (int)reader[10];
                    nbi.XmlString = (string)reader[1];
                    nbi.Lang = (string)reader[2];
                    nbi.UserId = (int)reader[11];
                    nbiList.Add(nbi);
                }
            }

            return nbiList;
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
