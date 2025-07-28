using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using test2.Areas.Frontend.Models;

namespace test2.Controllers
{
    public abstract class UserController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userAccount = User.FindFirst(InternalClaimTypes.IAccount);
            var userId = User.FindFirst(InternalClaimTypes.IId);
            var userName = User.FindFirst(InternalClaimTypes.IName);
            var userBorrowStatus = User.FindFirst(InternalClaimTypes.IBorrowStatus);
            var userBorrowCount = User.FindFirst(InternalClaimTypes.IBorrowCount);
            var userPermission = User.FindFirst(InternalClaimTypes.IPermission);

            ViewData["Account"] = (userAccount != null) ? userAccount.Value : string.Empty;
            ViewData["Id"] = (userId != null) ? userId.Value : string.Empty;
            ViewData["Name"] = (userName != null) ? userName.Value : string.Empty;
            ViewData["BorrowStatus"] = (userBorrowStatus != null) ? userBorrowStatus.Value : string.Empty;
            ViewData["BorrowCount"] = (userBorrowCount != null) ? userBorrowCount.Value : string.Empty;
            ViewData["Permission"] = (userPermission != null) ? userPermission.Value : string.Empty;

            base.OnActionExecuting(context);
        }
    }
}