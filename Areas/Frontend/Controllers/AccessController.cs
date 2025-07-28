using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using test2.Areas.Frontend.Models;
using test2.Models;

namespace test2.Areas.Frontend.Controllers
{
    [Area("Frontend")]
    public class AccessController : Controller
    {
        #region field
        private readonly Test2Context _context;
        #endregion

        #region constructor
        public AccessController(Test2Context context) { _context = context; }
        #endregion

        #region action
        [HttpGet]
        public IActionResult LoginC() { return View(new Guest()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccessCardC(Guest model)
        {
            if (!ModelState.IsValid) { return View("LoginC", model); }

            var guest = await _context.Clients.FirstOrDefaultAsync(x => x.CAccount == model.Account);

            if (guest == null)
            {
                ModelState.AddModelError("", "帳號不正確");
                return View("LoginC", model);
            }
            else if (!Argon2.Verify(guest.CPassword, model.Password))
            {
                ModelState.AddModelError("", "密碼不正確");
                return View("LoginC", model);
            }
            else
            {
                var borrowStatus = await _context.Borrows.AsNoTracking().AnyAsync(x => x.CId == guest.CId && x.BorrowStatusId == 3);
                var borrowCount = await _context.Borrows.CountAsync(x => x.CId == guest.CId);

                var guestClaim = new List<Claim>{
                    new Claim(InternalClaimTypes.IAccount, guest.CAccount),
                    new Claim(InternalClaimTypes.IId, guest.CId.ToString()),
                    new Claim(InternalClaimTypes.IName, guest.CName),
                    new Claim(InternalClaimTypes.IBorrowStatus, borrowStatus.ToString()),
                    new Claim(InternalClaimTypes.IBorrowCount, borrowCount.ToString())
                };

                var guessAccess = new ClaimsIdentity(guestClaim, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(guessAccess));

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AccessCardE(string provider, string userAccount)
        //{
        //    var redirectUrl = Url.Action("LoginE", "Access", new { Area = "Frontend" });
        //    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        //    properties.Items["provider"] = provider;
        //    properties.Items["userAccount"] = userAccount;

        //    if (provider == "Facebook") { properties.Parameters.Add("auth_type", "reauthenticate"); }
        //    else { properties.Parameters.Add("prompt", "select_account"); }

        //    return Challenge(properties, provider);
        //}

        //[HttpGet]
        //public async Task<IActionResult> LoginE()
        //{
        //    var authentication = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

        //    if (!authentication.Succeeded || authentication.Principal == null) { return RedirectToAction("Client", "Home", new { Area = "Frontend" }); }

        //    var externalId = authentication.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    authentication.Properties.Items.TryGetValue("provider", out var provider);
        //    authentication.Properties.Items.TryGetValue("userAccount", out var userAccount);

        //    var userAccountX = await _context.Clients.FirstOrDefaultAsync(x => x.CAccount == userAccount);

        //    if (provider == "Facebook") { userAccountX!.FacebookId = externalId; }
        //    else { userAccountX!.GoogleId = externalId; }

        //    await _context.SaveChangesAsync();

        //    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        //    return RedirectToAction("Client");
        //}

        [HttpGet]
        public IActionResult LoginM() { return View(new Guest()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccessCardM(Guest model)
        {
            if (!ModelState.IsValid) { return View("LoginM", model); }

            var guest = await _context.Clients.FirstOrDefaultAsync(x => x.CAccount == model.Account);

            if (guest == null)
            {
                ModelState.AddModelError("", "帳號不正確");
                return View("LoginM", model);
            }
            else if (!Argon2.Verify(guest.CPassword, model.Password))
            {
                ModelState.AddModelError("", "密碼不正確");
                return View("LoginM", model);
            }
            else if (guest.Permission < 2)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }
            else
            {
                var guestClaim = new List<Claim>{
                    new Claim(InternalClaimTypes.IAccount, guest.CAccount),
                    new Claim(InternalClaimTypes.IId, guest.CId.ToString()),
                    new Claim(InternalClaimTypes.IName, guest.CName),
                    new Claim(InternalClaimTypes.IPermission, guest.Permission.ToString())
                };

                if (guest.Permission == 2) { guestClaim.Add(new Claim(ClaimTypes.Role, "Admin")); }
                if (guest.Permission == 3) { guestClaim.Add(new Claim(ClaimTypes.Role, "SuperAdmin")); }

                var guessAccess = new ClaimsIdentity(guestClaim, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(guessAccess));

                return RedirectToAction("Index", "Manage", new { area = "Backend" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home", new { area = "Frontend" });
        }
        #endregion
    }
}