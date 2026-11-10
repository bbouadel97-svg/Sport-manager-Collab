using TeamManager.Components;
using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Services;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "mvc",
    pattern: "mvc/{controller=Teams}/{action=Index}/{id?}");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SportManagerDbContext>();
    dbContext.Database.EnsureCreated();
    SportManagerSeeder.Seed(dbContext);
}

app.Run();