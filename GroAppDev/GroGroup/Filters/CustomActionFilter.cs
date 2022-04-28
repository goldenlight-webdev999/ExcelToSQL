using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using GroGroup.Class;

namespace GroGroup.Filters
{

    public class CustomActionFilter : ActionFilterAttribute, IActionFilter
    {
        Aptus Ggrp = new Aptus();
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ActionDescriptor.ActionName.ToString().ToUpper().Contains("LOGIN") && !filterContext.HttpContext.Request.IsAjaxRequest())
            {
                if (filterContext.ActionDescriptor.ActionName.ToString().ToUpper() == "MAIN")
                {
                    filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                    filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
                    filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                    filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                    filterContext.HttpContext.Response.Cache.SetNoStore();
                    filterContext.HttpContext.Session["SessionTimeOut"] = filterContext.HttpContext.Session.Timeout.ToString();
                }
                if (filterContext.ActionDescriptor.ActionName.ToString().ToUpper() == "LOGOUT")
                {
                    filterContext.Result = new RedirectResult("~/GroGroup/Login");
                }
                // check if session is supported
                if (filterContext.HttpContext.Session["initials"] == null)
                {
                    filterContext.Result = new RedirectResult("~/GroGroup/Login");
                    return;
                }

                this.OnActionExecuting(filterContext);
            }

        }

        public static string GetVisitorIPAddress(bool GetLan = false)
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

    }

    public class CustomActionException : FilterAttribute, IExceptionFilter
    {
        Aptus Ggrp = new Aptus();

        void IExceptionFilter.OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {             
                string clientmacin = filterContext.HttpContext.Session["ClientMachineName"] != null ? filterContext.HttpContext.Session["ClientMachineName"].ToString() : "";
                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();
                string sourc = controller + "." + action;
                string conn = System.Configuration.ConfigurationManager.AppSettings["GroGroup"].ToString(); //filterContext.HttpContext.Session["Connstr"].ToString();
                SqlConnection Sconn = new SqlConnection(conn);
                Exception ex = filterContext.Exception;
                string stack = ex.TargetSite.ToString();
                filterContext.HttpContext.Session["Error"] = ex.Message.ToString();
                filterContext.ExceptionHandled = true;
                string message = ex.Message;
                if (message.Length > 300)
                    message = message.Replace("'", "''").Substring(0, 300);
                else
                    message = message.Replace("'", "''");
               
                string insertSQL = "insert into OP_ErrorLog(ErrSource,ErrDesc,ErrStackTrace,InnerErrSource,InnerErrDesc,InnerErrStackTrace,ErrLogdatetime,MachineName) values"
                    + "('" + sourc + "','" + message + "','" + stack + "'"
                    + ",'" + ex.InnerException + "','','','" + DateTime.Now + "','" + clientmacin + "')";
                Ggrp.Execute(insertSQL, Sconn);
                
            }
        }
    }
}