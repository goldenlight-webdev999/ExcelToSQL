using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Data;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace GroGroup
{
    public partial class XtraReport1 : DevExpress.XtraReports.UI.XtraReport
    {

        public XtraReport1()
        {
            InitializeComponent();
        }

        public XtraReport1(string ReportName, object p)
        {
            InitializeComponent();
            LoadReportFromFile(this, ReportName);
        }

        private void LoadReportFromFile(XtraReport1 xtraReport1, string ReportName)
        {
            if (HttpContext.Current.Session["ClubId"] != null)
            {
                string ClubReportFolderPath = System.AppDomain.CurrentDomain.BaseDirectory + HttpContext.Current.Session["ClubId"].ToString();
                if (System.IO.Directory.Exists(ClubReportFolderPath))
                {
                    string Repname = "";
                    if(ReportName.Contains("DevExpReports"))
                    {
                        string[] splitrepname = ReportName.Split(new string[] { "DevExpReports\\" }, StringSplitOptions.None);
                        Repname = splitrepname[1];
                    }
                    string ClubReportPath = System.AppDomain.CurrentDomain.BaseDirectory + HttpContext.Current.Session["ClubId"].ToString() + "\\DevExpReports\\" + Repname.Replace(".rpt", ".repx");
                    if (System.IO.File.Exists(ClubReportPath))
                        ReportName = ClubReportPath;
                }
            }

            ReportName = ReportName.Replace(".rpt", ".repx");
            ReportName = ReportName.Replace(".rdlc", ".repx");
            ReportName = ReportName.Replace(".repxc", ".repx");
            if (File.Exists(ReportName))
            {
                xtraReport1.LoadLayout(ReportName);
            }
            else
            {
                Console.WriteLine("The source file does not exist.");
            }
        }

    }
}
