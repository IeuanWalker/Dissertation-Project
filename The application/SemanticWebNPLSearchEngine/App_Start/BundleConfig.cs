using System.Web.Optimization;
using DissertationOriginal.Constants;

namespace DissertationOriginal
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Enable CDN usage.
            // Note: I am using Google's CDN where possible and then Microsoft if not available for
            //       better performance (Google is more likely to have been cached by the users browser).
            bundles.UseCdn = true;

            AddCss(bundles);
            AddJavaScript(bundles);
        }

        private static void AddCss(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle(
                "~/Content/css")
                .Include("~/Content/css/style.min.css"));

            // Font Awesome - Icons using font (http://fortawesome.github.io/Font-Awesome/).
            bundles.Add(new StyleBundle(
                "~/Content/fa",
                ContentDeliveryNetworks.MaxCdn.FontAwesomeUrl)
                .Include("~/Content/fontawesome/site.css"));
        }

        private static void AddJavaScript(BundleCollection bundles)
        {
            // jQuery - The JavaScript helper API (http://jquery.com/).
            Bundle jqueryBundle = new ScriptBundle("~/bundles/jquery", ContentDeliveryNetworks.Google.JQueryUrl)
                .Include("~/Scripts/jquery-{version}.js");
            bundles.Add(jqueryBundle);

            // jQuery Validate - Client side JavaScript form validation (http://jqueryvalidation.org/).
            Bundle jqueryValidateBundle = new ScriptBundle(
                "~/bundles/jqueryval",
                ContentDeliveryNetworks.Microsoft.JQueryValidateUrl)
                .Include("~/Scripts/jquery.validate*");
            bundles.Add(jqueryValidateBundle);

            Bundle modernizrBundle = new ScriptBundle(
                "~/bundles/modernizr",
                ContentDeliveryNetworks.Microsoft.ModernizrUrl)
                .Include("~/Scripts/modernizr-*");
            bundles.Add(modernizrBundle);

            // Bootstrap - Twitter Bootstrap JavaScript (http://getbootstrap.com/).
            Bundle bootstrapBundle = new ScriptBundle(
                "~/bundles/bootstrap",
                ContentDeliveryNetworks.Microsoft.BootstrapUrl)
                .Include("~/Scripts/bootstrap.js");
            bundles.Add(bootstrapBundle);

            //Custom scripts
            Bundle myScriptsBundle = new ScriptBundle(
                "~/bundles/myScripts")
                .Include("~/Scripts/connector.js")
                .Include("~/Scripts/search.js");
            bundles.Add(myScriptsBundle);
        }
    }
}