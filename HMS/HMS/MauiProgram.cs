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
using AdminModul.ViewModels.Pouzivatelia;
using UniComponents.Services;
using Blazored.SessionStorage;

using LudskeZdrojeModul.ViewModels.SpravaRoli;
using SkladModul;
using SkladModul.ViewModels.Dodavatelia;
using LudskeZdrojeModul.ViewModels.Zamestnanci;
using SkladModul.ViewModels.Sklady;
using UctovnyModul.ViewModels;
using UctovnyModul.ViewModels.Faktury;
using RecepciaModul.ViewModels;


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
            {
                string usr;
                string tpswd;
#if DEBUG
                usr = "sa";
                tpswd = "TaJnEhEsLo!!!123456789";
#else
                usr = "publicUser";
                tpswd = "TaJnEhEsLo???123456789";
#endif
                builder.Services.AddDbContext<DBContext>(opt => opt.UseSqlServer($"Data Source=localhost,1433;Database=MyDatabase;User ID={usr};Password={tpswd};Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False", sqlServerOptionsAction: sqlOptions => {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null
                        );
                }
                ));               //(pomohol som si z internetu tutoriály/AI)

                builder.Services.AddDbContext<DataContext>(opt => opt.UseSqlServer($"Data Source=localhost,1433;Database=HlavnaDatabaza;User ID={usr};Password={tpswd};Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False", sqlServerOptionsAction: sqlOptions => {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null
                        );
                }));               //(pomohol som si z internetu tutoriály/AI)
            }
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
                .AddRoles<IdentityRole>()
                .AddUserValidator<CustomUserValidator<IdentityUserOwn>>();      //(pomohol som si z internetu tutoriály/AI)


            //builder.Services.AddBlazorWebView()
            // .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUserOwn>>();

            //builder.Services.AddAuthentication();
            //builder.Services.AddAuthorizationCore();
            //builder.Services.TryAddScoped<AuthenticationStateProvider, CurrentThreadUserAuthenticationStateProvider>();

            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<RoleService>();
            builder.Services.AddSingleton<DbInitializeService>();
            builder.Services.AddSingleton<Navigator>();
            builder.Services.AddSingleton<ObjectHolder>();
            builder.Services.AddSingleton<IAppLifeCycleService, AppLifecycleService>();

            builder.Services.AddSingleton<LayoutService>();

            builder.Services.AddBlazoredSessionStorage();   //pridanie service pre spravu session storage (flagov)

            #region ViewModels
            builder.Services.AddScoped<IndexViewModel>();
            builder.Services.AddTransient<LoginViewModel>();

            #region SkladModul
            builder.Services.AddTransient<AddObjednavViewModel>();
            builder.Services.AddScoped<PridPolozkyViewModel>();
            builder.Services.AddTransient<ObjednavkaViewModel>();

            builder.Services.AddScoped<SkladViewModel>();
            builder.Services.AddTransient<ModifPolozSkladViewModel>();
            builder.Services.AddTransient<PrijemPolozViewModel>();
            builder.Services.AddScoped<ModifPrijemkaViewModel>();
            builder.Services.AddTransient<ModifPolozPohJednotkaViewModel>();
            builder.Services.AddTransient<VydajPolozViewModel>();
            builder.Services.AddScoped<ModifVydajkaViewModel>();
            builder.Services.AddTransient<UzavierkaViewModel>();

            builder.Services.AddTransient<DodavateliaViewModel>();
            builder.Services.AddTransient<CRUDodavatelViewModel>();
            #endregion
            #region AdminModul
            builder.Services.AddTransient<PouzivateliaViewModel>();
            builder.Services.AddTransient<AddPouzivatelViewModel>();
            builder.Services.AddTransient<ZmenaPouzivatelaViewModel>();
            #endregion
            #region LudskeZdrojeModul
            builder.Services.AddTransient<SpravaRoliViewModel>();
            builder.Services.AddTransient<AddRoleViewModel>();
            builder.Services.AddTransient<ZmenaRoleViewModel>();

            builder.Services.AddTransient<ZamViewModel>();
            builder.Services.AddTransient<CRUZamestnanciViewModel>();

            builder.Services.AddTransient<SkladyViewModel>();
            builder.Services.AddTransient<CRUSkladViewModel>();
            #endregion
            #region UctovnyModul
            builder.Services.AddTransient<FakturyViewModel>();

            builder.Services.AddTransient<UniConItemyViewModel>();
            builder.Services.AddTransient<CRUUniItemViewModel>();
            #endregion
            #region RecepciaModul
            builder.Services.AddTransient<RezervaciaViewModel>();

            builder.Services.AddTransient<HostiaViewModel>();
            builder.Services.AddTransient<CRUHostViewModel>();

            builder.Services.AddTransient<KasaViewModel>();
            builder.Services.AddTransient<PokladnicnyDokladViewModel>();
            builder.Services.AddTransient<PridatItemDokladuViewModel>();
            builder.Services.AddTransient<CRUDKasaViewModel>();
            builder.Services.AddTransient<KontrolaUzavretiaViewModel>();
            #endregion


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
