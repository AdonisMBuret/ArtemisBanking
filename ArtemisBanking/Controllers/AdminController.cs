using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Common;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces;
using ArtemisBanking.Web.ViewModels;
using ArtemisBanking.Web.ViewModels.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador principal del administrador
    /// Maneja el dashboard, gestión de usuarios y productos financieros
    /// </summary>
    [Authorize(Policy = "SoloAdministrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IRepositorioUsuario _repositorioUsuario;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<Usuario> userManager,
            IRepositorioUsuario repositorioUsuario,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioTransaccion repositorioTransaccion,
            IServicioCorreo servicioCorreo,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _repositorioUsuario = repositorioUsuario;
            _repositorioCuenta = repositorioCuenta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioTransaccion = repositorioTransaccion;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard principal del administrador
        /// Muestra todos los indicadores del sistema
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Obtener todos los indicadores para el dashboard
            var viewModel = new DashboardAdminViewModel
            {
                // Indicadores de transacciones
                TotalTransacciones = await _repositorioTransaccion.ContarTransaccionesTotalesAsync(),
                TransaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync(),
                TotalPagos = await _repositorioTransaccion.ContarPagosTotalesAsync(),
                PagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync(),

                // Indicadores de clientes
                ClientesActivos = await _repositorioUsuario.ContarAsync(u => u.EstaActivo),
                ClientesInactivos = await _repositorioUsuario.ContarAsync(u => !u.EstaActivo),

                // Indicadores de productos financieros
                PrestamosVigentes = await _repositorioPrestamo.ContarPrestamosActivosAsync(),
                TarjetasActivas = await _repositorioTarjeta.ContarTarjetasActivasAsync(),
                CuentasAhorro = await _repositorioCuenta.ContarAsync(c => c.EstaActiva),

                // Deuda promedio de clientes
                DeudaPromedioCliente = await _repositorioPrestamo.CalcularDeudaPromedioAsync()
            };

            // Calcular total de productos financieros
            viewModel.TotalProductosFinancieros = 
                viewModel.PrestamosVigentes + 
                viewModel.TarjetasActivas + 
                viewModel.CuentasAhorro;

            return View(viewModel);
        }

        #region Gestión de Usuarios

        /// <summary>
        /// Muestra la lista paginada de usuarios
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Usuarios(int pagina = 1, string rol = null)
        {
            // Obtener usuarios paginados
            var (usuarios, total) = await _repositorioUsuario.ObtenerUsuariosPaginadosAsync(
                pagina, 
                Constantes.TamanoPaginaPorDefecto, 
                rol);

            // Obtener el ID del usuario actual
            var usuarioActualId = _userManager.GetUserId(User);

            // Mapear a ViewModels
            var usuariosViewModel = new List<UsuarioListaItemViewModel>();

            foreach (var usuario in usuarios)
            {
                // Obtener los roles del usuario
                var roles = await _userManager.GetRolesAsync(usuario);
                var rolUsuario = roles.FirstOrDefault() ?? "Sin Rol";

                usuariosViewModel.Add(new UsuarioListaItemViewModel
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.UserName,
                    Cedula = usuario.Cedula,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Email,
                    TipoUsuario = rolUsuario,
                    EstaActivo = usuario.EstaActivo,
                    // El usuario actual no puede editarse a sí mismo
                    PuedeEditar = usuario.Id != usuarioActualId
                });
            }

            var viewModel = new ListaUsuariosViewModel
            {
                Usuarios = usuariosViewModel,
                PaginaActual = pagina,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)Constantes.TamanoPaginaPorDefecto),
                FiltroRol = rol
            };

            return View(viewModel);
        }

        /// <summary>
        /// Muestra el formulario para crear un nuevo usuario
        /// </summary>
        [HttpGet]
        public IActionResult CrearUsuario()
        {
            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que el usuario no exista
            var usuarioExistente = await _userManager.FindByNameAsync(model.NombreUsuario);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está en uso.");
                return View(model);
            }

            // Verificar que el correo no exista
            var correoExistente = await _userManager.FindByEmailAsync(model.Correo);
            if (correoExistente != null)
            {
                ModelState.AddModelError("Correo", "El correo electrónico ya está registrado.");
                return View(model);
            }

            // Crear el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                UserName = model.NombreUsuario,
                Email = model.Correo,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Cedula = model.Cedula,
                EstaActivo = false, // Se activa cuando confirme el correo
                FechaCreacion = DateTime.Now
            };

            // Intentar crear el usuario
            var resultado = await _userManager.CreateAsync(nuevoUsuario, model.Contrasena);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Asignar el rol al usuario
            await _userManager.AddToRoleAsync(nuevoUsuario, model.TipoUsuario);

            // Si es cliente, crear su cuenta de ahorro principal
            if (model.TipoUsuario == Constantes.RolCliente)
            {
                var numeroCuenta = await _repositorioCuenta.GenerarNumeroCuentaUnicoAsync();

                var cuentaPrincipal = new CuentaAhorro
                {
                    NumeroCuenta = numeroCuenta,
                    Balance = model.MontoInicial,
                    EsPrincipal = true,
                    EstaActiva = true,
                    UsuarioId = nuevoUsuario.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioCuenta.AgregarAsync(cuentaPrincipal);
                await _repositorioCuenta.GuardarCambiosAsync();
            }

            // Generar token de confirmación
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(nuevoUsuario);

            // Enviar correo de confirmación
            await _servicioCorreo.EnviarCorreoConfirmacionAsync(
                nuevoUsuario.Email,
                nuevoUsuario.UserName,
                token);

            _logger.LogInformation($"Usuario {nuevoUsuario.UserName} creado exitosamente");

            TempData["MensajeExito"] = $"Usuario {nuevoUsuario.UserName} creado exitosamente. Se ha enviado un correo de confirmación.";
            return RedirectToAction(nameof(Usuarios));
        }

        /// <summary>
        /// Muestra el formulario para editar un usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar que no sea el usuario actual
            var usuarioActualId = _userManager.GetUserId(User);
            if (usuario.Id == usuarioActualId)
            {
                TempData["MensajeError"] = "No puede editar su propia cuenta.";
                return RedirectToAction(nameof(Usuarios));
            }

            // Obtener el rol del usuario
            var roles = await _userManager.GetRolesAsync(usuario);
            var rolUsuario = roles.FirstOrDefault();

            var viewModel = new EditarUsuarioViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Cedula = usuario.Cedula,
                Correo = usuario.Email,
                NombreUsuario = usuario.UserName,
                TipoUsuario = rolUsuario
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la edición de un usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _userManager.FindByIdAsync(model.Id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Actualizar datos del usuario
            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Cedula = model.Cedula;
            usuario.Email = model.Correo;
            usuario.UserName = model.NombreUsuario;

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Si se proporcionó una nueva contraseña, actualizarla
            if (!string.IsNullOrEmpty(model.NuevaContrasena))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var resultadoContrasena = await _userManager.ResetPasswordAsync(
                    usuario, 
                    token, 
                    model.NuevaContrasena);

                if (!resultadoContrasena.Succeeded)
                {
                    foreach (var error in resultadoContrasena.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // Si es cliente y hay monto adicional, agregarlo a la cuenta principal
            if (model.TipoUsuario == Constantes.RolCliente && model.MontoAdicional > 0)
            {
                var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(usuario.Id);

                if (cuentaPrincipal != null)
                {
                    cuentaPrincipal.Balance += model.MontoAdicional;
                    await _repositorioCuenta.ActualizarAsync(cuentaPrincipal);
                    await _repositorioCuenta.GuardarCambiosAsync();
                }
            }

            _logger.LogInformation($"Usuario {usuario.UserName} actualizado exitosamente");

            TempData["MensajeExito"] = "Usuario actualizado exitosamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        /// <summary>
        /// Activa o desactiva un usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar que no sea el usuario actual
            var usuarioActualId = _userManager.GetUserId(User);
            if (usuario.Id == usuarioActualId)
            {
                return Json(new { 
                    exito = false, 
                    mensaje = "No puede modificar el estado de su propia cuenta." 
                });
            }

            // Cambiar el estado
            usuario.EstaActivo = !usuario.EstaActivo;
            await _userManager.UpdateAsync(usuario);

            _logger.LogInformation($"Estado del usuario {usuario.UserName} cambiado a {usuario.EstaActivo}");

            return Json(new { 
                exito = true, 
                mensaje = $"Usuario {(usuario.EstaActivo ? "activado" : "desactivado")} exitosamente.",
                nuevoEstado = usuario.EstaActivo
            });
        }

        #endregion
    }
}