using ExportToExcel;
using GroGroup.Class;
using GroGroup.Filters;
using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GroGroup.Controllers
{
    [CustomActionFilter]
    [CustomActionException]
    public class UtilityController : Controller
    {
        string sqlcon = System.Configuration.ConfigurationManager.AppSettings["GroGroup"].ToString();
        string fileName = "";
        SqlDataAdapter da = new SqlDataAdapter();
        DataSet ds = new DataSet();
        Aptus Ggrp = new Aptus();
        SqlConnection Sconn;

        #region Update P & S Data

        public ActionResult UpdateData()
        {
            string DefYrMonth = "";
            DateTime DefYearMonth;
            DefYearMonth = DateTime.Now.AddMonths(-1);
            DefYrMonth = DefYearMonth.ToString("yyyyMM");
            ViewBag.DefYrMonth = DefYrMonth;
            return View();
        }

        public ActionResult ProcessUpdatePSData(string ym)
        {
            string sYear = "", sMonth = "", msg = "";
            sYear = ym.Substring(0, 4);
            sMonth = ym.Substring(4, 2);

            List<Hashtable> fileList = new List<Hashtable>();
            List<Hashtable> StausFileList = new List<Hashtable>();

            if (Directory.Exists(ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth))
            {
                string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth);
                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        Hashtable obj = new Hashtable();
                        obj["fname"] = Path.GetFileName(files[i].ToString());
                        fileList.Add(obj);

                        //Staus File List
                        obj["filename"] = Path.GetFileName(files[i].ToString());
                        obj["usermsg"] = ""; obj["useraction"] = ""; obj["status"] = ""; obj["rows"] = "";
                        StausFileList.Add(obj);

                    }
                }
            }
            else
                msg = "File not found, Please upload a file.";

            return Json(new { Items = fileList, msg = msg, StausFileList = StausFileList });

        }

        public ActionResult UpdatePSData(string ym, string fname)
        {
            string cFileName = "";
            string msg = "", info = "", fext = "";
            string sYear = "", sMonth = "";
            bool isDelimiter = false;
            string fpath = "";

            sYear = ym.Substring(0, 4);
            sMonth = ym.Substring(4, 2);

            try
            {
                fpath = ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth + "\\" + fname;

                fileName = fpath;
                string _Ext = System.IO.Path.GetExtension(fileName).ToLower();
                cFileName = fileName;
                fext = _Ext;

                if (_Ext == ".sls" || _Ext == ".pur")
                {
                    _Ext = ".csv";
                    string Value = "UpdateData" + "_CC_" + DateTime.Now.ToString();
                    fileName = Value.ToString().Replace("/", "").Replace(":", "").Replace("-", "").Trim() + _Ext;
                    cFileName = fileName;
                    string path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + fileName;

                    var fileConts = System.IO.File.ReadAllLines(fpath);

                    if (fileConts != null && fileConts.Length > 0)
                    {
                        isDelimiter = fileConts[0].Contains("\"");

                        if (isDelimiter)
                        {
                            var fileContents = System.IO.File.ReadAllBytes(fpath);
                            var sr1 = new FileStream(path, FileMode.Create);
                            sr1.Write(fileContents, 0, fileContents.Length);
                            sr1.Close();
                            sr1.Dispose();

                            DataSet CheckDs = ConvertSlsFileToDataset(cFileName);
                            if (CheckDs != null && CheckDs.Tables[0] != null && CheckDs.Tables[0].Rows.Count > 0)
                            {
                                string SQL = "", yearmonth = "", distid = "";
                                yearmonth = CheckDs.Tables[0].Rows[0][1].ToString().Trim();
                                distid = CheckDs.Tables[0].Rows[0][0].ToString().Trim();

                                string syrmn = "";
                                syrmn = ym.Substring(2);
                                if (syrmn != yearmonth)
                                    info = "year month not match";
                                else
                                {
                                    if (fext.Trim().ToUpper() == ".SLS")
                                        SQL = "select top 1 yearmonth from Sales where yearmonth = cast('20" + yearmonth + "' as int) and distid='" + distid + "'";
                                    else
                                        SQL = "select top 1 yearmonth from Purchase where yearmonth = cast('20" + yearmonth + "' as int) and distid='" + distid + "'";
                                    SqlConnection Sconn = new SqlConnection(sqlcon);
                                    DataSet ds = new DataSet();
                                    SqlDataAdapter da = new SqlDataAdapter(SQL, Sconn);
                                    da.Fill(ds, "chkdata");
                                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                        info = "Data exists";
                                }
                            }
                            else
                                info = "No records found.";
                        }
                        else
                        {

                            string line = fileConts[0].ToString();
                            if (line.Length > 5)
                            {
                                string strdistid = line.Substring(0, 6).TrimEnd('~');
                                string strym = line.Substring(6, 4).TrimEnd('~');

                                string SQL = "", yearmonth = "", distid = "";
                                yearmonth = strym;
                                distid = strdistid;

                                string syrmn = "";
                                syrmn = ym.Substring(2);
                                if (syrmn != yearmonth)
                                    info = "year month not match";
                                else
                                {
                                    if (fext.Trim().ToUpper() == ".SLS")
                                        SQL = "select top 1 yearmonth from Sales where yearmonth = cast('20" + yearmonth + "' as int) and distid='" + distid + "'";
                                    else
                                        SQL = "select top 1 yearmonth from Purchase where yearmonth = cast('20" + yearmonth + "' as int) and distid='" + distid + "'";
                                    SqlConnection Sconn = new SqlConnection(sqlcon);
                                    DataSet ds = new DataSet();
                                    SqlDataAdapter da = new SqlDataAdapter(SQL, Sconn);
                                    da.Fill(ds, "chkdata");
                                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                        info = "Data exists";
                                }
                            }

                        }
                    }
                    else
                        info = "No records found.";

                }
                else
                    msg = "Invalid file format";

                return Json(new { fileName = cFileName, msg = msg, info = info, Ext = fext, isDelimiter = isDelimiter, fpath = fpath });
            }
            catch (Exception ex)
            {
                return Json(new { fileName = cFileName, msg = ex.Message, info = info, Ext = fext, isDelimiter = isDelimiter, fpath = fpath });
            }


        }

        private DataSet ConvertSlsFileToDataset(string fileName)
        {
            string _Path = "", _FullPath = "";
            _Path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString();
            _FullPath = _Path + "\\" + fileName;
            string excelConnectionString = "";

            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _Path + ";Extended Properties=\"text;HDR=No;FMT=CsvDelimited;\"";
            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            excelConnection.Open();
            OleDbCommand cmd1 = new OleDbCommand("SELECT * FROM [" + Path.GetFileName(_FullPath) + "]", excelConnection);
            OleDbDataAdapter oleda = new OleDbDataAdapter();
            oleda.SelectCommand = cmd1;
            DataSet ds = new DataSet();
            oleda.Fill(ds, "UpdData");
            excelConnection.Dispose();

            //Delete

            return ds;
        }

        public ActionResult DeleteTempCsvFile(string fileName)
        {
            try
            {
                string _Path = "", _FullPath = "";
                _Path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString();
                _FullPath = _Path + "\\" + fileName;

                if (System.IO.File.Exists(_FullPath))
                    System.IO.File.Delete(_FullPath);
            }
            catch (Exception) { }
            return Json(new { Items = "" });
        }

        public ActionResult UpdateSalesImport(string fileName, bool isDelimiter, string fpath, string ym = "")
        {
            int badCount = 0, RowCount = 0;
            string ErrMsg = "";
            bool recordcount = false;

            string sYear = "", sMonth = "";
            sYear = ym.Substring(0, 4);
            sMonth = ym.Substring(4, 2);

            DataSet SalesDs = new DataSet();
            DataSet lds = new DataSet();
            //int FoundLineNo = 0;
            try
            {
                /*
                var table = new DataTable();
                var fileContents = System.IO.File.ReadAllLines(ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + fileName);
                var splitFileContents = (from f in fileContents select f.Split(',')).ToArray();
                int maxLength = (from s in splitFileContents select s.Count()).Max();

                for (int i = 0; i < maxLength; i++)
                {
                    table.Columns.Add();
                }

                foreach (var line in splitFileContents)
                {
                    DataRow row = table.NewRow();
                    row.ItemArray = (object[])line;
                    table.Rows.Add(row);
                }*/

                //SalesDs.Tables.Add(table);

                if (isDelimiter)
                {
                    SalesDs = ConvertSlsFileToDataset(fileName);

                    //Delete temp csv delimiter file
                    try
                    {
                        string _Path = "", _FullPath = "";
                        _Path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString();
                        _FullPath = _Path + "\\" + fileName;

                        if (System.IO.File.Exists(_FullPath))
                            System.IO.File.Delete(_FullPath);
                    }
                    catch (Exception) { }
                }
                else
                {
                    var fileConts = System.IO.File.ReadAllLines(fpath);
                    DataTable dt = new DataTable();
                    dt.Columns.Add("distid");
                    dt.Columns.Add("ym");
                    dt.Columns.Add("upc");
                    dt.Columns.Add("Qty");
                    dt.Columns.Add("Qty_DC");
                    dt.Columns.Add("Sales");
                    dt.Columns.Add("Sales_DC");
                    dt.Columns.Add("qty_oh");
                    dt.Columns.Add("qty_comm");
                    dt.Columns.Add("mfg_item");
                    dt.Columns.Add("trcode");

                    for (int l = 0; l < fileConts.Length; l++)
                    {
                        string line = fileConts[l].ToString();

                        if (line.Length > 5)
                        {
                            //FoundLineNo = l;
                            //if(FoundLineNo == 2885)
                            //{
                            //    line = fileConts[2885].ToString();
                            //    line = fileConts[2886].ToString();
                            //}

                            if (line.Length < 65) //check for TXBWIC distid
                            {
                                string fline = "", sline = "", fullline = "";
                                fline = fileConts[l].ToString() + " ";
                                sline = fileConts[l + 1].ToString();
                                fullline = fline + sline;
                                line = fullline;
                                l++;
                            }

                            string strdistid = line.Substring(0, 6).TrimEnd('~');
                            string strym = line.Substring(6, 4).TrimEnd('~');
                            string strupc = line.Substring(10, 15).TrimEnd('~');
                            string strqty = line.Substring(25, 7).TrimEnd('~');
                            string strqty_dc = line.Substring(32, 1).TrimEnd('~');
                            string strsales = line.Substring(33, 7).TrimEnd('~');
                            string strsales_dc = line.Substring(40, 1).TrimEnd('~');
                            string strqty_oh = line.Substring(41, 5).TrimEnd('~');
                            string strqty_comm = line.Substring(46, 5).TrimEnd('~');
                            string strmfg_item = line.Substring(51, 10).TrimEnd('~');
                            string strtrcode = line.Substring(61, 4).TrimEnd('~');

                            //replace
                            strupc = strupc.Replace("-", "");

                            DataRow dr = dt.NewRow();
                            dr["distid"] = strdistid; dr["ym"] = strym;
                            dr["upc"] = strupc; dr["qty"] = strqty;
                            dr["qty_dc"] = strqty_dc; dr["sales"] = strsales;
                            dr["sales_dc"] = strsales_dc; dr["qty_oh"] = strqty_oh;
                            dr["qty_comm"] = strqty_comm; dr["mfg_item"] = strmfg_item; dr["trcode"] = strtrcode;

                            dt.Rows.Add(dr);
                        }

                    }
                    SalesDs.Tables.Add(dt);
                }

                if (SalesDs != null && SalesDs.Tables.Count > 0 && SalesDs.Tables[0] != null)
                    SalesDs.Tables[0].TableName = "SalesImport";
                else
                {
                    ErrMsg = "No records found in sls file";
                    return Json(new { Items = badCount, Msg = ErrMsg, rec = recordcount, RowCount = RowCount });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Items = badCount, Msg = ex.Message, rec = recordcount, RowCount = RowCount });
            }

            try
            {
                recordcount = true;
                DataTable dt = new DataTable();
                dt.TableName = "SalesImport";
                dt.Columns.Add("distid");
                dt.Columns.Add("ym");
                dt.Columns.Add("upc");
                dt.Columns.Add("Qty");
                dt.Columns.Add("Qty_DC");
                dt.Columns.Add("Sales");
                dt.Columns.Add("Sales_DC");
                dt.Columns.Add("qty_oh");
                dt.Columns.Add("qty_comm");
                dt.Columns.Add("mfg_item");
                dt.Columns.Add("trcode");

                for (int i = 0; i < SalesDs.Tables["SalesImport"].Rows.Count; i++)
                {
                    if (SalesDs.Tables["SalesImport"].Rows[i][0] != DBNull.Value && SalesDs.Tables["SalesImport"].Rows[i][0].ToString().Trim() != "")
                    {
                        int Qty = 0, Sales = 0, qty_oh = 0, qty_comm = 0;

                        int.TryParse(SalesDs.Tables["SalesImport"].Rows[i][3].ToString().Trim(), out Qty);
                        int.TryParse(SalesDs.Tables["SalesImport"].Rows[i][7].ToString().Trim(), out qty_oh);
                        int.TryParse(SalesDs.Tables["SalesImport"].Rows[i][8].ToString().Trim(), out qty_comm);

                        double dbl;
                        string value = SalesDs.Tables["SalesImport"].Rows[i][5].ToString().Trim();
                        double.TryParse(value, out dbl);
                        Sales = Convert.ToInt32(dbl);

                        DataRow dr;
                        dr = dt.NewRow();
                        dr["distid"] = SalesDs.Tables["SalesImport"].Rows[i][0].ToString().Trim();
                        dr["ym"] = SalesDs.Tables["SalesImport"].Rows[i][1].ToString().Trim();

                        //replace
                        string strupc = SalesDs.Tables["SalesImport"].Rows[i][2].ToString().Trim().Replace("-", "");

                        dr["upc"] = strupc;
                        dr["Qty"] = Qty;
                        dr["Qty_DC"] = SalesDs.Tables["SalesImport"].Rows[i][4].ToString().Trim();
                        dr["Sales"] = Sales;
                        dr["Sales_DC"] = SalesDs.Tables["SalesImport"].Rows[i][6].ToString().Trim();
                        dr["qty_oh"] = qty_oh;
                        dr["qty_comm"] = qty_comm;
                        dr["mfg_item"] = SalesDs.Tables["SalesImport"].Rows[i][9].ToString().Trim();
                        dr["trcode"] = SalesDs.Tables["SalesImport"].Rows[i][10].ToString().Trim();
                        dt.Rows.Add(dr);
                    }
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    string lSql = "";
                    SqlConnection tConn = new SqlConnection(sqlcon);
                    SqlDataAdapter lCmd;

                    lSql = "STP_SalesImport";
                    lCmd = new SqlDataAdapter(lSql, tConn);
                    lCmd.SelectCommand = new SqlCommand(lSql, tConn);
                    lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
                    lCmd.SelectCommand.Parameters.Clear();
                    lCmd.SelectCommand.Parameters.AddWithValue("@SlsImp", dt);
                    lCmd.SelectCommand.CommandTimeout = 240;
                    lCmd.Fill(lds, "tblSlsImp");

                    if (lds != null && lds.Tables["tblSlsImp"] != null && lds.Tables["tblSlsImp"].Rows.Count > 0)
                    {
                        int.TryParse(lds.Tables["tblSlsImp"].Rows[0][0].ToString(), out RowCount);
                        int.TryParse(lds.Tables["tblSlsImp"].Rows[0][1].ToString(), out badCount);
                    }

                    //if (lds != null && lds.Tables["tblSlsImp1"] != null && lds.Tables["tblSlsImp1"].Rows.Count > 0)
                    //{
                    //    int.TryParse(lds.Tables["tblSlsImp1"].Rows[0][0].ToString(), out badCount);
                    //}

                    if (lds != null && lds.Tables["tblSlsImp1"] != null && lds.Tables["tblSlsImp1"].Rows.Count > 0)
                    {
                        DataTable dtSlsErr = lds.Tables["tblSlsImp1"];
                        Session["dtSlsErr"] = dtSlsErr;
                    }

                    //move processed file
                    if (!string.IsNullOrEmpty(fpath) && !string.IsNullOrEmpty(sYear) && !string.IsNullOrEmpty(sMonth))
                    {
                        if (System.IO.File.Exists(fpath))
                        {
                            string sourceFileName = "", destFileName = "";
                            sourceFileName = Path.GetFileName(fpath);

                            string sExt = System.IO.Path.GetExtension(fpath).ToLower();
                            string[] sfarr = sourceFileName.Split('.');

                            if (sExt.Trim().ToUpper() == ".SLS")
                            {
                                destFileName = sfarr[0] + "_" + DateTime.Now.ToString("MM-dd-yyyy") + ".psls";
                            }
                            else if (sExt.Trim().ToUpper() == ".PUR")
                            {
                                destFileName = sfarr[0] + "_" + DateTime.Now.ToString("MM-dd-yyyy") + ".ppur";
                            }

                            string movepath = ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth + "\\ARCHIVE\\";
                            if (!Directory.Exists(movepath))
                            {
                                Directory.CreateDirectory(movepath);
                            }

                            if (System.IO.File.Exists(movepath + destFileName))
                                System.IO.File.Delete(movepath + destFileName);
                            System.IO.File.Move(fpath, movepath + destFileName);

                        }
                    }
                }
                else
                {
                    ErrMsg = "No records found";
                }

            }
            catch (Exception ex)
            {
                string ErrDesc = "";
                if (ex.InnerException != null && ex.InnerException.Message != "")
                    ErrDesc = ex.InnerException.Message;
                else
                    ErrDesc = ex.Message;

                if (ErrDesc.Contains("Cannot find column"))
                {
                    ErrDesc = "Column missing. Make sure all columns like this order (distid,ym,upc,Qty,Qty_DC,Sales,Sales_DC,qty_oh,qty_comm,mfg_item,trcode).";
                }

                return Json(new { Items = badCount, Msg = ErrDesc, rec = false, fileName = fileName, RowCount = RowCount });
            }

            return Json(new { Items = badCount, Msg = ErrMsg, rec = recordcount, fileName = fileName, RowCount = RowCount });

        }

        public ActionResult UpdatePurchaseImport(string fileName, bool isDelimiter, string fpath, string ym = "")
        {
            int badCount = 0, RowCount = 0;
            string ErrMsg = "";
            bool recordcount = false;

            string sYear = "", sMonth = "";
            sYear = ym.Substring(0, 4);
            sMonth = ym.Substring(4, 2);

            DataSet PurchDs = new DataSet();
            DataSet lds = new DataSet();

            try
            {

                if (isDelimiter)
                {
                    PurchDs = ConvertSlsFileToDataset(fileName);

                    //Delete temp csv delimiter file
                    try
                    {
                        string _Path = "", _FullPath = "";
                        _Path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString();
                        _FullPath = _Path + "\\" + fileName;

                        if (System.IO.File.Exists(_FullPath))
                            System.IO.File.Delete(_FullPath);
                    }
                    catch (Exception) { }
                }
                else
                {

                    var fileConts = System.IO.File.ReadAllLines(fpath);

                    DataTable dt = new DataTable();
                    dt.Columns.Add("distid"); dt.Columns.Add("ym"); dt.Columns.Add("mfgupc"); dt.Columns.Add("purchase"); dt.Columns.Add("mfgname"); dt.Columns.Add("trcode");

                    for (int l = 0; l < fileConts.Length; l++)
                    {
                        string line = fileConts[l].ToString();

                        if (line.Length > 5)
                        {
                            string strdistid = line.Substring(0, 6).TrimEnd('~');
                            string strym = line.Substring(6, 4).TrimEnd('~');
                            string strmfgupc = line.Substring(10, 7).TrimEnd('~');
                            string strpurchase = line.Substring(17, 7).TrimEnd('~');
                            string strmfgname = line.Substring(24, 30).TrimEnd('~');
                            string strtrcode = line.Substring(54, 4).TrimEnd('~');

                            //replace
                            strmfgupc = strmfgupc.Replace("-", "");

                            DataRow dr = dt.NewRow();
                            dr["distid"] = strdistid; dr["ym"] = strym; dr["mfgupc"] = strmfgupc; dr["purchase"] = strpurchase; dr["mfgname"] = strmfgname; dr["trcode"] = strtrcode;

                            dt.Rows.Add(dr);
                        }

                    }

                    PurchDs.Tables.Add(dt);

                }

                if (PurchDs != null && PurchDs.Tables.Count > 0 && PurchDs.Tables[0] != null)
                    PurchDs.Tables[0].TableName = "PurchaseImport";
                else
                {
                    ErrMsg = "No records found in sls file";
                    return Json(new { Items = badCount, Msg = ErrMsg, rec = recordcount, RowCount = RowCount });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Items = badCount, Msg = ex.Message, rec = recordcount, RowCount = RowCount });
            }

            try
            {
                recordcount = true;
                DataTable dt = new DataTable();
                dt.TableName = "PurchaseImport";
                dt.Columns.Add("distid");
                dt.Columns.Add("ym");
                dt.Columns.Add("mfgupc");
                dt.Columns.Add("purchase");
                dt.Columns.Add("mfgname");
                dt.Columns.Add("trcode");

                for (int i = 0; i < PurchDs.Tables["PurchaseImport"].Rows.Count; i++)
                {
                    if (PurchDs.Tables["PurchaseImport"].Rows[i][0] != DBNull.Value && PurchDs.Tables["PurchaseImport"].Rows[i][0].ToString().Trim() != "")
                    {
                        int Purchase = 0;

                        double dbl;
                        string value = PurchDs.Tables["PurchaseImport"].Rows[i][3].ToString().Trim();
                        double.TryParse(value, out dbl);
                        Purchase = Convert.ToInt32(dbl);

                        DataRow dr;
                        dr = dt.NewRow();
                        dr["distid"] = PurchDs.Tables["PurchaseImport"].Rows[i][0].ToString().Trim();
                        dr["ym"] = PurchDs.Tables["PurchaseImport"].Rows[i][1].ToString().Trim();

                        //replace
                        string strmfgupc = PurchDs.Tables["PurchaseImport"].Rows[i][2].ToString().Trim().Replace("-", "");

                        dr["mfgupc"] = strmfgupc;
                        dr["purchase"] = Purchase;
                        dr["mfgname"] = PurchDs.Tables["PurchaseImport"].Rows[i][4].ToString().Trim();
                        dr["trcode"] = PurchDs.Tables["PurchaseImport"].Rows[i][5].ToString().Trim();
                        dt.Rows.Add(dr);
                    }
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    string lSql = "";
                    SqlConnection tConn = new SqlConnection(sqlcon);
                    SqlDataAdapter lCmd;

                    lSql = "STP_PurchaseImport";
                    lCmd = new SqlDataAdapter(lSql, tConn);
                    lCmd.SelectCommand = new SqlCommand(lSql, tConn);
                    lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
                    lCmd.SelectCommand.Parameters.Clear();
                    lCmd.SelectCommand.Parameters.AddWithValue("@purImp", dt);
                    lCmd.SelectCommand.CommandTimeout = 240;
                    lCmd.Fill(lds, "tblPurImp");

                    if (lds != null && lds.Tables["tblPurImp"] != null && lds.Tables["tblPurImp"].Rows.Count > 0)
                    {
                        int.TryParse(lds.Tables["tblPurImp"].Rows[0][0].ToString(), out RowCount);
                        int.TryParse(lds.Tables["tblPurImp"].Rows[0][1].ToString(), out badCount);
                    }

                    //if (lds != null && lds.Tables["tblPurImp1"] != null && lds.Tables["tblPurImp1"].Rows.Count > 0)
                    //{
                    //    int.TryParse(lds.Tables["tblPurImp1"].Rows[0][0].ToString(), out badCount);
                    //}

                    if (lds != null && lds.Tables["tblPurImp1"] != null && lds.Tables["tblPurImp1"].Rows.Count > 0)
                    {
                        DataTable dtPurErr = lds.Tables["tblPurImp1"];
                        Session["dtPurErr"] = dtPurErr;
                    }

                    //move processed file
                    if (!string.IsNullOrEmpty(fpath) && !string.IsNullOrEmpty(sYear) && !string.IsNullOrEmpty(sMonth))
                    {
                        if (System.IO.File.Exists(fpath))
                        {
                            string sourceFileName = "", destFileName = "";
                            sourceFileName = Path.GetFileName(fpath);

                            string sExt = System.IO.Path.GetExtension(fpath).ToLower();
                            string[] sfarr = sourceFileName.Split('.');

                            if (sExt.Trim().ToUpper() == ".SLS")
                            {
                                destFileName = sfarr[0] + "_" + DateTime.Now.ToString("MM-dd-yyyy") + ".psls";
                            }
                            else if (sExt.Trim().ToUpper() == ".PUR")
                            {
                                destFileName = sfarr[0] + "_" + DateTime.Now.ToString("MM-dd-yyyy") + ".ppur";
                            }

                            string movepath = ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth + "\\ARCHIVE\\";
                            if (!Directory.Exists(movepath))
                            {
                                Directory.CreateDirectory(movepath);
                            }

                            if (System.IO.File.Exists(movepath + destFileName))
                                System.IO.File.Delete(movepath + destFileName);
                            System.IO.File.Move(fpath, movepath + destFileName);

                        }
                    }
                }
                else
                {
                    ErrMsg = "No records found";
                }

            }
            catch (Exception ex)
            {
                string ErrDesc = "";
                if (ex.InnerException != null && ex.InnerException.Message != "")
                    ErrDesc = ex.InnerException.Message;
                else
                    ErrDesc = ex.Message;

                if (ErrDesc.Contains("Cannot find column"))
                {
                    ErrDesc = "Column missing. Make sure all columns like this order (distid,ym,mfgupc,purchase,mfgname,trcode).";
                }

                return Json(new { Items = badCount, Msg = ErrDesc, rec = false, fileName = fileName, RowCount = RowCount });
            }

            return Json(new { Items = badCount, Msg = ErrMsg, rec = recordcount, fileName = fileName, RowCount = RowCount });

        }

        public ActionResult CreatePSErrorFile(string type)
        {
            bool blnFlag = false;
            string filename = "", filepath = "";
            DataSet dsExport = new DataSet();
            DataTable dtImp = new DataTable();
            DataTable dtImpCopy = new DataTable();

            if (type.Trim().ToUpper() == "SALES")
            {
                dtImp = (DataTable)Session["dtSlsErr"];
                filename = "SalesData_" + Session["initials"] + "_" + DateTime.Now.ToString("yyyMMddhhmmssffffff") + ".xlsx";
            }
            else
            {
                dtImp = (DataTable)Session["dtPurErr"];
                filename = "PurchaseData_" + Session["initials"] + "_" + DateTime.Now.ToString("yyyMMddhhmmssffffff") + ".xlsx";
            }
            filepath = System.Configuration.ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + filename;

            dtImpCopy = dtImp.Copy();
            dsExport.Tables.Add(dtImpCopy);
            blnFlag = CreateExcelFile.CreateExcelDocument(dsExport, filepath);
            return Json(new { Items = filename });
        }

        public FileContentResult DownloadPSErrorFile(string fname)
        {
            string FileName = System.Configuration.ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + fname;

            System.IO.FileInfo file = new System.IO.FileInfo(FileName);
            if (file.Exists)
            {
                byte[] bytes = null;
                try
                {
                    bytes = System.IO.File.ReadAllBytes(FileName);
                    System.IO.File.Delete(FileName);

                }
                catch (Exception ex)
                {
                    string ErrDesc = (ex.InnerException != null && ex.InnerException.Message != null && ex.InnerException.Message.ToString().Trim() != "") ? ex.InnerException.Message : "";
                    if (string.IsNullOrEmpty(ErrDesc))
                        ErrDesc = ex.Message;
                    SqlConnection conn = new SqlConnection(sqlcon);
                    string message = ErrDesc;
                    if (message.Length > 300)
                        message = message.Replace("'", "''").Substring(0, 300);
                    else
                        message = message.Replace("'", "''");
                    string stack = ex.TargetSite.ToString();
                    string insertSQL = "insert into OP_ErrorLog(ErrSource,ErrDesc,ErrStackTrace,InnerErrSource,InnerErrDesc,InnerErrStackTrace,ErrLogdatetime,MachineName) values"
                           + "('GetDBFileFromServer','" + message + "','" + stack + "'"
                           + ",'" + ex.InnerException + "','','','" + DateTime.Now + "','')";
                    Ggrp.Execute(insertSQL, conn);

                }
                return File(bytes, "text/plain", fname);
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public ActionResult UploadPURSLSFile(HttpPostedFileBase[] uploadedfiles, string ym)
        {
            string sYear = "", sMonth = "", fileName = "", _Ext = "";
            try
            {
                sYear = ym.Substring(0, 4);
                sMonth = ym.Substring(4, 2);
                if (!Directory.Exists(ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth))
                {
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth);
                }

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];

                    if (file.ContentLength == 0)
                        continue;

                    fileName = Path.GetFileName(file.FileName);
                    _Ext = System.IO.Path.GetExtension(fileName).ToLower();

                    if (_Ext.Trim().ToUpper() == ".SLS" || _Ext.Trim().ToUpper() == ".PUR")
                    {
                        string path = ConfigurationManager.AppSettings["PDSDataPath"].ToString() + "\\" + sYear + "\\" + sMonth + "\\" + fileName;
                        file.SaveAs(path);
                    }
                    else
                    {
                        //msg = "not sls or pur file";
                    }
                }

                return Json(new { fileName = fileName, msg = "" });
            }
            catch (Exception ex)
            {
                return Json(new { fileName = fileName, msg = ex.Message });
            }
        }

        #endregion

        #region Dashboard

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult FillViewType()
        {
            List<SelectListItem> ViewTypeList = new List<SelectListItem>();
            ViewTypeList.Add(new SelectListItem
            {
                Text = "",
                Value = ""
            });
            ViewTypeList.Add(new SelectListItem
            {
                Text = "Monthly",
                Value = "Monthly"
            });
            ViewTypeList.Add(new SelectListItem
            {
                Text = "Yearly",
                Value = "Yearly"
            });
            ViewTypeList.Add(new SelectListItem
            {
                Text = "Custom",
                Value = "Custom"
            });
            string FromDate = "", ToDate = "";
            DateTime lDate1 = DateTime.Today;
            FromDate = "01/01/" + lDate1.Year.ToString();
            ToDate = "12/31/" + lDate1.Year.ToString();

            return Json(new { Items = ViewTypeList, FromDate = FromDate, ToDate = ToDate });
        }

        public ActionResult ViewTypeSelectionchange(string ViewType)
        {
            string dtFromdate = "", dtEndDate = "";
            DateTime lDate1 = DateTime.Today;
            switch (ViewType.Trim().ToUpper())
            {
                case "MONTHLY":
                case "CUSTOM":
                    lDate1 = lDate1.AddDays((lDate1.Day * -1) + 1);
                    dtFromdate = lDate1.ToString("MM/dd/yyy");
                    dtEndDate = lDate1.AddMonths(1).AddDays(-1).ToString("MM/dd/yyy");
                    break;

                case "YEARLY":
                    dtFromdate = "01/01/" + lDate1.Year.ToString();
                    dtEndDate = "12/31/" + lDate1.Year.ToString();
                    break;
            }
            return Json(new { dtFromdate = dtFromdate, dtEndDate = dtEndDate });
        }

        public ActionResult FillTreeView()
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            DataSet ds = new DataSet();


            string sql = "";
            string lWhere = "1=1";
            sql = @"Select distinct QueryGroup from OP_Dashboard where " + lWhere + ";Select QueryName,QueryGroup from OP_Dashboard where " + lWhere;

            SqlDataAdapter da = new SqlDataAdapter(sql, Sconn);
            da.Fill(ds, "FillTreeView");

            List<Hashtable> TreeViewMainList = new List<Hashtable>();

            for (int i = 0; i < ds.Tables["FillTreeView"].Rows.Count; i++)
            {
                Hashtable hashtable = new Hashtable();
                for (int j = 0; j < ds.Tables["FillTreeView"].Columns.Count; j++)
                {
                    hashtable[ds.Tables["FillTreeView"].Columns[j].ToString()] = ds.Tables["FillTreeView"].Rows[i][j];

                    if (j == (ds.Tables["FillTreeView"].Columns.Count - 1))
                    {
                        string itemval = ds.Tables["FillTreeView"].Rows[i]["QueryGroup"].ToString();

                        string filterstring = "QueryGroup='" + itemval + "'";
                        DataSet newdataset = new DataSet();
                        newdataset.Merge(ds.Tables["FillTreeView1"].Select(filterstring));

                        List<Hashtable> TreeViewSubList = new List<Hashtable>();
                        if (newdataset != null && newdataset.Tables["FillTreeView1"] != null && newdataset.Tables["FillTreeView1"].Rows.Count > 0)
                        {
                            for (int m = 0; m < newdataset.Tables["FillTreeView1"].Rows.Count; m++)
                            {
                                Hashtable SecondTable = new Hashtable();
                                for (int n = 0; n < newdataset.Tables["FillTreeView1"].Columns.Count; n++)
                                {
                                    SecondTable[newdataset.Tables["FillTreeView1"].Columns[n].ToString()] = newdataset.Tables["FillTreeView1"].Rows[m][n];
                                }
                                TreeViewSubList.Add(SecondTable);
                            }
                        }
                        if (ds.Tables["FillTreeView"].Columns.Contains("Group") == false)
                            ds.Tables["FillTreeView"].Columns.Add("Group");
                        hashtable[ds.Tables["FillTreeView"].Columns["Group"].ToString()] = TreeViewSubList;
                    }
                }
                TreeViewMainList.Add(hashtable);
            }


            return Json(new { Items = TreeViewMainList });
        }


        public ActionResult LoadAnalysisChart(string QueryName, string QueryNo, string FromDate, string ToDate)
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();

            string[] Header = new string[2];
            Hashtable ObjHashtable = new Hashtable();

            string sql = "", ArgumentDataMember = "", PiechTitle = "", TableName = "";
            ArrayList ChartGridList = new ArrayList();
            List<Hashtable> FillChartList = new List<Hashtable>();

            sql = "Select * from OP_Dashboard where QueryGroup='" + QueryNo + "' and QueryName='" + QueryName + "'";
            da = new SqlDataAdapter(sql, Sconn);
            da.Fill(ds, "GetChartQry");
            if (ds.Tables["GetChartQry"].Rows.Count > 0)
            {
                DataSet dsChart = new DataSet();
                SqlDataAdapter daChart = new SqlDataAdapter();

                DataSet dsG = new DataSet();
                SqlDataAdapter daG = new SqlDataAdapter();

                string FilterClause = "", sOrderBy = "";
                if (ds.Tables["GetChartQry"].Rows[0]["MainFilterDateField"] != DBNull.Value && ds.Tables["GetChartQry"].Rows[0]["MainFilterDateField"].ToString().Trim() != "")
                    FilterClause = GetDateFilter(ds.Tables["GetChartQry"].Rows[0]["MainFilterDateField"].ToString(), FromDate, ToDate);
                string mainQryStrng = "";
                mainQryStrng = ds.Tables["GetChartQry"].Rows[0]["MainQuerystring"].ToString();
                if (!string.IsNullOrEmpty(FilterClause))
                    mainQryStrng += " " + FilterClause;

                sOrderBy = ds.Tables["GetChartQry"].Rows[0]["OrderbyColumn"].ToString();

                if (ds.Tables["GetChartQry"].Rows[0]["GroupBy"] != null && ds.Tables["GetChartQry"].Rows[0]["GroupBy"].ToString() != "")
                    mainQryStrng += " group by " + ds.Tables["GetChartQry"].Rows[0]["GroupBy"].ToString();

                if (sOrderBy != null && sOrderBy != "")
                    mainQryStrng += " order by " + sOrderBy;

                if (mainQryStrng.Contains("STP_"))
                {
                    DateTime sdt; DateTime edt;
                    DateTime.TryParse(FromDate, out sdt);
                    DateTime.TryParse(ToDate, out edt);
                    mainQryStrng += " '" + sdt.ToString("yyyyMM") + "','" + edt.ToString("yyyyMM") + "'";
                    daChart = new SqlDataAdapter(mainQryStrng, Sconn);
                    daChart.Fill(dsChart, "ChartData");

                }
                else
                {
                    daChart = new SqlDataAdapter(mainQryStrng, Sconn);
                    daChart.Fill(dsChart, "ChartData");
                    FillChartList = Ggrp.CovertDatasetToHashTbl(dsChart.Tables["ChartData"]);
                }

                ArgumentDataMember = "";

                TableName = "ChartData";
                PiechTitle = QueryName.Trim();

                DataSet dsChartGridList = new DataSet();
                string lsql = "", sDetailQueryString = "";
                sDetailQueryString = ds.Tables["GetChartQry"].Rows[0]["DetailQueryString"].ToString();

                lsql = sDetailQueryString;
                if (FilterClause != null && FilterClause != "")
                    lsql += FilterClause;
                if (!string.IsNullOrEmpty(sDetailQueryString) && sOrderBy != null && sOrderBy != "")
                    lsql += " order by " + sOrderBy;

                if (!string.IsNullOrEmpty(sDetailQueryString))
                {
                    daChart = new SqlDataAdapter(lsql, Sconn);
                    daChart.Fill(dsChartGridList, "ChartData");
                }
                else
                {
                    dsChartGridList = dsChart.Copy();
                }
                if (dsChartGridList.Tables[TableName] != null && dsChartGridList.Tables[TableName].Rows.Count > 0)
                {
                    Header = new string[dsChartGridList.Tables[TableName].Columns.Count];
                    for (int j = 0; j < dsChartGridList.Tables[TableName].Columns.Count; j++)
                    {
                        Header[j] = dsChartGridList.Tables[TableName].Columns[j].ToString();
                    }

                    foreach (DataRow dRow in dsChartGridList.Tables[TableName].Rows)
                    {
                        ArrayList values = new ArrayList();
                        foreach (object value in dRow.ItemArray)
                            values.Add(value);
                        ChartGridList.Add(values);
                    }
                }

                if (QueryName.Trim().ToUpper() == "SALES COMPARISON" || QueryName.Trim().ToUpper() == "PURCHASE COMPARISON")
                {
                    ArrayList PaymentList = new ArrayList();
                    List<string> columnNames = null;
                    if (dsChart.Tables["ChartData"] != null && dsChart.Tables["ChartData"].Rows.Count > 0)
                    {
                        columnNames = (from DataColumn dc in dsChart.Tables["ChartData"].Columns select dc.Caption).ToList();
                        foreach (DataRow dRow in dsChart.Tables["ChartData"].Rows)
                        {
                            ArrayList values = new ArrayList();
                            foreach (object value in dRow.ItemArray)
                                values.Add(value);
                            PaymentList.Add(values);
                        }
                    }
                    PaymentList.Insert(0, columnNames);
                    ObjHashtable.Add("ChartItemList", PaymentList);
                    ObjHashtable.Add("columnNames", columnNames);
                    ObjHashtable.Add("Field", ArgumentDataMember);
                    ObjHashtable.Add("Title", PiechTitle);
                    ObjHashtable.Add("header", Header);
                    ObjHashtable.Add("ChartGridList", ChartGridList);
                }
                else
                {
                    ObjHashtable.Add("ChartItemList", FillChartList);
                    ObjHashtable.Add("Field", ArgumentDataMember);
                    ObjHashtable.Add("Title", PiechTitle);
                    ObjHashtable.Add("header", Header);
                    ObjHashtable.Add("ChartGridList", ChartGridList);
                }

            }
            return Json(new
            {
                ChartItemList = ObjHashtable["ChartItemList"],
                Field = ObjHashtable["Field"],
                Title = ObjHashtable["Title"],
                header = ObjHashtable["header"],
                ChartGridList = ObjHashtable["ChartGridList"]
            });
        }

        private string GetDateFilter(string FieldName, string dtFromdate, string dtEndDate)
        {
            string lwhere = "";
            if (FieldName.Trim() != "" && (dtFromdate != null && dtFromdate.Trim() != "") && (dtEndDate != null && dtEndDate.Trim() != ""))
            {
                DateTime sdt; DateTime edt;
                DateTime.TryParse(dtFromdate, out sdt);
                DateTime.TryParse(dtEndDate, out edt);

                lwhere += " and " + FieldName + " between " + sdt.ToString("yyyyMM") + " and " + edt.ToString("yyyyMM");
            }
            return lwhere;
        }

        #endregion

        #region CategoryUpdate

        public ActionResult CategoryUpdate()
        {
            return View();
        }

        public ActionResult FillcategoryUpdate()
        {
            string Sql = "";
            SqlConnection Sconn = new SqlConnection(sqlcon);
            Sql = "select isnull(l.entry,'') as catdesc,isnull(c.catcode,'') as catcode,c.upc,c.ProductDescription,c.pds_date,c.MfgCompany,c.CurrentYTD,c.LastYearTotal from CategoryUpdate c left join OP_lookup l on c.catcode=l.entry and l.lookupid = 'CATCODE' order by upc";
            Ggrp.Getdataset(Sql, "catupdate", ds, ref da, Sconn);
            List<Hashtable> catupdate = new List<Hashtable>();
            catupdate = Ggrp.CovertDatasetToHashTbl(ds.Tables["catupdate"]);

            return Json(new { Items = catupdate });
        }

        public ActionResult FillCatCode()
        {
            string SQL = "";
            SqlConnection Sconn = new SqlConnection(sqlcon);
            SQL = "select entry,descript from OP_lookup where lookupid = 'CATCODE' and ISNULL(Inactive,0)=0 order by entry";
            Ggrp.Getdataset(SQL, "catupdate", ds, ref da, Sconn);
            List<Hashtable> catupdate = new List<Hashtable>();
            catupdate = Ggrp.CovertDatasetToHashTbl(ds.Tables["catupdate"]);

            return Json(new { Items = catupdate });
        }

        public ActionResult SaveCatCodeUpdate(string catcodelist)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            List<Hashtable> CatList = jss.Deserialize<List<Hashtable>>(catcodelist);
            if (CatList != null && CatList.Count > 0)
            {
                SqlConnection Sconn = new SqlConnection(sqlcon);
                string SQL = "";
                for (int i = 0; i < CatList.Count; i++)
                {
                    SQL = "Update CategoryUpdate set catcode='" + CatList[i]["catcode"].ToString().Trim() + "' where ltrim(rtrim(upc))='" + CatList[i]["upc"].ToString().Trim().Replace("'", "''") + "'";
                    Ggrp.Execute(SQL, Sconn);
                }
            }

            return Json(new { Items = "" });
        }

        public ActionResult Savecategoryupdate(string ObjCatList = "", bool IsAddMode = false)
        {
            string Sql = "";
            int MlineNo = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjCatList = "[" + ObjCatList + "]";
            List<Hashtable> AddCatList = jss.Deserialize<List<Hashtable>>(ObjCatList);
            DataSet sds = new DataSet();
            SqlDataAdapter sda = new SqlDataAdapter();

            string Upc = AddCatList[0]["upc"].ToString();
            if (Upc.Contains("'"))
            {
                Upc = Upc.Replace("'", "''");
            }
            decimal tlytotal = 0, tcurrytd = 0;
            if (AddCatList[0]["tlytotal"] != null && AddCatList[0]["tlytotal"].ToString() != "")
                decimal.TryParse(AddCatList[0]["tlytotal"].ToString(), out tlytotal);

            if (AddCatList[0]["tcurrytd"] != null && AddCatList[0]["tcurrytd"].ToString() != "")
                decimal.TryParse(AddCatList[0]["tcurrytd"].ToString(), out tcurrytd);

            string spdsdt = "";

            if (AddCatList[0]["pds_date"] != null && AddCatList[0]["pds_date"].ToString().Trim() != "")
                spdsdt = Convert.ToDateTime(AddCatList[0]["pds_date"].ToString()).ToString("MM/dd/yyyy");

            if (IsAddMode == true)
            {
                Sql = "select * from CategoryUpdate where 1=0";
            }
            else
            {
                Sql = "select * from CategoryUpdate where mlineno='" + AddCatList[0]["MLineNo"].ToString().Trim() + "'";
            }
            Ggrp.Getdataset(Sql, "CategoryUpdate", sds, ref sda, Sconn, false);
            if (IsAddMode)
            {
                if (!string.IsNullOrEmpty(spdsdt))
                    Sql = "insert into CategoryUpdate(catcode, newcat, model, proddesc, p_company, pds_date, tlytotal, tcurrytd, upc, m_cd)values('" + AddCatList[0]["catcode"].ToString().Trim() + "', '" + AddCatList[0]["newcat"].ToString().Trim() + "', '" + AddCatList[0]["model"].ToString().Trim() + "', '" + AddCatList[0]["proddesc"].ToString().Trim() + "', '" + AddCatList[0]["p_company"].ToString().Trim() + "', '" + spdsdt + "', '" + tlytotal + "', '" + tcurrytd + "', '" + Upc + "', '" + AddCatList[0]["m_cd"].ToString().Trim() + "')";
                else
                    Sql = "insert into CategoryUpdate(catcode, newcat, model, proddesc, p_company, pds_date, tlytotal, tcurrytd, upc, m_cd)values('" + AddCatList[0]["catcode"].ToString().Trim() + "', '" + AddCatList[0]["newcat"].ToString().Trim() + "', '" + AddCatList[0]["model"].ToString().Trim() + "', '" + AddCatList[0]["proddesc"].ToString().Trim() + "', '" + AddCatList[0]["p_company"].ToString().Trim() + "', null, '" + tlytotal + "', '" + tcurrytd + "', '" + Upc + "', '" + AddCatList[0]["m_cd"].ToString().Trim() + "')";
            }
            else
            {
                if (!string.IsNullOrEmpty(spdsdt))
                    Sql = "update CategoryUpdate set catcode='" + AddCatList[0]["catcode"].ToString().Trim() + "',newcat='" + AddCatList[0]["newcat"].ToString().Trim() + "',model='" + AddCatList[0]["model"].ToString().Trim() + "',proddesc='" + AddCatList[0]["proddesc"].ToString().Trim() + "',p_company='" + AddCatList[0]["p_company"].ToString().Trim() + "',pds_date='" + spdsdt + "',tlytotal='" + tlytotal + "',tcurrytd='" + tcurrytd + "',upc='" + Upc + "',m_cd='" + AddCatList[0]["m_cd"].ToString().Trim() + "' where mlineno='" + AddCatList[0]["MLineNo"].ToString().Trim() + "' and upc='" + Upc + "'";
                else
                    Sql = "update CategoryUpdate set catcode='" + AddCatList[0]["catcode"].ToString().Trim() + "',newcat='" + AddCatList[0]["newcat"].ToString().Trim() + "',model='" + AddCatList[0]["model"].ToString().Trim() + "',proddesc='" + AddCatList[0]["proddesc"].ToString().Trim() + "',p_company='" + AddCatList[0]["p_company"].ToString().Trim() + "',pds_date=null,tlytotal='" + tlytotal + "',tcurrytd='" + tcurrytd + "',upc='" + Upc + "',m_cd='" + AddCatList[0]["m_cd"].ToString().Trim() + "' where mlineno='" + AddCatList[0]["MLineNo"].ToString().Trim() + "' and upc='" + Upc + "'";
            }

            Ggrp.Execute(Sql, Sconn, false);
            if (IsAddMode == true)
            {
                MlineNo = Ggrp.GetIdentityValue(Sconn);
            }
            else
            {
                MlineNo = Convert.ToInt32(AddCatList[0]["MLineNo"].ToString().Trim());
            }
            return Json(new { Items = MlineNo });
        }

        public ActionResult UpcsearchList(string searchtext, string selectedvalue)
        {
            Sconn = new SqlConnection(sqlcon);

            if (searchtext.Contains("'"))
            {
                searchtext = searchtext.Replace("'", "''");
            }
            string lWhere = "";
            List<Hashtable> itemdes = new List<Hashtable>();
            if (searchtext.Contains("'"))
                searchtext = searchtext.Replace("'", "''");
            lWhere = "it." + selectedvalue + " like '" + searchtext + "%'";
            List<Hashtable> ProductList = new List<Hashtable>();
            if (searchtext == "%")
            {

                string sSQL = "Select top 50 * from product";
                Ggrp.Getdataset(sSQL, "Fillproduct", ds, ref da, Sconn);
                ProductList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Fillproduct"]);
            }
            else
            {
                string sSQL = "Select * from product it where " + lWhere;
                Ggrp.Getdataset(sSQL, "Fillproduct", ds, ref da, Sconn);
                ProductList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Fillproduct"]);
            }
            var jsonResult = Json(new { Items = ProductList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult FillCom(string Upc)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            if (!string.IsNullOrEmpty(Upc))
            {
                Upc = Upc.Replace("'", "''");
                SQl = "select m.company,p.* from product p left join mfg m on p.mfgno=m.mfgno where  upc='" + Upc + "'";
            }
            else
                SQl = "Select top 1 m.company,p.* from product p left join mfg m on p.mfgno=m.mfgno";
            Ggrp.Getdataset(SQl, "Fillcom", ds, ref da, Sconn);
            List<Hashtable> Fillcom = new List<Hashtable>();
            Fillcom = Ggrp.CovertDatasetToHashTbl(ds.Tables["Fillcom"]);
            return Json(new { Items = Fillcom });
        }
        public ActionResult Duplicateupc(string Upc, bool AddMode = false)
        {
            string Message = "";
            bool Ismsg = false;
            int count = 0;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (AddMode == false)
            {
                return Json(new { Message = Message, Ismsg = Ismsg, count = count });
            }
            else
            {
                if (AddMode)
                {
                    if (Upc.Contains("'"))
                    {
                        Upc = Upc.Replace("'", "''");
                    }
                    lSql = "select * from CategoryUpdate where upc='" + Upc + "'";
                    Ggrp.Getdataset(lSql, "upcchk", ds, ref da, Sconn);
                }

                if (ds.Tables["upcchk"].Rows.Count > 0)
                {
                    count = ds.Tables["upcchk"].Rows.Count;
                    Ismsg = true;
                    if (ds.Tables["upcchk"].Rows.Count == 1)
                        Message = "UPC already exists";
                }
                return Json(new { Message = Message, Ismsg = Ismsg, count = count });
            }
        }
        public ActionResult ProCategoryUpdate()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = "STP_Util_CateogoryUpdate";
            Ggrp.Getdataset(SQl, "Util_CateogoryUpdate", ds, ref da, Sconn);
            return Json(new { Items = "success" });

        }

        public ActionResult CategoryUpdateTable()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", cym = "";
            cym = DateTime.Now.ToString("yyyyMM");
            SQl = "STP_Util_NoCateogoryUPCs " + cym;
            Ggrp.Getdataset(SQl, "CatUpdateTable", ds, ref da, Sconn);
            return Json(new { Items = "success" });

        }

        public ActionResult DeleteCatUpdate(string MLineNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from CategoryUpdate where  mlineno ='" + MLineNo + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }
        #endregion


        #region Update PDS

        public ActionResult UpdatePDS()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdatePDSExcelform(HttpPostedFileBase file)
        {
            string cFileName = "";
            string msg = "", info = "";
            if (file != null && file.ContentLength > 0)
            {
                string fileName = Path.GetFileName(file.FileName);
                string _Ext = System.IO.Path.GetExtension(fileName).ToLower();
                cFileName = fileName;

                if (_Ext.Trim().ToUpper() == ".XLS" || _Ext.Trim().ToUpper() == ".XLSX")
                {
                    string Value = "UpdatePDSData" + "_CC_" + DateTime.Now.ToString();
                    fileName = Value.ToString().Replace("/", "").Replace(":", "").Replace("-", "").Trim() + _Ext;
                    cFileName = fileName;
                    string path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + fileName;
                    file.SaveAs(path);

                }
                else
                {
                    msg = "not excel file";
                }
            }

            return Json(new { fileName = cFileName, msg = msg, info = info });
        }

        private DataSet ConvertPDSExcelFileToDataset(string fileName)
        {
            DataSet ds = new DataSet();
            string _Path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + fileName;
            string excelConnectionString = "";
            String[] excelSheets = GetExcelSheetNames(fileName);

            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _Path + ";Extended Properties=Excel 12.0;";
            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            excelConnection.Open();
            OleDbCommand cmd1 = new OleDbCommand("SELECT * FROM [PDS$]", excelConnection);
            OleDbDataAdapter oleda = new OleDbDataAdapter();
            oleda.SelectCommand = cmd1;

            oleda.Fill(ds, "TblPDS");
            excelConnection.Dispose();

            return ds;
        }

        private String[] GetExcelSheetNames(string excelFile)
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;

            try
            {
                string _Path = ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + excelFile;
                string excelConnectionString = "";
                excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _Path + ";Extended Properties=Excel 12.0;";

                // Create connection object by using the preceding connection string.
                objConn = new OleDbConnection(excelConnectionString);
                // Open connection with the database.
                objConn.Open();
                // Get the data table containg the schema guid.
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }

                return excelSheets;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                // Clean up.
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        public ActionResult ProcessPDS(string fileName)
        {
            //============================================== UPDATED ON APRIL'2020 By bekingo@gmail.com
            string ErrMsg = "", upcno = "";
            int rcnt = 0, upccnt = 0, chkerr = 0;
            DataSet PDSDS = new DataSet();
            DataSet lds = new DataSet();
            DataSet dsErr = new DataSet();
            SqlDataAdapter daErr = new SqlDataAdapter();
            SqlDataAdapter lda = new SqlDataAdapter();
            try
            {
                PDSDS = ConvertPDSExcelFileToDataset(fileName);
                if (PDSDS != null && PDSDS.Tables.Count > 0 && PDSDS.Tables[0] != null)
                    PDSDS.Tables[0].TableName = "PDS";
                else
                {
                    ErrMsg = "PDS sheet not found";
                    return Json(new { Items = fileName, Msg = ErrMsg, upccnt = upccnt, chkerr = chkerr });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Items = fileName, Msg = ex.Message, upccnt = upccnt, chkerr = chkerr });
            }

            try
            {
                if ((PDSDS != null && PDSDS.Tables["PDS"] != null && PDSDS.Tables["PDS"].Rows.Count > 0))
                {
                    string SQL = "select * from temppds where 1=0";
                    Sconn = new SqlConnection(sqlcon);
                    Ggrp.Getdataset(SQL, "tblUpdPDS", lds, ref lda, Sconn);

                    SQL = "select top 0 '' as Status, '' as mfgname,'' as mfgupc,'' as mfgtitle,'' as Type,'' as Price,'' as Other,pds.mfgitem,pds.upc,'' as CaseUCC,prod_desc,dist_ea,eff_date from pds left join product p on pds.upc=p.upc left join mfg m on p.mfgno=m.mfgno where pds.mfgno is not null";
                    Sconn = new SqlConnection(sqlcon);
                    Ggrp.Getdataset(SQL, "PDSErr", dsErr, ref daErr, Sconn);

                    DataRow[] drUpd, drmfgupc;
                    //drUpd = PDSDS.Tables["PDS"].Select("isnull([UPC number  (Normally 12 Digits)],'') <> ''");

                    var upcColumnDisplayName = "UPC number  (Normally 12 Digits)";
                    drmfgupc = PDSDS.Tables["PDS"].Select($"isnull([{upcColumnDisplayName}],0) <> 0");
                    string toponemfgupc = "";
                    if (drmfgupc != null && drmfgupc.Length > 0)
                    {
                        toponemfgupc = drmfgupc[0][upcColumnDisplayName].ToString().Trim();
                        toponemfgupc = toponemfgupc.Substring(0, 6);
                    }



                    string TypeColName = "";
                    if (PDSDS.Tables["PDS"].Columns.Contains("Item Type  Required"))
                        drUpd = PDSDS.Tables["PDS"].Select("isnull([Item Type  Required],'') <> ''");
                    else
                    {
                        TypeColName = PDSDS.Tables["PDS"].Columns[0].ToString();
                        drUpd = PDSDS.Tables["PDS"].Select("isnull([" + TypeColName + "],'') <> ''");
                    }

                    for (int i = 0; i < drUpd.Length; i++)
                    {
                        //if (drUpd[i]["UPC number  (Normally 12 Digits)"] != DBNull.Value && drUpd[i]["UPC number  (Normally 12 Digits)"].ToString().Trim() != "")
                        {
                            rcnt = i;
                            bool IsValidUPC = true;
                            upcno = "";
                            //upcno = drUpd[i]["UPC number  (Normally 12 Digits)"].ToString().Trim();                          
                            upcno = (drUpd[i][upcColumnDisplayName] != null && drUpd[i][upcColumnDisplayName].ToString().Trim() != "") ? drUpd[i][upcColumnDisplayName].ToString().Trim() : "";

                            string checkdigit = "", Status = "";
                            if (!string.IsNullOrEmpty(upcno))
                            {
                                string lastdigit = upcno.Substring(upcno.Length -  1, 1);
                                checkdigit = Ggrp.ValidateUPC(upcno);
                                if (checkdigit != lastdigit)
                                {
                                    if (checkdigit.Trim().ToUpper() == "INVALID")
                                        Status = "Invalid character found in UPC";
                                    else
                                        Status = "UPC Check digit failed. Correct check digit is " + checkdigit;
                                    IsValidUPC = false;
                                }
                            }
                            else
                            {
                                Status = "UPC is missing";
                                IsValidUPC = false;
                            }

                            DataRow dr;
                            dr = lds.Tables[0].NewRow();
                            string mfgno = "", oldmfgno = "", mfgname = "", mfgupc = "", mfgtitle = "", mfgitem = "", sType = "", PriceChange = "", OtherChange = "", CaseUCC = "", prod_desc = "", dist_ea = "", eff_date = "";

                            if (!string.IsNullOrEmpty(upcno))
                            {
                                //Get MfgNo
                                SQL = "select m.MfgNo,m.company,m.Oldmfgno from product p left join mfg m on m.MfgNo=p.MfgNo where upc='" + upcno.Replace("'", "''") + "'";
                                Ggrp.Getdataset(SQL, "TblMfgNo", ds, ref da, Sconn);

                                if (ds != null && ds.Tables["TblMfgNo"] != null && ds.Tables["TblMfgNo"].Rows.Count > 0)
                                {
                                    mfgno = ds.Tables["TblMfgNo"].Rows[0]["MfgNo"].ToString();
                                    oldmfgno = ds.Tables["TblMfgNo"].Rows[0]["oldmfgno"].ToString();
                                    mfgname = ds.Tables["TblMfgNo"].Rows[0]["company"].ToString();
                                }
                            }

                            if (!string.IsNullOrEmpty(upcno))
                                mfgupc = upcno.Substring(0, 6);
                            else
                            {
                                mfgupc = toponemfgupc;
                            }
                            SQL = "select m.company,mu.* from mfgupc mu left join mfg m on mu.mfgno=m.mfgno where ltrim(rtrim(mu.mfgupc))='" + mfgupc + "'";
                            Ggrp.Getdataset(SQL, "TblMfgUpc", ds, ref da, Sconn);

                            if (ds.Tables["TblMfgUpc"] != null && ds.Tables["TblMfgUpc"].Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(mfgname))
                                    mfgname = ds.Tables["TblMfgUpc"].Rows[0]["company"].ToString();

                                if (string.IsNullOrEmpty(mfgno))
                                    mfgno = ds.Tables["TblMfgUpc"].Rows[0]["Mfgno"].ToString();

                                if (string.IsNullOrEmpty(oldmfgno))
                                    oldmfgno = ds.Tables["TblMfgUpc"].Rows[0]["oldmfgno"].ToString();
                            }

                            if (!string.IsNullOrEmpty(mfgno))
                                dr["mfgno"] = mfgno;

                            dr["oldmfgno"] = oldmfgno;
                            dr["mfgname"] = mfgname;

                            mfgtitle = mfgname.Trim() + "   " + mfgupc.Trim();

                            mfgitem = drUpd[i]["Mfr# Item number"].ToString();
                            dr["mfgitem"] = mfgitem;

                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Type  Required"))
                            {
                                sType = drUpd[i]["Item Type  Required"].ToString();
                                dr["f01A_itemType"] = sType;
                                dr["cp_cube"] = drUpd[i]["Case Pack Cubic / Feet"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Price Changes"))
                            {
                                PriceChange = drUpd[i]["Price Changes"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Other Changes"))
                            {
                                OtherChange = drUpd[i]["Other Changes"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Case Code Number"))
                            {
                                CaseUCC = drUpd[i]["Case Code Number"].ToString();
                                dr["cp_upc"] = CaseUCC;
                            }

                            prod_desc = drUpd[i][" Product Description (Inc# color, size, unit meas#)"].ToString();
                            dr["prod_desc"] = prod_desc;
                            dr["upc"] = upcno;

                            if (drUpd[i]["Price Effective Date  Required"] != DBNull.Value && drUpd[i]["Price Effective Date  Required"].ToString() != "")
                                eff_date = drUpd[i]["Price Effective Date  Required"].ToString();

                            if (!string.IsNullOrEmpty(eff_date))
                            {
                                dr["eff_date"] = eff_date;
                            }

                            if (drUpd[i]["Case Pack Cubic / Feet"] != DBNull.Value && drUpd[i]["Case Pack Cubic / Feet"].ToString() != "")
                                dr["cp_cube"] = drUpd[i]["Case Pack Cubic / Feet"].ToString();
                            if (drUpd[i]["Case Pack Weight / lbs"] != DBNull.Value && drUpd[i]["Case Pack Weight / lbs"].ToString() != "")
                                dr["cp_wt"] = drUpd[i]["Case Pack Weight / lbs"].ToString();
                            if (drUpd[i]["Case Pallet Quantity / cases "] != DBNull.Value && drUpd[i]["Case Pallet Quantity / cases "].ToString() != "")
                                dr["cp_palqty"] = drUpd[i]["Case Pallet Quantity / cases "].ToString();
                            if (drUpd[i]["Case Pack Height / inches "] != DBNull.Value && drUpd[i]["Case Pack Height / inches "].ToString() != "")
                                dr["cp_ht"] = drUpd[i]["Case Pack Height / inches "].ToString();
                            if (drUpd[i]["Case Pack Width / inches"] != DBNull.Value && drUpd[i]["Case Pack Width / inches"].ToString() != "")
                                dr["cp_wth"] = drUpd[i]["Case Pack Width / inches"].ToString();
                            if (drUpd[i]["Case Pack Depth / inches "] != DBNull.Value && drUpd[i]["Case Pack Depth / inches "].ToString() != "")
                                dr["cp_dpth"] = drUpd[i]["Case Pack Depth / inches "].ToString();
                            if (drUpd[i]["Master Carton Quantity "] != DBNull.Value && drUpd[i]["Master Carton Quantity "].ToString() != "")
                                dr["mp_qty"] = drUpd[i]["Master Carton Quantity "].ToString();
                            dr["h_importc"] = drUpd[i]["Country of Origin  Required if not USA"].ToString();
                            dr["country"] = drUpd[i]["Country of Origin  Required if not USA"].ToString();

                            /* 
                             if(drUpd[i]["Case Pack Quantity / Ea#"] != DBNull.Value && drUpd[i]["Case Pack Quantity / Ea#"].ToString() != "")
                                 dr["cp_qty"] = drUpd[i]["Case Pack Quantity / Ea#"].ToString();
                              */

                            try
                            {
                                if (drUpd[i]["Case Pack Quantity / Ea#"] != DBNull.Value && drUpd[i]["Case Pack Quantity / Ea#"].ToString() != "")
                                    dr["cp_qty"] = drUpd[i]["Case Pack Quantity / Ea#"].ToString();
                            }
                            catch { }

                            if (drUpd[i]["Distributor Price / Ea#   Required"] != DBNull.Value && drUpd[i]["Distributor Price / Ea#   Required"].ToString() != "")
                                dist_ea = drUpd[i]["Distributor Price / Ea#   Required"].ToString();

                            if (!string.IsNullOrEmpty(dist_ea))
                            {
                                dr["dist_ea"] = dist_ea;
                            }

                            /*
                            if(drUpd[i]["Drive Item Price / Ea#"] != DBNull.Value && drUpd[i]["Drive Item Price / Ea#"].ToString() != "")
                                dr["driveitem"] = drUpd[i]["Drive Item Price / Ea#"].ToString();
                                */

                            try
                            {
                                if (drUpd[i]["Drive Item Price / Ea#"] != DBNull.Value && drUpd[i]["Drive Item Price / Ea#"].ToString() != "")
                                    dr["driveitem"] = drUpd[i]["Drive Item Price / Ea#"].ToString();

                            }
                            catch { }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Full Case Price ea#"))
                            {
                                if (drUpd[i]["Full Case Price ea#"] != DBNull.Value && drUpd[i]["Full Case Price ea#"].ToString() != "")
                                    dr["price_a"] = drUpd[i]["Full Case Price ea#"].ToString();
                            }

                            /*
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price A - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price A - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price A - Distributor / Ea#"].ToString() != "")
                                    dr["price_a"] = drUpd[i]["Price A - Distributor / Ea#"].ToString();
                            }
                            */

                            try
                            {
                                if (PDSDS.Tables["PDS"].Columns.Contains("Price A - Distributor / Ea#"))
                                {
                                    if (drUpd[i]["Price A - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price A - Distributor / Ea#"].ToString() != "")
                                        dr["price_a"] = drUpd[i]["Price A - Distributor / Ea#"].ToString();
                                }
                            }
                            catch { }


                            try
                            {
                                if (PDSDS.Tables["PDS"].Columns.Contains("Price B - Distributor / Ea#"))
                                {
                                    if (drUpd[i]["Price B - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price B - Distributor / Ea#"].ToString() != "")
                                        dr["price_b"] = drUpd[i]["Price B - Distributor / Ea#"].ToString();
                                }
                            }
                            catch { }

                            try
                            {
                                if (PDSDS.Tables["PDS"].Columns.Contains("Price C - Distributor / Ea#"))
                                {
                                    if (drUpd[i]["Price C - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price C - Distributor / Ea#"].ToString() != "")
                                        dr["price_c"] = drUpd[i]["Price C - Distributor / Ea#"].ToString();
                                }
                            }
                            catch { }
                            try
                            {
                                if (PDSDS.Tables["PDS"].Columns.Contains("Price D - Distributor / Ea#"))
                                {
                                    if (drUpd[i]["Price D - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price D - Distributor / Ea#"].ToString() != "")
                                        dr["price_d"] = drUpd[i]["Price D - Distributor / Ea#"].ToString();
                                }
                            }
                            catch { }
                            try
                            {
                                if (PDSDS.Tables["PDS"].Columns.Contains("Price E - Distributor / Ea#"))
                                {
                                    if (drUpd[i]["Price E - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price E - Distributor / Ea#"].ToString() != "")
                                        dr["price_e"] = drUpd[i]["Price E - Distributor / Ea#"].ToString();
                                }
                            }
                            catch { }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Early Order Full Case Price"))
                            {
                                if (drUpd[i]["Early Order Full Case Price"] != DBNull.Value && drUpd[i]["Early Order Full Case Price"].ToString() != "")
                                    dr["price_b"] = drUpd[i]["Early Order Full Case Price"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Early Order Broken Case Price"))
                            {
                                if (drUpd[i]["Early Order Broken Case Price"] != DBNull.Value && drUpd[i]["Early Order Broken Case Price"].ToString() != "")
                                    dr["price_d"] = drUpd[i]["Early Order Broken Case Price"].ToString();
                            }

                            /*


                            if (PDSDS.Tables["PDS"].Columns.Contains("Price B - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price B - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price B - Distributor / Ea#"].ToString() != "")
                                    dr["price_b"] = drUpd[i]["Price B - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price C - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price C - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price C - Distributor / Ea#"].ToString() != "")
                                    dr["price_c"] = drUpd[i]["Price C - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Early Order Full Case Price"))
                            {
                                if (drUpd[i]["Early Order Full Case Price"] != DBNull.Value && drUpd[i]["Early Order Full Case Price"].ToString() != "")
                                    dr["price_b"] = drUpd[i]["Early Order Full Case Price"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price D - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price D - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price D - Distributor / Ea#"].ToString() != "")
                                    dr["price_d"] = drUpd[i]["Price D - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Early Order Broken Case Price"))
                            {
                                if (drUpd[i]["Early Order Broken Case Price"] != DBNull.Value && drUpd[i]["Early Order Broken Case Price"].ToString() != "")
                                    dr["price_d"] = drUpd[i]["Early Order Broken Case Price"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price E - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price E - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price E - Distributor / Ea#"].ToString() != "")
                                    dr["price_e"] = drUpd[i]["Price E - Distributor / Ea#"].ToString();
                            }
                            */

                            /*
                            if(drUpd[i]["Suggested Retail / Ea# "] != null && drUpd[i]["Suggested Retail / Ea# "].ToString().Trim() != "")
                                dr["sugg_retl"] = drUpd[i]["Suggested Retail / Ea# "].ToString();
                                */

                            try
                            {
                                if (drUpd[i]["Suggested Retail Price / Ea# "] != null && drUpd[i]["Suggested Retail Price / Ea# "].ToString().Trim() != "")
                                    dr["sugg_retl"] = drUpd[i]["Suggested Retail Price / Ea# "].ToString();
                            }
                            catch (Exception ex)
                            {

                            }
                            /*
                            if (drUpd[i]["Suggested Dealer / Ea# "] != null && drUpd[i]["Suggested Dealer / Ea# "].ToString().Trim() != "")
                                dr["sugg_deal"] = drUpd[i]["Suggested Dealer / Ea# "].ToString();
*/

                            try
                            {
                                if (drUpd[i]["Suggested Dealer Price / Ea# "] != null && drUpd[i]["Suggested Dealer Price / Ea# "].ToString().Trim() != "")
                                    dr["sugg_deal"] = drUpd[i]["Suggested Dealer Price / Ea# "].ToString();
                            }
                            catch { }

                            try
                            {
                                /*
                                if (drUpd[i]["Drop Ship Price / Ea#"] != null && drUpd[i]["Drop Ship Price / Ea#"].ToString().Trim() != "")
                                    dr["dropship"] = drUpd[i]["Drop Ship Price / Ea#"].ToString();*/

                                if (drUpd[i]["Drop Ship Price / Ea#"] != null && drUpd[i]["Drop Ship Price / Ea#"].ToString().Trim() != "")
                                    dr["dropship"] = drUpd[i]["Drop Ship Price / Ea#"].ToString();
                            }
                            catch { }


                            //==========================================================================  NEW COMMANDS APRIL'20 by bekingo@gmail.com
                            if (PDSDS.Tables["PDS"].Columns.Contains("Distributor Code #"))
                            {
                                if (drUpd[i]["Distributor Code #"] != DBNull.Value && drUpd[i]["Distributor Code #"].ToString() != "")
                                    dr["f01H_distCode"] = drUpd[i]["Distributor Code #"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack / Tier"))
                            {
                                if (drUpd[i]["Pack / Tier"] != DBNull.Value && drUpd[i]["Pack / Tier"].ToString() != "")
                                    dr["f01M_packTier"] = drUpd[i]["Pack / Tier"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Tier / Pit"))
                            {
                                if (drUpd[i]["Tier / Pit"] != DBNull.Value && drUpd[i]["Tier / Pit"].ToString() != "")
                                    dr["f01N_tierPit"] = drUpd[i]["Tier / Pit"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Height - Each"))
                            {
                                if (drUpd[i]["Item Height - Each"] != DBNull.Value && drUpd[i]["Item Height - Each"].ToString() != "")
                                    dr["f01V_itemHeightEach"] = drUpd[i]["Item Height - Each"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Width - Each"))
                            {
                                if (drUpd[i]["Item Width - Each"] != DBNull.Value && drUpd[i]["Item Width - Each"].ToString() != "")
                                    dr["f01W_itemWidthEach"] = drUpd[i]["Item Width - Each"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Depth - Each"))
                            {
                                if (drUpd[i]["Item Depth - Each"] != DBNull.Value && drUpd[i]["Item Depth - Each"].ToString() != "")
                                    dr["f01X_itemDepthEach"] = drUpd[i]["Item Depth - Each"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Cubic Feet - Each"))
                            {
                                if (drUpd[i]["Item Cubic Feet - Each"] != DBNull.Value && drUpd[i]["Item Cubic Feet - Each"].ToString() != "")
                                    dr["f01Y_itemCubicFeetEach"] = drUpd[i]["Item Cubic Feet - Each"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Weight - Each"))
                            {
                                if (drUpd[i]["Item Weight - Each"] != DBNull.Value && drUpd[i]["Item Weight - Each"].ToString() != "")
                                    dr["f01Z_itemWeightEach"] = drUpd[i]["Item Weight - Each"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Height - Purchasing UOM"))
                            {
                                if (drUpd[i]["Height - Purchasing UOM"] != DBNull.Value && drUpd[i]["Height - Purchasing UOM"].ToString() != "")
                                    dr["f01AA_heightPurchasingUOM"] = drUpd[i]["Height - Purchasing UOM"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Width - Purchasing UOM"))
                            {
                                if (drUpd[i]["Width - Purchasing UOM"] != DBNull.Value && drUpd[i]["Width - Purchasing UOM"].ToString() != "")
                                    dr["f01AB_widthPurchasingUOM"] = drUpd[i]["Width - Purchasing UOM"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Depth - Purchasing UOM"))
                            {
                                if (drUpd[i]["Depth - Purchasing UOM"] != DBNull.Value && drUpd[i]["Depth - Purchasing UOM"].ToString() != "")
                                    dr["f01AC_depthPurchasingUOM"] = drUpd[i]["Depth - Purchasing UOM"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Cubic Feet - Purchasing UOM"))
                            {
                                if (drUpd[i]["Cubic Feet - Purchasing UOM"] != DBNull.Value && drUpd[i]["Cubic Feet - Purchasing UOM"].ToString() != "")
                                    dr["f01AD_cubicFeePurchasingUOM"] = drUpd[i]["Cubic Feet - Purchasing UOM"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Weight - Purchasing UOM"))
                            {
                                if (drUpd[i]["Weight - Purchasing UOM"] != DBNull.Value && drUpd[i]["Weight - Purchasing UOM"].ToString() != "")
                                    dr["f01AE_weightFeePurchasingUOM"] = drUpd[i]["Weight - Purchasing UOM"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Purchasing UOM Quantity"))
                            {
                                if (drUpd[i]["Purchasing UOM Quantity"] != DBNull.Value && drUpd[i]["Purchasing UOM Quantity"].ToString() != "")
                                    dr["f01AF_purchasingUOMQty"] = drUpd[i]["Purchasing UOM Quantity"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Purchasing Unit of Measure Code"))
                            {
                                if (drUpd[i]["Purchasing Unit of Measure Code"] != DBNull.Value && drUpd[i]["Purchasing Unit of Measure Code"].ToString() != "")
                                    dr["f01AG_purchasingUOMCode"] = drUpd[i]["Purchasing Unit of Measure Code"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack Cu Ft"))
                            {
                                if (drUpd[i]["Pack Cu Ft"] != DBNull.Value && drUpd[i]["Pack Cu Ft"].ToString() != "")
                                    dr["f01AH_packCuFt"] = drUpd[i]["Pack Cu Ft"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack Weight"))
                            {
                                if (drUpd[i]["Pack Weight"] != DBNull.Value && drUpd[i]["Pack Weight"].ToString() != "")
                                    dr["f01AI_packWeight"] = drUpd[i]["Pack Weight"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack Width"))
                            {
                                if (drUpd[i]["Pack Width"] != DBNull.Value && drUpd[i]["Pack Width"].ToString() != "")
                                    dr["f01AJ_packWidth"] = drUpd[i]["Pack Width"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack Depth"))
                            {
                                if (drUpd[i]["Pack Depth"] != DBNull.Value && drUpd[i]["Pack Depth"].ToString() != "")
                                    dr["f01AK_packDepth"] = drUpd[i]["Pack Depth"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack Height"))
                            {
                                if (drUpd[i]["Pack Height"] != DBNull.Value && drUpd[i]["Pack Height"].ToString() != "")
                                    dr["f01AL_packHeigth"] = drUpd[i]["Pack Height"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Ti Hi Bottom Layer Qty"))
                            {
                                if (drUpd[i]["Ti Hi Bottom Layer Qty"] != DBNull.Value && drUpd[i]["Ti Hi Bottom Layer Qty"].ToString() != "")
                                    dr["f01AM_tihiBottomLayerQty"] = drUpd[i]["Ti Hi Bottom Layer Qty"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Ti Hi Row High Qty"))
                            {
                                if (drUpd[i]["Ti Hi Row High Qty"] != DBNull.Value && drUpd[i]["Ti Hi Row High Qty"].ToString() != "")
                                    dr["f01AN_tihiRowHighQty1"] = drUpd[i]["Ti Hi Row High Qty"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Ti Hi Bottom UOM (cs or bg)"))
                            {
                                if (drUpd[i]["Ti Hi Bottom UOM (cs or bg)"] != DBNull.Value && drUpd[i]["Ti Hi Bottom UOM (cs or bg)"].ToString() != "")
                                    dr["f01AO_tihiBottomLayerType"] = drUpd[i]["Ti Hi Bottom UOM (cs or bg)"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Pack/Tier"))
                            {
                                if (drUpd[i]["Pack/Tier"] != DBNull.Value && drUpd[i]["Pack/Tier"].ToString() != "")
                                    dr["f01AP_packTier"] = drUpd[i]["Pack/Tier"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Tier/ PLT"))
                            {
                                if (drUpd[i]["Tier/ PLT"] != DBNull.Value && drUpd[i]["Tier/ PLT"].ToString() != "")
                                    dr["f01AQ_tierPLT"] = drUpd[i]["Tier/ PLT"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("EPA Regulation Code/Registratiom #"))
                            {
                                if (drUpd[i]["EPA Regulation Code/Registratiom #"] != DBNull.Value && drUpd[i]["EPA Regulation Code/Registratiom #"].ToString() != "")
                                    dr["f01AR_epaRegulationCode"] = drUpd[i]["EPA Regulation Code/Registratiom #"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Insurance Class Code"))
                            {
                                if (drUpd[i]["Insurance Class Code"] != DBNull.Value && drUpd[i]["Insurance Class Code"].ToString() != "")
                                    dr["f01AS_insuranceClassCode"] = drUpd[i]["Insurance Class Code"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Hazard Code - Air Y/N"))
                            {
                                if (drUpd[i]["Hazard Code - Air Y/N"] != DBNull.Value && drUpd[i]["Hazard Code - Air Y/N"].ToString() != "")
                                    dr["f01AT_hazardAirCode"] = drUpd[i]["Hazard Code - Air Y/N"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Hazard Code - Water Y/N"))
                            {
                                if (drUpd[i]["Hazard Code - Water Y/N"] != DBNull.Value && drUpd[i]["Hazard Code - Water Y/N"].ToString() != "")
                                    dr["f01AU_hazardWaterCode"] = drUpd[i]["Hazard Code - Water Y/N"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Hazard Code - Ground Y/N"))
                            {
                                if (drUpd[i]["Hazard Code - Ground Y/N"] != DBNull.Value && drUpd[i]["Hazard Code - Ground Y/N"].ToString() != "")
                                    dr["f01AV_hazardGroundCode"] = drUpd[i]["Hazard Code - Ground Y/N"].ToString();
                            }


                            if (PDSDS.Tables["PDS"].Columns.Contains("Lot Controlled Item Y/N"))
                            {
                                if (drUpd[i]["Lot Controlled Item Y/N"] != DBNull.Value && drUpd[i]["Lot Controlled Item Y/N"].ToString() != "")
                                    dr["f01AW_lotControllerItem"] = drUpd[i]["Lot Controlled Item Y/N"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("FRT Class"))
                            {
                                if (drUpd[i]["FRT Class"] != DBNull.Value && drUpd[i]["FRT Class"].ToString() != "")
                                    dr["f01AX_frtClass"] = drUpd[i]["FRT Class"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("US Harmonized Tariff Number"))
                            {
                                if (drUpd[i]["US Harmonized Tariff Number"] != DBNull.Value && drUpd[i]["US Harmonized Tariff Number"].ToString() != "")
                                    dr["f01AY_usHamonizedTariffNbr"] = drUpd[i]["US Harmonized Tariff Number"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("HTS 301 Tariff Number"))
                            {
                                if (drUpd[i]["HTS 301 Tariff Number"] != DBNull.Value && drUpd[i]["HTS 301 Tariff Number"].ToString() != "")
                                    dr["f01AZ_hts301TariffNbr"] = drUpd[i]["HTS 301 Tariff Number"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("MSDS Document Date"))
                            {
                                if (drUpd[i]["MSDS Document Date"] != DBNull.Value && drUpd[i]["MSDS Document Date"].ToString() != "")
                                    dr["f01BA_msdsDocDate"] = drUpd[i]["MSDS Document Date"].ToString();
                            }


                            if (PDSDS.Tables["PDS"].Columns.Contains("List Price"))
                            {
                                if (drUpd[i]["List Price"] != DBNull.Value && drUpd[i]["List Price"].ToString() != "")
                                    dr["f01BD_listPrice"] = drUpd[i]["List Price"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Dealer Drop Ship / EA"))
                            {
                                if (drUpd[i]["Dealer Drop Ship / EA"] != DBNull.Value && drUpd[i]["Dealer Drop Ship / EA"].ToString() != "")
                                    dr["f01BH_dealerDropShip"] = drUpd[i]["Dealer Drop Ship / EA"].ToString();
                            }


                            if (PDSDS.Tables["PDS"].Columns.Contains("Distributor Import FOB/EA"))
                            {
                                if (drUpd[i]["Distributor Import FOB/EA"] != DBNull.Value && drUpd[i]["Distributor Import FOB/EA"].ToString() != "")
                                    dr["f01BI_distributionImportFOB"] = drUpd[i]["Distributor Import FOB/EA"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Distributor Domestic FOB/EA"))
                            {
                                if (drUpd[i]["Distributor Domestic FOB/EA"] != DBNull.Value && drUpd[i]["Distributor Domestic FOB/EA"].ToString() != "")
                                    dr["f01BJ_distributionDomesticFOB"] = drUpd[i]["Distributor Domestic FOB/EA"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Manufacturer Item Number_(If longer than 10 digits allowed in co"))
                            {
                                /*
                                if (drUpd[i]["Manufacturer Item Number_(If longer than 10 digits allowed in co"] != DBNull.Value && drUpd[i]["Manufacturer Item Number_(If longer than 10 digits allowed in co"].ToString() != "")
                                    dr["f01BR_mItemNbr"] = drUpd[i]["Manufacturer Item Number_(If longer than 10 digits allowed in co"].ToString();
                                 */
                                if (drUpd[i][69] != DBNull.Value && drUpd[i][69].ToString() != "")
                                    dr["f01BR_mItemNbr"] = drUpd[i][69].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("EAN Number  (13 digits)"))
                            {
                                if (drUpd[i]["EAN Number  (13 digits)"] != DBNull.Value && drUpd[i]["EAN Number  (13 digits)"].ToString() != "")
                                    dr["f01BS_eanNbr"] = drUpd[i]["EAN Number  (13 digits)"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Image Available Y/N"))
                            {
                                if (drUpd[i]["Image Available Y/N"] != DBNull.Value && drUpd[i]["Image Available Y/N"].ToString() != "")
                                    dr["f01BU_imageAvailable"] = drUpd[i]["Image Available Y/N"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Image Format PDF/Jpeg Format"))
                            {
                                if (drUpd[i]["Image Format PDF/Jpeg Format"] != DBNull.Value && drUpd[i]["Image Format PDF/Jpeg Format"].ToString() != "")
                                    dr["f01BV_imageFormat"] = drUpd[i]["Image Format PDF/Jpeg Format"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Product Shelf Life"))
                            {
                                if (drUpd[i]["Product Shelf Life"] != DBNull.Value && drUpd[i]["Product Shelf Life"].ToString() != "")
                                    dr["f01BW_productShelfLine"] = drUpd[i]["Product Shelf Life"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("P-65"))
                            {
                                if (drUpd[i]["P-65"] != DBNull.Value && drUpd[i]["P-65"].ToString() != "")
                                    dr["f01BX_P65"] = drUpd[i]["P-65"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("SDS"))
                            {
                                if (drUpd[i]["SDS"] != DBNull.Value && drUpd[i]["SDS"].ToString() != "")
                                    dr["f01BY_SDS"] = drUpd[i]["SDS"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("WA Haz"))
                            {
                                if (drUpd[i]["WA Haz"] != DBNull.Value && drUpd[i]["WA Haz"].ToString() != "")
                                    dr["f01BZ_waHaz"] = drUpd[i]["WA Haz"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Califirnia Reg #"))
                            {
                                if (drUpd[i]["WA Haz"] != DBNull.Value && drUpd[i]["Califirnia Reg #"].ToString() != "")
                                    dr["f01CA_califirniaRegNbr"] = drUpd[i]["Califirnia Reg #"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Product Description (Overflow from Column J)"))
                            {
                                if (drUpd[i]["Product Description (Overflow from Column J)"] != DBNull.Value && drUpd[i]["Product Description (Overflow from Column J)"].ToString() != "")
                                    dr["f01J_prodcDesc2"] = drUpd[i]["Product Description (Overflow from Column J)"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("SDS Required (IF YES ONLY)"))
                            {
                                if (drUpd[i]["SDS Required (IF YES ONLY)"] != DBNull.Value && drUpd[i]["SDS Required (IF YES ONLY)"].ToString() != "")
                                    dr["h_msdsreq"] = drUpd[i]["SDS Required (IF YES ONLY)"].ToString().Equals("Y");
                            }

                            if (IsValidUPC)
                                lds.Tables[0].Rows.Add(dr);
                            else
                            {
                                DataRow ErrRow;
                                ErrRow = dsErr.Tables[0].NewRow();
                                ErrRow["mfgname"] = mfgname;
                                ErrRow["mfgupc"] = mfgupc;
                                ErrRow["mfgtitle"] = mfgtitle;
                                ErrRow["Type"] = sType;
                                ErrRow["Price"] = PriceChange;
                                ErrRow["Other"] = OtherChange;
                                ErrRow["mfgitem"] = mfgitem;
                                ErrRow["upc"] = upcno;
                                ErrRow["CaseUCC"] = CaseUCC;
                                ErrRow["prod_desc"] = prod_desc;
                                if (!string.IsNullOrEmpty(dist_ea))
                                {
                                    ErrRow["dist_ea"] = dist_ea;
                                }
                                //ErrRow["dist_ea"] = dist_ea;
                                //ErrRow["eff_date"] = eff_date;
                                if (!string.IsNullOrEmpty(eff_date))
                                {
                                    ErrRow["eff_date"] = eff_date;
                                }
                                ErrRow["Status"] = Status;
                                dsErr.Tables[0].Rows.Add(ErrRow);
                            }

                            upcno = "";
                        }
                    }

                    if (dsErr.Tables[0] != null && dsErr.Tables[0].Rows.Count > 0)
                    {
                        chkerr = dsErr.Tables[0].Rows.Count;
                        Session["PDSErr"] = dsErr;
                    }

                    if (lds.Tables[0] != null && lds.Tables[0].Rows.Count > 0)
                    {
                        upccnt = lds.Tables[0].Rows.Count;
                        Session["UpdPDS"] = lds;
                        //string tSQL = "delete from temppds";
                        //Ggrp.Execute(tSQL, Sconn);
                        //lda.Update(lds, "tblUpdPDS");

                        //tSQL = "exec STP_Util_PDSUpdate ''";
                        //Ggrp.Getdataset(tSQL, "tblpds", ds, ref da, Sconn);
                    }
                    else
                    {
                        ErrMsg = "No records found";
                    }
                }
                else
                {
                    ErrMsg = "No records found";
                }
            }
            catch (Exception ex)
            {
                string ErrDesc = "";
                if (!string.IsNullOrEmpty(upcno))
                    ErrDesc = "UPC: " + upcno + "<br/>" + ex.Message;
                else
                    ErrDesc = ex.Message;

                return Json(new { Items = fileName, Msg = ErrDesc, upccnt = upccnt, chkerr = chkerr });
            }

            return Json(new { Items = fileName, Msg = ErrMsg, upccnt = upccnt, chkerr = chkerr });

        }

        public ActionResult ProcessPDS_ORIGINAL(string fileName)
        {
            string ErrMsg = "", upcno = "";
            int rcnt = 0, upccnt = 0, chkerr = 0;
            DataSet PDSDS = new DataSet();
            DataSet lds = new DataSet();
            DataSet dsErr = new DataSet();
            SqlDataAdapter daErr = new SqlDataAdapter();
            SqlDataAdapter lda = new SqlDataAdapter();
            try
            {
                PDSDS = ConvertPDSExcelFileToDataset(fileName);
                if (PDSDS != null && PDSDS.Tables.Count > 0 && PDSDS.Tables[0] != null)
                    PDSDS.Tables[0].TableName = "PDS";
                else
                {
                    ErrMsg = "PDS sheet not found";
                    return Json(new { Items = fileName, Msg = ErrMsg, upccnt = upccnt, chkerr = chkerr });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Items = fileName, Msg = ex.Message, upccnt = upccnt, chkerr = chkerr });
            }

            try
            {
                if ((PDSDS != null && PDSDS.Tables["PDS"] != null && PDSDS.Tables["PDS"].Rows.Count > 0))
                {
                    string SQL = "select * from temppds where 1=0";
                    Sconn = new SqlConnection(sqlcon);
                    Ggrp.Getdataset(SQL, "tblUpdPDS", lds, ref lda, Sconn);

                    SQL = "select top 0 '' as Status, '' as mfgname,'' as mfgupc,'' as mfgtitle,'' as Type,'' as Price,'' as Other,pds.mfgitem,pds.upc,'' as CaseUCC,prod_desc,dist_ea,eff_date from pds left join product p on pds.upc=p.upc left join mfg m on p.mfgno=m.mfgno where pds.mfgno is not null";
                    Sconn = new SqlConnection(sqlcon);
                    Ggrp.Getdataset(SQL, "PDSErr", dsErr, ref daErr, Sconn);

                    DataRow[] drUpd, drmfgupc;
                    //drUpd = PDSDS.Tables["PDS"].Select("isnull([UPC number  (Normally 12 Digits)],'') <> ''");

                    drmfgupc = PDSDS.Tables["PDS"].Select("isnull([UPC number  (Normally 12 Digits)],'') <> ''");
                    string toponemfgupc = "";
                    if (drmfgupc != null && drmfgupc.Length > 0)
                    {
                        toponemfgupc = drmfgupc[0]["UPC number  (Normally 12 Digits)"].ToString().Trim();
                        toponemfgupc = toponemfgupc.Substring(0, 6);
                    }

                    string TypeColName = "";
                    if (PDSDS.Tables["PDS"].Columns.Contains("Item Type  Required"))
                        drUpd = PDSDS.Tables["PDS"].Select("isnull([Item Type  Required],'') <> ''");
                    else
                    {
                        TypeColName = PDSDS.Tables["PDS"].Columns[0].ToString();
                        drUpd = PDSDS.Tables["PDS"].Select("isnull([" + TypeColName + "],'') <> ''");
                    }

                    for (int i = 0; i < drUpd.Length; i++)
                    {
                        //if (drUpd[i]["UPC number  (Normally 12 Digits)"] != DBNull.Value && drUpd[i]["UPC number  (Normally 12 Digits)"].ToString().Trim() != "")
                        {
                            rcnt = i;
                            bool IsValidUPC = true;
                            upcno = "";
                            //upcno = drUpd[i]["UPC number  (Normally 12 Digits)"].ToString().Trim();
                            upcno = (drUpd[i]["UPC number  (Normally 12 Digits)"] != null && drUpd[i]["UPC number  (Normally 12 Digits)"].ToString().Trim() != "") ? drUpd[i]["UPC number  (Normally 12 Digits)"].ToString().Trim() : "";

                            string checkdigit = "", Status = "";
                            if (!string.IsNullOrEmpty(upcno))
                            {
                                string lastdigit = upcno.Substring(upcno.Length - 1, 1);
                                checkdigit = Ggrp.ValidateUPC(upcno);
                                if (checkdigit != lastdigit)
                                {
                                    if (checkdigit.Trim().ToUpper() == "INVALID")
                                        Status = "Invalid character found in UPC";
                                    else
                                        Status = "UPC Check digit failed. Correct check digit is " + checkdigit;
                                    IsValidUPC = false;
                                }
                            }
                            else
                            {
                                Status = "UPC is missing";
                                IsValidUPC = false;
                            }

                            DataRow dr;
                            dr = lds.Tables[0].NewRow();
                            string mfgno = "", oldmfgno = "", mfgname = "", mfgupc = "", mfgtitle = "", mfgitem = "", sType = "", PriceChange = "", OtherChange = "", CaseUCC = "", prod_desc = "", dist_ea = "", eff_date = "";

                            if (!string.IsNullOrEmpty(upcno))
                            {
                                //Get MfgNo
                                SQL = "select m.MfgNo,m.company,m.Oldmfgno from product p left join mfg m on m.MfgNo=p.MfgNo where upc='" + upcno.Replace("'", "''") + "'";
                                Ggrp.Getdataset(SQL, "TblMfgNo", ds, ref da, Sconn);

                                if (ds != null && ds.Tables["TblMfgNo"] != null && ds.Tables["TblMfgNo"].Rows.Count > 0)
                                {
                                    mfgno = ds.Tables["TblMfgNo"].Rows[0]["MfgNo"].ToString();
                                    oldmfgno = ds.Tables["TblMfgNo"].Rows[0]["oldmfgno"].ToString();
                                    mfgname = ds.Tables["TblMfgNo"].Rows[0]["company"].ToString();
                                }
                            }

                            if (!string.IsNullOrEmpty(upcno))
                                mfgupc = upcno.Substring(0, 6);
                            else
                            {
                                mfgupc = toponemfgupc;
                            }
                            SQL = "select m.company,mu.* from mfgupc mu left join mfg m on mu.mfgno=m.mfgno where ltrim(rtrim(mu.mfgupc))='" + mfgupc + "'";
                            Ggrp.Getdataset(SQL, "TblMfgUpc", ds, ref da, Sconn);

                            if (ds.Tables["TblMfgUpc"] != null && ds.Tables["TblMfgUpc"].Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(mfgname))
                                    mfgname = ds.Tables["TblMfgUpc"].Rows[0]["company"].ToString();

                                if (string.IsNullOrEmpty(mfgno))
                                    mfgno = ds.Tables["TblMfgUpc"].Rows[0]["Mfgno"].ToString();

                                if (string.IsNullOrEmpty(oldmfgno))
                                    oldmfgno = ds.Tables["TblMfgUpc"].Rows[0]["oldmfgno"].ToString();
                            }

                            if (!string.IsNullOrEmpty(mfgno))
                                dr["mfgno"] = mfgno;

                            dr["oldmfgno"] = oldmfgno;
                            dr["mfgname"] = mfgname;

                            mfgtitle = mfgname.Trim() + "   " + mfgupc.Trim();

                            mfgitem = drUpd[i]["Mfr# Item number"].ToString();
                            dr["mfgitem"] = mfgitem;

                            if (PDSDS.Tables["PDS"].Columns.Contains("Item Type  Required"))
                            {
                                sType = drUpd[i]["Item Type  Required"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Price Changes"))
                            {
                                PriceChange = drUpd[i]["Price Changes"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Other Changes"))
                            {
                                OtherChange = drUpd[i]["Other Changes"].ToString();
                            }

                            if (PDSDS.Tables["PDS"].Columns.Contains("Case Code Number"))
                            {
                                CaseUCC = drUpd[i]["Case Code Number"].ToString();
                            }

                            prod_desc = drUpd[i][" Product Description (Inc# color, size, unit meas#)"].ToString();
                            dr["prod_desc"] = prod_desc;
                            dr["upc"] = upcno;

                            if (drUpd[i]["Price Effective Date  Required"] != DBNull.Value && drUpd[i]["Price Effective Date  Required"].ToString() != "")
                                eff_date = drUpd[i]["Price Effective Date  Required"].ToString();

                            if (!string.IsNullOrEmpty(eff_date))
                            {
                                dr["eff_date"] = eff_date;
                            }

                            if (drUpd[i]["Case Pack Cubic / Feet"] != DBNull.Value && drUpd[i]["Case Pack Cubic / Feet"].ToString() != "")
                                dr["cp_cube"] = drUpd[i]["Case Pack Cubic / Feet"].ToString();
                            if (drUpd[i]["Case Pack Weight / lbs"] != DBNull.Value && drUpd[i]["Case Pack Weight / lbs"].ToString() != "")
                                dr["cp_wt"] = drUpd[i]["Case Pack Weight / lbs"].ToString();
                            if (drUpd[i]["Case Pallet Quantity / cases "] != DBNull.Value && drUpd[i]["Case Pallet Quantity / cases "].ToString() != "")
                                dr["cp_palqty"] = drUpd[i]["Case Pallet Quantity / cases "].ToString();
                            if (drUpd[i]["Case Pack Height / inches "] != DBNull.Value && drUpd[i]["Case Pack Height / inches "].ToString() != "")
                                dr["cp_ht"] = drUpd[i]["Case Pack Height / inches "].ToString();
                            if (drUpd[i]["Case Pack Width / inches"] != DBNull.Value && drUpd[i]["Case Pack Width / inches"].ToString() != "")
                                dr["cp_wth"] = drUpd[i]["Case Pack Width / inches"].ToString();
                            if (drUpd[i]["Case Pack Depth / inches "] != DBNull.Value && drUpd[i]["Case Pack Depth / inches "].ToString() != "")
                                dr["cp_dpth"] = drUpd[i]["Case Pack Depth / inches "].ToString();
                            if (drUpd[i]["Master Carton Quantity "] != DBNull.Value && drUpd[i]["Master Carton Quantity "].ToString() != "")
                                dr["mp_qty"] = drUpd[i]["Master Carton Quantity "].ToString();
                            dr["h_importc"] = drUpd[i]["Country of Origin  Required if not USA"].ToString();
                            dr["country"] = drUpd[i]["Country of Origin  Required if not USA"].ToString();
                            if (drUpd[i]["Case Pack Quantity / Ea#"] != DBNull.Value && drUpd[i]["Case Pack Quantity / Ea#"].ToString() != "")
                                dr["cp_qty"] = drUpd[i]["Case Pack Quantity / Ea#"].ToString();

                            if (drUpd[i]["Distributor / Ea#   Required"] != DBNull.Value && drUpd[i]["Distributor / Ea#   Required"].ToString() != "")
                                dist_ea = drUpd[i]["Distributor / Ea#   Required"].ToString();

                            if (!string.IsNullOrEmpty(dist_ea))
                            {
                                dr["dist_ea"] = dist_ea;
                            }

                            if (drUpd[i]["Drive Item Price / Ea."] != DBNull.Value && drUpd[i]["Drive Item Price / Ea."].ToString() != "")
                                dr["driveitem"] = drUpd[i]["Drive Item Price / Ea."].ToString();
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price A - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price A - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price A - Distributor / Ea#"].ToString() != "")
                                    dr["price_a"] = drUpd[i]["Price A - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Full Case Price ea#"))
                            {
                                if (drUpd[i]["Full Case Price ea#"] != DBNull.Value && drUpd[i]["Full Case Price ea#"].ToString() != "")
                                    dr["price_a"] = drUpd[i]["Full Case Price ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price B - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price B - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price B - Distributor / Ea#"].ToString() != "")
                                    dr["price_b"] = drUpd[i]["Price B - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price C - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price C - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price C - Distributor / Ea#"].ToString() != "")
                                    dr["price_c"] = drUpd[i]["Price C - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Early Order Full Case Price"))
                            {
                                if (drUpd[i]["Early Order Full Case Price"] != DBNull.Value && drUpd[i]["Early Order Full Case Price"].ToString() != "")
                                    dr["price_b"] = drUpd[i]["Early Order Full Case Price"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price D - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price D - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price D - Distributor / Ea#"].ToString() != "")
                                    dr["price_d"] = drUpd[i]["Price D - Distributor / Ea#"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Early Order Broken Case Price"))
                            {
                                if (drUpd[i]["Early Order Broken Case Price"] != DBNull.Value && drUpd[i]["Early Order Broken Case Price"].ToString() != "")
                                    dr["price_d"] = drUpd[i]["Early Order Broken Case Price"].ToString();
                            }
                            if (PDSDS.Tables["PDS"].Columns.Contains("Price E - Distributor / Ea#"))
                            {
                                if (drUpd[i]["Price E - Distributor / Ea#"] != DBNull.Value && drUpd[i]["Price E - Distributor / Ea#"].ToString() != "")
                                    dr["price_e"] = drUpd[i]["Price E - Distributor / Ea#"].ToString();
                            }

                            if (drUpd[i]["Suggested Retail / Ea# "] != null && drUpd[i]["Suggested Retail / Ea# "].ToString().Trim() != "")
                                dr["sugg_retl"] = drUpd[i]["Suggested Retail / Ea# "].ToString();
                            if (drUpd[i]["Suggested Dealer / Ea# "] != null && drUpd[i]["Suggested Dealer / Ea# "].ToString().Trim() != "")
                                dr["sugg_deal"] = drUpd[i]["Suggested Dealer / Ea# "].ToString();

                            if (drUpd[i]["Drop Ship Price / Ea#"] != null && drUpd[i]["Drop Ship Price / Ea#"].ToString().Trim() != "")
                                dr["dropship"] = drUpd[i]["Drop Ship Price / Ea#"].ToString();

                            if (IsValidUPC)
                                lds.Tables[0].Rows.Add(dr);
                            else
                            {
                                DataRow ErrRow;
                                ErrRow = dsErr.Tables[0].NewRow();
                                ErrRow["mfgname"] = mfgname;
                                ErrRow["mfgupc"] = mfgupc;
                                ErrRow["mfgtitle"] = mfgtitle;
                                ErrRow["Type"] = sType;
                                ErrRow["Price"] = PriceChange;
                                ErrRow["Other"] = OtherChange;
                                ErrRow["mfgitem"] = mfgitem;
                                ErrRow["upc"] = upcno;
                                ErrRow["CaseUCC"] = CaseUCC;
                                ErrRow["prod_desc"] = prod_desc;
                                if (!string.IsNullOrEmpty(dist_ea))
                                {
                                    ErrRow["dist_ea"] = dist_ea;
                                }
                                //ErrRow["dist_ea"] = dist_ea;
                                //ErrRow["eff_date"] = eff_date;
                                if (!string.IsNullOrEmpty(eff_date))
                                {
                                    ErrRow["eff_date"] = eff_date;
                                }
                                ErrRow["Status"] = Status;
                                dsErr.Tables[0].Rows.Add(ErrRow);
                            }

                            upcno = "";
                        }
                    }

                    if (dsErr.Tables[0] != null && dsErr.Tables[0].Rows.Count > 0)
                    {
                        chkerr = dsErr.Tables[0].Rows.Count;
                        Session["PDSErr"] = dsErr;
                    }

                    if (lds.Tables[0] != null && lds.Tables[0].Rows.Count > 0)
                    {
                        upccnt = lds.Tables[0].Rows.Count;
                        Session["UpdPDS"] = lds;
                        //string tSQL = "delete from temppds";
                        //Ggrp.Execute(tSQL, Sconn);
                        //lda.Update(lds, "tblUpdPDS");

                        //tSQL = "exec STP_Util_PDSUpdate ''";
                        //Ggrp.Getdataset(tSQL, "tblpds", ds, ref da, Sconn);
                    }
                    else
                    {
                        ErrMsg = "No records found";
                    }
                }
                else
                {
                    ErrMsg = "No records found";
                }
            }
            catch (Exception ex)
            {
                string ErrDesc = "";
                if (!string.IsNullOrEmpty(upcno))
                    ErrDesc = "UPC: " + upcno + "<br/>" + ex.Message;
                else
                    ErrDesc = ex.Message;

                return Json(new { Items = fileName, Msg = ErrDesc, upccnt = upccnt, chkerr = chkerr });
            }

            return Json(new { Items = fileName, Msg = ErrMsg, upccnt = upccnt, chkerr = chkerr });

        }





        public ActionResult UpdatePDSFromTemp(string fileName)
        {
            Sconn = new SqlConnection(sqlcon);
            string ErrMsg = "";
            try
            {

                DataSet tPDS = new DataSet();
                tPDS = (DataSet)Session["UpdPDS"];

                string SQL = "select * from temppds where 1=0";
                DataSet lds = new DataSet();
                SqlDataAdapter lda = new SqlDataAdapter();
                Ggrp.Getdataset(SQL, "tPDS", lds, ref lda, Sconn);

                if (tPDS.Tables[0] != null && tPDS.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < tPDS.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = lds.Tables[0].NewRow();
                        dr = tPDS.Tables[0].Rows[i];
                        lds.Tables[0].Rows.Add(dr.ItemArray);
                    }
                }

                if (lds.Tables[0] != null && lds.Tables[0].Rows.Count > 0)
                {
                    string tSQL = "delete from temppds";
                    Ggrp.Execute(tSQL, Sconn);
                    lda.Update(lds, "tPDS");

                    tSQL = "exec STP_Util_PDSUpdate_V2 ''";
                    Ggrp.Getdataset(tSQL, "tblpds", ds, ref da, Sconn);
                }

            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message;
            }
            return Json(new { Items = fileName, Msg = ErrMsg });
        }

        public ActionResult PrintPDSVerificationReport()
        {
            DataSet PDS = new DataSet();
            PDS = (DataSet)Session["PDSErr"];
            Session["ReportCache"] = null;
            Session["Report_FilterFields"] = null;
            PDS.Tables[0].TableName = "Results";
            Session["dsReport"] = PDS;
            Session["Description"] = "PDS Verification";
            string RPTFileName = "RP_PDSVerification.repx";
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });
        }

        public ActionResult DeletePDSTempFile(string fname)
        {
            string fullfilename = ConfigurationManager.AppSettings["ExportFilesPath"].ToString() + "\\" + fname;
            System.IO.FileInfo file = new System.IO.FileInfo(fullfilename);
            if (file.Exists)
            {
                try
                {
                    System.IO.File.Delete(fullfilename);
                }
                catch (Exception) { }
                return Json(new { Items = "" });
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Create DBBackup

        public ActionResult CreateDBBackup()
        {
            return View();
        }

        public ActionResult CreateBackup(string db)
        {
            string dbname = "", SQL = "", filename = "";
            dbname = db;
            SQL = "STP_CreateDBBackup '" + dbname + "'";
            SqlConnection Conn = new SqlConnection(sqlcon);
            DataSet dsDB = new DataSet();
            SqlDataAdapter daDB = new SqlDataAdapter();
            Ggrp.Getdataset(SQL, "DBBackup", dsDB, ref daDB, Conn);


            if (dsDB != null && dsDB.Tables["DBBackup"] != null && dsDB.Tables["DBBackup"].Rows.Count > 0)
            {
                if (dsDB.Tables["DBBackup2"] != null)
                    filename = dsDB.Tables["DBBackup2"].Rows[0][0].ToString();
                else
                    filename = dsDB.Tables["DBBackup"].Rows[0][0].ToString();
            }

            string sfname = "", ZipFileName = "", FolderPath = "";
            FolderPath = System.Configuration.ConfigurationManager.AppSettings["DBFilesPath"].ToString() + "\\";
            sfname = FolderPath + filename;
            sfname = sfname.Replace(".BAK", ".ZIP"); ;
            System.IO.FileInfo file = new System.IO.FileInfo(sfname);
            if (file.Exists)
            {
                System.IO.File.Delete(sfname);
            }

            using (ZipFile zip = new ZipFile())
            {
                filename = filename.Replace(".ZIP", ".BAK");
                ZipFileName = FolderPath + filename;
                zip.AddFile(ZipFileName, "");
                filename = filename.Replace(".BAK", ".ZIP");
                zip.Save(FolderPath + filename);
            }

            return Json(new { Items = filename });
        }

        public FileContentResult GetDBFileFromServer(string fname) //FileStreamResult
        {
            string FileName = System.Configuration.ConfigurationManager.AppSettings["DBFilesPath"].ToString() + "\\" + fname;

            System.IO.FileInfo file = new System.IO.FileInfo(FileName);
            if (file.Exists)
            {
                byte[] bytes = null;
                try
                {
                    bytes = System.IO.File.ReadAllBytes(FileName);
                    System.IO.File.Delete(FileName);

                    //Delete .BAK file
                    string bakfname = "";
                    bakfname = fname.Replace(".ZIP", ".BAK");
                    bakfname = System.Configuration.ConfigurationManager.AppSettings["DBFilesPath"].ToString() + "\\" + bakfname;
                    System.IO.FileInfo bakfile = new System.IO.FileInfo(bakfname);
                    if (bakfile.Exists)
                        System.IO.File.Delete(bakfname);
                }
                catch (Exception ex)
                {
                    string ErrDesc = (ex.InnerException != null && ex.InnerException.Message != null && ex.InnerException.Message.ToString().Trim() != "") ? ex.InnerException.Message : "";
                    if (string.IsNullOrEmpty(ErrDesc))
                        ErrDesc = ex.Message;
                    SqlConnection conn = new SqlConnection(sqlcon);
                    string message = ErrDesc;
                    if (message.Length > 300)
                        message = message.Replace("'", "''").Substring(0, 300);
                    else
                        message = message.Replace("'", "''");
                    string stack = ex.TargetSite.ToString();
                    string insertSQL = "insert into OP_ErrorLog(ErrSource,ErrDesc,ErrStackTrace,InnerErrSource,InnerErrDesc,InnerErrStackTrace,ErrLogdatetime,MachineName) values"
                           + "('GetDBFileFromServer','" + message + "','" + stack + "'"
                           + ",'" + ex.InnerException + "','','','" + DateTime.Now + "','')";
                    Ggrp.Execute(insertSQL, conn);

                }
                return File(bytes, "image/jpeg", fname);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region CheckDigit

        public ActionResult UPCCheckDigit()
        {
            return View();
        }

        public ActionResult UPCCheckDigitValidate(string code)
        {
            string lastdigit = code.Substring(code.Length - 1, 1);
            string checkdigit = "", Status = "";
            checkdigit = Ggrp.ValidateUPC(code);
            if (checkdigit != lastdigit)
            {
                if (checkdigit.Trim().ToUpper() == "INVALID")
                    Status = "Invalid character";
                else
                    Status = "Check digit failed";
            }
            return Json(new { Items = checkdigit, Status = Status });
        }

        #endregion

    }


}