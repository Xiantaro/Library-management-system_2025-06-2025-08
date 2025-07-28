using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using test2.Models;
using test2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using test2.Models.ManagementModels.Services;

var facebookAppId = Environment.GetEnvironmentVariable("FACEBOOK_APP_ID");
var facebookAppSecret = Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET");
var googleClentId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

var builder = WebApplication.CreateBuilder(args);

//scoped servise
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<UserService>();

//database service
builder.Services.AddDbContext<Test2Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Test2ConnString")));

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

//memory service
builder.Services.AddDistributedMemoryCache();

//排程 host servise
//builder.Services.AddHostedService<ScheduleServices>();

//session service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//cookie service
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Frontend/Home/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    })
    .AddCookie(IdentityConstants.ExternalScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.Cookie.Name = "Identity.External";
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = googleClentId!;
        googleOptions.ClientSecret = googleClientSecret!;
        googleOptions.Scope.Add("profile");
        googleOptions.Scope.Add("email");

        // 當 Google 拒絕存取 (例如使用者按取消) 時，導向到這個路徑
        googleOptions.AccessDeniedPath = "/Frontend/Home/Index";
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = facebookAppId!;
        facebookOptions.AppSecret = facebookAppSecret!;
        facebookOptions.Scope.Add("public_profile");
        facebookOptions.Scope.Add("email");

        // 當 Facebook 拒絕存取 (例如使用者按取消) 時，導向到這個路徑
        facebookOptions.AccessDeniedPath = "/Frontend/Home/Index";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapStaticAssets();

//session start
app.UseSession();

//cookie start
app.UseAuthentication();
app.UseAuthorization();

//fronted area
app.MapControllerRoute(
    name: "FrontendArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Frontend" }
);

//backed area
app.MapControllerRoute(
    name: "BackendArea",
    pattern: "{area:exists}/{controller=Manage}/{action=Index}/{id?}",
    defaults: new { area = "Backend" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();