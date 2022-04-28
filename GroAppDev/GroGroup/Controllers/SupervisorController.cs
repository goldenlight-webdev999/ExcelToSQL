using GroGroup.Class;
using GroGroup.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroGroup.Controllers
{
    [CustomActionFilter]
    [CustomActionException]
    public class SupervisorController : Controller
    {
        string sqlcon = System.Configuration.ConfigurationManager.AppSettings["GroGroup"].ToString();
        DataSet ds = new DataSet();
        SqlDataAdapter da = new SqlDataAdapter();
        Aptus Ggrp = new Aptus();
        SqlConnection Sconn;
        // GET: Supervisor
        public ActionResult UnderConstruction()
        {
            return View();
        }
        
        #region Benefit CleanUp

        public ActionResult BenefitCleanUp()
        {
            return View();
        }

        public ActionResult PurgingBenefitRecords(string period)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = "exec STP_SUP_BenefitPurge '" + period + "'";
            Ggrp.Getdataset(SQl, "BenefitPurge", ds, ref da, Sconn);

            return Json(new { Items = "success" });

        }

        #endregion

        #region Benefits Copy For Next Year

        public ActionResult BenefitsCopyForNextYear()
        {
            string PrgNxtYr = "";
            Sconn = new SqlConnection(sqlcon);
            string sql = "select ProgramYear from OP_system";
            Ggrp.Getdataset(sql, "System", ds, ref da, Sconn);
            if (ds != null && ds.Tables["System"].Rows.Count > 0)
            {
                string ProgramYear = "";
                ProgramYear = (ds.Tables["System"].Rows[0]["ProgramYear"] != null && ds.Tables["System"].Rows[0]["ProgramYear"].ToString() != "") ? ds.Tables["System"].Rows[0]["ProgramYear"].ToString() : "";
                ViewBag.PrgYr = ProgramYear;
                if (!string.IsNullOrEmpty(ProgramYear))
                {
                    string[] sYr = ProgramYear.Split('-');
                    int st = 0, ed = 0;
                    int.TryParse(sYr[0].ToString(), out st);
                    int.TryParse(sYr[1].ToString(), out ed);
                    st = st + 1; ed = ed + 1;

                    string first = "", second = "";
                    first = st.ToString().Length == 1 ? "0" + st.ToString() : st.ToString();
                    second = ed.ToString().Length == 1 ? "0" + ed.ToString() : ed.ToString();

                    PrgNxtYr = first + "-" + second;
                    ViewBag.PrgNxtYr = PrgNxtYr;
                }
            }
            else
            {
                ViewBag.PrgYr = "";
                ViewBag.PrgNxtYr = "";
            }
            return View();
        }

        public ActionResult mfgcombo(bool isDefCheck = true)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            if(isDefCheck)
                sql = "select cast(1 as bit) checked, MfgNo,company from mfg where classcd in ('V','D','N') order by company";
            else
                sql = "select cast(0 as bit) checked, MfgNo,company from mfg where classcd in ('V','D','N') order by company";
            Ggrp.Getdataset(sql, "combofill", ds, ref da, Sconn);
            List<Hashtable> combo = new List<Hashtable>();
            combo = Ggrp.CovertDatasetToHashTbl(ds.Tables["combofill"]);
            return Json(new { items = combo });
        }

        public ActionResult programslist(string period)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select distinct cast(1 as bit) checked, program from benefitprogram where period='" + period + "' order by program";
            Ggrp.Getdataset(sql, "proglist", ds, ref da, Sconn);
            List<Hashtable> program = new List<Hashtable>();
            program = Ggrp.CovertDatasetToHashTbl(ds.Tables["proglist"]);
            return Json(new { items = program });
        }
        
        public ActionResult UpdateBenefitRecords(string periodfrom, string periodto, string Programs, string mfgno, string percent = "")
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", Msg = "";
            decimal perc = 0;
            if (!string.IsNullOrEmpty(percent))
            {
                decimal.TryParse(percent, out perc);
            }
                
            try
            {
                SQl = "exec STP_SUP_BenefitCopyNextYear '" + periodfrom + "','" + periodto + "','" + Programs + "','" + mfgno + "'," + perc + "";
                Ggrp.Getdataset(SQl, "BenefitCopyNextYear", ds, ref da, Sconn);
            }
            catch(Exception ex)
            {
                Msg = ex.Message;
            }

            return Json(new { Items = "success", Msg = Msg });

        }
        

        #endregion

        #region Change UPC

        public ActionResult ChangeUPC(string Upc = "", string Nusername = "", string distId = "", string ym = "", string purchase = "", string mfgname = "")
        {

            string Url = Request.QueryString.ToString();

            if (Url.Contains("distId"))
            {

                string stritemno = Url.Substring(Url.IndexOf("distId"));
                string[] test = stritemno.Split('=');
                distId = test[1].ToString();
                distId = distId.Substring(0, distId.Length - 3);
            }

            bool checkbox = false;
            if (!string.IsNullOrEmpty(Nusername))
            {
                ViewBag.Initials = Nusername;
                checkbox = true;
            }
            else
            {
                ViewBag.Initials = Session["initials"] != null ? Session["initials"].ToString() : "";
            }
            ViewBag.Upc = Upc;
            ViewBag.check = checkbox;

            ViewBag.distId = distId;
            ViewBag.ym = ym;
            ViewBag.purchase = purchase;
            ViewBag.mfgname = mfgname;
            return View();
        }

        public ActionResult GetUpdateUPCCount(string oldUPC)
        {
            int UPCCount = 0;
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "select count(*) as cnt from purchase where isnull(mfgupc,'')='" + oldUPC + "'"; // and yearmonth=" + ym + "";
                Ggrp.Getdataset(SQl, "tblUPCCount", ds, ref da, Sconn);
                int.TryParse(ds.Tables["tblUPCCount"].Rows[0]["cnt"].ToString(), out UPCCount);
            }
            catch (Exception)
            {
                return Json(new { UPCCount = UPCCount });
            }

            return Json(new { UPCCount = UPCCount });
        }

        public ActionResult GetUpdateUPCCountByYM(string oldUPC, string ym)
        {
            int UPCCount = 0;
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "select count(*) as cnt from purchase where isnull(mfgupc,'')='" + oldUPC + "' and yearmonth=" + ym + "";
                Ggrp.Getdataset(SQl, "tblUPCCount", ds, ref da, Sconn);
                int.TryParse(ds.Tables["tblUPCCount"].Rows[0]["cnt"].ToString(), out UPCCount);
            }
            catch (Exception)
            {
                return Json(new { UPCCount = UPCCount });
            }

            return Json(new { UPCCount = UPCCount });
        }

        public ActionResult updatingUPCs(string oldUPC, string NewUPC, string ym, string user)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec [STP_SUP_ChangeUPC] '" + oldUPC + "','" + NewUPC + "','" + ym + "','" + user + "'";
                Ggrp.Getdataset(SQl, "ChangeUPC", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }

            return Json(new { Items = "success" });
        }

        public ActionResult UpdateParticularUPC(string oldUPC, string NewUPC, string distId, string ym, string purchase, string mfgname, string user = "", bool thisRecordOnly = false)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                mfgname = mfgname.Replace("$***$", "&");
                mfgname = mfgname.Replace("$***!$", "''");
                if (thisRecordOnly)
                {
                    SQl = "insert into UPCChangeLog select *,getdate(),'" + user + "' from purchase where distid='" + distId + "' and yearmonth='" + ym + "' and mfgupc='" + oldUPC + "' and purchase='" + purchase + "' and mfgname='" + mfgname + "'";
                    Ggrp.Execute(SQl, Sconn);
                    SQl = "Update purchase set mfgupc='" + NewUPC + "' where distid='" + distId + "' and yearmonth='" + ym + "' and mfgupc='" + oldUPC + "' and purchase='" + purchase + "' and mfgname='" + mfgname + "'";
                }
                else
                {
                    SQl = "insert into UPCChangeLog select *,getdate(),'" + user + "' from purchase where yearmonth='" + ym + "' and mfgupc='" + oldUPC + "' and  mfgname='" + mfgname + "'";
                    Ggrp.Execute(SQl, Sconn);
                    SQl = "Update purchase set mfgupc='" + NewUPC + "' where yearmonth='" + ym + "' and mfgupc='" + oldUPC + "' and  mfgname='" + mfgname + "'";
                }
                Ggrp.Execute(SQl, Sconn);
            }
            catch (Exception)
            {

            }

            return Json(new { Items = "success" });
        }

        #endregion

        #region P&S Purge - Annual

        public ActionResult PSPurgeAnnual()
        {
            return View();
        }

        public ActionResult Annualpurgerecords(string AnnualYear)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", Msg = "";
            try
            {
                SQl = "exec [STP_SUP_PurgePSData] '" + AnnualYear+ "'";
                Ggrp.Getdataset(SQl, "PurgePSData", ds, ref da, Sconn);
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }

            return Json(new { Items = "success", Msg = Msg });

        }

        #endregion

        #region Reset Mfg. Rev Yr Comments

        public ActionResult ResetRevYear()
        {
            return View();
        }

        public ActionResult ResetRevYearComments(string FromYear, string ToYear, string mfgno)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", Msg = "";
            try
            {
                SQl = "exec [STP_SUP_ResetRevYearComments] '" + FromYear + "','" + ToYear + "','" + mfgno + "'";
                Ggrp.Getdataset(SQl, "PurgePSData", ds, ref da, Sconn);
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }

            return Json(new { Items = "success", Msg = Msg });

        }

        #endregion
        #region Reset Mfg. Comments

        public ActionResult ResetComments()
        {
            return View();
        }

        public ActionResult FillResetComments(string mfgno)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", Msg = "";
            try
            {
                SQl = "exec [STP_SUP_ResetComments] '" + mfgno + "'";
                Ggrp.Getdataset(SQl, "PurgePSData", ds, ref da, Sconn);
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }

            return Json(new { Items = "success", Msg = Msg });

        }

        #endregion

        #region Purged Unused UPC

        public ActionResult PurgedUnusedUPC()
        {
            string SQL = "", DefaultPeriod = "";
            SQL = "SELECT DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,cast(curr_dt as varchar(10))+'01'),0)) as pdsdate from OP_system";
            Sconn = new SqlConnection(sqlcon);
            Ggrp.Getdataset(SQL, "cutoffdate", ds, ref da, Sconn);
            if (ds != null && ds.Tables["cutoffdate"] != null && ds.Tables["cutoffdate"].Rows.Count > 0 && ds.Tables["cutoffdate"].Rows[0]["pdsdate"] != DBNull.Value && ds.Tables["cutoffdate"].Rows[0]["pdsdate"].ToString().Trim() != "")
                DefaultPeriod = Convert.ToDateTime(ds.Tables["cutoffdate"].Rows[0]["pdsdate"]).ToString("MM/dd/yyyy");
            ViewBag.pdsdate = DefaultPeriod;
            return View();
        }
        
        public ActionResult UnusedupcSelrecords(string cutoffdate, string value, string upcs = "")
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", Msg = "";
            List<Hashtable> FillPurgedUnusedUPC = new List<Hashtable>();
            try
            {
                SQl = "exec [STP_SUP_ProductsPurgeList] '" + cutoffdate + "','" + value + "','" + upcs + "'";
                Ggrp.Getdataset(SQl, "Unusedupc", ds, ref da, Sconn);
                FillPurgedUnusedUPC = Ggrp.CovertDatasetToHashTbl(ds.Tables["Unusedupc"]);
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }
            return Json(new { Items = FillPurgedUnusedUPC, Msg = Msg });

        }
        
        #endregion

        #region Benefit BatchNo Update

        public ActionResult BenifitBatchNoUpdate()
        {
            return View();
        }
        public ActionResult BenifitBatchRecords()
        {
            string SQl = "";
            Sconn = new SqlConnection(sqlcon);
            List<Hashtable> BenBatch = new List<Hashtable>();
            SQl = @"Select d.ProgNo,d.DistNo,dis.Company as Distributor,c.Checknum,d.Purchase,p.Mfgno, m.company as MfgName
                    from benefitDistribution d
                    left join benefitCheck c on c.Progno = d.Progno and c.checknum = d.checknum
                    left join benefitprogram p on c.Progno = p.Progno
                    left join distributor dis on d.distno = dis.distno
                    left join mfg m on p.mfgno = m.mfgno
                    where isnull(c.holdpay, 0)= 0 and isnull(d.batchno, '')= '' and isnull(c.batchno, '')= ''
                    order by p.Mfgno,m.company,d.DistNo,dis.Company";
            Ggrp.Getdataset(SQl, "BenefitBatch", ds, ref da, Sconn);
            BenBatch = Ggrp.CovertDatasetToHashTbl(ds.Tables["BenefitBatch"]);
            return Json(new { Items = BenBatch });

        }

        #endregion

    }
}