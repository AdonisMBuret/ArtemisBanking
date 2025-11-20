using ArtemisBanking.Application;
using ArtemisBanking.Infrastructure;
using ArtemisBanking.Infrastructure.Data;
using Hangfire;
using Hangfire.Dashboard;
using DependencyInjection = ArtemisBanking.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AgregarAplicacion();

builder.Services.AgregarInfraestructura(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdministrador", policy =>
        policy.RequireRole("Administrador"));

    options.AddPolicy("SoloCajero", policy =>
        policy.RequireRole("Cajero"));

    options.AddPolicy("SoloCliente", policy =>
        policy.RequireRole("Cliente"));
    
    options.AddPolicy("SoloComercio", policy =>
        policy.RequireRole("Comercio"));
});

var app = builder.Build();

// Inicializar la base de datos con datos por defecto (roles y usuarios)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.InicializarAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos");
    }
}

// Configurar el pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configurar Hangfire Dashboard (solo accesible para administradores en producción)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

DependencyInjection.ConfigurarJobsRecurrentes();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

// Filtro de autorización para el dashboard de Hangfire
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Administrador");
    }
}