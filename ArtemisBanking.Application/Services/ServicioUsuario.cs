using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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
            _mapper = mapper;
            _logger = logger;
        }

         
        /// Crea un nuevo usuario en el sistema
        /// Si es cliente, también crea su cuenta de ahorro principal con el monto inicial
        /// Envía correo de confirmación con token para activar la cuenta
         
        public async Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos)
        {
            try
            {
                // 1. Validar que el usuario y correo sean únicos
                if (await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(datos.NombreUsuario) != null)
                {
                    return ResultadoOperacion<UsuarioDTO>.Fallo("El nombre de usuario ya está en uso");
                }

                if (await _repositorioUsuario.ObtenerPorCorreoAsync(datos.Correo) != null)
                {
                    return ResultadoOperacion<UsuarioDTO>.Fallo("El correo electrónico ya está registrado");
                }

                // 2. Crear el usuario
                var nuevoUsuario = new Usuario
                {
                    UserName = datos.NombreUsuario,
                    Email = datos.Correo,
                    Nombre = datos.Nombre,
                    Apellido = datos.Apellido,
                    Cedula = datos.Cedula,
                    EstaActivo = false, // Empieza inactivo hasta confirmar correo
                    FechaCreacion = DateTime.Now
                };

                // 3. Crear usuario con contraseña
                var resultado = await _userManager.CreateAsync(nuevoUsuario, datos.Contrasena);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return ResultadoOperacion<UsuarioDTO>.Fallo($"Error al crear usuario: {errores}");
                }

                // 4. Asignar el rol correspondiente
                await _userManager.AddToRoleAsync(nuevoUsuario, datos.TipoUsuario);

                // 5. Si es cliente, crear su cuenta de ahorro principal
                if (datos.TipoUsuario == Constantes.RolCliente)
                {
                    // Generar número de cuenta único
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

                // 6. Generar token de confirmación y enviar correo
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(nuevoUsuario);

                    // Obtener la URL base desde la configuración o usar una predeterminada
                    var urlBase = "https://localhost:7096"; // Cambiar según tu dominio en producción

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

                // 7. Retornar DTO
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
        /// Si es cliente y hay monto adicional, lo suma a la cuenta principal
        /// Permite cambiar contraseña opcionalmente
         
        public async Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos)
        {
            try
            {
                // 1. Obtener el usuario
                var usuario = await _userManager.FindByIdAsync(datos.UsuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado");
                }

                // 2. Validar unicidad de usuario y correo (excepto el propio)
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

                // 3. Actualizar datos básicos
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

                // 4. Si se proporcionó una nueva contraseña, cambiarla
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

                // 5. Si es cliente y hay monto adicional, sumarlo a la cuenta principal
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
        /// El administrador logueado NO puede cambiar su propio estado
         
        public async Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId)
        {
            try
            {
                // 1. Validar que no sea el mismo usuario
                if (usuarioId == usuarioActualId)
                {
                    return ResultadoOperacion.Fallo("No puede cambiar su propio estado");
                }

                // 2. Obtener el usuario
                var usuario = await _userManager.FindByIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado");
                }

                // 3. Cambiar el estado
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
        /// Incluye estadísticas de transacciones, clientes y productos financieros
         
        public async Task<ResultadoOperacion<DashboardAdminDTO>> ObtenerDashboardAdminAsync()
        {
            try
            {
                var dashboard = new DashboardAdminDTO
                {
                    // Transacciones y pagos
                    TotalTransacciones = await _repositorioTransaccion.ContarTransaccionesTotalesAsync(),
                    TransaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync(),
                    TotalPagos = await _repositorioTransaccion.ContarPagosTotalesAsync(),
                    PagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync(),

                    // Clientes
                    ClientesActivos = await _repositorioUsuario.ContarAsync(u => u.EstaActivo),
                    ClientesInactivos = await _repositorioUsuario.ContarAsync(u => !u.EstaActivo),

                    // Productos financieros
                    PrestamosVigentes = await _repositorioPrestamo.ContarPrestamosActivosAsync(),
                    TarjetasActivas = await _repositorioTarjeta.ContarTarjetasActivasAsync(),
                    CuentasAhorro = await _repositorioCuenta.ContarAsync(c => c.EstaActiva),

                    // Deuda promedio
                    DeudaPromedioCliente = await _repositorioPrestamo.CalcularDeudaPromedioAsync()
                };

                // Calcular total de productos financieros
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
        /// Este método se usa en el Home del cajero
         
        public async Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardCajeroAsync(string cajeroId)
        {
            try
            {
                var dashboard = new DashboardCajeroDTO
                {
                    // Transacciones realizadas HOY por este cajero
                    TransaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync(),

                    // Pagos (a TC y préstamos) realizados HOY
                    PagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync(),

                    // Depósitos realizados HOY
                    DepositosDelDia = await _repositorioTransaccion.ContarDepositosDelDiaAsync(),

                    // Retiros realizados HOY
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
        /// Se usa al asignar nuevos préstamos (un cliente solo puede tener uno a la vez)
         
        public async Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesSinPrestamoActivoAsync()
        {
            try
            {
                // Obtener todos los usuarios con rol Cliente que estén activos
                var clientes = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);

                var clientesActivos = clientes.Where(c => c.EstaActivo).ToList();

                // Filtrar solo los que NO tienen préstamo activo
                var clientesSinPrestamo = new List<Usuario>();

                foreach (var cliente in clientesActivos)
                {
                    var tienePrestamo = await _repositorioPrestamo.TienePrestamoActivoAsync(cliente.Id);

                    if (!tienePrestamo)
                    {
                        clientesSinPrestamo.Add(cliente);
                    }
                }

                // Convertir a DTOs
                var clientesDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(clientesSinPrestamo);

                // Para cada cliente, calcular su deuda total
                foreach (var clienteDTO in clientesDTO)
                {
                    var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteDTO.Id);
                    clienteDTO.MontoInicial = deudaTotal; // Reutilizamos este campo para la deuda
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
        /// Se usa para asignar tarjetas y cuentas de ahorro secundarias
         
        public async Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesActivosAsync()
        {
            try
            {
                // Obtener todos los usuarios con rol Cliente que estén activos
                var clientes = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);

                var clientesActivos = clientes.Where(c => c.EstaActivo).ToList();

                // Convertir a DTOs
                var clientesDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(clientesActivos);

                // Para cada cliente, calcular su deuda total
                foreach (var clienteDTO in clientesDTO)
                {
                    var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteDTO.Id);
                    clienteDTO.MontoInicial = deudaTotal; // Reutilizamos este campo para la deuda
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
        /// Usado en validaciones de formularios (Ajax)
         
        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, string usuarioIdExcluir = null)
        {
            try
            {
                var usuario = await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(nombreUsuario);

                if (usuario == null)
                    return false;

                // Si se proporciona un ID para excluir (modo edición), verificar que no sea el mismo
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
        /// Usado en validaciones de formularios (Ajax)
         
        public async Task<bool> ExisteCorreoAsync(string correo, string usuarioIdExcluir = null)
        {
            try
            {
                var usuario = await _repositorioUsuario.ObtenerPorCorreoAsync(correo);

                if (usuario == null)
                    return false;

                // Si se proporciona un ID para excluir (modo edición), verificar que no sea el mismo
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
        /// Usado para estadísticas y dashboards
         
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
        /// Se usa para ver detalles de un usuario específico
         
        public async Task<ResultadoOperacion<UsuarioDTO>> ObtenerUsuarioPorIdAsync(string usuarioId)
        {
            try
            {
                // Buscar el usuario
                var usuario = await _repositorioUsuario.ObtenerPorIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion<UsuarioDTO>.Fallo("Usuario no encontrado");
                }

                // Obtener el rol del usuario
                var roles = await _userManager.GetRolesAsync(usuario);
                var rol = roles.FirstOrDefault() ?? "Cliente";

                // Convertir a DTO
                var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
                usuarioDTO.Rol = rol;

                // Si es cliente, obtener su cuenta principal para mostrar el balance inicial
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
        /// Permite filtrar por rol y excluir al usuario actual de la lista
         
        public async Task<ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null,
            string usuarioActualId = null)
        {
            try
            {
                // Obtener usuarios paginados del repositorio
                var (usuarios, total) = await _repositorioUsuario.ObtenerUsuariosPaginadosAsync(
                    pagina,
                    tamano,
                    rol);

                // Convertir a DTOs
                var usuariosDTO = new List<UsuarioDTO>();

                foreach (var usuario in usuarios)
                {
                    // Obtener el rol del usuario
                    var roles = await _userManager.GetRolesAsync(usuario);
                    var rolUsuario = roles.FirstOrDefault() ?? "Cliente";

                    // Convertir a DTO
                    var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
                    usuarioDTO.Rol = rolUsuario;

                    // Determinar si se puede editar (no se puede editar a uno mismo)
                    if (!string.IsNullOrEmpty(usuarioActualId))
                    {
                        // El campo MontoInicial lo usamos para indicar si se puede editar
                        // (No es lo ideal, pero reutilizamos el campo para no crear otro DTO)
                        usuarioDTO.MontoInicial = usuario.Id == usuarioActualId ? 0 : 1;
                    }

                    usuariosDTO.Add(usuarioDTO);
                }

                // Crear el resultado paginado
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
