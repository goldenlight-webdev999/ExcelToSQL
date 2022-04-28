using DevExpress.Web;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.InternalAccess;
using DevExpress.XtraReports.UI;
using GroGroup.Class;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using VedasNs;
using ExportToExcel;
using System.Globalization;
using System.Net;
using GroGroup.Filters;

namespace GroGroup.Controllers
{
    [CustomActionFilter]
    [CustomActionException]
    public class ReportsController : Controller
    {

        string sqlcon = System.Configuration.ConfigurationManager.AppSettings["GroGroup"].ToString();
        DataSet ds = new DataSet();
        SqlDataAdapter da = new SqlDataAdapter();
        SqlConnection Sconn;
        Aptus Ggrp = new Aptus();
        string Sqlqry;
        DataSet dsReport;
        SqlDataAdapter daReport = new SqlDataAdapter();
        XtraReport xRpt = new XtraReport();
        List<ArrayList> newval = new List<ArrayList>();
        string ExecutiveVerificationPDFNAME = "";

        # region GridView

        public ActionResult GridView(string GuId)
        {
            if (Session["Description" + GuId] != null)
                ViewBag.Description = Session["Description" + GuId].ToString();
            ViewBag.GUID = GuId;
            if (Session["GridViewDS" + GuId] != null)
                return View(Session["GridViewDS" + GuId]);
            else
                return View();
        }

        public ActionResult GridViewPartial(string GuId)
        {
            ViewBag.GUID = GuId;
            ViewBag.Description = Session["Description" + GuId].ToString();
            return PartialView(Session["GridViewDS" + GuId]);
        }

        public ActionResult GridReportExport(string Printvalue = "", string GuId = "")
        {
            if (Session["GridViewDS" + GuId] == null)
                return Json(new { Items = "" });

            DataTable dt = new DataTable();
            string strDlgFilter = "", rootpath = "", file_name = "", filepath = "", Message = "";
            dt = (DataTable)Session["GridViewDS" + GuId];
            if (dt.Columns.Contains("SELECT"))
            {
                DataView DvEmail = new DataView(dt);
                if (DvEmail.Count == 0)
                {
                    DvEmail = new DataView(dt);
                    Message = "No records to export";
                }
                dt = DvEmail.ToTable();
            }

            if (!string.IsNullOrEmpty(Printvalue))
            {
                rootpath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "ExportFiles\\";
                Session["rootpath"] = rootpath;
                switch (Printvalue.ToUpper())
                {
                    case "HTML":
                        strDlgFilter = ".html";
                        break;
                    case "MHT":
                        strDlgFilter = ".mht";
                        break;
                    case "PDF":
                        strDlgFilter = ".pdf";
                        break;
                    case "XLS":
                        strDlgFilter = ".xls";
                        break;
                    case "XLSX":
                        strDlgFilter = ".xlsx";
                        break;
                    case "RTF":
                        strDlgFilter = ".rtf";
                        break;
                    case "TXT":
                        strDlgFilter = ".txt";
                        break;
                    case "CSV":
                        strDlgFilter = ".csv";
                        break;
                    case "XML":
                        strDlgFilter = ".xml";
                        break;
                }
            }

            file_name = Regex.Replace(Session["Description" + GuId].ToString(), "[\\~#%&*{}/:<>?|\"-]+", "") + "_" + DateTime.Now.ToString("MM_dd_yyy_HH_mm_ss") + strDlgFilter;
            Session["Filetext"] = Regex.Replace(Session["Description" + GuId].ToString(), "[\\~#%&*{}/:<>?|\"-]+", "");
            Session["Exporttype"] = Printvalue;
            filepath = rootpath + file_name.ToString().Trim();
            Session["Filename"] = file_name;
            Session["File_path"] = filepath;
            Export(dt, Printvalue);
            int count = dt.Rows.Count;
            Sconn = new SqlConnection(sqlcon);
            if (Session["ReplaceFilter" + GuId] == null)
                Session["ReplaceFilter" + GuId] = "";
            if (Session["Description" + GuId] == null)
                Session["Description" + GuId] = "";
            if (Session["Filename"] == null)
                Session["Filename"] = "";
            return Json(new { Items = "success", Message = Message });
        }

        private void Export(DataTable Dt, string ExportType)
        {
            string filename = "", filepath = "";
            filepath = Session["File_path"].ToString();
            ExporterClass parentExpClass = new ExporterClass();
            switch (ExportType.ToUpper())
            {
                case "HTML":
                    parentExpClass.destfileType = ExporterClass.fileType.HTM;
                    break;
                case "MHT":
                    break;
                case "PDF":
                    parentExpClass.destfileType = ExporterClass.fileType.HTM;
                    break;
                case "XLS":
                    parentExpClass.destfileType = ExporterClass.fileType.XLS;
                    break;
                case "XLSX":
                    parentExpClass.destfileType = ExporterClass.fileType.XLSX;
                    break;
                case "RTF":
                    break;
                case "TXT":
                    parentExpClass.destfileType = ExporterClass.fileType.TXT;
                    break;
                case "CSV":
                    parentExpClass.destfileType = ExporterClass.fileType.CSV;
                    break;
                case "XML":
                    parentExpClass.destfileType = ExporterClass.fileType.XML;
                    break;
            }
            if (ExportType == "RTF")
            {
                try
                {
                    GridView GridView1 = new GridView();
                    GridView1.AllowPaging = false;
                    GridView1.DataSource = Dt;
                    GridView1.DataBind();
                    Response.ContentEncoding = System.Text.Encoding.UTF7;
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter hw = new HtmlTextWriter(sw);
                    GridView1.RenderControl(hw);
                    string strfileName = Session["Filename"].ToString();
                    string path = Session["rootpath"].ToString();
                    FileStream MyStream = new FileStream(path + strfileName, FileMode.Create);
                    System.IO.StreamWriter MyWriter = new System.IO.StreamWriter(MyStream);
                    MyWriter.Write(sw.ToString());
                    MyWriter.Close();
                    MyStream.Close();
                    Response.End();
                    parentExpClass = null;
                    return;
                }
                catch (Exception ex)
                {
                }
            }
            DataSet dsExport = new DataSet();
            dsExport.Tables.Add(Dt.Copy());
            if (ExportType.Trim().ToUpper() == "XLSX")
            {
                bool blnFlag = false;
                blnFlag = CreateExcelFile.CreateExcelDocument(dsExport, filepath);
            }
            else
            {
                string Expres = parentExpClass.exportData(dsExport, parentExpClass.destfileType, filepath);
                if (ExportType == "PDF")
                {
                    filename = filepath.Substring(filepath.IndexOf(Session["Filetext"].ToString()));
                    string html = System.IO.File.ReadAllText((HttpContext.Server.MapPath("~/ExportFiles/" + filename)));
                    EO.Pdf.HtmlToPdf.Options.BaseUrl = AppDomain.CurrentDomain.BaseDirectory + @"ExportFiles\";
                    EO.Pdf.HtmlToPdf.ConvertHtml(html, AppDomain.CurrentDomain.BaseDirectory + @"ExportFiles\" + Session["Filename"]);
                }
            }
            parentExpClass = null;
        }

        public ActionResult GetFileFromDisk()
        {
            string filename = "", filepath = "", filetype = "";
            filepath = Session["File_path"].ToString();
            filename = filepath.Substring(filepath.IndexOf(Session["Filetext"].ToString()));
            filetype = Session["Exporttype"].ToString();
            if (filetype == "MHT") { return Json(new { Items = "" }); }
            var bytes = System.IO.File.ReadAllBytes(filepath);
            System.IO.File.Delete(filepath);
            return File(bytes, filetype, filename);
        }

        #endregion


        #region ReportView

        public ActionResult ReportView(string GuId)
        {
            ViewBag.GUID = GuId;
            return View();
        }

        public ActionResult ReportViewerPartial(string GuId)
        {
            if (Session["DataTableModel" + GuId] != null)
            {
                ViewData["Report"] = GetReport(GuId);
            }
            else
                ViewData["Report"] = new XtraReport1();
            return PartialView("ReportViewerPartial");
        }

        public ActionResult ExportReportViewerPartial(string GuId)
        {
            XtraReport exportreport = new XtraReport();
            if (Session["DataTableModel" + GuId] != null)
            {
                exportreport = GetReport(GuId);
            }
            Sconn = new SqlConnection(sqlcon);
            if (Session["ReplaceFilter" + GuId] == null)
                Session["ReplaceFilter" + GuId] = "";
            if (Session["ActivityType"] == null)
                Session["ActivityType"] = "";
            if (Session["Description" + GuId] == null)
                Session["Description" + GuId] = "";
            if (Session["Filename"] == null)
                Session["Filename"] = "";
            // Vedas.ExporterClass.ActivitychangeLog(Session["nusername"].ToString(), Session["Location"].ToString(), Session["ClientMachineName"].ToString(), Session["Description"].ToString(), Session["ReplaceFilter"].ToString(), Session["ActivityType"].ToString(), Session["ReportCount"].ToString(), "", Sconn);
            return DevExpress.Web.Mvc.ReportViewerExtension.ExportTo(exportreport);
        }


        public FileContentResult GetExecutiveVerificationPages(string fname)
        {
            string FileName = System.Configuration.ConfigurationManager.AppSettings["PdfFilesPath"].ToString() + "\\" + fname;

            try
            {
                System.IO.FileInfo file = new System.IO.FileInfo(FileName);
                if (file.Exists)
                {
                    var bytes = System.IO.File.ReadAllBytes(FileName);
                    try
                    {
                        System.IO.File.Delete(FileName);
                    }
                    catch (Exception) { }
                    return File(bytes, "application/pdf", fname);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult GenerateExecutiveVerificationReport(string GuId)
        {
            List<string> FileNameList = new List<string>();

            string filePath = "", fname = "";
            filePath = System.Configuration.ConfigurationManager.AppSettings["PdfFilesPath"].ToString() + "\\";
            string GetDistributor = "";
            string Pdfname = "";
            if (Session["ParamTitle"] != null && Session["ParamTitle"].ToString() != "")
                ExecutiveVerificationPDFNAME = Session["ParamTitle"].ToString().Trim();
            if (ExecutiveVerificationPDFNAME != "")
            {
                Pdfname = Regex.Replace(ExecutiveVerificationPDFNAME, "[\\~#%&*{}/:<>?|\"-]+", "");
            }
            else
            {
                if (Session["lwhereParameter" + GuId] != null)
                    GetDistributor = Session["lwhereParameter" + GuId].ToString();
                if (GetDistributor != null && GetDistributor.ToString() != "" && GetDistributor.Contains("Distributor="))
                {
                    String value = GetDistributor;
                    Char delimiter = '=';
                    String[] substrings = value.Split(delimiter);
                    Sconn = new SqlConnection(sqlcon);
                    DataSet dsc = new DataSet();
                    DataSet xmlds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    int indexOfFirst = substrings[2].IndexOf('\'');
                    string Distname = substrings[2].Remove(indexOfFirst, 1);
                    int indexOfLast = Distname.LastIndexOf('\'');
                    Distname = Distname.Remove(indexOfLast, 1);
                    //string Distname= substrings[2].ToString().Remove(1).Remove
                    Ggrp.Getdataset("select company from Distributor where company ='" + Ggrp.SQLString(Distname) + "'", "getdist", ds, ref da, Sconn);
                    if (ds.Tables["getdist"].Rows.Count > 0)
                    {
                        Pdfname = ds.Tables["getdist"].Rows[0][0].ToString().Replace('.', ' ').Replace("'", " ");
                    }

                }
            }


            DataSet dsReport = (DataSet)Session["dsSubReport" + GuId];
            if (Pdfname.ToString() != "")
                fname = Pdfname + "_EVP_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            else
                fname = "RP_ExecutiveVerificationPurchase" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            Session["EReportCache" + GuId] = null;
            CreateExecutiveVerificationReport(fname, "RP_ExecutiveVerificationPurchase.repx", dsReport.Tables["Res"], GuId);
            FileNameList.Add(fname);
            if (Pdfname.ToString() != "")
                fname = Pdfname + "_EVS_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            else
                fname = "RP_ExecutiveVerificationSales" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            Session["EReportCache" + GuId] = null;
            CreateExecutiveVerificationReport(fname, "RP_ExecutiveVerificationSales.repx", dsReport.Tables["Res1"], GuId);
            FileNameList.Add(fname);
            if (Pdfname.ToString() != "")
                fname = Pdfname + "_EVPD_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            else
                fname = "RP_ExecutiveVerificationPurchaseDetails" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            Session["EReportCache" + GuId] = null;
            CreateExecutiveVerificationReport(fname, "RP_ExecutiveVerificationPurchaseDetails.repx", dsReport.Tables["Res2"], GuId);
            FileNameList.Add(fname);

            if (Pdfname.ToString() != "")
                fname = Pdfname + "_EVSD_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            else
                fname = "RP_ExecutiveVerificationSalesDetails" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            Session["EReportCache" + GuId] = null;
            CreateExecutiveVerificationReport(fname, "RP_ExecutiveVerificationSalesDetails.repx", dsReport.Tables["Res3"], GuId);
            FileNameList.Add(fname);
            fname = MergeExecutiveVerificationPdf(FileNameList, Pdfname);
            return Json(new { Items = "success", fname = fname });
        }


        private string MergeExecutiveVerificationPdf(List<string> fNameList, string Pdfname = "")
        {
            string filePath = "", fname = "";
            filePath = System.Configuration.ConfigurationManager.AppSettings["PdfFilesPath"].ToString() + "\\";
            MergePDF objMergePdf = new MergePDF();

            for (int i = 0; i < fNameList.Count; i++)
            {
                fname = fNameList[i].ToString();
                if (System.IO.File.Exists(filePath + fname))
                {
                    objMergePdf.AddFile(filePath + fname);
                }
            }
            if (Pdfname != "")
                fname = Pdfname.Trim() + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            else
                fname = "RP_ExecutiveVerification_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".pdf";
            objMergePdf.DestinationFile = filePath + fname;
            objMergePdf.Execute();
            return fname;
        }

        private string CreateExecutiveVerificationReport(string reportName, string repxName, DataTable result,string GuId)
        {
            string lFilter = "", filePath = "";
            filePath = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + repxName;
            XtraReport1 report = new XtraReport1(filePath, Session["DataTableModel"+ GuId]);
            try
            {
                DataTable dsReport1 = result;
                report.DataSourceDemanded += (s, e) =>
                {
                    ((XtraReport)s).DataSource = dsReport1;
                };

                if (Session["EReportCache"+ GuId] == null)
                {
                    foreach (DevExpress.XtraReports.Parameters.Parameter ffd in report.Parameters)
                    {
                        if (ffd.Name.Trim().ToUpper() == "CLUBID")
                        {
                            ffd.Value = "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "FILTER")
                        {
                            if (Session["lwhereParameter" + GuId] != null)
                                lFilter = Session["lwhereParameter" + GuId].ToString();
                            lFilter = lFilter.Replace("M.", "");
                            lFilter = lFilter.Replace("'", "");
                            if (lFilter != "")
                            {
                                ffd.Value = lFilter;
                            }
                        }
                        if (ffd.Name.Trim().ToUpper() == "USERNAME")
                        {
                            ffd.Value = (Session["UserName"] != null && Session["UserName"].ToString() != "") ? Session["UserName"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "PARAMTITLE")
                        {
                            ffd.Value = (Session["ParamTitle"] != null && Session["ParamTitle"].ToString() != "") ? Session["ParamTitle"].ToString() : Session["Description"].ToString();
                        }
                        if (ffd.Name.Trim().ToUpper() == "VPCCURRENTYEAR")
                        {
                            ffd.Value = (Session["vpccurrentyear"] != null && Session["vpccurrentyear"].ToString() != "") ? Session["vpccurrentyear"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "VPCLASTYEAR")
                        {
                            ffd.Value = (Session["vpclastyear"] != null && Session["vpclastyear"].ToString() != "") ? Session["vpclastyear"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "LOCATIONID")
                        {
                            ffd.Value = (Session["location"] != null && Session["location"].ToString() != "") ? Session["location"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "PYTODATE")
                        {
                            ffd.Value = (Session["PYToDate"] != null && Session["PYToDate"].ToString() != "") ? Session["PYToDate"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "PYEND")
                        {
                            ffd.Value = (Session["PYEnd"] != null && Session["PYEnd"].ToString() != "") ? Session["PYEnd"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "PERIOD1")
                        {
                            ffd.Value = (Session["PERIOD1"] != null && Session["PERIOD1"].ToString() != "") ? Session["PERIOD1"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "PERIOD2")
                        {
                            ffd.Value = (Session["PERIOD2"] != null && Session["PERIOD2"].ToString() != "") ? Session["PERIOD2"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "PERIOD3")
                        {
                            ffd.Value = (Session["PERIOD3"] != null && Session["PERIOD3"].ToString() != "") ? Session["PERIOD3"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "CURRENTYTD")
                        {
                            ffd.Value = (Session["currentYtd"] != null && Session["currentYtd"].ToString() != "") ? Session["currentYtd"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "LASTYTD")
                        {
                            ffd.Value = (Session["lastytd"] != null && Session["lastytd"].ToString() != "") ? Session["lastytd"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "LASTMTH")
                        {
                            ffd.Value = (Session["lastmth"] != null && Session["lastmth"].ToString() != "") ? Session["lastmth"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "REPORTNAME")
                        {
                            ffd.Value = Session["Description" + GuId].ToString();
                        }
                        if (ffd.Name.Trim().ToUpper() == "CURPRGYEAR")
                        {
                            ffd.Value = (Session["CurPrgYear"] != null && Session["CurPrgYear"].ToString() != "") ? Session["CurPrgYear"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "LASTPRGYEAR")
                        {
                            ffd.Value = (Session["LastPrgYear"] != null && Session["LastPrgYear"].ToString() != "") ? Session["LastPrgYear"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "CURRENTMONTH")
                        {
                            ffd.Value = (Session["CurrentMonth"] != null && Session["CurrentMonth"].ToString() != "") ? Session["CurrentMonth"].ToString() : "";
                        }
                        if (ffd.Name.Trim().ToUpper() == "HOLDPAYMENT")
                        {
                            ffd.Value = (Session["HoldPayment"] != null && Session["HoldPayment"].ToString() != "") ? Session["HoldPayment"].ToString() : "";
                        }
                    }

                    try
                    {
                        report.CreateDocument();

                        string Directorypath = System.Configuration.ConfigurationManager.AppSettings["PdfFilesPath"].ToString();
                        string pdfname = "";
                        if (!System.IO.Directory.Exists(Directorypath))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(Directorypath);
                        }

                        pdfname = System.Configuration.ConfigurationManager.AppSettings["PdfFilesPath"].ToString() + "\\" + reportName;
                        report.ExportToPdf(pdfname);

                        ms = new MemoryStream();
                        PrintingSystemAccessor.SaveIndependentPages(report.PrintingSystem, ms);
                        Session["EReportCache" + GuId] = ms;
                    }
                    catch (Exception ex)
                    {
                        return "error";
                    }
                }
                else
                {
                    ms = (MemoryStream)Session["EReportCache" + GuId];
                    PrintingSystemAccessor.LoadVirtualDocument(report.PrintingSystem, ms);
                }
                return "success";
            }
            catch (Exception)
            {
                return "error";
            }
        }

        MemoryStream ms;
        XtraReport GetReport(string GuId)
        {
            string lFilter = "";
            XtraReport1 report = new XtraReport1(Session["ReportName" + GuId].ToString(), Session["DataTableModel" + GuId]);
            try
            {
                DataTable dsReport1 = (DataTable)Session["DataTableModel" + GuId];
                DataSet dsSubReport = (DataSet)Session["dsSubReport" + GuId];
                report.DataSourceDemanded += (s, e) =>
                {
                    ((XtraReport)s).DataSource = dsReport1;
                };

                string Repname = "";
                if (Session["ReportName" + GuId].ToString().Contains("DevExpReports"))
                {
                    string[] splitrepname = Session["ReportName" + GuId].ToString().Split(new string[] { "DevExpReports\\" }, StringSplitOptions.None);
                    Repname = splitrepname[1];
                }

                if (Session["ReportCache" + GuId] == null)
                {
                    foreach (DevExpress.XtraReports.Parameters.Parameter ffd in report.Parameters)
                    {
                        if (ffd.Name.ToString().ToUpper() == "CLUBID")
                        {
                            ffd.Value = "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "FILTER")
                        {
                            if (Session["lwhereParameter" + GuId] != null)
                                lFilter = Session["lwhereParameter" + GuId].ToString();
                            lFilter = lFilter.Replace("M.", "");
                            lFilter = lFilter.Replace("'", "");
                            if (lFilter != "")
                            {
                                ffd.Value = lFilter;
                            }
                        }
                        if (ffd.Name.ToString().ToUpper() == "USERNAME")
                        {
                            ffd.Value = (Session["UserName"] != null && Session["UserName"].ToString() != "") ? Session["UserName"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "FAX")
                        {
                            ffd.Value = (Session["Fax"] != null && Session["Fax"].ToString() != "") ? Session["Fax"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "EMAIL")
                        {
                            ffd.Value = (Session["Email"] != null && Session["Email"].ToString() != "") ? Session["Email"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "PARAMTITLE")
                        {
                            ffd.Value = (Session["ParamTitle"] != null && Session["ParamTitle"].ToString() != "") ? Session["ParamTitle"].ToString() : Session["Description"].ToString();
                        }
                        if (ffd.Name.ToString().ToUpper() == "VPCCURRENTYEAR")
                        {
                            ffd.Value = (Session["vpccurrentyear"] != null && Session["vpccurrentyear"].ToString() != "") ? Session["vpccurrentyear"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "VPCLASTYEAR")
                        {
                            ffd.Value = (Session["vpclastyear"] != null && Session["vpclastyear"].ToString() != "") ? Session["vpclastyear"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "LOCATIONID")
                        {
                            ffd.Value = (Session["location"] != null && Session["location"].ToString() != "") ? Session["location"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "PYTODATE")
                        {
                            ffd.Value = (Session["PYToDate"] != null && Session["PYToDate"].ToString() != "") ? Session["PYToDate"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "PYEND")
                        {
                            ffd.Value = (Session["PYEnd"] != null && Session["PYEnd"].ToString() != "") ? Session["PYEnd"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "PERIOD1")
                        {
                            ffd.Value = (Session["PERIOD1"] != null && Session["PERIOD1"].ToString() != "") ? Session["PERIOD1"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "PERIOD2")
                        {
                            ffd.Value = (Session["PERIOD2"] != null && Session["PERIOD2"].ToString() != "") ? Session["PERIOD2"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "PERIOD3")
                        {
                            ffd.Value = (Session["PERIOD3"] != null && Session["PERIOD3"].ToString() != "") ? Session["PERIOD3"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "CURRENTYTD")
                        {
                            ffd.Value = (Session["currentYtd"] != null && Session["currentYtd"].ToString() != "") ? Session["currentYtd"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "LASTYTD")
                        {
                            ffd.Value = (Session["lastytd"] != null && Session["lastytd"].ToString() != "") ? Session["lastytd"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "LASTMTH")
                        {
                            ffd.Value = (Session["lastmth"] != null && Session["lastmth"].ToString() != "") ? Session["lastmth"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "REPORTNAME")
                        {
                            ffd.Value = Session["Description" + GuId].ToString();
                        }
                        if (ffd.Name.Trim().ToUpper() == "MFGTYPE")
                        {
                            ffd.Value = (Session["mfgtype"] != null && Session["mfgtype"].ToString() != "") ? Session["mfgtype"].ToString() : "";
                        }
                    }
                    XRPictureBox pictureBox = new XRPictureBox();
                    if (((XRPictureBox)report.Report.FindControl("xrPictureBox1", false)) != null)
                    {
                        if (System.IO.File.Exists(Server.MapPath("~/images/" + "GrogroupRep.png")))
                        {
                            pictureBox.Image = System.Drawing.Image.FromFile(Server.MapPath("~/Images/" + "GrogroupRep.png"));
                            ((XRPictureBox)report.Report.FindControl("xrPictureBox1", false)).Image = pictureBox.Image;
                            ((XRPictureBox)report.Report.FindControl("xrPictureBox1", false)).Visible = true;
                            ((XRPictureBox)report.Report.FindControl("xrPictureBox1", false)).Height = 40;
                            ((XRPictureBox)report.Report.FindControl("xrPictureBox1", false)).Width = 80;
                            ((XRPictureBox)report.Report.FindControl("xrPictureBox1", false)).Sizing = ImageSizeMode.AutoSize;
                        }
                    }
                    switch (Repname.ToUpper())
                    {
                        case "RP_MFRDATASHEET.REPX":
                        case "RP_MFRDATASHEETWITHSIGN.REPX":
                        case "RP_SPPBYDISTRIBUTOR.REPX":
                        case "RP_NIGHTSUMMARY.REPX":
                        case "RP_MDSBPOPRG.REPX":
                            setSubReport(dsSubReport, report, Repname.ToUpper());
                            break;
                    }
                    string defaultfilename = Session["Description" + GuId] + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
                    defaultfilename = Regex.Replace(defaultfilename, "[^0-9a-zA-Z:,]+", " ");
                    defaultfilename = Regex.Replace(defaultfilename, ",", "");
                    report.Name = defaultfilename;
                    report.CreateDocument();
                    ms = new MemoryStream();
                    PrintingSystemAccessor.SaveIndependentPages(report.PrintingSystem, ms);
                    Session["ReportCache" + GuId] = ms;
                }
                else
                {
                    ms = (MemoryStream)Session["ReportCache" + GuId];
                    PrintingSystemAccessor.LoadVirtualDocument(report.PrintingSystem, ms);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return report;
        }
        public void setSubReport(DataSet ds, XtraReport xRpt, string Repname)
        {
            try
            {
                string Subrep = "", filePath = "", pbosubrep = "", prgsubrep = "";
                filePath = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\";
                XtraReport subreport1 = new XtraReport();
                switch (Repname.ToUpper())
                {

                    case "RP_SPPBYDISTRIBUTOR.REPX":
                        Subrep = filePath + "RP_SubSPPDist.repx";
                        subreport1.LoadLayout(Subrep.ToString());
                        subreport1.DataSource = ds.Tables["Res1"];
                        //subreport1.DataSource = ds.Tables["GridRes"];
                        ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport1;
                        ((XRSubreport)xRpt.FindControl("subreport1", true)).BeforePrint += new System.Drawing.Printing.PrintEventHandler(SubrepSPPDist_BeforePrint);
                        break;
                    case "RP_MFRDATASHEET.REPX":
                    case "RP_MFRDATASHEETWITHSIGN.REPX":
                        Subrep = filePath + "RP_mfrSubdatasheet.repx";
                        subreport1.LoadLayout(Subrep.ToString());
                        subreport1.DataSource = ds.Tables["Res1"];
                        ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport1;
                        ((XRSubreport)xRpt.FindControl("subreport1", true)).BeforePrint += new System.Drawing.Printing.PrintEventHandler(Subrep_BeforePrint);
                        break;
                    case "RP_NIGHTSUMMARY.REPX":
                        Subrep = filePath + "RP_Sub_NightSummary.repx";
                        XtraReport subreport = new XtraReport();
                        //string Subrep = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + "RP_Sub_NightSummary.repx";
                        subreport.LoadLayout(Subrep.ToString());
                        subreport.DataSource = ds.Tables["Res1"];
                        ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport;
                        break;
                    case "RP_MDSBPOPRG.REPX":
                        Subrep = filePath + "RP_mfrSubdatasheet.repx";
                        subreport1.LoadLayout(Subrep.ToString());
                        subreport1.DataSource = ds.Tables["Res1"];
                        ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport1;
                        Band bnd = subreport1.Bands.GetBandByType(typeof(PageFooterBand));

                        if (Session["MdsBpoPrgSign"].ToString().ToUpper() == "TRUE")
                            pbosubrep = "RP_BpoSign.repx";
                        else
                            pbosubrep = "RP_Prgoinfo.repx";

                        XtraReport subreport2 = new XtraReport();
                        string Subrep2 = filePath + pbosubrep;
                        subreport2.LoadLayout(Subrep2.ToString());
                        subreport2.DataSource = ds.Tables["Res2"];
                        ((XRSubreport)xRpt.FindControl("subreport2", true)).ReportSource = subreport2;
                        Band bnd1 = subreport2.Bands.GetBandByType(typeof(PageFooterBand));
                        bnd1.Visible = false;

                        if (Session["MdsBpoPrgSign"].ToString().ToUpper() == "TRUE")
                            prgsubrep = "RP_ProgRefGuidewithSign.repx";
                        else
                            prgsubrep = "RP_ProgRefGuide.repx";

                        XtraReport subreport3 = new XtraReport();
                        string Subrep3 = filePath + prgsubrep;
                        subreport3.LoadLayout(Subrep3.ToString());
                        subreport3.DataSource = ds.Tables["Res3"];
                        ((XRSubreport)xRpt.FindControl("subreport3", true)).ReportSource = subreport3;
                        Band bnd2 = subreport3.Bands.GetBandByType(typeof(PageFooterBand));
                        bnd2.Visible = false;
                        break;
                }

            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
            }

        }

        private void Subrep_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (((XRSubreport)sender) != null && ((XRSubreport)sender).Report != null && ((XRSubreport)sender).Report.GetCurrentColumnValue("mfgno") != null && ((XRSubreport)sender).Report.GetCurrentColumnValue("mfgno").ToString() != "")
            {
                ((XRSubreport)sender).ReportSource.FilterString = "mfgno = '" + ((XRSubreport)sender).Report.GetCurrentColumnValue("mfgno").ToString() + "'";
            }
        }

        private void SubrepSPPDist_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (((XRSubreport)sender) != null && ((XRSubreport)sender).Report != null && ((XRSubreport)sender).Report.GetCurrentColumnValue("upc") != null && ((XRSubreport)sender).Report.GetCurrentColumnValue("upc").ToString() != "" && ((XRSubreport)sender).Report.GetCurrentColumnValue("MfgNo") != null && ((XRSubreport)sender).Report.GetCurrentColumnValue("MfgNo").ToString() != "")
            {
                ((XRSubreport)sender).ReportSource.FilterString = "upc = '" + ((XRSubreport)sender).Report.GetCurrentColumnValue("upc").ToString() + "' and MfgNo='" + ((XRSubreport)sender).Report.GetCurrentColumnValue("MfgNo").ToString() + "'";
            }
        }

        #region Reportsearch

        public ActionResult Lsearch(string table, string tosearch, string toassign, string text)
        {
            Sconn = new SqlConnection(sqlcon);
            ds = new DataSet();
            if (text.Contains("'"))
                text = text.Replace("'", "''");
            int count = 0;
            string sql = "";
            sql = "select " + toassign + "," + tosearch + " from " + table + " where " + tosearch + " like '" + text + "%' ";

            Ggrp.Getdataset(sql, "lsearch", ds, ref da, Sconn);
            if (ds != null && ds.Tables["lsearch"].Rows.Count == 1)
            {
                foreach (DataRow dRow in ds.Tables["lsearch"].Rows)
                {
                    ArrayList values = new ArrayList();
                    foreach (object value in dRow.ItemArray)
                        values.Add(value);
                    newval.Add(values);
                }
                count = 1;
            }
            return Json(new { Items = newval, Count = count });
        }

        public ActionResult LsearchLookupfieldList(string lookupfields = "")
        {
            string ColField = "", colinx = "";
            string[] colvalues = null;
            string[] colindex = null;
            char sep = '=';
            if (lookupfields.Contains("~~~"))
            {
                string[] colu_DispValues = Regex.Split(lookupfields, "~~~");
                colu_DispValues = colu_DispValues.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                for (int i = 0; i < colu_DispValues.Count(); i++)
                {
                    string[] col_name = new string[3];
                    char[] delimiters = new char[] { '^', '^' };
                    string[] temp = colu_DispValues[i].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    Array.Copy(temp, 0, col_name, 0, 3);
                    ColField += "," + col_name[0].Trim().ToString();
                    colinx += "," + col_name[1].ToString();
                }
            }
            else
            {
                string[] colu_DispValues = Regex.Split(lookupfields, ";");
                colu_DispValues = colu_DispValues.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                for (int i = 0; i < colu_DispValues.Length; i++)
                {
                    string[] split = colu_DispValues[i].ToString().Split(sep).Where(s => !String.IsNullOrEmpty(s)).ToArray();
                    if (split.Length > 0)
                    {
                        string colfield = split[1].ToString();
                        string[] colfieldary = Regex.Split(colfield, ",");
                        ColField += "," + split[0].Trim().ToString();
                        colinx += "," + colfieldary[0].ToString();
                    }
                }
            }
            ColField = ColField.Substring(1);
            colinx = colinx.Substring(1);
            colvalues = ColField.Split(',');
            colindex = colinx.Split(',');
            return Json(new { Itemsfield = colinx, Itemsheader = ColField });
        }

        # endregion

        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            int destWidth = 130;
            int destHeight = 50;
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }

        #endregion

        #region Report Control

        public DataSet CallMethod(Hashtable HstParam, string MethodName, string ReportNameToLoad)
        {
            DataSet DsCustomRpt = new DataSet();
            vCustomReports obj = new vCustomReports(sqlcon);
            obj = new vCustomReports(ReportNameToLoad, "");

            Type Typ = obj.GetType();
            object[] UserParameters = null;
            MethodInfo theMethod = Typ.GetMethod(MethodName);
            if (theMethod == null || (!CheckMethod(theMethod)))
            {
                throw new Exception("Method Not Found or Access denied");
            }
            UserParameters = new object[1];
            UserParameters[0] = (object)HstParam;
            try
            {
                DsCustomRpt = (DataSet)theMethod.Invoke(obj, UserParameters);
                Ggrp.SendErrMessage = obj.SendErrMessage;
            }
            catch (Exception E)
            {
            }
            return DsCustomRpt;
        }

        #endregion

        #region Reports

        public ActionResult OPfrmNewRepWizard()
        {
            Session["KeyNameMfgNo"] = "";
            Session["ShowIntelligentFilter"] = false;
            Session["Viewallcolumns"] = false;
            ViewBag.Nuser = ""; //Session["nusername"].ToString();
            ViewBag.Location = ""; //Session["Location"].ToString();
            ViewBag.AdminMem = ""; //Session["isAdminEmployee"].ToString();
            string guid = Guid.NewGuid().ToString();
            ViewBag.GUID = guid;
            return View();
        }

        public ActionResult GetIntelligentFilter()
        {
            Sconn = new SqlConnection(sqlcon);
            string lsql = "", UserPreference = "";
            bool ShowIntelligentFilter = false, Viewallcolumns = false;
            lsql = "select top 1 * from OP_UserPreference where (EmployeeNo='" + Session["employeeno"].ToString() + "' or GroupId in (select groupid from OP_Employee_Groups where EmployeeID='" + Session["employeeno"].ToString() + "')) and Preftype='UserPreference' order by EmployeeNo desc";

            DataSet dsc = new DataSet();
            DataSet xmlds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();

            Ggrp.Getdataset(lsql, "UserPreference", dsc, ref da, Sconn);
            UserPreference = dsc.Tables["UserPreference"] != null && dsc.Tables["UserPreference"].Rows.Count > 0 ? dsc.Tables["UserPreference"].Rows[0]["Preference"].ToString() : "";
            if (!string.IsNullOrEmpty(UserPreference))
            {
                StringReader stread = new StringReader(UserPreference);
                xmlds.ReadXml(stread);
                if (xmlds != null && xmlds.Tables.Count > 0)
                {
                    for (int i = 0; i < xmlds.Tables.Count; i++)
                    {
                        if (xmlds.Tables.Contains("ShowIntelligentFilter") && xmlds.Tables[i].TableName.ToString().ToUpper() == "SHOWINTELLIGENTFILTER")
                        {
                            ShowIntelligentFilter = xmlds.Tables[i].Rows[0][0] != DBNull.Value ? Convert.ToBoolean(xmlds.Tables[i].Rows[0][0].ToString()) : false;
                            Session["ShowIntelligentFilter"] = ShowIntelligentFilter;
                            continue;
                        }
                        if (xmlds.Tables.Contains("Viewallcolumns") && xmlds.Tables[i].TableName.ToString().ToUpper() == "VIEWALLCOLUMNS")
                        {
                            Viewallcolumns = xmlds.Tables[i].Rows[0][0] != DBNull.Value ? Convert.ToBoolean(xmlds.Tables[i].Rows[0][0].ToString()) : false;
                            Session["Viewallcolumns"] = Viewallcolumns;
                            continue;
                        }
                    }
                }
            }
            ShowIntelligentFilter = Session["ShowIntelligentFilter"] != null ? (bool)Session["ShowIntelligentFilter"] : false;
            Viewallcolumns = Session["Viewallcolumns"] != null ? (bool)Session["Viewallcolumns"] : false;
            return Json(new { ShowIntelligentFilter = ShowIntelligentFilter, Viewallcolumns = Viewallcolumns });
        }

        public ActionResult ReportDescriptionOnload(string EditSecPointsNO)
        {
            string descript = "";
            DataSet dds = new DataSet();
            SqlDataAdapter dda = new SqlDataAdapter();
            Sconn = new SqlConnection(sqlcon);
            string reportdesc = "select Reportdescription from  op_security_points where Secpointsno='" + EditSecPointsNO + "'";
            Ggrp.Getdataset(reportdesc, "Descript", dds, ref dda, Sconn);
            if (dds.Tables["Descript"].Rows.Count > 0)
                descript = dds.Tables["Descript"].Rows[0]["Reportdescription"].ToString();
            return Json(new { Items = descript });
        }

        public ActionResult ReportDescriptionClick(string RepName, string EditSecPointsNO)
        {
            Sconn = new SqlConnection(sqlcon);
            string rptdesc = "update op_security_points set Reportdescription='" + RepName + "' where Secpointsno='" + EditSecPointsNO + "'";
            Ggrp.Execute(rptdesc, Sconn);
            return Json(new { Items = "" });
        }

        public ActionResult RepSecpointsGroup()
        {
            Sconn = new SqlConnection(sqlcon);
            List<Hashtable> repgroup = new List<Hashtable>();
            string SQl = "";
            SQl = "select A.SecPointsNo, a.* from  OP_Security_Points A where A.SecurityID like 'REPORTGROUP%' order by A.SecurityID";

            Ggrp.Getdataset(SQl, "repgroup", this.ds, ref this.da, Sconn);
            repgroup = Ggrp.CovertDatasetToHashTbl(this.ds.Tables["repgroup"]);
            return Json(new { Items = repgroup });
        }

        public ActionResult RepSecpointsList(string um)
        {
            Sconn = new SqlConnection(sqlcon);
            List<Hashtable> replist = new List<Hashtable>();
            string SQl = "";
            SQl = "select A.SecPointsNo, a.* from  OP_Security_Points A where A.UserModule='" + um + "' and A.SecurityID like 'ReportName_%' and A.SecurityID not like '%-OPT-%' order by A.SecurityID,Description";
            Ggrp.Getdataset(SQl, "rightstbl", this.ds, ref this.da, Sconn);
            replist = Ggrp.CovertDatasetToHashTbl(this.ds.Tables["rightstbl"]);
            return Json(new { Items = replist });
        }

        public ActionResult ReportSelectedall(string userModule = "", string description = "")
        {
            Sconn = new SqlConnection(sqlcon);
            bool showFilter = false, viewCols = false;
            List<Hashtable> ReportSelectedall = new List<Hashtable>();
            string Sql = "select * from OP_ReportSelect where UserModule='" + userModule + "' and Description ='" + description + "'";
            Ggrp.Getdataset(Sql, "Reportselect", ds, ref da, Sconn);


            string meetcode = "", stdate = "";
            DataSet lds = new DataSet();

            if (ds.Tables["Reportselect"].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables["Reportselect"].Rows.Count; i++)
                {
                    if (ds.Tables["Reportselect"].Rows[i]["defaultvalue"].ToString().Trim().ToUpper() == "OP_SYSTEM")
                    {
                        lds = new DataSet();
                        string lsql = "select " + ds.Tables["Reportselect"].Rows[i]["Field_Name"].ToString().Trim() + " from OP_system ";
                        Ggrp.Getdataset(lsql, "OP_System", lds, ref da, Sconn);
                        if (lds.Tables["OP_System"].Rows.Count > 0)
                            ds.Tables["Reportselect"].Rows[i]["defaultvalue"] = lds.Tables["OP_System"].Rows[0][0].ToString();
                    }
                    else if (ds.Tables["Reportselect"].Rows[i]["defaultvalue"].ToString().Contains(","))
                    {
                        string[] Field;
                        string test = ds.Tables["Reportselect"].Rows[i]["defaultvalue"].ToString().Trim();
                        Field = test.Split(',');

                        lds = new DataSet();
                        string lsql = "select " + Field[1] + " from " + Field[0];
                        Ggrp.Getdataset(lsql, "OP_System", lds, ref da, Sconn);
                        if (lds.Tables["OP_System"].Rows.Count > 0)
                            ds.Tables["Reportselect"].Rows[i]["defaultvalue"] = lds.Tables["OP_System"].Rows[0][0].ToString();
                    }
                    else if (ds.Tables["Reportselect"].Rows[i]["defaultvalue"].ToString().Contains("select"))
                    {
                        string test = ds.Tables["Reportselect"].Rows[i]["defaultvalue"].ToString().Trim();
                        lds = new DataSet();
                        string lsql = test;
                        Ggrp.Getdataset(lsql, "tbltemp", lds, ref da, Sconn);
                        if (lds.Tables["tbltemp"].Rows.Count > 0)
                            ds.Tables["Reportselect"].Rows[i]["defaultvalue"] = lds.Tables["tbltemp"].Rows[0][0].ToString();
                    }
                }
            }
            ReportSelectedall = Ggrp.CovertDatasetToHashTbl(ds.Tables["Reportselect"]);
            showFilter = (bool)Session["ShowIntelligentFilter"];
            viewCols = (bool)Session["Viewallcolumns"];
            return Json(new { Items = ReportSelectedall, ShowIntelligentFilter = showFilter, Viewallcolumns = viewCols, MeetCode = meetcode, StartDate = stdate });
        }

        public ActionResult ReportLookupList(string lookupid, string field1 = "", string field2 = "", string where = "")
        {
            string filds = "", ssqlAc = "";
            if (!string.IsNullOrEmpty(field1) && !string.IsNullOrEmpty(field2))
                filds = field1 + "," + field2;
            else
                filds = "'' as entry,'' as descript union select entry as entry, descript as descript";
            Sconn = new SqlConnection(sqlcon);
            DataSet oDs = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            if (!string.IsNullOrEmpty(where))
            {
                if (lookupid == "Catcode")
                {
                    ssqlAc = "select entry,(entry+'-'+descript) as descript,descript as orderbydesc from OP_Lookup  where lookupid ='Catcode' and isnull(inactive,0)<>1 order by orderbydesc";
                }
                else
                {
                    ssqlAc = "select " + filds + " from OP_Lookup " + where;
                }
            }
            else
                ssqlAc = "select " + filds + " from OP_Lookup where lookupid='" + lookupid + "'";
            Ggrp.Getdataset(ssqlAc, "lukuplist", oDs, ref da, Sconn);
            List<Hashtable> LookupList = new List<Hashtable>();
            LookupList = Ggrp.CovertDatasetToHashTbl(oDs.Tables["lukuplist"]);
            var jsonResult = Json(new { Items = LookupList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult ReportMfgList(string field1 = "", string field2 = "", string where = "")
        {
            string filds = "", ssqlAc = "";
            filds = field1 + "," + field2;
            Sconn = new SqlConnection(sqlcon);
            DataSet oDs = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            ssqlAc = "select " + filds + " from mfg order by company";
            Ggrp.Getdataset(ssqlAc, "mfglist", oDs, ref da, Sconn);
            List<Hashtable> MFGList = new List<Hashtable>();
            MFGList = Ggrp.CovertDatasetToHashTbl(oDs.Tables["mfglist"]);
            var jsonResult = Json(new { Items = MFGList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult ReportLookuptableList(string Lookuptable, string field1 = "", string field2 = "", string where = "")
        {
            string filds = "", ssqlAc = "";
            if (!string.IsNullOrEmpty(field1) && !string.IsNullOrEmpty(field2))
                filds = field1 + "," + field2;
            if (Lookuptable == "Distributor")
                filds = "distinct(" + field2 + ") as ddtlink," + field1;
            Sconn = new SqlConnection(sqlcon);
            DataSet oDs = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            if (!string.IsNullOrEmpty(where))
                ssqlAc = "select " + filds + " from " + Lookuptable + " " + where;
            Ggrp.Getdataset(ssqlAc, "lukuplist", oDs, ref da, Sconn);
            List<Hashtable> LookupList = new List<Hashtable>();
            LookupList = Ggrp.CovertDatasetToHashTbl(oDs.Tables["lukuplist"]);
            var jsonResult = Json(new { Items = LookupList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult FillLookupValuesCombo(string lkupValues)
        {
            string LookupValues = lkupValues.Trim(); //"Over 30:31, Over 60:61, Over 90:91";
            string[] LookupValuesArr;
            DataSet ds = new DataSet();
            DataTable dtLookup = new DataTable();
            dtLookup.TableName = "FillSINGLOC";
            dtLookup.Columns.Add("Descript");
            dtLookup.Columns.Add("entry");
            if (ds.Tables.Contains("FillSINGLOC")) ds.Tables.Remove("FillSINGLOC");
            ds.Tables.Add(dtLookup);
            LookupValuesArr = LookupValues.Split(',');
            for (int l = 0; l < LookupValuesArr.Length; l++)
            {
                DataRow dr = ds.Tables["FillSINGLOC"].NewRow();
                string[] arr = LookupValuesArr[l].Split(':');
                dr["Descript"] = arr[0].ToString().Trim();
                dr["entry"] = arr[1].ToString().Trim();
                dtLookup.Rows.Add(dr);
            }
            List<Hashtable> LookupList = new List<Hashtable>();
            LookupList = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillSINGLOC"]);
            var jsonResult = Json(new { Items = LookupList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult FillLookupValuesDateCalc()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select Entry,Descript from op_lookup where lookupid = 'DateCalc'";
            Ggrp.Getdataset(sql, "DateCalc", ds, ref da, Sconn);
            List<Hashtable> LookupList = new List<Hashtable>();
            LookupList = Ggrp.CovertDatasetToHashTbl(ds.Tables["DateCalc"]);
            var jsonResult = Json(new { Items = LookupList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult ReportQry(string Repname, string Filter, string UserModule, string Description, string controls, string qry = "", bool viewcolumns = false, string FilterColname = "", string sortby = "", string SecPointsNo = "", string SqlWhere = "", string ReportviewCol = "", string getreport = "", string GuId = "")
        {
            string Reportviewcol = ""; string prmtitle = "";
            Session["MdsBpoPrgSign"] = "";
            Session["ParamTitle"] = "";
            Session["getreport" + GuId] = getreport;
            List<Hashtable> Filledgrid = new List<Hashtable>();
            try
            {
                Sconn = new SqlConnection(sqlcon);
                if (Filter.ToString().ToUpper().Trim() == "ORDER BY NAME DESC" || Filter.ToString().ToUpper().Trim() == "ORDER BY LOOKUPID, ENTRY")
                {
                    Filter = "1=1 " + Filter;
                }
                if (Description.ToString().ToUpper() == "PRODUCT GROUP")
                {
                    if (Filter.ToUpper().Contains("MANUFACTURER"))
                    {
                        string fltr = "";
                        string inputstr = Filter;
                        //string result = inputstr.Replace(@"Reporttype = 'Manufacturer' and ", "");
                        string result = "";
                        if (Filter.Contains(" and Reporttype = 'Manufacturer'"))
                        {
                            result = inputstr.Replace(@" and Reporttype = 'Manufacturer' ", " ");
                        }
                        else
                        {
                            result = inputstr.Replace(@"Reporttype = 'Manufacturer'", " ");
                        }
                        if (result.EndsWith("and"))
                        {
                            fltr = result.Remove(result.Length - 3);
                        }
                        else if (result.EndsWith("and "))
                        {
                            fltr = result.Remove(result.Length - 4);
                        }
                        else
                            fltr = result;
                        Filter = fltr;

                        Repname = "RP_ProductGroupwithinMFR.repx";
                    }
                    else if (Filter.ToUpper().Contains("INDEX"))
                    {

                        //string fltr = "";
                        //string inputstr = Filter;
                        //string result = inputstr.Replace(@"Reporttype = 'Index' and ", "");
                        //if (result.EndsWith("and"))
                        //{
                        //    fltr = result.Remove(result.Length - 3);
                        //}
                        //else if (result.EndsWith("and "))
                        //{
                        //    fltr = result.Remove(result.Length - 4);
                        //}
                        //else
                        //    fltr = result;
                        //Filter = fltr;

                        // qry = "select distinct(substring(entry,0,3)) as grpby,entry,descript,comment+''+comment2+''+note1+''+note2 as Inculdes from OP_lookup where lookupid like 'catcode%'";

                        qry = " select distinct(substring(OL.entry,0,3)) as grpby,OL.entry,OL.descript,OL.comment+''+OL.comment2+''+OL.note1+''+OL.note2 as Inculdes, OS.company as GGCompany,OS.address1 as GGaddress,(rtrim(OS.city) + ', ' + OS.state + ' ' + OS.zip) as GGCSZ, OS.phone as GGphone from OP_lookup OL  left join OP_system OS on 1 = 1 where lookupid like 'catcode%'";

                        Repname = "RP_ProductGroupByIndex.repx";
                        Filter = "";
                    }
                    else if (Filter.ToUpper().Contains("TITLE"))
                    {
                        string fltr = "";
                        string inputstr = Filter;
                        string result = "";
                        if (Filter.Contains(" and Reporttype = 'Title' "))
                        {
                            result = inputstr.Replace(@" and Reporttype = 'Title' ", "");
                        }
                        else
                        {
                            result = inputstr.Replace(@"Reporttype = 'Title'", "");
                        }

                        result = result.Replace(@"order by M.MfgNo,M.company,descript", "");
                        if (result.EndsWith("and"))
                        {
                            fltr = result.Remove(result.Length - 3);
                        }
                        else if (result.EndsWith("and "))
                        {
                            fltr = result.Remove(result.Length - 4);
                        }
                        else
                            fltr = result;
                        if (fltr == null || fltr == " ")
                            Filter = "";
                        else
                            Filter = "and " + fltr;

                        //qry = "select P.catcode,company,M.MfgNo,webpage,Op.descript from mfg m left join Product p on M.MfgNo = p.MfgNo left join OP_lookup OP on P.catcode = op.entry where op.lookupid = 'Catcode' "+ Filter + " group by p.catcode,company,M.MfgNo,webpage,Op.descript order by P.catcode,descript,company";
                        //qry= "  select P.catcode,m.company,M.MfgNo,webpage,Op.descript ,  OS.company as GGCompany,OS.address1 as GGaddress,(rtrim(OS.city)+', '+ OS.state+' '+OS.zip) as GGCSZ, OS.phone as GGphone from mfg m  left join OP_system OS on 1 = 1  left  join Product p on M.MfgNo = p.MfgNo left  join OP_lookup OP on P.catcode = op.entry   where op.lookupid = 'Catcode' "+ Filter + " group by p.catcode,company,M.MfgNo,webpage,Op.descript order by P.catcode,descript,company";

                        qry = " select distinct(m.company), P.catcode,M.MfgNo,webpage,Op.descript ,  OS.company as GGCompany,OS.address1 as GGaddress,(rtrim(OS.city)+', '+ OS.state+' '+OS.zip) as GGCSZ, OS.phone as GGphone from mfg m  left join OP_system OS on 1 = 1  left   join Product p on M.MfgNo = p.MfgNo left join OP_lookup OP on P.catcode = op.entry where op.lookupid = 'Catcode' " + Filter + " order by P.catcode,descript,m.company";

                        Repname = "RP_ProductGroupByTitle.repx";
                        Filter = "";
                    }
                    else
                    {

                    }
                }
                else if (Description.ToString().ToUpper() == "PRODUCT GROUP AUDIT SALES")
                {
                    Repname = "RP_ProductGroupAuditSales.repx";

                }
                else if (Description.ToString().ToUpper() == "PRODUCT GROUP MFR. WITHIN PRODUCT GROUP")
                {
                    Repname = "RP_ProductGroupMfrWithinProductGroup.repx";
                }
                else if (Description.ToString().ToUpper() == "MANUFACTURER UPC PARTNER")
                {
                    Repname = "RP_ManufacturerUPCsPartner.repx";
                }



                if (Description.ToString().ToUpper() == "BASIC PROGRAM OUTLINE" || Description.ToString().ToUpper() == "MANUFACTURER DATA SHEET" || Description.ToString().ToUpper() == "PROGRAM REFERENCE GUIDE" || Description.ToString().ToUpper() == "MDS, BPO & PRG REPORTS")
                {
                    if (Filter.ToUpper().Contains("BPOSIGN = 'N'"))
                    {
                        string fltr = "";
                        string inputstr = Filter;
                        string result = inputstr.Replace(@"BPOSIGN = 'N'", "");
                        if (result.EndsWith("and"))
                        {
                            fltr = result.Remove(result.Length - 3);
                        }
                        else if (result.EndsWith("and "))
                        {
                            fltr = result.Remove(result.Length - 4);
                        }
                        else
                            fltr = result;
                        Filter = fltr;
                        if (Description.ToString().ToUpper() == "BASIC PROGRAM OUTLINE")
                        {
                            Repname = "RP_Prgoinfo.repx";
                        }
                        if (Description.ToString().ToUpper() == "MANUFACTURER DATA SHEET")
                        {
                            Repname = "RP_MfrDataSheet.repx";
                        }
                        if (Description.ToString().ToUpper() == "PROGRAM REFERENCE GUIDE")
                        {
                            Repname = "RP_ProgRefGuide.repx";
                        }
                        if (Description.ToString().ToUpper() == "MDS, BPO & PRG REPORTS")
                        {
                            Session["MdsBpoPrgSign"] = false;
                            prmtitle = "MdsBpoPrgWithOutSign";
                        }
                    }
                    else
                    {
                        string fltr = "";
                        string inputstr = Filter;
                        string result = inputstr.Replace(@"BPOSIGN = 'Y'", "");
                        if (result.EndsWith("and"))
                        {
                            fltr = result.Remove(result.Length - 3);
                        }
                        else if (result.EndsWith("and "))
                        {
                            fltr = result.Remove(result.Length - 4);
                        }
                        else
                            fltr = result;
                        Filter = fltr;
                        if (Description.ToString().ToUpper() == "BASIC PROGRAM OUTLINE")
                        {
                            Repname = "RP_BpoSign.repx";
                        }
                        if (Description.ToString().ToUpper() == "MANUFACTURER DATA SHEET")
                        {
                            Repname = "RP_MfrDataSheetwithSign.repx";
                        }
                        if (Description.ToString().ToUpper() == "PROGRAM REFERENCE GUIDE")
                        {
                            Repname = "RP_ProgRefGuidewithSign.repx";
                        }
                        if (Description.ToString().ToUpper() == "MDS, BPO & PRG REPORTS")
                        {
                            Session["MdsBpoPrgSign"] = true;
                            prmtitle = "MdsBpoPrgWithSign";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(prmtitle))
                    Session["ParamTitle"] = prmtitle;
                else
                    Session["ParamTitle"] = "   ";

                Session["lwhereParameter" + GuId] = FilterColname;
                Session["KeyNameMfgNo" + GuId] = "";
                Session["KeyNameDistNo" + GuId] = "";
                string SQLGroupby = "", SQLOrderby = "";

                if (qry == "")
                {
                    return Json(new { Items = "" });
                }
                JavaScriptSerializer opser = new JavaScriptSerializer();
                Hashtable HstParam = opser.Deserialize<Hashtable>(controls);
                Session["ReportCache" + GuId] = null;
                string paratitle = "";

                Session["PYToDate"] = "";
                Session["PYEnd"] = "";
                Session["currentYtd"] = "";
                Session["lastmth"] = "";
                Session["lastytd"] = "";
                Session["CurrentMonth"] = "";

                /*
                if (Repname.ToString().ToUpper() == "RP_MANUFACTURERPARTNER.REPX")
                {
                    Session["ParamTitle"] = null;
                    bool IsPartners = false, IsNonPartners = false;
                    string clscd = HstParam["classcd"].ToString();
                    string[] words = clscd.Split(',');
                    if (words.Length > 0)
                    {
                        var Partners = (from s in words where s.Contains("V") || s.Contains("D") || s.Contains("N") select s).ToList();
                        if (Partners != null && Partners.Count > 0) IsPartners = true;
                        var NonPartners = (from s in words where !s.Contains("V") && !s.Contains("D") && !s.Contains("N") select s).ToList();
                        if (NonPartners != null && NonPartners.Count > 0)
                        {
                            if (NonPartners[0] != "")
                                IsNonPartners = true;
                            else
                                IsNonPartners = false;
                        }

                        if (IsPartners && IsNonPartners)
                        {
                            paratitle = "Prospects and Partner's";
                            Session["ParamTitle"] = paratitle;
                        }
                        if (IsPartners && !IsNonPartners)
                        {
                            paratitle = "Partner's";
                            Session["ParamTitle"] = paratitle;
                        }
                        if (IsNonPartners && !IsPartners)
                        {
                            paratitle = "Prospects";
                            Session["ParamTitle"] = paratitle;
                        }
                        if (!IsPartners && !IsNonPartners)
                        {
                            paratitle = " ";
                            Session["ParamTitle"] = paratitle;
                        }
                    }
                }*/

                string YearMonth = "", Year = "", Month = "", monthName = "", PYToDate = "";
                string ytds = "", ytdeYear = "", ytdeMonth = "", ytdemonthName = "", PYEnd = "";
                int ytd = 0, ytde = 0;

                switch (Repname)
                {
                    case "RP_TopSalesbyUPCRankedwithinMfg.repx":
                        string paramtype = HstParam["type"].ToString();
                        if (HstParam["type"].ToString() == "Dollar")
                            paramtype = "$$";
                        paratitle = "Sales UPC Ranked by Top " + HstParam["rows"] + " Items by " + paramtype + " within Mfg";
                        Session["ParamTitle"] = paratitle;
                        Session["mfgtype"] = "";
                        if (HstParam["mfgtype"].ToString() == "Partner")
                            Session["mfgtype"] = HstParam["mfgtype"];
                        YearMonth = HstParam["cym"].ToString();
                        Year = YearMonth.Substring(0, 4);
                        Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                        monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                        PYToDate = monthName + "-" + Year;
                        Session["PYToDate"] = PYToDate;

                        if (Convert.ToInt32(Month) < 8)
                            ytd = Convert.ToInt32(Year) - 1;
                        else
                            ytd = Convert.ToInt32(Year) + 0 * 100;

                        ytds = ytd.ToString() + "08";
                        ytde = Convert.ToInt32(ytds) + 99;
                        ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                        ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                        ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);

                        PYEnd = ytdemonthName + "-" + ytdeYear;
                        Session["PYEnd"] = PYEnd;
                        break;
                    case "RP_PurchasestoVPC.repx":
                        string currentperiod = HstParam["period"].ToString();
                        string[] lastperiod = currentperiod.Split('-');
                        int lyear = Convert.ToInt32(lastperiod[0]) - 1;
                        string vpclastyear = lyear.ToString() + "-" + lastperiod[0];
                        Session["vpccurrentyear"] = currentperiod;
                        Session["vpclastyear"] = vpclastyear;
                        break;
                    case "RP_Notes.repx":
                        string PDuedate = "", PDuedate1 = "";
                        if (HstParam.Contains("N.duedate"))
                        {
                            if (HstParam.Contains("N.duedate") && HstParam.Contains("EN.duedate"))
                            {
                                PDuedate = HstParam["N.duedate"].ToString();
                                PDuedate1 = HstParam["EN.duedate"].ToString();
                                Session["PERIOD1"] = PDuedate;
                                Session["PERIOD2"] = PDuedate1;
                            }
                            else if (HstParam.Contains("N.duedate") && !HstParam.Contains("EN.duedate"))
                            {
                                PDuedate = HstParam["N.duedate"].ToString();
                                Session["PERIOD1"] = PDuedate;
                                Session["PERIOD2"] = "  ";
                            }
                            else
                            {
                                Session["PERIOD1"] = "  ";
                                Session["PERIOD2"] = "  ";
                            }
                        }
                        break;
                    case "RP_NationalGroupProgram.repx":
                        if (HstParam.Contains("period"))
                        {
                            if (HstParam["period"].ToString() != "")
                            {
                                string[] splitfname = HstParam["period"].ToString().Split('-');
                                paratitle = splitfname[0] + "/" + splitfname[1];
                                Session["ParamTitle"] = paratitle;
                            }
                            else
                                Session["ParamTitle"] = "   ";
                        }
                        break;
                    case "RP_GroGrpDataworksheet.repx":
                        string lsql = "select Fax from OP_system ";
                        Ggrp.Getdataset(lsql, "OP_System", ds, ref da, Sconn);
                        Session["Fax"] = ds.Tables["OP_System"].Rows[0][0].ToString();
                        break;
                    case "RP_PurchaseByMfgDistributor.repx":
                        YearMonth = HstParam["cym"].ToString();
                        Year = YearMonth.Substring(0, 4);
                        Month = YearMonth.Substring(YearMonth.Length - 2, 2);

                        if (Convert.ToInt32(Month) < 8)
                            ytd = Convert.ToInt32(Year) - 1;
                        else
                            ytd = Convert.ToInt32(Year) + 0 * 100;

                        ytds = "08-" + ytd.ToString();

                        int lastyearstart = ytd - 1;
                        string lastyearstart1 = "08-" + lastyearstart;
                        string lastyearend = "07-" + ytd;

                        Session["currentYtd"] = ytds;
                        Session["lastytd"] = lastyearstart1;
                        Session["lastmth"] = "thru " + lastyearend;
                        paratitle = "Distributor Purchase by ";
                        if (HstParam != null && HstParam.ContainsKey("MfgClass"))
                        {
                            Sconn = new SqlConnection(sqlcon);
                            string Sql = "select descript from OP_lookup where lookupid ='CLCD' and isnull(inactive,0)<>1 and entry='" + HstParam["MfgClass"].ToString() + "'";
                            Ggrp.Getdataset(Sql, "mfgclass", ds, ref da, Sconn);
                            paratitle += ds.Tables["mfgclass"].Rows[0]["descript"].ToString();
                            Session["ParamTitle"] = paratitle;
                        }
                        else
                            Session["ParamTitle"] = "Distributor Purchase by Prospect";
                        break;
                    case "RP_PurchaseSummaryByMfg.repx":
                        paratitle = "Purchase Summary - " + HstParam["ReportType"];
                        Session["ParamTitle"] = paratitle;
                        break;
                    case "RP_VpcCountMfgs.repx":
                        string period1 = HstParam["period"].ToString();
                        Session["PERIOD1"] = period1;
                        break;
                    case "RP_MfgBenefitSummaryFiscalyear.repx":
                        Session["PERIOD1"] = null; Session["PERIOD2"] = null; Session["PERIOD3"] = null;
                        Session["PERIOD1"] = HstParam["period1"];
                        Session["PERIOD2"] = HstParam["period2"];
                        Session["PERIOD3"] = HstParam["period3"];
                        break;
                    case "RP_MerchandiseProgFiscalYr.repx":

                        Session["PERIOD1"] = null; Session["PERIOD2"] = null;

                        if (HstParam["distNO"] != null && HstParam["distNO"].ToString() != "")
                        {
                            string fdt = HstParam["period1"].ToString();
                            string[] ffdt = fdt.Split('-');
                            string sfdate = "11/01/" + ffdt[0].ToString();
                            string ssfdate = "10/31/" + ffdt[1].ToString();

                            string edt = HstParam["period2"].ToString();
                            string[] eedt = edt.Split('-');
                            string efdate = "11/01/" + eedt[0].ToString();
                            string eefdate = "10/31/" + eedt[1].ToString();

                            Session["PERIOD1"] = sfdate + " - " + ssfdate;
                            Session["PERIOD2"] = efdate + " - " + eefdate;
                        }
                        else
                        {
                            return Json(new { Items = "Please select a distributor." });
                        }
                        
                        //Session["PERIOD1"] = HstParam["period1"];
                        //Session["PERIOD2"] = HstParam["period2"];

                        break;
                    case "RP_BenefitSummary.repx":
                        Session["ParamTitle"] = null;
                        if (HstParam.Contains("distNO").ToString() == "True" && HstParam["distNO"].ToString() != "")
                        {
                            Sconn = new SqlConnection(sqlcon);
                            string Sql = "select company from distributor where distno ='" + HstParam["distNO"].ToString() + "'";
                            Ggrp.Getdataset(Sql, "Distname", ds, ref da, Sconn);
                            paratitle = ds.Tables["Distname"].Rows[0]["company"].ToString();
                        }
                        else
                        {
                            paratitle = "  ";
                        }
                        Session["ParamTitle"] = paratitle;
                        Session["PERIOD1"] = null; Session["PERIOD2"] = null; Session["PERIOD3"] = null;
                        Session["PERIOD1"] = HstParam["period1"];
                        Session["PERIOD2"] = HstParam["period2"];
                        Session["PERIOD3"] = HstParam["period3"];
                        break;
                    case "RP_BenefitTransPymbyBatch.repx":
                        Session["ParamTitle"] = null;
                        if (HstParam["batch"].ToString() != "" && HstParam["batchto"].ToString() != "")
                            paratitle = "Batch No From: " + HstParam["batch"] + " to " + HstParam["batchto"] + "";
                        else if (HstParam["batch"].ToString() != "")
                            paratitle = "Batch No From: " + HstParam["batch"] + "";
                        else
                            paratitle = "  ";
                        Session["ParamTitle"] = paratitle;
                        break;
                    case "RP_BenefitReconMgfByDist.repx":
                        Session["HoldPayment"] = "";
                        if (HstParam.Contains("ProgNo").ToString() != null && HstParam["ProgNo"].ToString() != "")
                        {
                            string HoldPayment = "";
                            Sconn = new SqlConnection(sqlcon);
                            string Sql = "select holdpay from benefitcheck where progno='" + HstParam["ProgNo"].ToString() + "'";
                            Ggrp.Getdataset(Sql, "chkholdpay", ds, ref da, Sconn);
                            if (ds.Tables["chkholdpay"].Rows.Count > 0)
                            {
                                HoldPayment = ds.Tables["chkholdpay"].Rows[0]["holdpay"].ToString();
                                if (HoldPayment == "True")
                                    Session["HoldPayment"] = "HOLD payment";
                            }
                        }
                        break;
                    case "Rp_MfgBenefit.repx":
                        Session["ParamTitle"] = null;
                        if (HstParam["BATCH"].ToString() != "")
                            paratitle = "Batch No: " + HstParam["BATCH"] + "";
                        else
                            paratitle = "  ";
                        Session["ParamTitle"] = paratitle;
                        break;
                    case "Rp_MfgBenefitDistributorTransmittal.repx":
                        Session["ParamTitle"] = null;
                        if (HstParam["BATCH"].ToString() != "")
                            paratitle = "Batch No: " + HstParam["BATCH"] + "";
                        else
                            paratitle = "  ";
                        Session["ParamTitle"] = paratitle;
                        break;
                    case "Rp_MfgChecksPaid.repx":
                        Session["ParamTitle"] = null;
                        if (HstParam["BATCH"].ToString() != "")
                            paratitle = "Included In This Batch: " + HstParam["BATCH"] + "";
                        else
                            paratitle = "  ";
                        Session["ParamTitle"] = paratitle;
                        break;
                    case "RP_ExecutiveVerificationPurchase.repx":
                    case "RP_ExecutiveVerificationSales.repx":
                        Sconn = new SqlConnection(sqlcon);
                        DataSet Dsval = new DataSet();
                        SqlDataAdapter daval = new SqlDataAdapter();
                        string sql = "select company from distributor where ddtlink='" + HstParam["distid"] + "'";
                        Ggrp.Getdataset(sql, "distributorlist", Dsval, ref daval, Sconn);
                        paratitle = Dsval.Tables["distributorlist"].Rows[0]["company"].ToString();
                        Session["ParamTitle"] = paratitle;
                        ExecutiveVerificationPDFNAME = paratitle;
                        string currentPrgYtd = HstParam["cym"].ToString();

                        //string currentPrgYear = currentPrgYtd.Substring(0, 4);
                        //int NxtYear = Convert.ToInt32(currentPrgYear) + 1;
                        //int PrvYear = Convert.ToInt32(currentPrgYear) - 1;
                        //string CurPrgYear = currentPrgYear + "-08 thru " + NxtYear + "-07";
                        //string LastPrgYear = PrvYear + "-08 thru " + currentPrgYear + "-07";


                        string sqldate = "select * from fnt_ReturnDatesforExecutiveVerification('" + HstParam["cym"] + "')";
                        Ggrp.Getdataset(sqldate, "TblDateRegion", Dsval, ref daval, Sconn);

                        string stlytds = Dsval.Tables["TblDateRegion"].Rows[0]["lytds"].ToString();
                        string stlytde = Dsval.Tables["TblDateRegion"].Rows[0]["lytde"].ToString();
                        string stytds = Dsval.Tables["TblDateRegion"].Rows[0]["ytds"].ToString();
                        string stytde = Dsval.Tables["TblDateRegion"].Rows[0]["ytde"].ToString();

                        string currentPrgYear = stytds.Substring(0, 4);
                        string NxtYear = stytde.Substring(0, 4);
                        string lstYear = stlytds.Substring(0, 4);

                        string CurPrgYear = currentPrgYear + "-08 thru " + NxtYear + "-07";
                        string LastPrgYear = lstYear + "-08 thru " + currentPrgYear + "-07";

                        Session["CurPrgYear"] = CurPrgYear;
                        Session["LastPrgYear"] = LastPrgYear;

                        YearMonth = HstParam["cym"].ToString();

                        Year = YearMonth.Substring(0, 4);
                        Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                        monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                        PYToDate = monthName + "-" + Year;
                        Session["PYToDate"] = PYToDate;

                        string CurrentMonth = Year + "-" + Month;
                        Session["CurrentMonth"] = CurrentMonth;

                        if (Convert.ToInt32(Month) < 8)
                            ytd = Convert.ToInt32(Year) - 1;
                        else
                            ytd = Convert.ToInt32(Year) + 0 * 100;

                        ytds = ytd.ToString() + "08";
                        ytde = Convert.ToInt32(ytds) + 99;
                        ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                        ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                        ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);
                        PYEnd = ytdemonthName + "-" + ytdeYear;
                        Session["PYEnd"] = PYEnd;
                        break;
                    case "RP_PurchaseSummaryByMfgType.repx":
                    case "RP_PurchaseSummaryByDistributorByMonth.repx":
                    case "RP_PurchaseSummaryByDistributorandMfg.repx":
                    case "RP_SalesSummaryByMfgType.repx":
                    case "RP_SalesSummaryByDistributorbymonth.repx":
                    case "RP_SalesSummaryByDistributorandMfg.repx":
                    case "RP_PSDistributorSummary.repx":
                    case "RP_PSDistributorSummarybyMfg.repx":
                    case "RP_PurchaseSummaryByMfgPartner.repx":
                    case "RP_SalesSummaryByMfgCompany.repx":
                    case "RP_PSMfgSummary.repx":
                    case "RP_PSManufacturerSummaryByDistributor.repx":
                    case "RP_SalesSummaryByMfg.repx":
                    case "RP_SalesCategoryunAssignedSummary.repx":
                    case "RP_SalesCategoryunAssignedDetail.repx":
                        if (Repname.ToUpper() == "RP_UPCWITHOUTCATEGORY.REPX")
                        {
                            paratitle = "Category is Spaces for Mfg - " + HstParam["mfgGroup"];
                            Session["ParamTitle"] = paratitle;
                        }
                        if (Repname.ToUpper() == "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFG.REPX")
                        {
                            paratitle = "Purchases - Partners";

                            if (HstParam["ReportType"].ToString().ToUpper() == "NON-PARTNER")
                            {
                                paratitle = "Purchases - Non-Partners";
                                Repname = "RP_PurchaseSummaryByDistributorandMfgNonPartner.repx";
                            }
                            if (HstParam["ReportType"].ToString().ToUpper() == "COMPANY")
                            {
                                paratitle = "Purchases - Company";
                                Repname = "RP_PurchaseSumByDistMfgWithoutGroupby.repx";
                            }
                            Session["ParamTitle"] = paratitle;
                        }
                        if (Repname.Trim().ToUpper() == "RP_SALESSUMMARYBYDISTRIBUTORANDMFG.REPX")
                        {
                            paratitle = "Sales - Partners";

                            if (HstParam["ReportType"].ToString().ToUpper() == "NON-PARTNER")
                            {
                                paratitle = "Sales - Non-Partners";
                                Repname = "RP_SalesSummaryByDistributorandMfgNonPartner.repx";
                            }
                            if (HstParam["ReportType"].ToString().ToUpper() == "COMPANY")
                            {
                                paratitle = "Sales - Company";
                                Repname = "RP_SalesSumByDistMfgWithoutGroupby.repx";
                            }
                            Session["ParamTitle"] = paratitle;
                        }
                        if (Repname.ToUpper() == "RP_PURCHASESUMMARYBYMFGPARTNER.REPX")
                        {
                            if (HstParam["ReportType"].ToString().ToUpper() == "PARTNER")
                                Session["ParamTitle"] = "Purchases - Partners";
                            else if (HstParam["ReportType"].ToString().ToUpper() == "NON-PARTNER")
                            {
                                Session["ParamTitle"] = "Purchases - Non-Partners";
                                Repname = "RP_PurchaseSummaryByMfgNonPartner.repx";
                            }
                            else
                            {
                                Session["ParamTitle"] = "Purchases - Company";
                                Repname = "RP_PurchaseSummaryByMfgCompany.repx";
                            }
                        }
                        if (Repname.ToUpper() == "RP_SALESSUMMARYBYMFG.REPX")
                        {
                            if (HstParam["ReportType"].ToString().ToUpper() == "PARTNER")
                                Session["ParamTitle"] = "Sales - Partners";
                            else if (HstParam["ReportType"].ToString().ToUpper() == "NON-PARTNER")
                            {
                                Session["ParamTitle"] = "Sales - Non-Partners";
                                Repname = "RP_SalesSummaryByMfgNonPartner.repx";
                            }
                            else
                            {
                                Session["ParamTitle"] = "Sales - Company";
                                Repname = "RP_SalesSummaryByMfgCompany.repx";
                            }
                        }

                        if (Repname.ToUpper() == "RP_PURCHASESUMMARYBYDISTRIBUTORBYMONTH.REPX" || Repname.ToUpper() == "RP_SALESSUMMARYBYDISTRIBUTORBYMONTH.REPX")
                            YearMonth = HstParam["end"].ToString();
                        else
                            YearMonth = HstParam["cym"].ToString();

                        Year = YearMonth.Substring(0, 4);
                        Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                        monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                        PYToDate = monthName + "-" + Year;
                        Session["PYToDate"] = PYToDate;

                        if (Convert.ToInt32(Month) < 8)
                            ytd = Convert.ToInt32(Year) - 1;
                        else
                            ytd = Convert.ToInt32(Year) + 0 * 100;

                        ytds = ytd.ToString() + "08";
                        ytde = Convert.ToInt32(ytds) + 99;
                        ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                        ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                        ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);

                        PYEnd = ytdemonthName + "-" + ytdeYear;
                        Session["PYEnd"] = PYEnd;
                        break;
                    case "RP_VPCBenefitSummary.repx":
                    case "RP_MerchProgsPBDetail.repx":
                    case "RP_MerchandisePrgPBMfgSummary.repx":
                    case "Rp_MfgGGDistributorSummary.repx":
                        Session["PERIOD1"] = null; Session["PERIOD2"] = null; Session["PERIOD3"] = null;
                        Session["PERIOD1"] = HstParam["period1"];
                        Session["PERIOD2"] = HstParam["period2"];
                        Session["PERIOD3"] = HstParam["period3"];
                        break;

                    case "RP_SalesUPCProductGroupByMfg.repx":
                    case "RP_ProductGroupAuditSales.repx":
                        if (HstParam.Contains("cym").ToString() != null && HstParam["cym"].ToString() != "")
                        {
                            YearMonth = HstParam["cym"].ToString();

                            Year = YearMonth.Substring(0, 4);
                            Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                            monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                            PYToDate = monthName + "-" + Year;
                            Session["PYToDate"] = PYToDate;
                            Session["PYEnd"] = PYEnd;

                            string CM = Year + "-" + Month;
                            Session["CurrentMonth"] = CM;

                            if (Convert.ToInt32(Month) < 8)
                                ytd = Convert.ToInt32(Year) - 1;
                            else
                                ytd = Convert.ToInt32(Year) + 0 * 100;

                            ytds = ytd.ToString() + "08";
                            ytde = Convert.ToInt32(ytds) + 99;
                            ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                            ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                            ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);
                            PYEnd = ytdemonthName + "-" + ytdeYear;
                            Session["PYEnd"] = PYEnd;
                        }
                        break;
                    case "RP_SalesUPCProductGroupByCat.repx":
                        if (HstParam.Contains("cym").ToString() != null && HstParam["cym"].ToString() != "")
                        {
                            YearMonth = HstParam["cym"].ToString();

                            Year = YearMonth.Substring(0, 4);
                            Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                            monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                            PYToDate = monthName + "-" + Year;
                            Session["PYToDate"] = PYToDate;
                            if (Convert.ToInt32(Month) < 8)
                                ytd = Convert.ToInt32(Year) - 1;
                            else
                                ytd = Convert.ToInt32(Year) + 0 * 100;

                            ytds = ytd.ToString() + "08";
                            ytde = Convert.ToInt32(ytds) + 99;
                            ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                            ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                            ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);
                            PYEnd = ytdemonthName + "-" + ytdeYear;
                            Session["PYEnd"] = PYEnd;
                        }
                        break;
                }
                if (Description != null && Description != "")
                {
                    Session["Description" + GuId] = Description;
                }
                dsReport = new DataSet();

                Ggrp.Getdataset("Select Description,Filename,UserModule,ReportOrderBy,GridViewColumns,MailMergeDocumentList,SQLSelect,SQLOrderBy,SQLWhere  from op_security_points where Description='" + Description + "' and UserModule='" + UserModule + "'", "GVColumns", dsReport, ref daReport, Sconn);
                SQLGroupby = ""; //dsReport.Tables["GVColumns"].Rows[0]["SQLGroupby"].ToString();
                SQLOrderby = ""; //dsReport.Tables["GVColumns"].Rows[0]["SQLOrderBy"].ToString(); 

                if (ReportviewCol == "")
                {
                    if (!Filter.ToUpper().Contains("GROUP BY") && SQLGroupby.Trim() != "") Filter += " group by " + SQLGroupby;
                    if (!Filter.ToUpper().Contains("ORDER BY") && SQLOrderby.Trim() != "") Filter += " order by " + SQLOrderby;

                    if (qry.ToUpper().Trim().StartsWith("SELECT") && Filter != null && Filter != "")
                    {
                        if (qry.ToUpper().Contains("WHERE") && !Filter.Trim().StartsWith("order by") && !Filter.Trim().StartsWith("group by"))
                            qry = qry + " and " + Filter;
                        else if (Filter.Trim().StartsWith("order by") || Filter.Trim().StartsWith("group by"))
                            qry = qry + " " + Filter;
                        else
                        {
                            if (Filter.TrimStart().ToUpper().StartsWith("AND"))
                                Filter = " where 1=1 " + Filter.Trim();
                            else
                                Filter = " where " + Filter.Trim();
                            qry = qry + Filter;
                        }
                        Ggrp.Getdataset(qry, "Res", dsReport, ref daReport, Sconn);
                        DataTable newdatatable = new DataTable();
                        newdatatable = dsReport.Tables["Res"].Copy();
                        newdatatable.TableName = "GridRes";
                        dsReport.Tables.Add(newdatatable);
                        Session["MainDataTableModel" + GuId] = dsReport.Tables["Res"];
                        Session["dsSubReport" + GuId] = dsReport;
                    }
                    else if ((qry.ToUpper().Trim().StartsWith("SELECT")) && (Filter == null || Filter == ""))
                    {
                        Ggrp.Getdataset(qry, "Res", dsReport, ref daReport, Sconn);
                        DataTable newdatatable = new DataTable();
                        newdatatable = dsReport.Tables["Res"].Copy();
                        newdatatable.TableName = "GridRes";
                        dsReport.Tables.Add(newdatatable);
                    }
                    else if (qry.ToUpper().StartsWith("STP_") && !string.IsNullOrEmpty(SqlWhere) && SqlWhere == "1" && HstParam.Count > 0)
                    {
                        SqlCommand cmd = new SqlCommand(qry, Sconn);
                        daReport.SelectCommand = cmd;

                        if (Repname.Trim().ToUpper() == "RP_MFRDATASHEET.REPX" || Repname.Trim().ToUpper() == "RP_MFRDATASHEETWITHSIGN.REPX")
                        {
                            string MfgNo = "", classCd = "", prim = "", Sec = "";
                            if (HstParam.Contains("m.mfgno"))
                                MfgNo = HstParam["m.mfgno"].ToString();
                            if (HstParam.Contains("classCd"))
                                classCd = HstParam["classCd"].ToString();
                            if (HstParam.Contains("[primary]"))
                                prim = HstParam["[primary]"].ToString();
                            if (HstParam.Contains("secondary"))
                                Sec = HstParam["secondary"].ToString();

                            Ggrp.Getdataset(qry + "'" + prim + "','" + Sec + "','" + classCd + "','" + MfgNo + "'", "Res", dsReport, ref daReport, Sconn);
                        }
                        else if (Repname.Trim().ToUpper() == "RP_MDSBPOPRG.REPX")
                        {
                            // Manufacrer Data Sheet
                            string MfgNo = "", classCd = "", prim = "", Sec = "";
                            if (HstParam.Contains("m.mfgno"))
                                MfgNo = HstParam["m.mfgno"].ToString();
                            if (HstParam.Contains("classcd"))
                                classCd = HstParam["classcd"].ToString();
                            if (HstParam.Contains("[primary]"))
                                prim = HstParam["[primary]"].ToString();
                            if (HstParam.Contains("secondary"))
                                Sec = HstParam["secondary"].ToString();

                            Ggrp.Getdataset(qry + "'" + prim + "','" + Sec + "','" + classCd + "','" + MfgNo + "'", "Res", dsReport, ref daReport, Sconn);


                            string sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,OP.Phone as GGphone, p.*,m.french,m.spanish,m.cnreg,m.cnna,m.bilinpkg, m.youtube,m.facebook,m.twitter,m.eprodinfo,m.elineart,m.eadcopy,m.wprodinfo,
                m.wlineart,m.wadcopy,m.factreps,m.mfrreps,m.company,'PY'+REPLACE(BP.period, '-', '/') as PrgPeriod,
                RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progfrom)), 2)+'/'+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(DAY, BP.progfrom)), 2)+' - '+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progto)), 2)+'/'+ RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(DAY, BP.progto)), 2) as Term from mfgprginfo p 
                left join mfg m on p.mfgno=m.mfgno left join  op_system OP on 1=1
                left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system) where m.mfgno='" + MfgNo + "'";
                            Ggrp.Getdataset(sql, "Res2", dsReport, ref daReport, Sconn);

                            string sQl = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,OP.Phone as GGphone,'PY'+REPLACE(BP.period, '-', '/') as period,  
                            p.*,CASE M.french WHEN 1 THEN 'Y' ELSE 'N' END AS french,
                            CASE M.spanish WHEN 1 THEN 'Y' ELSE 'N' END AS spanish,
                            CASE M.cnreg WHEN 1 THEN 'Y' ELSE 'N' END AS cnreg,
                            CASE M.cnna WHEN 1 THEN 'Y' ELSE 'N' END AS cnna,
                            CASE M.bilinpkg WHEN 1 THEN 'Y' ELSE 'N' END AS bilinpkg,
                            CASE M.youtube WHEN 1 THEN 'Y' ELSE 'N' END AS youtube,
                            CASE M.facebook WHEN 1 THEN 'Y' ELSE 'N' END AS facebook,
                            CASE M.twitter WHEN 1 THEN 'Y' ELSE 'N' END AS twitter,
                            CASE M.eprodinfo WHEN 1 THEN 'Y' ELSE 'N' END AS eprodinfo,
                            CASE M.elineart WHEN 1 THEN 'Y' ELSE 'N' END AS elineart,
                            CASE M.eadcopy WHEN 1 THEN 'Y' ELSE 'N' END AS eadcopy,
                            CASE M.wprodinfo WHEN 1 THEN 'Y' ELSE 'N' END AS wprodinfo,
                            CASE M.wlineart WHEN 1 THEN 'Y' ELSE 'N' END AS wlineart,
                            CASE M.wadcopy WHEN 1 THEN 'Y' ELSE 'N' END AS wadcopy,
                            CASE M.factreps WHEN 1 THEN 'Y' ELSE 'N' END AS factreps,
                            CASE M.mfrreps WHEN 1 THEN 'Y' ELSE 'N' END AS mfrreps,
                            CASE M.prog_stat WHEN 1 THEN 'Y' ELSE 'N' END AS progstat,
                            m.company,RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progfrom)), 2)+'/'+RIGHT('00' + CONVERT(NVARCHAR(2), 
                        DATEPART(DAY, BP.progfrom)), 2)+' - '+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progto)), 2)+'/'+ RIGHT('00' + CONVERT(NVARCHAR(2), 
                        DATEPART(DAY, BP.progto)), 2) as Term from mfg m left join mfgprginfo p on p.mfgno=m.mfgno left join  op_system OP on 1=1
                            left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system) where m.mfgno='" + MfgNo + "'";
                            Ggrp.Getdataset(sQl, "Res3", dsReport, ref daReport, Sconn);

                        }
                        else if (Description.Trim().ToUpper() == "SEASONAL PROMOTIONAL PROGRAM")
                        {
                            if (HstParam["SortBy"].ToString().ToUpper().Trim() == "MANUFACUTURER")
                            {
                                Repname = "RP_SppByMfr.repx";
                            }
                            else if (HstParam["SortBy"].ToString().ToUpper().Trim() == "CATEGORY")
                            {
                                Repname = "RP_SppByCategory.repx";
                            }
                            else if (HstParam["SortBy"].ToString().ToUpper().Trim() == "DISTRIBUTOR")
                            {
                                Repname = "RP_SppByDistributor.repx";
                            }
                            else
                            {
                                Repname = "RP_SppByItems.repx";
                            }

                            if (HstParam["SortBy"].ToString().ToUpper().Trim() == "DISTRIBUTOR")
                            {
                                qry = "STP_RP_SPPListByDist";
                                Ggrp.Getdataset(qry + "'" + HstParam["cym"].ToString() + "','" + HstParam["type"].ToString() + "','" + HstParam["Desc"].ToString() + "'", "Res", dsReport, ref daReport, Sconn);
                            }
                            else
                            {
                                string DriveItem = "";
                                if (HstParam.ContainsKey("type"))
                                {
                                    string sql = "select descript from OP_lookup where lookupid='SPPTYPE' and entry='" + HstParam["type"] + "'";
                                    Sconn = new SqlConnection(sqlcon);
                                    Ggrp.Getdataset(sql, "oplookup", ds, ref da, Sconn);
                                    DriveItem = ds.Tables["oplookup"].Rows[0]["descript"].ToString();
                                    if (!string.IsNullOrEmpty(DriveItem))
                                    {
                                        Session["ParamTitle"] = DriveItem;
                                    }
                                    else
                                        Session["ParamTitle"] = "  ";

                                }
                                Ggrp.Getdataset(qry + "'" + HstParam["cym"].ToString() + "','" + HstParam["SortBy"].ToString() + "','" + HstParam["type"].ToString() + "','" + HstParam["Desc"].ToString() + "'", "Res", dsReport, ref daReport, Sconn);
                            }

                            Session["PERIOD1"] = null; Session["PYToDate"] = null; Session["PYEnd"] = null;


                            YearMonth = HstParam["cym"].ToString();
                            Year = YearMonth.Substring(0, 4);
                            Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                            monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                            PYToDate = monthName + "-" + Year;
                            Session["PYToDate"] = PYToDate;

                            if (Convert.ToInt32(Month) < 8)
                                ytd = Convert.ToInt32(Year) - 1;
                            else
                                ytd = Convert.ToInt32(Year) + 0 * 100;

                            ytds = ytd.ToString() + "08";
                            ytde = Convert.ToInt32(ytds) + 99;
                            ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                            ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                            ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);

                            PYEnd = ytdemonthName + "-" + ytdeYear;
                            Session["PYEnd"] = PYEnd;
                            Session["PERIOD1"] = HstParam["Desc"].ToString();
                        }
                        else if (Description.Trim().ToUpper() == "SALES BY UPC - DOLLARS/QTY")
                        {
                            string paramtype = HstParam["type"].ToString();
                            if (HstParam["type"].ToString() == "Dollar")
                                paramtype = "$$";
                            if (HstParam["UPCRank"].ToString().Trim().ToUpper() == "CATEGORY")
                            {
                                qry = "STP_RP_SalesTopUPCByCategory";
                                paratitle = "Sales UPC Ranked by Top " + HstParam["rows"] + " Items by " + paramtype + " within Category";
                            }
                            if (HstParam["UPCRank"].ToString().Trim().ToUpper() == "CATEGORYGROUP")
                            {
                                qry = "STP_RP_SalesTopUPCByCategoryGroup";
                                paratitle = "Sales UPC Ranked by Top " + HstParam["rows"] + " Items by " + paramtype + " within Product Group";
                            }
                            if (HstParam["UPCRank"].ToString().Trim().ToUpper() == "CATEGORYSUBGROUP")
                            {
                                qry = "STP_RP_SalesTopUPCByCategorySubGroup";
                                paratitle = "Sales UPC Ranked by Top " + HstParam["rows"] + " Items by " + paramtype + " within Category Subgroup";
                            }

                            Ggrp.Getdataset(qry + " '" + HstParam["cym"].ToString() + "','" + HstParam["rows"].ToString() + "','" + HstParam["type"].ToString() + "'", "Res", dsReport, ref daReport, Sconn);
                            Session["ParamTitle"] = paratitle;

                            YearMonth = HstParam["cym"].ToString();
                            Year = YearMonth.Substring(0, 4);
                            Month = YearMonth.Substring(YearMonth.Length - 2, 2);
                            monthName = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(Month), 1).ToString("MMM", CultureInfo.InvariantCulture);
                            PYToDate = monthName + "-" + Year;
                            Session["PYToDate"] = PYToDate;

                            if (Convert.ToInt32(Month) < 8)
                                ytd = Convert.ToInt32(Year) - 1;
                            else
                                ytd = Convert.ToInt32(Year) + 0 * 100;

                            ytds = ytd.ToString() + "08";
                            ytde = Convert.ToInt32(ytds) + 99;
                            ytdeYear = Convert.ToString(ytde).Substring(0, 4);
                            ytdeMonth = Convert.ToString(ytde).Substring(Convert.ToString(ytde).Length - 2, 2);
                            ytdemonthName = new DateTime(Convert.ToInt32(ytdeYear), Convert.ToInt32(ytdeMonth), 1).ToString("MMM", CultureInfo.InvariantCulture);

                            PYEnd = ytdemonthName + "-" + ytdeYear;
                            Session["PYEnd"] = PYEnd;
                        }
                        else
                        {
                            SqlParameter[] Sqlparam = BuildParamList(UserModule, Description, HstParam);
                            Ggrp.GetDataSetFromSP(qry, "Res", ref dsReport, ref daReport, Sqlparam, Sconn);
                        }

                        DataTable newdatatable = new DataTable();
                        newdatatable = dsReport.Tables["Res"].Copy();
                        newdatatable.TableName = "GridRes";
                        dsReport.Tables.Add(newdatatable);
                    }
                    else if (qry.ToUpper().StartsWith("STP_") && Filter != null && Filter != "")
                    {
                        SqlCommand cmd = new SqlCommand(qry, Sconn);
                        daReport.SelectCommand = cmd;
                        SqlParameter[] Sqlparam;
                        SqlParameter lparam = new SqlParameter("@lwhere", DbType.String);
                        lparam.Value = " " + Filter;
                        Sqlparam = new SqlParameter[] { lparam };
                        Ggrp.GetDataSetFromSP(qry, "Res", ref dsReport, ref daReport, Sqlparam, Sconn);
                        DataTable newdatatable = new DataTable();
                        newdatatable = dsReport.Tables["Res"].Copy();
                        newdatatable.TableName = "GridRes";
                        dsReport.Tables.Add(newdatatable);
                    }
                    else if (qry.ToUpper().StartsWith("STP_"))
                    {
                        Ggrp.Getdataset(qry, "Res", dsReport, ref daReport, Sconn);
                        DataTable newdatatable = new DataTable();
                        newdatatable = dsReport.Tables["Res"].Copy();
                        newdatatable.TableName = "GridRes";
                        dsReport.Tables.Add(newdatatable);
                    }
                    else
                    {
                        dsReport = (DataSet)CallMethod(HstParam, qry, Repname);
                        if (dsReport == null || dsReport.Tables.Count == 0)
                        {
                            return Json(new { Items = Ggrp.SendErrMessage });
                        }
                        DataTable newdatatable = new DataTable();
                        newdatatable = dsReport.Tables["Results"].Copy();
                        newdatatable.TableName = "Res";
                        dsReport.Tables.Add(newdatatable);
                        newdatatable = new DataTable();
                        newdatatable = dsReport.Tables[0].Copy();
                        newdatatable.TableName = "GridRes";
                        dsReport.Tables.Add(newdatatable);
                    }

                    switch (Repname.ToUpper())
                    {
                        case "RP_MFGCONTACTS.REPX":
                        case "RP_DISTCONTACTS.REPX":
                        case "RP_PSMFGSUMMARY.REPX":
                        case "RP_PSDISTRIBUTORSUMMARYBYMFG.REPX":
                        case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFG.REPX":
                        case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                        case "RP_PURCHASESUMBYDISTMFGWITHOUTGROUPBY.REPX":
                        case "RP_SALESSUMMARYBYDISTRIBUTORANDMFG.REPX":
                        case "RP_SALESSUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                        case "RP_SALESSUMBYDISTMFGWITHOUTGROUPBY.REPX":
                        case "RP_PURCHASESUMMARYBYMFGPARTNER.REPX":
                        case "RP_PURCHASESUMMARYBYMFGNONPARTNER.REPX":
                        case "RP_PURCHASESUMMARYBYMFGCOMPANY.REPX":
                        case "RP_SALESSUMMARYBYMFG.REPX":
                        case "RP_SALESSUMMARYBYMFGNONPARTNER.REPX":
                        case "RP_SALESSUMMARYBYMFGCOMPANY.REPX":

                            DataColumn columnSel;
                            columnSel = new DataColumn();
                            columnSel.DataType = System.Type.GetType("System.Boolean");
                            columnSel.ColumnName = "SELECT";
                            columnSel.DefaultValue = 0;
                            dsReport.Tables["GridRes"].Columns.Add(columnSel);
                            dsReport.Tables["GridRes"].Columns["Select"].SetOrdinal(0);
                            DataColumn columnSel1;
                            columnSel1 = new DataColumn();
                            columnSel1.DataType = System.Type.GetType("System.Boolean");
                            columnSel1.ColumnName = "SELECT";
                            columnSel1.DefaultValue = 0;
                            dsReport.Tables["Res"].Columns.Add(columnSel1);
                            dsReport.Tables["Res"].Columns["Select"].SetOrdinal(0);
                            break;
                    }

                    Session["ReportEmailDS" + GuId] = dsReport.Tables["Res"];
                    Session["DataTableModel" + GuId] = dsReport.Tables["Res"];
                    Session["GridViewDS" + GuId] = dsReport.Tables["GridRes"];
                    Session["MainGridViewDS" + GuId] = dsReport.Tables["GridRes"];
                    Session["MainDataTableModel" + GuId] = dsReport.Tables["Res"];
                    Session["dsSubReport" + GuId] = dsReport;
                    Reportviewcol = "check";
                }
                if (Reportviewcol != "check")
                {
                    DataTable GridList = (DataTable)Session["MainGridViewDS" + GuId];
                    DataTable dtCopy = GridList.Copy();
                    dsReport.Tables.Add(dtCopy);
                    dtCopy.TableName = "GridRes";

                    DataTable GridList1 = (DataTable)Session["MainDataTableModel" + GuId];
                    DataTable dtCopy1 = GridList1.Copy();
                    dsReport.Tables.Add(dtCopy1);
                }

                Repname = GetValidReportName(Repname);
                if (Repname != null && Repname != "")
                {
                    Session["ReportName" + GuId] = Server.MapPath("~/DevExpReports/") + Repname;

                    Ggrp.Getdataset("Select GridViewColumns from op_security_points where Description='" + Description + "' and UserModule='" + UserModule + "'", "GVColumns", dsReport, ref daReport, Sconn);
                    if (this.dsReport.Tables["GVColumns"].Rows[0]["GridViewColumns"].ToString() != "" && viewcolumns == false)
                    {
                        char sep = '~';
                        int leth = 0;
                        string[] splitMain = this.dsReport.Tables["GVColumns"].Rows[0]["GridViewColumns"].ToString().ToUpper().Split(sep).Where(s => !String.IsNullOrEmpty(s)).ToArray();
                        leth = splitMain.Length;
                        sep = '^';
                        string[] CLName = new string[leth];
                        string[] FLName = new string[leth];
                        for (int i = 0; i < splitMain.Length; i++)
                        {
                            splitMain[i] = Regex.Replace(splitMain[i], @"\s+$", "");
                            string[] split = splitMain[i].ToString().ToUpper().Split(sep).Where(s => !String.IsNullOrEmpty(s)).ToArray();
                            if (split.Length > 0)
                            {
                                if (!FLName.Contains(split[1]) && !CLName.Contains(split[0]) && dsReport.Tables["GridRes"].Columns.Contains(split[1]))
                                {
                                    FLName[i] = split[1].ToString();
                                    CLName[i] = split[0].ToString();
                                }
                            }
                        }

                        switch (Repname.ToUpper())
                        {
                            case "RP_MFGCONTACTS.REPX":
                            case "RP_DISTCONTACTS.REPX":
                            case "RP_PSMFGSUMMARY.REPX":
                            case "RP_PSDISTRIBUTORSUMMARYBYMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMBYDISTMFGWITHOUTGROUPBY.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_SALESSUMBYDISTMFGWITHOUTGROUPBY.REPX":
                            case "RP_PURCHASESUMMARYBYMFGPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGCOMPANY.REPX":
                            case "RP_SALESSUMMARYBYMFG.REPX":
                            case "RP_SALESSUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_SALESSUMMARYBYMFGCOMPANY.REPX":
                                if ((CLName.ToString().ToUpper().Contains("MFGNO") || CLName.Contains("DistNo") || CLName.Contains("EMAIL")))
                                {
                                    foreach (string s in CLName)
                                    {
                                        if (s == "MfgNo" || s == "DistNo" || s == "EMAIL")
                                        {
                                            FLName[FLName.Length - 1] = "SELECT";
                                            CLName[CLName.Length - 1] = "SELECT";
                                            break;
                                        }
                                    }
                                }
                                break;
                        }


                        CLName = CLName.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        Session["CLNAME" + GuId] = CLName;
                        FLName = FLName.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        DataTable DT = dsReport.Tables["GridRes"];

                        switch (Repname.ToUpper())
                        {
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMBYDISTMFGWITHOUTGROUPBY.REPX":

                            case "RP_PURCHASESUMMARYBYMFGPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGCOMPANY.REPX":

                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_SALESSUMBYDISTMFGWITHOUTGROUPBY.REPX":
                                /* for Last Year */
                                if (DT.Columns.Contains("LastYear"))
                                    DT.Columns.Add(new DataColumn("LastYears", typeof(decimal), "LastYear/1000"));

                                foreach (DataRow dr in DT.Rows)
                                {
                                    double Cymonth, Lymonth;

                                    double.TryParse(dr["CYMONTH"].ToString(), out Cymonth);
                                    double.TryParse(dr["LYMONTH"].ToString(), out Lymonth);

                                    double cmchng = Cymonth - Lymonth;
                                    double cmdiff = 0.0;

                                    if (Lymonth != null && Lymonth.ToString() != "")
                                        cmdiff = cmchng * 100 / Lymonth;
                                    else
                                        cmdiff = 0.0;

                                    if (DT.Columns.Contains("GVPMchg"))
                                    {

                                        if (Double.IsInfinity(cmdiff))
                                        {
                                            dr["GVPMchg"] = 0.0;
                                        }
                                        else if (Double.IsNaN(cmdiff))
                                        {
                                            dr["GVPMchg"] = 0.0;

                                        }
                                        else
                                        {
                                            dr["GVPMchg"] = Math.Round(cmdiff, 1);
                                        }

                                        dr["GVMchg"] = cmchng;
                                    }
                                    if (DT.Columns.Contains("GVMchg"))
                                        dr["GVMchg"] = cmchng;

                                    double YTD, LYTD;

                                    double.TryParse(dr["Ytd"].ToString(), out YTD);
                                    double.TryParse(dr["LYTD"].ToString(), out LYTD);
                                    double YTDChng = YTD - LYTD;
                                    double YTDDiff = 0.0;

                                    if (LYTD != null && LYTD.ToString() != "")
                                        YTDDiff = YTDChng * 100 / LYTD;
                                    else
                                        YTDDiff = 0.0;
                                    if (DT.Columns.Contains("Ychg"))
                                        dr["Ychg"] = YTDChng;

                                    if (DT.Columns.Contains("Pychg"))
                                    {
                                        if (Double.IsInfinity(YTDDiff))
                                        {
                                            dr["Pychg"] = 0.0;
                                        }
                                        else if (Double.IsNaN(YTDDiff))
                                        {
                                            dr["Pychg"] = 0.0;
                                        }
                                        else
                                        {
                                            dr["Pychg"] = Math.Round(YTDDiff, 1);
                                        }
                                    }
                                    if (DT.Columns.Contains("LastYear"))
                                        dr["LastYear"] = dr["LastYears"];

                                }
                                break;

                            case "RP_SALESSUMMARYBYMFG.REPX":
                            case "RP_SALESSUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_SALESSUMMARYBYMFGCOMPANY.REPX":
                                if (DT.Columns.Contains("LastYear"))
                                    DT.Columns.Add(new DataColumn("LastYears", typeof(decimal), "LastYear/1000"));
                                if (DT.Columns.Contains("LYTotal"))
                                    DT.Columns.Add(new DataColumn("LYTotals", typeof(decimal), "LYTotal/1000"));
                                if (DT.Columns.Contains("PYTD"))
                                    DT.Columns.Add(new DataColumn("PYTDs", typeof(decimal), "PYTD/1000"));
                                if (DT.Columns.Contains("PYTotal"))
                                    DT.Columns.Add(new DataColumn("PYTotals", typeof(decimal), "PYTotal/1000"));

                                foreach (DataRow dr in DT.Rows)
                                {
                                    double Cymonth, Lymonth;

                                    double.TryParse(dr["CYMONTH"].ToString(), out Cymonth);
                                    double.TryParse(dr["LYMONTH"].ToString(), out Lymonth);

                                    double cmchng = Cymonth - Lymonth;
                                    double cmdiff = 0.0;

                                    if (Lymonth != null && Lymonth.ToString() != "")
                                        cmdiff = cmchng * 100 / Lymonth;
                                    else
                                        cmdiff = 0.0;

                                    if (DT.Columns.Contains("GVPMchg"))
                                    {

                                        if (Double.IsInfinity(cmdiff))
                                        {
                                            dr["GVPMchg"] = 0.0;
                                        }
                                        else if (Double.IsNaN(cmdiff))
                                        {
                                            dr["GVPMchg"] = 0.0;

                                        }
                                        else
                                        {
                                            dr["GVPMchg"] = Math.Round(cmdiff, 1);
                                        }

                                        dr["GVMchg"] = cmchng;
                                    }
                                    if (DT.Columns.Contains("GVMchg"))
                                        dr["GVMchg"] = cmchng;


                                    double YTD, LYTD;
                                    double.TryParse(dr["Ytd"].ToString(), out YTD);
                                    double.TryParse(dr["LYTD"].ToString(), out LYTD);
                                    double YTDChng = YTD - LYTD;
                                    double YTDDiff = 0.0;

                                    if (LYTD != null && LYTD.ToString() != "")
                                        YTDDiff = YTDChng * 100 / LYTD;
                                    else
                                        YTDDiff = 0.0;
                                    if (DT.Columns.Contains("Ychg"))
                                        dr["Ychg"] = YTDChng;

                                    if (DT.Columns.Contains("Pychg"))
                                    {
                                        if (Double.IsInfinity(YTDDiff))
                                        {
                                            dr["Pychg"] = 0.0;
                                        }
                                        else if (Double.IsNaN(YTDDiff))
                                        {
                                            dr["Pychg"] = 0.0;
                                        }
                                        else
                                        {
                                            dr["Pychg"] = Math.Round(YTDDiff, 1);
                                        }
                                    }
                                    if (DT.Columns.Contains("LastYear"))
                                        dr["LastYear"] = dr["LastYears"];
                                    if (DT.Columns.Contains("LYTotal"))
                                        dr["LYTotal"] = dr["LYTotals"];

                                    if (DT.Columns.Contains("PYTD"))
                                        dr["PYTD"] = dr["PYTDs"];

                                    if (DT.Columns.Contains("PYTotal"))
                                        dr["PYTotal"] = dr["PYTotals"];

                                }

                                break;

                            case "RP_PURCHASESUMMARYBYMFGTYPE.REPX":
                            case "RP_SALESSUMMARYBYMFGTYPE.REPX":
                                /* for Last Year */
                                if (DT.Columns.Contains("LastYear"))
                                    DT.Columns.Add(new DataColumn("LastYears", typeof(decimal), "LastYear"));

                                foreach (DataRow dr in DT.Rows)
                                {
                                    double Cymonth, Lymonth;

                                    double.TryParse(dr["CYMONTH"].ToString(), out Cymonth);
                                    double.TryParse(dr["LYMONTH"].ToString(), out Lymonth);

                                    double cmchng = Cymonth - Lymonth;
                                    double cmdiff = 0.0;

                                    if (Lymonth != null && Lymonth.ToString() != "")
                                        cmdiff = cmchng * 100 / Lymonth;
                                    else
                                        cmdiff = 0.0;

                                    if (DT.Columns.Contains("GVPMchg"))
                                    {

                                        if (Double.IsInfinity(cmdiff))
                                        {
                                            dr["GVPMchg"] = 0.0;
                                        }
                                        else if (Double.IsNaN(cmdiff))
                                        {
                                            dr["GVPMchg"] = 0.0;

                                        }
                                        else
                                        {
                                            dr["GVPMchg"] = Math.Round(cmdiff, 1);
                                        }

                                        dr["GVMchg"] = cmchng;
                                    }
                                    if (DT.Columns.Contains("GVMchg"))
                                        dr["GVMchg"] = cmchng;

                                    double YTD, LYTD;
                                    double.TryParse(dr["Ytd"].ToString(), out YTD);
                                    double.TryParse(dr["LYTD"].ToString(), out LYTD);
                                    double YTDChng = YTD - LYTD;
                                    double YTDDiff = 0.0;

                                    if (LYTD != null && LYTD.ToString() != "")
                                        YTDDiff = YTDChng * 100 / LYTD;
                                    else
                                        YTDDiff = 0.0;
                                    if (DT.Columns.Contains("Ychg"))
                                        dr["Ychg"] = YTDChng;

                                    if (DT.Columns.Contains("Pychg"))
                                    {
                                        if (Double.IsInfinity(YTDDiff))
                                        {
                                            dr["Pychg"] = 0.0;
                                        }
                                        else if (Double.IsNaN(YTDDiff))
                                        {
                                            dr["Pychg"] = 0.0;
                                        }
                                        else
                                        {
                                            dr["Pychg"] = Math.Round(YTDDiff, 1);
                                        }
                                    }
                                    if (DT.Columns.Contains("LastYear"))
                                        dr["LastYear"] = dr["LastYears"];

                                }
                                break;
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORBYMONTH.REPX":
                                if (DT.Columns.Contains("Total"))
                                    DT.Columns.Add(new DataColumn("Totalval", typeof(decimal), "Total/1000"));
                                if (DT.Columns.Contains("Partners"))
                                    DT.Columns.Add(new DataColumn("Partnersval", typeof(decimal), "Partners/1000"));

                                foreach (DataRow dr in DT.Rows)
                                {
                                    if (DT.Columns.Contains("Total"))
                                        dr["Total"] = dr["Totalval"];
                                    if (DT.Columns.Contains("Partners"))
                                        dr["Partners"] = dr["Partnersval"];
                                }

                                break;

                        }

                        DataTable NewTable = DT.DefaultView.ToTable(false, FLName);
                        for (int i = 0; i < NewTable.Columns.Count; i++)
                        {
                            if (CLName[i] != null)
                                NewTable.Columns[i].ColumnName = CLName[i].ToUpper().ToString();
                        }
                        Session["ReportEmailDS" + GuId] = dsReport.Tables["Res"];
                        Session["DataTableModel" + GuId] = dsReport.Tables["Res"];
                        Session["SALESDETAILSRECONDS" + GuId] = dsReport;
                        Session["GridViewDS" + GuId] = NewTable;// dsReport.Tables["GridRes"];
                        Filledgrid = Ggrp.CovertDatasetToHashTbl(NewTable);

                        switch (Repname.ToUpper())
                        {
                            case "RP_MFGCONTACTS.REPX":
                            case "RP_DISTCONTACTS.REPX":
                            case "RP_PSMFGSUMMARY.REPX":
                            case "RP_PSDISTRIBUTORSUMMARYBYMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMBYDISTMFGWITHOUTGROUPBY.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_SALESSUMBYDISTMFGWITHOUTGROUPBY.REPX":
                            case "RP_PURCHASESUMMARYBYMFGPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGCOMPANY.REPX":
                            case "RP_SALESSUMMARYBYMFG.REPX":
                            case "RP_SALESSUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_SALESSUMMARYBYMFGCOMPANY.REPX":

                                if (NewTable.Columns.Contains("MFGNO"))
                                    Session["KeyNameMfgNo" + GuId] = "YES";
                                if (NewTable.Columns.Contains("DistNo"))
                                    Session["KeyNameDistNo" + GuId] = "YES";
                                break;
                        }

                    }
                    else
                    {
                        Session["ReportEmailDS" + GuId] = dsReport.Tables["Res"];
                        Session["DataTableModel" + GuId] = dsReport.Tables["Res"];
                        Session["GridViewDS" + GuId] = dsReport.Tables["GridRes"];
                        for (int i = 0; i < dsReport.Tables["GridRes"].Columns.Count; i++)
                        {
                            dsReport.Tables["GridRes"].Columns[i].ColumnName = dsReport.Tables["GridRes"].Columns[i].ColumnName.ToUpper().ToString();
                        }
                        Filledgrid = Ggrp.CovertDatasetToHashTbl(dsReport.Tables["GridRes"]);
                        DataTable dt = dsReport.Tables["GridRes"];
                        string[] columnNames = (from dc in dt.Columns.Cast<DataColumn>()
                                                select dc.ColumnName).ToArray();
                        Session["CLNAME" + GuId] = columnNames;
                        switch (Repname.ToUpper())
                        {
                            case "RP_MFGCONTACTS.REPX":
                            case "RP_DISTCONTACTS.REPX":
                            case "RP_PSMFGSUMMARY.REPX":
                            case "RP_PSDISTRIBUTORSUMMARYBYMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_PURCHASESUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMBYDISTMFGWITHOUTGROUPBY.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFG.REPX":
                            case "RP_SALESSUMMARYBYDISTRIBUTORANDMFGNONPARTNER.REPX":
                            case "RP_SALESSUMBYDISTMFGWITHOUTGROUPBY.REPX":
                            case "RP_PURCHASESUMMARYBYMFGPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_PURCHASESUMMARYBYMFGCOMPANY.REPX":
                            case "RP_SALESSUMMARYBYMFG.REPX":
                            case "RP_SALESSUMMARYBYMFGNONPARTNER.REPX":
                            case "RP_SALESSUMMARYBYMFGCOMPANY.REPX":


                                if (dsReport.Tables["GridRes"].Columns.Contains("MFGNO"))
                                {
                                    Session["KeyNameMfgNo" + GuId] = "YES";
                                    dsReport.Tables["GridRes"].Columns["MFGNO"].ColumnName = "MFGNO";
                                }
                                if (dsReport.Tables["GridRes"].Columns.Contains("DistNo"))
                                {
                                    Session["KeyNameDistNo" + GuId] = "YES";
                                    dsReport.Tables["GridRes"].Columns["DISTNO"].ColumnName = "DISTNO";
                                }
                                break;
                        }
                    }
                }
                Sqlqry = qry;
                qry = "";
                Session["ReportCount" + GuId] = Filledgrid.Count;
                Session["ActivityType"] = "Report";
            }
            catch (Exception Ex)
            {
                Session["DataTableModel" + GuId] = null;
                return Json(new { Items = Ex.Message });
            }
            string filterActivity = "";
            filterActivity = Session["lwhereParameter" + GuId].ToString();
            Session["ReplaceFilter" + GuId] = filterActivity.Replace("'", "''");
            var jsonResult = Json(new { Items = "", Item = Filledgrid, IsAcctnoColumn = Session["KeyNameMfgNo" + GuId].ToString() }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult GetDateCalcDatesById(string dateCalc)
        {
            string StartDate = "", EndDate = "";
            Sconn = new SqlConnection(sqlcon);
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            string lsql = "STP_DateCalc '','" + dateCalc + "'";
            Ggrp.Getdataset(lsql, "tblDataCal", ds, ref da, Sconn);
            if (ds.Tables["tblDataCal"].Rows.Count > 0)
            {
                StartDate = (ds.Tables["tblDataCal"].Rows[0]["StartDate"] != DBNull.Value && ds.Tables["tblDataCal"].Rows[0]["StartDate"].ToString() != "") ? Convert.ToDateTime(ds.Tables["tblDataCal"].Rows[0]["StartDate"].ToString()).ToString("MM/dd/yyyy") : "";
                EndDate = (ds.Tables["tblDataCal"].Rows[0]["EndDate"] != DBNull.Value && ds.Tables["tblDataCal"].Rows[0]["EndDate"].ToString() != "") ? Convert.ToDateTime(ds.Tables["tblDataCal"].Rows[0]["EndDate"].ToString()).ToString("MM/dd/yyyy") : "";
            }
            return Json(new { StartDate = StartDate, EndDate = EndDate });
        }

        private SqlParameter[] BuildParamList(string userModule, string description, Hashtable HstParam)
        {
            Sconn = new SqlConnection(sqlcon);
            string sql = "select RS.Field_Name,RS.Usermodule,RS.Description,RS.Type,SP.SQLWhere  from OP_Security_Points SP left join op_reportselect RS on SP.Usermodule=RS.Usermodule and SP.Description=RS.Description where SP.Usermodule='" + userModule + "' and " + "SP.Description='" + description + "'"
                + "Union select 'E'+RS.Field_Name,RS.Usermodule,RS.Description,RS.Type,SP.SQLWhere  from OP_Security_Points SP left join op_reportselect RS on SP.Usermodule=RS.Usermodule and SP.Description=RS.Description where SP.Usermodule='" + userModule + "' and " + "SP.Description='" + description + "' and Type='D2'";
            Ggrp.Getdataset(sql, "GetParamList", ds, ref da, Sconn);
            SqlParameter[] Sqlparam = new SqlParameter[ds.Tables["GetParamList"].Rows.Count];
            if (ds.Tables["GetParamList"] != null && ds.Tables["GetParamList"].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables["GetParamList"].Rows.Count; i++)
                {
                    string fieldname = ds.Tables["GetParamList"].Rows[i]["Field_Name"].ToString();
                    string fieldnamevalues = ds.Tables["GetParamList"].Rows[i]["Field_Name"].ToString();
                    string fieldtype = ds.Tables["GetParamList"].Rows[i]["Type"].ToString();
                    if (fieldname.Contains("."))
                    {
                        string[] splitfname = fieldname.Split('.');
                        fieldname = splitfname[1];
                    }
                    if (HstParam[fieldnamevalues] != null && HstParam[fieldnamevalues].ToString() != "")
                        Sqlparam[i] = new SqlParameter("@" + fieldname, HstParam[fieldnamevalues]);
                    else
                    {
                        if (ds.Tables["GetParamList"].Rows[i]["Description"].ToString().ToUpper() == "PROGRESS REPORT")
                            Sqlparam[i] = new SqlParameter("@" + fieldname, DBNull.Value);
                        else
                            Sqlparam[i] = new SqlParameter("@" + fieldname, "");
                    }
                }
            }
            return Sqlparam;
        }

        public ActionResult SelectCheckboxEvent(bool IsStatus, string GuId)
        {
            if (Session["GridViewDS" + GuId] == null)
                return Json(new { Items = "" });
            DataSet ds = new DataSet();
            DataTable dda = (DataTable)Session["GridViewDS" + GuId];
            DataTable dtCopy = dda.Copy();
            ds.Tables.Add(dtCopy);
            for (var i = 0; i < ds.Tables["GridRes"].Rows.Count; i++)
            {
                ds.Tables["GridRes"].Rows[i]["SELECT"] = IsStatus;
            }
            Session["GridViewDS" + GuId] = null;
            Session["GridViewDS" + GuId] = ds.Tables["GridRes"];
            return Json(new { Items = "" });
        }

        public ActionResult GridListRowindex(string GenMembers, int RowIndex = 0, bool IsSelected = false, bool Selectall = false, string GuId = "")
        {
            if (Selectall.ToString() == "False")
            {
                string p = @"[\[\]']+";
                var replaced = Regex.Replace(GenMembers, p, "");
                replaced = replaced.Replace('"', ' ');
                string[] StrArray = replaced.ToString().Split(',');
                DataSet ds = new DataSet();
                DataTable DT = new DataTable();
                DT.TableName = "GenMailing";

                if (Session["Description" + GuId].ToString().ToUpper() == "DISTRIBUTOR CONTACTS")
                    DT.Columns.Add("DistNo");
                if (Session["Description" + GuId].ToString().Trim().ToUpper() == "MANUFACTURER CONTACTS" || Session["Description" + GuId].ToString().Trim().ToUpper() == "PURCHASE SUMMARY - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "PURCHASES BY DISTRIBUTOR - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "P&S PARTNER DETAIL BY DISTRIBUTOR - 3YEARS" || Session["Description" + GuId].ToString().Trim().ToUpper() == "P&S PARTNER SUMMARY - 3YEARS" || Session["Description" + GuId].ToString().Trim().ToUpper() == "SALES BY DISTRIBUTOR - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "SALES SUMMARY - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "P&S PARTNER DETAIL BY DISTRIBUTOR - 3YEARS")
                    DT.Columns.Add("MfgNo");

                if (ds.Tables.Contains("GenMailing")) ds.Tables.Remove("GenMailing");
                ds.Tables.Add(DT);
                DataRow dr = ds.Tables["GenMailing"].NewRow();
                for (int i = 0; i < StrArray.Length; i++)
                {
                    if (StrArray[i] != "")
                    {
                        dr = ds.Tables["GenMailing"].NewRow();
                        if (Session["Description" + GuId].ToString().Trim().ToUpper() == "DISTRIBUTOR CONTACTS")
                            dr["DistNo"] = StrArray[i].ToString().Trim();
                        if (Session["Description" + GuId].ToString().Trim().ToUpper() == "MANUFACTURER CONTACTS" || Session["Description" + GuId].ToString().Trim().ToUpper() == "PURCHASE SUMMARY - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "PURCHASES BY DISTRIBUTOR - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "P&S PARTNER DETAIL BY DISTRIBUTOR - 3YEARS" || Session["Description" + GuId].ToString().Trim().ToUpper() == "P&S PARTNER SUMMARY - 3YEARS" || Session["Description" + GuId].ToString().Trim().ToUpper() == "SALES BY DISTRIBUTOR - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "SALES SUMMARY - PARTNER/NON-PARTNER" || Session["Description" + GuId].ToString().Trim().ToUpper() == "P&S PARTNER DETAIL BY DISTRIBUTOR - 3YEARS")
                            dr["MfgNo"] = StrArray[i].ToString().Trim();
                        DT.Rows.Add(dr);
                    }
                }
                DataColumn objDataColumn = new DataColumn();
                objDataColumn.ColumnName = "SELECT";
                objDataColumn.DefaultValue = "False";
                DataTable GridList = (DataTable)Session["GridViewDS" + GuId];
                if (GridList.Columns.Contains("SELECT"))
                    GridList.Columns.Remove("SELECT");
                GridList.Columns.Add(objDataColumn);
                objDataColumn.SetOrdinal(0);
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    if (GridList.Columns.Contains("MFGNO"))
                        GridList.Select("MFGNO=" + DT.Rows[i]["MfgNo"].ToString())[0]["SELECT"] = "True";
                    if (GridList.Columns.Contains("DISTNO"))
                        GridList.Select("DISTNO=" + DT.Rows[i]["DistNo"].ToString())[0]["SELECT"] = "True";
                }
                Session["GridViewDS" + GuId] = null;
                Session["GridViewDS" + GuId] = GridList;
            }
            else
            {
                DataColumn objDataColumn = new DataColumn();
                objDataColumn.ColumnName = "SELECT";
                objDataColumn.DefaultValue = Selectall;
                DataTable GridList = (DataTable)Session["GridViewDS" + GuId];
                if (GridList.Columns.Contains("SELECT"))
                    GridList.Columns.Remove("SELECT");
                GridList.Columns.Add(objDataColumn);
                objDataColumn.SetOrdinal(0);
                Session["GridViewDS" + GuId] = null;
                Session["GridViewDS" + GuId] = GridList;
            }
            return Json(new { Items = "" });
        }
        private string GetValidReportName(string ReportName)
        {
            if (ReportName.IndexOf(".") > -1)
            {
                string[] FileName = ReportName.Split('.');
                if (FileName[1].Trim().ToUpper() != "REPX")
                    return FileName[0].Trim() + ".Repx";
                else
                    return ReportName;
            }
            else
                return ReportName + ".Repx";
        }

        #endregion

        #region Custom reports

        private bool CheckMethod(MethodInfo method)
        {
            if (!method.IsPublic)
                return false;
            if (method.IsStatic)
                return false;
            return true;
        }

        #endregion


    }

    public class MergePDF
    {
        #region Fields
        private string destinationfile;
        private IList fileList = new ArrayList();
        #endregion
        //default constructor
        public MergePDF()
        {
        }
        public MergePDF(IList _fileList)
        {
            this.fileList = _fileList;
        }
        #region Public Methods
        ///
        /// Add a new file, together with a given  docname to the fileList and namelist collection
        ///
        public void AddFile(string pathnname)
        {
            fileList.Add(pathnname);
        }

        ///
        /// Generate the merged PDF
        ///
        public void Execute()
        {
            MergeDocs();
        }

        public void ExecuteWithData(bool delFile = false)
        {
            MergeDocsWithData(delFile);
        }

        #endregion

        #region Private Methods
        ///
        /// Merges the Docs and renders the destinationFile
        ///
        private void MergeDocs()
        {

            //Step 1: Create a Docuement-Object
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            try
            {
                //Step 2: we create a writer that listens to the document
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document,
               new FileStream(destinationfile, FileMode.Create));

                //Step 3: Open the document
                document.Open();

                iTextSharp.text.pdf.PdfContentByte cb = writer.DirectContent;
                iTextSharp.text.pdf.PdfImportedPage page;

                int n = 0;
                int rotation = 0;

                //Loops for each file that has been listed
                foreach (string filename in fileList)
                {
                    //The current file path
                    //  string filePath = sourcefolder + filename; 

                    // we create a reader for the document
                    //PdfReader reader = new PdfReader(filename);
                    byte[] pdfbytes = System.IO.File.ReadAllBytes(filename);
                    iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(pdfbytes);

                    //Gets the number of pages to process
                    n = reader.NumberOfPages;

                    int i = 0;
                    while (i < n)
                    {
                        i++;
                        document.SetPageSize(reader.GetPageSizeWithRotation(1));
                        document.NewPage();

                        //Insert to Destination on the first page
                        if (i == 1)
                        {
                            iTextSharp.text.Chunk fileRef = new iTextSharp.text.Chunk(" ");
                            fileRef.SetLocalDestination(filename);
                            document.Add(fileRef);
                        }

                        page = writer.GetImportedPage(reader, i);
                        rotation = reader.GetPageRotation(i);
                        if (rotation == 90 || rotation == 270)
                        {
                            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                        }
                        else
                        {
                            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                        }
                    }
                }

                foreach (string filename in fileList)
                {
                    try
                    {
                        if (System.IO.File.Exists(filename))
                        {
                            System.IO.File.Delete(filename);
                        }
                    }
                    catch (Exception ex) { string s = ex.Message; }
                }

            }
            catch (Exception e) { throw e; }
            finally { document.Close(); }


        }

        private void MergeDocsWithData(bool delFile = false)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            try
            {
                //string destinationfile = desktopPath.Replace(@"d:\outputfile.pdf");
                iTextSharp.text.pdf.PdfCopyFields copier = new iTextSharp.text.pdf.PdfCopyFields(new FileStream(destinationfile, FileMode.Create));
                //PdfImportedPage page;

                //Loops for each file that has been listed               
                foreach (string filename in fileList)
                {
                    //flag++;
                    try
                    {
                        //The current file path
                        string filePath = filename;

                        byte[] pdfbytes = System.IO.File.ReadAllBytes(filePath);
                        iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(pdfbytes);
                        copier.AddDocument(reader);

                    }
                    catch (Exception ex)
                    {
                        string str = ex.Message;
                    }
                }
                copier.Close();

                if (delFile)
                {
                    foreach (string filename in fileList)
                    {
                        try
                        {
                            if (System.IO.File.Exists(filename))
                            {
                                System.IO.File.Delete(filename);
                            }
                        }
                        catch (Exception ex) { string s = ex.Message; }
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                document.Close();
            }
        }

        #endregion
        /* Gets or Sets the DestinationFile*/
        public string DestinationFile
        {
            get { return destinationfile; }
            set { destinationfile = value; }
        }

    }
}