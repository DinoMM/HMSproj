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
using RecepciaModul.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using HSKModul.ViewModels;
using System.Globalization;
using AdminModul.ViewModels;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using UniComponents.Classes.Services;

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

#if ANDROID
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
#endif

#region pridanie vlastnych env premennych
#if WINDOWS
            var dotenv = Path.Combine(AppContext.BaseDirectory, ".env");  //windows
            //var dotenv = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, ".env"); //endoid nefunguje 
            if (File.Exists(dotenv))
            {
                foreach (var line in File.ReadAllLines(dotenv))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        Environment.SetEnvironmentVariable(parts[0], parts[1]);
                    }
                }
            }
#endif

#endregion
            //pridanie service pre pripojenie na databazu
            {
                string usr;
                string tpswd;
#if DEBUG
                usr = "sa";
                tpswd = Environment.GetEnvironmentVariable("PASSWORD_DB_SA");
#if ANDROID
                 tpswd = SecureStorage.GetAsync("DB_PSWD").GetAwaiter().GetResult() ?? "heslo";
#endif
#else
                //usr = "publicUser";
                usr = "sa";
                //tpswd = Environment.GetEnvironmentVariable("PASSWORD_DB_PUBLICUSER");
                tpswd = Environment.GetEnvironmentVariable("PASSWORD_DB_SA");
#if ANDROID
                //tpswd = SecureStorage.GetAsync("DB_PSWD_PUBLIC").GetAwaiter().GetResult() ?? "heslo";
                tpswd = SecureStorage.GetAsync("DB_PSWD").GetAwaiter().GetResult() ?? "heslo";
#endif
#endif
                var dbsource = SecureStorage.GetAsync("DB_Source").GetAwaiter().GetResult() ?? "localhost";
                var dbport = SecureStorage.GetAsync("DB_Port").GetAwaiter().GetResult() ?? "1433";

                builder.Services.AddDbContext<DBContext>(opt => {
                    opt.UseSqlServer($"Data Source={dbsource},{dbport};Database=MyDatabase;User ID={usr};Password={tpswd};Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False", sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                        );
                }

                );
                    //opt.AddInterceptors(new DelayCommandInterceptor(TimeSpan.FromSeconds(1)));  //Testovacie
                });               //(pomohol som si z internetu tutoriály/AI)

                builder.Services.AddDbContext<DataContext>(opt => {
                    opt.UseSqlServer($"Data Source={dbsource},{dbport};Database=HlavnaDatabaza;User ID={usr};Password={tpswd};Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False", sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                        );
                });
                    //opt.AddInterceptors(new DelayCommandInterceptor(TimeSpan.FromSeconds(1)));  //Testovacie
                });               //(pomohol som si z internetu tutoriály/AI)
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

            builder.Services.AddDataProtection()
                .PersistKeysToDbContext<DBContext>()
                .SetDefaultKeyLifetime(TimeSpan.FromDays(7)) //TODO zmenit na vacsi pocet dni
                .SetApplicationName("HMS_app"); //pridanie scopu na ochranu dat -> Microsoft.AspNetCore.DataProtection

            DataEncryptor.Init(builder.Services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>()); //inicializacia DataEncryptor

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
            builder.Services.AddSingleton<TransientHolderService>();    //pre manazment Transient objektov na dlhšie ako jedna stranka

            builder.Services.AddSingleton<LayoutService>();

            builder.Services.AddBlazoredSessionStorage();   //pridanie service pre spravu session storage (flagov)

            #region ViewModels
            builder.Services.AddScoped<IndexViewModel>();
            builder.Services.AddTransient<LoginViewModel>();

            #region SkladModul
            builder.Services.AddTransient<AddObjednavViewModel>();
            builder.Services.AddScoped<PridPolozkyViewModel>();
            builder.Services.AddTransient<ObjednavkaViewModel>();

            builder.Services.AddTransient<SkladViewModel>();
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

            builder.Services.AddTransient<RoomsViewModel>();
            builder.Services.AddTransient<CRURoomViewModel>();
            #endregion
            #region HSKModul
            builder.Services.AddTransient<HousekeepingViewModel>();
            builder.Services.AddTransient<CRURoomCompViewModel>();
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
            builder.Services.AddTransient<CRUFakturaViewModel>();

            builder.Services.AddTransient<UniConItemyViewModel>();
            builder.Services.AddTransient<CRUUniItemViewModel>();

            builder.Services.AddTransient<ZmenarenViewModel>();
            #endregion
            #region RecepciaModul
            builder.Services.AddTransient<RezervaciaViewModel>();

            builder.Services.AddTransient<HostiaViewModel>();
            builder.Services.AddTransient<CRUHostViewModel>();
            builder.Services.AddTransient<HostFlagyViewModel>();
            builder.Services.AddTransient<HostConFlagyViewModel>();

            builder.Services.AddTransient<KasaViewModel>();
            builder.Services.AddTransient<PokladnicnyDokladViewModel>();
            builder.Services.AddTransient<PridatItemDokladuViewModel>();
            builder.Services.AddTransient<CRUDKasaViewModel>();
            builder.Services.AddTransient<KontrolaUzavretiaViewModel>();
            #endregion


            #endregion
            #region Misc
            builder.Services.AddTransient(typeof(TransientPageHolder<>));   //pridavanie Transient objektov do/z TransientHolderService
            builder.Services.AddSingleton(typeof(ICascadingService), typeof(CascadingService));
            #endregion



//#if DEBUG
//            builder.Services.AddBlazorWebViewDeveloperTools();
//            builder.Logging.AddDebug();
//#endif


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
