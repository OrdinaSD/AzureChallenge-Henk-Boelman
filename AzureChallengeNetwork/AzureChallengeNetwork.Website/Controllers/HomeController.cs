using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AzureChallengeNetwork.Website.Models;
using B2CGraphShell;

namespace AzureChallengeNetwork.Website.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Claims()
        {
            Claim displayName = ClaimsPrincipal.Current.FindFirst(ClaimsPrincipal.Current.Identities.First().NameClaimType);
            ViewBag.DisplayName = displayName != null ? displayName.Value : string.Empty;
            return View();
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }


    }
}