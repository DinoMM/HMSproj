using HMS.ViewModels;
using Microsoft.Extensions.Logging;
using DBLayer;
using static HMS.Components.Routes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkladModul.ViewModels.Objednavka;
using UniComponents;
using SkladModul.ViewModels.Sklad;


namespace HMS
{
    public static class MauiProgram
    {

        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();



            //pridanie service pre pripojenie na databazu
            builder.Services.AddDbContext<DBContext>(opt => opt.UseSqlServer("Data Source=localhost,1433;Database=MyDatabase;User ID=sa;Password=TaJnEhEsLo!!!123456789;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False")
            .EnableSensitiveDataLogging()); //vymazat               //(pomohol som si z internetu tutoriály/AI)
            //pridanie service pre spravu usera a jeho role
            builder.Services.AddIdentity<IdentityUserOwn, IdentityRole>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequiredLength = 6;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.SignIn.RequireConfirmedEmail = false;

            }).AddEntityFrameworkStores<DBContext>()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>();      //(pomohol som si z internetu tutoriály/AI)

            //builder.Services.AddBlazorWebView()
            // .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUserOwn>>();

            //builder.Services.AddAuthentication();
            //builder.Services.AddAuthorizationCore();
            //builder.Services.TryAddScoped<AuthenticationStateProvider, CurrentThreadUserAuthenticationStateProvider>();

            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<DbInitializeService>();
            builder.Services.AddSingleton<Navigator>();
            builder.Services.AddSingleton<ObjectHolder>();

            builder.Services.AddSingleton<LayoutService>();

            #region ViewModels
            builder.Services.AddScoped<IndexViewModel>();

            builder.Services.AddTransient<AddObjednavViewModel>();
            builder.Services.AddScoped<PridPolozkyViewModel>();
            builder.Services.AddTransient<ObjednavkaViewModel>();

            builder.Services.AddTransient<SkladViewModel>();
            builder.Services.AddTransient<ModifPolozSkladViewModel>();
            #endregion


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    //public class CurrentThreadUserAuthenticationStateProvider : AuthenticationStateProvider
    //{
    //    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
    //        Task.FromResult(
    //            new AuthenticationState(Thread.CurrentPrincipal as ClaimsPrincipal ??
    //                new ClaimsPrincipal(new ClaimsIdentity())));
    //}


}
