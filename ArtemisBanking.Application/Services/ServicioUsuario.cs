using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ArtemisBanking.Application.Services
{
    public class ServicioUsuario : IServicioUsuario
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IRepositorioUsuario _repositorioUsuario;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicioUsuario> _logger;

        public ServicioUsuario(
            UserManager<Usuario> userManager,
            IRepositorioUsuario repositorioUsuario,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTransaccion repositorioTransaccion,

            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,

            IServicioCorreo servicioCorreo,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<ServicioUsuario> logger)
        {
            _userManager = userManager;
            _repositorioUsuario = repositorioUsuario;
            _repositorioCuenta = repositorioCuenta;
            _repositorioTransaccion = repositorioTransaccion;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _servicioCorreo = servicioCorreo;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

         
        /// Crea un nuevo usuario en el sistema

        public async Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos)
        {
            try
            {
                if (await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(datos.NombreUsuario) != null)
                {
                    return ResultadoOperacion<UsuarioDTO>.Fallo("El nombre de usuario ya está en uso");
                }

                if (await _repositorioUsuario.ObtenerPorCorreoAsync(datos.Correo) != null)
                {
                    return ResultadoOperacion<UsuarioDTO>.Fallo("El correo electrónico ya está registrado");
                }

                var nuevoUsuario = new Usuario
                {
                    UserName = datos.NombreUsuario,
                    Email = datos.Correo,
                    Nombre = datos.Nombre,
                    Apellido = datos.Apellido,
                    Cedula = datos.Cedula,
                    EstaActivo = false, 
                    FechaCreacion = DateTime.Now
                };

                var resultado = await _userManager.CreateAsync(nuevoUsuario, datos.Contrasena);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return ResultadoOperacion<UsuarioDTO>.Fallo($"Error al crear usuario: {errores}");
                }

                await _userManager.AddToRoleAsync(nuevoUsuario, datos.TipoUsuario);

                if (datos.TipoUsuario == Constantes.RolCliente)
                {
                    var numeroCuenta = await _repositorioCuenta.GenerarNumeroCuentaUnicoAsync();

                    var cuentaPrincipal = new CuentaAhorro
                    {
                        NumeroCuenta = numeroCuenta,
                        Balance = datos.MontoInicial,
                        EsPrincipal = true,
                        EstaActiva = true,
                        UsuarioId = nuevoUsuario.Id,
                        FechaCreacion = DateTime.Now
                    };

                    await _repositorioCuenta.AgregarAsync(cuentaPrincipal);
                    await _repositorioCuenta.GuardarCambiosAsync();

                    _logger.LogInformation($"Cuenta principal {numeroCuenta} creada para cliente {nuevoUsuario.UserName}");
                }

                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(nuevoUsuario);

                    var urlBase = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7103";

                    await _servicioCorreo.EnviarCorreoConfirmacionAsync(
                        nuevoUsuario.Email,
                        nuevoUsuario.UserName,
                        nuevoUsuario.Id,
                        token,
                        urlBase);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de confirmación");
                }

                _logger.LogInformation($"Usuario {nuevoUsuario.UserName} creado exitosamente con rol {datos.TipoUsuario}");

                var usuarioDTO = _mapper.Map<UsuarioDTO>(nuevoUsuario);
                usuarioDTO.Rol = datos.TipoUsuario;
                usuarioDTO.MontoInicial = datos.MontoInicial;

                return ResultadoOperacion<UsuarioDTO>.Ok(
                    usuarioDTO,
                    "Usuario creado exitosamente. Se ha enviado un correo para activar la cuenta.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return ResultadoOperacion<UsuarioDTO>.Fallo("Error al crear el usuario");
            }
        }

         
        /// Actualiza los datos de un usuario existente
 
        public async Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(datos.UsuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado");
                }

                var usuarioConMismoNombre = await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(datos.NombreUsuario);
                if (usuarioConMismoNombre != null && usuarioConMismoNombre.Id != datos.UsuarioId)
                {
                    return ResultadoOperacion.Fallo("El nombre de usuario ya está en uso");
                }

                var usuarioConMismoCorreo = await _repositorioUsuario.ObtenerPorCorreoAsync(datos.Correo);
                if (usuarioConMismoCorreo != null && usuarioConMismoCorreo.Id != datos.UsuarioId)
                {
                    return ResultadoOperacion.Fallo("El correo electrónico ya está registrado");
                }

                usuario.Nombre = datos.Nombre;
                usuario.Apellido = datos.Apellido;
                usuario.Cedula = datos.Cedula;
                usuario.Email = datos.Correo;
                usuario.UserName = datos.NombreUsuario;

                var resultadoActualizacion = await _userManager.UpdateAsync(usuario);

                if (!resultadoActualizacion.Succeeded)
                {
                    var errores = string.Join(", ", resultadoActualizacion.Errors.Select(e => e.Description));
                    return ResultadoOperacion.Fallo($"Error al actualizar: {errores}");
                }

                if (!string.IsNullOrWhiteSpace(datos.NuevaContrasena))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                    var resultadoCambioPass = await _userManager.ResetPasswordAsync(usuario, token, datos.NuevaContrasena);

                    if (!resultadoCambioPass.Succeeded)
                    {
                        var errores = string.Join(", ", resultadoCambioPass.Errors.Select(e => e.Description));
                        return ResultadoOperacion.Fallo($"Error al cambiar contraseña: {errores}");
                    }
                }

                var roles = await _userManager.GetRolesAsync(usuario);
                if (roles.Contains(Constantes.RolCliente) && datos.MontoAdicional > 0)
                {
                    var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(usuario.Id);

                    if (cuentaPrincipal != null)
                    {
                        cuentaPrincipal.Balance += datos.MontoAdicional;
                        await _repositorioCuenta.ActualizarAsync(cuentaPrincipal);
                        await _repositorioCuenta.GuardarCambiosAsync();

                        _logger.LogInformation($"RD${datos.MontoAdicional} agregados a cuenta principal de {usuario.UserName}");
                    }
                }

                _logger.LogInformation($"Usuario {usuario.UserName} actualizado exitosamente");

                return ResultadoOperacion.Ok("Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return ResultadoOperacion.Fallo("Error al actualizar el usuario");
            }
        }

         
        /// Activa o desactiva un usuario
        public async Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId)
        {
            try
            {
                if (usuarioId == usuarioActualId)
                {
                    return ResultadoOperacion.Fallo("No puede cambiar su propio estado");
                }

                var usuario = await _userManager.FindByIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado");
                }

                usuario.EstaActivo = !usuario.EstaActivo;
                await _userManager.UpdateAsync(usuario);

                var estadoTexto = usuario.EstaActivo ? "activado" : "desactivado";
                _logger.LogInformation($"Usuario {usuario.UserName} {estadoTexto}");

                return ResultadoOperacion.Ok($"Usuario {estadoTexto} exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del usuario");
                return ResultadoOperacion.Fallo("Error al cambiar el estado");
            }
        }

         
        /// Obtiene los indicadores para el dashboard del administrador

        public async Task<ResultadoOperacion<DashboardAdminDTO>> ObtenerDashboardAdminAsync()
        {
            try
            {
                var dashboard = new DashboardAdminDTO
                {
                    TotalTransacciones = await _repositorioTransaccion.ContarTransaccionesTotalesAsync(),
                    TransaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync(),
                    TotalPagos = await _repositorioTransaccion.ContarPagosTotalesAsync(),
                    PagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync(),

                    ClientesActivos = await _repositorioUsuario.ContarAsync(u => u.EstaActivo),
                    ClientesInactivos = await _repositorioUsuario.ContarAsync(u => !u.EstaActivo),

                    PrestamosVigentes = await _repositorioPrestamo.ContarPrestamosActivosAsync(),
                    TarjetasActivas = await _repositorioTarjeta.ContarTarjetasActivasAsync(),
                    CuentasAhorro = await _repositorioCuenta.ContarAsync(c => c.EstaActiva),

                    DeudaPromedioCliente = await _repositorioPrestamo.CalcularDeudaPromedioAsync()
                };

                dashboard.TotalProductosFinancieros = dashboard.PrestamosVigentes +
                                                      dashboard.TarjetasActivas +
                                                      dashboard.CuentasAhorro;

                return ResultadoOperacion<DashboardAdminDTO>.Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard del administrador");
                return ResultadoOperacion<DashboardAdminDTO>.Fallo("Error al cargar el dashboard");
            }
        }


         
        /// Obtiene el dashboard del cajero con las estadísticas del día
      
        public async Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardCajeroAsync(string cajeroId)
        {
            try
            {
                var dashboard = new DashboardCajeroDTO
                {
                    TransaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync(),

                    PagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync(),

                    DepositosDelDia = await _repositorioTransaccion.ContarDepositosDelDiaAsync(),

                    RetirosDelDia = await _repositorioTransaccion.ContarRetirosDelDiaAsync()
                };

                return ResultadoOperacion<DashboardCajeroDTO>.Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard del cajero");
                return ResultadoOperacion<DashboardCajeroDTO>.Fallo("Error al cargar el dashboard");
            }
        }

         
        /// Obtiene todos los clientes activos que NO tienen préstamo activo
      
        public async Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesSinPrestamoActivoAsync()
        {
            try
            {
                var clientes = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);

                var clientesActivos = clientes.Where(c => c.EstaActivo).ToList();

                var clientesSinPrestamo = new List<Usuario>();

                foreach (var cliente in clientesActivos)
                {
                    var tienePrestamo = await _repositorioPrestamo.TienePrestamoActivoAsync(cliente.Id);

                    if (!tienePrestamo)
                    {
                        clientesSinPrestamo.Add(cliente);
                    }
                }


                var clientesDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(clientesSinPrestamo);

                foreach (var clienteDTO in clientesDTO)
                {
                    var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteDTO.Id);
                    clienteDTO.MontoInicial = deudaTotal; 
                }

                return ResultadoOperacion<IEnumerable<UsuarioDTO>>.Ok(clientesDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes sin préstamo activo");
                return ResultadoOperacion<IEnumerable<UsuarioDTO>>.Fallo("Error al obtener los clientes");
            }
        }

         
        /// Obtiene todos los clientes activos del sistema

        public async Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesActivosAsync()
        {
            try
            {
            
                var clientes = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);

                var clientesActivos = clientes.Where(c => c.EstaActivo).ToList();

            
                var clientesDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(clientesActivos);

 
                foreach (var clienteDTO in clientesDTO)
                {
                    var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteDTO.Id);
                    clienteDTO.MontoInicial = deudaTotal; 
                }

                return ResultadoOperacion<IEnumerable<UsuarioDTO>>.Ok(clientesDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes activos");
                return ResultadoOperacion<IEnumerable<UsuarioDTO>>.Fallo("Error al obtener los clientes");
            }
        }

         
        /// Verifica si un nombre de usuario ya existe
   
        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, string usuarioIdExcluir = null)
        {
            try
            {
                var usuario = await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(nombreUsuario);

                if (usuario == null)
                    return false;

                if (!string.IsNullOrEmpty(usuarioIdExcluir))
                    return usuario.Id != usuarioIdExcluir;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar nombre de usuario");
                return false;
            }
        }
         
        /// Verifica si un correo ya existe
         
        public async Task<bool> ExisteCorreoAsync(string correo, string usuarioIdExcluir = null)
        {
            try
            {
                var usuario = await _repositorioUsuario.ObtenerPorCorreoAsync(correo);

                if (usuario == null)
                    return false;

                if (!string.IsNullOrEmpty(usuarioIdExcluir))
                    return usuario.Id != usuarioIdExcluir;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar correo");
                return false;
            }
        }

         
        /// Cuenta cuántos usuarios activos hay del rol especificado
       
        public async Task<int> ContarUsuariosActivosPorRolAsync(string rol)
        {
            try
            {
                var usuarios = await _userManager.GetUsersInRoleAsync(rol);
                return usuarios.Count(u => u.EstaActivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al contar usuarios activos del rol {rol}");
                return 0;
            }
        }


        /// Obtiene un usuario por su ID con toda su información
      
        public async Task<ResultadoOperacion<UsuarioDTO>> ObtenerUsuarioPorIdAsync(string usuarioId)
        {
            try
            {
                var usuario = await _repositorioUsuario.ObtenerPorIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion<UsuarioDTO>.Fallo("Usuario no encontrado");
                }

                var roles = await _userManager.GetRolesAsync(usuario);
                var rol = roles.FirstOrDefault() ?? "Cliente";

                var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
                usuarioDTO.Rol = rol;

                if (rol == Constantes.RolCliente)
                {
                    var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(usuarioId);
                    if (cuentaPrincipal != null)
                    {
                        usuarioDTO.MontoInicial = cuentaPrincipal.Balance;
                    }
                }

                return ResultadoOperacion<UsuarioDTO>.Ok(usuarioDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID");
                return ResultadoOperacion<UsuarioDTO>.Fallo("Error al obtener el usuario");
            }
        }
         
        /// Obtiene un listado paginado de usuarios
 
        public async Task<ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null,
            string usuarioActualId = null)
        {
            try
            {
                var (usuarios, total) = await _repositorioUsuario.ObtenerUsuariosPaginadosAsync(
                    pagina,
                    tamano,
                    rol);

                var usuariosDTO = new List<UsuarioDTO>();

                foreach (var usuario in usuarios)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    var rolUsuario = roles.FirstOrDefault() ?? "Cliente";

                    var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
                    usuarioDTO.Rol = rolUsuario;

                    if (!string.IsNullOrEmpty(usuarioActualId))
                    {
                        usuarioDTO.MontoInicial = usuario.Id == usuarioActualId ? 0 : 1;
                    }

                    usuariosDTO.Add(usuarioDTO);
                }

                var resultado = new ResultadoPaginadoDTO<UsuarioDTO>
                {
                    Datos = usuariosDTO,
                    TotalRegistros = total,
                    PaginaActual = pagina,
                    TamañoPagina = tamano
                };

                return ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>.Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios paginados");
                return ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>.Fallo("Error al obtener los usuarios");
            }
        }
    }
}
