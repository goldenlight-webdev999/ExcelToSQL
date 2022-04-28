using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using System.Data.Common;
using System.Collections;
using System.Net;
//using TelAPI;
//using Twilio;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
//using Twilio.Lookups;
using System.Net.Mail;
using System.Threading.Tasks;
using GroGroup.Controllers;

namespace GroGroup.Class
{

    public class Aptus
    {
            enum CryptoMode
            {
                Encrypt,
                Decrypt
            }
            public string gCryptoSeed = "!@#$%^&*()_+";
            const string ERROR_LINE = "<error>{0}</error>";
            public string gloguser = "";
            public string gMemberID = "";
            public string gEmployeeID = "";
            public string gMemberName = "";
            public string gMemFirstName = "";
            public string gMemLastName = "";
            public string gLocation = "";
            public string Connstr = "";
            public string strconn = "";
            public string gAppno = "";
            public string ItemNo = "";
            public string nusername = "";
            public string resourcetype = "";
            public string cmblocation = "";
            private SqlDataAdapter da = new SqlDataAdapter();
            private DataSet ds = new DataSet();
            private SqlConnection con;
            public string depositValue;
            private DataSet dsSystem = new DataSet();

            string _SendErrMessage = "";
            public string SendErrMessage
            {
                get
                {
                    return this._SendErrMessage;
                }
                set
                {
                    this._SendErrMessage = value;
                }
            }

            public string vSqlString(string strvalue)
            {
                return strvalue.Replace("'", "''");
            }
            public DataSet GetDataSetFromSP(string lSql, string tblname, ref DataSet lds, ref SqlDataAdapter lCmd, SqlParameter[] lparams, SqlConnection tConn)
            {
                SqlDataAdapter da = (SqlDataAdapter)lCmd;
                //tConn = vDBProvider.CreateConnection(_gConnectString); 
                if (tConn.State == 0) tConn.Open();
                da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(lSql, tConn);
                lCmd = (SqlDataAdapter)da;
                lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
            lCmd.SelectCommand.CommandTimeout = 240;
            lCmd.SelectCommand.Parameters.Clear();
                for (int i = 0; i < lparams.Length; i++)
                {
                    lCmd.SelectCommand.Parameters.Add(lparams[i]);
                }
                if (lds.Tables[tblname] != null) lds.Tables[tblname].Clear();
                try
                {
                    lCmd.Fill(lds, tblname);
                }
                catch (Exception e)
                {
                    throw new Exception("Error(s) occured in GetDataSetFromSP \n" + e.ToString());
                }
            if(tConn.State == ConnectionState.Open)
                tConn.Close();
                //lCmd.Fill(lds);
                return lds;
            }

            public DataSet GetDataSetFromSP(string lSql, string tblname, ref DataSet lds, ref SqlDataAdapter lCmd, SqlParameter[] lparams, SqlConnection tConn, ref SqlTransaction sqlTrans)
            {
                //tConn = vDBProvider.CreateConnection(_gConnectString); 
                if (tConn.State == 0) tConn.Open();
                lCmd.SelectCommand = new SqlCommand(lSql, tConn, sqlTrans);
                lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
            lCmd.SelectCommand.CommandTimeout = 240;
            lCmd.SelectCommand.Parameters.Clear();
                for (int i = 0; i < lparams.Length; i++)
                {
                    lCmd.SelectCommand.Parameters.Add(lparams[i]);
                }
                if (lds.Tables[tblname] != null) lds.Tables[tblname].Clear();
                try
                {
                    lCmd.Fill(lds, tblname);
                }
                catch (Exception e)
                {
                    throw new Exception("Error(s) occured in GetDataSetFromSP \n" + e.ToString());
                }
                // tConn.Close();
                //lCmd.Fill(lds);
                return lds;
            }

            public DataSet GetDataSetFromSPV(string lSql, string tblname, ref DataSet lds, ref SqlDataAdapter lCmd, SqlParameter[] lparams, SqlConnection tConn)
            {
                SqlDataAdapter da = (SqlDataAdapter)lCmd;
                //tConn = vDBProvider.CreateConnection(_gConnectString); 
                if (tConn.State == 0) tConn.Open();
                da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(lSql, tConn);
                lCmd = (SqlDataAdapter)da;
                lCmd.SelectCommand = new SqlCommand(lSql, tConn);
                lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
            lCmd.SelectCommand.CommandTimeout = 240;
            lCmd.SelectCommand.Parameters.Clear();
                for (int i = 0; i < lparams.Length; i++)
                {
                    lCmd.SelectCommand.Parameters.Add(lparams[i]);
                }
                
                if (lds.Tables[tblname] != null) lds.Tables[tblname].Clear();
                try
                {
                    lCmd.Fill(lds, tblname);
                }
                catch (Exception e)
                {
                    throw new Exception("Error(s) occured in GetDataSetFromSP \n" + e.ToString());
                }
            if (tConn.State == ConnectionState.Open)
                tConn.Close();
                //lCmd.Fill(lds);
                return lds;
            }

            public void Getdataset(string lsql, string Tablename, DataSet oDs, ref SqlDataAdapter oDa, SqlConnection oConn, bool isConnClose = true)
            {
                oDa = new SqlDataAdapter();
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                oDa.SelectCommand = new SqlCommand(lsql, oConn);
                oDa.SelectCommand.CommandType = CommandType.Text;
                oDa.SelectCommand.CommandTimeout = 240;
                object custCB = new SqlCommandBuilder((SqlDataAdapter)oDa);
                if (oDs.Tables[Tablename] != null) oDs.Tables[Tablename].Clear();   
                oDa.Fill(oDs, Tablename);
                if (isConnClose)
                {
                    if (oConn.State == ConnectionState.Open)
                        oConn.Close();
                }
            }
            public void Getdataset(string lsql, string Tablename, DataSet oDs, ref SqlDataAdapter oDa, SqlConnection oConn, ref SqlTransaction sqlTrans)
            {
                oDa = new SqlDataAdapter();
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                oDa.SelectCommand = new SqlCommand(lsql, oConn, sqlTrans);
                oDa.SelectCommand.CommandType = CommandType.Text;
            oDa.SelectCommand.CommandTimeout = 240;
            object custCB = new SqlCommandBuilder((SqlDataAdapter)oDa);
                if (oDs.Tables[Tablename] != null) oDs.Tables[Tablename].Clear();
                oDa.Fill(oDs, Tablename);
                // oConn.Close();
            }
            public void Execute(string lsql, SqlConnection oConn, bool isConnClose = true)
            {
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = oConn;
                cmd.CommandText = lsql;
                cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 240;
            cmd.ExecuteNonQuery();
            if (isConnClose)
            {
                if (oConn.State == ConnectionState.Open)
                    oConn.Close();
            }
        }
            public void Execute(string lsql, SqlConnection oConn, ref SqlTransaction sqlTrans)
            {
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = oConn;
                cmd.CommandText = lsql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = sqlTrans;
                cmd.ExecuteNonQuery();
                //oConn.Close();
            }


            public void Getdataset(string lsql, string Tablename, DataSet oDs, ref SqlDataAdapter oDa, SqlConnection oConn, SqlParameter[] SQLparams)
            {
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                oDa.SelectCommand = new SqlCommand(lsql, oConn);
                oDa.SelectCommand.CommandType = CommandType.Text;
            oDa.SelectCommand.CommandTimeout = 240;
            object custCB = new SqlCommandBuilder((SqlDataAdapter)oDa);
                if (oDs.Tables[Tablename] != null) oDs.Tables[Tablename].Clear();
                for (int i = 0; i < SQLparams.Length; i++)
                {
                    if (SQLparams[i] != null)
                        oDa.SelectCommand.Parameters.Add(SQLparams[i]);
                }
                oDa.Fill(oDs, Tablename);
            if (oConn.State == ConnectionState.Open)
                oConn.Close();
            }

            public void DeleteCheckMain(string Tablename, DataSet oDs, ref SqlDataAdapter oDa, SqlConnection oConn, string ID, bool ShowWarning)
            {
                string lsql = "STP_DeleteCheck '" + Tablename + "','" + ID + "'";
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                oDa.SelectCommand = new SqlCommand(lsql, oConn);
                oDa.SelectCommand.CommandType = CommandType.Text;
                object custCB = new SqlCommandBuilder((SqlDataAdapter)oDa);
                if (oDs.Tables[Tablename] != null) oDs.Tables[Tablename].Clear();
                oDa.Fill(oDs, Tablename);
            }


            private string CryptoMain(CryptoMode Mode, string Key, string Text)
            {
                byte[] SeedHash, StrBuffer;
                string Ret = "";

                MD5CryptoServiceProvider HashMD5;
                TripleDESCryptoServiceProvider Des;


                HashMD5 = new MD5CryptoServiceProvider();
                SeedHash = HashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(Key));

                Des = new TripleDESCryptoServiceProvider();
                Des.Key = SeedHash;

                Des.Mode = CipherMode.ECB; //CBC, CFB1`
                try
                {
                    if (Mode == CryptoMode.Encrypt)
                    {
                        StrBuffer = ASCIIEncoding.ASCII.GetBytes(Text);
                        Ret = Convert.ToBase64String(Des.CreateEncryptor().TransformFinalBlock(StrBuffer, 0,
                            StrBuffer.Length));
                    }
                    else
                    {
                        StrBuffer = Convert.FromBase64String(Text);

                        Ret = ASCIIEncoding.ASCII.GetString(
                            Des.CreateDecryptor().TransformFinalBlock(StrBuffer, 0, StrBuffer.Length)
                            );
                    }

                    return Ret;
                }
                catch
                {
                    return Ret;
                }
            }

            public string Encrypt(string Key, string Text)
            {
                return CryptoMain(CryptoMode.Encrypt, Key, Text);
            }
            public string Decrypt(string Key, string Text)
            {
                return CryptoMain(CryptoMode.Decrypt, Key, Text);
            }
            public string Encrypt(string Text)
            {
                return CryptoMain(CryptoMode.Encrypt, this.gCryptoSeed, Text);
            }
            public string Decrypt(string Text)
            {
                Text = Text.Replace(" ", "+");
                return CryptoMain(CryptoMode.Decrypt, this.gCryptoSeed, Text);
            }


            public void LoadLicenseInfo()
            {
                DataSet Dsr = new DataSet();
                string XmlValues = "";
                string Filename = "";

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"VALIDATEDAptussoft_License.xml"))
                    Filename = AppDomain.CurrentDomain.BaseDirectory + @"VALIDATEDAptussoft_License.xml";


                StreamReader strReadxml = new StreamReader(Filename);
                XmlValues = strReadxml.ReadToEnd();
                strReadxml.Dispose();
                XmlValues = Decrypt(XmlValues.Trim());
                int sindex = XmlValues.IndexOf("<DatabaseConnection>");
                int eindex = XmlValues.IndexOf("<Location>");
                string conn = XmlValues.Substring(sindex, eindex - sindex);
                conn = conn.Replace("<DatabaseConnection>", "");
                conn = conn.Replace("</DatabaseConnection>", "");
                conn = conn.Replace("\r\n", "&");
                string[] connstr = conn.Split('&');
                connstr[1] = connstr[1].Replace("<Server>", "");
                connstr[1] = connstr[1].Replace("</Server>", "");

                connstr[2] = connstr[2].Replace("<Database>", "");
                connstr[2] = connstr[2].Replace("</Database>", "");

                connstr[3] = connstr[3].Replace("<UserID>", "");
                connstr[3] = connstr[3].Replace("</UserID>", "");

                connstr[4] = connstr[4].Replace("<Password>", "");
                connstr[4] = connstr[4].Replace("</Password>", "");
                string Server = "", DB = "", Uid = "", password = "";
                Server = "server=" + connstr[1].ToString().Trim() + ";";
                DB = "database=" + connstr[2].Trim() + ";";
                Uid = "uid=" + connstr[3].Trim() + ";";
                password = "pwd=" + connstr[4].Trim();
                strconn = Server.Trim() + DB.Trim() + Uid.Trim() + password.Trim();
                Connstr = strconn.Trim();
                // objVedas.gLocation = (string)ConfigurationManager.AppSettings["Location"];

            }

            public int dsUpdate(string tblName,DataSet lds, SqlDataAdapter lCmd, SqlConnection tConn)
            {
                if (tConn.State == 0) tConn.Open();
                return lCmd.Update(lds, tblName);
            }

            public int GetNextID(string KeyFld, SqlConnection Conn, SqlTransaction iTrans)
            {
                SqlCommand oCmd = new SqlCommand("getnextidcount", Conn);
                oCmd.CommandType = CommandType.StoredProcedure;
                oCmd.Transaction = iTrans;
                SqlParameter parameterKeyFld = new SqlParameter("@in_keyfld", SqlDbType.VarChar, 50);
                parameterKeyFld.Value = KeyFld;
                oCmd.Parameters.Add(parameterKeyFld);
                SqlParameter parameterCnt = new SqlParameter("@in_count", SqlDbType.Int, 4);
                parameterCnt.Value = 1;
                oCmd.Parameters.Add(parameterCnt);

                SqlParameter parameterOut = new SqlParameter("@out", SqlDbType.Int, 4); ;
                parameterOut.Direction = ParameterDirection.Output;
                oCmd.Parameters.Add(parameterOut);

                oCmd.ExecuteNonQuery();
                return (int)parameterOut.Value; 
            }

            public int GetIdentityValue(SqlConnection Conn)
            {
                try
                {
                    DataSet dsIdentityFetch = new DataSet();
                    string lsql = "";
                    //if (Provider == ProviderType.SqlLite)
                    //{
                    //    lsql = "select last_insert_rowid()";
                    //}
                    //else
                    //{
                    lsql = "select @@IDENTITY";
                    //}
                    SqlDataAdapter da = new SqlDataAdapter(lsql, Conn);
                    da.Fill(dsIdentityFetch, "IdentityTbl");
                    return Convert.ToInt32(dsIdentityFetch.Tables["IdentityTbl"].Rows[0][0]);
                }
                catch (Exception e)
                {
                    throw new Exception("Error(s) occured in GetIdentityValue in vMultiData \n" + e.ToString());
                }
            }

            public int GetIdentityValue(SqlConnection Conn, SqlTransaction trans)
            {
                try
                {
                    DataSet dsIdentityFetch = new DataSet();
                    string lsql = "";
                    //if (Provider == ProviderType.SqlLite)
                    //{
                    //    lsql = "select last_insert_rowid()";
                    //}
                    //else
                    //{
                    lsql = "select @@IDENTITY";
                    //}
                    SqlDataAdapter da = new SqlDataAdapter(lsql, Conn);
                    da.SelectCommand.Transaction = trans;
                    da.Fill(dsIdentityFetch, "IdentityTbl");
                    return Convert.ToInt32(dsIdentityFetch.Tables["IdentityTbl"].Rows[0][0]);
                }
                catch (Exception e)
                {
                    throw new Exception("Error(s) occured in GetIdentityValue in vMultiData \n" + e.ToString());
                }
            }

            public DataTable SelectDistinct(string TableName, DataTable SourceTable, string FieldName)
            {
                DataTable dt = new DataTable(TableName);
                dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

                object LastValue = null;
                foreach (DataRow dr in SourceTable.Select("", FieldName))
                {
                    if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
                    {
                        LastValue = dr[FieldName];
                        dt.Rows.Add(new object[] { LastValue });
                    }
                }
                /*			if (ds != null) 
                                ds.Tables.Add(dt);
                */
                return dt;
            }

            private bool ColumnEqual(object A, object B)
            {

                // Compares two values to see if they are equal. Also compares DBNULL.Value.
                // Note: If your DataTable contains object fields, then you must extend this
                // function to handle them in a meaningful way if you intend to group on them.

                if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
                    return true;
                if (A == DBNull.Value || B == DBNull.Value) //  only one is DBNull.Value
                    return false;
                return (A.Equals(B));  // value type standard comparison
            }

            public string GetConfigValue(string configGroup, string Parameter)
            {
                if (this.dsSystem.Tables["OP_Config"] == null)
                {
                    //MessageBox.Show("Config table not found. Please fill vApp.dsSystem with \"config\" table in your main exe.","Config",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    return null;
                }
                DataRow[] dRows = this.dsSystem.Tables["OP_Config"].Select("ConfigGroup='" + configGroup + "' and Parameter='" + Parameter + "'");
                if (dRows.Length > 0)
                    return dRows[0]["Value"].ToString();
                else
                    return "";
            }

            public string GetConfigValue(string FieldName)
            {
                DataTable dtconfig = (DataTable)HttpContext.Current.Session["ConfigList"];
                DataRow[] dRows = dtconfig.Select("Parameter='" + FieldName + "'");
                if (dRows.Length > 0)
                    return dRows[0]["Value"].ToString();
                else
                    return "";
            }

            public string SQLString(string lstr)
            {
                if (!string.IsNullOrEmpty(lstr))
                {
                    return lstr.Replace("'", "''");
                }
                return lstr;
            }

        public string ValidateUPC(string code)
        {
            int i, ln = 0, mul = 3, sum = 0;
            ln = code.Length - 1;
            for (i = 0; i < ln; i++)
            {
                int v;
                if (!int.TryParse(code[i].ToString(), out v))
                {
                    return "Invalid";
                }
                sum += v * mul;
                if (mul == 3) mul = 1;
                else mul = 3;
            }

            int check = 10 - (sum % 10);
            check = check % 10;

            return check.ToString();
        }

        public List<Hashtable> CovertDatasetToHashTbl(DataTable dt)
            {
                List<Hashtable> StartList = new List<Hashtable>();
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Hashtable hashtable = new Hashtable();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            if (dt.Columns[j].DataType.ToString() == "System.Byte[]")
                            {
                                if (dt.Rows[i][j] != null && dt.Rows[i][j].ToString() != "")
                                {
                                    byte[] array = (byte[])dt.Rows[i][j];
                                    hashtable[dt.Columns[j].ToString()] = "data:image/png;base64," + System.Convert.ToBase64String(array);
                                }
                                else
                                    hashtable[dt.Columns[j].ToString()] = "../images/photos/nothumb.png";
                            }
                            else
                                hashtable[dt.Columns[j].ToString()] = dt.Rows[i][j].ToString();
                        }
                        StartList.Add(hashtable);
                    }
                }
                return StartList;
            }

            public DataSet CreateDataSet<T>(List<T> list, string TableName)
            {
                //list is nothing or has nothing, return nothing (or add exception handling)
                if (list == null || list.Count == 0) { return null; }

                //get the type of the first obj in the list
                var obj = list[0].GetType();

                //now grab all properties
                var properties = obj.GetProperties();

                //make sure the obj has properties, return nothing (or add exception handling)
                if (properties.Length == 0) { return null; }

                //it does so create the dataset and table
                var dataSet = new DataSet();
                var dataTable = new DataTable();

                //now build the columns from the properties
                var columns = new DataColumn[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    columns[i] = new DataColumn(properties[i].Name, properties[i].PropertyType);
                }

                //add columns to table
                dataTable.Columns.AddRange(columns);

                //now add the list values to the table
                foreach (var item in list)
                {
                    //create a new row from table
                    var dataRow = dataTable.NewRow();

                    //now we have to iterate thru each property of the item and retrieve it's value for the corresponding row's cell
                    var itemProperties = item.GetType().GetProperties();

                    for (int i = 0; i < itemProperties.Length; i++)
                    {
                        dataRow[i] = itemProperties[i].GetValue(item, null);
                    }

                    //now add the populated row to the table
                    dataTable.Rows.Add(dataRow);
                }

                dataTable.TableName = TableName;
                //add table to dataset
                dataSet.Tables.Add(dataTable);
                
                //return dataset
                return dataSet;
            }

        

        //public List<SelecetedZiplist> UPCSrchlist()
        //{
        //    List<SelecetedZiplist> MenuList = new List<SelecetedZiplist>();
        //    MenuList.Add(new SelecetedZiplist
        //    {
        //        Display_FName = "UPC",
        //        Field_Name = "upc"
        //    });
        //    MenuList.Add(new SelecetedZiplist
        //    {
        //        Display_FName = "PartNo",
        //        Field_Name = "mfgpartno",
        //    });
        //    return MenuList;
        //}
        
        /// <summary>
        /// method to get Client ip address
        /// </summary>
        /// <param name="GetLan"> set to true if want to get local(LAN) Connected ip address</param>
        /// <returns></returns>
        public string GetVisitorIPAddress(bool GetLan = false)
            {
                string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (String.IsNullOrEmpty(visitorIPAddress))
                    visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

                if (string.IsNullOrEmpty(visitorIPAddress))
                    visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

                if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
                {
                    GetLan = true;
                    visitorIPAddress = string.Empty;
                }

                if (GetLan)
                {
                    if (string.IsNullOrEmpty(visitorIPAddress))
                    {
                        //This is for Local(LAN) Connected ID Address
                        string stringHostName = Dns.GetHostName();
                        //Get Ip Host Entry
                        IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                        //Get Ip Address From The Ip Host Entry Address List
                        IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                        try
                        {
                            visitorIPAddress = arrIpAddress[arrIpAddress.Length - 1].ToString();
                        }
                        catch
                        {
                            try
                            {
                                visitorIPAddress = arrIpAddress[0].ToString();
                            }
                            catch
                            {
                                try
                                {
                                    arrIpAddress = Dns.GetHostAddresses(stringHostName);
                                    visitorIPAddress = arrIpAddress[0].ToString();
                                }
                                catch
                                {
                                    visitorIPAddress = "127.0.0.1";
                                }
                            }
                        }
                    }
                }

                return visitorIPAddress;
            }

            public string SetVisibleToolbarButton(DataTable dt, string userModule, string description, string condition)
            {
                string msg = "";
                bool blnFlag = true;
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = "Module='Web' and SecurityID like 'SUBMENU_%' and UserModule='" + userModule + "' and Description='" + description + "' and SecurityID not like 'X%' and SecurityID not like '%OPT%' and Description not like '-%'";

                    DataTable dtMenu = dv.ToTable();

                    if(dtMenu != null && dtMenu.Rows.Count > 0)
                    {
                        if (condition.Trim().ToUpper() == "ADD")
                        {
                            blnFlag = bool.Parse(dtMenu.Rows[0]["UserAdd"].ToString());
                        }
                        if (condition.Trim().ToUpper() == "EDIT")
                        {
                            blnFlag = bool.Parse(dtMenu.Rows[0]["UserEdit"].ToString());
                        }
                        if (condition.Trim().ToUpper() == "DELETE")
                        {
                            blnFlag = bool.Parse(dtMenu.Rows[0]["UserDelete"].ToString());
                        }
                        if (condition.Trim().ToUpper() == "SEARCH")
                        {
                            blnFlag = bool.Parse(dtMenu.Rows[0]["UserSearch"].ToString());
                        }

                        if (!blnFlag)
                            msg = "You do not have enough rights to perform this operation";

                    }
                    
                }

                return msg;
            }
        
            public void ChangeLog(object EntityObject, object ChangedObject, string TableName, string TableKey, string UserName, SqlConnection Sconn)
            {
                string[] propertyNames = EntityObject.GetType().GetProperties().Select(p => p.Name).ToArray();
                foreach (var prop in propertyNames)
                {
                    object entityval = EntityObject.GetType().GetProperty(prop).GetValue(EntityObject, null);
                    object entitychangedval = ChangedObject.GetType().GetProperty(prop).GetValue(ChangedObject, null);
                    if ((entityval == null && entitychangedval == null) || (entityval == "" && entitychangedval == "") || (entityval == "" && entitychangedval == null) || (entityval == null && entitychangedval == ""))
                        continue;
                    if (entityval == null && entitychangedval != null)
                        entityval = "";
                    if (entitychangedval == null && entityval != null)
                        entitychangedval = "";

                    if (entityval.ToString().Trim().ToUpper() != entitychangedval.ToString().Trim().ToUpper() && prop.ToString().Trim().ToUpper() != "ENTRYDATE" && prop.ToString().Trim().ToUpper() != "EDITDATE" && prop.ToString().Trim().ToUpper() != "ENTITYSTATE" && prop.ToString().Trim().ToUpper() != "ENTITYKEY")
                    {
                        if (prop.ToString().Trim().ToUpper() == "HRLYRATE" && entityval.ToString() == "0.0000" && entitychangedval.ToString() == "0")
                            continue;
                        if (entityval.ToString().Contains("'"))
                            entityval = entityval.ToString().Replace("'", "''");
                        if (entitychangedval.ToString().Contains("'"))
                            entitychangedval = entitychangedval.ToString().Replace("'", "''");
                        string InsertEmailQry = "insert into OP_Log(TableName,TableKey,FieldName,OldVal,NewVal,EntryDate,EntryUser) values ('" + TableName + "','" + TableKey + "','" + prop + "','" + entityval + "','" + entitychangedval + "','" + System.DateTime.Now + "','" + UserName + "')";
                        Execute(InsertEmailQry, Sconn);
                    }
                }
            }

            public object ds2json(DataSet ds, string dsname)
            {
                var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented));
                var Result = dict[dsname];
                return Result;
            }

            /*
            public class SecurityRights
            {
                public string SecPointsNo;
                public string SecurityID;
                public string UserModule;
                public string disabled;
                public string SubModule;
                public string Description;
                public string Module;
                public string UserAdd;
                public string UserEdit;
                public string UserDelete;
                public string UserSearch;
                public string AddToolBar;
                public string ToolBarOrder;
                public string FileName;
                public string ToolButtonText;
                public string ToolBarImagePath;
                public string Tooltip;
                public string MenuShortCutKeys;
                public string SubMenuCreated;
            }*/

        }

    public static class EmailAsync
    {
        public static Task SendAsync(this SmtpClient client, MailMessage message)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Guid sendGuid = Guid.NewGuid();

            SendCompletedEventHandler handler = null;
            handler = (o, ea) =>
            {
                if (ea.UserState is Guid && ((Guid)ea.UserState) == sendGuid)
                {
                    client.SendCompleted -= handler;
                    if (ea.Cancelled)
                    {
                        tcs.SetCanceled();
                    }
                    else if (ea.Error != null)
                    {
                        tcs.SetException(ea.Error);
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                }
            };

            client.SendCompleted += handler;
            client.SendAsync(message, sendGuid);
            return tcs.Task;
        }
    }
    
}