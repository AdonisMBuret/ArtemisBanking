using ArtemisBanking.Application.Common;
using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Infrastructure.Data
{
    /// <summary>
    /// Clase que inicializa la base de datos con datos por defecto
    /// Se ejecuta al iniciar la aplicación
    /// Crea los roles y usuarios de prueba para poder empezar a usar el sistema
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Método principal que inicializa todo
        /// Se llama desde Program.cs al iniciar la aplicación
        /// </summary>
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            // Obtener los servicios necesarios
            var context = serviceProvider.GetRequiredService<ArtemisBankingDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplicar migraciones pendientes (crear tablas si no existen)
            await context.Database.MigrateAsync();

            // Crear los roles del sistema
            await CrearRolesAsync(roleManager);

            // Crear usuarios de prueba
            await CrearUsuariosPorDefectoAsync(userManager, context);
        }

        /// <summary>
        /// Crea los 3 roles del sistema si no existen
        /// </summary>
        private static async Task CrearRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Lista de roles que necesita el sistema
            string[] roles = { 
                Constantes.RolAdministrador, 
                Constantes.RolCajero, 
                Constantes.RolCliente 
            };

            // Por cada rol, verificar si existe, si no, crearlo
            foreach (var rol in roles)
            {
                // ¿Existe este rol en la base de datos?
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    // No existe, entonces lo creamos
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }

        /// <summary>
        /// Crea 3 usuarios de prueba (uno de cada tipo) si no existen
        /// Esto nos permite probar el sistema sin tener que registrar usuarios manualmente
        /// </summary>
        private static async Task CrearUsuariosPorDefectoAsync(
            UserManager<Usuario> userManager, 
            ArtemisBankingDbContext context)
        {
            // ==================== CREAR USUARIO ADMINISTRADOR ====================
            // Verificar si ya existe un administrador con este username
            if (await userManager.FindByNameAsync("admin") == null)
            {
                // Crear el usuario administrador
                var adminUser = new Usuario
                {
                    UserName = "admin",
                    Email = "admin@artemisbanking.com",
                    Nombre = "Administrador",
                    Apellido = "Sistema",
                    Cedula = "00000000001",
                    EstaActivo = true, // El admin se crea activo directamente
                    FechaCreacion = DateTime.Now,
                    EmailConfirmed = true // Confirmar el email automáticamente
                };

                // Crear el usuario con contraseña
                var resultado = await userManager.CreateAsync(adminUser, "Admin123!");

                if (resultado.Succeeded)
                {
                    // Asignarle el rol de Administrador
                    await userManager.AddToRoleAsync(adminUser, Constantes.RolAdministrador);
                }
            }

            // ==================== CREAR USUARIO CAJERO ====================
            // Verificar si ya existe un cajero con este username
            if (await userManager.FindByNameAsync("cajero") == null)
            {
                // Crear el usuario cajero
                var cajeroUser = new Usuario
                {
                    UserName = "cajero",
                    Email = "cajero@artemisbanking.com",
                    Nombre = "Cajero",
                    Apellido = "Prueba",
                    Cedula = "00000000002",
                    EstaActivo = true, // El cajero se crea activo directamente
                    FechaCreacion = DateTime.Now,
                    EmailConfirmed = true
                };

                // Crear el usuario con contraseña
                var resultado = await userManager.CreateAsync(cajeroUser, "Cajero123!");

                if (resultado.Succeeded)
                {
                    // Asignarle el rol de Cajero
                    await userManager.AddToRoleAsync(cajeroUser, Constantes.RolCajero);
                }
            }

            // ==================== CREAR USUARIO CLIENTE ====================
            // Verificar si ya existe un cliente con este username
            if (await userManager.FindByNameAsync("cliente") == null)
            {
                // Crear el usuario cliente
                var clienteUser = new Usuario
                {
                    UserName = "cliente",
                    Email = "cliente@artemisbanking.com",
                    Nombre = "Cliente",
                    Apellido = "Prueba",
                    Cedula = "00000000003",
                    EstaActivo = true, // El cliente se crea activo directamente
                    FechaCreacion = DateTime.Now,
                    EmailConfirmed = true
                };

                // Crear el usuario con contraseña
                var resultado = await userManager.CreateAsync(clienteUser, "Cliente123!");

                if (resultado.Succeeded)
                {
                    // Asignarle el rol de Cliente
                    await userManager.AddToRoleAsync(clienteUser, Constantes.RolCliente);

                    // ==================== CREAR CUENTA DE AHORRO PRINCIPAL ====================
                    // Todos los clientes necesitan una cuenta de ahorro principal
                    // Generar número de cuenta único de 9 dígitos
                    var random = new Random();
                    string numeroCuenta;
                    
                    do
                    {
                        // Generar 9 dígitos aleatorios
                        numeroCuenta = "";
                        for (int i = 0; i < 9; i++)
                        {
                            numeroCuenta += random.Next(0, 10).ToString();
                        }
                    }
                    // Repetir hasta que el número sea único
                    while (await context.CuentasAhorro.AnyAsync(c => c.NumeroCuenta == numeroCuenta));

                    // Crear la cuenta de ahorro principal
                    var cuentaPrincipal = new CuentaAhorro
                    {
                        NumeroCuenta = numeroCuenta,
                        Balance = 10000, // Balance inicial de RD$10,000 para hacer pruebas
                        EsPrincipal = true,
                        EstaActiva = true,
                        UsuarioId = clienteUser.Id,
                        FechaCreacion = DateTime.Now
                    };

                    // Guardar la cuenta en la base de datos
                    context.CuentasAhorro.Add(cuentaPrincipal);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}