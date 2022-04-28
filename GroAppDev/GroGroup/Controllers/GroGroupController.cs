using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.InternalAccess;
using DevExpress.XtraReports.UI;
using ExportToExcel;
using GroGroup.Class;
using GroGroup.Filters;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GroGroup.Controllers
{
    [CustomActionFilter]
    [CustomActionException]
    public class GroGroupController : Controller
    {
        string sqlcon = System.Configuration.ConfigurationManager.AppSettings["GroGroup"].ToString();
        DataSet ds = new DataSet();
        SqlDataAdapter da = new SqlDataAdapter();
        Aptus Ggrp = new Aptus();
        SqlConnection Sconn;

        #region Login
        // GET: GroGroup
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            //Employee Validation
            DataSet dsEmp = new DataSet();
            SqlDataAdapter daEmp = new SqlDataAdapter();
            Sconn = new SqlConnection(sqlcon);
            string SQL = "";
            SQL = "Select * from OP_Employee where initials='" + username + "' and password='" + password + "' and isnull(Inactive,0)=0";
            Ggrp.Getdataset(SQL, "tblEmp", dsEmp, ref daEmp, Sconn);
            if (dsEmp != null && dsEmp.Tables["tblEmp"] != null && dsEmp.Tables["tblEmp"].Rows.Count > 0)
            {
                Session["initials"] = dsEmp.Tables["tblEmp"].Rows[0]["initials"].ToString();
                Session["UserName"] = dsEmp.Tables["tblEmp"].Rows[0]["name"].ToString();
                Session["empLevel"] = dsEmp.Tables["tblEmp"].Rows[0]["empLevel"].ToString();
                Session["Email"] = dsEmp.Tables["tblEmp"].Rows[0]["Email"].ToString();
                return RedirectToAction("Main");
            }
            else
            {
                ViewBag.Message = "Invalid Initials/Password";
                return View();
            }
        }
        #endregion

        #region Logout

        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            return Json(new { Items = "" });
        }

        #endregion

        #region SessionTimeout

        public ActionResult GetSessionTimeout()
        {
            int value = (ConfigurationManager.AppSettings["SessionTimeout"] == null || ConfigurationManager.AppSettings["SessionTimeout"] == "") ? 60 : Convert.ToInt32(ConfigurationManager.AppSettings["SessionTimeout"].ToString());
            return Json(new { timeout = value });
        }

        #endregion

        #region IPRestriction

        public ActionResult ValidateClientIPAddress()
        {
            string ClientIP = "", Msg = "", AllowedIPs = "";
            ClientIP = Ggrp.GetVisitorIPAddress();

            string SQL = "";
            Sconn = new SqlConnection(sqlcon);

            SQL = "Select * from OP_system";
            Ggrp.Getdataset(SQL, "tblIPAddr", ds, ref da, Sconn);

            if(ds != null && ds.Tables["tblIPAddr"] != null && ds.Tables["tblIPAddr"].Rows.Count > 0 && ds.Tables["tblIPAddr"].Rows[0]["AllowedIPs"] != DBNull.Value && ds.Tables["tblIPAddr"].Rows[0]["AllowedIPs"].ToString().Trim() != "")
            {
                AllowedIPs = ds.Tables["tblIPAddr"].Rows[0]["AllowedIPs"].ToString().Trim();
                if(!AllowedIPs.Contains(ClientIP))
                {
                    Msg = "IP: " + ClientIP + " Blocked";
                }
            }

            Session["RestrictionMsg"] = Msg;
            return Json(new { Items = Msg });
        }

        public ActionResult IPRestriction()
        {
            ViewBag.Msg = Session["RestrictionMsg"] != null ? Session["RestrictionMsg"].ToString() : "";
            return View();
        }

        public ActionResult SecretCode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SecretCode(string EditSecretCode)
        {
            return RedirectToAction("Login");
        }

        #endregion

        #region HomePage

        public ActionResult HomePage()
        {
            return View();
        }

        #endregion

        #region Main
        public ActionResult Main()
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter("STP_GetRights 'Supervisor'", Sconn);
            da.Fill(ds, "SecPoints");
            Session["SecPoints"] = ds.Tables["SecPoints"];
            ViewBag.Initials = Session["initials"].ToString();
            ViewBag.UserName = Session["UserName"].ToString();
            return View();
        }

        public ActionResult FillSearchComboList(string ScreenFrom, string FormClass)
        {
            Sconn = new SqlConnection(sqlcon);
            List<Hashtable> Fillcombo = new List<Hashtable>();
            string SQl = "select * from op_searchselect where Screen ='" + ScreenFrom + "' and FormClass ='" + FormClass + "' order by orderby asc";
            Ggrp.Getdataset(SQl, "Op_searchselect", ds, ref da, Sconn);
            Fillcombo = Ggrp.CovertDatasetToHashTbl(ds.Tables["Op_searchselect"]);
            return Json(new { Items = Fillcombo });
        }

        #endregion

        #region ReInitSession

        public ActionResult ReIniSession()
        {
            return Json(new { Items = "" });
        }

        #endregion

        #region Menus
        public ActionResult GetList(string selection)
        {
            List<MainMenus> SearchList = new List<MainMenus>();
            List<MainMenus> List = new List<MainMenus>();
            if (selection == "OP") SearchList = OperationList();

            if (Session["SecPoints"] != null)
            {
                DataTable dt = (DataTable)Session["SecPoints"];

                for (int j = 0; j < SearchList.Count; j++)
                {
                    DataRow drMainMenu = dt.Select("SecurityID like 'MAINMENU%' and usermodule='" + SearchList[j].userModule + "'", "SecurityID,UserModule").FirstOrDefault();
                    if (drMainMenu != null)
                    {
                        if (drMainMenu["UserAdd"].ToString().Trim() == "True"
                        || drMainMenu["UserEdit"].ToString().Trim() == "True"
                        || drMainMenu["UserDelete"].ToString().Trim() == "True"
                        || drMainMenu["UserSearch"].ToString().Trim() == "True")
                        {
                            MainMenus menuitem = new MainMenus();
                            menuitem.Description = SearchList[j].Description;
                            menuitem.userModule = SearchList[j].userModule;
                            menuitem.ControllerName = SearchList[j].ControllerName;
                            List.Add(menuitem);
                        }
                    }
                }
                Session["MenuList"] = List;
            }
            return Json(new { Items = List, Preference = "HomePage" });
        }

        public List<MainMenus> OperationList()
        {
            string conn = sqlcon;
            List<MainMenus> MenuList = new List<MainMenus>();
            DataSet ds = new DataSet();
            string lsql = "";
            lsql = "select UserModule from OP_Security_Points where Module='web'";
            Sconn = new SqlConnection(sqlcon);
            SqlDataAdapter da = new SqlDataAdapter(lsql, Sconn);
            da.Fill(ds, "resource");
            DataRow[] result1 = ds.Tables[0].Select("UserModule= 'Files'");
            if (result1.Length > 0)
            {
                MenuList.Add(new MainMenus
                {
                    Description = "Files",
                    userModule = "Files",
                    ControllerName = "GroGroup"
                });
            }
            DataRow[] result2 = ds.Tables[0].Select("UserModule= 'Utility'");
            if (result2.Length > 0)
            {
                MenuList.Add(new MainMenus
                {
                    Description = "Utility",
                    userModule = "Utility",
                    ControllerName = "Utility"
                });
            }
            DataRow[] result3 = ds.Tables[0].Select("UserModule= 'SP'");
            if (result3.Length > 0)
            {
                MenuList.Add(new MainMenus
                {
                    Description = "Supervisor",
                    userModule = "SP",
                    ControllerName = "Supervisor"
                });
            }
            DataRow[] result4 = ds.Tables[0].Select("UserModule= 'RP'");
            if (result4.Length > 0)
            {
                MenuList.Add(new MainMenus
                {
                    Description = "Reports",
                    userModule = "RP",
                    ControllerName = "Reports"
                });
            }
            return MenuList;
        }

        public ActionResult FillSubMenu(string usermodule)
        {
            List<Hashtable> dtSubMenu = new List<Hashtable>();
            if (Session["SecPoints"] != null)
            {
                DataTable dt = (DataTable)Session["SecPoints"];
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = "Module='Web' and SecurityID like 'SUBMENU_%' and UserModule='" + usermodule + "' and SecurityID not like 'X%' and SecurityID not like '%OPT%' and (useradd=1 or useredit=1 or userdelete=1 or UserSearch=1)";
                    dv.Sort = "SecurityID,Description";
                    DataTable dtMenu = dv.ToTable();
                    if (usermodule.Trim().ToUpper() != "RP")
                    {
                        dtSubMenu = Ggrp.CovertDatasetToHashTbl(dtMenu);
                    }

                }
            }
            return Json(new { Items = dtSubMenu });
        }

        #endregion

        #region ZipCode

        public ActionResult Zipcode()
        {
            return View();
        }
        
        public ActionResult SearchZipcombo()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "Select Distinct state from OP_ZipCode order by state";
            Ggrp.Getdataset(sql, "Zipcode", ds, ref da, Sconn);
            List<Hashtable> Zipcode = new List<Hashtable>();
            Zipcode = Ggrp.CovertDatasetToHashTbl(ds.Tables["Zipcode"]);
            return Json(new { Items = Zipcode });
        }

        public ActionResult FillZipCodeGrid(string Filterstate, string cityid)
        {
            string Sql = "";
            int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            Sql = "Select * from OP_ZipCode where state='" + Filterstate + "' order by city";
            Ggrp.Getdataset(Sql, "zipcodeGrid", ds, ref da, Sconn);
            List<Hashtable> zipcodeGrid = new List<Hashtable>();
            zipcodeGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["zipcodeGrid"]);
            if (ds.Tables["zipcodeGrid"].Rows.Count > 0 && !string.IsNullOrEmpty(cityid))
            {
                var rowlist = ds.Tables["zipcodeGrid"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["zipcodeGrid"].Select().ToList()
                                   where dr["cityid"].ToString() == cityid
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            Session["zipcodeGrid"] = ds;
            return Json(new { Items = zipcodeGrid, rowindex = rowindex });
        }

        public ActionResult SaveZipCode(string zipcodelist, bool IsAddMode = false)
        {
            string Qry = "";
            int CityNo = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            zipcodelist = "[" + zipcodelist + "]";
            List<Hashtable> zipList = jss.Deserialize<List<Hashtable>>(zipcodelist);
            DataSet sds = new DataSet();
            SqlDataAdapter sda = new SqlDataAdapter();

            decimal lat = 0, lng = 0;
            if (zipList[0]["lat"] != null && zipList[0]["lat"].ToString() != "")
                decimal.TryParse(zipList[0]["lat"].ToString(), out lat);

            if (zipList[0]["lng"] != null && zipList[0]["lng"].ToString() != "")
                decimal.TryParse(zipList[0]["lng"].ToString(), out lng);
            if (IsAddMode == true)
            {
                Qry = "select * from op_zipcode where 1=0";
            }
            else
            {
                Qry = "select * from op_zipcode where cityid='" + zipList[0]["cityid"].ToString().Trim() + "'";
            }
            Ggrp.Getdataset(Qry, "ZipCode", sds, ref sda, Sconn, false);
            if (IsAddMode)
            {
                Qry = "insert into op_zipcode(City,State,Zip,Country,lat,lng) values ('" + zipList[0]["city"].ToString() + "','" + zipList[0]["state"].ToString() + "','" + zipList[0]["zip"].ToString() + "','" + zipList[0]["country"].ToString() + "','" + lat + "','" + lng + "')";

            }
            else
                Qry = "update op_zipcode set City ='" + zipList[0]["city"].ToString() + "', State = '" + zipList[0]["state"].ToString() + "',Zip ='" + zipList[0]["zip"].ToString() + "', Country='" + zipList[0]["country"].ToString() + "', lat = '" + lat + "', lng = '" + lng + "' where cityid = '" + zipList[0]["cityid"].ToString() + "'";
            Ggrp.Execute(Qry, Sconn, false);
            if (IsAddMode == true)
            {
                CityNo = Ggrp.GetIdentityValue(Sconn);
            }
            else
            {
                CityNo = Convert.ToInt32(zipList[0]["cityid"].ToString().Trim());
            }
            return Json(new { Items = CityNo });
        }

        public ActionResult ValidateZipcode(string City, string State, string Zip, bool AddMode, string Gcityid)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";

            if (AddMode == true)
                lSql = " select City,State,Zip from op_zipcode where City='" + City + "' and State='" + State + "'and  Zip='" + Zip + "'";
            else
                lSql = " select City,State,Zip from op_zipcode where City = '" + City + "'and State='" + State + "' and  Zip='" + Zip + "'and cityid!='" + Gcityid + "'";

            Ggrp.Getdataset(lSql, "DuplicateZip", ds, ref da, Sconn);

            if (ds.Tables["DuplicateZip"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }


        public ActionResult DeleteZipCode(string cityid)
        {
            Sconn = new SqlConnection(sqlcon);
            string sSQL = "delete from OP_ZipCode where cityid =" + cityid;
            Ggrp.Execute(sSQL, Sconn);
            return Json(new { Items = "" });
        }
        public ActionResult FilterCity(string SearchCity)
        {
            if (SearchCity.Contains("'"))
                SearchCity = SearchCity.Replace("'", "''");
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = " Select * from OP_ZipCode where 1=1 and  City Like'" + SearchCity + "%'";
            Ggrp.Getdataset(sql, "Zipcode", ds, ref da, Sconn);
            List<Hashtable> Zipcode = new List<Hashtable>();
            Zipcode = Ggrp.CovertDatasetToHashTbl(ds.Tables["Zipcode"]);
            return Json(new { Items = Zipcode });
        }

        public ActionResult CancelZipCode(string cityid)
        {
            var rowindex = 0;
            DataSet Zds = (DataSet)Session["zipcodeGrid"];
            List<Hashtable> zipcodeGrid = new List<Hashtable>();
            zipcodeGrid = Ggrp.CovertDatasetToHashTbl(Zds.Tables["zipcodeGrid"]);
            if (Zds.Tables["zipcodeGrid"].Rows.Count > 0 && !string.IsNullOrEmpty(cityid))
            {
                var rowlist = Zds.Tables["zipcodeGrid"].Select().ToList();
                var getCaseRows = (from dr in Zds.Tables["zipcodeGrid"].Select().ToList()
                                   where dr["cityid"].ToString() == cityid
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = zipcodeGrid, rowindex = rowindex });
        }

        public ActionResult FilterZip(string SearchZIP)
        {
            if (SearchZIP.Contains("'"))
                SearchZIP = SearchZIP.Replace("'", "''");
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = " Select * from OP_ZipCode where 1=1 and  zip Like'" + SearchZIP + "%'";
            Ggrp.Getdataset(sql, "Zipcode", ds, ref da, Sconn);
            List<Hashtable> Zipcode = new List<Hashtable>();
            Zipcode = Ggrp.CovertDatasetToHashTbl(ds.Tables["Zipcode"]);
            return Json(new { Items = Zipcode });
        }

        public ActionResult searchzip(string getzip)
        {
            if (getzip.Contains("'"))
                getzip = getzip.Replace("'", "''");
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select * from OP_ZipCode where zip='" + getzip + "'";
            Ggrp.Getdataset(sql, "zipcode", ds, ref da, Sconn);
            List<Hashtable> Zipcode = new List<Hashtable>();
            Zipcode = Ggrp.CovertDatasetToHashTbl(ds.Tables["zipcode"]);
            return Json(new { Items = Zipcode });
        }

        #endregion

        #region Lookup
        public ActionResult Lookup()
        {
            return View();
        }

        public ActionResult Filllookup(string search)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            if (search != null && search != "")
            {
                sql = "Select  distinct(lookupid)  from OP_lookup where lookupid like '" + search + "%' ";
                Ggrp.Getdataset(sql, "cmblookup", ds, ref da, Sconn);
            }
            else
            {
                sql = "Select '' as lookupid union all Select  distinct(lookupid) as Lookup from OP_lookup order by lookupid";
                Ggrp.Getdataset(sql, "cmblookup", ds, ref da, Sconn);
            }
            List<Hashtable> cmblookup = new List<Hashtable>();
            cmblookup = Ggrp.CovertDatasetToHashTbl(ds.Tables["cmblookup"]);
            return Json(new { Items = cmblookup });
        }
        public ActionResult FilllookupGrid(String lookupid, String entry)
        {
            string Sql = "";
            int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            if (!string.IsNullOrEmpty(lookupid))
                Sql = "select lookupid,entry,descript,code_a,code_b,Inactive,comment from OP_lookup where lookupid='" + lookupid + "' order by descript";
            else
                Sql = "select top 10 lookupid ,entry,descript,code_a,code_b,Inactive,comment from OP_lookup order by descript";
            Ggrp.Getdataset(Sql, "lookup", ds, ref da, Sconn);
            List<Hashtable> lookup = new List<Hashtable>();
            lookup = Ggrp.CovertDatasetToHashTbl(ds.Tables["lookup"]);

            if (ds.Tables["lookup"].Rows.Count > 0 && !string.IsNullOrEmpty(entry))
            {
                var rowlist = ds.Tables["lookup"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["lookup"].Select().ToList()
                                   where dr["entry"].ToString() == entry
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }

            return Json(new { Items = lookup, rowindex = rowindex });
        }

        public ActionResult DeleteLookup(string lookupid, string entry)
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            if (!string.IsNullOrEmpty(lookupid) && !string.IsNullOrEmpty(entry))
            {
                string SQl = "";
                SQl = "delete from OP_lookup where  lookupid ='" + lookupid + "' and entry ='" + entry + "'";
                Ggrp.Execute(SQl, Sconn);
            }
            return Json(new { Items = "" });
        }
        public ActionResult SaveLookup(string lookupid = "", string entry = "", string descript = "", string CodeA = "", string CodeB = "", string Inactive = "", string comments = "", bool IsAdd = true)
        {
            string sSQL = "";
            Sconn = new SqlConnection(sqlcon);

            if (IsAdd == true)
                sSQL = "insert into OP_lookup(lookupid,entry,descript,code_a,code_b,Inactive,comment) values('" + lookupid + "','" + entry + "','" + descript + "','" + CodeA + "','" + CodeB + "','" + Inactive + "','" + comments + "')";
            else
                sSQL = "update OP_lookup set descript='" + descript + "',code_a='" + CodeA + "',code_b='" + CodeB + "',Inactive='" + Inactive + "',comment='" + comments + "' where lookupid= '" + lookupid + "' and entry= '" + entry + "'";
            Ggrp.Execute(sSQL, Sconn);
            return Json(new { Items = "" });
        }
        public ActionResult DuplicateLookup(string entry, string lookupid, bool AddMode = false)
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
                    lSql = "select entry from OP_lookup where lookupid='" + lookupid + "' and entry='" + entry + "'";
                    Ggrp.Getdataset(lSql, "entrychk", ds, ref da, Sconn);
                }

                if (ds.Tables["entrychk"].Rows.Count > 0)
                {
                    count = ds.Tables["entrychk"].Rows.Count;
                    Ismsg = true;
                    if (ds.Tables["entrychk"].Rows.Count == 1)
                        Message = "Entry already exists";
                }
                return Json(new { Message = Message, Ismsg = Ismsg, count = count });
            }
        }

        
        #endregion

        #region Employee
        public ActionResult Employee()
        {
            ViewBag.empLevel = Session["empLevel"].ToString();
            return View();
        }
        public ActionResult GetEmployeesList(string initial = "")
        {
            var rowindex = 0;
            SqlConnection Sconn = new SqlConnection(sqlcon);
            List<Hashtable> emplist = new List<Hashtable>();
            string SQl = "select e.*, g.Description from op_employee e  left join OP_Security_UserGroup g on e.empLevel =  g.GroupID";
            Ggrp.Getdataset(SQl, "Op_employee", ds, ref da, Sconn);
            emplist = Ggrp.CovertDatasetToHashTbl(ds.Tables["Op_employee"]);
            if (!string.IsNullOrEmpty(initial) && ds.Tables["Op_employee"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["Op_employee"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["Op_employee"].Select().ToList()
                                   where dr["initials"].ToString().Trim() == initial
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = emplist, rowindex = rowindex });
        }

        public ActionResult GetUserGroup()
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            List<Hashtable> UserGrps = new List<Hashtable>();
            string SQl = "select * from OP_Security_UserGroup";
            Ggrp.Getdataset(SQl, "OP_Security_UserGroup", ds, ref da, Sconn);
            UserGrps = Ggrp.CovertDatasetToHashTbl(ds.Tables["OP_Security_UserGroup"]);
            return Json(new { Items = UserGrps });
        }

        public ActionResult ValidateEmpInitial(string initials)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            lSql = " select initials from op_employee where initials='" + initials + "'";
            Ggrp.Getdataset(lSql, "GetInitials", ds, ref da, Sconn);
            if (ds.Tables["GetInitials"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult SaveEmployee(string ObjEmpList = "", bool IsAddMode = false)
        {
            string sSQL = "";
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjEmpList = "[" + ObjEmpList + "]";
            List<Hashtable> AddEmployeeList = jss.Deserialize<List<Hashtable>>(ObjEmpList);
            if (IsAddMode)
                sSQL = "insert into op_employee(initials,name,password,Phone,Email,empLevel,Inactive) values('" + AddEmployeeList[0]["initials"].ToString().Trim() + "','" + AddEmployeeList[0]["name"].ToString().Trim() + "','" + AddEmployeeList[0]["password"].ToString().Trim() + "','" + AddEmployeeList[0]["Phone"].ToString().Trim() + "','" + AddEmployeeList[0]["Email"].ToString().Trim() + "','" + AddEmployeeList[0]["empLevel"].ToString().Trim() + "','" + AddEmployeeList[0]["inactive"].ToString().Trim() + "')";
            else
                sSQL = "update op_employee set name='" + AddEmployeeList[0]["name"].ToString().Trim() + "',password='" + AddEmployeeList[0]["password"].ToString().Trim() + "',empLevel='" + AddEmployeeList[0]["empLevel"].ToString().Trim() + "',Phone='" + AddEmployeeList[0]["Phone"].ToString().Trim() + "',Email='" + AddEmployeeList[0]["Email"].ToString().Trim() + "',Inactive='" + AddEmployeeList[0]["inactive"].ToString().Trim() + "' where initials='" + AddEmployeeList[0]["initials"].ToString().Trim() + "'";
            Ggrp.Execute(sSQL, Sconn);
            return Json(new { Items = AddEmployeeList[0]["initials"].ToString().Trim() });
        }

        public ActionResult DeleteEmployee(string Initials)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from op_employee where  initials ='" + Initials + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }

        #endregion

        #region Manufacturers

        public ActionResult Manufacturers()
        {
            Session["roomlistfrom"] = "Manufacturer";
            return View("Manufacturers");
        }
        
        public ActionResult OpSearchList(string searchtable, string selectedvalue, string searchtext)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            List<Hashtable> ResultList = new List<Hashtable>();
            if (searchtext.Contains("'"))
                searchtext = searchtext.Replace("'", "''");
            if (searchtext == "%")
            {
                if (searchtable == "OPCATCODE")
                    SQl = "Select top 50 * from op_lookup where lookupid='catcode' and " + selectedvalue + " like '" + searchtext + "%' order by " + selectedvalue + " desc";
                else
                    SQl = "Select top 50 * from " + searchtable + " order by " + selectedvalue + " desc";
            }
            else
            {
                if (searchtable == "OPCATCODE")
                    SQl = "Select top 5 * from op_lookup where lookupid='catcode' and " + selectedvalue + " like '" + searchtext + "%' order by " + selectedvalue + " desc";
                else
                    SQl = "Select top 5 * from " + searchtable + " where " + selectedvalue + " like '" + searchtext + "%'";
            }
            Ggrp.Getdataset(SQl, "Results", ds, ref da, Sconn);
            ResultList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Results"]);
            return Json(new { Items = ResultList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowMfgList(string mfgno)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            if (!string.IsNullOrEmpty(mfgno))
                SQl = " Select * from mfg where  mfgno='" + mfgno + "'";
            else
                SQl = "Select top 1 * from mfg order by mfgno desc";
            Ggrp.Getdataset(SQl, "Mfg", ds, ref da, Sconn);
            List<Hashtable> MfgList = new List<Hashtable>();
            MfgList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Mfg"]);

            Session["mfgno"] = MfgList[0]["MfgNo"];
            Session["company"] = MfgList[0]["company"];

            Session["roomlistfrom"] = "Manufacturer";

            Session["DistNo"] = "";
            return Json(new { Items = MfgList });
        }

        public ActionResult ValidatemfgCompany(string company, bool AddMode, string mfgno)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (company.Contains("'"))
            {
                company = company.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select company from mfg where company='" + company + "'";
            else
                lSql = " select company from mfg where company = '" + company + "' and MfgNo!='" + mfgno + "'";

            Ggrp.Getdataset(lSql, "FillCompany", ds, ref da, Sconn);

            if (ds.Tables["FillCompany"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult ShowshipinfoList(string MfgNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = " Select * from mfgshipinfo where  mfgno='" + MfgNo + "'";
            Ggrp.Getdataset(SQl, "shipinfo", ds, ref da, Sconn);
            List<Hashtable> ShipInfoList = new List<Hashtable>();
            ShipInfoList = Ggrp.CovertDatasetToHashTbl(ds.Tables["shipinfo"]);
            return Json(new { Items = ShipInfoList });
        }

        public ActionResult GetLookupList(string LukpID, bool includeEmpty = true)
        {
            string SQl = "";
            Sconn = new SqlConnection(sqlcon);
            if(includeEmpty)
                SQl = "select '' as entry ,'' as descript union Select distinct(entry),descript from op_lookup where lookupid='" + LukpID + "' and isnull(inactive,0)<>1";
            else
                SQl = "Select distinct(entry),descript from op_lookup where lookupid='" + LukpID + "' and isnull(inactive,0)<>1";
            Ggrp.Getdataset(SQl, "lookup", ds, ref da, Sconn);
            List<Hashtable> LukupList = new List<Hashtable>();
            LukupList = Ggrp.CovertDatasetToHashTbl(ds.Tables["lookup"]);
            return Json(new { Items = LukupList });
        }
        

        public ActionResult SaveMfg(string ObjmfgList = "", bool IsAddMode = false)
        {
            string Sql = ""; int NewMfno = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjmfgList = "[" + ObjmfgList + "]";
            List<Hashtable> AddMfg = jss.Deserialize<List<Hashtable>>(ObjmfgList);
            DataSet mds = new DataSet();
            SqlDataAdapter mda = new SqlDataAdapter();
            if (Sconn.State != ConnectionState.Open)
                Sconn.Open();
            SqlTransaction SqlTrans = Sconn.BeginTransaction();
            DataRow drAdd;

            if (AddMfg.Count > 0)
            {
                try
                {
                    mds = new DataSet();
                    if (IsAddMode == true)
                    {
                        Sql = "select * from mfg where 1=0";
                    }
                    else
                    {
                        Sql = "select * from mfg where mfgno='" + AddMfg[0]["mfgno"].ToString() + "'";
                    }
                    mda = new SqlDataAdapter(Sql, Sconn);
                    Ggrp.Getdataset(Sql, "tblmfg", mds, ref mda, Sconn, ref SqlTrans);
                    if (IsAddMode == true)
                    {
                        drAdd = mds.Tables["tblmfg"].NewRow();
                    }
                    else
                    {
                        drAdd = mds.Tables["tblmfg"].Rows[0];
                    }
                    drAdd["company"] = AddMfg[0]["company"].ToString();
                    drAdd["mailadd"] = AddMfg[0]["mailadd"].ToString();
                    drAdd["city"] = AddMfg[0]["city"].ToString();
                    drAdd["state"] = AddMfg[0]["state"] == null ? "" : AddMfg[0]["state"].ToString();
                    drAdd["zip"] = AddMfg[0]["zip"].ToString();
                    drAdd["country"] = AddMfg[0]["country"] == null ? "" : AddMfg[0]["country"].ToString();
                    drAdd["phone"] = AddMfg[0]["phone"].ToString();
                    drAdd["phone2"] = AddMfg[0]["phone2"].ToString();
                    drAdd["fax"] = AddMfg[0]["fax"].ToString();
                    drAdd["voicemail"] = AddMfg[0]["voicemail"].ToString(); //bit
                    drAdd["email"] = AddMfg[0]["email"].ToString();
                    drAdd["webpage"] = AddMfg[0]["webpage"].ToString();
                    drAdd["webpage2"] = AddMfg[0]["webpage2"].ToString();
                    drAdd["corpname"] = AddMfg[0]["corpname"].ToString();
                    drAdd["shipadd"] = AddMfg[0]["shipadd"].ToString();
                    drAdd["scity"] = AddMfg[0]["scity"].ToString();
                    drAdd["sstate"] = AddMfg[0]["sstate"] == null ? "" : AddMfg[0]["sstate"].ToString();
                    drAdd["nszip"] = AddMfg[0]["nszip"].ToString();
                    drAdd["scountry"] = AddMfg[0]["scountry"] == null ? "" : AddMfg[0]["scountry"].ToString();
                    drAdd["ngpeffdt"] = AddMfg[0]["ngpeffdt"].ToString();
                    drAdd["ngp_xcpt"] = AddMfg[0]["ngp_xcpt"].ToString();
                    drAdd["ngpmoddt"] = AddMfg[0]["ngpmoddt"].ToString();
                    drAdd["mrevdesc"] = AddMfg[0]["mrevdesc"].ToString();
                    drAdd["vpc"] = AddMfg[0]["vpc"].ToString();
                    drAdd["vpc_rmks"] = AddMfg[0]["vpc_rmks"].ToString();
                    drAdd["canprog"] = AddMfg[0]["canprog"].ToString();
                    drAdd["primary"] = AddMfg[0]["primary"] == null ? "" : AddMfg[0]["primary"].ToString();
                    drAdd["secondary"] = AddMfg[0]["secondary"] == null ? "" : AddMfg[0]["secondary"].ToString();
                    drAdd["classcd"] = AddMfg[0]["classcd"] == null ? "" : AddMfg[0]["classcd"].ToString();
                    drAdd["codec"] = AddMfg[0]["codec"] == null ? "" : AddMfg[0]["codec"].ToString().Trim();
                    drAdd["codea"] = AddMfg[0]["codea"] == null ? "" : AddMfg[0]["codea"].ToString().Trim();
                    drAdd["status"] = AddMfg[0]["status"] == null ? "" : AddMfg[0]["status"].ToString();
                    drAdd["termyr"] = AddMfg[0]["termyr"] == null ? "" : AddMfg[0]["termyr"].ToString();
                    drAdd["officehrs"] = AddMfg[0]["officehrs"].ToString();
                    drAdd["mds_stat"] = AddMfg[0]["mds_stat"] == null ? "" : AddMfg[0]["mds_stat"].ToString();
                    drAdd["mdsrevyr"] = AddMfg[0]["mdsrevyr"] == null ? "" : AddMfg[0]["mdsrevyr"].ToString();
                    drAdd["prog_ly"] = AddMfg[0]["prog_ly"].ToString();
                    drAdd["prog_cy"] = AddMfg[0]["prog_cy"].ToString();
                    drAdd["pds_ly"] = AddMfg[0]["pds_ly"].ToString();
                    drAdd["pds_cy"] = AddMfg[0]["pds_cy"].ToString();
                    drAdd["prgrevty"] = AddMfg[0]["prgrevty"] == null ? "" : AddMfg[0]["prgrevty"].ToString();
                    drAdd["prgrevly"] = AddMfg[0]["prgrevly"] == null ? "" : AddMfg[0]["prgrevly"].ToString();
                    drAdd["prgrevpy"] = AddMfg[0]["prgrevpy"] == null ? "" : AddMfg[0]["prgrevpy"].ToString();
                    drAdd["hwsb"] = AddMfg[0]["hwsb"].ToString();
                    drAdd["igc_chi"] = AddMfg[0]["igc_chi"].ToString();
                    drAdd["ref_by"] = AddMfg[0]["ref_by"].ToString();
                    drAdd["products"] = AddMfg[0]["products"].ToString();
                    if (IsAddMode == true)
                    {
                        mds.Tables["tblmfg"].Rows.Add(drAdd);
                    }
                    else
                    {
                        mds.Tables["tblmfg"].GetChanges();
                    }
                    mda.Update(mds, "tblmfg");

                    if (IsAddMode == true)
                    {
                        NewMfno = Ggrp.GetIdentityValue(Sconn, SqlTrans);
                    }
                    else
                    {
                        NewMfno = Convert.ToInt32(AddMfg[0]["mfgno"].ToString());
                    }
                    
                    SqlTrans.Commit();
                    if (Sconn.State == ConnectionState.Open) Sconn.Close();
                }
                catch (Exception ex)
                {
                    if (Sconn.State == ConnectionState.Open && SqlTrans != null && SqlTrans.Connection != null)
                        SqlTrans.Rollback();
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { Items = NewMfno });
        }

        #endregion

        #region SearchScreen

        public ActionResult SearchScreen()
        {
            string srchtext = "";
            Session["Table"] = Request.QueryString["table"] != null ? Request.QueryString["table"].ToString() : "";
            if (Request.QueryString["text"] !=null && Request.QueryString["text"].Contains("�"))
            {
                string strStatus = Request.Url.GetComponents(UriComponents.Query, UriFormat.SafeUnescaped);
                var strStatus1 = strStatus.Split('%');
                var strStatus2 = strStatus1[1].Split('&');
                Session["Text"] = "%" + strStatus2[0].ToLower();
            }
            else
            {
                if (Request.QueryString["text"] != null)
                {
                    srchtext = Request.QueryString["text"].ToString();
                    srchtext = srchtext.Replace("amp;", "&");
                }
                Session["Text"] = Request.QueryString["text"] != null ? srchtext : "";
            }
            Session["Field"] = Request.QueryString["field"] != null ? Request.QueryString["field"].ToString() : "";
            Session["header"] = Request.QueryString["header"] != null ? Request.QueryString["header"].ToString() : "";
            Session["Where"] = Request.QueryString["where"] != null ? Request.QueryString["where"].ToString() : "";
            Session["Element"] = Request.QueryString["element"] != null ? Request.QueryString["element"].ToString() : "";
            ViewBag.LabelDescription = Request.QueryString["LabelDescription"] != null ? Request.QueryString["LabelDescription"].ToString() : "";
            if (Request.QueryString["LabelDescription"] != null) Session["LabelDescription"] = Request.QueryString["LabelDescription"].ToString();
            else Session["LabelDescription"] = null;
            Session["JoinContition"] = Request.QueryString["JoinContition"] != null ? Request.QueryString["JoinContition"].ToString() : "";
            if (Request.QueryString["condition"] != null) Session["Condition"] = Request.QueryString["condition"].ToString();
            else Session["Condition"] = null;
            ViewBag.Schtable = Session["Table"];
            ViewBag.Schelement = Session["Element"];
            return View();
        }

        public ActionResult GetComboList()
        {
            if (sqlcon != null)
                Sconn = new SqlConnection(sqlcon);
            DataSet dsc = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            List<ArrayList> newval = new List<ArrayList>();
            string sql = "select '' Value,'' as Text from op_searchselect where 1=0";
            Ggrp.Getdataset(sql, "combolist", dsc, ref da, Sconn);
            if (Session["Field"] != null && Session["header"] != null)
            {
                string Field = Session["Field"].ToString();
                string head = Session["header"].ToString();
                string[] combo = Field.Split(',');
                string[] headcombo = head.Split(',');
                for (int i = 0; i < combo.Length; i++)
                {
                    DataRow dr = dsc.Tables["combolist"].NewRow();
                    dr["Value"] = combo[i];
                    dr["Text"] = headcombo[i];
                    dsc.Tables["combolist"].Rows.Add(dr);
                }
                foreach (DataRow dRow in dsc.Tables["combolist"].Rows)
                {
                    ArrayList values = new ArrayList();
                    foreach (object value in dRow.ItemArray)
                        values.Add(value);
                    newval.Add(values);
                }
            }
            return Json(new { Items = newval, Items1 = Session["Where"] });
        }

        public ActionResult SearchIn(string value, string text, string LabelDescription)
        {
            Session["Text"] = text;
            Session["Where"] = value;
            Session["LabelDescription"] = LabelDescription;
            return Json(new { Items = value });
        }

        public ActionResult PagingSearch(int pagecount)
        {
            int pagefrom = (pagecount * 30);
            int pageto = pagefrom + 30;
            List<Hashtable> getvalue = new List<Hashtable>();
            List<ArrayList> newval = new List<ArrayList>();
            string[] SearchHeader = null;
            if (Session["searchlist"] != null)
            {
                DataSet ds1 = (DataSet)Session["searchlist"];
                if (ds1.Tables["list"].Rows.Count > 0)
                {
                    DataView dv = ds1.Tables["list"].DefaultView;
                    dv.RowFilter = "row > " + pagefrom + " and row <= " + pageto + "";
                    DataTable dt = dv.ToTable();
                    dt.Columns.Remove("row");
                    string head = Session["header"].ToString();
                    string[] headcombo = head.Split(',');
                    SearchHeader = new string[headcombo.Count()];
                    for (int i = 0; i < headcombo.Count(); i++)
                    {
                        SearchHeader[i] = headcombo[i].ToString();
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ArrayList values = new ArrayList();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string type = dt.Columns[j].DataType.ToString();
                            if (dt.Columns[j].DataType.ToString().ToUpper() == "SYSTEM.DATETIME")
                            {
                                string datetime = dt.Rows[i][j] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i][j]).ToShortDateString();
                                values.Add(datetime);
                            }
                            else
                            {
                                values.Add(dt.Rows[i][j].ToString());
                            }
                        }
                        newval.Add(values);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Hashtable hashtable = new Hashtable();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            hashtable[dt.Columns[j].ToString()] = dt.Rows[i][j].ToString();
                        }
                        getvalue.Add(hashtable);
                    }
                }
            }
            return Json(new { Items = newval, head = SearchHeader, Items1 = getvalue });
        }

        public ActionResult PagingSorting(string pagecnt, string Order, string BY, string TDIndex)
        {
            if (sqlcon != null)
                Sconn = new SqlConnection(sqlcon);

            string columnname = "";
            string fields = Session["Field"].ToString();
            string header = Session["header"].ToString();

            string[] fds = fields.Split(',');
            string[] hds = header.Split(',');
            if ((!string.IsNullOrEmpty(Order)) && (!string.IsNullOrEmpty(BY)) && (!string.IsNullOrEmpty(TDIndex)))
            {
                int index = Convert.ToInt32(TDIndex);
                if (hds[index] == Order)
                {
                    columnname = fds[index];
                }
            }

            string sql = Session["searchlistqry"].ToString();
            sql = sql.Replace("order by " + Session["Where"].ToString() + "", "order by " + (columnname + BY) + "");

            DataSet RDS = new DataSet();
            Ggrp.Getdataset(sql, "list", RDS, ref da, Sconn);

            Session["searchlist"] = RDS;

            int rowcount = 0;
            int pagecount = 0;

            int pagefrom = 0;
            int pageto = 0;
            int pgcnt = 0;
            if (!string.IsNullOrEmpty(pagecnt))
            {
                pgcnt = Convert.ToInt32(pagecnt);
                pagefrom = (pgcnt * 30);
                pageto = pagefrom + 30;
            }

            List<Hashtable> dsList = new List<Hashtable>();
            List<ArrayList> newval = new List<ArrayList>();
            string[] SearchHeader = null;
            if (Session["searchlist"] != null)
            {
                DataSet ds1 = (DataSet)Session["searchlist"];
                if (ds1.Tables["list"].Rows.Count > 0)
                {
                    if (ds1.Tables["list"].Columns.Contains("mfgno"))
                    {
                        List<Hashtable> mfgList = new List<Hashtable>();
                        string SQl = "select * from mfg";
                        Ggrp.Getdataset(SQl, "mfg", ds, ref da, Sconn);
                        mfgList = Ggrp.CovertDatasetToHashTbl(ds.Tables["mfg"]);
                        Session["ViewDatasetList"] = mfgList;

                    }
                    rowcount = ds1.Tables["list"].Rows.Count;
                    Session["searchlist"] = ds1;
                    pagecount = (ds1.Tables["list"].Rows.Count / 30);
                    DataView dv = ds1.Tables["list"].DefaultView;

                    if (pagefrom != 0 && pageto != 0)
                        dv.RowFilter = "row > " + pagefrom + " and row <= " + pageto + "";
                    else
                        dv.RowFilter = "row >= 1 and row <=30";
                    DataTable dt = dv.ToTable();
                    dt.Columns.Remove("row");
                    string head = "";
                    if (Session["JoinContition"] != null && Session["JoinContition"].ToString() != "")
                    {
                        string[] columnNames = (from dc in ds1.Tables["list"].Columns.Cast<DataColumn>()
                                                select dc.ColumnName).ToArray();
                        foreach (string str in columnNames)
                        {
                            if (str.ToUpper() != "ROW")
                                head += str + ",";
                        }
                        head = head.Substring(0, head.Length - 1);
                    }
                    else
                    {
                        head = Session["header"].ToString();
                    }
                    string[] headcombo = head.Split(',');
                    SearchHeader = new string[headcombo.Count()];
                    for (int i = 0; i < headcombo.Count(); i++)
                    {
                        SearchHeader[i] = headcombo[i].ToString();
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ArrayList values = new ArrayList();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string type = dt.Columns[j].DataType.ToString();
                            if (dt.Columns[j].DataType.ToString().ToUpper() == "SYSTEM.DATETIME")
                            {
                                string datetime = dt.Rows[i][j] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i][j]).ToShortDateString();
                                values.Add(datetime);
                            }
                            else
                            {
                                values.Add(dt.Rows[i][j].ToString());
                            }
                        }
                        newval.Add(values);
                    }
                    dsList = Ggrp.CovertDatasetToHashTbl(dt);
                }
            }
            var jsonResult2 = Json(new { Items = newval, hastable = dsList, rowcount = rowcount, head = SearchHeader, count = pagecount }, JsonRequestBehavior.AllowGet);
            jsonResult2.MaxJsonLength = int.MaxValue;
            return jsonResult2;
        }

        public ActionResult FillSearch(string IncludeAllStatus)
        {
            int rowcount = 0;
            int pagecount = 0;
            DataSet ds1 = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            List<ArrayList> newval = new List<ArrayList>();
            string[] SearchHeader = null;
            string MemberLocationStatus = "";
            List<Hashtable> dsList = new List<Hashtable>();
            if (sqlcon != null)
            {
                Sconn = new SqlConnection(sqlcon);
            }
            if (Session["Text"] != null)
            {
                string text = Session["Text"].ToString();
                if (text.Contains("'"))
                    text = text.Replace("'", "''");
                Session["Text"] = text;
            }
            if (Session["Table"] != null && Session["Field"] != null && Session["Text"] != null && Session["Where"] != null && Session["header"] != null)
            {
                string sql = "";
                if (Session["Condition"] != null && string.IsNullOrEmpty(Session["JoinContition"].ToString()) && Session["Text"].ToString() != "" && !(Session["Text"].ToString().Contains(',') || Session["Text"].ToString().Contains('|')))
                    sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from " + Session["Table"].ToString() + " where " + Session["Where"].ToString() + " like '" + Session["Text"].ToString() + "%' " + Session["Condition"].ToString() + " " + MemberLocationStatus + ") t ";
                else if (!string.IsNullOrEmpty(Session["JoinContition"].ToString()))
                {
                    sql = "select * from ( select ROW_NUMBER() over(order by c." + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from " + Session["Table"].ToString() + " " + Session["JoinContition"] + " where c." + Session["Where"].ToString() + " like '" + Session["Text"].ToString() + "%' " + Session["Condition"].ToString() + "" + MemberLocationStatus + ") t ";
                }
                else
                {
                    string[] KeyFields = null;
                    string lwhere = "";
                    bool IsCommaSearch = false;
                    string SearchSelectedValues = Session["Where"].ToString();
                    if (Session["Text"].ToString().Contains(',') || Session["Text"].ToString().Contains('|'))
                    {
                        Char[] Sep = { ',', '|' };
                        KeyFields = Session["Text"].ToString().ToString().Split(Sep);
                        if (Session["Table"].ToString().Trim().ToUpper() == "VW_SRCH_MM_ACCOUNT")
                        {
                            Ggrp.Getdataset("select distinct(field_name) as Field_Name,Display_FName as Text, orderby from op_searchselect where tablename ='" + Session["Table"].ToString() + "' and screen='GroGroup' and FormClass='Manufacturers' order by orderby ", "MFGsearch", ds1, ref da, Sconn);
                            if (ds1 != null && ds1.Tables["MFGsearch"] != null && ds1.Tables["MFGsearch"].Rows.Count > 0)
                            {
                                for (int R = 0; R < KeyFields.Length; R++)
                                {
                                    IsCommaSearch = false;
                                    if (Session["Text"].ToString().Contains(','))
                                    {
                                        for (int i = 0; i < ds1.Tables["MFGsearch"].Rows.Count; i++)
                                        {
                                            if (IsCommaSearch)
                                                continue;
                                            if (SearchSelectedValues.Trim().ToUpper() == ds1.Tables["MFGsearch"].Rows[i]["Field_Name"].ToString().Trim().ToUpper())
                                            {
                                                IsCommaSearch = true;
                                                int length = ds1.Tables["MFGsearch"].Rows.Count;
                                                if (length != i + 1)
                                                    SearchSelectedValues = ds1.Tables["MFGsearch"].Rows[i + 1]["Field_Name"].ToString();
                                                lwhere += ds1.Tables["MFGsearch"].Rows[i]["Field_Name"].ToString() + " like '" + KeyFields[R].ToString() + "%' and ";
                                            }
                                        }
                                    }
                                    else
                                        lwhere += ds1.Tables["MFGsearch"].Rows[R]["Field_Name"].ToString() + " like '" + KeyFields[R].ToString() + "%' or ";
                                }
                            }
                            else
                            {
                                lwhere = "1=0";
                            }
                        }
                        else
                        {
                            Ggrp.Getdataset("select distinct(field_name) as Field_Name,Display_FName as Text, orderby from op_searchselect where tablename ='" + Session["Table"].ToString() + "' order by orderby ", "MFGsearch", ds1, ref da, Sconn);
                            if (ds1 != null && ds1.Tables["MFGsearch"] != null && ds1.Tables["MFGsearch"].Rows.Count > 0)
                            {
                                for (int R = 0; R < KeyFields.Length; R++)
                                {
                                    IsCommaSearch = false;
                                    if (Session["Text"].ToString().Contains(','))
                                    {
                                        for (int i = 0; i < ds1.Tables["MFGsearch"].Rows.Count; i++)
                                        {
                                            if (IsCommaSearch)
                                                continue;
                                            if (SearchSelectedValues.Trim().ToUpper() == ds1.Tables["MFGsearch"].Rows[i]["Field_Name"].ToString().Trim().ToUpper())
                                            {
                                                IsCommaSearch = true;
                                                int length = ds1.Tables["MFGsearch"].Rows.Count;
                                                if (length != i + 1)
                                                    SearchSelectedValues = ds1.Tables["MFGsearch"].Rows[i + 1]["Field_Name"].ToString();
                                                lwhere += ds1.Tables["MFGsearch"].Rows[i]["Field_Name"].ToString() + " like '" + KeyFields[R].ToString() + "%' and ";
                                            }
                                        }
                                    }
                                    else
                                        lwhere += ds1.Tables["MFGsearch"].Rows[R]["Field_Name"].ToString() + " like '" + KeyFields[R].ToString() + "%' or ";
                                }
                            }
                            else
                            {
                                if (Session["LabelDescription"].ToString().ToUpper() == "MAIN MEMBER")
                                    lwhere = " lastname like '" + KeyFields[0] + "%' and firstname like '" + KeyFields[1] + "%'";
                                else
                                    lwhere = "1=0";
                            }
                        }
                        if (lwhere.EndsWith("and "))
                        {
                            lwhere = lwhere.Substring(0, lwhere.Length - 4);
                        }
                        if (lwhere.EndsWith("or "))
                        {
                            lwhere = lwhere.Substring(0, lwhere.Length - 3);
                        }
                        if (Session["Condition"] != null)
                            sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from " + Session["Table"].ToString() + " where " + lwhere + " " + Session["Condition"].ToString() + "" + MemberLocationStatus + ") t ";
                        else
                            sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from " + Session["Table"].ToString() + " where " + lwhere + "" + MemberLocationStatus + ") t ";
                    }
                    else if (Session["Text"] == null || Session["Text"].ToString() == "")
                    {
                        sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from " + Session["Table"].ToString() + " where  0=1) t ";
                    }
                    else
                    {
                        if (Session["Table"].ToString().ToUpper() == "PRODUCT,MFG")
                        {
                            sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from product p left join mfg m on  p.mfgno=m.mfgno  where " + Session["Where"].ToString() + " like '" + Session["Text"].ToString() + "%'" + MemberLocationStatus + ") t ";
                        }
                        else if (Session["Table"].ToString().ToUpper() == "OPCATCODE")
                        {
                            sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from op_lookup where lookupid='catcode' and  " + Session["Where"].ToString() + " like '" + Session["Text"].ToString() + "%'" + MemberLocationStatus + ") t ";
                        }
                        else
                        {
                            sql = "select * from ( select ROW_NUMBER() over(order by " + Session["Where"].ToString() + ") as row," + Session["Field"].ToString() + " from " + Session["Table"].ToString() + " where " + Session["Where"].ToString() + " like '" + Session["Text"].ToString() + "%'" + MemberLocationStatus + ") t ";
                        }
                    }
                }

                Ggrp.Getdataset(sql, "list", ds1, ref da, Sconn);
                Session["searchlistqry"] = sql;
                if (Session["Text"] == null || Session["Text"].ToString() == "")
                {
                    if (Session["CreateDatasetList"] != null && Session["CreateDatasetList"].ToString() != "")
                    {
                        List<Hashtable> DatasetList = (List<Hashtable>)Session["CreateDatasetList"];
                        for (int k = 0; k < DatasetList.Count; k++)
                        {
                            DataRow dr = ds1.Tables["list"].NewRow();
                            dr["row"] = k + 1;
                            dr["mfgno"] = DatasetList[k]["mfgno"].ToString();
                            dr["oldmfgno"] = DatasetList[k]["oldmfgno"].ToString();
                            dr["company"] = DatasetList[k]["company"].ToString();
                            dr["city"] = DatasetList[k]["city"].ToString();
                            dr["state"] = DatasetList[k]["state"].ToString();
                            dr["country"] = DatasetList[k]["country"].ToString();
                            dr["zip"] = DatasetList[k]["zip"].ToString();
                            dr["status"] = DatasetList[k]["status"].ToString();
                            dr["phone"] = DatasetList[k]["phone"].ToString();
                            dr["email"] = DatasetList[k]["email"].ToString();
                            ds1.Tables["list"].Rows.Add(dr);
                        }
                    }
                }
                else
                    Session["CreateDatasetList"] = "";
                if (ds1.Tables["list"].Rows.Count > 0)
                {
                    if (ds1.Tables["list"].Columns.Contains("mfgno"))
                    {
                        List<Hashtable> mfgList = new List<Hashtable>();
                        string SQl = "select * from mfg";
                        Ggrp.Getdataset(SQl, "mfg", ds, ref da, Sconn);
                        mfgList = Ggrp.CovertDatasetToHashTbl(ds.Tables["mfg"]);
                        Session["ViewDatasetList"] = mfgList;
                    }
                    rowcount = ds1.Tables["list"].Rows.Count;
                    Session["searchlist"] = ds1;
                    pagecount = (ds1.Tables["list"].Rows.Count / 30);
                    DataView dv = ds1.Tables["list"].DefaultView;
                    dv.RowFilter = "row >= 1 and row <=30";
                    DataTable dt = dv.ToTable();
                    dt.Columns.Remove("row");
                    string head = "";
                    if (Session["JoinContition"] != null && Session["JoinContition"].ToString() != "")
                    {
                        string[] columnNames = (from dc in ds1.Tables["list"].Columns.Cast<DataColumn>()
                                                select dc.ColumnName).ToArray();
                        foreach (string str in columnNames)
                        {
                            if (str.ToUpper() != "ROW")
                                head += str + ",";
                        }
                        head = head.Substring(0, head.Length - 1);
                    }
                    else
                    {
                        head = Session["header"].ToString();
                    }
                    string[] headcombo = head.Split(',');
                    SearchHeader = new string[headcombo.Count()];
                    for (int i = 0; i < headcombo.Count(); i++)
                    {
                        SearchHeader[i] = headcombo[i].ToString();
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ArrayList values = new ArrayList();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string type = dt.Columns[j].DataType.ToString();
                            if (dt.Columns[j].DataType.ToString().ToUpper() == "SYSTEM.DATETIME")
                            {
                                string datetime = dt.Rows[i][j] == DBNull.Value ? "" : Convert.ToDateTime(dt.Rows[i][j]).ToShortDateString();
                                values.Add(datetime);
                            }
                            else
                            {
                                values.Add(dt.Rows[i][j].ToString());
                            }
                        }
                        newval.Add(values);
                    }
                    dsList = Ggrp.CovertDatasetToHashTbl(dt);
                }
                if (Session["Text"] != null)
                {
                    string text = Session["Text"].ToString();
                    if (text.Contains("'"))
                    {
                        text = text.Replace("''", "'");
                        Session["Text"] = text;
                    }
                }
                return Json(new { Items = newval, hastable = dsList, rowcount = rowcount, head = SearchHeader, count = pagecount, where = Session["Where"].ToString(), text = Session["Text"].ToString(), table = Session["Table"].ToString(), element = Session["Element"].ToString() });
            }
            return Json(new { Items = newval, hastable = dsList, rowcount = rowcount, head = SearchHeader, count = pagecount, where = "", text = "", table = "" });
        }

        #endregion

        #region MfgContacts

        public ActionResult Contacts()
        {
            return View();
        }
        public ActionResult FillMfgContGrid(string mfgNo, string contactno = "")
        {
            string sql = ""; var rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = "select * from mfgContact mc left join op_lookup op on op.lookupid = 'titlecd' and mc.ntitlecd = op.entry where mfgno = '" + mfgNo + "' order by nseqcd,nlast,nfirst";
            Ggrp.Getdataset(sql, "FillMfgCont", ds, ref da, Sconn);
            List<Hashtable> MfgContList = new List<Hashtable>();
            MfgContList = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgCont"]);
            if (!string.IsNullOrEmpty(contactno) && ds.Tables["FillMfgCont"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["FillMfgCont"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["FillMfgCont"].Select().ToList()
                                   where dr["contactno"].ToString().Trim() == contactno
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = MfgContList, rowindex = rowindex });
        }
        public ActionResult ValidateManufactFirstLastName(string FirstName, string LastName, string mfgcontno, bool AddMode, string mfgNo, string titlecd)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (FirstName.Contains("'"))
            {
                FirstName = FirstName.Replace("'", "''");
            }
            if (LastName.Contains("'"))
            {
                LastName = LastName.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select nfirst,nlast from mfgContact where nfirst='" + FirstName + "' and nlast='" + LastName + "' and mfgno='" + mfgNo + "' and ntitlecd='" + titlecd + "'";
            else
                lSql = " select nfirst,nlast from   mfgContact where nfirst='" + FirstName + "' and nlast='" + LastName + "' and mfgno='" + mfgNo + " ' and mfgcontno != '" + mfgcontno + "' and ntitlecd='" + titlecd + "'";

            Ggrp.Getdataset(lSql, "GetManufactureName", ds, ref da, Sconn);
            if (ds.Tables["GetManufactureName"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }
        public ActionResult SaveMfgContacts(string ObjMfgContactArray = "", string mfgno = "", bool IsAddMode = false)
        {
            string sSQL = ""; int Contactno = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjMfgContactArray = "[" + ObjMfgContactArray + "]";
            List<Hashtable> ContactsList = jss.Deserialize<List<Hashtable>>(ObjMfgContactArray);
            DataSet Cds = new DataSet();
            SqlDataAdapter Cda = new SqlDataAdapter();
            DataRow drAdd;
            if (ContactsList.Count > 0)
            {
                try
                {
                    Cds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from mfgContact where 1=0";
                        string sql = "select max(isnull(contactno,0))+1 as ContactNo from mfgcontact where mfgno='" + mfgno + "'";
                        Ggrp.Getdataset(sql, "contact", ds, ref da, Sconn);
                        if (ds != null && ds.Tables["contact"].Rows.Count > 0)
                        {
                            if (ds.Tables["contact"].Rows[0]["contactno"].ToString() == "")
                                Contactno = 1;
                            else
                                Contactno = Convert.ToInt32(ds.Tables["contact"].Rows[0]["contactno"].ToString());
                        }
                    }
                    else
                    {
                        sSQL = "select * from mfgContact where contactno='" + ContactsList[0]["contactno"].ToString() + "' and mfgno='" + ContactsList[0]["mfgno"].ToString() + "'";
                    }
                    Ggrp.Getdataset(sSQL, "tblmfgContact", Cds, ref Cda, Sconn);
                    if (IsAddMode == true)
                    {
                        drAdd = Cds.Tables["tblmfgContact"].NewRow();
                        drAdd["contactno"] = Contactno;
                    }
                    else
                    {
                        drAdd = Cds.Tables["tblmfgContact"].Rows[0];
                    }
                    drAdd["mfgno"] = ContactsList[0]["mfgno"].ToString();
                    drAdd["nsalut"] = ContactsList[0]["nsalut"] != null ? ContactsList[0]["nsalut"].ToString() : "";
                    drAdd["nfirst"] = ContactsList[0]["nfirst"].ToString();
                    drAdd["minitial"] = ContactsList[0]["minitial"].ToString();
                    drAdd["nlast"] = ContactsList[0]["nlast"].ToString();
                    drAdd["ngreet"] = ContactsList[0]["ngreet"].ToString();
                    drAdd["ntitlecd"] = ContactsList[0]["ntitlecd"] != null ? ContactsList[0]["ntitlecd"].ToString() : "";
                    drAdd["ntitle"] = ContactsList[0]["ntitle"] != null ? ContactsList[0]["ntitle"].ToString() : "";
                    drAdd["nseqcd"] = ContactsList[0]["nseqcd"].ToString();
                    drAdd["naddress"] = ContactsList[0]["naddress"].ToString();
                    drAdd["nstate"] = ContactsList[0]["nstate"].ToString();
                    drAdd["nphone"] = ContactsList[0]["nphone"].ToString();
                    drAdd["n_ext"] = ContactsList[0]["n_ext"].ToString();
                    drAdd["ncity"] = ContactsList[0]["ncity"].ToString();
                    drAdd["nphone2"] = ContactsList[0]["nphone2"].ToString();
                    drAdd["eextn"] = ContactsList[0]["eextn"].ToString();
                    drAdd["nstate"] = ContactsList[0]["nstate"].ToString();
                    drAdd["nzip"] = ContactsList[0]["nzip"].ToString();
                    drAdd["country"] = ContactsList[0]["country"] != null ? ContactsList[0]["country"].ToString() : "";
                    drAdd["nfax"] = ContactsList[0]["nfax"].ToString();
                    drAdd["cell"] = ContactsList[0]["cell"].ToString();
                    drAdd["newsltr"] = ContactsList[0]["newsltr"].ToString() == "True" ? "Y" : "";
                    drAdd["nemail"] = ContactsList[0]["nemail"].ToString();
                    if (IsAddMode == true)
                    {
                        Cds.Tables["tblmfgContact"].Rows.Add(drAdd);
                    }
                    else
                    {
                        Cds.Tables["tblmfgContact"].GetChanges();
                    }
                    Cda.Update(Cds, "tblmfgContact");
                    if (IsAddMode == true)
                    {
                        // Contactno = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        Contactno = Convert.ToInt32(ContactsList[0]["contactno"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = Contactno });
        }
        public ActionResult DeleteMfgContacts(string contactno, string mfgno)
        {
            bool Roomlistverify = false;
            Sconn = new SqlConnection(sqlcon);
            string lsql = "select * from roomlist where mfgno='" + mfgno + "' and mfgcontactno='" + contactno + "'";
            Ggrp.Getdataset(lsql, "TblRoomlist", ds, ref da, Sconn);
            if (ds != null && ds.Tables["TblRoomlist"].Rows.Count > 0)
            {
                Roomlistverify = true;
            }
            else
            {
                string SQl = "delete from mfgContact where  contactno ='" + contactno + "' and mfgno='" + mfgno + "'";
                Ggrp.Execute(SQl, Sconn);
            }
            return Json(new { Items = "", checkroomlist = Roomlistverify });
        }

        #endregion MfgContacts

        #region SystemDefaults

        public ActionResult SystemDefaults()
        {
            return View();
        }

        public ActionResult FillSystemDefaults()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);

            sql = "select  * from OP_system";
            Ggrp.Getdataset(sql, "system", ds, ref da, Sconn);
            List<Hashtable> systemdefault = new List<Hashtable>();
            systemdefault = Ggrp.CovertDatasetToHashTbl(ds.Tables["system"]);

            return Json(new { Items = systemdefault });
        }

        public ActionResult FillSystemDefaultsRoomlistinfo()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);

            sql = " select  meetingcode,Startdate,DATEADD(day, 1,Startdate) as depart,datediff(day, Startdate, DATEADD(day, 1,Startdate)) as nights from OP_system";
            Ggrp.Getdataset(sql, "system", ds, ref da, Sconn);
            List<Hashtable> systemdefault = new List<Hashtable>();
            systemdefault = Ggrp.CovertDatasetToHashTbl(ds.Tables["system"]);

            return Json(new { Items = systemdefault });
        }

        public ActionResult SavesystemDefaults(string objsystemDefaults)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            objsystemDefaults = "[" + objsystemDefaults + "]";
            List<Hashtable> savesystems = jss.Deserialize<List<Hashtable>>(objsystemDefaults);
            SqlDataAdapter sda = new SqlDataAdapter();
            DataSet systemds = new DataSet();
            DataRow sdrAdd;

            if (savesystems.Count > 0)
            {
                systemds = new DataSet();
                try
                {
                    sql = "select * from op_system where company='" + savesystems[0]["company"].ToString() + "'";
                    sda = new SqlDataAdapter(sql, Sconn);
                    Ggrp.Getdataset(sql, "systemtbl", systemds, ref sda, Sconn);
                    if (systemds != null && systemds.Tables["systemtbl"].Rows.Count == 0)
                    {
                        sql = "select * from op_system where 1=0";
                        Ggrp.Getdataset(sql, "systemtbl", systemds, ref sda, Sconn);
                        sdrAdd = systemds.Tables["systemtbl"].NewRow();
                    }
                    else
                    {
                        sdrAdd = systemds.Tables["systemtbl"].Rows[0];
                    }
                    if (savesystems[0]["systemtbl"] != null && savesystems[0]["systemtbl"].ToString() != "")
                        sdrAdd["company"] = savesystems[0]["company"].ToString();
                    sdrAdd["address1"] = savesystems[0]["address1"].ToString();
                    sdrAdd["address2"] = savesystems[0]["address2"].ToString();
                    sdrAdd["city"] = savesystems[0]["city"].ToString();
                    sdrAdd["state"] = savesystems[0]["state"].ToString();
                    sdrAdd["zip"] = savesystems[0]["zip"].ToString();
                    sdrAdd["curr_dt"] = savesystems[0]["curr_dt"].ToString();
                    sdrAdd["prg_end_dt"] = savesystems[0]["prg_end_dt"].ToString();
                    sdrAdd["fy_end_dt"] = savesystems[0]["fy_end_dt"].ToString();
                    sdrAdd["rrty"] = savesystems[0]["rrty"].ToString();
                    sdrAdd["rrly"] = savesystems[0]["rrly"].ToString();
                    sdrAdd["rrpy"] = savesystems[0]["rrpy"].ToString();
                    sdrAdd["rrbatch"] = savesystems[0]["rrbatch"].ToString();
                    sdrAdd["ym1"] = savesystems[0]["ym1"].ToString();
                    sdrAdd["ym2"] = savesystems[0]["ym2"].ToString();
                    sdrAdd["ProgramYear"] = savesystems[0]["pgmyr"].ToString();
                    sdrAdd["Meetingcode"] = savesystems[0]["Meetingcode"].ToString();
                    if (savesystems[0]["Startdate"] != null && savesystems[0]["Startdate"].ToString() != "")
                        sdrAdd["Startdate"] = savesystems[0]["Startdate"].ToString();
                    else
                        sdrAdd["Startdate"] = DBNull.Value;
                    sdrAdd["Day1"] = string.IsNullOrEmpty(savesystems[0]["Day1"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day1"].ToString());
                    sdrAdd["Day2"] = string.IsNullOrEmpty(savesystems[0]["Day2"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day2"].ToString());
                    sdrAdd["Day3"] = string.IsNullOrEmpty(savesystems[0]["Day3"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day3"].ToString());
                    sdrAdd["Day4"] = string.IsNullOrEmpty(savesystems[0]["Day4"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day4"].ToString());
                    sdrAdd["Day5"] = string.IsNullOrEmpty(savesystems[0]["Day5"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day5"].ToString());
                    sdrAdd["Day6"] = string.IsNullOrEmpty(savesystems[0]["Day6"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day6"].ToString());
                    sdrAdd["Day7"] = string.IsNullOrEmpty(savesystems[0]["Day7"].ToString()) ? DBNull.Value : (object)Convert.ToInt32(savesystems[0]["Day7"].ToString());
                    sdrAdd["AllowedIPs"] = savesystems[0]["AllowedIPs"] == null || savesystems[0]["AllowedIPs"].ToString() == "" ? DBNull.Value : savesystems[0]["AllowedIPs"];

                    if (systemds != null && systemds.Tables["systemtbl"].Rows.Count == 0)
                    {
                        systemds.Tables["systemtbl"].Rows.Add(sdrAdd);
                    }
                    else
                    {
                        systemds.Tables["systemtbl"].GetChanges();
                    }
                    sda.Update(systemds, "systemtbl");
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { ItemMsg = "" });
        }

        #endregion

        #region Notes
        public ActionResult Notes(string No, string ScreenName)
        {
            ViewBag.Initials = Session["initials"].ToString();
            ViewBag.No = No;
            ViewBag.ScreenName = ScreenName;
            return View();
        }

        public ActionResult FillMfgNotesGrid(string No, string ScreenName, string noteid)
        {
            string sql = "";
            int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            if (ScreenName == "Manufacturer")
            {
                sql = "select n.*,op.descript as ntypedesc from notes n left join op_lookup op on op.lookupid = 'NTYPE' and n.ntype = op.entry where n.mfgno = '" + No + "' order by n.ndate desc";
            }
            if (ScreenName == "Distributor")
            {
                sql = "select n.*,op.descript as ntypedesc from notes n left join op_lookup op on op.lookupid = 'NTYPE' and n.ntype = op.entry where n.DistNo = '" + No + "' order by n.ndate desc";
            }
            if (ScreenName == "Retailer")
            {
                sql = "select n.*,op.descript as ntypedesc from notes n left join op_lookup op on op.lookupid = 'NTYPE' and n.ntype = op.entry where n.RetailerNo = '" + No + "' order by n.ndate desc";
            }
            Ggrp.Getdataset(sql, "FillMfgNotes", ds, ref da, Sconn);
            List<Hashtable> MfgNotesList = new List<Hashtable>();
            MfgNotesList = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgNotes"]);

            if (ds.Tables["FillMfgNotes"].Rows.Count > 0 && !string.IsNullOrEmpty(noteid))
            {
                var rowlist = ds.Tables["FillMfgNotes"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["FillMfgNotes"].Select().ToList()
                                   where dr["noteid"].ToString() == noteid
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = MfgNotesList, rowindex = rowindex });
        }

        public ActionResult GetContactList(string No, string ScreenName)
        {
            Sconn = new SqlConnection(sqlcon);
            string sql = "";

            if (ScreenName == "Manufacturer")
                sql = "select '' as contactno,'' as contactname union select contactno,isnull(nfirst,'') + ' ' + isnull(nlast,'') as contactname from mfgcontact where mfgno='" + No + "'";
            if (ScreenName == "Distributor")
                sql = "select '' as contactno,'' as contactname union select contactno,isnull(nfirst,'') + ' ' + isnull(nlast,'') as contactname from DistributorContact where DistNo='" + No + "'";
            if (ScreenName == "Retailer")
                sql = "select '' as contactno,'' as contactname union select contactno,isnull(firstname,'') + ' ' + isnull(lastname,'') as contactname from Retailercontact where RetailerNo='" + No + "'";
            Ggrp.Getdataset(sql, "contact", ds, ref da, Sconn);
            List<Hashtable> ContactList = new List<Hashtable>();
            ContactList = Ggrp.CovertDatasetToHashTbl(ds.Tables["contact"]);
            return Json(new { Items = ContactList });
        }
        public ActionResult GetEmployeeUserList()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "select '' as initials,'' as name union select initials,name from op_employee order by name";
            Ggrp.Getdataset(SQl, "empuser", ds, ref da, Sconn);
            List<Hashtable> EmpusrList = new List<Hashtable>();
            EmpusrList = Ggrp.CovertDatasetToHashTbl(ds.Tables["empuser"]);
            return Json(new { Items = EmpusrList });
        }

        public ActionResult SaveNotes(string ObjNotesList = "", bool IsAddMode = false, string mfgno = "")
        {
            string Sql = ""; int Newnote = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjNotesList = "[" + ObjNotesList + "]";
            List<Hashtable> AddNotes = jss.Deserialize<List<Hashtable>>(ObjNotesList);
            DataSet mds = new DataSet();
            SqlDataAdapter mda = new SqlDataAdapter();
            DataRow drAdd;

            if (AddNotes.Count > 0)
            {
                try
                {
                    mds = new DataSet();
                    if (IsAddMode == true)
                    {
                        Sql = "select * from notes where 1=0";
                    }
                    else
                    {
                        Sql = "select * from notes where NoteId='" + AddNotes[0]["NoteId"].ToString() + "'";
                    }
                    mda = new SqlDataAdapter(Sql, Sconn);
                    Ggrp.Getdataset(Sql, "tblnotes", mds, ref mda, Sconn, false);
                    if (IsAddMode == true)
                    {
                        drAdd = mds.Tables["tblnotes"].NewRow();
                    }
                    else
                    {
                        drAdd = mds.Tables["tblnotes"].Rows[0];
                    }
                    if (AddNotes[0]["mfgno"] != null && AddNotes[0]["mfgno"].ToString() != "")
                        drAdd["mfgno"] = AddNotes[0]["mfgno"].ToString();
                    if (AddNotes[0]["DistNo"] != null && AddNotes[0]["DistNo"].ToString() != "")
                        drAdd["DistNo"] = AddNotes[0]["DistNo"].ToString();
                    if (AddNotes[0]["RetailerNo"] != null && AddNotes[0]["RetailerNo"].ToString() != "")
                        drAdd["RetailerNo"] = AddNotes[0]["RetailerNo"].ToString();
                    drAdd["fileid"] = AddNotes[0]["field"].ToString();
                    drAdd["ndate"] = AddNotes[0]["ndate"].ToString();
                    drAdd["ntype"] = AddNotes[0]["ntype"].ToString();
                    drAdd["nuser"] = AddNotes[0]["nuser"].ToString();
                    drAdd["notes"] = (AddNotes[0]["notes"].ToString()).Replace("|", "<").Replace("~", ">");
                    drAdd["duedate"] = AddNotes[0]["duedate"] == null || AddNotes[0]["duedate"].ToString() == "" ? DBNull.Value : AddNotes[0]["duedate"];
                    drAdd["for_"] = AddNotes[0]["for_"] == null ? "" : AddNotes[0]["for_"].ToString();
                    drAdd["completed"] = Convert.ToBoolean(AddNotes[0]["completed"]);
                    drAdd["datecompl"] = AddNotes[0]["datecompl"] == null || AddNotes[0]["datecompl"].ToString() == "" ? DBNull.Value : AddNotes[0]["datecompl"];
                    if (IsAddMode == true)
                    {
                        mds.Tables["tblnotes"].Rows.Add(drAdd);
                    }
                    else
                    {
                        mds.Tables["tblnotes"].GetChanges();
                    }
                    mda.Update(mds, "tblnotes");
                    if (IsAddMode == true)
                    {
                        Newnote = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        Newnote = Convert.ToInt32(AddNotes[0]["NoteId"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { Items = Newnote });
        }


        public ActionResult DeleteNotes(string noteid)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from notes where  NoteId ='" + noteid + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }

        #endregion

        #region Benefits

        public ActionResult Benefits()
        {
            return View();
        }

        public ActionResult FillMfgBenefitsGrid(string mfgNo, string prgno = "")
        {
            string sql = ""; int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = " select * from BenefitProgram where mfgno='" + mfgNo + "' order by progfrom desc,program asc";
            Ggrp.Getdataset(sql, "FillMfgBenefitsGrid", ds, ref da, Sconn);
            List<Hashtable> FillMfgBenefitsGrid = new List<Hashtable>();
            FillMfgBenefitsGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgBenefitsGrid"]);
            if (!string.IsNullOrEmpty(prgno) && ds.Tables["FillMfgBenefitsGrid"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["FillMfgBenefitsGrid"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["FillMfgBenefitsGrid"].Select().ToList()
                                   where dr["ProgNo"].ToString().Trim() == prgno
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = FillMfgBenefitsGrid, rowindex = rowindex });
        }
        public ActionResult SaveBenefitPrg(string ObjBenefit = "", bool IsAddMode = false)
        {
            Sconn = new SqlConnection(sqlcon); int prgno = 0;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjBenefit = "[" + ObjBenefit + "]";
            List<Hashtable> AddBenefit = jss.Deserialize<List<Hashtable>>(ObjBenefit);
            DataRow BenefitAdd;
            if (AddBenefit.Count > 0)
            {
                try
                {
                    string sql = "";
                    DataSet dsBenefit = new DataSet();
                    SqlDataAdapter daBenefit = new SqlDataAdapter();
                    sql = "Select * from benefitprogram where ProgNo='" + AddBenefit[0]["ProgNo"].ToString() + "'";
                    Ggrp.Getdataset(sql, "benefit", dsBenefit, ref daBenefit, Sconn, false);
                    if (dsBenefit != null && dsBenefit.Tables["benefit"].Rows.Count == 0)
                    {
                        sql = "Select * from benefitprogram where 1=0";
                        Ggrp.Getdataset(sql, "benefit", dsBenefit, ref daBenefit, Sconn, false);
                        BenefitAdd = dsBenefit.Tables["benefit"].NewRow();
                    }
                    else
                    {
                        BenefitAdd = dsBenefit.Tables["benefit"].Rows[0];
                    }
                    BenefitAdd["mfgno"] = AddBenefit[0]["mfgNo"].ToString();
                    BenefitAdd["progcntl"] = AddBenefit[0]["progcntl"] == null ? "" : AddBenefit[0]["progcntl"].ToString();
                    BenefitAdd["period"] = AddBenefit[0]["period"] == null ? "" : AddBenefit[0]["period"].ToString();
                    BenefitAdd["program"] = AddBenefit[0]["program"] == null ? "" : AddBenefit[0]["program"].ToString();
                    BenefitAdd["percent"] = AddBenefit[0]["percent"] == null || AddBenefit[0]["percent"].ToString() == "" ? DBNull.Value : AddBenefit[0]["percent"];
                    BenefitAdd["progfrom"] = AddBenefit[0]["progfrom"] == null || AddBenefit[0]["progfrom"].ToString() == "" ? DBNull.Value : AddBenefit[0]["progfrom"];
                    BenefitAdd["progto"] = AddBenefit[0]["progto"] == null || AddBenefit[0]["progto"].ToString() == "" ? DBNull.Value : AddBenefit[0]["progto"];
                    BenefitAdd["pur_xcld"] = AddBenefit[0]["pur_xcld"] == null || AddBenefit[0]["pur_xcld"].ToString() == "" ? DBNull.Value : AddBenefit[0]["pur_xcld"];
                    BenefitAdd["plvl_fro"] = AddBenefit[0]["plvl_fro"] == null || AddBenefit[0]["plvl_fro"].ToString() == "" ? DBNull.Value : AddBenefit[0]["plvl_fro"];
                    BenefitAdd["plvl_to"] = AddBenefit[0]["plvl_to"] == null || AddBenefit[0]["plvl_to"].ToString() == "" ? DBNull.Value : AddBenefit[0]["plvl_to"];
                    BenefitAdd["comments"] = AddBenefit[0]["comments"] == null || AddBenefit[0]["comments"].ToString() == "" ? DBNull.Value : AddBenefit[0]["comments"];
                    if (dsBenefit != null && dsBenefit.Tables["benefit"].Rows.Count == 0)
                    {
                        dsBenefit.Tables["benefit"].Rows.Add(BenefitAdd);
                    }
                    else
                    {
                        dsBenefit.Tables["benefit"].GetChanges();
                    }
                    daBenefit.Update(dsBenefit, "benefit");
                    if (IsAddMode == true)
                    {
                        prgno = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        prgno = Convert.ToInt32(AddBenefit[0]["ProgNo"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { Items = ex.ToString() });

                }
            }
            return Json(new { ItemMsg = prgno });
        }


        public ActionResult FillMfgPaymentGrid(string ProgNo, string chkno = "")
        {
            string sql = ""; int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = "select * from Benefitcheck where ProgNo='" + ProgNo + "' order by checknum";
            Ggrp.Getdataset(sql, "FillMfgPaymentGrid", ds, ref da, Sconn);
            List<Hashtable> FillMfgPaymentGrid = new List<Hashtable>();
            FillMfgPaymentGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgPaymentGrid"]);
            if (!string.IsNullOrEmpty(chkno) && ds.Tables["FillMfgPaymentGrid"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["FillMfgPaymentGrid"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["FillMfgPaymentGrid"].Select().ToList()
                                   where dr["checknum"].ToString().Trim() == chkno
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = FillMfgPaymentGrid, rowindex = rowindex });
        }

        public ActionResult ValidateProgPaymentCheck(string progNo, string checknum, bool AddMode) 
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            
            if (AddMode == true)
                lSql = "select checknum from benefitcheck where progNo='" + progNo + "' and checknum='" + checknum + "'";

            Ggrp.Getdataset(lSql, "tblPayCheck", ds, ref da, Sconn);
            if (ds.Tables["tblPayCheck"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult SaveBenefitPay(string ObjBenefit = "", bool IsAddMode = false)
        {
            Sconn = new SqlConnection(sqlcon);
            string chkno = "";
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjBenefit = "[" + ObjBenefit + "]";
            List<Hashtable> AddBenefit = jss.Deserialize<List<Hashtable>>(ObjBenefit);
            DataRow BenefitAdd;
            if (AddBenefit.Count > 0)
            {
                try
                {
                    string sql = "";
                    DataSet dsBenefit = new DataSet();
                    SqlDataAdapter daBenefit = new SqlDataAdapter();
                    sql = "Select * from benefitcheck where progno='" + AddBenefit[0]["ProgNo"].ToString() + "' and checknum='" + AddBenefit[0]["checknum"].ToString() + "'";
                    Ggrp.Getdataset(sql, "benefitcheck", dsBenefit, ref daBenefit, Sconn); //ely
                    if (dsBenefit != null && dsBenefit.Tables["benefitcheck"].Rows.Count == 0)
                    {
                        sql = "Select * from benefitcheck where 1=0";
                        Ggrp.Getdataset(sql, "benefitcheck", dsBenefit, ref daBenefit, Sconn); //ely
                        BenefitAdd = dsBenefit.Tables["benefitcheck"].NewRow();
                    }
                    else
                    {
                        BenefitAdd = dsBenefit.Tables["benefitcheck"].Rows[0];
                    }
                    BenefitAdd["ProgNo"] = AddBenefit[0]["ProgNo"] == null ? "" : AddBenefit[0]["ProgNo"].ToString();
                    BenefitAdd["checknum"] = AddBenefit[0]["checknum"] == null ? "" : AddBenefit[0]["checknum"].ToString();
                    if (AddBenefit[0]["chkfrom"] != null && AddBenefit[0]["chkfrom"].ToString() != "")
                        BenefitAdd["chkfrom"] = AddBenefit[0]["chkfrom"].ToString();
                    if (AddBenefit[0]["chkto"] != null && AddBenefit[0]["chkto"].ToString() != "")
                        BenefitAdd["chkto"] = AddBenefit[0]["chkto"].ToString();
                    BenefitAdd["holdpay"] = AddBenefit[0]["holdpay"] == null || AddBenefit[0]["holdpay"].ToString() == "" ? DBNull.Value : AddBenefit[0]["holdpay"];
                    BenefitAdd["hqper"] = AddBenefit[0]["hqper"] == null || AddBenefit[0]["hqper"].ToString() == "" ? DBNull.Value : AddBenefit[0]["hqper"];
                    BenefitAdd["recvdon"] = AddBenefit[0]["recvdon"] == null || AddBenefit[0]["recvdon"].ToString() == "" ? DBNull.Value : AddBenefit[0]["recvdon"];
                    BenefitAdd["amount"] = AddBenefit[0]["amount"] == null || AddBenefit[0]["amount"].ToString() == "" ? DBNull.Value : AddBenefit[0]["amount"];
                    BenefitAdd["wsrecvd"] = AddBenefit[0]["wsrecvd"] == null || AddBenefit[0]["wsrecvd"].ToString() == "" ? DBNull.Value : AddBenefit[0]["wsrecvd"];
                    BenefitAdd["dueon"] = AddBenefit[0]["dueon"] == null || AddBenefit[0]["dueon"].ToString() == "" ? DBNull.Value : AddBenefit[0]["dueon"];

                    if (dsBenefit != null && dsBenefit.Tables["benefitcheck"].Rows.Count == 0)
                    {
                        dsBenefit.Tables["benefitcheck"].Rows.Add(BenefitAdd);
                    }
                    else
                    {
                        dsBenefit.Tables["benefitcheck"].GetChanges();
                    }
                    daBenefit.Update(dsBenefit, "benefitcheck");
                    chkno = AddBenefit[0]["checknum"].ToString();
                }
                catch (Exception ex)
                {
                    return Json(new { Items = ex.ToString() });

                }
            }
            return Json(new { ItemMsg = chkno });
        }

        
        public ActionResult FillNGPNotes(string mfgNo)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = " select company,NGPNotes from mfg where mfgNo='" + mfgNo + "'";
            Ggrp.Getdataset(sql, "FillNGPNotes", ds, ref da, Sconn);
            List<Hashtable> FillMfgProgInfoGrid = new List<Hashtable>();
            FillMfgProgInfoGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillNGPNotes"]);

            return Json(new { Items = FillMfgProgInfoGrid });
        }

        public ActionResult NGPSaveNotes(string ngpnotes, string MfgNo)
        {
            string Sql = ""; int NewMfno = 0;
            Sconn = new SqlConnection(sqlcon);
            DataSet mds = new DataSet();
            SqlDataAdapter mda = new SqlDataAdapter();
            DataRow drAdd;

            try
            {
                Sql = "select * from mfg where MfgNo='" + MfgNo + "'";
                mda = new SqlDataAdapter(Sql, Sconn);
                Ggrp.Getdataset(Sql, "tblngpnotes", mds, ref mda, Sconn);
                List<Hashtable> FillNGPNoteslist = new List<Hashtable>();
                FillNGPNoteslist = Ggrp.CovertDatasetToHashTbl(mds.Tables["tblngpnotes"]);
                drAdd = mds.Tables["tblngpnotes"].Rows[0];
                drAdd["NGPNotes"] = ngpnotes;
                mds.Tables["tblngpnotes"].GetChanges();

                mda.Update(mds, "tblngpnotes");
            }
            catch (Exception ex)
            {
                return Json(new { ItemMsg = ex.ToString() });

            }

            return Json(new { Items = NewMfno });
        }

        public ActionResult BenefitRecon(string ProgNo = "", string ChkNo = "", string Percent = "", string Hqper = "", string Company = "", string Chkfrom = "", string Chkto = "")
        {
            ViewBag.ProgNo = ProgNo;
            ViewBag.ChkNo = ChkNo;
            ViewBag.Percent = Percent;
            ViewBag.Hqper = Hqper;
            ViewBag.Company = Company;
            ViewBag.Chkfrom = Chkfrom;
            ViewBag.Chkto = Chkto;
            return View();
        }

        public ActionResult FillBenefitDistDataGrid(string ProgNo, string ChkNo)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "STP_BeneDistGetData '" + ProgNo + "','" + ChkNo + "'";
            Ggrp.Getdataset(sql, "tblBeneDist", ds, ref da, Sconn);

            if (ds.Tables["tblBeneDist"] != null && ds.Tables["tblBeneDist"].Rows.Count == 0)
            {
                sql = "select top 1 company,DistNo from distributor where isnull(ddtlink,'')!=''  order by 1";
                Ggrp.Getdataset(sql, "tblDistNo", ds, ref da, Sconn);

                if (ds.Tables["tblDistNo"] != null && ds.Tables["tblDistNo"].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables["tblBeneDist"].NewRow();
                    dr["company"] = ds.Tables["tblDistNo"].Rows[0]["company"].ToString();
                    dr["distno"] = ds.Tables["tblDistNo"].Rows[0]["DistNo"].ToString();
                    dr["purchase"] = 0.00; dr["adjustment"] = 0.00;
                    dr["pspurchase"] = 0; dr["diff"] = 0; dr["DPercent"] = 0; dr["Benefit"] = 0;
                    dr["HQFee"] = 0; dr["NetPay"] = 0; dr["Discrepency"] = "False";
                    ds.Tables["tblBeneDist"].Rows.Add(dr);
                }

            }

            List<Hashtable> BeneDistList = new List<Hashtable>();
            BeneDistList = Ggrp.CovertDatasetToHashTbl(ds.Tables["tblBeneDist"]);
            return Json(new { Items = BeneDistList });
        }

        public ActionResult SaveBenefitRecon(string ObjBenefit = "", string Progno = "", string Chkno = "", string Remarks = "")
        {
            string ErrMsg = "";
            try
            {
                Sconn = new SqlConnection(sqlcon);
                DataTable dttemp = new DataTable();
                DataSet lds = new DataSet();
                if (!string.IsNullOrEmpty(ObjBenefit) && ObjBenefit != "\"\"")
                    dttemp = (DataTable)JsonConvert.DeserializeObject(ObjBenefit, (typeof(DataTable)));
                DataTable dt = new DataTable();
                dt.TableName = "Benefitdist";
                dt.Columns.Add("ProgNo");
                dt.Columns.Add("checknum");
                dt.Columns.Add("distNo");
                dt.Columns.Add("purchase");
                dt.Columns.Add("adjustment");
                dt.Columns.Add("PSpurchase");
                dt.Columns.Add("Discrepency");
                dt.Columns.Add("Benefit");
                dt.Columns.Add("HQFee");

                for (int i = 0; i < dttemp.Rows.Count; i++)
                {

                    DataRow dr;
                    dr = dt.NewRow();
                    dr["ProgNo"] = Progno;
                    dr["checknum"] = Chkno;
                    dr["distNo"] = Convert.ToInt32(dttemp.Rows[i]["distno"].ToString().Trim());
                    dr["purchase"] = dttemp.Rows[i]["purchase"].ToString().Trim();
                    dr["adjustment"] = dttemp.Rows[i]["adjustment"].ToString().Trim();

                    dr["PSpurchase"] = dttemp.Rows[i]["pspurchase"].ToString().Trim();

                    if (dttemp.Rows[i]["Discrepency"] != DBNull.Value && dttemp.Rows[i]["Discrepency"].ToString() != "" && dttemp.Rows[i]["Discrepency"].ToString().Trim().ToUpper() == "TRUE")
                        dr["Discrepency"] = 1;
                    else
                        dr["Discrepency"] = 0;

                    dr["Benefit"] = dttemp.Rows[i]["Benefit"].ToString().Trim();
                    dr["HQFee"] = dttemp.Rows[i]["HQFee"].ToString().Trim();
                    dt.Rows.Add(dr);
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    string lSql = "";
                    SqlConnection tConn = new SqlConnection(sqlcon);
                    SqlDataAdapter lCmd;

                    lSql = "STP_BenefitDistUpdate";
                    lCmd = new SqlDataAdapter(lSql, tConn);
                    lCmd.SelectCommand = new SqlCommand(lSql, tConn);
                    lCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
                    lCmd.SelectCommand.Parameters.Clear();
                    lCmd.SelectCommand.Parameters.AddWithValue("@Progno", Progno);
                    lCmd.SelectCommand.Parameters.AddWithValue("@Checknum", Chkno);
                    lCmd.SelectCommand.Parameters.AddWithValue("@Remarks", Remarks);
                    lCmd.SelectCommand.Parameters.AddWithValue("@BDistUpdate", dt);
                    lCmd.Fill(lds, "tblBenefit");
                }
                else
                {
                    ErrMsg = "No records found";
                }
            }
            catch (Exception ex)
            {

            }
            return Json(new { Items = "", ErrMsg = ErrMsg });
        }

        public ActionResult DeleteBenefitRecon(string Distno = "", string Progno = "", string Chkno = "")
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "delete from benefitdistribution where progno='" + Progno + "' and checknum='" + Chkno + "' and distno='" + Distno + "'";
            Ggrp.Execute(sql, Sconn);
            return Json(new { Items = "" });
        }

        public ActionResult FillCompanyname()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select company,DistNo from distributor where isnull(ddtlink,'')!=''  order by 1";
            Ggrp.Getdataset(sql, "FillBenefitDistDataGrid", ds, ref da, Sconn);
            List<Hashtable> FillCompanyname = new List<Hashtable>();
            FillCompanyname = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillBenefitDistDataGrid"]);
            return Json(new { Items = FillCompanyname });
        }

        public ActionResult FillDiscrepency()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select 'True' as Discrepency,'C' as DiscrepencyDisp union select 'False' as Discrepency,'D' as DiscrepencyDisp";
            Ggrp.Getdataset(sql, "FillDiscrepency", ds, ref da, Sconn);
            List<Hashtable> FillDiscrepency = new List<Hashtable>();
            FillDiscrepency = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillDiscrepency"]);
            return Json(new { Items = FillDiscrepency });
        }

        public ActionResult FillRemarks(string ProgNo = "")
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select DistributionRemarks from benefitcheck where progno='" + ProgNo + "'";
            Ggrp.Getdataset(sql, "FillRemarks", ds, ref da, Sconn);
            List<Hashtable> FillRemarks = new List<Hashtable>();
            FillRemarks = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillRemarks"]);
            return Json(new { Items = FillRemarks });
        }


        public ActionResult BgenefitReconReport(string progNo, string ChkNo = "")
        {
            Session["dsReport"] = null;
            string SQL = "";
            Sconn = new SqlConnection(sqlcon);
            DataSet bds = new DataSet();
            SQL = "STP_RP_BenefitBatchRecon '',''," + progNo + ",'" + ChkNo + "'";
            Ggrp.Getdataset(SQL, "Results", bds, ref da, Sconn);
            Session["dsReport"] = bds;
            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            string RPTFileName = "RP_BenefitReconMgfByDist.repx";
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });
        }

        //public ActionResult ViewCompTotal(string MfgNo, string Distno, string Period)
        //{
        //    string sql = "";
        //    Sconn = new SqlConnection(sqlcon);
        //    //sql = "select DistributionRemarks from benefitcheck where progno='" + MfgNo + "'";

        //    string fdt = Period;
        //    string[] ffdt = fdt.Split('-');
        //    string sfdate = "11/01/" + ffdt[0].ToString();
        //    string ssfdate = "10/31/" + ffdt[1].ToString();

        //    sql = "select * from benefitprogram p";
        //    sql += " left join benefitdistribution d on d.ProgNo = p.ProgNo left join benefitcheck c on d.ProgNo = c.ProgNo and d.checknum = c.checknum ";
        //    sql += " where(recvdon between '" + sfdate + "' and '" + ssfdate + "') and distNO = '" + Distno + "' and MfgNo = '" + MfgNo + "' and isnull(p.pur_xcld, 0)= 1";

        //    Ggrp.Getdataset(sql, "FillRemarks", ds, ref da, Sconn);
        //    List<Hashtable> FillRemarks = new List<Hashtable>();
        //    FillRemarks = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillRemarks"]);
        //    return Json(new { Items = FillRemarks, Sdate = sfdate, Edate = ssfdate });
        //}

        #endregion

        #region ProgInfo
        public ActionResult ProgInfo()
        {
            return View();
        }

        public ActionResult FillMfgProgInfoGrid(string mfgNo)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = @"select  p.*,m.french,m.spanish,m.cnreg,m.cnna,m.bilinpkg, m.youtube,m.facebook,m.twitter,m.eprodinfo,m.elineart,m.eadcopy,m.wprodinfo,
                 m.wlineart,m.wadcopy,m.factreps,m.mfrreps from mfg m left join mfgprginfo p on p.mfgno=m.mfgno  where m.mfgNo='" + mfgNo + "'";
            Ggrp.Getdataset(sql, "FillMfgProgInfoGrid", ds, ref da, Sconn);
            List<Hashtable> FillMfgProgInfoGrid = new List<Hashtable>();
            FillMfgProgInfoGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgProgInfoGrid"]);
            return Json(new { Items = FillMfgProgInfoGrid });
        }

        public ActionResult FillProgReport(string Repname, string mfgNo)
        {
            Session["dsReport"] = null;
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,OP.phone as GGphone,'PY'+REPLACE(BP.period, '-', '/') as period,  
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
            left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system) where m.mfgNo='" + mfgNo + "'";
            Ggrp.Getdataset(sql, "Results", ds, ref da, Sconn);
            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            Session["dsReport"] = ds;
            string RPTFileName = Repname;
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });
        }

        #endregion

        #region BPO

        public ActionResult SaveProgInfoBPO(string ObjProgInfo = "")
        {
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjProgInfo = "[" + ObjProgInfo + "]";
            ObjProgInfo = ObjProgInfo.Replace("|", "<").Replace("~", ">");
            List<Hashtable> AddProgInfo = jss.Deserialize<List<Hashtable>>(ObjProgInfo);
            DataRow ProgInfoAdd;
            if (AddProgInfo.Count > 0)
            {
                try
                {
                    string sql = "";
                    DataSet dsProgInfo = new DataSet();
                    SqlDataAdapter daProgInfo = new SqlDataAdapter();
                    sql = "Select * from mfgprginfo where mfgno='" + AddProgInfo[0]["mfgNo"].ToString() + "'";
                    Ggrp.Getdataset(sql, "mfgprginfo", dsProgInfo, ref daProgInfo, Sconn);
                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfo"].Rows.Count == 0)
                    {
                        sql = "Select * from mfgprginfo where 1=0";
                        Ggrp.Getdataset(sql, "mfgprginfo", dsProgInfo, ref daProgInfo, Sconn);
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfo"].NewRow();
                    }
                    else
                    {
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfo"].Rows[0];
                    }
                    if (AddProgInfo[0]["mfgprginfo"] != null && AddProgInfo[0]["mfgprginfo"].ToString() != "")
                        ProgInfoAdd["PrgInfoNo"] = AddProgInfo[0]["PrgInfoNo"].ToString();
                    ProgInfoAdd["mfgno"] = AddProgInfo[0]["mfgNo"].ToString();
                    ProgInfoAdd["bpo_stat"] = AddProgInfo[0]["bpo_stat"].ToString();
                    ProgInfoAdd["indexcmmt"] = AddProgInfo[0]["indexcmmt"] == null ? "" : AddProgInfo[0]["indexcmmt"].ToString();
                    ProgInfoAdd["bpo_revyr"] = AddProgInfo[0]["bpo_revyr"] == null ? "" : AddProgInfo[0]["bpo_revyr"].ToString();
                    ProgInfoAdd["ft_fob"] = AddProgInfo[0]["ft_fob"] == null ? "" : AddProgInfo[0]["ft_fob"].ToString();
                    ProgInfoAdd["ft_pickup"] = AddProgInfo[0]["ft_pickup"] == null ? "" : AddProgInfo[0]["ft_pickup"].ToString();
                    ProgInfoAdd["ft_oth"] = AddProgInfo[0]["ft_oth"] == null ? "" : AddProgInfo[0]["ft_oth"].ToString();
                    ProgInfoAdd["ft_fobadd"] = AddProgInfo[0]["ft_fobadd"] == null ? "" : AddProgInfo[0]["ft_fobadd"].ToString();
                    ProgInfoAdd["pt_stand"] = AddProgInfo[0]["pt_stand"] == null ? "" : AddProgInfo[0]["pt_stand"].ToString();
                    ProgInfoAdd["pt_specgg"] = AddProgInfo[0]["pt_specgg"] == null ? "" : AddProgInfo[0]["pt_specgg"].ToString();
                    ProgInfoAdd["pt_eospec"] = AddProgInfo[0]["pt_eospec"] == null ? "" : AddProgInfo[0]["pt_eospec"].ToString();
                    ProgInfoAdd["pt_eostnd"] = AddProgInfo[0]["pt_eostnd"] == null ? "" : AddProgInfo[0]["pt_eostnd"].ToString();
                    ProgInfoAdd["pt_disc"] = AddProgInfo[0]["pt_disc"] == null ? "" : AddProgInfo[0]["pt_disc"].ToString();
                    ProgInfoAdd["eop_eod"] = AddProgInfo[0]["eop_eod"] == null ? "" : AddProgInfo[0]["eop_eod"].ToString();
                    ProgInfoAdd["eop_op"] = AddProgInfo[0]["eop_op"] == null ? "" : AddProgInfo[0]["eop_op"].ToString();
                    ProgInfoAdd["eop_et"] = AddProgInfo[0]["eop_et"] == null ? "" : AddProgInfo[0]["eop_et"].ToString();
                    if (AddProgInfo[0]["eop_etd"] != null && AddProgInfo[0]["eop_etd"].ToString() != "")
                        ProgInfoAdd["eop_etd"] = AddProgInfo[0]["eop_etd"].ToString();
                    else
                        ProgInfoAdd["eop_etd"] = DBNull.Value;
                    ProgInfoAdd["eop_takby"] = AddProgInfo[0]["eop_takby"] == null ? "" : AddProgInfo[0]["eop_takby"].ToString();
                    ProgInfoAdd["eop_sp"] = AddProgInfo[0]["eop_sp"] == null ? "" : AddProgInfo[0]["eop_sp"].ToString();
                    ProgInfoAdd["ds_dmin"] = AddProgInfo[0]["ds_dmin"] == null ? "" : AddProgInfo[0]["ds_dmin"].ToString();
                    ProgInfoAdd["ds_note"] = AddProgInfo[0]["ds_note"] == null ? "" : AddProgInfo[0]["ds_note"].ToString();
                    ProgInfoAdd["ds_dft"] = AddProgInfo[0]["ds_dft"] == null ? "" : AddProgInfo[0]["ds_dft"].ToString();
                    ProgInfoAdd["ds_cmmt"] = AddProgInfo[0]["ds_cmmt"] == null ? "" : AddProgInfo[0]["ds_cmmt"].ToString();
                    ProgInfoAdd["as_sr1"] = AddProgInfo[0]["as_sr1"] == null ? "" : AddProgInfo[0]["as_sr1"].ToString();
                    ProgInfoAdd["as_ggdist"] = AddProgInfo[0]["as_ggdist"] == null ? "" : AddProgInfo[0]["as_ggdist"].ToString();
                    ProgInfoAdd["as_sdist"] = AddProgInfo[0]["as_sdist"] == null ? "" : AddProgInfo[0]["as_sdist"].ToString();
                    ProgInfoAdd["as_coop"] = AddProgInfo[0]["as_coop"] == null ? "" : AddProgInfo[0]["as_coop"].ToString();

                    if (AddProgInfo[0]["as_isprof"] != null && AddProgInfo[0]["as_isprof"].ToString().Trim().ToUpper() == "TRUE")
                        ProgInfoAdd["as_isprof"] = "Y";
                    else
                        ProgInfoAdd["as_isprof"] = "N";

                    if (AddProgInfo[0]["as_ofinv"] != null && AddProgInfo[0]["as_ofinv"].ToString().Trim().ToUpper() == "TRUE")
                        ProgInfoAdd["as_ofinv"] = "Y";
                    else
                        ProgInfoAdd["as_ofinv"] = "N";

                    if (AddProgInfo[0]["dmp_fldds"] != null && AddProgInfo[0]["dmp_fldds"].ToString().Trim().ToUpper() == "TRUE")
                        ProgInfoAdd["dmp_fldds"] = "Y";
                    else
                        ProgInfoAdd["dmp_fldds"] = "N";

                    if (AddProgInfo[0]["dmp_callm"] != null && AddProgInfo[0]["dmp_callm"].ToString().Trim().ToUpper() == "TRUE")
                        ProgInfoAdd["dmp_callm"] = "Y";
                    else
                        ProgInfoAdd["dmp_callm"] = "N";

                    if (AddProgInfo[0]["dmp_callc"] != null && AddProgInfo[0]["dmp_callc"].ToString().Trim().ToUpper() == "TRUE")
                        ProgInfoAdd["dmp_callc"] = "Y";
                    else
                        ProgInfoAdd["dmp_callc"] = "N";

                    if (AddProgInfo[0]["dmp_calln"] != null && AddProgInfo[0]["dmp_calln"].ToString().Trim().ToUpper() == "TRUE")
                        ProgInfoAdd["dmp_calln"] = "Y";
                    else
                        ProgInfoAdd["dmp_calln"] = "N";

                    ProgInfoAdd["dmp_oth"] = AddProgInfo[0]["dmp_oth"] == null ? "" : AddProgInfo[0]["dmp_oth"].ToString();

                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfo"].Rows.Count == 0)
                    {
                        dsProgInfo.Tables["mfgprginfo"].Rows.Add(ProgInfoAdd);
                    }
                    else
                    {
                        dsProgInfo.Tables["mfgprginfo"].GetChanges();
                    }
                    daProgInfo.Update(dsProgInfo, "mfgprginfo");
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { ItemMsg = "" });
        }

        public ActionResult FillBPOReport(string Repname, string mfgNo)
        {
            DataSet bpds = new DataSet();
            Sconn = new SqlConnection(sqlcon);
            Session["dsReport"] = null;
            Session["Description"] = "";
            string sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,
                        OP.phone as GGphone, p.*,m.company,'PY'+REPLACE(BP.period, '-', '/') as PrgPeriod,
                        RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progfrom)), 2)+'/'+RIGHT('00' + CONVERT(NVARCHAR(2), 
                        DATEPART(DAY, BP.progfrom)), 2)+' - '+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progto)), 2)+'/'+ RIGHT('00' + CONVERT(NVARCHAR(2), 
                        DATEPART(DAY, BP.progto)), 2) as Term,lk.descript as BpoStatus from mfgprginfo p 
                        left join mfg m on p.mfgno=m.mfgno 
                        left join  op_system OP on 1=1 left join op_lookup lk on lk.entry=p.bpo_stat and lookupid='BPOStat'
                        left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system)  where m.mfgno = '" + mfgNo + "'";
            Ggrp.Getdataset(sql, "Results", bpds, ref da, Sconn);
            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            Session["dsReport"] = bpds;
            string RPTFileName = Repname;
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });

        }

        #endregion

        #region BPONotes

        public ActionResult SaveProgInfoBPONotes(string ObjProgInfo = "")
        {
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjProgInfo = "[" + ObjProgInfo + "]";
            ObjProgInfo = ObjProgInfo.Replace("|", "<").Replace("~", ">");
            List<Hashtable> AddProgInfo = jss.Deserialize<List<Hashtable>>(ObjProgInfo);
            DataRow ProgInfoAdd;
            if (AddProgInfo.Count > 0)
            {
                try
                {
                    string sql = "";
                    DataSet dsProgInfo = new DataSet();
                    SqlDataAdapter daProgInfo = new SqlDataAdapter();
                    sql = "Select * from mfgprginfo where mfgno='" + AddProgInfo[0]["mfgNo"].ToString() + "'";
                    Ggrp.Getdataset(sql, "mfgprginfo", dsProgInfo, ref daProgInfo, Sconn);
                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfo"].Rows.Count == 0)
                    {
                        sql = "Select * from mfgprginfo where 1=0";
                        Ggrp.Getdataset(sql, "mfgprginfo", dsProgInfo, ref daProgInfo, Sconn);
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfo"].NewRow();
                    }
                    else
                    {
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfo"].Rows[0];
                    }
                    if (AddProgInfo[0]["mfgprginfo"] != null && AddProgInfo[0]["mfgprginfo"].ToString() != "")
                        ProgInfoAdd["PrgInfoNo"] = AddProgInfo[0]["PrgInfoNo"].ToString();
                    ProgInfoAdd["mfgno"] = AddProgInfo[0]["mfgNo"].ToString();
                    ProgInfoAdd["bpo_notes"] = AddProgInfo[0]["bpo_notes"] == null ? "" : AddProgInfo[0]["bpo_notes"].ToString();

                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfo"].Rows.Count == 0)
                    {
                        dsProgInfo.Tables["mfgprginfo"].Rows.Add(ProgInfoAdd);
                    }
                    else
                    {
                        dsProgInfo.Tables["mfgprginfo"].GetChanges();
                    }
                    daProgInfo.Update(dsProgInfo, "mfgprginfo");
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { ItemMsg = "" });
        }

        #endregion

        #region MDS Section

        public ActionResult FillMDSInfo(string mfgNo)
        {
            string SQL = "";
            Sconn = new SqlConnection(sqlcon);
            SQL = "select m.edi850,edi810,edi856,edi856fax,s.* from mfg m left join mfgshipinfo s on m.mfgno=s.mfgno where m.mfgno=" + mfgNo;
            DataSet dsMDS = new DataSet();
            SqlDataAdapter daMDS = new SqlDataAdapter();
            Ggrp.Getdataset(SQL, "tblMDS", dsMDS, ref daMDS, Sconn);
            List<Hashtable> MDSList = new List<Hashtable>();
            MDSList = Ggrp.CovertDatasetToHashTbl(dsMDS.Tables["tblMDS"]);
            
            return Json(new { Items = MDSList });
        }

        public ActionResult FillMdsReport(string Prim, string Sec, string ClassCd, string mfgNo, string Repname)
        {
            string SQL = "";
            SQL = "STP_RP_MfgDataSheet '" + Prim + "','" + Sec + "','" + ClassCd + "','" + mfgNo + "'";
            Sconn = new SqlConnection(sqlcon);
            Ggrp.Getdataset(SQL, "Results", ds, ref da, Sconn);
            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            Session["dsReport"] = ds;
            string RPTFileName = Repname;
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });
        }

        public ActionResult SaveMDS(string ObjmfgList)
        {
            string sql = "";
            DataSet dsShip = new DataSet();
            SqlDataAdapter daship = new SqlDataAdapter();
            DataRow ShipdrAdd;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjmfgList = "[" + ObjmfgList + "]";
            ObjmfgList = ObjmfgList.Replace("|", "<").Replace("~", ">");
            List<Hashtable> AddMfg = jss.Deserialize<List<Hashtable>>(ObjmfgList);

            Sconn = new SqlConnection(sqlcon);
            if (Sconn.State != ConnectionState.Open)
                Sconn.Open();
            SqlTransaction SqlTrans = Sconn.BeginTransaction();
            DataRow drAdd;
            try
            {
                int MfgNo = 0;
                int.TryParse(AddMfg[0]["mfgno"].ToString(), out MfgNo);

                DataSet mds = new DataSet();
                
                string Sql = "select * from mfg where mfgno='" + MfgNo + "'";
                SqlDataAdapter mda = new SqlDataAdapter(Sql, Sconn);
                Ggrp.Getdataset(Sql, "tblmfg", mds, ref mda, Sconn, ref SqlTrans);
                
                drAdd = mds.Tables["tblmfg"].Rows[0];
                drAdd["edi850"] = AddMfg[0]["edi850"].ToString();
                drAdd["edi810"] = AddMfg[0]["edi810"].ToString();
                drAdd["edi856"] = AddMfg[0]["edi856"].ToString();
                drAdd["edi856fax"] = AddMfg[0]["edi856fax"].ToString();
                
                mds.Tables["tblmfg"].GetChanges();
                mda.Update(mds, "tblmfg");

                sql = "Select * from mfgshipinfo where mfgno='" + MfgNo + "'";
                Ggrp.Getdataset(sql, "tblshipinfo", dsShip, ref daship, Sconn, ref SqlTrans);
                if (dsShip != null && dsShip.Tables["tblshipinfo"].Rows.Count == 0)
                {
                    sql = "Select * from mfgshipinfo where 1=0";
                    Ggrp.Getdataset(sql, "tblshipinfo", dsShip, ref daship, Sconn, ref SqlTrans);
                    ShipdrAdd = dsShip.Tables["tblshipinfo"].NewRow();
                }
                else
                {
                    ShipdrAdd = dsShip.Tables["tblshipinfo"].Rows[0];
                }
                ShipdrAdd["mfgno"] = MfgNo;
                ShipdrAdd["oldmfgno"] = AddMfg[0]["oldmfgno"] == null ? "" : AddMfg[0]["oldmfgno"].ToString();
                if (AddMfg[0]["leadtime"] != null && AddMfg[0]["leadtime"].ToString() != "")
                    ShipdrAdd["leadtime"] = AddMfg[0]["leadtime"].ToString();
                else
                    ShipdrAdd["leadtime"] = DBNull.Value;
                if (AddMfg[0]["fillrate"] != null && AddMfg[0]["fillrate"].ToString() != "")
                    ShipdrAdd["fillrate"] = AddMfg[0]["fillrate"].ToString();
                else
                    ShipdrAdd["fillrate"] = DBNull.Value;
                ShipdrAdd["siadd1"] = AddMfg[0]["siadd1"] == null ? "" : AddMfg[0]["siadd1"].ToString();
                ShipdrAdd["siphone1"] = AddMfg[0]["siphone1"] == null ? "" : AddMfg[0]["siphone1"].ToString();
                ShipdrAdd["siext1"] = AddMfg[0]["siext1"] == null ? "" : AddMfg[0]["siext1"].ToString();
                ShipdrAdd["sidocks1"] = AddMfg[0]["sidocks1"] == null ? "" : AddMfg[0]["sidocks1"].ToString();
                ShipdrAdd["sicity1"] = AddMfg[0]["sicity1"] == null ? "" : AddMfg[0]["sicity1"].ToString();
                ShipdrAdd["sistate1"] = AddMfg[0]["sistate1"] == null ? "" : AddMfg[0]["sistate1"].ToString();
                ShipdrAdd["sizip1"] = AddMfg[0]["sizip1"] == null ? "" : AddMfg[0]["sizip1"].ToString();
                ShipdrAdd["sisuper1"] = AddMfg[0]["sisuper1"] == null ? "" : AddMfg[0]["sisuper1"].ToString();
                ShipdrAdd["sihours1"] = AddMfg[0]["sihours1"] == null ? "" : AddMfg[0]["sihours1"].ToString();
                ShipdrAdd["siadd2"] = AddMfg[0]["siadd2"] == null ? "" : AddMfg[0]["siadd2"].ToString();
                ShipdrAdd["siphone2"] = AddMfg[0]["siphone2"] == null ? "" : AddMfg[0]["siphone2"].ToString();
                ShipdrAdd["siext2"] = AddMfg[0]["siext2"] == null ? "" : AddMfg[0]["siext2"].ToString();
                ShipdrAdd["sidocks2"] = AddMfg[0]["sidocks2"] == null ? "" : AddMfg[0]["sidocks2"].ToString();
                ShipdrAdd["sicity2"] = AddMfg[0]["sicity2"] == null ? "" : AddMfg[0]["sicity2"].ToString();
                ShipdrAdd["sistate2"] = AddMfg[0]["sistate2"] == null ? "" : AddMfg[0]["sistate2"].ToString();
                ShipdrAdd["sizip2"] = AddMfg[0]["sizip2"] == null ? "" : AddMfg[0]["sizip2"].ToString();
                ShipdrAdd["sisuper2"] = AddMfg[0]["sisuper2"] == null ? "" : AddMfg[0]["sisuper2"].ToString();
                ShipdrAdd["sihours2"] = AddMfg[0]["sihours2"] == null ? "" : AddMfg[0]["sihours2"].ToString();
                ShipdrAdd["siadd3"] = AddMfg[0]["siadd3"] == null ? "" : AddMfg[0]["siadd3"].ToString();
                ShipdrAdd["siphone3"] = AddMfg[0]["siphone3"] == null ? "" : AddMfg[0]["siphone3"].ToString();
                ShipdrAdd["siext3"] = AddMfg[0]["siext3"] == null ? "" : AddMfg[0]["siext3"].ToString();
                ShipdrAdd["sidocks3"] = AddMfg[0]["sidocks3"] == null ? "" : AddMfg[0]["sidocks3"].ToString();
                ShipdrAdd["sicity3"] = AddMfg[0]["sicity3"] == null ? "" : AddMfg[0]["sicity3"].ToString();
                ShipdrAdd["sistate3"] = AddMfg[0]["sistate3"] == null ? "" : AddMfg[0]["sistate3"].ToString();
                ShipdrAdd["sizip3"] = AddMfg[0]["sizip3"] == null ? "" : AddMfg[0]["sizip3"].ToString();
                ShipdrAdd["sisuper3"] = AddMfg[0]["sisuper3"] == null ? "" : AddMfg[0]["sisuper3"].ToString();
                ShipdrAdd["sihours3"] = AddMfg[0]["sihours3"] == null ? "" : AddMfg[0]["sihours3"].ToString();
                ShipdrAdd["siadd4"] = AddMfg[0]["siadd4"] == null ? "" : AddMfg[0]["siadd4"].ToString();
                ShipdrAdd["siphone4"] = AddMfg[0]["siphone4"] == null ? "" : AddMfg[0]["siphone4"].ToString();
                ShipdrAdd["siext4"] = AddMfg[0]["siext4"] == null ? "" : AddMfg[0]["siext4"].ToString();
                ShipdrAdd["sidocks4"] = AddMfg[0]["sidocks4"] == null ? "" : AddMfg[0]["sidocks4"].ToString();
                ShipdrAdd["sicity4"] = AddMfg[0]["sicity4"] == null ? "" : AddMfg[0]["sicity4"].ToString();
                ShipdrAdd["sistate4"] = AddMfg[0]["sistate4"] == null ? "" : AddMfg[0]["sistate4"].ToString();
                ShipdrAdd["sizip4"] = AddMfg[0]["sizip4"] == null ? "" : AddMfg[0]["sizip4"].ToString();
                ShipdrAdd["sisuper4"] = AddMfg[0]["sisuper4"] == null ? "" : AddMfg[0]["sisuper4"].ToString();
                ShipdrAdd["sihours4"] = AddMfg[0]["sihours4"] == null ? "" : AddMfg[0]["sihours4"].ToString();
                ShipdrAdd["comments"] = AddMfg[0]["comments"] == null ? "" : AddMfg[0]["comments"].ToString();

                if (dsShip != null && dsShip.Tables["tblshipinfo"].Rows.Count == 0)
                {
                    dsShip.Tables["tblshipinfo"].Rows.Add(ShipdrAdd);
                }
                else
                {
                    dsShip.Tables["tblshipinfo"].GetChanges();
                }
                daship.Update(dsShip, "tblshipinfo");
                SqlTrans.Commit();
                if (Sconn.State == ConnectionState.Open) Sconn.Close();
            }
            catch (Exception ex)
            {
                if (Sconn.State == ConnectionState.Open && SqlTrans != null && SqlTrans.Connection != null)
                    SqlTrans.Rollback();
                return Json(new { ItemMsg = ex.ToString() });

            }
            return Json(new { ItemMsg = "" });
        }

        #endregion

        #region Spp

        public ActionResult FillMfgProgSPPGrid(string SppScpNo, string mfgNo)
        {
            string sql = ""; var rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = "select * from mfgsppscp where mfgno ='" + mfgNo + "' order by s_revyr desc";
            Ggrp.Getdataset(sql, "MfgProgSPPGrid", ds, ref da, Sconn);
            List<Hashtable> FillMfgProgSPPGrid = new List<Hashtable>();
            FillMfgProgSPPGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["MfgProgSPPGrid"]);
            if (!string.IsNullOrEmpty(SppScpNo) && ds.Tables["MfgProgSPPGrid"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["MfgProgSPPGrid"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["MfgProgSPPGrid"].Select().ToList()
                                   where dr["SppScpNo"].ToString() == SppScpNo
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            DataSet rds = new DataSet();
            sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,OP.Phone as GGphone,OP.period, 
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
                        m.company from mfg m left join mfgprginfo p on p.mfgno=m.mfgno left join  op_system OP on 1=1 where m.mfgno='" + mfgNo + "'";
            Ggrp.Getdataset(sql, "Results", rds, ref da, Sconn);

            Session["dsReport"] = null;
            Session["dsReport"] = rds;

            return Json(new { Items = FillMfgProgSPPGrid, rowindex = rowindex });
        }

        public ActionResult SaveSpp(string ObjSpp = "", bool IsAddMode = false)
        {
            string Sql = ""; var SppScpNo = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjSpp = "[" + ObjSpp + "]";
            List<Hashtable> AddSppList = jss.Deserialize<List<Hashtable>>(ObjSpp);
            DataSet sds = new DataSet();
            SqlDataAdapter sda = new SqlDataAdapter();
            DataRow drAdd;
            if (AddSppList.Count > 0)
            {
                try
                {
                    sds = new DataSet();
                    if (IsAddMode == true)
                    {
                        Sql = "select * from mfgsppscp where 1=0";
                    }
                    else
                    {
                        Sql = "select * from mfgsppscp where SppScpNo='" + AddSppList[0]["SppScpNo"].ToString() + "'";
                    }
                    Ggrp.Getdataset(Sql, "mfgsppscp", sds, ref sda, Sconn, false);
                    if (IsAddMode == true)
                    {
                        drAdd = sds.Tables["mfgsppscp"].NewRow();
                    }
                    else
                    {
                        drAdd = sds.Tables["mfgsppscp"].Rows[0];
                    }
                    drAdd["MfgNo"] = AddSppList[0]["MfgNo"] == null ? "" : AddSppList[0]["MfgNo"].ToString();
                    drAdd["oldmfgno"] = AddSppList[0]["oldmfgno"] == null ? "" : AddSppList[0]["oldmfgno"].ToString();
                    drAdd["s_revyr"] = AddSppList[0]["s_revyr"].ToString();
                    drAdd["s_status"] = AddSppList[0]["s_status"].ToString();
                    drAdd["desc_"] = AddSppList[0]["desc_"].ToString();
                    drAdd["abc_op"] = AddSppList[0]["abc_op"].ToString();
                    drAdd["abc_sp"] = AddSppList[0]["abc_sp"].ToString();
                    drAdd["abc_extrm"] = AddSppList[0]["abc_extrm"].ToString();
                    drAdd["abc_oreq"] = AddSppList[0]["abc_oreq"].ToString();
                    if (IsAddMode == true)
                    {
                        sds.Tables["mfgsppscp"].Rows.Add(drAdd);
                    }
                    else
                    {
                        sds.Tables["mfgsppscp"].GetChanges();
                    }
                    sda.Update(sds, "mfgsppscp");
                    if (IsAddMode == true)
                    {
                        SppScpNo = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        SppScpNo = Convert.ToInt32(AddSppList[0]["SppScpNo"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { Items = SppScpNo });
        }

        #endregion

        #region ProgInfoPRG1

        public ActionResult SaveProgInfoPRG1(string ObjProgInfo = "")
        {
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjProgInfo = "[" + ObjProgInfo + "]";
            ObjProgInfo = ObjProgInfo.Replace("|", "<").Replace("~", ">");
            List<Hashtable> AddProgInfo = jss.Deserialize<List<Hashtable>>(ObjProgInfo);
            DataRow ProgInfoAdd;
            if (AddProgInfo.Count > 0)
            {
                try
                {
                    string sql = "";
                    DataSet dsProgInfo = new DataSet();
                    SqlDataAdapter daProgInfo = new SqlDataAdapter();
                    sql = "Select * from mfgprginfo where mfgno='" + AddProgInfo[0]["mfgNo"].ToString() + "'";
                    Ggrp.Getdataset(sql, "mfgprginfoprg1", dsProgInfo, ref daProgInfo, Sconn);
                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfoprg1"].Rows.Count == 0)
                    {
                        sql = "Select * from mfgprginfo where 1=0";
                        Ggrp.Getdataset(sql, "mfgprginfoprg1", dsProgInfo, ref daProgInfo, Sconn);
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfoprg1"].NewRow();
                    }
                    else
                    {
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfoprg1"].Rows[0];
                    }
                    if (AddProgInfo[0]["mfgprginfoprg1"] != null && AddProgInfo[0]["mfgprginfoprg1"].ToString() != "")
                        ProgInfoAdd["PrgInfoNo"] = AddProgInfo[0]["PrgInfoNo"].ToString();
                    ProgInfoAdd["mfgno"] = AddProgInfo[0]["mfgNo"].ToString();
                    ProgInfoAdd["prg_stat"] = AddProgInfo[0]["prg_stat"] == null ? "" : AddProgInfo[0]["prg_stat"].ToString();
                    ProgInfoAdd["prg_revyr"] = AddProgInfo[0]["prg_revyr"] == null ? "" : AddProgInfo[0]["prg_revyr"].ToString();
                    ProgInfoAdd["prg_revds"] = AddProgInfo[0]["prg_revds"] == null ? "" : AddProgInfo[0]["prg_revds"].ToString();
                    ProgInfoAdd["ProdLines"] = AddProgInfo[0]["ProdLines"] == null ? "" : AddProgInfo[0]["ProdLines"].ToString();
                    ProgInfoAdd["MfgPoints"] = AddProgInfo[0]["MfgPoints"] == null ? "" : AddProgInfo[0]["MfgPoints"].ToString();
                    ProgInfoAdd["PromoOpportunity"] = AddProgInfo[0]["PromoOpportunity"] == null ? "" : AddProgInfo[0]["PromoOpportunity"].ToString();

                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfoprg1"].Rows.Count == 0)
                    {
                        dsProgInfo.Tables["mfgprginfoprg1"].Rows.Add(ProgInfoAdd);
                    }
                    else
                    {
                        dsProgInfo.Tables["mfgprginfoprg1"].GetChanges();
                    }
                    daProgInfo.Update(dsProgInfo, "mfgprginfoprg1");
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { ItemMsg = "" });
        }

        #endregion

        #region ProgInfoPRG2

        public ActionResult SaveProgInfoPRG2(string ObjProgInfo = "")
        {
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjProgInfo = "[" + ObjProgInfo + "]";
            ObjProgInfo = ObjProgInfo.Replace("|", "<").Replace("~", ">");
            List<Hashtable> AddProgInfo = jss.Deserialize<List<Hashtable>>(ObjProgInfo);
            DataRow ProgInfoAdd;
            DataRow ProgmfgAdd;
            if (AddProgInfo.Count > 0)
            {
                try
                {
                    string sql = "";
                    DataSet dsProgInfo = new DataSet();
                    SqlDataAdapter daProgInfo = new SqlDataAdapter();
                    sql = "Select * from mfgprginfo where mfgno='" + AddProgInfo[0]["mfgNo"].ToString() + "'";
                    Ggrp.Getdataset(sql, "mfgprginfoprg2", dsProgInfo, ref daProgInfo, Sconn);
                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfoprg2"].Rows.Count == 0)
                    {
                        sql = "Select * from mfgprginfo where 1=0";
                        Ggrp.Getdataset(sql, "mfgprginfoprg2", dsProgInfo, ref daProgInfo, Sconn);
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfoprg2"].NewRow();
                    }
                    else
                    {
                        ProgInfoAdd = dsProgInfo.Tables["mfgprginfoprg2"].Rows[0];
                    }
                    if (AddProgInfo[0]["mfgprginfoprg2"] != null && AddProgInfo[0]["mfgprginfoprg2"].ToString() != "")
                        ProgInfoAdd["PrgInfoNo"] = AddProgInfo[0]["PrgInfoNo"].ToString();
                    ProgInfoAdd["mfgno"] = AddProgInfo[0]["mfgNo"].ToString();
                    ProgInfoAdd["PackagingDetails"] = AddProgInfo[0]["PackagingDetails"] == null ? "" : AddProgInfo[0]["PackagingDetails"].ToString();
                    ProgInfoAdd["MTeamHQDetails"] = AddProgInfo[0]["MTeamHQDetails"] == null ? "" : AddProgInfo[0]["MTeamHQDetails"].ToString();

                    if (dsProgInfo != null && dsProgInfo.Tables["mfgprginfoprg2"].Rows.Count == 0)
                    {
                        dsProgInfo.Tables["mfgprginfoprg2"].Rows.Add(ProgInfoAdd);
                    }
                    else
                    {
                        dsProgInfo.Tables["mfgprginfoprg2"].GetChanges();
                    }
                    daProgInfo.Update(dsProgInfo, "mfgprginfoprg2");

                    //Mfg table

                    string sqlmfg = "";
                    DataSet dsProgmfg = new DataSet();
                    SqlDataAdapter daProgmfg = new SqlDataAdapter();
                    sqlmfg = "Select * from mfg where mfgno='" + AddProgInfo[0]["mfgNo"].ToString() + "'";
                    Ggrp.Getdataset(sqlmfg, "mfgprginfo", dsProgmfg, ref daProgmfg, Sconn);

                    ProgmfgAdd = dsProgmfg.Tables["mfgprginfo"].Rows[0];
                    ProgmfgAdd["factreps"] = AddProgInfo[0]["factreps"].ToString();
                    ProgmfgAdd["eprodinfo"] = AddProgInfo[0]["eprodinfo"].ToString();
                    ProgmfgAdd["elineart"] = AddProgInfo[0]["elineart"].ToString();
                    ProgmfgAdd["eadcopy"] = AddProgInfo[0]["eadcopy"].ToString();
                    ProgmfgAdd["youtube"] = AddProgInfo[0]["youtube"].ToString();
                    ProgmfgAdd["facebook"] = AddProgInfo[0]["facebook"].ToString();
                    ProgmfgAdd["twitter"] = AddProgInfo[0]["twitter"].ToString();
                    ProgmfgAdd["mfrreps"] = AddProgInfo[0]["mfrreps"].ToString();
                    ProgmfgAdd["wprodinfo"] = AddProgInfo[0]["wprodinfo"].ToString();
                    ProgmfgAdd["wlineart"] = AddProgInfo[0]["wlineart"].ToString();
                    ProgmfgAdd["wadcopy"] = AddProgInfo[0]["wadcopy"].ToString();
                    ProgmfgAdd["cnreg"] = AddProgInfo[0]["cnreg"].ToString();
                    ProgmfgAdd["cnna"] = AddProgInfo[0]["cnna"].ToString();
                    ProgmfgAdd["french"] = AddProgInfo[0]["french"].ToString();
                    ProgmfgAdd["spanish"] = AddProgInfo[0]["spanish"].ToString();

                    dsProgmfg.Tables["mfgprginfo"].GetChanges();
                    daProgmfg.Update(dsProgmfg, "mfgprginfo");

                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { ItemMsg = "" });
        }

        #endregion

        #region MPL

        public ActionResult FillMfgProgMPLGrid(string mfgNo, string Mplno)
        {
            string sql = ""; int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = " select sugg_retl,sugg_deal,dist_ea,m.*, p.proddesc, p.proddesc2, p.size, p.um_desc, p.pack, p.um, p.ship_um,p.mfgpartno from mfgmpl m left join product p on p.upc = m.upc left join pds on p.upc=pds.upc where m.mfgno='" + mfgNo + "' order by MplNo desc";
            Ggrp.Getdataset(sql, "FillMfgProgMPLGrid", ds, ref da, Sconn);
            List<Hashtable> FillMfgProgMPLGrid = new List<Hashtable>();
            FillMfgProgMPLGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgProgMPLGrid"]);
            if (!string.IsNullOrEmpty(Mplno) && ds.Tables["FillMfgProgMPLGrid"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["FillMfgProgMPLGrid"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["FillMfgProgMPLGrid"].Select().ToList()
                                   where dr["Mplno"].ToString() == Mplno
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = FillMfgProgMPLGrid, rowindex = rowindex });
        }

        public ActionResult GetSuggestedInfo(string upc)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select sugg_retl,sugg_deal,dist_ea from pds where upc='" + upc + "'";
            Ggrp.Getdataset(sql, "SuggestedInfo", ds, ref da, Sconn);
            List<Hashtable> SuggestedInfo = new List<Hashtable>();
            SuggestedInfo = Ggrp.CovertDatasetToHashTbl(ds.Tables["SuggestedInfo"]);
            return Json(new { Items = SuggestedInfo });
        }

        public ActionResult SaveMPL(string objMPLlist = "", bool IsAddMode = false)
        {
            string Sql = "";
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            objMPLlist = "[" + objMPLlist + "]";
            List<Hashtable> AddMPL = jss.Deserialize<List<Hashtable>>(objMPLlist);
            DataSet mds = new DataSet();
            SqlDataAdapter mda = new SqlDataAdapter();
            DataRow drAdd;
            int MplNo = 0;
            if (AddMPL.Count > 0)
            {
                try
                {
                    mds = new DataSet();
                    if (IsAddMode == true)
                    {
                        Sql = "select * from mfgMpl  where 1=0";
                    }
                    else
                    {
                        Sql = "select * from mfgMpl  where MplNo='" + AddMPL[0]["MplNo"].ToString() + "'";
                    }
                    mda = new SqlDataAdapter(Sql, Sconn);
                    Ggrp.Getdataset(Sql, "tblMPL", mds, ref mda, Sconn, false);
                    if (IsAddMode == true)
                    {
                        drAdd = mds.Tables["tblMPL"].NewRow();
                    }
                    else
                    {
                        drAdd = mds.Tables["tblMPL"].Rows[0];
                        drAdd["MplNo"] = AddMPL[0]["MplNo"].ToString();
                        MplNo = Convert.ToInt32(AddMPL[0]["MplNo"].ToString());
                    }
                    drAdd["MfgNo"] = AddMPL[0]["MfgNo"] == null ? "" : AddMPL[0]["MfgNo"].ToString();
                    drAdd["upc"] = AddMPL[0]["upc"] == null ? "" : AddMPL[0]["upc"].ToString();
                    drAdd["seq_cd"] = AddMPL[0]["seq_cd"] == null ? "" : AddMPL[0]["seq_cd"].ToString().Trim();
                    drAdd["status"] = AddMPL[0]["status"] == null ? "" : AddMPL[0]["status"].ToString().Trim();
                    drAdd["spp_type"] = AddMPL[0]["spp_type"] == null ? "" : AddMPL[0]["spp_type"].ToString().Trim();
                    
                    if (IsAddMode == true)
                    {
                        mds.Tables["tblMPL"].Rows.Add(drAdd);
                    }
                    else
                    {
                        mds.Tables["tblMPL"].GetChanges();
                    }
                    mda.Update(mds, "tblMPL");
                    if (IsAddMode == true)
                        MplNo = Convert.ToInt32(Ggrp.GetIdentityValue(Sconn));

                    //Update Suggested Info in pds by UPC
                    string sUPC = "";
                    sUPC = AddMPL[0]["upc"].ToString().Trim();
                    sUPC = sUPC.Replace("'", "''");
                    string lSQL = "Select * from pds where upc='" + sUPC + "'";
                    DataSet pdsDs = new DataSet();
                    SqlDataAdapter pdsda = new SqlDataAdapter();
                    Ggrp.Getdataset(lSQL, "pdsDs", pdsDs, ref pdsda, Sconn);

                    if (pdsDs.Tables["pdsDs"] != null && pdsDs.Tables["pdsDs"].Rows.Count > 0)
                    {
                        DataRow drPDS;

                        drPDS = pdsDs.Tables["pdsDs"].Rows[0];
                        drPDS["sugg_retl"] = AddMPL[0]["std_retl"] == null || AddMPL[0]["std_retl"].ToString() == "" ? DBNull.Value : AddMPL[0]["std_retl"];
                        drPDS["sugg_deal"] = AddMPL[0]["std_deal"] == null || AddMPL[0]["std_deal"].ToString() == "" ? DBNull.Value : AddMPL[0]["std_deal"];
                        drPDS["dist_ea"] = AddMPL[0]["std_dist"] == null || AddMPL[0]["std_dist"].ToString() == "" ? DBNull.Value : AddMPL[0]["std_dist"];

                        pdsDs.Tables["pdsDs"].GetChanges();
                        pdsda.Update(pdsDs, "pdsDs");
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = MplNo });
        }

        public ActionResult GetUPC(string upc)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            if (upc.Contains("'"))
            {
                upc = upc.Replace("'", "''");
            }
            sql = " select upc from product where upc='" + upc + "' ";
            Ggrp.Getdataset(sql, "FillUPC", ds, ref da, Sconn);
            List<Hashtable> FillUPC = new List<Hashtable>();
            FillUPC = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillUPC"]);
            return Json(new { Items = FillUPC });
        }

        public ActionResult DeleteMPL(string mplNo)
        {
            string DelSQL = "";
            Sconn = new SqlConnection(sqlcon);
            DelSQL = "Delete from mfgMpl where MplNo=" + mplNo;
            Ggrp.Execute(DelSQL, Sconn);
            return Json(new { Items = "" });
        }

        #endregion

        #region UPC
        public ActionResult UPC(string No, string ScreenName)
        {
            ViewBag.No = No;
            ViewBag.ScreenName = ScreenName;
            return View();
        }

        public ActionResult FillMfgUPCGrid(string mfgNo, string MfgUpc)
        {
            string sql = "";
            int rowindex = 0;

            Sconn = new SqlConnection(sqlcon);
            sql = " select * from mfgupc where mfgno='" + mfgNo + "' order by upcparent,mfgupc";
            Ggrp.Getdataset(sql, "FillMfgUPCGrid", ds, ref da, Sconn);
            List<Hashtable> FillMfgUPCGrid = new List<Hashtable>();
            FillMfgUPCGrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["FillMfgUPCGrid"]);

            if (ds.Tables["FillMfgUPCGrid"].Rows.Count > 0 && !string.IsNullOrEmpty(MfgUpc))
            {
                var rowlist = ds.Tables["FillMfgUPCGrid"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["FillMfgUPCGrid"].Select().ToList()
                                   where dr["MfgUpc"].ToString() == MfgUpc
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }

            return Json(new { Items = FillMfgUPCGrid, rowindex = rowindex });
        }

        public ActionResult ValidateMfgUPC(string mfgupc, string mfgno, bool AddMode, string numbermfg)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (AddMode == true)
            {
                lSql = "select mfgupc from mfgupc where Mfgno='" + mfgno + "' and mfgupc='" + mfgupc + "'";
            }
            else
                lSql = "select mfgupc from mfgupc where Mfgno='" + mfgno + "' and mfgupc!='" + numbermfg + "' and mfgupc='" + mfgupc + "'";
            Ggrp.Getdataset(lSql, "GetMfgUPC", ds, ref da, Sconn);
            if (ds.Tables["GetMfgUPC"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult SaveMfgUPC(string ObjMfgUPCList = "", bool IsAddMode = false, string numbermfg = "")
        {
            string Sql = ""; int NewMfno = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjMfgUPCList = "[" + ObjMfgUPCList + "]";
            List<Hashtable> AddMfgUPC = jss.Deserialize<List<Hashtable>>(ObjMfgUPCList);
            DataSet mds = new DataSet();
            SqlDataAdapter mda = new SqlDataAdapter();
            DataRow drAdd;

            if (AddMfgUPC.Count > 0)
            {
                try
                {
                    mds = new DataSet();
                    if (IsAddMode == true)
                    {
                        Sql = "select * from mfgupc where 1=0";
                    }
                    else
                    {
                        Sql = "select * from mfgupc where mfgno='" + AddMfgUPC[0]["mfgno"].ToString() + "' and mfgupc='" + numbermfg + "'";
                    }
                    mda = new SqlDataAdapter(Sql, Sconn);
                    Ggrp.Getdataset(Sql, "tblmfgupc", mds, ref mda, Sconn);
                    if (IsAddMode == true)
                    {
                        drAdd = mds.Tables["tblmfgupc"].NewRow();
                    }
                    else
                    {
                        drAdd = mds.Tables["tblmfgupc"].Rows[0];
                    }
                    if (AddMfgUPC[0]["mfgno"] != null && AddMfgUPC[0]["mfgno"].ToString() != "")
                        drAdd["mfgno"] = AddMfgUPC[0]["mfgno"].ToString();
                    if (AddMfgUPC[0]["refcomp"] != null && AddMfgUPC[0]["refcomp"].ToString() != "")
                        drAdd["refcomp"] = AddMfgUPC[0]["refcomp"].ToString();
                    else
                        drAdd["refcomp"] = DBNull.Value;
                    if (AddMfgUPC[0]["mfgupc"] != null && AddMfgUPC[0]["mfgupc"].ToString() != "")
                        drAdd["mfgupc"] = AddMfgUPC[0]["mfgupc"].ToString();
                    drAdd["upcparent"] = AddMfgUPC[0]["upcparent"].ToString();
                    drAdd["mcodea"] = AddMfgUPC[0]["mcodea"].ToString();
                    drAdd["mcodeb"] = AddMfgUPC[0]["mcodeb"].ToString();

                    if (IsAddMode == true)
                    {
                        mds.Tables["tblmfgupc"].Rows.Add(drAdd);
                    }
                    else
                    {
                        mds.Tables["tblmfgupc"].GetChanges();
                    }
                    mda.Update(mds, "tblmfgupc");
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { Items = NewMfno });
        }

        #endregion

        #region Products

        public ActionResult Products()
        {
            return View();
        }


        public ActionResult GetMfgNameByMfgNo(string mfgNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = "select * from mfg where mfgno='" + mfgNo + "'";
            Ggrp.Getdataset(SQl, "tblmfgname", ds, ref da, Sconn);
            List<Hashtable> MfgList = new List<Hashtable>();
            MfgList = Ggrp.CovertDatasetToHashTbl(ds.Tables["tblmfgname"]);
            return Json(new { Items = MfgList });
        }
        
        public ActionResult ProductsearchList(string searchtext, string selectedvalue)
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
            lWhere = "" + selectedvalue + " like '" + searchtext + "%'";
            List<Hashtable> ProductList = new List<Hashtable>();
            if (searchtext == "%")
            {
                string sSQL = "select top 50 m.company,p.* from product p left join mfg m on p.mfgno=m.mfgno where " + lWhere;
                Ggrp.Getdataset(sSQL, "Fillproduct", ds, ref da, Sconn);
                ProductList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Fillproduct"]);
            }
            else
            {
                string sSQL = "select top 50 m.company,p.* from product p left join mfg m on p.mfgno=m.mfgno where " + lWhere;
                Ggrp.Getdataset(sSQL, "Fillproduct", ds, ref da, Sconn);
                ProductList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Fillproduct"]);
            }
            var jsonResult = Json(new { Items = ProductList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }


        public ActionResult Fillproducts(string Upc)
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


            Ggrp.Getdataset(SQl, "Products", ds, ref da, Sconn);
            List<Hashtable> ProductsList = new List<Hashtable>();
            ProductsList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Products"]);
            return Json(new { Items = ProductsList });

        }
        public ActionResult ExistUPC(string Upc, bool IsAddMode = false, string productno = "")
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            bool IsexistUpc = false;
            string SQl = "";
            if (IsAddMode == true)
            {
                SQl = "select  * from product where upc='" + Upc + "'";
            }
            else
            {
                SQl = "select  * from product where upc='" + Upc + "' and productno!='" + productno + "'";
            }
            Ggrp.Getdataset(SQl, "ExistsUpc", ds, ref da, Sconn);
            if (ds != null && ds.Tables["ExistsUpc"].Rows.Count > 0)
            {
                IsexistUpc = true;
            }
            return Json(new { Items = IsexistUpc });
        }

        public ActionResult CheckDigit(string Upc)
        {
            SqlConnection Sconn = new SqlConnection(sqlcon);
            string lastdigit = Upc.Substring(Upc.Length - 1, 1);
            string checkdigit = "", Status = "";
            checkdigit = Ggrp.ValidateUPC(Upc);
            if (checkdigit != lastdigit)
            {
                if (checkdigit.Trim().ToUpper() == "INVALID")
                    Status = "Invalid character";
                else
                    Status = "Check digit failed";
            }
            return Json(new { Items = Status, checkdigit = checkdigit });
        }
        
        public ActionResult SaveProducts(string ObjproductList = "", bool IsAddMode = false)
        {
            string sSQL = ""; 
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjproductList = "[" + ObjproductList + "]";
            List<Hashtable> AddPrdList = jss.Deserialize<List<Hashtable>>(ObjproductList);
            SqlDataAdapter SQlDa = new SqlDataAdapter();
            DataSet productds = new DataSet();
            DataRow drAdd;

            if (AddPrdList.Count > 0)
            {
                try
                {
                    productds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from product  where 1=0";
                    }
                    else
                    {
                        sSQL = "select * from product where productno='" + AddPrdList[0]["ProductNo"].ToString() + "'";
                    }
                    Ggrp.Getdataset(sSQL, "tblProducts", productds, ref SQlDa, Sconn);
                    if (IsAddMode == true)
                    {
                        drAdd = productds.Tables["tblProducts"].NewRow();
                    }
                    else
                    {
                        drAdd = productds.Tables["tblProducts"].Rows[0];
                        drAdd["ProductNo"] = AddPrdList[0]["ProductNo"].ToString();
                    }
                    if (AddPrdList[0]["mfgno"] != null && AddPrdList[0]["mfgno"].ToString() != "")
                        drAdd["mfgno"] = AddPrdList[0]["mfgno"].ToString();
                    else
                        drAdd["mfgno"] = DBNull.Value;
                    drAdd["upc"] = AddPrdList[0]["upc"].ToString();
                    drAdd["mfgpartno"] = AddPrdList[0]["mfgpartno"].ToString();
                    drAdd["pack"] = AddPrdList[0]["pack"].ToString();
                    drAdd["proddesc"] = AddPrdList[0]["proddesc"].ToString();
                    drAdd["proddesc2"] = AddPrdList[0]["proddesc2"].ToString();
                    drAdd["catcode"] = AddPrdList[0]["catcode"].ToString();
                    drAdd["ggdist"] = AddPrdList[0]["ggdist"].ToString();
                    drAdd["new_up"] = AddPrdList[0]["new_up"].ToString();
                    if (AddPrdList[0]["pds_date"] != null && AddPrdList[0]["pds_date"].ToString() != "")
                        drAdd["pds_date"] = AddPrdList[0]["pds_date"].ToString();
                    else
                        drAdd["pds_date"] = DBNull.Value;
                    if (IsAddMode == true)
                    {
                        productds.Tables["tblProducts"].Rows.Add(drAdd);
                    }
                    else
                    {
                        productds.Tables["tblProducts"].GetChanges();
                    }

                    SqlCommandBuilder cb = new SqlCommandBuilder(SQlDa);
                    SQlDa.UpdateCommand = cb.GetUpdateCommand();

                    SQlDa.Update(productds, "tblProducts");
                    

                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = "" });
        }

        public ActionResult DeleteProducts(string Upc)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from product where  upc ='" + Upc + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }

        #endregion

        #region Sales

        public ActionResult Sales(string Upc = "", string ScreenName = "")
        {
            ViewBag.UPC = Upc;
            ViewBag.ScreenName = ScreenName;
            Sconn = new SqlConnection(sqlcon);
            string sql = "select ym1,ym2 from OP_system";
            Ggrp.Getdataset(sql, "System", ds, ref da, Sconn);
            if (ds != null && ds.Tables["System"].Rows.Count > 0)
            {
                ViewBag.YM1 = ds.Tables["System"].Rows[0]["ym1"].ToString();
                ViewBag.YM2 = ds.Tables["System"].Rows[0]["ym2"].ToString();
            }
            else
            {
                ViewBag.YM1 = "";
                ViewBag.YM2 = "";
            }
            return View();
        }
        public ActionResult FillSalesInfo(string Upc, string periodfrom, string periodto, bool ProductAll = false)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            List<Hashtable> SalesSumarryList = new List<Hashtable>();
            List<Hashtable> SalesDetalisList = new List<Hashtable>();
            SQl = "STP_SalesDisplay '" + Upc + "','" + periodfrom + "','" + periodto + "'";
            Ggrp.Getdataset(SQl, "Sales", ds, ref da, Sconn);
            if (ProductAll == true)
            {
                string ProductAllSQl = "STP_SalesDisplayAllProducts '" + periodfrom + "','" + periodto + "'";
                Ggrp.Getdataset(ProductAllSQl, "Sales", ds, ref da, Sconn);
            }
            SalesSumarryList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Sales"]);
            SalesDetalisList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Sales1"]);
            var jsonResult = Json(new { Items = SalesSumarryList, Itemdetail = SalesDetalisList }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion

        #region Purchase

        public ActionResult Purchase()
        {
            Sconn = new SqlConnection(sqlcon);
            DataSet dsCurr = new DataSet();
            SqlDataAdapter daCurr = new SqlDataAdapter();
            string sql = "select ym1,ym2,curr_dt from OP_system";
            Ggrp.Getdataset(sql, "System", dsCurr, ref daCurr, Sconn);
            if (dsCurr != null && dsCurr.Tables["System"].Rows.Count > 0)
            {
                ViewBag.YM1 = dsCurr.Tables["System"].Rows[0]["ym1"].ToString();
                ViewBag.YM2 = dsCurr.Tables["System"].Rows[0]["ym2"].ToString();
            }
            else
            {
                ViewBag.YM1 = "";
                ViewBag.YM2 = "";
            }

            return View();
        }
        public ActionResult FillPurchaseInfo(string Upc, string ParentUpc, string periodfrom, string periodto, bool ProductAll = false)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            List<Hashtable> PurchaseSumarryList = new List<Hashtable>();
            List<Hashtable> PurchaseDetalisList = new List<Hashtable>();
            SQl = "STP_PurchaseDisplay '" + Upc + "','" + ParentUpc + "','" + periodfrom + "','" + periodto + "'";
            Ggrp.Getdataset(SQl, "Purchase", ds, ref da, Sconn);
            if (ProductAll == true)
            {
                string ProductAllSQl = "STP_PurchaseDisplayAllProducts '" + periodfrom + "','" + periodto + "'";
                Ggrp.Getdataset(ProductAllSQl, "Purchase", ds, ref da, Sconn);
            }
            PurchaseSumarryList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Purchase"]);
            PurchaseDetalisList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Purchase1"]);
            return Json(new { Items = PurchaseSumarryList, Itemdetail = PurchaseDetalisList });
        }
        #endregion

        #region PDS

        public ActionResult ProductDataSheet()
        {
            return View();
        }

        public ActionResult ProductDataSheet1(string Upc = "")
        {
            ViewBag.PDSUPC = Upc;
            return View();
        }

        public ActionResult ProductDataSheet2(string Upc = "")
        {
            ViewBag.PDSUPC = Upc;
            return View();
        }

        public ActionResult ProductDataSheet3(string Upc = "")
        {
            ViewBag.PDSUPC = Upc;
            return View();
        }



        public ActionResult FillPds(string Upc)
        {
            string SQL = "", sql = "", tablename = "";
            DataSet ds1 = new DataSet();
            Sconn = new SqlConnection(sqlcon);
            List<Hashtable> PdsList = new List<Hashtable>();
            SQL = "select * from pds where upc='" + Upc + "'";
            Ggrp.Getdataset(SQL, "pds", ds, ref da, Sconn);
            PdsList = Ggrp.CovertDatasetToHashTbl(ds.Tables["pds"]);
            if (ds.Tables["pds"].Rows.Count == 0)
            {
                tablename = "product";
                List<Hashtable> PdsList1 = new List<Hashtable>();
                sql = "select * from product where upc='" + Upc + "'";
                Ggrp.Getdataset(sql, "pdsproduct", ds1, ref da, Sconn);
                PdsList1 = Ggrp.CovertDatasetToHashTbl(ds1.Tables["pdsproduct"]);
                return Json(new { Items = PdsList1, tablename = tablename });

            }
            return Json(new { Items = PdsList, tablename = "" });
        }

        public ActionResult Savepds(string objpds, string Scrname)
        {
            string SQL = ""; 
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer js = new JavaScriptSerializer();
            objpds = "[" + objpds + "]";
            List<Hashtable> Addpds = js.Deserialize<List<Hashtable>>(objpds);
            SqlDataAdapter Pda = new SqlDataAdapter();
            DataSet Pds = new DataSet();
            DataRow drpds;
            if (Addpds.Count > 0)
            {
                Pds = new DataSet();
                try
                {

                    SQL = "select * from pds where PdsNo='" + Addpds[0]["PdsNo"].ToString() + "'";
                    Pda = new SqlDataAdapter(SQL, Sconn);
                    Ggrp.Getdataset(SQL, "pdstbl", Pds, ref Pda, Sconn);

                    if (Pds != null && Pds.Tables["pdstbl"].Rows.Count == 0)
                    {
                        SQL = "select * from pds where 1=0";
                        Ggrp.Getdataset(SQL, "pdstbl", Pds, ref Pda, Sconn);
                        drpds = Pds.Tables["pdstbl"].NewRow();
                    }
                    else
                    {
                        drpds = Pds.Tables["pdstbl"].Rows[0];

                    }


                    if (Scrname != null && Scrname == "ProductDataSheet1")
                    {
                        drpds["mfgno"] = Addpds[0]["mfgno"] == null || Addpds[0]["mfgno"].ToString() == "" ? DBNull.Value : Addpds[0]["mfgno"];
                        drpds["mfgname"] = Addpds[0]["mfgname"] != null ? Addpds[0]["mfgname"].ToString().Trim() : "";
                        drpds["prod_desc"] = Addpds[0]["prod_desc"] != null ? Addpds[0]["prod_desc"].ToString() : "";
                        drpds["mfgitem"] = Addpds[0]["mfgitem"] != null ? Addpds[0]["mfgitem"].ToString() : "";
                        drpds["upc"] = Addpds[0]["upc"].ToString();

                        drpds["sugg_retl"] = Addpds[0]["sugg_retl"] != null && Addpds[0]["sugg_retl"].ToString() != "" ? Addpds[0]["sugg_retl"] : DBNull.Value;
                        drpds["sugg_deal"] = Addpds[0]["sugg_deal"] != null && Addpds[0]["sugg_deal"].ToString() != "" ? Addpds[0]["sugg_deal"] : DBNull.Value;
                        drpds["dist_ea"] = Addpds[0]["dist_ea"] != null && Addpds[0]["dist_ea"].ToString() != "" ? Addpds[0]["dist_ea"] : DBNull.Value;
                        drpds["dropship"] = Addpds[0]["dropship"] != null && Addpds[0]["dropship"].ToString() != "" ? Addpds[0]["dropship"] : DBNull.Value;
                        drpds["driveitem"] = Addpds[0]["driveitem"] != null && Addpds[0]["driveitem"].ToString() != "" ? Addpds[0]["driveitem"] : DBNull.Value;
                        drpds["price_a"] = Addpds[0]["price_a"] == null || Addpds[0]["price_a"].ToString() == "" ? DBNull.Value : Addpds[0]["price_a"];
                        drpds["a_desc"] = Addpds[0]["a_desc"] != null ? Addpds[0]["a_desc"].ToString() : "";
                        drpds["price_b"] = Addpds[0]["price_b"] == null || Addpds[0]["price_b"].ToString() == "" ? DBNull.Value : Addpds[0]["price_b"];
                        drpds["b_desc"] = Addpds[0]["b_desc"] != null ? Addpds[0]["b_desc"].ToString() : "";
                        drpds["price_c"] = Addpds[0]["price_c"] == null || Addpds[0]["price_c"].ToString().Trim() == "" ? DBNull.Value : Addpds[0]["price_c"];
                        drpds["c_desc"] = Addpds[0]["c_desc"] != null ? Addpds[0]["c_desc"].ToString() : "";
                        drpds["price_d"] = Addpds[0]["price_d"] == null || Addpds[0]["price_d"].ToString() == "" ? DBNull.Value : Addpds[0]["price_d"];
                        drpds["d_desc"] = Addpds[0]["d_desc"] != null ? Addpds[0]["d_desc"].ToString() : "";
                        drpds["price_e"] = Addpds[0]["price_e"] == null || Addpds[0]["price_e"].ToString() == "" ? DBNull.Value : Addpds[0]["price_e"];
                        drpds["e_desc"] = Addpds[0]["e_desc"] == null || Addpds[0]["e_desc"].ToString() == "" ? DBNull.Value : Addpds[0]["e_desc"];
                        drpds["lastupdt"] = Addpds[0]["lastupdt"] == null || Addpds[0]["lastupdt"].ToString() == "" ? DBNull.Value : Addpds[0]["lastupdt"];
                        drpds["eff_date"] = Addpds[0]["eff_date"] == null || Addpds[0]["eff_date"].ToString() == "" ? DBNull.Value : Addpds[0]["eff_date"];
                        drpds["newchng"] = Addpds[0]["newchng"] == null || Addpds[0]["newchng"].ToString() == "" ? DBNull.Value : Addpds[0]["newchng"];
                    }

                    else
                    {
                        drpds["mfgno"] = Addpds[0]["mfgno"] == null || Addpds[0]["mfgno"].ToString() == "" ? DBNull.Value : Addpds[0]["mfgno"];
                        drpds["mfgname"] = Addpds[0]["mfgname"].ToString().Trim();
                        drpds["prod_desc"] = Addpds[0]["prod_desc"].ToString();
                        drpds["mfgitem"] = Addpds[0]["mfgitem"].ToString();
                        drpds["upc"] = Addpds[0]["upc"].ToString();

                        drpds["cp_upc"] = Addpds[0]["cp_upc"] == null || Addpds[0]["cp_upc"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_upc"];
                        drpds["mp_upc"] = Addpds[0]["mp_upc"] == null || Addpds[0]["mp_upc"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_upc"];
                        drpds["cp_dpth"] = Addpds[0]["cp_dpth"] == null || Addpds[0]["cp_dpth"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_dpth"];
                        drpds["mp_dpth"] = Addpds[0]["mp_dpth"] == null || Addpds[0]["mp_dpth"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_dpth"];
                        drpds["cp_qty"] = Addpds[0]["cp_qty"] == null || Addpds[0]["cp_qty"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_qty"];
                        drpds["mp_qty"] = Addpds[0]["mp_qty"] == null || Addpds[0]["mp_qty"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_qty"];
                        drpds["cp_cube"] = Addpds[0]["cp_cube"] == null || Addpds[0]["cp_cube"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_cube"];
                        drpds["mp_cube"] = Addpds[0]["mp_cube"] == null || Addpds[0]["mp_cube"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_cube"];
                        drpds["cp_palqty"] = Addpds[0]["cp_palqty"] == null || Addpds[0]["cp_palqty"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_palqty"];
                        drpds["mp_palqty"] = Addpds[0]["mp_palqty"] == null || Addpds[0]["mp_palqty"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_palqty"];


                        drpds["cp_wt"] = Addpds[0]["cp_wt"] == null || Addpds[0]["cp_wt"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_wt"];
                        drpds["mp_wt"] = Addpds[0]["mp_wt"] == null || Addpds[0]["mp_wt"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_wt"];
                        drpds["cp_ht"] = Addpds[0]["cp_ht"] == null || Addpds[0]["cp_ht"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_ht"];
                        drpds["mp_ht"] = Addpds[0]["mp_ht"] == null || Addpds[0]["mp_ht"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_ht"];
                        drpds["cp_wth"] = Addpds[0]["cp_wth"] == null || Addpds[0]["cp_wth"].ToString() == "" ? DBNull.Value : Addpds[0]["cp_wth"];
                        drpds["mp_wth"] = Addpds[0]["mp_wth"] == null || Addpds[0]["mp_wth"].ToString() == "" ? DBNull.Value : Addpds[0]["mp_wth"];
                        drpds["h_hazmat"] = Addpds[0]["h_hazmat"] == null || Addpds[0]["h_hazmat"].ToString() == "" ? DBNull.Value : Addpds[0]["h_hazmat"];
                        drpds["h_importc"] = Addpds[0]["h_importc"] == null || Addpds[0]["h_importc"].ToString() == "" ? DBNull.Value : Addpds[0]["h_importc"];
                        drpds["h_msdsreq"] = Addpds[0]["h_msdsreq"] == null || Addpds[0]["h_msdsreq"].ToString() == "" ? DBNull.Value : Addpds[0]["h_msdsreq"];
                        drpds["h_eparegs"] = Addpds[0]["h_eparegs"] == null || Addpds[0]["h_eparegs"].ToString() == "" ? DBNull.Value : Addpds[0]["h_eparegs"];
                        drpds["h_nmfc"] = Addpds[0]["h_nmfc"] == null || Addpds[0]["h_nmfc"].ToString() == "" ? DBNull.Value : Addpds[0]["h_nmfc"];
                        drpds["h_st_rstr"] = Addpds[0]["h_st_rstr"] == null || Addpds[0]["h_st_rstr"].ToString() == "" ? DBNull.Value : Addpds[0]["h_st_rstr"];

                    }

                    if (Pds != null && Pds.Tables["pdstbl"].Rows.Count == 0)
                    {
                        Pds.Tables["pdstbl"].Rows.Add(drpds);
                    }
                    else
                    {
                        Pds.Tables["pdstbl"].GetChanges();
                    }
                    Pda.Update(Pds, "pdstbl");
                }

                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }

            return Json(new { Items = "" });
        }


        #endregion

        #region Roomlist

        public ActionResult Roomlist(string No, string ScreenName)
        {
            ViewBag.No = No;
            ViewBag.ScreenName = ScreenName;
            return View();
        }

        public ActionResult FillRoomlistgrid(string mfgno, string MeetId = "")
        {
            string sql = "";
            int rowindex = 0;
            Sconn = new SqlConnection(sqlcon);

            if (Session["roomlistfrom"].ToString() == "Distributor")
            {
                sql = " select MeetingId,MtgNo,R.remarks,ltrim(rtrim(R.LateCheckout))as LateCheckout,R.DlistContactNo as MfgContactNo,R.DlistNo as MfgNo,roomno,roommate,type";
                sql += " ,nights,arrive,depart,code,share,sessionno,Confirmation,m.company,c.nfirst + ' ' + nlast as ContactName,";
                sql += " c.nfirst as FirstName,nlast as LastName,c.ntitle as Title,nemail as Email  from Roomlist R left join Distributor m on R.DlistNo = m.DistNo ";
                sql += " left join DistributorContact C on R.DlistContactNo = C.contactno and r.DlistNo = C.DistNo where R.DlistNo = '" + mfgno + " '";

            }
            else
            {
                sql = "select MeetingId,MtgNo,R.MfgNo,R.remarks,ltrim(rtrim(R.LateCheckout))as LateCheckout,R.DlistContactNo,R.DlistNo,MfgContactNo,roomno,roommate,type";
                sql += " ,nights,arrive,depart,code,share,sessionno,Confirmation,m.company,c.nfirst +' '+nlast as ContactName ,";
                sql += "c.nfirst as FirstName,nlast as LastName,c.ntitle as Title,nemail as Email from Roomlist R left join mfg m on R.MfgNo = m.MfgNo left join mfgContact C on ";
                sql += " R.MfgNo= c.MfgNo and R.MfgContactNo = c.contactno where R.mfgno = '" + mfgno + "'";
            }
            Ggrp.Getdataset(sql, "roomlist", ds, ref da, Sconn);
            List<Hashtable> Roomlistgrid = new List<Hashtable>();
            Roomlistgrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["roomlist"]);
            if (ds.Tables["roomlist"].Rows.Count > 0 && !string.IsNullOrEmpty(MeetId))
            {
                var rowlist = ds.Tables["roomlist"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["roomlist"].Select().ToList()
                                   where dr["MeetingId"].ToString().Trim() == MeetId
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = Roomlistgrid, rowindex = rowindex });
        }

        public ActionResult FillMeetinglistgrid(string meetingcode, string Date)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = " select  meetingcode,Startdate,DATEADD(day, 1,Startdate) as depart,datediff(day, Startdate, DATEADD(day, 1,Startdate)) as nights from OP_system";
            Ggrp.Getdataset(SQl, "system", ds, ref da, Sconn);
            DataSet rds = new DataSet();
            if (meetingcode != null && meetingcode.ToString() != "")
                SQl = "STP_RP_RoomListNightSummary '" + meetingcode + "','" + this.ds.Tables["system"].Rows[0]["Startdate"] + "'";
            else
                SQl = "STP_RP_RoomListNightSummary '" + this.ds.Tables["system"].Rows[0]["meetingcode"] + "','" + this.ds.Tables["system"].Rows[0]["Startdate"] + "'";
            Ggrp.Getdataset(SQl, "Results", rds, ref da, Sconn);
            List<Hashtable> Roomlistgrid = new List<Hashtable>();
            Roomlistgrid = Ggrp.CovertDatasetToHashTbl(rds.Tables["Results"]);
            List<Hashtable> Roomlistgrid2 = new List<Hashtable>();
            Roomlistgrid2 = Ggrp.CovertDatasetToHashTbl(rds.Tables["Results1"]);
            Session["dsReport"] = null;
            Session["dsReport"] = rds;

            string mtgcode = ds.Tables["system"].Rows[0]["meetingcode"].ToString();

            return Json(new { Items = Roomlistgrid, Items2 = Roomlistgrid2, MeetCode = mtgcode });
            
        }

        public ActionResult FillMeetinglistgridwithsession(string meetingcode, string Date, string Sessionno, string Type)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = " select  meetingcode,Startdate,DATEADD(day, 1,Startdate) as depart,datediff(day, Startdate, DATEADD(day, 1,Startdate)) as nights from OP_system";
            Ggrp.Getdataset(SQl, "system", ds, ref da, Sconn);
            if (meetingcode != null && meetingcode.ToString() != "")
                SQl = "STP_RP_RoomListNightSummary '" + meetingcode + "','" + this.ds.Tables["system"].Rows[0]["Startdate"] + "','" + Sessionno + "'";
            else
                SQl = "STP_RP_RoomListNightSummary '" + this.ds.Tables["system"].Rows[0]["meetingcode"] + "','" + this.ds.Tables["system"].Rows[0]["Startdate"] + "','" + Sessionno + "'";
            Ggrp.Getdataset(SQl, "Results", ds, ref da, Sconn);
            List<Hashtable> Roomlistgrid = new List<Hashtable>();
            Roomlistgrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["Results"]);
            if (Type != null && Type != "")
            {
                DataView dv = ds.Tables["Results"].DefaultView;
                dv.RowFilter = "CompanyType= '" + Type + "'";
                DataTable dtMenu = dv.ToTable();
                ds.Tables.Remove(ds.Tables["Results"]);
                ds.Tables.Add(dtMenu);
                Roomlistgrid = Ggrp.CovertDatasetToHashTbl(dtMenu);
            }
            List<Hashtable> Roomlistgrid2 = new List<Hashtable>();
            Roomlistgrid2 = Ggrp.CovertDatasetToHashTbl(ds.Tables["Results1"]);
            Session["dsReport"] = null;
            Session["dsReport"] = ds;
            return Json(new { Items = Roomlistgrid, Items2 = Roomlistgrid2 });
        }

        public ActionResult saveroomlist(string ObjroomList = "", string MeetingId = "", string MtgNo = "", bool IsAddMode = false)
        {
            string sSQL = ""; int MeetId = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjroomList = "[" + ObjroomList + "]";
            List<Hashtable> Addroom = jss.Deserialize<List<Hashtable>>(ObjroomList);
            DataSet rds = new DataSet();
            SqlDataAdapter rda = new SqlDataAdapter();
            DataRow drAdd;

            if (Addroom.Count > 0)
            {

                try
                {
                    rds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from roomlist where 1=0";
                    }
                    else
                    {
                        sSQL = "select * from roomlist where MeetingId='" + Addroom[0]["MeetingId"].ToString() + "'";
                    }
                    Ggrp.Getdataset(sSQL, "tblroom", rds, ref rda, Sconn, false);

                    if (IsAddMode == true)
                    {
                        drAdd = rds.Tables["tblroom"].NewRow();
                    }
                    else
                    {
                        drAdd = rds.Tables["tblroom"].Rows[0];
                        drAdd["MeetingId"] = Addroom[0]["MeetingId"].ToString();
                    }
                    drAdd["MtgNo"] = Addroom[0]["MtgNo"].ToString();

                    if (Session["roomlistfrom"].ToString() == "Distributor")
                    {
                        drAdd["DlistNo"] = Addroom[0]["mfgno"].ToString();
                        if (Addroom[0]["contactno"] != null && Addroom[0]["contactno"].ToString() != "")
                            drAdd["DlistContactNo"] = Addroom[0]["contactno"].ToString();
                    }
                    else
                    {
                        drAdd["mfgno"] = Addroom[0]["mfgno"].ToString();
                        if (Addroom[0]["contactno"] != null && Addroom[0]["contactno"].ToString() != "")
                            drAdd["MfgContactNo"] = Addroom[0]["contactno"].ToString();

                    }

                    drAdd["roommate"] = Addroom[0]["roommate"].ToString().Trim();
                    drAdd["share"] = Addroom[0]["sharing"].ToString().Trim();

                    if (Addroom[0]["arrive"] != null && Addroom[0]["arrive"].ToString().Trim() != "")
                        drAdd["arrive"] = Addroom[0]["arrive"].ToString().Trim();
                    else
                        drAdd["arrive"] = DBNull.Value;
                    if (Addroom[0]["nights"] != null && Addroom[0]["nights"].ToString().Trim() != "")
                        drAdd["nights"] = Addroom[0]["nights"].ToString().Trim();
                    else
                        drAdd["nights"] = DBNull.Value;
                    if (Addroom[0]["depart"] != null && Addroom[0]["depart"].ToString().Trim() != "")
                        drAdd["depart"] = Addroom[0]["depart"].ToString().Trim();
                    else
                        drAdd["depart"] = DBNull.Value;
                    if (Addroom[0]["typeroom"] != null && Addroom[0]["typeroom"].ToString().Trim() != "")
                        drAdd["type"] = Addroom[0]["typeroom"].ToString().Trim();
                    else
                        drAdd["type"] = DBNull.Value;
                    if (Addroom[0]["roomno"] != null && Addroom[0]["roomno"].ToString().Trim() != "")
                        drAdd["roomno"] = Addroom[0]["roomno"].ToString().Trim();
                    else
                        drAdd["roomno"] = DBNull.Value;

                    drAdd["Confirmation"] = Addroom[0]["confno"].ToString().Trim();
                    drAdd["sessionno"] = Addroom[0]["sessno"].ToString().Trim();
                    if (Addroom[0]["remarks"] != null && Addroom[0]["remarks"].ToString().Trim() != "")
                        drAdd["remarks"] = Addroom[0]["remarks"].ToString().Trim();
                    else
                        drAdd["remarks"] = DBNull.Value;

                    drAdd["LateCheckout"] = Addroom[0]["latecheck"].ToString().Trim();

                    if (IsAddMode == true)
                    {
                        rds.Tables["tblroom"].Rows.Add(drAdd);
                    }
                    else
                    {
                        rds.Tables["tblroom"].GetChanges();
                    }
                    rda.Update(rds, "tblroom");

                    if (IsAddMode == true)
                    {
                        MeetId = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        MeetId = Convert.ToInt32(Addroom[0]["MeetingId"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });

                }
            }
            return Json(new { Items = MeetId });
        }

        public ActionResult DeleteRoomlist(string meetcode)
        {
            string SQL = "";
            Sconn = new SqlConnection(sqlcon);
            SQL = "delete from roomlist where MeetingId='" + meetcode + "'";
            Ggrp.Execute(SQL, Sconn);

            return Json(new { Items = "" });
        }

        public ActionResult MfgContactList(string MfgNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            if (Session["roomlistfrom"].ToString() == "Distributor")
            {
                SQl = "select '' as contactno,'' as contactname union select  ltrim(rtrim(contactno)) as contactno ,isnull(nfirst, '') + ' ' + isnull(nlast, '') as contactname ";
                SQl += " from DistributorContact where DistNo = '" + MfgNo + "'";
            }
            else
            {
                SQl = "select '' as contactno,'' as contactname union select  ltrim(rtrim(contactno))as contactno ,isnull(nfirst,'') + ' ' + isnull(nlast,'') as contactname ";
                SQl += " from mfgcontact where mfgno='" + MfgNo + "'";
            }
            Ggrp.Getdataset(SQl, "contact", ds, ref da, Sconn);
            List<Hashtable> ContactList = new List<Hashtable>();
            ContactList = Ggrp.CovertDatasetToHashTbl(ds.Tables["contact"]);
            return Json(new { Items = ContactList });
        }

        public ActionResult GetRoomlistContactInfo(string mfgNo, string contactNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            if (Session["roomlistfrom"].ToString() == "Distributor")
            {
                SQl = "select * from DistributorContact where DistNo = '" + mfgNo + "' and contactno='" + contactNo + "'";
            }
            else
            {
                SQl = "select * from mfgcontact where mfgno='" + mfgNo + "' and contactno='" + contactNo + "'";
            }
            Ggrp.Getdataset(SQl, "contactinfo", ds, ref da, Sconn);
            List<Hashtable> ContactList = new List<Hashtable>();
            ContactList = Ggrp.CovertDatasetToHashTbl(ds.Tables["contactinfo"]);
            return Json(new { Items = ContactList });
        }

        public ActionResult exportsummarytable(string meetingcode, string Date)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = "STP_RP_RoomListNightSummary '" + meetingcode + "','" + Date + "'";
            Ggrp.Getdataset(SQl, "Results", ds, ref da, Sconn);
            List<Hashtable> Roomlistgrid = new List<Hashtable>();
            Roomlistgrid = Ggrp.CovertDatasetToHashTbl(ds.Tables["Results"]);
            List<Hashtable> Roomlistgrid2 = new List<Hashtable>();
            Roomlistgrid2 = Ggrp.CovertDatasetToHashTbl(ds.Tables["Results1"]);


            string rootpath = "", file_name = "", filepath = "";
            rootpath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "ExportFiles\\";
            file_name = "Summary" + "_" + DateTime.Now.ToString("MM_dd_yyy_HH_mm_ss") + ".csv";
            filepath = rootpath + file_name.ToString().Trim();

            bool blnFlag = false;
            blnFlag = CreateExcelFile.CreateExcelDocument(ds, filepath);

            Session["SummaryFile_path"] = filepath;
            Session["Filetext"] = "Summary";
            return Json(new { Items = "success" });
        }

        public ActionResult GetFileFromDisk()
        {
            string filename = "", filepath = "", filetype = "";
            filepath = Session["SummaryFile_path"].ToString();
            filename = filepath.Substring(filepath.IndexOf(Session["Filetext"].ToString()));
            filetype = ".csv";
            if (filetype == "MHT") { return Json(new { Items = "" }); }
            var bytes = System.IO.File.ReadAllBytes(filepath);
            System.IO.File.Delete(filepath);
            return File(bytes, filetype, filename);
        }

        public ActionResult GetCompanyType()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = "select '' as  CompanyType union select  distinct(CASE WHEN MfgNo is not null THEN 'M'  ELSE 'D' END) AS CompanyType from roomlist";
            Ggrp.Getdataset(SQl, "companytype", ds, ref da, Sconn);
            List<Hashtable> CompanyTypeList = new List<Hashtable>();
            CompanyTypeList = Ggrp.CovertDatasetToHashTbl(ds.Tables["companytype"]);
            return Json(new { Items = CompanyTypeList });
        }

        #endregion

        #region ScreenReport

        public ActionResult FillMfrReport(string Repname, string Description, string Prim, string Sec, string ClassCd, string Mfgno)
        {
            string SQL = "";
            Sconn = new SqlConnection(sqlcon);
            DataSet mds = new DataSet();
            Session["dsReport"] = null;
            if (Repname == "RP_MfrDataSheet.repx" || Repname == "RP_MfrDataSheetwithSign.repx")
            {
                SQL = "STP_RP_MfgDataSheet '" + Prim + "','" + Sec + "','" + ClassCd + "','" + Mfgno + "'";
                Ggrp.Getdataset(SQL, "Results", mds, ref da, Sconn);
            }

            if (Repname == "RP_Prgoinfo.repx" || Repname == "RP_BpoSign.repx")
            {
                string sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,
                        OP.phone as GGphone, p.*,m.company,'PY'+REPLACE(BP.period, '-', '/') as PrgPeriod,
                        RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progfrom)), 2)+'/'+RIGHT('00' + CONVERT(NVARCHAR(2), 
                        DATEPART(DAY, BP.progfrom)), 2)+' - '+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progto)), 2)+'/'+ RIGHT('00' + CONVERT(NVARCHAR(2), 
                        DATEPART(DAY, BP.progto)), 2) as Term,lk.descript as BpoStatus from mfgprginfo p 
                        left join mfg m on p.mfgno=m.mfgno 
                        left join  op_system OP on 1=1 left join op_lookup lk on lk.entry=p.bpo_stat and lookupid='BPOStat'
                        left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system)  where m.mfgno = '" + Mfgno + "'";
                Ggrp.Getdataset(sql, "Results", mds, ref da, Sconn);
            }
            if (Repname == "RP_ProgRefGuide.repx" || Repname == "RP_ProgRefGuidewithSign.repx")
            {
                string sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,OP.Phone as GGphone,'PY'+REPLACE(BP.period, '-', '/') as period,  
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
                            left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system) where m.mfgno='" + Mfgno + "'";
                Ggrp.Getdataset(sql, "Results", mds, ref da, Sconn);
            }

            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            Session["dsReport"] = mds;
            Session["Description"] = Description;
            string RPTFileName = Repname;
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });

        }

        public ActionResult FillAllMfrReports(string Repname, string Description, string Prim, string Sec, string ClassCd, string Mfgno, bool MfrSign)
        {
            string SQL = "";
            Sconn = new SqlConnection(sqlcon);
            DataSet mds = new DataSet();
            Session["dsReport"] = null;
            SQL = "STP_RP_MfgDataSheet '" + Prim + "','" + Sec + "','" + ClassCd + "','" + Mfgno + "'";
            Ggrp.Getdataset(SQL, "Results", mds, ref da, Sconn);
            string sql = @"select OP.company as GGCompany,OP.address1 as GGaddress,(rtrim(OP.city)+', '+ OP.state+' '+OP.zip) as GGCSZ,OP.Phone as GGphone, p.*,m.french,m.spanish,m.cnreg,m.cnna,m.bilinpkg, m.youtube,m.facebook,m.twitter,m.eprodinfo,m.elineart,m.eadcopy,m.wprodinfo,
                m.wlineart,m.wadcopy,m.factreps,m.mfrreps,m.company,'PY'+REPLACE(BP.period, '-', '/') as PrgPeriod,
                RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progfrom)), 2)+'/'+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(DAY, BP.progfrom)), 2)+' - '+RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(month, BP.progto)), 2)+'/'+ RIGHT('00' + CONVERT(NVARCHAR(2), DATEPART(DAY, BP.progto)), 2) as Term from mfgprginfo p 
                left join mfg m on p.mfgno=m.mfgno left join  op_system OP on 1=1
                left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system) where m.mfgno='" + Mfgno + "'";
            Ggrp.Getdataset(sql, "Results2", mds, ref da, Sconn);
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
                            left join BenefitProgram Bp on Bp.mfgno=m.mfgno and Bp.program='BHC' and BP.period =(Select ProgramYear from OP_system) where m.mfgno='" + Mfgno + "'";
            Ggrp.Getdataset(sQl, "Results3", mds, ref da, Sconn);
            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            Session["dsReport"] = mds;
            Session["Description"] = Description;
            Session["MdsBpoPrgSign"] = MfrSign;
            string prmtitle = "";
            if (MfrSign)
                prmtitle = "MdsBpoPrgWithSign";
            else
                prmtitle = "MdsBpoPrgWithOutSign";
            Session["ParamTitle"] = prmtitle;
            string RPTFileName = Repname;
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });
        }

        public ActionResult FillReport(string Repname)
        {
            DataSet lds = (DataSet)Session["dsReport"];
            Session["SReportCache"] = null;
            Session["Report_FilterFields"] = null;
            Session["Description"] = "";
            Session["dsReport"] = lds;
            string RPTFileName = Repname;
            Session["ReportName"] = Server.MapPath("~/DevExpReports/") + RPTFileName;
            return Json(new { Items = "" });

        }
        public ActionResult ReportViewerPartial(string guid)
        {
            if (!string.IsNullOrEmpty(Session["dsReport"+ guid].ToString()))
            {
                ViewData["Report"] = GetReport(guid);
            }
            else
                ViewData["Report"] = new XtraReport1();
            return PartialView("FormReportViewerPartial");
        }
        public ActionResult FormReport(string GuId="")
        {
            if (GuId == "")
            {
                string guid = Guid.NewGuid().ToString();
                ViewBag.GUID = guid;
                Session["ReportName" + guid] = Session["ReportName"].ToString();
                Session["dsReport" + guid] = Session["dsReport"];
                Session["Description" + guid] = Session["Description"];
                Session["SReportCache" + guid] = Session["SReportCache"];
            }
            else
            {
                ViewBag.GUID = GuId;
            }
            return View();
        }
        public ActionResult ExportReportViewerPartial(string guid)
        {
            XtraReport exportreport = new XtraReport();
            if (Session["DataTableModel"+ guid] != null)
            {
                exportreport = GetReport(guid);
            }
            Sconn = new SqlConnection(sqlcon);
            if (Session["ReplaceFilter"] == null)
                Session["ReplaceFilter"] = "";
            if (Session["ActivityType"] == null)
                Session["ActivityType"] = "";
            if (Session["Description"] == null)
                Session["Description"] = "";
            if (Session["Filename"] == null)
                Session["Filename"] = "";
            return DevExpress.Web.Mvc.ReportViewerExtension.ExportTo(GetReport(guid));
        }

        MemoryStream ms;
        XtraReport GetReport(string guid="")
        {
            string lFilter = "";
            XtraReport1 report = new XtraReport1(Session["ReportName"+ guid].ToString(), "");
            try
            {
                DataSet dsReport = (DataSet)Session["dsReport" + guid];
                report.DataSourceDemanded += (s, e) =>
                {
                    ((XtraReport)s).DataSource = dsReport.Tables["Results"];
                };
                string Repname = "";
                if (Session["ReportName" + guid].ToString().Contains("DevExpReports"))
                {
                    string[] splitrepname = Session["ReportName" + guid].ToString().Split(new string[] { "DevExpReports\\" }, StringSplitOptions.None);
                    Repname = splitrepname[1];
                }
                if (Session["SReportCache" + guid] == null)
                {
                    foreach (DevExpress.XtraReports.Parameters.Parameter ffd in report.Parameters)
                    {
                        if (ffd.Name.ToString().ToUpper() == "CLUBID")
                        {
                            ffd.Value = "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "FILTER")
                        {
                            if (Session["lwhereParameter" + guid] != null)
                                lFilter = Session["lwhereParameter" + guid].ToString();
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
                        if (ffd.Name.ToString().ToUpper() == "LOCATIONID")
                        {
                            ffd.Value = (Session["location"] != null && Session["location"].ToString() != "") ? Session["location"].ToString() : "";
                        }
                        if (ffd.Name.ToString().ToUpper() == "REPORTNAME")
                        {
                            ffd.Value = Session["Description"].ToString();
                        }
                        if (ffd.Name.ToString().ToUpper() == "PARAMTITLE")
                        {
                            ffd.Value = (Session["ParamTitle"] != null && Session["ParamTitle"].ToString() != "") ? Session["ParamTitle"].ToString() : Session["Description"].ToString();
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
                        case "RP_NIGHTSUMMARY.REPX":
                            SubReport_NightSummary(dsReport, report);
                            break;
                        case "RP_MFRDATASHEET.REPX":
                            SubReport_MfrSubReport(dsReport, report);
                            break;
                        case "RP_MFRDATASHEETWITHSIGN.REPX":
                            SubReport_MfrSubReport(dsReport, report);
                            break;
                        case "RP_MDSBPOPRG.REPX":
                            SubReport_MdsBpoPrg(dsReport, report);
                            break;
                    }

                    if (Session["Description" + guid] != null && Session["Description" + guid].ToString() != "")
                    {
                        string defaultfilename = Session["Description" + guid] + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
                        defaultfilename = Regex.Replace(defaultfilename, "[^0-9a-zA-Z:,]+", " ");
                        defaultfilename = Regex.Replace(defaultfilename, ",", "");
                        report.Name = defaultfilename;
                    }
                    report.CreateDocument();
                    ms = new MemoryStream();
                    PrintingSystemAccessor.SaveIndependentPages(report.PrintingSystem, ms);
                    Session["SReportCache" + guid] = ms;
                }
                else
                {
                    ms = (MemoryStream)Session["SReportCache" + guid];
                    PrintingSystemAccessor.LoadVirtualDocument(report.PrintingSystem, ms);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return report;
        }

        public void SubReport_MdsBpoPrg(DataSet ds, XtraReport xRpt)
        {
            try
            {
                string pbosubrep = "", prgsubrep = "";
                XtraReport subreport1 = new XtraReport();
                string Subrep = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + "RP_mfrSubdatasheet.repx";
                subreport1.LoadLayout(Subrep.ToString());
                subreport1.DataSource = ds.Tables["Results1"];
                ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport1;
                Band bnd = subreport1.Bands.GetBandByType(typeof(PageFooterBand));

                if (Session["MdsBpoPrgSign"].ToString().ToUpper() == "TRUE")
                    pbosubrep = "RP_BpoSign.repx";
                else
                    pbosubrep = "RP_Prgoinfo.repx";

                XtraReport subreport2 = new XtraReport();
                string Subrep2 = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + pbosubrep;
                subreport2.LoadLayout(Subrep2.ToString());
                subreport2.DataSource = ds.Tables["Results2"];
                ((XRSubreport)xRpt.FindControl("subreport2", true)).ReportSource = subreport2;
                Band bnd1 = subreport2.Bands.GetBandByType(typeof(PageFooterBand));
                bnd1.Visible = false;

                if (Session["MdsBpoPrgSign"].ToString().ToUpper() == "TRUE")
                    prgsubrep = "RP_ProgRefGuidewithSign.repx";
                else
                    prgsubrep = "RP_ProgRefGuide.repx";

                XtraReport subreport3 = new XtraReport();
                string Subrep3 = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + prgsubrep;
                subreport3.LoadLayout(Subrep3.ToString());
                subreport3.DataSource = ds.Tables["Results3"];
                ((XRSubreport)xRpt.FindControl("subreport3", true)).ReportSource = subreport3;
                Band bnd2 = subreport3.Bands.GetBandByType(typeof(PageFooterBand));
                bnd2.Visible = false;
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
            }
        }

        public void SubReport_NightSummary(DataSet ds, XtraReport xRpt)
        {
            try
            {
                XtraReport subreport1 = new XtraReport();
                string Subrep = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + "RP_Sub_NightSummary.repx";
                subreport1.LoadLayout(Subrep.ToString());
                subreport1.DataSource = ds.Tables["Results1"];
                ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport1;
                Band bnd = subreport1.Bands.GetBandByType(typeof(PageFooterBand));
                if (bnd != null)
                    bnd.Height = 77;
                subreport1.Margins.Bottom = 10;
                subreport1.Margins.Top = 25;
                subreport1.PaperKind = System.Drawing.Printing.PaperKind.Letter;
                if (subreport1.Landscape == true)
                {
                    subreport1.Margins.Right = 1;
                    subreport1.Margins.Left = 30;
                }
                else
                {
                    subreport1.Margins.Left = 25;
                }
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
            }
        }

        public void SubReport_MfrSubReport(DataSet ds, XtraReport xRpt)
        {
            try
            {
                XtraReport subreport1 = new XtraReport();
                string Subrep = AppDomain.CurrentDomain.BaseDirectory + "DevExpReports\\" + "RP_mfrSubdatasheet.repx";
                subreport1.LoadLayout(Subrep.ToString());
                subreport1.DataSource = ds.Tables["Results1"];
                ((XRSubreport)xRpt.FindControl("subreport1", true)).ReportSource = subreport1;
                Band bnd = subreport1.Bands.GetBandByType(typeof(PageFooterBand));
                if (bnd != null)
                    bnd.Height = 77;
                subreport1.Margins.Bottom = 10;
                subreport1.Margins.Top = 25;
                subreport1.PaperKind = System.Drawing.Printing.PaperKind.Letter;
                if (subreport1.Landscape == true)
                {
                    subreport1.Margins.Right = 1;
                    subreport1.Margins.Left = 30;
                }
                else
                {
                    subreport1.Margins.Left = 25;
                }
            }
            catch (Exception ex)
            {
                string Msg = ex.Message;
            }
        }
        

        #endregion

        #region Distributors

        public ActionResult Distributors()
        {
            Session["roomlistfrom"] = "Distributor";
            return View();
        }

        public ActionResult FillDistGrid(string DistnNo)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            if (string.IsNullOrEmpty(DistnNo))
                sql = "select top 1 * from Distributor order by distno desc";
            else
                sql = "select * from Distributor where distno='" + DistnNo + "'";
            Ggrp.Getdataset(sql, "Distributors", ds, ref da, Sconn);
            List<Hashtable> DList = new List<Hashtable>();
            DList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Distributors"]);

            Session["DistNo"] = DList[0]["DistNo"];
            Session["company"] = DList[0]["company"];
            Session["roomlistfrom"] = "Distributor";

            Session["mfgno"] = "";

            return Json(new { Items = DList });
        }

        public ActionResult ValidateDistCompany(string company, bool AddMode, string distributorno)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (company.Contains("'"))
            {
                company = company.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select company from Distributor where company='" + company + "' ";
            else
                lSql = " select company from Distributor where company = '" + company + "' and DistNo!='" + distributorno + "'";
            Ggrp.Getdataset(lSql, "FillCompany", ds, ref da, Sconn);

            if (ds.Tables["FillCompany"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult SaveDistributor(string ObjDistArray = "", string DistNo = "", bool IsAddMode = false)
        {
            string sSQL = ""; int Distno = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjDistArray = "[" + ObjDistArray + "]";
            List<Hashtable> DistList = jss.Deserialize<List<Hashtable>>(ObjDistArray);
            DataSet DCds = new DataSet();
            SqlDataAdapter DCda = new SqlDataAdapter();
            DataRow drAdd;
            if (DistList.Count > 0)
            {
                try
                {
                    DCds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from Distributor where 1=0";
                    }
                    else
                    {
                        sSQL = "select * from Distributor where distno='" + DistList[0]["DistNo"].ToString() + "'";
                    }
                    Ggrp.Getdataset(sSQL, "tblDistributor", DCds, ref DCda, Sconn, false);
                    if (IsAddMode == true)
                    {
                        drAdd = DCds.Tables["tblDistributor"].NewRow();
                    }
                    else
                    {

                        drAdd = DCds.Tables["tblDistributor"].Rows[0];
                        drAdd["DistNo"] = DistList[0]["DistNo"].ToString();
                    }
                    drAdd["company"] = DistList[0]["company"].ToString();
                    drAdd["mailadd"] = DistList[0]["mailadd"].ToString();
                    drAdd["city"] = DistList[0]["city"].ToString();
                    drAdd["state"] = DistList[0]["state"].ToString();
                    drAdd["zip"] = DistList[0]["zip"].ToString();
                    drAdd["country"] = DistList[0]["country"].ToString();
                    drAdd["phone"] = DistList[0]["phone"].ToString();
                    drAdd["phone2"] = DistList[0]["phone2"].ToString();
                    drAdd["fax"] = DistList[0]["fax"].ToString();
                    drAdd["officehrs"] = DistList[0]["officehrs"].ToString();
                    drAdd["d_email"] = DistList[0]["d_email"].ToString();
                    drAdd["www"] = DistList[0]["www"].ToString();
                    drAdd["corpname"] = DistList[0]["corpname"].ToString();
                    drAdd["shipadd"] = DistList[0]["shipadd"].ToString();
                    drAdd["scity"] = DistList[0]["scity"].ToString();
                    drAdd["sstate"] = DistList[0]["sstate"].ToString();
                    drAdd["szip"] = DistList[0]["szip"].ToString();
                    drAdd["scountry"] = DistList[0]["scountry"].ToString();
                    drAdd["webadd"] = DistList[0]["webadd"].ToString();
                    drAdd["ddtlink"] = DistList[0]["ddtlink"].ToString();
                    drAdd["primary_"] = DistList[0]["primary_"].ToString();
                    drAdd["codea"] = DistList[0]["codea"].ToString();
                    drAdd["parentact"] = DistList[0]["parentact"].ToString();
                    if (DistList[0]["datejoin"] == null || DistList[0]["datejoin"].ToString() == "")
                    {
                        drAdd["datejoin"] = DBNull.Value;
                    }
                    else
                    {
                        drAdd["datejoin"] = Convert.ToDateTime(DistList[0]["datejoin"].ToString());
                    }
                    if (DistList[0]["dateterm"] == null || DistList[0]["dateterm"].ToString() == "")
                    {
                        drAdd["dateterm"] = DBNull.Value;
                    }
                    else
                    {
                        drAdd["dateterm"] = Convert.ToDateTime(DistList[0]["dateterm"].ToString());
                    }

                    if (IsAddMode == true)
                    {
                        DCds.Tables["tblDistributor"].Rows.Add(drAdd);
                    }
                    else
                    {
                        DCds.Tables["tblDistributor"].GetChanges();
                    }
                    DCda.Update(DCds, "tblDistributor");
                    if (IsAddMode == true)
                    {
                        Distno = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        Distno = Convert.ToInt32(DistList[0]["DistNo"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = Distno });
        }

        public ActionResult DeleteDistributor(string DistnNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from Distributor where  DistNo='" + DistnNo + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }
        #endregion

        #region DistContacts

        public ActionResult DistContacts()
        {
            return View();
        }

        public ActionResult FillDistContGrid(string DistnNo, string Contactno = "")
        {
            string sql = ""; var rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = "select * from DistributorContact where DistNo='" + DistnNo + "' order by nseqcd,nfirst,nlast";
            Ggrp.Getdataset(sql, "DistContacts", ds, ref da, Sconn);
            List<Hashtable> DContList = new List<Hashtable>();
            DContList = Ggrp.CovertDatasetToHashTbl(ds.Tables["DistContacts"]);
            if (!string.IsNullOrEmpty(DistnNo) && ds.Tables["DistContacts"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["DistContacts"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["DistContacts"].Select().ToList()
                                   where dr["contactno"].ToString().Trim() == Contactno
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = DContList, rowindex = rowindex });
        }

        public ActionResult ValidateFirstLastName(string FirstName, string LastName, string DistContNo, bool AddMode, string DistNo, string titlecd)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (FirstName.Contains("'"))
            {
                FirstName = FirstName.Replace("'", "''");
            }
            if (LastName.Contains("'"))
            {
                LastName = LastName.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select nfirst,nlast from DistributorContact where nfirst='" + FirstName + "' and nlast='" + LastName + "' and DistNo='" + DistNo + "' and ntitlecd='" + titlecd + "'";
            else
                lSql = " select nfirst,nlast from   DistributorContact where nfirst='" + FirstName + "' and nlast='" + LastName + "' and DistContNo != '" + DistContNo + "' and DistNo='" + DistNo + "' and ntitlecd='" + titlecd + "'";

            Ggrp.Getdataset(lSql, "GetName", ds, ref da, Sconn);
            if (ds.Tables["GetName"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult SaveDistContacts(string ObjDistCntsArray = "", string DistNo = "", bool IsAddMode = false)
        {
            string sSQL = ""; int DContactno = 0; int distno = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjDistCntsArray = "[" + ObjDistCntsArray + "]";
            List<Hashtable> DContactsList = jss.Deserialize<List<Hashtable>>(ObjDistCntsArray);
            DataSet DCds = new DataSet();
            SqlDataAdapter DCda = new SqlDataAdapter();
            DataRow drAdd;
            if (DContactsList.Count > 0)
            {
                try
                {
                    DCds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from DistributorContact where 1=0";
                        string sql = "select max(isnull(contactno,0))+1 as ContactNo from DistributorContact where distno='" + DistNo + "'";
                        Ggrp.Getdataset(sql, "Dcontacts", ds, ref da, Sconn);
                        if (ds != null && ds.Tables["Dcontacts"].Rows.Count > 0)
                        {
                            if (ds.Tables["Dcontacts"].Rows[0]["contactno"].ToString() == "")
                            {
                                DContactno = 1;
                            }
                            else
                            {
                                DContactno = Convert.ToInt32(ds.Tables["Dcontacts"].Rows[0]["contactno"].ToString());
                            }
                        }
                    }
                    else
                    {
                        sSQL = "select * from DistributorContact where distno='" + DContactsList[0]["DistNo"].ToString() + "' and contactno='" + DContactsList[0]["contactno"].ToString() + "'";

                    }
                    Ggrp.Getdataset(sSQL, "tblDContacts", DCds, ref DCda, Sconn);
                    if (IsAddMode == true)
                    {
                        drAdd = DCds.Tables["tblDContacts"].NewRow();
                        drAdd["contactno"] = DContactno;
                        drAdd["DistNo"] = DistNo;
                    }
                    else
                    {
                        drAdd = DCds.Tables["tblDContacts"].Rows[0];
                    }
                    drAdd["nsalut"] = DContactsList[0]["nsalut"].ToString();
                    drAdd["nfirst"] = DContactsList[0]["nfirst"].ToString();
                    drAdd["minitial"] = DContactsList[0]["minitial"].ToString();
                    drAdd["nlast"] = DContactsList[0]["nlast"].ToString();
                    drAdd["ntitle"] = DContactsList[0]["ntitle"].ToString();
                    drAdd["nseqcd"] = DContactsList[0]["nseqcd"].ToString();
                    drAdd["ngreet"] = DContactsList[0]["ngreet"].ToString();
                    drAdd["ntitlecd"] = DContactsList[0]["ntitlecd"].ToString();
                    drAdd["codea"] = DContactsList[0]["codea"].ToString();
                    drAdd["naddress"] = DContactsList[0]["naddress"].ToString();
                    drAdd["ncity"] = DContactsList[0]["ncity"].ToString();
                    drAdd["nstate"] = DContactsList[0]["nstate"].ToString();
                    drAdd["nzip"] = DContactsList[0]["nzip"].ToString();
                    drAdd["country"] = DContactsList[0]["country"].ToString();
                    drAdd["nphone"] = DContactsList[0]["nphone"].ToString();
                    drAdd["n_ext"] = DContactsList[0]["n_ext"] == null ? "" : DContactsList[0]["n_ext"].ToString();
                    drAdd["nphone2"] = DContactsList[0]["nphone2"] == null ? "" : DContactsList[0]["nphone2"].ToString();
                    drAdd["eextn"] = DContactsList[0]["eextn"].ToString();
                    drAdd["cell"] = DContactsList[0]["cell"].ToString();
                    drAdd["nshp_city"] = DContactsList[0]["nshp_city"].ToString();
                    drAdd["nfax"] = DContactsList[0]["nfax"].ToString();
                    drAdd["newsltr"] = DContactsList[0]["newsltr"].ToString() == "True" ? "Y" : "";
                    drAdd["cloudlvl"] = DContactsList[0]["cloudlvl"].ToString();
                    drAdd["nemail"] = DContactsList[0]["nemail"].ToString();
                    drAdd["nshp_st"] = DContactsList[0]["nshp_st"].ToString();
                    drAdd["nshp_zip"] = DContactsList[0]["nshp_zip"].ToString();
                    drAdd["nshp_cntry"] = DContactsList[0]["nshp_cntry"].ToString();
                    if (IsAddMode == true)
                    {
                        DCds.Tables["tblDContacts"].Rows.Add(drAdd);
                    }
                    else
                    {
                        DCds.Tables["tblDContacts"].GetChanges();
                    }
                    DCda.Update(DCds, "tblDContacts");
                    if (IsAddMode == true)
                    {
                        // distno = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        distno = Convert.ToInt32(DContactsList[0]["DistNo"].ToString());
                        DContactno = Convert.ToInt32(DContactsList[0]["contactno"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = DContactno });
        }

        public ActionResult DeleteDistContacts(string Distno, string contactno)
        {
            bool Roomlistverify = false;
            Sconn = new SqlConnection(sqlcon);

            string lsql = "select * from roomlist where dlistno='" + Distno + "' and dlistcontactno='" + contactno + "'";
            Ggrp.Getdataset(lsql, "TblRoomlist", ds, ref da, Sconn);
            if (ds != null && ds.Tables["TblRoomlist"].Rows.Count > 0)
            {
                Roomlistverify = true;
            }
            else
            {
                string SQl = "delete from DistributorContact where  contactno ='" + contactno + "' and distno='" + Distno + "'";
                Ggrp.Execute(SQl, Sconn);
            }

            return Json(new { Items = "", checkroomlist = Roomlistverify });
        }
        #endregion

        #region Retailers

        public ActionResult Retailers(string RcorpNo = "")
        {
            ViewBag.RcorpNo = RcorpNo;
            return View();
        }

        public ActionResult CoorporateCombo()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select '' as  RcorpNo,'' as gwcorp union select RcorpNo,gwcorp from retailercorp";
            Ggrp.Getdataset(sql, "Rcorp", ds, ref da, Sconn);
            List<Hashtable> Rcorp = new List<Hashtable>();
            Rcorp = Ggrp.CovertDatasetToHashTbl(ds.Tables["Rcorp"]);
            return Json(new { Items = Rcorp });
        }

        public ActionResult ValidateRetailerCompany(string company, bool AddMode, string RetailerNo)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (company.Contains("'"))
            {
                company = company.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select company from retailer where company='" + company + "'";
            else
                lSql = " select company from retailer where company = '" + company + "' and RetailerNo!='" + RetailerNo + "'";

            Ggrp.Getdataset(lSql, "FillCompany", ds, ref da, Sconn);

            if (ds.Tables["FillCompany"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult DistributorCombo()
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            sql = "select '' as  DistNo,'' as company union select DistNo,company from distributor order by company";
            Ggrp.Getdataset(sql, "Distributor", ds, ref da, Sconn);
            List<Hashtable> DistributorList = new List<Hashtable>();
            DistributorList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Distributor"]);
            return Json(new { Items = DistributorList });
        }

        public ActionResult FillRetailerGrid(string GwNo)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);
            if (string.IsNullOrEmpty(GwNo))
                sql = "select top 1 * from Retailer order by RetailerNo desc";
            else
                sql = "select * from Retailer where RetailerNo='" + GwNo + "'";
            Ggrp.Getdataset(sql, "Reatilers", ds, ref da, Sconn);
            List<Hashtable> Retail = new List<Hashtable>();
            Retail = Ggrp.CovertDatasetToHashTbl(ds.Tables["Reatilers"]);
            return Json(new { Items = Retail });
        }

        public ActionResult SaveRetailer(string ObjRetailArray = "", bool IsAddMode = false)
        {
            string sSQL = ""; int RetailerNo = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjRetailArray = "[" + ObjRetailArray + "]";
            List<Hashtable> RetailList = jss.Deserialize<List<Hashtable>>(ObjRetailArray);
            DataSet Rds = new DataSet();
            SqlDataAdapter Rda = new SqlDataAdapter();
            DataRow drAdd;
            if (RetailList.Count > 0)
            {
                try
                {
                    Rds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from Retailer where 1=0";
                    }
                    else
                    {
                        sSQL = "select * from Retailer where RetailerNo='" + RetailList[0]["RetailerNo"].ToString() + "'";
                    }
                    Ggrp.Getdataset(sSQL, "tblRetailer", Rds, ref Rda, Sconn, false);
                    if (IsAddMode == true)
                    {
                        drAdd = Rds.Tables["tblRetailer"].NewRow();
                    }
                    else
                    {
                        drAdd = Rds.Tables["tblRetailer"].Rows[0];
                    }
                    drAdd["company"] = RetailList[0]["company"].ToString();
                    drAdd["address1"] = RetailList[0]["address1"].ToString();
                    drAdd["city"] = RetailList[0]["city"].ToString();
                    drAdd["state"] = RetailList[0]["state"].ToString();
                    drAdd["zip"] = RetailList[0]["zip"].ToString();
                    drAdd["country"] = RetailList[0]["country"].ToString();
                    drAdd["phone"] = RetailList[0]["phone"].ToString();
                    drAdd["fax"] = RetailList[0]["fax"].ToString();
                    drAdd["email"] = RetailList[0]["email"].ToString();
                    drAdd["www"] = RetailList[0]["www"].ToString();
                    drAdd["saddress1"] = RetailList[0]["saddress1"].ToString();
                    drAdd["scity"] = RetailList[0]["scity"].ToString();
                    drAdd["sstate"] = RetailList[0]["sstate"].ToString();
                    drAdd["szip"] = RetailList[0]["szip"].ToString();
                    if (RetailList[0]["dateterm"] != null && RetailList[0]["dateterm"].ToString() != "")
                        drAdd["dateterm"] = RetailList[0]["dateterm"].ToString();
                    else
                        drAdd["dateterm"] = DBNull.Value;
                    if (RetailList[0]["gsplus"] != null && RetailList[0]["gsplus"].ToString() != "")
                        drAdd["gsplus"] = RetailList[0]["gsplus"].ToString();
                    else
                        drAdd["gsplus"] = DBNull.Value;
                    if (RetailList[0]["primary_"] != null && RetailList[0]["primary_"].ToString() != "")
                        drAdd["primary_"] = RetailList[0]["primary_"].ToString();
                    else
                        drAdd["primary_"] = DBNull.Value;
                    if (RetailList[0]["mbrsince"] != null && RetailList[0]["mbrsince"].ToString() != "")
                        drAdd["mbrsince"] = RetailList[0]["mbrsince"].ToString();
                    else
                        drAdd["mbrsince"] = DBNull.Value;
                    drAdd["typeco"] = RetailList[0]["typeco"].ToString();
                    drAdd["fedid_tax"] = RetailList[0]["fedid_tax"].ToString();
                    drAdd["scountry"] = RetailList[0]["scountry"].ToString();
                    drAdd["corpname"] = RetailList[0]["corpname"].ToString();
                    drAdd["QueryFilter"] = RetailList[0]["QueryFilter"] == null ? "" : RetailList[0]["QueryFilter"].ToString().Trim();
                    drAdd["DistNo"] = RetailList[0]["DistNo"].ToString();
                    drAdd["emailshr"] = RetailList[0]["emailshr"].ToString();
                    drAdd["comm_via"] = RetailList[0]["comm_via"].ToString();
                    if (IsAddMode == true)
                    {
                        Rds.Tables["tblRetailer"].Rows.Add(drAdd);
                    }
                    else
                    {
                        Rds.Tables["tblRetailer"].GetChanges();
                    }
                    Rda.Update(Rds, "tblRetailer");
                    if (IsAddMode == true)
                    {
                        RetailerNo = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        RetailerNo = Convert.ToInt32(RetailList[0]["RetailerNo"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = RetailerNo });
        }

        public ActionResult DeleteRetailer(string GwNo)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from Retailer where  RetailerNo ='" + GwNo + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }
        #endregion Retailers

        #region RetailersContacts

        public ActionResult RetailersContacts()
        {
            return View();
        }

        public ActionResult FillRCGrid(string RetailerNo, string RetailerCont = "")
        {
            string sql = ""; var rowindex = 0;
            Sconn = new SqlConnection(sqlcon);
            sql = "select * from Retailercontact where RetailerNo='" + RetailerNo + "' order by lastname,firstname";
            Ggrp.Getdataset(sql, "RetailerContacts", ds, ref da, Sconn);
            List<Hashtable> RetailCont = new List<Hashtable>();
            RetailCont = Ggrp.CovertDatasetToHashTbl(ds.Tables["RetailerContacts"]);
            if (!string.IsNullOrEmpty(RetailerNo) && ds.Tables["RetailerContacts"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["RetailerContacts"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["RetailerContacts"].Select().ToList()
                                   where dr["contactno"].ToString() == RetailerCont
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }
            return Json(new { Items = RetailCont, rowindex = rowindex });
        }

        public ActionResult ValidateRetcontNames(string FirstName, string LastName, bool AddMode, string RetailerContNo, string RetailerNo, string titlecd)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (FirstName.Contains("'"))
            {
                FirstName = FirstName.Replace("'", "''");
            }
            if (LastName.Contains("'"))
            {
                LastName = LastName.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select firstname,lastname from Retailercontact where firstname='" + FirstName + "' and lastname='" + LastName + "' and RetailerNo='" + RetailerNo + "' and gwtitlecd='" + titlecd + "'";
            else
                lSql = " select firstname,lastname from Retailercontact where firstname = '" + FirstName + "'and lastname='" + LastName + "' and RetailerContNo!='" + RetailerContNo + "' and RetailerNo='" + RetailerNo + "' and gwtitlecd='" + titlecd + "'";

            Ggrp.Getdataset(lSql, "FillCompany", ds, ref da, Sconn);

            if (ds.Tables["FillCompany"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult SaveRetailerContact(string ObjRCArray = "", string RetailNo = "", bool IsAddMode = false)
        {
            string sSQL = ""; int ContNo = 0; int RetailerNo = 0;
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ObjRCArray = "[" + ObjRCArray + "]";
            List<Hashtable> RetailContList = jss.Deserialize<List<Hashtable>>(ObjRCArray);
            DataSet RCds = new DataSet();
            SqlDataAdapter RCda = new SqlDataAdapter();
            DataRow drAdd;
            if (RetailContList.Count > 0)
            {
                try
                {
                    RCds = new DataSet();
                    if (IsAddMode == true)
                    {
                        sSQL = "select * from Retailercontact where 1=0";
                        string sql = "select max(isnull(contactno,0))+1 as ContactNo from Retailercontact where RetailerNo='" + RetailNo + "'";
                        Ggrp.Getdataset(sql, "Retailcontacts", ds, ref da, Sconn);
                        if (ds != null && ds.Tables["Retailcontacts"].Rows.Count > 0)
                        {
                            if (ds.Tables["Retailcontacts"].Rows[0]["contactno"].ToString() == "")
                                ContNo = 1;
                            else
                                ContNo = Convert.ToInt32(ds.Tables["Retailcontacts"].Rows[0]["contactno"].ToString());
                        }
                    }
                    else
                    {
                        sSQL = "select * from Retailercontact where RetailerNo='" + RetailContList[0]["RetailerNo"].ToString() + "' and contactno='" + RetailContList[0]["contactno"].ToString() + "'";
                    }
                    Ggrp.Getdataset(sSQL, "tblRetailcontacts", RCds, ref RCda, Sconn);
                    if (IsAddMode == true)
                    {
                        drAdd = RCds.Tables["tblRetailcontacts"].NewRow();
                        drAdd["contactno"] = ContNo;
                        drAdd["RetailerNo"] = RetailNo;
                    }
                    else
                    {
                        drAdd = RCds.Tables["tblRetailcontacts"].Rows[0];
                        drAdd["RetailerNo"] = RetailContList[0]["RetailerNo"].ToString();
                    }
                    drAdd["salute"] = RetailContList[0]["salute"].ToString();
                    drAdd["firstname"] = RetailContList[0]["firstname"].ToString();
                    drAdd["lastname"] = RetailContList[0]["lastname"].ToString();
                    drAdd["minitial"] = RetailContList[0]["minitial"].ToString();
                    drAdd["fax"] = RetailContList[0]["fax"].ToString();
                    drAdd["greeting"] = RetailContList[0]["greeting"].ToString();
                    drAdd["cell"] = RetailContList[0]["cell"].ToString();
                    drAdd["title"] = RetailContList[0]["title"].ToString();
                    drAdd["gwtitlecd"] = RetailContList[0]["gwtitlecd"].ToString();
                    drAdd["gwseq"] = RetailContList[0]["gwseq"].ToString();
                    drAdd["phone"] = RetailContList[0]["phone"].ToString();
                    drAdd["evephone"] = RetailContList[0]["evephone"].ToString();
                    drAdd["fax"] = RetailContList[0]["fax"].ToString();
                    drAdd["email"] = RetailContList[0]["email"].ToString();
                    drAdd["dextn"] = RetailContList[0]["dextn"].ToString();
                    drAdd["eextn"] = RetailContList[0]["eextn"].ToString();
                    if (IsAddMode == true)
                    {
                        RCds.Tables["tblRetailcontacts"].Rows.Add(drAdd);
                    }
                    else
                    {
                        RCds.Tables["tblRetailcontacts"].GetChanges();
                    }
                    RCda.Update(RCds, "tblRetailcontacts");
                    if (IsAddMode == true)
                    {
                        // RetailerNo = Ggrp.GetIdentityValue(Sconn);
                    }
                    else
                    {
                        RetailerNo = Convert.ToInt32(RetailContList[0]["RetailerNo"].ToString());
                        ContNo = Convert.ToInt32(RetailContList[0]["contactno"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Items = ContNo });
        }

        public ActionResult DeleteContacts(string RetailerNo, string contactno)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "delete from Retailercontact where  RetailerNo ='" + RetailerNo + "' and contactno='" + contactno + "'";
            Ggrp.Execute(SQl, Sconn);
            return Json(new { Items = "" });
        }

        #endregion

        #region Benefit Update Batch Number
        public ActionResult FillBenefitBatchNoUpdate()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            SQl = "exec STP_SUP_BenefitBatchNumberUpdate";
            Ggrp.Getdataset(SQl, "BenefitBatchNumber", ds, ref da, Sconn);

            return Json(new { Items = "success" });

        }
        #endregion

        #region Reset Hardware Show Booth Number
        public ActionResult FillResetHardware()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ResetBoothNumberHardwareShow";
                Ggrp.Getdataset(SQl, "ResetBoothNumber", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region Reset IGC Chicago Show Booth Number
        public ActionResult ResetIGCChicago()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ResetBoothNumberChicagoShow";
                Ggrp.Getdataset(SQl, "ResetBoothNumberChicago", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region Reset Mfg. CodeA
        public ActionResult ResetCodeA()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ResetCodeA";
                Ggrp.Getdataset(SQl, "ResetCodeA", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region Reset Mfg. CodeC
        public ActionResult ResetCodeC()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ResetCodeC";
                Ggrp.Getdataset(SQl, "ResetCodeC", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region Roll PDS and Program CY to LY
        public ActionResult RollPDSProgram()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_RolloverPDSProgReceived";
                Ggrp.Getdataset(SQl, "RollPDS", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region Products - Update all MFG.No
        public ActionResult ProductsUpdate()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ProductsUpdateAllMFGNo";
                Ggrp.Getdataset(SQl, "ProductsUpdateAll", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion
        
        #region Products - Annual Clear New Change Delete code
        public ActionResult ProductsAnnualClear()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ProductsAnnualClearCodes";
                Ggrp.Getdataset(SQl, "ProductsAnnual", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region P&S UPC Change Blanks to Zeroes
        public ActionResult UPCChangeZeros()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ZeroOutUPCs";
                Ggrp.Getdataset(SQl, "ZeroOutUPCs", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region Reset Kickoff

        public ActionResult RevisionRollover()
        {
            Sconn = new SqlConnection(sqlcon);
            string sql = "";
            sql = "SELECT rollcnt=COUNT(*)  FROM mfg where ISNULL(prgrevty,'') <>''";
            Ggrp.Getdataset(sql, "rollcount", ds, ref da, Sconn);
            List<Hashtable> rollcountList = new List<Hashtable>();
            rollcountList = Ggrp.CovertDatasetToHashTbl(ds.Tables["rollcount"]);
            return Json(new { Items = rollcountList });
        }

        public ActionResult ResetKickoff()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            try
            {
                SQl = "exec STP_SUP_ResetKickoff";
                Ggrp.Getdataset(SQl, "Kickoff", ds, ref da, Sconn);
            }
            catch (Exception)
            {

            }
            return Json(new { Items = "success" });

        }
        #endregion

        #region frmSalesPurchase

        public ActionResult frmSalesPurchase()
        {
            ViewBag.Initials = Session["initials"].ToString();
            Sconn = new SqlConnection(sqlcon);
            string sql = "select ym1,ym2,curr_dt from OP_system";
            Ggrp.Getdataset(sql, "System", ds, ref da, Sconn);
            if (ds != null && ds.Tables["System"].Rows.Count > 0)
            {
                ViewBag.YM1 = ds.Tables["System"].Rows[0]["curr_dt"].ToString();
                ViewBag.YM2 = ds.Tables["System"].Rows[0]["curr_dt"].ToString();
            }
            else
            {
                ViewBag.YM1 = "";
                ViewBag.YM2 = "";
            }
            return View();
        }

        public ActionResult GetDistSaleList()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "select '' as distid union select distinct(distid) from sales";
            Ggrp.Getdataset(SQl, "Grpsale", ds, ref da, Sconn);
            List<Hashtable> GrpsaleList = new List<Hashtable>();
            GrpsaleList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Grpsale"]);
            return Json(new { Items = GrpsaleList });
        }

        public ActionResult FillSalesPurchaseRptOptions()
        {
            Sconn = new SqlConnection(sqlcon);
            string Sql = "select Description,SQLSelect from op_security_points where usermodule='RP_PS' and securityid!='REPORTGROUP_06'";
            Ggrp.Getdataset(Sql, "SalesPurchaseRpt", ds, ref da, Sconn);
            List<Hashtable> SalesPurchaseRptList = new List<Hashtable>();
            SalesPurchaseRptList = Ggrp.CovertDatasetToHashTbl(ds.Tables["SalesPurchaseRpt"]);
            return Json(new { Items = SalesPurchaseRptList });
        }

        public ActionResult FillSalesList(int pagecount, string distid, string periodfrom, string periodto)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            string DistID = "";
            if (distid != null && distid != "")
                DistID = "distid='" + distid + "' and ";
            string SQlCNt = "SELECT count(distid)as cnt,SUM(sales) as salecount FROM sales where " + DistID + " yearmonth between '" + periodfrom + "' and '" + periodto + "'";
            Ggrp.Getdataset(SQlCNt, "Salescnt", ds, ref da, Sconn);
            List<Hashtable> SalesCnt = new List<Hashtable>();
            SalesCnt = Ggrp.CovertDatasetToHashTbl(ds.Tables["Salescnt"]);
            string totalrecord = SalesCnt[0]["cnt"].ToString();
            string totalsale = SalesCnt[0]["salecount"].ToString();
            int totalpage = (Convert.ToInt32(totalrecord) / 20000);
            int pagefrom;

            pagefrom = (pagecount * 20000);
            int pageto = pagefrom + 20000;
            int pafrom = pagefrom + 1;

            SQl = "SELECT distid, upc, Qty, Sales as Sales,yearmonth,qty_dc,sales_dc,mfg_item ";
            SQl += "FROM (SELECT distid, upc, Qty, Sales as Sales, yearmonth,qty_dc,sales_dc,mfg_item, ROW_NUMBER() OVER (ORDER BY yearmonth desc,distid,upc asc) AS RowNum ";
            SQl += "FROM sales where " + DistID + " yearmonth between '" + periodfrom + "' and '" + periodto + "') AS MyDerivedTable ";
            SQl += "WHERE MyDerivedTable.RowNum BETWEEN '" + pafrom + "' and '" + pageto + "'";
            Ggrp.Getdataset(SQl, "SalesDetails", ds, ref da, Sconn);
            List<Hashtable> SalesDetalisList = new List<Hashtable>();
            SalesDetalisList = Ggrp.CovertDatasetToHashTbl(ds.Tables["SalesDetails"]);
            return Json(new { Itemdetail = SalesDetalisList, count = totalpage, TotalRecords = totalrecord, TotalSales = totalsale });
        }
        
        public ActionResult FillSalesPurchaseList(int pagecount, string periodfrom, string periodto, string distid, string mfgNo = "", string Tab = "", string mfgupc = "", string upc = "")
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "", PurclWhere = "", SaleslWhere = "";
            string DistID = "", PurMfgSearch = "", SalesMfgSearch = "", MfgUPCSearch = "", UPCSearch = "";

            PurclWhere = " where 1=1 "; SaleslWhere = " where 1=1 ";

            if (!string.IsNullOrEmpty(distid))
                DistID = " and distid='" + distid + "' ";

            if (!string.IsNullOrEmpty(mfgNo))
            {
                PurMfgSearch = " and m.mfgno=" + mfgNo + " ";
                SalesMfgSearch = " and p.mfgno=" + mfgNo + " ";
            }

            if (!string.IsNullOrEmpty(mfgupc))
            {
                MfgUPCSearch = " and p.mfgupc like '" + mfgupc + "%' ";
            }

            if (!string.IsNullOrEmpty(upc))
            {
                UPCSearch = " and s.upc like '" + upc + "%' ";
            }

            PurclWhere += " and yearmonth between '" + periodfrom + "' and '" + periodto + "' " + DistID + PurMfgSearch + MfgUPCSearch;
            SaleslWhere += " and yearmonth between '" + periodfrom + "' and '" + periodto + "' " + DistID + SalesMfgSearch + UPCSearch;
            int Salestotalpage = 0, Salespagefrom = 0, Purchasetotalpage = 0, Purchasepagefrom;
            string Salestotalrecord = "", totalsale = "", Purchasetotalrecord = "", totalPurchase = "";

            List<Hashtable> SalesDetalisList = new List<Hashtable>();
            List<Hashtable> PurchaseDetailsList = new List<Hashtable>();

            if (Tab.Trim().ToUpper() == "SALESTAB")
            {
                //Sales
                string SQlSalesCNt = "SELECT count(distid)as cnt,SUM(sales) as salecount FROM sales s left join Product p on p.upc=s.upc " + SaleslWhere;
                Ggrp.Getdataset(SQlSalesCNt, "Salescnt", ds, ref da, Sconn);
                List<Hashtable> SalesCnt = new List<Hashtable>();
                SalesCnt = Ggrp.CovertDatasetToHashTbl(ds.Tables["Salescnt"]);
                Salestotalrecord = SalesCnt[0]["cnt"].ToString();
                totalsale = SalesCnt[0]["salecount"].ToString();
                Salestotalpage = (Convert.ToInt32(Salestotalrecord) / 20000);

                Salespagefrom = (pagecount * 20000);
                int Salespageto = Salespagefrom + 20000;
                int Salespafrom = Salespagefrom + 1;

                SQl = "SELECT distid, upc, Qty, Sales as Sales,yearmonth,qty_dc,sales_dc,mfg_item ";
                SQl += "FROM (SELECT distid, s.upc, Qty, Sales as Sales, yearmonth,qty_dc,sales_dc,mfg_item, ROW_NUMBER() OVER (ORDER BY yearmonth desc,distid,s.upc asc) AS RowNum ";
                SQl += "FROM sales s left join Product p on p.upc=s.upc " + SaleslWhere + ") AS MyDerivedTable ";
                SQl += "WHERE MyDerivedTable.RowNum BETWEEN '" + Salespafrom + "' and '" + Salespageto + "'";
                Ggrp.Getdataset(SQl, "SalesDetails", ds, ref da, Sconn);

                SalesDetalisList = Ggrp.CovertDatasetToHashTbl(ds.Tables["SalesDetails"]);
            }
            if (Tab.Trim().ToUpper() == "PURCHASETAB")
            {
                //Purchase
                string SQlPurchaseCNt = "SELECT count(distid)as cnt,SUM(purchase) as purchasecount FROM purchase p left join mfgupc m on m.mfgupc=p.mfgupc left join Distributor d on d.ddtlink=p.distid " + PurclWhere;
                Ggrp.Getdataset(SQlPurchaseCNt, "purchasecnt", ds, ref da, Sconn);
                List<Hashtable> PurchaseCnt = new List<Hashtable>();
                PurchaseCnt = Ggrp.CovertDatasetToHashTbl(ds.Tables["purchasecnt"]);
                Purchasetotalrecord = PurchaseCnt[0]["cnt"].ToString();
                totalPurchase = PurchaseCnt[0]["purchasecount"].ToString();

                Purchasetotalpage = (Convert.ToInt32(Purchasetotalrecord) / 20000);
                Purchasepagefrom = (pagecount * 20000);
                int Purchasepageto = Purchasepagefrom + 20000;
                int Purchasepafrom = Purchasepagefrom + 1;

                SQl = "SELECT distid, yearmonth,mfgupc,Purchase,mfgname ";
                SQl += "FROM (SELECT distid, yearmonth,p.mfgupc,Purchase, mfgname, ROW_NUMBER() OVER(ORDER BY yearmonth desc, distid, p.mfgupc asc) AS RowNum ";
                SQl += "FROM purchase p left join mfgupc m on m.mfgupc=p.mfgupc left join Distributor d on d.ddtlink=p.distid " + PurclWhere + ") AS MyDerivedTable ";
                SQl += "WHERE MyDerivedTable.RowNum BETWEEN '" + Purchasepafrom + "' and '" + Purchasepageto + "'";
                Ggrp.Getdataset(SQl, "PurchaseDetails", ds, ref da, Sconn);

                PurchaseDetailsList = Ggrp.CovertDatasetToHashTbl(ds.Tables["PurchaseDetails"]);
            }
            return Json(new { SalesItemdetail = SalesDetalisList, Salescount = Salestotalpage, SalesTotalRecords = Salestotalrecord, TotalSales = totalsale, PurchaseItemdetail = PurchaseDetailsList, Purchasecount = Purchasetotalpage, PurchaseTotalRecords = Purchasetotalrecord, TotalPurchase = totalPurchase });
        }

        public ActionResult FillExecutiveVerifications(string Description, string sqlselect, string Filter = "")
        {
            Sconn = new SqlConnection(sqlcon);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Filter = "[" + Filter + "]";
            List<Hashtable> repfilter = jss.Deserialize<List<Hashtable>>(Filter);

            Ggrp.Getdataset("select Field_Name,Display_FName,type from OP_ReportSelect where usermodule='RP_PS' and Description='" + Description + "'", "GetParamList", ds, ref da, Sconn);
            SqlParameter[] Sqlparam = new SqlParameter[ds.Tables["GetParamList"].Rows.Count];
            if (ds.Tables["GetParamList"] != null && ds.Tables["GetParamList"].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables["GetParamList"].Rows.Count; i++)
                {
                    string fieldname = ds.Tables["GetParamList"].Rows[i]["Field_Name"].ToString();
                    string fieldnamevalues = ds.Tables["GetParamList"].Rows[i]["Display_FName"].ToString();
                    string fieldtype = ds.Tables["GetParamList"].Rows[i]["Type"].ToString();
                    if (fieldname.Contains("."))
                    {
                        string[] splitfname = fieldname.Split('.');
                        fieldname = splitfname[1];
                    }
                    if (repfilter[0][fieldname] == null || repfilter[0][fieldname].ToString() == "")
                    {
                        string msg = "Please fill the " + fieldnamevalues + " value.";
                        return Json(new { msg = msg });

                    }
                    Sqlparam[i] = new SqlParameter("@" + fieldname, repfilter[0][fieldname]);

                }
            }
            Ggrp.GetDataSetFromSP(sqlselect, "FillExecutiveVerifications", ref ds, ref da, Sqlparam, Sconn);

            string[] Headervalues;
            if (ds.Tables["Gridviewclo"].Rows[0]["GridViewColumns"] != null && ds.Tables["Gridviewclo"].Rows[0]["GridViewColumns"].ToString() != "")
            {
                char sep = '~';
                int leth = 0;
                string[] splitMain = ds.Tables["Gridviewclo"].Rows[0]["GridViewColumns"].ToString().ToUpper().Split(sep).Where(s => !String.IsNullOrEmpty(s)).ToArray();
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
                        if (!FLName.Contains(split[1]) && !CLName.Contains(split[0]) && ds.Tables["FillExecutiveVerifications"].Columns.Contains(split[1]))
                        {
                            FLName[i] = split[1].ToString();
                            CLName[i] = split[0].ToString();
                        }
                    }
                }
                CLName = CLName.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                FLName = FLName.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                Headervalues = CLName;
            }
            else
            {
                Headervalues = new string[ds.Tables["FillExecutiveVerifications"].Columns.Count];
                for (int i = 0; i < ds.Tables["FillExecutiveVerifications"].Columns.Count; i++)
                {
                    Headervalues[i] = ds.Tables["FillExecutiveVerifications"].Columns[i].ColumnName.ToString();
                }
            }
            List<ArrayList> newval = new List<ArrayList>();
            for (int i = 0; i < ds.Tables["FillExecutiveVerifications"].Rows.Count; i++)
            {
                ArrayList values = new ArrayList();
                for (int j = 0; j < ds.Tables["FillExecutiveVerifications"].Columns.Count; j++)
                {
                    string type = ds.Tables["FillExecutiveVerifications"].Columns[j].DataType.ToString();
                    if (ds.Tables["FillExecutiveVerifications"].Columns[j].DataType.ToString().ToUpper() == "SYSTEM.DATETIME")
                    {
                        string datetime = ds.Tables["FillExecutiveVerifications"].Rows[i][j] == DBNull.Value ? "" : Convert.ToDateTime(ds.Tables["FillExecutiveVerifications"].Rows[i][j]).ToShortDateString();
                        values.Add(datetime);
                    }
                    else
                    {
                        values.Add(ds.Tables["FillExecutiveVerifications"].Rows[i][j].ToString());
                    }
                }
                newval.Add(values);
            }
            return Json(new { Items = newval, Header = Headervalues });
        }

        public ActionResult FillDistTotalsList(string ym1, string ym2)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "STP_PSSummaryDisplay " + ym1 + "," + ym2;
            Ggrp.Getdataset(SQl, "DistTotals", ds, ref da, Sconn);
            List<Hashtable> DistTotalsList = new List<Hashtable>();
            DistTotalsList = Ggrp.CovertDatasetToHashTbl(ds.Tables["DistTotals"]);
            return Json(new { Items = DistTotalsList });
        }

        #endregion frmSalesPurchase

        #region frmPurchase
        public ActionResult frmPurchase()
        {
            Sconn = new SqlConnection(sqlcon);
            string sql = "select ym1,ym2 from OP_system";
            Ggrp.Getdataset(sql, "System", ds, ref da, Sconn);
            if (ds != null && ds.Tables["System"].Rows.Count > 0)
            {
                ViewBag.YM1 = ds.Tables["System"].Rows[0]["ym1"].ToString();
                ViewBag.YM2 = ds.Tables["System"].Rows[0]["ym2"].ToString();
            }
            else
            {
                ViewBag.YM1 = "";
                ViewBag.YM2 = "";
            }
            return View();
        }
        public ActionResult GetDistPurchaseList()
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "select '' as distid union select distinct(distid) from purchase";
            Ggrp.Getdataset(SQl, "Grppurchase", ds, ref da, Sconn);
            List<Hashtable> GrppurchaseList = new List<Hashtable>();
            GrppurchaseList = Ggrp.CovertDatasetToHashTbl(ds.Tables["Grppurchase"]);
            return Json(new { Items = GrppurchaseList });
        }
        public ActionResult FillPurchaseList(int pagecount, string distid, string periodfrom, string periodto)
        {
            Sconn = new SqlConnection(sqlcon);
            string SQl = "";
            string DistID = "";
            if (distid != null && distid != "")
                DistID = "distid='" + distid + "' and ";
            string SQlCNt = "SELECT count(distid)as cnt,SUM(purchase) as purchasecount FROM purchase where " + DistID + " yearmonth between '" + periodfrom + "' and '" + periodto + "'";
            Ggrp.Getdataset(SQlCNt, "purchasecnt", ds, ref da, Sconn);
            List<Hashtable> PurchaseCnt = new List<Hashtable>();
            PurchaseCnt = Ggrp.CovertDatasetToHashTbl(ds.Tables["purchasecnt"]);
            string totalrecord = PurchaseCnt[0]["cnt"].ToString();
            string totalPurchase = PurchaseCnt[0]["purchasecount"].ToString();

            int totalpage = (Convert.ToInt32(totalrecord) / 20000);
            int pagefrom;

            pagefrom = (pagecount * 20000);
            int pageto = pagefrom + 20000;
            int pafrom = pagefrom + 1;

            SQl = "SELECT distid, mfgupc,Purchase ";
            SQl += "FROM (SELECT distid, mfgupc,Purchase, ROW_NUMBER() OVER(ORDER BY yearmonth desc, distid, mfgupc asc) AS RowNum ";
            SQl += "FROM purchase where  " + DistID + " yearmonth between '" + periodfrom + "' and '" + periodto + "') AS MyDerivedTable ";
            SQl += "WHERE MyDerivedTable.RowNum BETWEEN '" + pafrom + "' and '" + pageto + "'";
            Ggrp.Getdataset(SQl, "PurchaseDetails", ds, ref da, Sconn);
            List<Hashtable> PurchaseDetailsList = new List<Hashtable>();
            PurchaseDetailsList = Ggrp.CovertDatasetToHashTbl(ds.Tables["PurchaseDetails"]);
            return Json(new { Itemdetail = PurchaseDetailsList, count = totalpage, TotalRecords = totalrecord, TotalPurchase = totalPurchase });
        }



        #endregion

        #region RetailerCorporate

        public ActionResult RetailerCorporate()
        {
            return View();
        }

        public ActionResult FillCorprof(string corpno = "")
        {
            string sql = ""; var rowindex = 0;
            Sconn = new SqlConnection(sqlcon);

            sql = "select  * from retailerCorp order by gwcorp";
            Ggrp.Getdataset(sql, "retail", ds, ref da, Sconn);
            List<Hashtable> retailer = new List<Hashtable>();
            retailer = Ggrp.CovertDatasetToHashTbl(ds.Tables["retail"]);
            if (!string.IsNullOrEmpty(corpno) && ds.Tables["retail"].Rows.Count > 0)
            {
                var rowlist = ds.Tables["retail"].Select().ToList();
                var getCaseRows = (from dr in ds.Tables["retail"].Select().ToList()
                                   where dr["RCorpNo"].ToString().Trim() == corpno
                                   select dr).FirstOrDefault();
                rowindex = rowlist.IndexOf(getCaseRows);
            }

            return Json(new { Item = retailer, rowindex = rowindex });
        }

        public ActionResult Fillretail(string Rcorpno)
        {
            string sql = "";
            Sconn = new SqlConnection(sqlcon);

            sql = "select * from Retailer where RCorpNo='" + Rcorpno + "' ";
            Ggrp.Getdataset(sql, "retailer", ds, ref da, Sconn);
            List<Hashtable> retail = new List<Hashtable>();
            retail = Ggrp.CovertDatasetToHashTbl(ds.Tables["retailer"]);

            return Json(new { Item1 = retail });
        }

        public ActionResult ValidateRetCorpCompany(string company, bool AddMode, string RetCorpNum)
        {
            bool Ismsg = false;
            Sconn = new SqlConnection(sqlcon);
            string lSql = "";
            if (company.Contains("'"))
            {
                company = company.Replace("'", "''");
            }
            if (AddMode == true)
                lSql = " select gwcorp from retailercorp where gwcorp='" + company + "'  ";
            else
                lSql = " select gwcorp from retailercorp where gwcorp = '" + company + "' and RCorpNo!='" + RetCorpNum + "'";
            Ggrp.Getdataset(lSql, "fillretcorpcomp", ds, ref da, Sconn);
            if (ds.Tables["fillretcorpcomp"].Rows.Count > 0)
            {
                Ismsg = true;
            }
            return Json(new { Items = Ismsg });
        }

        public ActionResult Saveretailcorp(string objret, bool IsAddMode = false, string Rcorpno = "")
        {
            string SQL; int corpno = 0;
            Sconn = new SqlConnection(sqlcon);
            objret = "[" + objret + "]";
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Hashtable> Rettbl = js.Deserialize<List<Hashtable>>(objret);
            DataSet ds = new DataSet();
            DataRow adddr;

            if (Rettbl.Count > 0)
            {
                ds = new DataSet();
                try
                {
                    if (IsAddMode == true)
                    {
                        SQL = "select * from retailercorp where 1=0";
                    }
                    else
                    {
                        SQL = "select * from retailercorp where RCorpNo='" + Rettbl[0]["RCorpNo"].ToString() + "'";
                    }
                    Ggrp.Getdataset(SQL, "Retailercorp", ds, ref da, Sconn, false);
                    if (IsAddMode == true)
                    {
                        adddr = ds.Tables["Retailercorp"].NewRow();
                    }
                    else
                    {
                        adddr = ds.Tables["Retailercorp"].Rows[0];
                        adddr["RCorpNo"] = Rettbl[0]["RCorpNo"].ToString();
                    }
                    adddr["gcno"] = Rettbl[0]["gcno"].ToString() == "" ? DBNull.Value : Rettbl[0]["gcno"];
                    adddr["gwcorp"] = Rettbl[0]["gwcorp"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcorp"];
                    adddr["gwcadd1"] = Rettbl[0]["gwcadd1"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcadd1"];
                    adddr["gwccity"] = Rettbl[0]["gwccity"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwccity"];
                    adddr["gwcst"] = Rettbl[0]["gwcst"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcst"];
                    adddr["gwczip"] = Rettbl[0]["gwczip"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwczip"];
                    adddr["gwcountry"] = Rettbl[0]["gwcountry"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcountry"];
                    adddr["gwcphone"] = Rettbl[0]["gwcphone"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcphone"];
                    adddr["gwcfax"] = Rettbl[0]["gwcfax"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcfax"];
                    adddr["gwccodea"] = Rettbl[0]["gwccodea"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwccodea"];
                    adddr["gwcext"] = Rettbl[0]["gwcext"].ToString() == "" ? DBNull.Value : Rettbl[0]["gwcext"];

                    if (IsAddMode == true)
                    {
                        ds.Tables["Retailercorp"].Rows.Add(adddr);
                    }
                    else
                    {
                        ds.Tables["Retailercorp"].GetChanges();
                    }
                    da.Update(ds, "Retailercorp");

                    if (IsAddMode == true)
                    {
                        corpno = Convert.ToInt32(Ggrp.GetIdentityValue(Sconn).ToString());
                    }
                    else
                    {
                        corpno = Convert.ToInt32(Rettbl[0]["RCorpNo"].ToString());
                    }
                }

                catch (Exception ex)
                {
                    return Json(new { ItemMsg = ex.ToString() });
                }
            }
            return Json(new { Item = corpno });
        }

        #endregion

    }

    public struct SelecetedZiplist
    {
        public string Field_Name;
        public string Display_FName;
    }

    public struct MainMenus
    {
        public string Description;
        public string userModule;
        public string ControllerName;
    }
}