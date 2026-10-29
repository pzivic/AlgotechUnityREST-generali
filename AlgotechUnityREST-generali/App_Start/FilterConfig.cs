using System.Web;
using System.Web.Mvc;

namespace AlgotechUnityREST_generali
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
