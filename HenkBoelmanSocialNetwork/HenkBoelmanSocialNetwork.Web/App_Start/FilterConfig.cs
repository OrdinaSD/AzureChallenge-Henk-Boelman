using System.Web;
using System.Web.Mvc;

namespace HenkBoelmanSocialNetwork.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
