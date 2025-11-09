using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Common;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces;
using ArtemisBanking.Web.ViewModels;
using ArtemisBanking.Web.ViewModels.CuentaAhorro;
using ArtemisBanking.Web.ViewModels.Prestamo;
using ArtemisBanking.Web.ViewModels.TarjetaCredito;
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
        private readonly IServicioCifrado _servicioCifrado;


        public AdminController(
            UserManager<Usuario> userManager,
            IRepositorioUsuario repositorioUsuario,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioTransaccion repositorioTransaccion,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IServicioCorreo servicioCorreo,
            IServicioCalculoPrestamo servicioCalculoPrestamo,
            IServicioCifrado servicioCifrado,
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
            _servicioCifrado = servicioCifrado;
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


        // ArtemisBanking.Web/Controllers/AdminController.cs (Continuación - Gestión de Préstamos)

        #region Gestión de Préstamos

        /// <summary>
        /// Muestra la lista paginada de préstamos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Prestamos(int pagina = 1, string cedula = null, bool? estado = null)
        {
            // Obtener préstamos paginados con filtros
            var (prestamos, total) = await _repositorioPrestamo.ObtenerPrestamosPaginadosAsync(
                pagina,
                Constantes.TamanoPaginaPorDefecto,
                cedula,
                estado);

            // Mapear a ViewModels
            var prestamosViewModel = prestamos.Select(p => new PrestamoListaItemViewModel
            {
                Id = p.Id,
                NumeroPrestamo = p.NumeroPrestamo,
                NombreCliente = p.Cliente.Nombre,
                ApellidoCliente = p.Cliente.Apellido,
                MontoCapital = p.MontoCapital,
                TotalCuotas = p.PlazoMeses,
                CuotasPagadas = p.CuotasPagadas,
                MontoPendiente = p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota),
                TasaInteresAnual = p.TasaInteresAnual,
                PlazoMeses = p.PlazoMeses,
                EstaAlDia = p.EstaAlDia,
                EstaActivo = p.EstaActivo
            }).ToList();

            var viewModel = new ListaPrestamosViewModel
            {
                Prestamos = prestamosViewModel,
                PaginaActual = pagina,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)Constantes.TamanoPaginaPorDefecto),
                FiltroCedula = cedula,
                FiltroEstado = estado
            };

            return View(viewModel);
        }

        /// <summary>
        /// Paso 1: Mostrar lista de clientes para seleccionar uno
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SeleccionarClientePrestamo(string cedula = null)
        {
            // Obtener todos los usuarios con rol Cliente que estén activos
            var todosLosUsuarios = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);
            var clientesActivos = todosLosUsuarios.Where(u => u.EstaActivo).ToList();

            // Filtrar clientes que NO tengan préstamo activo
            var clientesSinPrestamoActivo = new List<ClienteParaPrestamoViewModel>();

            foreach (var cliente in clientesActivos)
            {
                var tienePrestamoActivo = await _repositorioPrestamo.TienePrestamoActivoAsync(cliente.Id);

                if (!tienePrestamoActivo)
                {
                    // Si hay filtro por cédula, aplicarlo
                    if (!string.IsNullOrEmpty(cedula) && cliente.Cedula != cedula)
                        continue;

                    var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(cliente.Id);

                    clientesSinPrestamoActivo.Add(new ClienteParaPrestamoViewModel
                    {
                        Id = cliente.Id,
                        Cedula = cliente.Cedula,
                        NombreCompleto = cliente.NombreCompleto,
                        Correo = cliente.Email,
                        DeudaTotal = deudaTotal
                    });
                }
            }

            var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

            var viewModel = new SeleccionarClientePrestamoViewModel
            {
                Clientes = clientesSinPrestamoActivo,
                DeudaPromedio = deudaPromedio,
                FiltroCedula = cedula
            };

            return View(viewModel);
        }

        /// <summary>
        /// Paso 2: Mostrar formulario para configurar el préstamo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfigurarPrestamo(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                return RedirectToAction(nameof(SeleccionarClientePrestamo));
            }

            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(SeleccionarClientePrestamo));
            }

            var deudaActual = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteId);
            var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

            var viewModel = new ConfigurarPrestamoViewModel
            {
                ClienteId = clienteId,
                NombreCliente = cliente.NombreCompleto,
                DeudaActualCliente = deudaActual,
                DeudaPromedio = deudaPromedio
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la asignación del préstamo (con validación de riesgo)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPrestamo(ConfigurarPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarPrestamo", model);
            }

            var cliente = await _userManager.FindByIdAsync(model.ClienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Prestamos));
            }

            // Calcular deuda actual y promedio
            var deudaActual = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(model.ClienteId);
            var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

            // Calcular cuota mensual
            var cuotaMensual = _servicioCalculoPrestamo.CalcularCuotaMensual(
                model.MontoCapital,
                model.TasaInteresAnual,
                model.PlazoMeses);

            // Calcular deuda total con el nuevo préstamo (capital + todos los intereses)
            var deudaTotalDelPrestamo = cuotaMensual * model.PlazoMeses;
            var deudaDespuesPrestamo = deudaActual + deudaTotalDelPrestamo;

            // Validar si el cliente se convierte en cliente de alto riesgo
            bool esAltoRiesgo = deudaActual > deudaPromedio || deudaDespuesPrestamo > deudaPromedio;

            if (esAltoRiesgo)
            {
                // Redirigir a pantalla de advertencia
                var advertenciaViewModel = new AdvertenciaRiesgoViewModel
                {
                    ClienteId = model.ClienteId,
                    NombreCliente = cliente.NombreCompleto,
                    MontoCapital = model.MontoCapital,
                    PlazoMeses = model.PlazoMeses,
                    TasaInteresAnual = model.TasaInteresAnual,
                    DeudaActual = deudaActual,
                    DeudaPromedio = deudaPromedio,
                    DeudaDespuesDelPrestamo = deudaDespuesPrestamo,
                    MensajeAdvertencia = deudaActual > deudaPromedio
                        ? "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema."
                        : "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema."
                };

                return View("AdvertenciaRiesgoPrestamo", advertenciaViewModel);
            }

            // Si no es alto riesgo, proceder directamente
            return await ProcesarAsignacionPrestamo(model);
        }

        /// <summary>
        /// Confirma y procesa la asignación del préstamo (después de advertencia o directo)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarAsignacionPrestamo(AdvertenciaRiesgoViewModel model)
        {
            var configuracionModel = new ConfigurarPrestamoViewModel
            {
                ClienteId = model.ClienteId,
                MontoCapital = model.MontoCapital,
                PlazoMeses = model.PlazoMeses,
                TasaInteresAnual = model.TasaInteresAnual
            };

            return await ProcesarAsignacionPrestamo(configuracionModel);
        }

        /// <summary>
        /// Método privado que procesa la asignación real del préstamo
        /// </summary>
        private async Task<IActionResult> ProcesarAsignacionPrestamo(ConfigurarPrestamoViewModel model)
        {
            var cliente = await _userManager.FindByIdAsync(model.ClienteId);
            var adminId = _userManager.GetUserId(User);

            // Generar número de préstamo único
            var numeroPrestamo = await _repositorioPrestamo.GenerarNumeroPrestamoUnicoAsync();

            // Calcular cuota mensual
            var cuotaMensual = _servicioCalculoPrestamo.CalcularCuotaMensual(
                model.MontoCapital,
                model.TasaInteresAnual,
                model.PlazoMeses);

            // Crear el préstamo
            var nuevoPrestamo = new Prestamo
            {
                NumeroPrestamo = numeroPrestamo,
                MontoCapital = model.MontoCapital,
                TasaInteresAnual = model.TasaInteresAnual,
                PlazoMeses = model.PlazoMeses,
                CuotaMensual = cuotaMensual,
                EstaActivo = true,
                ClienteId = model.ClienteId,
                AdministradorId = adminId,
                FechaCreacion = DateTime.Now
            };

            await _repositorioPrestamo.AgregarAsync(nuevoPrestamo);
            await _repositorioPrestamo.GuardarCambiosAsync();

            // Generar tabla de amortización
            var tablaAmortizacion = _servicioCalculoPrestamo.GenerarTablaAmortizacion(
                DateTime.Now,
                model.MontoCapital,
                model.TasaInteresAnual,
                model.PlazoMeses);

            foreach (var (fechaPago, montoCuota) in tablaAmortizacion)
            {
                var cuota = new CuotaPrestamo
                {
                    FechaPago = fechaPago,
                    MontoCuota = montoCuota,
                    EstaPagada = false,
                    EstaAtrasada = false,
                    PrestamoId = nuevoPrestamo.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioCuotaPrestamo.AgregarAsync(cuota);
            }

            await _repositorioCuotaPrestamo.GuardarCambiosAsync();

            // Depositar el monto en la cuenta principal del cliente
            var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(model.ClienteId);

            if (cuentaPrincipal != null)
            {
                cuentaPrincipal.Balance += model.MontoCapital;
                await _repositorioCuenta.ActualizarAsync(cuentaPrincipal);

                // Registrar la transacción del depósito del préstamo
                var transaccion = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = model.MontoCapital,
                    TipoTransaccion = Constantes.TipoCredito,
                    Beneficiario = cuentaPrincipal.NumeroCuenta,
                    Origen = numeroPrestamo,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuentaPrincipal.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccion);
                await _repositorioTransaccion.GuardarCambiosAsync();
            }

            // Enviar correo de notificación
            await _servicioCorreo.EnviarNotificacionPrestamoAprobadoAsync(
                cliente.Email,
                cliente.NombreCompleto,
                model.MontoCapital,
                model.PlazoMeses,
                model.TasaInteresAnual,
                cuotaMensual);

            _logger.LogInformation($"Préstamo {numeroPrestamo} asignado al cliente {cliente.UserName}");

            TempData["MensajeExito"] = $"Préstamo asignado exitosamente. Número de préstamo: {numeroPrestamo}";
            return RedirectToAction(nameof(Prestamos));
        }

        /// <summary>
        /// Ver detalle del préstamo (tabla de amortización)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

            if (prestamo == null)
            {
                return NotFound();
            }

            // Cargar relaciones necesarias
            prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(prestamo.NumeroPrestamo);

            var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(id);

            var viewModel = new DetallePrestamoViewModel
            {
                Id = prestamo.Id,
                NumeroPrestamo = prestamo.NumeroPrestamo,
                NombreCliente = prestamo.Cliente.NombreCompleto,
                MontoCapital = prestamo.MontoCapital,
                TasaInteresAnual = prestamo.TasaInteresAnual,
                PlazoMeses = prestamo.PlazoMeses,
                CuotaMensual = prestamo.CuotaMensual,
                EstaActivo = prestamo.EstaActivo,
                TablaAmortizacion = cuotas.Select(c => new CuotaPrestamoViewModel
                {
                    Id = c.Id,
                    FechaPago = c.FechaPago.ToString("dd/MM/yyyy"),
                    MontoCuota = c.MontoCuota,
                    EstaPagada = c.EstaPagada,
                    EstaAtrasada = c.EstaAtrasada
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Mostrar formulario para editar tasa de interés
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarPrestamo(int id)
        {
            var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

            if (prestamo == null)
            {
                return NotFound();
            }

            // Cargar cliente
            prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(prestamo.NumeroPrestamo);

            var viewModel = new EditarPrestamoViewModel
            {
                Id = prestamo.Id,
                NumeroPrestamo = prestamo.NumeroPrestamo,
                NombreCliente = prestamo.Cliente.NombreCompleto,
                TasaInteresAnual = prestamo.TasaInteresAnual
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesar edición de tasa de interés (recalcula cuotas futuras)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPrestamo(EditarPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(model.Id);

            if (prestamo == null)
            {
                return NotFound();
            }

            // Actualizar la tasa de interés
            prestamo.TasaInteresAnual = model.TasaInteresAnual;

            // Obtener cuotas futuras (no vencidas y no pagadas)
            var cuotasFuturas = await _repositorioCuotaPrestamo.ObtenerCuotasFuturasAsync(prestamo.Id);

            if (cuotasFuturas.Any())
            {
                // Calcular cuántas cuotas quedan pendientes
                int cuotasRestantes = cuotasFuturas.Count();

                // Calcular el capital pendiente (suma de todas las cuotas futuras antes del cambio)
                decimal capitalPendiente = cuotasFuturas.Sum(c => c.MontoCuota);

                // Recalcular la nueva cuota mensual
                var nuevaCuotaMensual = _servicioCalculoPrestamo.CalcularCuotaMensual(
                    capitalPendiente,
                    model.TasaInteresAnual,
                    cuotasRestantes);

                // Actualizar todas las cuotas futuras con el nuevo monto
                foreach (var cuota in cuotasFuturas)
                {
                    cuota.MontoCuota = nuevaCuotaMensual;
                    await _repositorioCuotaPrestamo.ActualizarAsync(cuota);
                }

                // Actualizar la cuota mensual del préstamo
                prestamo.CuotaMensual = nuevaCuotaMensual;

                await _repositorioCuotaPrestamo.GuardarCambiosAsync();

                // Enviar correo al cliente notificando el cambio
                var cliente = await _userManager.FindByIdAsync(prestamo.ClienteId);
                await _servicioCorreo.EnviarNotificacionCambioTasaPrestamoAsync(
                    cliente.Email,
                    cliente.NombreCompleto,
                    model.TasaInteresAnual,
                    nuevaCuotaMensual);
            }

            await _repositorioPrestamo.ActualizarAsync(prestamo);
            await _repositorioPrestamo.GuardarCambiosAsync();

            _logger.LogInformation($"Tasa de interés del préstamo {prestamo.NumeroPrestamo} actualizada");

            TempData["MensajeExito"] = "Tasa de interés actualizada exitosamente.";
            return RedirectToAction(nameof(Prestamos));
        }

        #endregion

        #region Gestión de Tarjetas de Crédito

        /// <summary>
        /// Muestra la lista paginada de tarjetas de crédito
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Tarjetas(int pagina = 1, string cedula = null, bool? estado = null)
        {
            // Obtener tarjetas paginadas con filtros
            var (tarjetas, total) = await _repositorioTarjeta.ObtenerTarjetasPaginadasAsync(
                pagina,
                Constantes.TamanoPaginaPorDefecto,
                cedula,
                estado);

            // Mapear a ViewModels
            var tarjetasViewModel = tarjetas.Select(t => new TarjetaListaItemViewModel
            {
                Id = t.Id,
                NumeroTarjeta = t.NumeroTarjeta,
                UltimosCuatroDigitos = t.UltimosCuatroDigitos,
                NombreCliente = t.Cliente.Nombre,
                ApellidoCliente = t.Cliente.Apellido,
                LimiteCredito = t.LimiteCredito,
                FechaExpiracion = t.FechaExpiracion,
                DeudaActual = t.DeudaActual,
                EstaActiva = t.EstaActiva
            }).ToList();

            var viewModel = new ListaTarjetasViewModel
            {
                Tarjetas = tarjetasViewModel,
                PaginaActual = pagina,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)Constantes.TamanoPaginaPorDefecto),
                FiltroCedula = cedula,
                FiltroEstado = estado
            };

            return View(viewModel);
        }

        /// <summary>
        /// Paso 1: Mostrar lista de clientes activos para seleccionar uno
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SeleccionarClienteTarjeta(string cedula = null)
        {
            // Obtener todos los usuarios con rol Cliente que estén activos
            var todosLosUsuarios = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);
            var clientesActivos = todosLosUsuarios.Where(u => u.EstaActivo).ToList();

            var clientesViewModel = new List<ClienteParaTarjetaViewModel>();

            foreach (var cliente in clientesActivos)
            {
                // Si hay filtro por cédula, aplicarlo
                if (!string.IsNullOrEmpty(cedula) && cliente.Cedula != cedula)
                    continue;

                var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(cliente.Id);

                clientesViewModel.Add(new ClienteParaTarjetaViewModel
                {
                    Id = cliente.Id,
                    Cedula = cliente.Cedula,
                    NombreCompleto = cliente.NombreCompleto,
                    Correo = cliente.Email,
                    DeudaTotal = deudaTotal
                });
            }

            var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

            var viewModel = new SeleccionarClienteTarjetaViewModel
            {
                Clientes = clientesViewModel,
                DeudaPromedio = deudaPromedio,
                FiltroCedula = cedula
            };

            return View(viewModel);
        }

        /// <summary>
        /// Paso 2: Mostrar formulario para configurar la tarjeta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfigurarTarjeta(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                return RedirectToAction(nameof(SeleccionarClienteTarjeta));
            }

            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(SeleccionarClienteTarjeta));
            }

            var viewModel = new ConfigurarTarjetaViewModel
            {
                ClienteId = clienteId,
                NombreCliente = cliente.NombreCompleto
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la asignación de la tarjeta de crédito
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTarjeta(ConfigurarTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarTarjeta", model);
            }

            var cliente = await _userManager.FindByIdAsync(model.ClienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Tarjetas));
            }

            var adminId = _userManager.GetUserId(User);

            // Generar número de tarjeta único (16 dígitos)
            var numeroTarjeta = await _repositorioTarjeta.GenerarNumeroTarjetaUnicoAsync();

            // Generar CVC aleatorio de 3 dígitos
            var random = new Random();
            var cvc = random.Next(100, 1000).ToString();

            // Cifrar el CVC con SHA-256
            var cvcCifrado = _servicioCifrado.CifrarCVC(cvc);

            // Calcular fecha de expiración (3 años desde hoy)
            var fechaExpiracion = DateTime.Now.AddYears(3);
            var fechaExpiracionFormato = fechaExpiracion.ToString("MM/yy");

            // Crear la tarjeta
            var nuevaTarjeta = new TarjetaCredito
            {
                NumeroTarjeta = numeroTarjeta,
                LimiteCredito = model.LimiteCredito,
                DeudaActual = 0,
                FechaExpiracion = fechaExpiracionFormato,
                CVC = cvcCifrado,
                EstaActiva = true,
                ClienteId = model.ClienteId,
                AdministradorId = adminId,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTarjeta.AgregarAsync(nuevaTarjeta);
            await _repositorioTarjeta.GuardarCambiosAsync();

            _logger.LogInformation($"Tarjeta {numeroTarjeta} asignada al cliente {cliente.UserName}");

            TempData["MensajeExito"] = $"Tarjeta asignada exitosamente. Número de tarjeta: {numeroTarjeta}";
            return RedirectToAction(nameof(Tarjetas));
        }

        /// <summary>
        /// Ver detalle de la tarjeta (consumos)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

            if (tarjeta == null)
            {
                return NotFound();
            }

            // Cargar relaciones necesarias
            tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

            var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(id);

            var viewModel = new DetalleTarjetaViewModel
            {
                Id = tarjeta.Id,
                NumeroTarjeta = tarjeta.NumeroTarjeta,
                UltimosCuatroDigitos = tarjeta.UltimosCuatroDigitos,
                NombreCliente = tarjeta.Cliente.NombreCompleto,
                LimiteCredito = tarjeta.LimiteCredito,
                DeudaActual = tarjeta.DeudaActual,
                CreditoDisponible = tarjeta.CreditoDisponible,
                FechaExpiracion = tarjeta.FechaExpiracion,
                EstaActiva = tarjeta.EstaActiva,
                Consumos = consumos.Select(c => new ConsumoTarjetaViewModel
                {
                    Id = c.Id,
                    FechaConsumo = c.FechaConsumo.ToString("dd/MM/yyyy HH:mm"),
                    Monto = c.Monto,
                    NombreComercio = c.NombreComercio,
                    EstadoConsumo = c.EstadoConsumo
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Mostrar formulario para editar límite de crédito
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarTarjeta(int id)
        {
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

            if (tarjeta == null)
            {
                return NotFound();
            }

            // Cargar cliente
            tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

            var viewModel = new EditarTarjetaViewModel
            {
                Id = tarjeta.Id,
                NumeroTarjeta = tarjeta.NumeroTarjeta,
                UltimosCuatroDigitos = tarjeta.UltimosCuatroDigitos,
                NombreCliente = tarjeta.Cliente.NombreCompleto,
                DeudaActual = tarjeta.DeudaActual,
                LimiteCredito = tarjeta.LimiteCredito
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesar edición de límite de crédito
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTarjeta(EditarTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(model.Id);

            if (tarjeta == null)
            {
                return NotFound();
            }

            // Validar que el nuevo límite no sea inferior a la deuda actual
            if (model.LimiteCredito < tarjeta.DeudaActual)
            {
                ModelState.AddModelError("LimiteCredito",
                    $"El límite no puede ser menor a la deuda actual (RD${tarjeta.DeudaActual:N2}).");
                return View(model);
            }

            // Actualizar el límite
            tarjeta.LimiteCredito = model.LimiteCredito;
            await _repositorioTarjeta.ActualizarAsync(tarjeta);
            await _repositorioTarjeta.GuardarCambiosAsync();

            // Enviar correo al cliente notificando el cambio
            var cliente = await _userManager.FindByIdAsync(tarjeta.ClienteId);
            await _servicioCorreo.EnviarNotificacionCambioLimiteTarjetaAsync(
                cliente.Email,
                cliente.NombreCompleto,
                tarjeta.UltimosCuatroDigitos,
                model.LimiteCredito);

            _logger.LogInformation($"Límite de tarjeta {tarjeta.NumeroTarjeta} actualizado");

            TempData["MensajeExito"] = "Límite de crédito actualizado exitosamente.";
            return RedirectToAction(nameof(Tarjetas));
        }

        /// <summary>
        /// Mostrar pantalla de confirmación para cancelar tarjeta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CancelarTarjeta(int id)
        {
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

            if (tarjeta == null)
            {
                return NotFound();
            }

            // Cargar cliente
            tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

            var viewModel = new CancelarTarjetaViewModel
            {
                Id = tarjeta.Id,
                UltimosCuatroDigitos = tarjeta.UltimosCuatroDigitos,
                NombreCliente = tarjeta.Cliente.NombreCompleto,
                DeudaActual = tarjeta.DeudaActual
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesar cancelación de tarjeta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionTarjeta(int id)
        {
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

            if (tarjeta == null)
            {
                return NotFound();
            }

            // Verificar si tiene deuda
            if (tarjeta.DeudaActual > 0)
            {
                TempData["MensajeError"] = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.";
                return RedirectToAction(nameof(Tarjetas));
            }

            // Cancelar la tarjeta
            tarjeta.EstaActiva = false;
            await _repositorioTarjeta.ActualizarAsync(tarjeta);
            await _repositorioTarjeta.GuardarCambiosAsync();

            _logger.LogInformation($"Tarjeta {tarjeta.NumeroTarjeta} cancelada");

            TempData["MensajeExito"] = "Tarjeta cancelada exitosamente.";
            return RedirectToAction(nameof(Tarjetas));
        }

        #endregion

        #region Gestión de Cuentas de Ahorro

        /// <summary>
        /// Muestra la lista paginada de cuentas de ahorro
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Cuentas(int pagina = 1, string cedula = null, bool? estado = null, bool? tipo = null)
        {
            // Obtener cuentas paginadas con filtros
            var (cuentas, total) = await _repositorioCuenta.ObtenerCuentasPaginadasAsync(
                pagina,
                Constantes.TamanoPaginaPorDefecto,
                cedula,
                estado,
                tipo);

            // Mapear a ViewModels
            var cuentasViewModel = cuentas.Select(c => new CuentaListaItemViewModel
            {
                Id = c.Id,
                NumeroCuenta = c.NumeroCuenta,
                NombreCliente = c.Usuario.Nombre,
                ApellidoCliente = c.Usuario.Apellido,
                Balance = c.Balance,
                EsPrincipal = c.EsPrincipal,
                EstaActiva = c.EstaActiva
            }).ToList();

            var viewModel = new ListaCuentasViewModel
            {
                Cuentas = cuentasViewModel,
                PaginaActual = pagina,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)Constantes.TamanoPaginaPorDefecto),
                FiltroCedula = cedula,
                FiltroEstado = estado,
                FiltroTipo = tipo
            };

            return View(viewModel);
        }

        /// <summary>
        /// Paso 1: Mostrar lista de clientes activos para seleccionar uno
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SeleccionarClienteCuenta(string cedula = null)
        {
            // Obtener todos los usuarios con rol Cliente que estén activos
            var todosLosUsuarios = await _userManager.GetUsersInRoleAsync(Constantes.RolCliente);
            var clientesActivos = todosLosUsuarios.Where(u => u.EstaActivo).ToList();

            var clientesViewModel = new List<ClienteParaCuentaViewModel>();

            foreach (var cliente in clientesActivos)
            {
                // Si hay filtro por cédula, aplicarlo
                if (!string.IsNullOrEmpty(cedula) && cliente.Cedula != cedula)
                    continue;

                var deudaTotal = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(cliente.Id);

                clientesViewModel.Add(new ClienteParaCuentaViewModel
                {
                    Id = cliente.Id,
                    Cedula = cliente.Cedula,
                    NombreCompleto = cliente.NombreCompleto,
                    Correo = cliente.Email,
                    DeudaTotal = deudaTotal
                });
            }

            var viewModel = new SeleccionarClienteCuentaViewModel
            {
                Clientes = clientesViewModel,
                FiltroCedula = cedula
            };

            return View(viewModel);
        }

        /// <summary>
        /// Paso 2: Mostrar formulario para configurar la cuenta secundaria
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfigurarCuenta(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                return RedirectToAction(nameof(SeleccionarClienteCuenta));
            }

            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(SeleccionarClienteCuenta));
            }

            var viewModel = new ConfigurarCuentaViewModel
            {
                ClienteId = clienteId,
                NombreCliente = cliente.NombreCompleto
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la asignación de la cuenta de ahorro secundaria
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarCuenta(ConfigurarCuentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarCuenta", model);
            }

            var cliente = await _userManager.FindByIdAsync(model.ClienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Cuentas));
            }

            // Generar número de cuenta único (9 dígitos)
            var numeroCuenta = await _repositorioCuenta.GenerarNumeroCuentaUnicoAsync();

            // Crear la cuenta secundaria
            var nuevaCuenta = new CuentaAhorro
            {
                NumeroCuenta = numeroCuenta,
                Balance = model.BalanceInicial,
                EsPrincipal = false, // Es cuenta secundaria
                EstaActiva = true,
                UsuarioId = model.ClienteId,
                FechaCreacion = DateTime.Now
            };

            await _repositorioCuenta.AgregarAsync(nuevaCuenta);
            await _repositorioCuenta.GuardarCambiosAsync();

            _logger.LogInformation($"Cuenta secundaria {numeroCuenta} asignada al cliente {cliente.UserName}");

            TempData["MensajeExito"] = $"Cuenta de ahorro asignada exitosamente. Número de cuenta: {numeroCuenta}";
            return RedirectToAction(nameof(Cuentas));
        }

        /// <summary>
        /// Ver detalle de la cuenta (transacciones)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(id);

            if (cuenta == null)
            {
                return NotFound();
            }

            // Cargar relaciones necesarias
            cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);

            var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(id);

            var viewModel = new DetalleCuentaViewModel
            {
                Id = cuenta.Id,
                NumeroCuenta = cuenta.NumeroCuenta,
                NombreCliente = cuenta.Usuario.NombreCompleto,
                Balance = cuenta.Balance,
                EsPrincipal = cuenta.EsPrincipal,
                EstaActiva = cuenta.EstaActiva,
                Transacciones = transacciones.Select(t => new TransaccionViewModel
                {
                    Id = t.Id,
                    FechaTransaccion = t.FechaTransaccion.ToString("dd/MM/yyyy HH:mm"),
                    Monto = t.Monto,
                    TipoTransaccion = t.TipoTransaccion,
                    Beneficiario = t.Beneficiario,
                    Origen = t.Origen,
                    EstadoTransaccion = t.EstadoTransaccion
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Mostrar pantalla de confirmación para cancelar cuenta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CancelarCuenta(int id)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(id);

            if (cuenta == null)
            {
                return NotFound();
            }

            // No se puede cancelar una cuenta principal
            if (cuenta.EsPrincipal)
            {
                TempData["MensajeError"] = "No se puede cancelar una cuenta principal.";
                return RedirectToAction(nameof(Cuentas));
            }

            // Cargar usuario
            cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);

            var viewModel = new CancelarCuentaViewModel
            {
                Id = cuenta.Id,
                NumeroCuenta = cuenta.NumeroCuenta,
                NombreCliente = cuenta.Usuario.NombreCompleto,
                Balance = cuenta.Balance,
                EsPrincipal = cuenta.EsPrincipal
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesar cancelación de cuenta secundaria
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionCuenta(int id)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(id);

            if (cuenta == null)
            {
                return NotFound();
            }

            // Verificar que no sea cuenta principal
            if (cuenta.EsPrincipal)
            {
                TempData["MensajeError"] = "No se puede cancelar una cuenta principal.";
                return RedirectToAction(nameof(Cuentas));
            }

            // Si tiene balance, transferirlo a la cuenta principal
            if (cuenta.Balance > 0)
            {
                var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(cuenta.UsuarioId);

                if (cuentaPrincipal != null)
                {
                    // Transferir el balance
                    cuentaPrincipal.Balance += cuenta.Balance;
                    cuenta.Balance = 0;

                    await _repositorioCuenta.ActualizarAsync(cuentaPrincipal);

                    // Registrar la transferencia en ambas cuentas
                    var transaccionOrigen = new Transaccion
                    {
                        FechaTransaccion = DateTime.Now,
                        Monto = cuenta.Balance,
                        TipoTransaccion = Constantes.TipoDebito,
                        Beneficiario = cuentaPrincipal.NumeroCuenta,
                        Origen = cuenta.NumeroCuenta,
                        EstadoTransaccion = Constantes.EstadoAprobada,
                        CuentaAhorroId = cuenta.Id,
                        FechaCreacion = DateTime.Now
                    };

                    var transaccionDestino = new Transaccion
                    {
                        FechaTransaccion = DateTime.Now,
                        Monto = cuenta.Balance,
                        TipoTransaccion = Constantes.TipoCredito,
                        Beneficiario = cuentaPrincipal.NumeroCuenta,
                        Origen = cuenta.NumeroCuenta,
                        EstadoTransaccion = Constantes.EstadoAprobada,
                        CuentaAhorroId = cuentaPrincipal.Id,
                        FechaCreacion = DateTime.Now
                    };

                    await _repositorioTransaccion.AgregarAsync(transaccionOrigen);
                    await _repositorioTransaccion.AgregarAsync(transaccionDestino);
                    await _repositorioTransaccion.GuardarCambiosAsync();
                }
            }

            // Cancelar la cuenta
            cuenta.EstaActiva = false;
            await _repositorioCuenta.ActualizarAsync(cuenta);
            await _repositorioCuenta.GuardarCambiosAsync();

            _logger.LogInformation($"Cuenta {cuenta.NumeroCuenta} cancelada");

            TempData["MensajeExito"] = "Cuenta cancelada exitosamente.";
            return RedirectToAction(nameof(Cuentas));
        }

        #endregion
    }
}