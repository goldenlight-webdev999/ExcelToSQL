using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;

using System.Linq;
using System.Reflection;
using System.Data.OleDb;
using GroGroup.Class;

namespace VedasNs
{
    class vCustomReports
    {
        Aptus objVedas = new Aptus();
       
        private static Type[] _dataAdapterTypes = new Type[] { typeof(OleDbDataAdapter), typeof(SqlDataAdapter) };
        private ProviderType _provider;
        private static Type[] _commandTypes = new Type[] { typeof(OleDbCommand), typeof(SqlCommand) };
        private static Type[] _dataParameterTypes = new Type[] { typeof(OleDbParameter), typeof(SqlParameter) };
        private static Type[] _connectionTypes = new Type[] { typeof(OleDbConnection), typeof(SqlConnection) };
        private static SqlConnection oConn;
        private DataSet DsCustom = new DataSet();
        private SqlDataAdapter DaCustom;
        SqlParameter[] paramslist;
        SqlParameter param1, param2, param3, param4, param5, param6, param7, param8;
        private string BuildFldValues = "", BuildFldNames = "", lcReportName = "", lcUsermod = "";
        private string lcparamLocation = "", lcparamAcctNo = "", lcparamMainAcctNo = "";
        private string lcparamAcctno = "", lcparamCategory = "", lcparamitem_class = "", lcparamitem_classid = "", lcparamitem_classType = "", lcparamitemNo = "", lcparamEdate = "", lcparamSdate = "", lcparamsalesrep = "", lcparamStime = "", lcparamEtime = "", lcparmEmachine = "", lcResource = "", lchkHistory = "", lc_EntryUser = "", lc_Balance = "", lcparam_memtype = "";
        private string PayLoc = "", category = "", iclass = "", classId = "", classType = "", icode = "", lwhere = "", and = "", location = "", timewhere = "", Acctno = "", salesRep = "", lcstatus = "";
        private string lcparamCardNo = "", lcparamexpdate = "";
        private string creditcardno = "";
        public string SendErrMessage = "";
        private SqlDataAdapter SqlDaCustom;
        private static SqlConnection SqlConn;

        public vCustomReports(string sconn)
        {
           // oConn = sconn;
            //string connection = "";// "Provider=SQLOLEDB;Server=192.168.168.41;Database=Vedascms26;Uid=sa;Pwd=systems;";
            //connection = "Provider=SQLOLEDB;"+ sconn;// String.Format("Provider=SQLOLEDB;Server={0};Database={1};Uid={2};Pwd={3};", sconn.DataSource, sconn.Database);

            oConn = new SqlConnection(sconn);  //CreateConnection(sconn);
            SqlConn = new SqlConnection(sconn);
            DsCustom = new DataSet();
            DaCustom = new SqlDataAdapter();
        }

        public vCustomReports(string ReportName, string UserMod)
        {
            lcReportName = ReportName;
            lcUsermod = UserMod;
        }

        # region Management Report

        private string ManagementReports(Hashtable lcParam)
        {
            try
            {
                IDictionaryEnumerator myEnumerator = lcParam.GetEnumerator();
                string Sdate = "", Edate = "";
                while (myEnumerator.MoveNext())
                {
                    BuildFldNames = myEnumerator.Key.ToString();
                    BuildFldValues = myEnumerator.Value.ToString();

                    if (BuildFldNames.ToUpper() == "MAINACCTNO")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage = "Please select/unselect checkbox";
                            return "";
                        }
                        else
                        {
                            param5 = new SqlParameter("@MAINACCTNO", DbType.Int32); //CreateDataParameter("@MAINACCTNO", DbType.Int32);
                            if (myEnumerator.Value.ToString() == "Checked")
                                param5.Value = "1";
                            else
                                param5.Value = "0";
                            lcparamMainAcctNo = BuildFldValues;
                        }
                    }

                    if (BuildFldNames.ToUpper() == "LOCATION")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage = "Location cannot be blank";
                            return "";
                        }
                        else
                        {
                            param1 = new SqlParameter("@Locationid", DbType.String); //CreateDataParameter("@Locationid", DbType.String, 300);
                            param1.Value = BuildFldValues;
                            lcparamLocation = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "SDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage="Please enter Start Date";
                            return "";
                        }
                        else
                        {
                            param2 = new SqlParameter("@stdate", DbType.String); //CreateDataParameter("@stdate", DbType.String, 12);
                            param2.Value = BuildFldValues;
                            Sdate = BuildFldValues;
                            lcparamSdate = BuildFldValues;
                        }
                    }

                    if (BuildFldNames.ToUpper() == "EDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage="Please enter End Date";
                            return "";
                        }
                        else
                        {
                            param3 = new SqlParameter("@enddate", DbType.String);//CreateDataParameter("@enddate", DbType.String, 12);
                            param3.Value = BuildFldValues;
                            Edate = BuildFldValues;
                            lcparamEdate = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "EDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage="Please enter End Date";
                            return "";
                        }
                        else
                        {

                            param3 = new SqlParameter("@enddate", DbType.String);//CreateDataParameter("@enddate", DbType.String, 12);
                            param3.Value = BuildFldValues;
                            Edate = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "ASOFDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage="Please enter Date";
                            return "";
                        }
                        else
                        {
                            param2 = new SqlParameter("@stdate", DbType.DateTime); //CreateDataParameter("@stdate", DbType.DateTime, 8);
                            param2.Value = BuildFldValues;
                            param3 = new SqlParameter("@enddate", DbType.DateTime); //CreateDataParameter("@enddate", DbType.DateTime, 8);
                            param3.Value = BuildFldValues;
                            lcparamEdate = BuildFldValues;
                        }
                    }


                }

                if (Sdate.Trim() != "" && Edate.Trim() != "")
                {
                    DateTime Dt1 = Convert.ToDateTime(Sdate.ToString());
                    DateTime Dt2 = Convert.ToDateTime(Edate.ToString());
                    if (Dt1 > Dt2)
                    {
                        SendErrMessage = "StartDate should be less than the enddate.";
                        return "";
                    }
                }
                //
                string[] chk_location = lcparamLocation.Split(',');
                PayLoc = "";

                if (chk_location.Length > 0)
                {
                    for (int i = 0; i <= chk_location.Length - 1; i++)
                    {
                        PayLoc += "" + chk_location[i].ToString() + ",";
                    }
                    PayLoc = PayLoc.Remove(PayLoc.Length - 1, 1);
                }
                return "S";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        
        
        public DataSet ManagementReport_ActiveMembershipListingpermonth(Hashtable lcParameters)
        {
            //4
            string getValidMsg = ManagementReports(lcParameters);
            if (getValidMsg == "") return null;
            paramslist = new SqlParameter[] { param1, param2, param3 };
            GetDataSetFromSPV("STP_RP_MemTypeslisting", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        

        # endregion Management Report

        #region Inventory Sales Report

        private string InventorySalesReports(Hashtable lcParam)
        {
            IDictionaryEnumerator myEnumerator = lcParam.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                BuildFldNames = myEnumerator.Key.ToString();
                BuildFldValues = myEnumerator.Value.ToString();

                if (BuildFldNames.ToUpper() == "LOCATION")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                       SendErrMessage = "Location cannot be blank.";
                       return "";
                    }
                    else
                    {
                        lcparamLocation = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "MM_ACCOUNT.STATUS")
                {
                    lcstatus = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                        SendErrMessage = "Please enter Start Date";
                        return "";
                    }
                    else
                    {
                        lcparamSdate = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "ACCTNO")
                {
                    lcparamAcctNo = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "EDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                        SendErrMessage = "Please enter End Date";
                        return "";
                    }
                    else
                    {
                        lcparamEdate = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "CATEGORY")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamCategory = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "ITEMNO")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamitemNo = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "CLASS")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamitem_class = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "MM.ITEMNO")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparam_memtype = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SALESPERSON")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamsalesrep = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "STIME")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lcparamStime = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "ETIME")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lcparamEtime = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SM.ENTRYMACHINE")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lcparmEmachine = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SM.ENTRYUSER")
                {
                    if (BuildFldValues.Trim() != "")
                    {
                        this.lc_EntryUser = BuildFldValues;
                    }
                    else
                    {
                        this.lc_EntryUser = "";
                    }
                }
                if (BuildFldNames.ToUpper() == "SM.BALANCE")
                {
                    if (BuildFldValues.Trim() != "")
                    {
                        this.lc_Balance = BuildFldValues;
                    }
                    else
                    {
                        this.lc_Balance = "";
                    }
                }
            }

            //copied
            string[] chk_location = lcparamLocation.Split(',');
            PayLoc = "";
            if (chk_location.Length > 0)
            {
                for (int i = 0; i <= chk_location.Length - 1; i++)
                {
                    PayLoc += chk_location[i].ToString().Trim() + ",";
                }
                PayLoc = PayLoc.Remove(PayLoc.Length - 1, 1).Trim();
            }

            string[] chk_category = lcparamCategory.Split(',');
            string cat = "";
            if (chk_category.Length > 0)
            {
                for (int i = 0; i <= chk_category.Length - 1; i++)
                {
                    if (i + 1 <= chk_category.Length - 1)
                        cat += "" + chk_category[i].ToString().Trim() + "','";
                    else
                        cat += "" + chk_category[i].ToString().Trim();
                }
            }
            lcparamCategory = cat;

            string[] chk_memtype = lcparam_memtype.Split(',');
            string type = "";
            if (chk_memtype.Length > 0)
            {
                for (int i = 0; i <= chk_memtype.Length - 1; i++)
                {
                    if (i + 1 <= chk_memtype.Length - 1)
                        type += "" + chk_memtype[i].ToString().Trim() + "','";
                    else
                        type += "" + chk_memtype[i].ToString().Trim();
                }
                lcparam_memtype = type;
            }

            string[] chk_class = lcparamitem_class.Split(',');
            cat = "";
            if (chk_class.Length > 0)
            {
                for (int i = 0; i <= chk_class.Length - 1; i++)
                {
                    if (i + 1 <= chk_class.Length - 1)
                        cat += "" + chk_class[i].ToString().Trim() + "','";
                    else
                        cat += "" + chk_class[i].ToString().Trim();
                }
            }
            this.lcparamitem_class = cat;

            string[] chk_item = lcparamitemNo.Split(',');
            cat = "";
            if (chk_item.Length > 0)
            {
                for (int i = 0; i <= chk_item.Length - 1; i++)
                {
                    if (i + 1 <= chk_item.Length - 1)
                        cat += "" + chk_item[i].ToString().Trim() + "','";
                    else
                        cat += "" + chk_item[i].ToString().Trim();
                }
            }
            this.lcparamitemNo = cat;

            string[] chk_status = lcstatus.Split(',');
            cat = "";
            if (chk_status.Length > 0)
            {
                for (int i = 0; i <= chk_status.Length - 1; i++)
                {
                    if (i + 1 <= chk_status.Length - 1)
                        cat += "" + chk_status[i].ToString().Trim() + "','";
                    else
                        cat += "" + chk_status[i].ToString().Trim();
                }
            }
            this.lcstatus = cat;

            and = "  and  ";
            location = " and locationid in (" + PayLoc + ") ";
            if (lcparamCategory.ToString().Trim() != "")
                category = " and ic.group1 in ('" + this.lcparamCategory.ToString().Trim() + "') ";
            else
                category = "";
            if (this.lcparamitem_class.ToString() != "")
                iclass = " and class in ('" + this.lcparamitem_class.ToString().Trim() + "') ";
            else
                iclass = "";

            if (this.lcparamitemNo.ToString() != "")
                icode = " and st.itemno in ('" + this.lcparamitemNo.ToString() + "') ";
            else
                icode = "";

            lwhere = " '" + this.lcparamSdate.ToString() + "' " + and + " '" + this.lcparamEdate.ToString() + "' " + location.Trim() + category + iclass + icode;

            param1 = new SqlParameter("@lwhere", DbType.String); //CreateDataParameter("@lwhere", DbType.String, 8000);
            param3 = new SqlParameter("@edate", DbType.String);//CreateDataParameter("@edate", DbType.String, 8000);
            param4 = new SqlParameter("@eloc", DbType.String);//CreateDataParameter("@eloc", DbType.String, 8000);
            param5 = new SqlParameter("@edate", DbType.String);//CreateDataParameter("@edate", DbType.String, 8000);
            param6 = new SqlParameter("@lwhere", DbType.String);//CreateDataParameter("@lwhere", DbType.String, 8000);
            param7 = new SqlParameter("@Balance", DbType.String);//CreateDataParameter("@Balance", DbType.Decimal, 8000);
            param8 = new SqlParameter("@plocation", DbType.String);//CreateDataParameter("@Balance", DbType.Decimal, 8000);
            //end
            return "S";
        }
        public DataSet InventorySalesReport_ItemSalesYTDComparison(Hashtable lcParameters)
        {
            //1
            string getValidMsg = InventorySalesReports(lcParameters);
            if (getValidMsg == "") return null;
          
            lwhere = " '" + this.lcparamSdate.ToString() + "' " + and + " '" + this.lcparamEdate.ToString() + "' " + category + iclass + icode;
            string where = " InvoiceDate between  " + lwhere + "";
            param1.Value = where;
            
           
            string lwhere2 = this.category + this.iclass + this.icode;
            string MMddfrom = GetConfigValue("salesytdmmdd");
            param3.Value = this.lcparamEdate.ToString();
            param4.Value = this.PayLoc;
            param5.Value = lwhere2;
            param6.Value = MMddfrom.ToString();
            paramslist = new SqlParameter[] { param1, param3, param4, param5, param6 };

            GetDataSetFromSPV("stp_rp_ic_itemsales", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet InventorySalesReport_ItemSalesPeriodComparison(Hashtable lcParameters)
        {

            //2
            string getValidMsg = InventorySalesReports(lcParameters);
            if (getValidMsg == "") return null;
           
            lwhere = " '" + this.lcparamSdate.ToString() + "' " + and + " '" + this.lcparamEdate.ToString() + "' " + this.category + this.iclass + this.icode;
            string where = " InvoiceDate between  " + lwhere + "";
            param1.Value = where;
            

            string lwhere2 = " InvoiceDate between '" + Convert.ToDateTime(this.lcparamSdate.ToString()).AddYears(-1).ToShortDateString() + "' " + and + " '" + Convert.ToDateTime(this.lcparamEdate.ToString()).AddYears(-1).ToShortDateString() + "' " + this.category + this.iclass + this.icode;
            param3.Value = this.lcparamEdate.ToString();
            param4.Value = this.PayLoc;
            param5.Value = this.lcparamSdate.ToString();
            param6.Value = lwhere2;

            paramslist = new SqlParameter[] { param1, param3, param4, param5, param6 };
            GetDataSetFromSPV("stp_rp_ic_itemsalescomp", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }

        public DataSet InventorySalesReport_ItemSalesbyLocation(Hashtable lcParameters)
        {
            string sdate = lcParameters["Date"].ToString();
            lcParameters.Remove("Date");
            lcParameters.Add("SDate", sdate);
            string getValidMsg = InventorySalesReports(lcParameters);
            if (getValidMsg == "") return null;

            lwhere = " '" + this.lcparamSdate.ToString() + "' " + and + " '" + lcparamEdate.ToString() + "' " + this.category + this.iclass + this.icode;
            string where = " st.InvoiceDate between  " + lwhere + "";
            string Workstation = "";
            if (this.lcparmEmachine.ToString() != "")
            {
                lcparmEmachine = this.lcparmEmachine.Replace(",", "','");
                Workstation = " and st.entrymachine in ('" + this.lcparmEmachine.ToString() + "')";
                where = where + Workstation;
            }
            if (this.lcparamStime.ToString() != "" && this.lcparamEtime.ToString() != "")
            {
                /*Name:Dharmaraj on 29-Apr-2011
                  * Description:Need to convert stat and end time as datate time for 24 hour format
                  * Bug No:1025(check all sales reports)
               */
                timewhere = "and Convert(varchar,entrydate,108) between Convert(varchar,Convert(datetime,'" + this.lcparamStime.ToString() + "',108),108)" + and + " Convert(varchar,Convert(datetime,'" + lcparamEtime.ToString() + "',108),108) ";
                param1.Value = where + this.timewhere;
            }
            else
                param1.Value = where;

            /*Name:Suresh S on 28-Apr-2011
            * Description:To search details based on itemcode the below code has been added
              to display item code text on top of the Item sales by Location
            * Bug No:1025(check all sales reports)
            */

            param8.Value = this.PayLoc;
            paramslist = new SqlParameter[] { param1, param8 };

            
            GetDataSetFromSPV("STP_RP_IC_ItemSalesbyLocationComparison", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
       
        # endregion Inventory Sales Report

        #region Accounts Receivable Reports

        private string ARReports(Hashtable lcParam)
        {
            try
            {
                IDictionaryEnumerator myEnumerator = lcParam.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    BuildFldNames = myEnumerator.Key.ToString();
                    BuildFldValues = myEnumerator.Value.ToString();

                    if (BuildFldNames.ToUpper() == "LOCATION" || BuildFldNames.ToUpper() == "LOCATIONID")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            lcparamLocation = "";
                        }
                        else
                        {
                            lcparamLocation = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "CARDNO")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            lcparamCardNo = "";
                        }
                        else
                        {
                            lcparamCardNo = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "EXPDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage = "Please enter Expiry Date";
                            return "";
                        }
                        else
                        {
                            lcparamexpdate = BuildFldValues;
                        }
                    }
                }
                //ExpDate
                return "S";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        public DataSet Letter_PartyConfirmationLetter(Hashtable lcParameters)
        {
            //1
            string getValidMsg = ARReports(lcParameters);
            if (getValidMsg == "") return null;


            param1 = new SqlParameter("@stdate", DbType.DateTime); //CreateDataParameter("@stdate", DbType.DateTime, 10);
            param2 = new SqlParameter("@eddate", DbType.DateTime); //CreateDataParameter("@eddate", DbType.DateTime, 80);
            param3 = new SqlParameter("@loc", DbType.String); //CreateDataParameter("@loc", DbType.String, 80);
            param4 = new SqlParameter("@acctno", DbType.Int32); //CreateDataParameter("@acctno", DbType.Int32, 80);
            param1.Value = lcParameters["stdate"];
            param2.Value = lcParameters["eddate"];
            param3.Value = lcParameters["loc"];
            param4.Value = lcParameters["acctno"] == "" ? null : lcParameters["acctno"];



            paramslist = new SqlParameter[] { param1, param2, param3, param4 };

            GetDataSetFromSPV("STP_RP_IC_PartyDetails", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);



            return DsCustom;
        }
        public DataSet ARReports_ActiveCreditCardListing(Hashtable lcParameters)
        {
            //1
            string getValidMsg = ARReports(lcParameters);
            if (getValidMsg == "") return null;

            //copied

            param1 = new SqlParameter("@Locationid", DbType.String); //CreateDataParameter("@Locationid", DbType.String, 10);
            param2 = new SqlParameter("@creditCardNo", DbType.String); //CreateDataParameter("@creditCardNo", DbType.String, 80);
            if (lcparamLocation.ToUpper() != "ALL")
                param1.Value = lcparamLocation.ToString();
            else
                param1.Value = null;

            if (this.lcparamCardNo.Trim() != "")
            {
                if (this.lcparamCardNo.Trim().Length > 4)
                    creditcardno = objVedas.Encrypt(this.lcparamCardNo.Trim());
                else
                    creditcardno = this.lcparamCardNo.Trim();
                param2.Value = creditcardno;
            }
            else
                param2.Value = null;

            paramslist = new SqlParameter[] { param1, param2 };
            GetDataSetFromSPV("STP_RP_AR_ActiveCreditCardListing", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet ARReports_ExpiredCreditCardListing(Hashtable lcParameters)
        {
            //2
            string getValidMsg = ARReports(lcParameters);
            if (getValidMsg == "") return null;

            param1 = new SqlParameter("@Locationid", DbType.String); //CreateDataParameter("@Locationid", DbType.String, 10);

            if (this.lcparamLocation.ToString().ToUpper() != "ALL")
                param1.Value = this.lcparamLocation.ToString();
            else
                param1.Value = null;

            param2 = new SqlParameter("@expdate", DbType.Date);//CreateDataParameter("@expdate", DbType.Date);
            param2.Value = lcparamexpdate.ToString();

            paramslist = new SqlParameter[] { param1, param2 };
            GetDataSetFromSPV("STP_RP_AR_ExpiredCreditCardListing", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            if (this.DsCustom.Tables["Results"].Rows.Count > 0)
            {

                for (int i = 0; i < this.DsCustom.Tables["Results"].Rows.Count; i++)
                {
                    if (this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"].ToString().Trim() == "")
                    {
                        continue;
                    }

                    if (this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"] == DBNull.Value || this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"].ToString().Trim() != "" || this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"].ToString().Substring(0, 1) == "*" || this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"].ToString().Substring(0, 1) == "X")
                        continue;
                    int Temp = 0;
                    if (Int32.TryParse(this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"].ToString(), out Temp)) continue;
                    string decrypt = objVedas.Decrypt(this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"].ToString().Trim());
                    string fetch = "", last4val = "";
                    int Max;
                    if (decrypt != "")
                    {
                        Max = decrypt.Trim().Length;
                        if (Max > 4)
                        {
                            fetch = decrypt.Substring(0, Max - 4);

                            last4val = decrypt.Substring(Max - 4, 4);

                            decrypt = "";
                            for (int j = 0; j < fetch.Length; j++)
                            {
                                decrypt += "*";
                            }
                            decrypt += last4val;
                        }
                    }
                    this.DsCustom.Tables["Results"].Rows[i]["creditCardnumber"] = decrypt.Trim();
                }
            }
            return DsCustom;
        }
        public DataSet ARReports_ActiveEFTListing(Hashtable lcParameters)
        {
            //3
            string getValidMsg = ARReports(lcParameters);
            if (getValidMsg == "") return null;
            param1 = new SqlParameter("@Locationid", DbType.String); //CreateDataParameter("@Locationid", DbType.String, 10);
            if (this.lcparamLocation.ToString().ToUpper() != "ALL")
                param1.Value = this.lcparamLocation.ToString();
            else
                param1.Value = null;

            paramslist = new SqlParameter[] { param1 };
            GetDataSetFromSPV("STP_RP_AR_ActiveEFTListing", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            if (this.DsCustom.Tables["Results"].Rows.Count > 0)
            {

                for (int i = 0; i < this.DsCustom.Tables["Results"].Rows.Count; i++)
                {
                    if (this.DsCustom.Tables["Results"].Rows[i]["account"].ToString().Trim() == "")
                    {
                        continue;
                    }

                    string decrypt = objVedas.Decrypt(this.DsCustom.Tables["Results"].Rows[i]["account"].ToString().Trim());
                    DsCustom.Tables["Results"].Rows[i]["account"] = decrypt.Trim();
                }
            }
            return DsCustom;
        }

        #endregion Accounts Receivable Reports

        #region Schedule Reports

        private string ScheduleReports(Hashtable lcParam)
        {
            try
            {

                IDictionaryEnumerator myEnumerator = lcParam.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    BuildFldNames = myEnumerator.Key.ToString();
                    BuildFldValues = myEnumerator.Value.ToString();


                    if (BuildFldNames.ToUpper() == "LOCATION" || BuildFldNames.ToUpper() == "LOCATIONID")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage = "Location cannot be blank";
                            return "";
                        }
                        else
                        {
                            lcparamLocation = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "SDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage = "Please enter Start Date";
                            return "";
                        }
                        else
                        {
                            lcparamSdate = BuildFldValues;
                        }
                    }

                    if (BuildFldNames.ToUpper() == "EDATE")
                    {
                        if (BuildFldValues.Trim() == "")
                        {
                            SendErrMessage = "Please enter End Date";
                            return "";
                        }
                        else
                        {
                            lcparamEdate = BuildFldValues;
                        }
                    }
                    if (BuildFldNames.ToUpper() == "AS_APPT.RESOURCEID")
                    {
                        if (BuildFldValues.Trim() != "")
                            lcResource = BuildFldValues;
                    }
                }


                string[] chk_location = lcparamLocation.Split(',');

                if (chk_location.Length > 0)
                {

                    for (int i = 0; i <= chk_location.Length - 1; i++)
                    {
                        PayLoc += chk_location[i].ToString() + ",";
                    }
                    PayLoc = PayLoc.Remove(PayLoc.Length - 1, 1);
                }
               
                string location = PayLoc;

                param1 = new SqlParameter("@sdate", DbType.String); //CreateDataParameter("@sdate", DbType.String, 500);
                param2 = new SqlParameter("@edate", DbType.String); //CreateDataParameter("@edate", DbType.String, 1000);
                param3 = new SqlParameter("@plocation", DbType.String); //CreateDataParameter("@plocation", DbType.String, 1000);
                param5 = new SqlParameter("@as_appt.resourceid", DbType.String); //CreateDataParameter("@as_appt.resourceid", DbType.String, 3000);

                param1.Value = this.lcparamSdate.ToString();
                param2.Value = this.lcparamEdate.ToString();
                param3.Value = location.Trim();
                if (this.lcResource.ToString() != "")
                    param5.Value = this.lcResource.ToString();
                else
                    param5.Value = "";

                paramslist = new SqlParameter[] { param1, param2, param3, param4, param5 };
                return "S";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        public DataSet ScheduleReports_ResourceUtilization(Hashtable lcParameters)
        {
            //1
            string getValidMsg = ScheduleReports(lcParameters);
            if (getValidMsg == "") return null;
            param5 = new SqlParameter("@ResourceType", DbType.String); //CreateDataParameter("@ResourceType", DbType.String, 20);
            param5.Value = lcParameters["ResourceType"].ToString();
            paramslist = new SqlParameter[] { param1, param2, param3, param5 };
            GetDataSetFromSPV("STP_RP_AS_ResourceUtilization", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet ScheduleReports_ResourceRevenue(Hashtable lcParameters)
        {
            //2
            string getValidMsg = ScheduleReports(lcParameters);
            if (getValidMsg == "") return null;
            paramslist = new SqlParameter[] { param1, param2, param3 };
            GetDataSetFromSPV("STP_RP_AS_ResourceRevenue", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet ScheduleReports_ResourceOccupancy(Hashtable lcParameters)
        {
            //3
            string getValidMsg = ScheduleReports(lcParameters);
            if (getValidMsg == "") return null;
            paramslist = new SqlParameter[] { param1, param2, param3 };
            GetDataSetFromSPV("STP_RP_AS_Resourceoccupancy", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        #endregion Schedule Reports

        #region Scheduling Analysis Reports

        private string SchedulingAnalysisReports(Hashtable lcParam)
        {
            IDictionaryEnumerator myEnumerator = lcParam.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                BuildFldNames = myEnumerator.Key.ToString();
                BuildFldValues = myEnumerator.Value.ToString();

                if (BuildFldNames.ToUpper() == "LOCATION")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                        SendErrMessage = "Location cannot be blank";
                        return "";
                    }
                    else
                    {
                        lcparamLocation = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "SDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                        SendErrMessage = "Please enter Start Date";
                        return "";
                    }
                    else
                    {
                        lcparamSdate = BuildFldValues;
                    }
                }

                if (BuildFldNames.ToUpper() == "EDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                        SendErrMessage = "Please enter End Date";
                        return "";
                    }
                    else
                    {
                        lcparamEdate = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "CATEGORY")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamCategory = BuildFldValues;
                }

                if (BuildFldNames.ToUpper() == "CLASS")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamitem_class = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "CLASSTYPE")
                {
                    if (BuildFldValues.Trim() != "")
                    {
                        param8 = new SqlParameter("@SchClassType", DbType.String); //CreateDataParameter("@SchClassType", DbType.String, 2000);
                        param8.Value = BuildFldValues;
                        lcparamitem_classType = BuildFldValues;

                        string[] chk_ClassType = lcparamitem_classType.Split(',');
                        classType = "";
                        if (chk_ClassType.Length > 0)
                        {
                            for (int i = 0; i <= chk_ClassType.Length - 1; i++)
                            {
                                classType += "'" + chk_ClassType[i].ToString().Trim() + "',";
                            }
                            classType = classType.Remove(classType.LastIndexOf(','));
                        }
                    }
                }

                if (BuildFldNames.ToUpper() == "CLASSID")
                {
                    if (BuildFldValues.Trim() != "")
                    {

                        param7 = new SqlParameter("@SchClass", DbType.String);//CreateDataParameter("@SchClass", DbType.String, 2000);
                        param7.Value = BuildFldValues;
                        lcparamitem_classid = BuildFldValues;

                        string[] chk_ClassType = lcparamitem_classid.Split(',');
                        classId = "";
                        if (chk_ClassType.Length > 0)
                        {
                            for (int i = 0; i <= chk_ClassType.Length - 1; i++)
                            {
                                classId += "'" + chk_ClassType[i].ToString().Trim() + "',";
                            }
                            classId = classId.Remove(classId.LastIndexOf(','));
                        }
                    }
                }
            }

            //copied
            string[] chk_location = lcparamLocation.Split(',');
            PayLoc = "";
            if (chk_location.Length > 0)
            {
                for (int i = 0; i <= chk_location.Length - 1; i++)
                {
                    PayLoc += chk_location[i].ToString() + ",";
                }
                PayLoc = PayLoc.Remove(PayLoc.Length - 1, 1);
            }
            
            location = PayLoc;
            param1 = new SqlParameter("@sdate", DbType.String); //CreateDataParameter("@sdate", DbType.String, 500);
            param2 = new SqlParameter("@edate", DbType.String); //CreateDataParameter("@edate", DbType.String, 1000);
            param3 = new SqlParameter("@plocation", DbType.String); //CreateDataParameter("@plocation", DbType.String, 1000);
            param5 = new SqlParameter("@cat", DbType.String); //CreateDataParameter("@cat", DbType.String, 500); //changes for center court
            param6 = new SqlParameter("@class", DbType.String); //CreateDataParameter("@class", DbType.String, 500);
            param7 = new SqlParameter("@SchClass", DbType.String); //CreateDataParameter("@SchClass", DbType.String, 2000);
            param8 = new SqlParameter("@SchClassType", DbType.String); //CreateDataParameter("@SchClassType", DbType.String, 2000);

            param1.Value = this.lcparamSdate.ToString();
            param2.Value = this.lcparamEdate.ToString();
            param3.Value = location.Trim();
            if (this.lcparamCategory.Trim() != "")
                param5.Value = this.lcparamCategory.Trim();
            else
                param5.Value = "";
            if (this.lcparamitem_class.Trim() != "")
            {
                string[] chklcparamitem_class = lcparamitem_class.Split(',');
                string chklcparamitem = "";
                for (int j = 0; j < chklcparamitem_class.Length; j++)
                {
                    chklcparamitem += chklcparamitem_class[j].Trim() + ",";
                }
                chklcparamitem = chklcparamitem.Remove(chklcparamitem.LastIndexOf(','));
                param6.Value = chklcparamitem.Trim();
            }
            else
                param6.Value = "";
            return "S";
        }
        public DataSet SchedulingAnalysisReports_ApptDetails(Hashtable lcParameters)
        {
            //1
            string getValidMsg = SchedulingAnalysisReports(lcParameters);
            if (getValidMsg == "") return null;

            paramslist = new SqlParameter[] { param1, param2, param3, param5, param6 };

            GetDataSetFromSPV("STP_RP_AS_scheduleDetail", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
       
    
        #endregion Scheduling Analysis Reports

        # region screen reports

        private string Reports(Hashtable lcParam)
        {
            IDictionaryEnumerator myEnumerator = lcParam.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                BuildFldNames = myEnumerator.Key.ToString();
                BuildFldValues = myEnumerator.Value.ToString();

                if (BuildFldNames.ToUpper() == "LOCATION")
                {
                    if (BuildFldValues.Trim() == "")
                    {

                        SendErrMessage = "Location cannot be blank";
                        return "";
                    }
                    else
                    {
                        lcparamLocation = BuildFldValues;
                    }
                }

                if (BuildFldNames.ToUpper() == "MM_ACCOUNT.STATUS")
                {
                    lcstatus = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {

                        SendErrMessage = "Please enter Start Date";
                        return "";
                    }
                    else
                    {
                        lcparamSdate = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "ACCTNO")
                {
                    lcparamAcctNo = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "MEMBERID")
                {
                    lcparamAcctNo = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "EDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {

                        SendErrMessage = "Please enter End Date";
                        return "";
                    }
                    else
                    {
                        lcparamEdate = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "CATEGORY")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamCategory = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "ITEMNO")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamitemNo = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "CLASS")
                {
                    if (BuildFldValues.Trim() != "")
                        lcparamitem_class = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SALESPERSON")
                {
                    if (BuildFldValues.Trim() != "")
                    {
                        lcparamsalesrep = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "STIME")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lcparamStime = BuildFldValues;
                }

                if (BuildFldNames.ToUpper() == "ETIME")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lcparamEtime = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "SM.ENTRYMACHINE")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lcparmEmachine = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "ASOFDATE")
                {
                    if (BuildFldValues.Trim() == "")
                    {
                        SendErrMessage = "Please enter Date";
                        return "";
                    }
                    else
                    {
                        param2 = new SqlParameter("@stdate", DbType.DateTime); //CreateDataParameter("@stdate", DbType.DateTime, 8);
                        param2.Value = BuildFldValues;
                        param3 = new SqlParameter("@enddate", DbType.DateTime); //CreateDataParameter("@enddate", DbType.DateTime, 8);
                        param3.Value = BuildFldValues;
                        lcparamEdate = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "CHKHISTORY")
                {
                    if (BuildFldValues.Trim() != "")
                        this.lchkHistory = BuildFldValues;
                }


            }

            //copied
            string[] chk_location = lcparamLocation.Split(',');
            PayLoc = "";
            if (chk_location.Length > 0)
            {
                for (int i = 0; i <= chk_location.Length - 1; i++)
                {
                    PayLoc += chk_location[i].ToString() + ",";
                }
                PayLoc = PayLoc.Remove(PayLoc.Length - 1, 1);
            }
            else
                return "";

            string[] chk_category = lcparamCategory.Split(',');
            string cat = "";
            if (chk_category.Length > 0)
            {
                for (int i = 0; i <= chk_category.Length - 1; i++)
                {
                    if (i + 1 <= chk_category.Length - 1)
                        cat += "" + chk_category[i].ToString() + "','";
                    else
                        cat += "" + chk_category[i].ToString();
                }
            }
            lcparamCategory = cat;

            string[] chk_class = lcparamitem_class.Split(',');
            cat = "";
            if (chk_class.Length > 0)
            {
                for (int i = 0; i <= chk_class.Length - 1; i++)
                {
                    if (i + 1 <= chk_class.Length - 1)
                        cat += "" + chk_class[i].ToString() + "','";
                    else
                        cat += "" + chk_class[i].ToString();
                }
            }
            this.lcparamitem_class = cat;

            and = "  and  ";
            location = " and locationid in (" + PayLoc + ") ";
            if (lcparamCategory.ToString().Trim() != "")
                category = " and ic.group1 in ('" + this.lcparamCategory.ToString() + "') ";
            else
                category = "";
            if (this.lcparamitem_class.ToString() != "")
                iclass = " and class in ('" + this.lcparamitem_class.ToString() + "') ";
            else
                iclass = "";
            lwhere = " '" + this.lcparamSdate.ToString() + "' " + and + " '" + this.lcparamEdate.ToString() + "' " + location.Trim() + category + iclass;

            param1 = new SqlParameter("@lwhere", DbType.String); //CreateDataParameter("@lwhere", DbType.String, 1000);
            param3 = new SqlParameter("@edate", DbType.String); //CreateDataParameter("@edate", DbType.String, 1000);
            param4 = new SqlParameter("@eloc", DbType.String); //CreateDataParameter("@eloc", DbType.String, 1000);
            param5 = new SqlParameter("@edate", DbType.String); //CreateDataParameter("@edate", DbType.String, 1000);
            param6 = new SqlParameter("@lwhere", DbType.String); //CreateDataParameter("@lwhere", DbType.String, 1000);
            //end
            return "S";
        }
        public DataSet MemberInvoice(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            lcparamAcctno = lcParameters["ACCTNO"].ToString();
            param1.Value = this.lcparamAcctno;
            param2.Value = this.lcparamSdate;
            param3.Value = this.lcparamEdate;
            param4.Value = this.lchkHistory;
            param5.Value = 0;
            paramslist = new SqlParameter[] { param1, param2, param3, param4, param5 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_MM_History", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet MemberSalesReport(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            lcparamAcctno = lcParameters["acctno"].ToString();
            if (lcParameters["acctno"].ToString().Contains(","))
                lwhere = "'" + this.lcparamSdate.Trim() + "' and '" + this.lcparamEdate.Trim() + "' and mm.acctno in (" + lcparamAcctno.ToString() + ")";
           
            string csort = "''";
            param1 = new SqlParameter("@lwhere", DbType.String); //CreateDataParameter("@lwhere", DbType.String, 8000);
            param3 = new SqlParameter("@csort", DbType.String); //CreateDataParameter("@csort", DbType.String, 10);
            param1.Value = lwhere.Replace("''", "'");
            param3.Value = csort;
            paramslist = new SqlParameter[] { param1, param3 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_IC_SalesDetails", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet SchedulerCheckInvoices(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            param1.Value = this.lcparamSdate;
            param2.Value = this.lcparamEdate;
            param3.Value = this.lcparamLocation;
            paramslist = new SqlParameter[] { param1, param2, param3 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_AS_InvoiceCheck", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet InventoryOnHand(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            //copied
            param1.Value = this.lcparamLocation;
            param2.Value = this.lcparamEdate;
            paramslist = new SqlParameter[] { param1, param2 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_IC_ItemOnHand", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet BarcodeLabels(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            string strlocation = "", strbarcode = "", endbarcode = "";
            int count = 0;
            IDictionaryEnumerator myEnumerator = lcParameters.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                BuildFldNames = myEnumerator.Key.ToString();
                BuildFldValues = myEnumerator.Value.ToString();
                if (BuildFldNames.ToUpper() == "LOCATION")
                {
                    if (BuildFldValues.Trim() != "")
                    {
                        strlocation = BuildFldValues;
                    }
                }
                if (BuildFldNames.ToUpper() == "IC_ITEM.UPC")
                {
                    if (BuildFldValues.Trim() != "")
                        strbarcode = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "UPC")
                {
                    if (BuildFldValues.Trim() != "")
                        endbarcode = BuildFldValues;
                }
                if (BuildFldNames.ToUpper() == "COUNT")
                {
                    if (BuildFldValues.Trim() != "")
                        count = Convert.ToInt32(BuildFldValues);
                }
            }

            if (strlocation != "")
                lwhere = " locationid ='" + strlocation + "' and ";
            if ((strbarcode.Length > 0) && (endbarcode.Length > 0))
                lwhere += " ic_item.upc between '" + strbarcode + "' and '" + endbarcode + "'";
            else if ((strbarcode.Length > 0) && (endbarcode == ""))
                lwhere += " ic_item.upc between '" + strbarcode + "' and '" + endbarcode + "'";

            param1 = new SqlParameter("@lwhere", DbType.String); //CreateDataParameter("@lwhere", DbType.String, 8000);
            param2 = new SqlParameter("@Counts", DbType.Int32); //CreateDataParameter("@Counts", DbType.Int32, 4);

            //param1.Value = Vedas.vCommon.SQLString(lwhere);
            param1.Value = lwhere.Replace("''", "");
            param2.Value = count;
            paramslist = new SqlParameter[] { param1, param2 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_IC_InvoiceLabel", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet prospectJouralCallLog(Hashtable lcParameters)
        {
            string stDate = "", endDate = "";
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            if (lcParameters.Count != 0)
            {
                stDate = lcParameters["SDate"].ToString();
                endDate = lcParameters["EDate"].ToString();
                salesRep = lcParameters["salesRep"].ToString();
            }
            string sQry = "";
            if (salesRep.Trim() != "")
            {
                sQry = "select activityno,pt_activity.acctno,MM_Account.firstname + ' ' +MM_Account.lastname as name,ntype,pt_activity.notes,convert(varchar,pt_activity.entrydate,101) as entrydate,pt_activity.entryuser,salesrep from pt_activity ,MM_Account where MM_Account.acctno = pt_activity.acctno and  isnull(pt_activity.entryuser,'')='" + objVedas.nusername + "' and  isnull(salesrep,'')='" + salesRep + "' and pt_activity.entrydate between '" + stDate.ToString() + " 00:00:00' and '" + endDate.ToString() + " 23:58:59' and ntype in (select entry from op_lookup where lookupid = 'PROSNOTES')";
            }
            else
            {
                sQry = "select activityno,pt_activity.acctno,firstname + ' ' +lastname as name,ntype,notes,convert(varchar,pt_activity.entrydate,101) as entrydate,pt_activity.entryuser,salesrep from pt_activity ,MM_Account where MM_Account.acctno = pt_activity.acctno  and pt_activity.entrydate between '" + stDate.ToString() + " 00:00:00' and '" + endDate.ToString() + " 23:58:59' and ntype in (select entry from op_lookup where lookupid = 'PROSNOTES')";
            }
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV(sQry, "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet prospectCallList(Hashtable lcParameters)
        {
            string stDate = "", endDate = "", Clubid = "";
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            if (lcParameters.Count != 0)
            {
                stDate = lcParameters["SDate"].ToString();
                endDate = lcParameters["EDate"].ToString();
                salesRep = lcParameters["salesRep"].ToString();
            }
            string sQry = "select mm_account.*,descript = ( select top 1 descript from op_lookup where lookupid ='PROSTYPE' and entry = mm_prospectaccount.prostype),Name,'' as FillProsType,'' as ProsQuickNotes,'' as longnotes,'' as nexteditcalldate,salesrep,'" + stDate + " 'as FromDate,'" + endDate + "' as EndDate,'" + objVedas.nusername + "' as EntryUser,'" + Clubid.ToString() + "' as clubname,'" + objVedas.gLocation.ToString() + "' as locationid  from mm_account,mm_prospectaccount";
            if (salesRep.Trim() != "")
            {
                sQry = sQry + " where mm_account.acctno=mm_prospectaccount.acctno and nextcalldate between '" + stDate + " 00:00:00' and '" + endDate + " 23:58:00' and  salesrep='" + salesRep.ToString() + "'  and isnull(nextcalldate,'')<>'' order by nextcalldate desc";
            }
            else
            {
                sQry = sQry + " where mm_account.acctno=mm_prospectaccount.acctno and nextcalldate between '" + stDate + " 00:00:00' and '" + endDate + " 23:58:00' and salesrep='" + objVedas.nusername.ToString() + "' and isnull(nextcalldate,'')<>'' order by nextcalldate desc";
            }
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV(sQry, "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet CheckInReport(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            lwhere = " co.locationid = '" + this.lcparamLocation.ToString() + "' ";
            lwhere += "and Checkin  between '" + this.lcparamSdate.Trim().ToString() + "'  and  '" + this.lcparamEdate.Trim().ToString() + "'";
            param1.Value = lwhere;
            paramslist = new SqlParameter[] { param1 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_MM_Checkinout", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet InvoicePaymentTrackingReport(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            param1.Value = lcParameters["SDate"].ToString();
            param2.Value = lcParameters["EDate"].ToString();
            string payloc = lcParameters["LOCATION"].ToString();
            string invloc = lcParameters["INVOICE LOCATION"].ToString();
            param3.Value = payloc.Replace("'", "");
            param4.Value = invloc.Replace("'", "");
            paramslist = new SqlParameter[] { param1, param2, param3, param4 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_RP_AR_Tracking", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }
        public DataSet AttendanceReport(Hashtable lcParameters)
        {
            string getValidMsg = Reports(lcParameters);
            if (getValidMsg == "") return null;
            string Program = "", Groups = "", Session = "";
            param1.Value = lcParameters["LOCATION"].ToString();
            if (lcParameters["Program"] != null && lcParameters["Program"].ToString() != "")
                Program = lcParameters["Program"].ToString();
            if (lcParameters["Groups"] != null && lcParameters["Groups"].ToString() != "")
                Groups = lcParameters["Groups"].ToString();
            if (lcParameters["Session"] != null && lcParameters["Session"].ToString() != "")
                Session = lcParameters["Session"].ToString();
            param2.Value = Program.ToString();
            param3.Value = Groups.ToString();
            param4.Value = Session.ToString();
            paramslist = new SqlParameter[] { param1, param2, param3, param4 };
            if (this.DsCustom.Tables["Results"] != null) this.DsCustom.Tables["Results"].Clear();
            GetDataSetFromSPV("STP_CM_Attendance", "Results", ref DsCustom, ref DaCustom, paramslist, oConn);
            return DsCustom;
        }

        #endregion

        #region HR Reports
        
        
        #endregion

        #region Custom Functions

        private DataSet GetDataSetFromSPV(string lSql, string tblname, ref DataSet lds, ref SqlDataAdapter lCmd, SqlParameter[] lparams, SqlConnection tConn)
        {
            SqlDataAdapter da = (SqlDataAdapter)lCmd;
            //tConn = vDBProvider.CreateConnection(_gConnectString); 
            if (tConn.State == 0) tConn.Open();
            da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(lSql, tConn);
            lCmd = (SqlDataAdapter)da;
            lCmd.SelectCommand = new SqlCommand(lSql, tConn);
            lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
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
            return lds;
        }

        public SqlDataAdapter CreateDataAdapter()
        {
            SqlDataAdapter da = null;
            try
            {
                da = (SqlDataAdapter)Activator.CreateInstance(_dataAdapterTypes[(int)_provider]);

            }
            catch (TargetInvocationException e)
            {
                throw new SystemException(e.InnerException.Message, e.InnerException);
            }
            return da;
        }

        public SqlCommand CreateCommand(string cmdText, IDbConnection connection)
        {
            SqlCommand cmd = null;
            if (connection.State == 0) connection.Open();
            object[] args = { cmdText, connection };

            try
            {
                cmd = (SqlCommand)Activator.CreateInstance(_commandTypes[(int)_provider], args);
            }
            catch (TargetInvocationException e)
            {
                //throw new SystemException(e.InnerException.Message, e.InnerException);
            }

            return cmd;
        }

        public enum ProviderType
        {
            /// <summary>
            /// The OLE DB (<see cref="System.Data.OleDb"/>) .NET data provider.
            /// </summary>
            OleDb = 0,
            /// <summary>
            /// The SQL Server (<see cref="System.Data.SqlClient"/>) .NET data provider.
            /// </summary>
            SqlClient = 1,

            /// <summary>
            /// The SQLLite Database (<see cref="System.Data.SQLite"/>) .NET data provider.
            /// </summary>
            SqlLite = 2,
            /// <summary>
            /// The SQL Server (<see cref="System.Data.SqlClient"/>) .NET data provider.
            /// </summary>
            Oracle = 3,
            /// <summary>
            /// The SQL Server (<see cref="System.Data.SqlClient"/>) .NET data provider.
            /// </summary>
            ODBC = 4

        };

        public SqlParameter CreateDataParameter()
        {
            SqlParameter param = null;

            try
            {
                param = (SqlParameter)Activator.CreateInstance(_dataParameterTypes[(int)_provider]);
            }
            catch (TargetInvocationException e)
            {
                throw new SystemException(e.InnerException.Message, e.InnerException);
            }

            return param;
        }

        public SqlParameter CreateDataParameter(string parameterName, DbType dataType)
        {
            SqlParameter param = CreateDataParameter();

            if (param != null)
            {
                param.ParameterName = parameterName;
                param.DbType = dataType;
            }

            return param;
        }

        public SqlParameter CreateDataParameter(string parameterName, DbType dataType, int size)
        {
            SqlParameter param = CreateDataParameter();

            if (param != null)
            {
                param.ParameterName = parameterName;
                param.DbType = dataType;
                param.Size = size;
            }

            return param;
        }

        public SqlConnection CreateConnection(string connectionString)
        {
            SqlConnection conn = null;
            object[] args = { connectionString };

            try
            {
                conn = (SqlConnection)Activator.CreateInstance(_connectionTypes[(int)_provider], args);
                //conn.Open(); 
            }
            catch (TargetInvocationException e)
            {
                throw new SystemException(e.InnerException.Message, e.InnerException);
            }

            return conn;
        }

        public string GetConfigValue(string FieldName)
        {
            //var opconfig = (from m in cmsservice.GetOP_Config() where m.Parameter == FieldName select m).SingleOrDefault();
            //if (opconfig != null) return opconfig.Value.ToString();
            //else 
             return "";
        }

        #endregion
    }
}
