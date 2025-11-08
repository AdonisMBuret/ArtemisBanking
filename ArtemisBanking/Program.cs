using ArtemisBanking.Infrastructure;
using ArtemisBanking.Infrastructure.Data;
using Hangfire;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllersWithViews();

// Agregar servicios de la capa de infraestructura (DbContext, Identity, Repositorios, Servicios)
builder.Services.AgregarInfraestructura(builder.Configuration);

// Configurar políticas de autorización por roles
builder.Services.AddAuthorization(options =>
{
    // Política para administradores
    options.AddPolicy("SoloAdministrador", policy =>
        policy.RequireRole("Administrador"));

    // Política para cajeros
    options.AddPolicy("SoloCajero", policy =>
        policy.RequireRole("Cajero"));

    // Política para clientes
    options.AddPolicy("SoloCliente", policy =>
        policy.RequireRole("Cliente"));
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

// Activar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configurar Hangfire Dashboard (solo accesible para administradores en producción)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Configurar los jobs recurrentes de Hangfire
DependencyInjection.ConfigurarJobsRecurrentes();

// Configurar las rutas de los controladores
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
        
        // Permitir acceso solo si el usuario está autenticado y es administrador
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Administrador");
    }
}