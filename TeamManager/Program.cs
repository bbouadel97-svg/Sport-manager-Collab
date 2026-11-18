using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeamManager.Components;
using TeamManager.Data;
using TeamManager.Models;
using TeamManager.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey) || Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Jwt:SigningKey with at least 32 bytes using user-secrets or an environment variable.");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<SportManagerDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("SportManager")!));

builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<AvatarService>();
builder.Services.AddScoped<PositionService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<MatchSimulationService>();
builder.Services.AddScoped<InjuryService>();
builder.Services.AddControllersWithViews();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "AppAuth";
        options.DefaultChallengeScheme = "AppAuth";
        options.DefaultForbidScheme = "AppAuth";
    })
    .AddPolicyScheme("AppAuth", "JWT or cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Path.StartsWithSegments("/api")
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/login";
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SportManager",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SportManager.Mobile",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jwtId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jwtId))
                {
                    context.Fail("Le jeton ne contient pas d’identifiant.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<SportManagerDbContext>();
                if (await dbContext.RevokedTokens.AnyAsync(token => token.JwtId == jwtId))
                {
                    context.Fail("Le jeton a été révoqué.");
                }
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterWebDev", policy => policy
        .SetIsOriginAllowed(origin =>
            builder.Environment.IsDevelopment() &&
            Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
            (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build(); // c'est ici que l'application est construite

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute(
        "/not-found",
        createScopeForStatusCodePages: true));

app.UseHttpsRedirection();

app.UseCors("FlutterWebDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "account",
    pattern: "account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });
app.MapControllerRoute(
    name: "mvc",
    pattern: "mvc/{controller=Teams}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SportManagerDbContext>();
    dbContext.Database.EnsureCreated();
    await AuthSchemaInitializer.EnsureCreatedAsync(dbContext);
    SportManagerSeeder.Seed(dbContext);
}

app.Run();