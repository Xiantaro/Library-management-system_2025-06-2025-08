using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Security.Claims;
using test2.Areas.Frontend.Models;
using test2.Areas.Frontend.Models.Dtos;
using test2.Models;


namespace test2.Services
{
    public class RegisterResult
    {
        public bool IsSuccess;
        public string FailMessage = "無錯誤訊息";
        public string SuccessMessage = "無成功訊息";
    }
    public class UserService
    {
        private readonly Test2Context _context;

        public UserService(Test2Context context)
        {
            _context = context;
        }

        /// <summary>
        /// 使用 EF core 將新的用戶資料傳入資料庫
        /// </summary>
        public async Task<RegisterResult> RegisterNewUser(string name, string phoneNumber, string email, string password)
        {
            RegisterResult result = new RegisterResult();

            bool isEmailRegistered = await IsEmailRegisteredAsync(email);
            bool isPhoneRegistered = await _context.Clients.AnyAsync(c => c.CPhone == phoneNumber);

            if (isEmailRegistered)
            {
                result.IsSuccess = false;
                result.FailMessage = "電子信箱已經註冊過";
                return result;
            }

            else if (isPhoneRegistered)
            {
                result.IsSuccess = false;
                result.FailMessage = "電話號碼已經註冊過";
                return result;
            }

            else
            {
                var newUser = new Client
                {
                    CName = name,
                    CAccount = email,
                    CPassword = Argon2.Hash(password), // 使用 Argon2 加密
                    CPhone = phoneNumber,
                    Permission = 1
                };

                _context.Clients.Add(newUser);
                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.SuccessMessage = "註冊成功！";

                return result;
            }
        }

        /// <summary>
        /// 使用 EF core 將新的用戶資料傳入資料庫並且自動綁定指定的外部登入方式
        /// </summary>
        public async Task<RegisterResult> RegisterExternalNewUser(string name, string phoneNumber, string email, string password, string providerName, string externalUserId)
        {
            RegisterResult result = new RegisterResult();

            // 外部登入時，會先檢查電子信箱是否存在資料庫，所以不需要再檢查電子信箱是否已註冊

            // 檢查電話號碼是否已經註冊過
            bool isPhoneRegistered = await _context.Clients.AnyAsync(c => c.CPhone == phoneNumber);

            if (isPhoneRegistered)
            {
                result.IsSuccess = false;
                result.FailMessage = "電話號碼已經註冊過";
                return result;
            }

            // 根據提供者建立新使用者物件

            Client newUser = new Client
            {
                CName = name,
                CAccount = email,
                CPassword = Argon2.Hash(password), // 使用 Argon2 加密
                CPhone = phoneNumber,
                Permission = 1
            };

            switch (providerName)
            {
                case "Facebook":
                    newUser.FacebookId = externalUserId;
                    break;
                case "Google":
                    newUser.GoogleId = externalUserId;
                    break;
                default:
                    result.IsSuccess = false;
                    result.FailMessage = "無效的外部驗證提供者";
                    return result;
            }

            _context.Clients.Add(newUser);
            await _context.SaveChangesAsync();

            result.IsSuccess = true;
            result.SuccessMessage = providerName switch
            {
                "Facebook" => $"註冊成功！我們已使用您的 Facebook 帳號電子信箱（{email}）為您建立新帳號，並已自動綁定 Facebook 登入方式。現在您可以直接使用 Facebook 快速登入囉！",
                "Google" => $"註冊成功！我們已使用您的 Google 帳號電子信箱（{email}）為您建立新帳號，並已自動綁定 Google 登入方式。現在您可以直接使用 Google 快速登入囉！",
                _ => throw new ArgumentOutOfRangeException(nameof(providerName), $"不支援的外部驗證提供者名稱：{providerName}") // 拋出例外
            };

            return result;
        }

        /// <summary>
        /// 使用 EF core 查詢資料庫中某個 email 是否已經被註冊
        /// </summary>
        /// <param name="email">要檢查的電子郵件地址。</param>
        /// <returns>如果 email 已被註冊，則回傳 true；否則回傳 false。</returns>
        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            return await _context.Clients.AnyAsync(c => c.CAccount == email);
        }

        /// <summary>
        /// 使用 EF core 查詢資料庫中某個 外部 User ID 是否已經被註冊
        /// </summary>
        /// <param name="email">要檢查的電子郵件地址。</param>
        /// <returns>如果 email 已被註冊，則回傳 true；否則回傳 false。</returns>
        public async Task<bool> IsExternalIdRegisteredAsync(string providerName, string externalUserId)
        {
            if (providerName == "Google")
            {
                return await _context.Clients.AllAsync(c => c.GoogleId == externalUserId);
            }
            else if (providerName == "Facebook")
            {
                return await _context.Clients.AnyAsync(c => c.FacebookId == externalUserId);
            }
            else
            {
                throw new ArgumentException($"無效的提供者名稱: {providerName}。", nameof(providerName));
            }
        }

        /// <summary>
        /// 根據傳入的使用者獲得使用者聲明。
        /// </summary>
        /// <param name="user">要登入的使用者資料。</param>
        /// <returns>回傳對應的 ClaimsIdentity 物件</returns>
        public async Task<ClaimsPrincipal> CreateUserClaimsPrincipalAsync(Client user)
        {
            // 載入使用者狀態
            var borrowStatus = await _context.Borrows.AsNoTracking().AnyAsync(x => x.CId == user!.CId && x.BorrowStatusId == 3);
            var borrowCount = await _context.Borrows.CountAsync(x => x.CId == user!.CId);

            var claims = new List<Claim>{
                new Claim(InternalClaimTypes.IAccount, user!.CAccount),
                new Claim(InternalClaimTypes.IId, user!.CId.ToString()),
                new Claim(InternalClaimTypes.IName, user!.CName),
                new Claim(InternalClaimTypes.IBorrowStatus, borrowStatus.ToString()),
                new Claim(InternalClaimTypes.IBorrowCount, borrowCount.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
