using System.Web;
using System.Web.Optimization;

namespace GroGroup
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/site.css"));




            var AddBundle = BundleTable.Bundles;

            AddBundle.Add(new ScriptBundle("~/bundles/lOGINjquery").Include("~/js/jquery-1.9.1.min.js",
              "~/js/knockout-2.2.0.js",
             "~/Js/jquery-ui-1.9.2.min.js"));

            AddBundle.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Js/modernizr.min.js",
                "~/Js/jquery-migrate-1.1.1.min.js",
            "~/Js/jquery.cookie.js",
            "~/Js/chosen.jquery.min.js",
            "~/js/bootstrap-multiselect.js",
            "~/js/modernizr-2.5.3.js",
            "~/js/jquery-ui-1.10.3.min.js",
            "~/js/bootstrap.min.js",
            "~/js/flot/jquery.flot.min.js",
            "~/js/flot/jquery.flot.resize.min.js",
            "~/js/responsive-tables.js",
            "~/js/custom.js",
            "~/prettify/prettify.js",
            "~/js/elements.js",
            "~/js/Validation.js",
            "~/js/jquery.cookie.js",
            "~/js/jquery.msgBox.js",
            "~/js/jquery.form.js",
            "~/js/jquery.dataTables.min.js",
            "~/js/jquery.slimscroll.js",
            "~/js/colorpicker.js",
            "~/Scripts/parent.js",
            "~/Scripts/UserRestrictions.js",
            "~/js/jquery.uniform.min.js"));

            BundleTable.EnableOptimizations = true;

        }
    }
}
