using DBLayer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region pridanie vlastnych env premennych
var dotenv = Path.Combine(AppContext.BaseDirectory, ".env");
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
#endregion
#region pridanie service pre pripojenie na databazu
{
    string usr;
    string tpswd;

#if DEBUG
    usr = "sa";
    tpswd = Environment.GetEnvironmentVariable("PASSWORD_DB_SA");
#else
                usr = "publicUser";
                tpswd = Environment.GetEnvironmentVariable("PASSWORD_DB_PUBLICUSER");;
#endif
    builder.Services.AddDbContext<DBContext>(opt => opt.UseSqlServer($"Data Source=localhost,1433;Database=MyDatabase;User ID={usr};Password={tpswd};Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False", sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null
            );
    }
    ));               //(pomohol som si z internetu tutoriály/AI)

    builder.Services.AddDbContext<DataContext>(opt => opt.UseSqlServer($"Data Source=localhost,1433;Database=HlavnaDatabaza;User ID={usr};Password={tpswd};Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False", sqlServerOptionsAction: sqlOptions =>
    {
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
#endregion

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<DBContext>()
    .SetDefaultKeyLifetime(TimeSpan.FromDays(7)) //TODO zmenit na vacsi pocet dni
    .SetApplicationName("HMS_app"); //pridanie scopu na ochranu dat -> Microsoft.AspNetCore.DataProtection

DBLayer.DataEncryptor.Init(builder.Services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>()); //inicializacia DataEncryptor

HMSModels.DataEncryptor.Init(builder.Services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>()); //inicializacia DataEncryptor

#region JwtAuthConfiguration
{ 
    string jwtKey;  //kluc pre JWT, mozno menit na zaklade buildov ak treba
#if DEBUG
    jwtKey = Environment.GetEnvironmentVariable("JWT_BEARER_SECRET");
#else
    jwtKey = Environment.GetEnvironmentVariable("JWT_BEARER_SECRET");
#endif
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new ArgumentNullException("JWT_BEARER_SECRET is not set in environment variables");
    }
    builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "YourIssuer",      // Set your issuer
        ValidAudience = "YourAudience",  // Set your audience
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
}

builder.Services.AddAuthorization();
#endregion


builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
