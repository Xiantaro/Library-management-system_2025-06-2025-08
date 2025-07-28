using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using test2.Areas.Frontend.Models;
using test2.Areas.Frontend.Models.Dtos;
using test2.Models;
using test2.Services;


[Area("Frontend")]
public class AccountController : Controller
{
    private readonly Test2Context _context;
    private readonly UserService _userService;

    public AccountController(Test2Context context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    /// <summary>
    /// 啟動外部登入流程 (例如 Google 或 Facebook)
    /// </summary>
    /// <param name="provider">外部驗證提供者的名稱 (例如 "Google", "Facebook")</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult ExternalLogin(string provider)
    {
        // 建立 AuthenticationProperties 物件，指定登入成功後要重新導向的 URI
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { Area = "Frontend" });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        // 觸發外部驗證流程
        // provider 會是 "Google" 或 "Facebook" (ASP.NET Core 會自動對應到 GoogleDefaults.AuthenticationScheme 或 FacebookDefaults.AuthenticationScheme)
        return Challenge(properties, provider);
    }

    /// <summary>
    /// 處理外部登入完成後的回呼
    /// 外部驗證提供者會將使用者導向到在其平台上設定的 /signin-google 或 /signin-facebook
    /// 然後 ASP.NET Core 的驗證中介軟體會處理它，並將使用者導向到這個 ExternalLoginCallback
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        // 嘗試取得外部登入的驗證結果
        // 這會從暫存的外部認證資訊中讀取。
        // 使用 CookieAuthenticationDefaults.AuthenticationScheme 是因為外部驗證成功後，
        // ASP.NET Core 會將外部身份暫時儲存在一個內部 Cookie 中，並使用預設的 Cookie 方案來讀取。
        var externalAuthResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 如果沒有成功驗證，可能原因：使用者取消登入、網路問題等
        if (!externalAuthResult.Succeeded)
        {
            // 導回登入頁面或其他錯誤頁面，並顯示錯誤訊息
            TempData["ErrorMessage"] = "外部登入失敗。";
            return RedirectToAction("LoginC", "Access", new { Area = "Frontend" });
        }

        // 取得外部登入的使用者身份資訊 (Claims)
        // 這些 Claims 包含了來自外部提供者的使用者資料，例如 Name, Email 等
        var externalClaims = externalAuthResult.Principal.Claims;

        // 取得外部提供者提供的的使用者資料
        // User ID 通常是在 ClaimTypes.NameIdentifier
        string externalUserId = externalClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
        string email = externalClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? String.Empty;
        string name = externalClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? String.Empty;
        string providerName = externalAuthResult.Properties?.Items[".AuthScheme"] ?? String.Empty;

        var IsEmailRegistered = await _userService.IsEmailRegisteredAsync(email);

        // 檢查這個 email 是否已經在系統中註冊過
        // 如果 email 已經在系統註冊過
        if (IsEmailRegistered)
        {
            // 載入使用者物件 
            var user = await _context.Clients.FirstOrDefaultAsync(x => x.CAccount == email);

            // 檢查使用者的外部帳號 id 是否已經存在資料庫中

            if (providerName == "Google")
            {
                // 如果使用者的外部帳號 id 已經存在資料庫中
                if (user!.GoogleId == externalUserId){

                    // 執行登入
                    var principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // 登入成功後重新導向到首頁
                    return RedirectToAction("Index", "Home", new { Area = "Frontend" });
                }
                // 如果使用者的外部帳號 Id 不存在資料庫中
                else
                {
                    // 自動將外部帳號 Id 與本地帳號綁定 
                    user!.GoogleId = externalUserId;

                    TempData["Result"] = "success";
                    TempData["ShowModal"] = true;
                    TempData["ResultMessage"] = $"這個外部登入方式（{providerName}）的 Email 已經在我們系統註冊過了，我們已經自動幫你綁定{providerName}登入方式到本地帳號囉！";

                    // 儲存變更
                    await _context.SaveChangesAsync();

                    // 執行登入
                    var principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // 登入成功後重新導向到首頁
                    return RedirectToAction("Index", "Home", new { Area = "Frontend" });
                }
            }
            else if (providerName == "Facebook")
            {
                // 如果使用者的外部帳號 id 已經存在資料庫中
                if (user!.FacebookId == externalUserId)
                {

                    // 執行登入
                    var principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // 登入成功後重新導向到首頁
                    return RedirectToAction("Index", "Home", new { Area = "Frontend" });
                }
                // 如果使用者的外部帳號 Id 不存在資料庫中
                else
                {
                    // 自動將外部帳號 Id 與本地帳號綁定 
                    user!.FacebookId = externalUserId;

                    TempData["Result"] = "success";
                    TempData["ShowModal"] = true;
                    TempData["ResultMessage"] = $"這個外部登入方式（{providerName}）的 Email 已經在我們系統註冊過了，我們已經自動幫你綁定{providerName}登入方式到本地帳號囉！";

                    // 儲存變更
                    await _context.SaveChangesAsync();

                    // 執行登入
                    var principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // 登入成功後重新導向到首頁
                    return RedirectToAction("Index", "Home", new { Area = "Frontend" });
                }
            }
            else
            {
                // 如果外部驗證提供者不是 Google 或 Facebook
                throw new ArgumentException($"無效的提供者名稱: {providerName}。", nameof(providerName));
            }
        }

        // 如果 Email 不存在，表示是新使用者，引導使用者填寫額外的帳號資料
        // 把外部認證提供的資訊存到 TempData 中
        TempData["ExternalUserId"] = externalUserId;
        TempData["Email"] = email;
        TempData["Name"] = name;
        TempData["ProviderName"] = providerName;

        // 清除外部認證資訊
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 重新導向到填寫資料頁面
        return RedirectToAction("ExternalRegistration");
    }

    /// <summary>
    /// 顯示外部登入新使用者註冊表單
    /// </summary>
    /// <returns></returns

    [HttpGet]
    public IActionResult ExternalRegistration()
    {
        ExternalRegistrationDto model = new ExternalRegistrationDto();

        if (TempData["PreFillPhoneNumber"] != null)
        {
            model.PhoneNumber = TempData["PreFillPhoneNumber"] as string ?? string.Empty;
        }
        return View(model);
    }

    /// <summary>
    /// 處裡外部登入新使用者提交的註冊表單
    /// </summary>
    /// <returns></returns
    [HttpPost]
    public async Task<IActionResult> ExternalRegistration(ExternalRegistrationDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string externalUserId = TempData["ExternalUserId"] as string ?? string.Empty;
        string email = TempData["Email"] as string ?? string.Empty;
        string name = TempData["Name"] as string ?? string.Empty;
        string providerName = TempData["ProviderName"] as string ?? string.Empty;

        // 取得註冊結果
        var registerResult = await _userService.RegisterExternalNewUser(name, model.PhoneNumber, email, model.Password, providerName, externalUserId);

        // 如果註冊成功此帳號自動綁定外部登入方式
        if (registerResult.IsSuccess)
        {
            TempData["Result"] = "success";
            TempData["ShowModal"] = true;
            TempData["ResultMessage"] = registerResult.SuccessMessage;

            // 載入使用者物件 
            var user = await _context.Clients.FirstOrDefaultAsync(x => x.CAccount == email);

            // 為這個使用者在應用程式中建立一個新的身份
            var principal = await _userService.CreateUserClaimsPrincipalAsync(user!);

            // 使用 HttpContext.SignInAsync() 讓使用者在應用程式中正式登入
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // 重定向到 GET 版本的 Register 避免重複提交表單問題
            return RedirectToAction("ExternalRegistration");
        }

        else
        {
            TempData["Result"] = "fail";
            TempData["ShowModal"] = true;
            TempData["ResultMessage"] = registerResult.FailMessage;

            // 失敗時，將部分數據存入 TempData
            TempData["PreFillPhoneNumber"] = model.PhoneNumber;

            // 重定向到 GET 版本的 Register 避免重複提交表單問題
            return RedirectToAction("ExternalRegistration");
        }
    }

    /// <summary>
    /// 在個人帳號頁面觸發外部帳號綁定流程
    /// 只有已經登入的用戶才能調用這個功能
    /// </summary>
    /// <param name="provider">外部驗證提供者的名稱 (例如 "Google", "Facebook")</param>
    /// <returns></returns>
        [HttpPost]
        [Authorize] // 確保只有登入的用戶才能執行此操作
        public IActionResult LinkExternalLogin(string provider)
        {
            // 建立 AuthenticationProperties 物件，指定綁定成功後要重新導向的 URI
            // 注意：這裡的回呼 URI 需要是專門處理綁定邏輯的 Action
            var redirectUrl = Url.Action("LinkExternalLoginCallback", "Account", new { Area = "Frontend" });
            var properties = new AuthenticationProperties 
            {
                RedirectUri = redirectUrl
            };

            // 把當前已登入的使用者 email 存到 properties 中，這樣回呼時才能拿到
            properties.Items["currentLoggedInEmail"] = User.Claims.FirstOrDefault(c => c.Type == InternalClaimTypes.IAccount)?.Value ?? String.Empty;

            // 觸發外部驗證流程
            // provider 會是 "Google" 或 "Facebook"
            return Challenge(properties, provider);
        }

    /// <summary>
    /// 處理外部登入綁定完成後的回呼
    /// </summary>
    [HttpGet]
    [Authorize] // 確保只有登入的用戶才能執行此操作
    public async Task<IActionResult> LinkExternalLoginCallback()
    {
        ClaimsPrincipal principal;

        // 1. 嘗試取得外部登入的驗證結果
        var externalAuthResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2. 取得當前登入的本地使用者 email (從 AuthenticationProperties 中取出)
        string currentLoggedInEmail = externalAuthResult.Properties?.Items["currentLoggedInEmail"] ?? string.Empty;

        // 3. 載入之前已登入的本地使用者物件
        // 這裡確保我們是修改當前登入的使用者資料
        var user = await _context.Clients.FirstOrDefaultAsync(x => x.CAccount == currentLoggedInEmail);

        if (!externalAuthResult.Succeeded)
        {
            TempData["Result"] = "fail";
            TempData["ShowModal"] = true;
            TempData["ResultMessage"] = "外部帳號綁定失敗。";

            // 外部帳號綁定失敗後重新登入原本的帳號
            principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Client", "Home", new { Area = "Frontend" }); // 導回帳號管理頁面
        }

        // 4. 取得外部登入的使用者身份資訊 (Claims)
        string externalUserId = externalAuthResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
        string externalEmail = externalAuthResult.Principal.FindFirst(ClaimTypes.Email)?.Value ?? String.Empty;
        string providerName = externalAuthResult.Properties?.Items[".AuthScheme"] ?? String.Empty;

        // 檢查當前登入用戶的 Email 是否與外部驗證回傳的 Email 相符
        if (string.IsNullOrWhiteSpace(currentLoggedInEmail) || !string.Equals(currentLoggedInEmail, externalEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Result"] = "fail";
            TempData["ShowModal"] = true;
            TempData["ResultMessage"] = "綁定失敗：外部帳號 Email 與當前登入帳號 Email 不符。";

            // 外部帳號綁定失敗後重新登入原本的帳號
            principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Client", "Home", new { Area = "Frontend" }); // 導回帳號管理頁面
        }

        if (user == null)
        {
            TempData["Result"] = "fail";
            TempData["ShowModal"] = true;
            TempData["ResultMessage"] = "綁定失敗：找不到當前登入的帳號。";
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("AccountManagement", "Account", new { Area = "Frontend" });
        }

        bool isBindingSucceeded = false;
        string successMessage = "";

        // 5. 根據提供者更新使用者的外部 ID
        if (providerName == GoogleDefaults.AuthenticationScheme) // "Google"
        {
            if (!string.IsNullOrEmpty(user.GoogleId) && user.GoogleId != externalUserId)
            {
                // 如果已經綁定過其他 Google 帳號 ID (不常見，但以防萬一)
                TempData["ResultMessage"] = "綁定失敗：此帳號已綁定其他 Google 帳號。";
            }
            else if (user.GoogleId == externalUserId)
            {
                // 已經綁定過同一個 Google 帳號
                TempData["ResultMessage"] = "您已綁定過此 Google 帳號。"; // 提示而不是錯誤
            }
            else
            {
                user.GoogleId = externalUserId;
                successMessage = "Google 帳號綁定成功！";
                isBindingSucceeded = true;
            }
        }
        else if (providerName == FacebookDefaults.AuthenticationScheme) // "Facebook"
        {
            if (!string.IsNullOrEmpty(user.FacebookId) && user.FacebookId != externalUserId)
            {
                // 如果已經綁定過其他 Facebook 帳號 ID
                TempData["ResultMessage"] = "綁定失敗：此帳號已綁定其他 Facebook 帳號。";
            }
            else if (user.FacebookId == externalUserId)
            {
                // 已經綁定過同一個 Facebook 帳號
                TempData["ResultMessage"] = "您已綁定過此 Facebook 帳號。"; // 提示而不是錯誤
            }
            else
            {
                user.FacebookId = externalUserId;
                successMessage = "Facebook 帳號綁定成功！";
                isBindingSucceeded = true;
            }
        }
        else
        {
            TempData["ResultMessage"] = $"無效的外部提供者: {providerName}。";
        }

        // 6. 儲存變更到資料庫
        if (isBindingSucceeded)
        {
            await _context.SaveChangesAsync();
        }

        // 清除外部認證資訊，避免殘留
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 重新登入
        principal = await _userService.CreateUserClaimsPrincipalAsync(user!);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


        TempData["Result"] = "success";
        TempData["ShowModal"] = true;
        TempData["ResultMessage"] = successMessage;

        // 導回帳號管理頁面
        return RedirectToAction("Client", "Home", new { Area = "Frontend" });
    }

    /// <summary>
    /// 登出功能
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken] // 加上防偽造標記，避免 CSRF 攻擊
    public async Task<IActionResult> Logout()
    {
        // 清除所有驗證 Cookie，讓使用者登出
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 登出後導向到首頁或登入頁面
        return RedirectToAction("LoginC", "Access", new { Area = "Frontend" });
    }
}