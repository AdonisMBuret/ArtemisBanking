using ArtemisBanking.Application.Common;
using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ArtemisBankingDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            await CrearRolesAsync(roleManager);

            await CrearComerciosDePruebaAsync(context);

            await CrearUsuariosPorDefectoAsync(userManager, context);
        }

        private static async Task CrearRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { 
                Constantes.RolAdministrador, 
                Constantes.RolCajero, 
                Constantes.RolCliente,
                Constantes.RolComercio 
            };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }

        private static async Task CrearComerciosDePruebaAsync(ArtemisBankingDbContext context)
        {
            if (!await context.Comercios.AnyAsync())
            {
                var comerciosPrueba = new List<Comercio>
                {
                    new Comercio
                    {
                        Nombre = "Supermercado La Econmica jajaj",
                        RNC = "130123456",
                        EstaActivo = true,
                        FechaCreacion = DateTime.Now
                    },
                    new Comercio
                    {
                        Nombre = "Farmacia GBS",
                        RNC = "130987654",
                        EstaActivo = true,
                        FechaCreacion = DateTime.Now
                    },
                    new Comercio
                    {
                        Nombre = "Restaurante El Sabroson",
                        RNC = "130555555",
                        EstaActivo = true,
                        FechaCreacion = DateTime.Now
                    }
                };

                context.Comercios.AddRange(comerciosPrueba);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CrearUsuariosPorDefectoAsync(
            UserManager<Usuario> userManager, 
            ArtemisBankingDbContext context)
        {
            //  CREAR USUARIO ADMINISTRADOR 
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new Usuario
                {
                    UserName = "admin",
                    Email = "admin@artemisbanking.com",
                    Nombre = "Administrador",
                    Apellido = "Sistema",
                    Cedula = "00000000001",
                    EstaActivo = true,
                    FechaCreacion = DateTime.Now,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(adminUser, "Admin123@");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Constantes.RolAdministrador);
                }
            }

            //  CREAR USUARIO CAJERO 
            if (await userManager.FindByNameAsync("cajero") == null)
            {
                var cajeroUser = new Usuario
                {
                    UserName = "cajero",
                    Email = "cajero@artemisbanking.com",
                    Nombre = "Cajero",
                    Apellido = "Prueba",
                    Cedula = "00000000002",
                    EstaActivo = true,
                    FechaCreacion = DateTime.Now,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(cajeroUser, "Cajero123@");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(cajeroUser, Constantes.RolCajero);
                }
            }

            //  CREAR USUARIO CLIENTE 
            if (await userManager.FindByNameAsync("cliente") == null)
            {
                var clienteUser = new Usuario
                {
                    UserName = "cliente",
                    Email = "cliente@artemisbanking.com",
                    Nombre = "Henry",
                    Apellido = "Cavill",
                    Cedula = "00000000003",
                    EstaActivo = true,
                    FechaCreacion = DateTime.Now,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(clienteUser, "Cliente123@");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(clienteUser, Constantes.RolCliente);

                    var numeroCuenta = await GenerarNumeroCuentaUnicoAsync(context);

                    var cuentaPrincipal = new CuentaAhorro
                    {
                        NumeroCuenta = numeroCuenta,
                        Balance = 10000,
                        EsPrincipal = true,
                        EstaActiva = true,
                        UsuarioId = clienteUser.Id,
                        FechaCreacion = DateTime.Now
                    };

                    context.CuentasAhorro.Add(cuentaPrincipal);
                    await context.SaveChangesAsync();
                }
            }

            //  CREAR USUARIO COMERCIO 

            if (await userManager.FindByNameAsync("comercio") == null)
            {
                var comercio = await context.Comercios.FirstOrDefaultAsync();
                
                if (comercio != null)
                {
                    var comercioUser = new Usuario
                    {
                        UserName = "comerciante",
                        Email = "comercioX@artemisbanking.com",
                        Nombre = "Billie",
                        Apellido = "Eilish",
                        Cedula = "00000000004",
                        EstaActivo = true,
                        FechaCreacion = DateTime.Now,
                        EmailConfirmed = true,
                        ComercioId = comercio.Id 
                    };

                    var resultado = await userManager.CreateAsync(comercioUser, "Comerciante123@");

                    if (resultado.Succeeded)
                    {
                        await userManager.AddToRoleAsync(comercioUser, Constantes.RolComercio);

                        comercio.UsuarioId = comercioUser.Id;
                        context.Comercios.Update(comercio);

                        var numeroCuenta = await GenerarNumeroCuentaUnicoAsync(context);

                        var cuentaComercio = new CuentaAhorro
                        {
                            NumeroCuenta = numeroCuenta,
                            Balance = 0, 
                            EsPrincipal = true,
                            EstaActiva = true,
                            UsuarioId = comercioUser.Id,
                            FechaCreacion = DateTime.Now
                        };

                        context.CuentasAhorro.Add(cuentaComercio);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task<string> GenerarNumeroCuentaUnicoAsync(ArtemisBankingDbContext context)
        {
            var random = new Random();
            string numeroCuenta;

            do
            {
                numeroCuenta = "";
                for (int i = 0; i < 9; i++)
                {
                    numeroCuenta += random.Next(0, 10).ToString();
                }
            }
            while (await context.CuentasAhorro.AnyAsync(c => c.NumeroCuenta == numeroCuenta) ||
                   await context.Prestamos.AnyAsync(p => p.NumeroPrestamo == numeroCuenta));

            return numeroCuenta;
        }
    }
}

//Ya hice las Datas, push Db