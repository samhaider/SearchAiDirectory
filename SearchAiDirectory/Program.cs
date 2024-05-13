using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SearchAiDirectory.Services;
using System.ComponentModel.Design;
using System.Net;

namespace SearchAiDirectory;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        // Add services to the container.
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

        builder.Services.AddDistributedMemoryCache();
        builder.Services.Configure<CookieTempDataProviderOptions>(options =>
        {
            options.Cookie.Name = "TempDataCookie";
            options.Cookie.Expiration = TimeSpan.FromMinutes(30);
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.MaxAge = TimeSpan.FromMinutes(32);
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Cookie.IsEssential = true;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });
        builder.Services.AddCors(options => options.AddDefaultPolicy(builder =>
        {
            builder
            .WithOrigins(config["WebsiteURL"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }));

        //Adding services to the container
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDbContext<ApplicationDataContext>(options => options.UseSqlServer(
            config.GetConnectionString("Default"),
            sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(1), errorNumbersToAdd: null)));

        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<IToolService, ToolService>();
        builder.Services.AddSingleton<JWTokenService>(new JWTokenService(builder.Configuration["WebsiteURL"]));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
        else app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCookiePolicy();
        app.UseSession();
        app.UseCors();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

public class ErrorHandlerMiddleware(IServiceScopeFactory serviceScopeFactory, RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            var newLog = new Log
            {
                User = context.User is not null && context.User.Claims is not null && context.User.Claims.Any(a => a.Type == "unique_name")
                ? context.User.Claims.First(f => f.Type == "unique_name").Value : string.Empty,

                UserIp = !string.IsNullOrEmpty(context.Connection.RemoteIpAddress.ToString())
                ? context.Connection.RemoteIpAddress.ToString() : string.Empty,

                Host = context.Request.Host.HasValue
                ? context.Request.Host.Value : string.Empty,

                Url = context.Request.GetDisplayUrl(),

                Code = error is KeyNotFoundException ? (int)HttpStatusCode.NotFound
                : error is ApplicationException ? (int)HttpStatusCode.BadRequest
                : error is UnauthorizedAccessException ? (int)HttpStatusCode.Forbidden
                : error is ArgumentNullException ? (int)HttpStatusCode.BadRequest
                : error is InvalidOperationException ? (int)HttpStatusCode.BadRequest
                : error is NotSupportedException ? (int)HttpStatusCode.NotImplemented
                : (int)HttpStatusCode.InternalServerError,

                Message = error.Message.Length > 500
                ? error.Message[..500] : error.Message,

                StackTrace = !string.IsNullOrEmpty(error.StackTrace)
                ? error.StackTrace.Length > 1000 ? error.StackTrace[..1000] : error.StackTrace.ToString() : string.Empty,

                Created = DateTime.UtcNow
            };

            using var scope = serviceScopeFactory.CreateScope();
            ILogServices logService = scope.ServiceProvider.GetRequiredService<ILogServices>();
            await logService.Log(newLog);

            context.Response.Redirect("/Error");
        }
    }
}