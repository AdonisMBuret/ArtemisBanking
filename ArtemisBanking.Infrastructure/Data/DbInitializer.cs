using ArtemisBanking.Domain.Common;
using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBanking.Infrastructure.Data
{
   
    public static class DbInitializer
    {
        /// <summary>
        /// Inicializa la base de datos con roles y usuarios por defecto
        /// </summary>
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            // Obtener los servicios necesarios
            var context = serviceProvider.GetRequiredService<ArtemisBankingDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplicar migraciones pendientes
            await context.Database.MigrateAsync();

            // Crear roles si no existen
            await CrearRolesAsync(roleManager);

            // Crear usuarios por defecto si no existen
            await CrearUsuariosPorDefectoAsync(userManager, context);
        }

        /// <summary>
        /// Crea los roles del sistema: Administrador, Cajero y Cliente
        /// </summary>
        private static async Task CrearRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Array con los roles del sistema
            string[] roles = {
                Constantes.RolAdministrador,
                Constantes.RolCajero,
                Constantes.RolCliente
            };

            // Crear cada rol si no existe
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }

        /// <summary>
        /// Crea tres usuarios por defecto: admin, cajero y cliente
        /// Cada uno con su respectivo rol
        /// </summary>
        private static async Task CrearUsuariosPorDefectoAsync(
            UserManager<Usuario> userManager,
            ArtemisBankingDbContext context)
        {
            // Crear usuario administrador
            await CrearUsuarioSiNoExisteAsync(
                userManager,
                context,
                userName: "admin",
                email: "admin@artemisbanking.com",
                password: "Admin123!",
                nombre: "Administrador",
                apellido: "Sistema",
                cedula: "00000000001",
                rol: Constantes.RolAdministrador,
                activar: true // El admin se crea activo por defecto
            );

            // Crear usuario cajero
            await CrearUsuarioSiNoExisteAsync(
                userManager,
                context,
                userName: "cajero",
                email: "cajero@artemisbanking.com",
                password: "Cajero123!",
                nombre: "Cajero",
                apellido: "Principal",
                cedula: "00000000002",
                rol: Constantes.RolCajero,
                activar: true // El cajero se crea activo por defecto
            );

            // Crear usuario cliente con cuenta de ahorro principal
            var clienteCreado = await CrearUsuarioSiNoExisteAsync(
                userManager,
                context,
                userName: "cliente",
                email: "cliente@artemisbanking.com",
                password: "Cliente123!",
                nombre: "Cliente",
                apellido: "Demo",
                cedula: "00000000003",
                rol: Constantes.RolCliente,
                activar: true, // El cliente demo se crea activo
                montoInicial: 10000.00m // Balance inicial de RD$10,000
            );
        }

        /// <summary>
        /// Crea un usuario si no existe y le asigna su rol
        /// Si es cliente, también crea su cuenta de ahorro principal
        /// </summary>
        private static async Task<Usuario> CrearUsuarioSiNoExisteAsync(
            UserManager<Usuario> userManager,
            ArtemisBankingDbContext context,
            string userName,
            string email,
            string password,
            string nombre,
            string apellido,
            string cedula,
            string rol,
            bool activar = false,
            decimal montoInicial = 0)
        {
            // Verificar si el usuario ya existe
            var usuarioExistente = await userManager.FindByNameAsync(userName);
            if (usuarioExistente != null)
            {
                return usuarioExistente;
            }

            // Crear el nuevo usuario
            var usuario = new Usuario
            {
                UserName = userName,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Cedula = cedula,
                EstaActivo = activar,
                EmailConfirmed = activar, // Si se activa, también confirmar el correo
                FechaCreacion = DateTime.Now
            };

            // Intentar crear el usuario en Identity
            var resultado = await userManager.CreateAsync(usuario, password);

            if (resultado.Succeeded)
            {
                // Asignar el rol al usuario
                await userManager.AddToRoleAsync(usuario, rol);

                // Si es cliente, crear su cuenta de ahorro principal
                if (rol == Constantes.RolCliente)
                {
                    await CrearCuentaAhorroPrincipalAsync(context, usuario.Id, montoInicial);
                }
            }

            return usuario;
        }

        /// <summary>
        /// Crea la cuenta de ahorro principal para un cliente
        /// Genera un número de cuenta único de 9 dígitos
        /// </summary>
        private static async Task CrearCuentaAhorroPrincipalAsync(
            ArtemisBankingDbContext context,
            string usuarioId,
            decimal balanceInicial)
        {
            // Generar número de cuenta único
            string numeroCuenta;
            do
            {
                numeroCuenta = GenerarNumeroAleatorio(9);
            }
            while (await context.CuentasAhorro.AnyAsync(c => c.NumeroCuenta == numeroCuenta) ||
                   await context.Prestamos.AnyAsync(p => p.NumeroPrestamo == numeroCuenta));

            // Crear la cuenta de ahorro principal
            var cuentaPrincipal = new CuentaAhorro
            {
                NumeroCuenta = numeroCuenta,
                Balance = balanceInicial,
                EsPrincipal = true,
                EstaActiva = true,
                UsuarioId = usuarioId,
                FechaCreacion = DateTime.Now
            };

            context.CuentasAhorro.Add(cuentaPrincipal);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Genera un número aleatorio de la longitud especificada
        /// </summary>
        private static string GenerarNumeroAleatorio(int longitud)
        {
            var random = new Random();
            var numero = "";
            for (int i = 0; i < longitud; i++)
            {
                numero += random.Next(0, 10).ToString();
            }
            return numero;
        }
    }
}