using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels.Cliente;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.Controllers
{

    /// Controlador para funcionalidades del cliente
    
    [Authorize(Policy = "SoloCliente")]
    public class ClienteController : Controller
    {
        private readonly IServicioDashboardCliente _servicioDashboard;
        private readonly IServicioTransaccion _servicioTransaccion;
        private readonly IServicioBeneficiario _servicioBeneficiario;
        private readonly IServicioCuentaAhorro _servicioCuenta;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(
            IServicioDashboardCliente servicioDashboard,
            IServicioTransaccion servicioTransaccion,
            IServicioBeneficiario servicioBeneficiario,
            IServicioCuentaAhorro servicioCuenta,
            IMapper mapper,
            ILogger<ClienteController> logger)
        {
            _servicioDashboard = servicioDashboard;
            _servicioTransaccion = servicioTransaccion;
            _servicioBeneficiario = servicioBeneficiario;
            _servicioCuenta = servicioCuenta;
            _mapper = mapper;
            _logger = logger;
        }

        private string ObtenerUsuarioActualId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        //  DASHBOARD 

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioActualId();

            var resultado = await _servicioDashboard.ObtenerDashboardAsync(usuarioId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new HomeClienteViewModel
                {
                    CuentasAhorro = new List<CuentaClienteViewModel>(),
                    Prestamos = new List<PrestamoClienteViewModel>(),
                    TarjetasCredito = new List<TarjetaClienteViewModel>()
                });
            }

            var viewModel = _mapper.Map<HomeClienteViewModel>(resultado.Datos);
            return View(viewModel);
        }

        //  DETALLES DE PRODUCTOS 

        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            var usuarioId = ObtenerUsuarioActualId();

            var resultado = await _servicioDashboard.ObtenerDetalleCuentaAsync(id, usuarioId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<DetalleCuentaClienteViewModel>(resultado.Datos);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            var usuarioId = ObtenerUsuarioActualId();

            var resultado = await _servicioDashboard.ObtenerDetallePrestamoAsync(id, usuarioId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<DetallePrestamoClienteViewModel>(resultado.Datos);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            var usuarioId = ObtenerUsuarioActualId();

            var resultado = await _servicioDashboard.ObtenerDetalleTarjetaAsync(id, usuarioId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<DetalleTarjetaClienteViewModel>(resultado.Datos);
            return View(viewModel);
        }

        //  BENEFICIARIOS 

        [HttpGet]
        public async Task<IActionResult> Beneficiarios()
        {
            var usuarioId = ObtenerUsuarioActualId();
            var resultado = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new ListaBeneficiariosViewModel
                {
                    Beneficiarios = new List<BeneficiarioItemViewModel>()
                });
            }

            var viewModel = new ListaBeneficiariosViewModel
            {
                Beneficiarios = _mapper.Map<IEnumerable<BeneficiarioItemViewModel>>(resultado.Datos)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarBeneficiario(AgregarBeneficiarioViewModel model)
        {
            _logger.LogInformation($"=== AGREGANDO BENEFICIARIO ===");
            _logger.LogInformation($"NumeroCuenta: {model.NumeroCuenta}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { 
                        Campo = x.Key, 
                        Errores = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                
                foreach (var error in errores)
                {
                    _logger.LogError($"Error en {error.Campo}: {error.Errores}");
                }
                
                TempData["ErrorMessage"] = $"Error de validación: {string.Join("; ", errores.Select(e => $"{e.Campo}: {e.Errores}"))}";
                return RedirectToAction(nameof(Beneficiarios));
            }

            var usuarioId = ObtenerUsuarioActualId();
            var resultado = await _servicioBeneficiario.AgregarBeneficiarioAsync(usuarioId, model.NumeroCuenta);

            TempData[resultado.Exito ? "SuccessMessage" : "ErrorMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(Beneficiarios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarBeneficiario(int id)
        {
            var usuarioId = ObtenerUsuarioActualId();
            var resultado = await _servicioBeneficiario.EliminarBeneficiarioAsync(id, usuarioId);

            TempData[resultado.Exito ? "SuccessMessage" : "ErrorMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(Beneficiarios));
        }

        //  TRANSACCIONES 

        [HttpGet]
        public async Task<IActionResult> TransaccionExpress()
        {
            var viewModel = new TransaccionExpressViewModel
            {
                CuentasDisponibles = await ObtenerCuentasSelectAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionExpress(TransaccionExpressViewModel model)
        {
            _logger.LogInformation($"=== TRANSACCION EXPRESS ===");
            _logger.LogInformation($"CuentaOrigenId: {model.CuentaOrigenId}");
            _logger.LogInformation($"NumeroCuentaDestino: {model.NumeroCuentaDestino}");
            _logger.LogInformation($"Monto: {model.Monto}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { 
                        Campo = x.Key, 
                        Errores = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                
                foreach (var error in errores)
                {
                    _logger.LogError($"Error en {error.Campo}: {error.Errores}");
                }
                
                TempData["ErrorMessage"] = $"Error de validación: {string.Join("; ", errores.Select(e => $"{e.Campo}: {e.Errores}"))}";
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var infoDestino = await _servicioTransaccion.ObtenerInfoCuentaDestinoAsync(model.NumeroCuentaDestino);

            if (!infoDestino.Exito)
            {
                _logger.LogWarning($"No se pudo obtener info de cuenta destino: {infoDestino.Mensaje}");
                TempData["ErrorMessage"] = infoDestino.Mensaje;
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var confirmacionVM = new ConfirmarTransaccionViewModel
            {
                NombreDestinatario = infoDestino.Datos.nombre,
                ApellidoDestinatario = infoDestino.Datos.apellido,
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                CuentaOrigenId = model.CuentaOrigenId
            };

            return View("ConfirmarTransaccionExpress", confirmacionVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarTransaccionExpress(ConfirmarTransaccionViewModel model)
        {
            var usuarioId = ObtenerUsuarioActualId();

            var dto = new TransaccionExpressDTO
            {
                CuentaOrigenId = model.CuentaOrigenId,
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                UsuarioId = usuarioId
            };

            var resultado = await _servicioTransaccion.RealizarTransaccionExpressAsync(dto);

            TempData[resultado.Exito ? "SuccessMessage" : "ErrorMessage"] = resultado.Mensaje;
            return resultado.Exito ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(TransaccionExpress));
        }

        //  PAGOS 

        [HttpGet]
        public async Task<IActionResult> PagarTarjeta()
        {
            var viewModel = new PagoTarjetaViewModel
            {
                TarjetasDisponibles = await ObtenerTarjetasSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasSelectAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(PagoTarjetaViewModel model)
        {
            _logger.LogInformation($"=== PAGAR TARJETA ===");
            _logger.LogInformation($"TarjetaId: {model.TarjetaId}");
            _logger.LogInformation($"CuentaOrigenId: {model.CuentaOrigenId}");
            _logger.LogInformation($"Monto: {model.Monto}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { 
                        Campo = x.Key, 
                        Errores = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                
                foreach (var error in errores)
                {
                    _logger.LogError($"Error en {error.Campo}: {error.Errores}");
                }
                
                TempData["ErrorMessage"] = $"Error de validación: {string.Join("; ", errores.Select(e => $"{e.Campo}: {e.Errores}"))}";
                model.TarjetasDisponibles = await ObtenerTarjetasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var dto = new PagoTarjetaClienteDTO
            {
                TarjetaId = model.TarjetaId,
                CuentaOrigenId = model.CuentaOrigenId,
                Monto = model.Monto,
                UsuarioId = ObtenerUsuarioActualId()
            };

            var resultado = await _servicioTransaccion.PagarTarjetaCreditoClienteAsync(dto);

            if (resultado.Exito)
            {
                _logger.LogInformation($"Pago de tarjeta exitoso");
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning($"Error al pagar tarjeta: {resultado.Mensaje}");
            TempData["ErrorMessage"] = resultado.Mensaje;
            model.TarjetasDisponibles = await ObtenerTarjetasSelectAsync();
            model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PagarPrestamo()
        {
            var viewModel = new PagoPrestamoViewModel
            {
                PrestamosDisponibles = await ObtenerPrestamosSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasSelectAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPrestamo(PagoPrestamoViewModel model)
        {
            _logger.LogInformation($"=== PAGAR PRESTAMO ===");
            _logger.LogInformation($"PrestamoId: {model.PrestamoId}");
            _logger.LogInformation($"CuentaOrigenId: {model.CuentaOrigenId}");
            _logger.LogInformation($"Monto: {model.Monto}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { 
                        Campo = x.Key, 
                        Errores = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                
                foreach (var error in errores)
                {
                    _logger.LogError($"Error en {error.Campo}: {error.Errores}");
                }
                
                TempData["ErrorMessage"] = $"Error de validación: {string.Join("; ", errores.Select(e => $"{e.Campo}: {e.Errores}"))}";
                model.PrestamosDisponibles = await ObtenerPrestamosSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var dto = new PagoPrestamoClienteDTO
            {
                PrestamoId = model.PrestamoId,
                CuentaOrigenId = model.CuentaOrigenId,
                Monto = model.Monto,
                UsuarioId = ObtenerUsuarioActualId()
            };

            var resultado = await _servicioTransaccion.PagarPrestamoClienteAsync(dto);

            if (resultado.Exito)
            {
                _logger.LogInformation($"Pago de préstamo exitoso");
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning($"Error al pagar préstamo: {resultado.Mensaje}");
            TempData["ErrorMessage"] = resultado.Mensaje;
            model.PrestamosDisponibles = await ObtenerPrestamosSelectAsync();
            model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PagarBeneficiario()
        {
            var viewModel = new PagoBeneficiarioViewModel
            {
                BeneficiariosDisponibles = await ObtenerBeneficiariosSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasSelectAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarBeneficiario(PagoBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los campos del formulario";
                model.BeneficiariosDisponibles = await ObtenerBeneficiariosSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var usuarioId = ObtenerUsuarioActualId();
            var beneficiariosResult = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

            if (!beneficiariosResult.Exito)
            {
                TempData["ErrorMessage"] = "No pudimos obtener el beneficiario";
                return RedirectToAction(nameof(PagarBeneficiario));
            }

            var beneficiario = beneficiariosResult.Datos.FirstOrDefault(b => b.Id == model.BeneficiarioId);

            if (beneficiario == null)
            {
                TempData["ErrorMessage"] = "Beneficiario no encontrado";
                return RedirectToAction(nameof(PagarBeneficiario));
            }

            var confirmacionVM = new ConfirmarTransaccionViewModel
            {
                NombreDestinatario = beneficiario.NombreBeneficiario,
                ApellidoDestinatario = beneficiario.ApellidoBeneficiario,
                NumeroCuentaDestino = beneficiario.NumeroCuentaBeneficiario,
                Monto = model.Monto,
                CuentaOrigenId = model.CuentaOrigenId
            };

            TempData["BeneficiarioId"] = model.BeneficiarioId;
            return View("ConfirmarPagoBeneficiario", confirmacionVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoBeneficiario(ConfirmarTransaccionViewModel model)
        {
            var usuarioId = ObtenerUsuarioActualId();
            var beneficiarioId = (int)TempData["BeneficiarioId"];

            var dto = new PagoBeneficiarioDTO
            {
                BeneficiarioId = beneficiarioId,
                CuentaOrigenId = model.CuentaOrigenId,
                Monto = model.Monto,
                UsuarioId = usuarioId
            };

            var resultado = await _servicioTransaccion.PagarBeneficiarioAsync(dto);

            TempData[resultado.Exito ? "SuccessMessage" : "ErrorMessage"] = resultado.Mensaje;
            return resultado.Exito ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(PagarBeneficiario));
        }

        [HttpGet]
        public async Task<IActionResult> AvanceEfectivo()
        {
            var viewModel = new AvanceEfectivoViewModel
            {
                TarjetasDisponibles = await ObtenerTarjetasSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasSelectAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvanceEfectivo(AvanceEfectivoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los campos del formulario";
                model.TarjetasDisponibles = await ObtenerTarjetasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var dto = new AvanceEfectivoDTO
            {
                TarjetaId = model.TarjetaId,
                CuentaDestinoId = model.CuentaDestinoId,
                Monto = model.Monto,
                UsuarioId = ObtenerUsuarioActualId()
            };

            var resultado = await _servicioTransaccion.RealizarAvanceEfectivoClienteAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = resultado.Mensaje;
            model.TarjetasDisponibles = await ObtenerTarjetasSelectAsync();
            model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> TransferirEntreCuentas()
        {
            var viewModel = new TransferenciaEntreCuentasViewModel
            {
                CuentasDisponibles = await ObtenerCuentasSelectAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferirEntreCuentas(TransferenciaEntreCuentasViewModel model)
        {
            _logger.LogInformation($"=== TRANSFERIR ENTRE CUENTAS ===");
            _logger.LogInformation($"CuentaOrigenId: {model.CuentaOrigenId}");
            _logger.LogInformation($"CuentaDestinoId: {model.CuentaDestinoId}");
            _logger.LogInformation($"Monto: {model.Monto}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { 
                        Campo = x.Key, 
                        Errores = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                
                foreach (var error in errores)
                {
                    _logger.LogError($"Error en {error.Campo}: {error.Errores}");
                }
                
                TempData["ErrorMessage"] = $"Error de validación: {string.Join("; ", errores.Select(e => $"{e.Campo}: {e.Errores}"))}";
                model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
                return View(model);
            }

            var dto = new TransferirEntrePropiasDTO
            {
                CuentaOrigenId = model.CuentaOrigenId,
                CuentaDestinoId = model.CuentaDestinoId,
                Monto = model.Monto,
                UsuarioId = ObtenerUsuarioActualId()
            };

            var resultado = await _servicioCuenta.TransferirEntreCuentasPropiasAsync(dto);

            if (resultado.Exito)
            {
                _logger.LogInformation($"Transferencia entre cuentas exitosa");
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning($"Error al transferir: {resultado.Mensaje}");
            TempData["ErrorMessage"] = resultado.Mensaje;
            model.CuentasDisponibles = await ObtenerCuentasSelectAsync();
            return View(model);
        }

        //  MÉTODOS HELPER 

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentasSelectAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();
            var dashboard = await _servicioDashboard.ObtenerDashboardAsync(usuarioId);

            if (!dashboard.Exito || dashboard.Datos?.CuentasAhorro == null)
                return Enumerable.Empty<SelectListItem>();

            return dashboard.Datos.CuentasAhorro.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTarjetasSelectAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();
            var dashboard = await _servicioDashboard.ObtenerDashboardAsync(usuarioId);

            if (!dashboard.Exito || dashboard.Datos?.TarjetasCredito == null)
                return Enumerable.Empty<SelectListItem>();

            return dashboard.Datos.TarjetasCredito.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"****{t.UltimosCuatroDigitos} - Disponible: RD${t.CreditoDisponible:N2}"
            });
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerPrestamosSelectAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();
            var dashboard = await _servicioDashboard.ObtenerDashboardAsync(usuarioId);

            if (!dashboard.Exito || dashboard.Datos?.Prestamos == null)
                return Enumerable.Empty<SelectListItem>();

            return dashboard.Datos.Prestamos.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.NumeroPrestamo} - Pendiente: RD${p.MontoPendiente:N2}"
            });
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerBeneficiariosSelectAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();
            var resultado = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

            if (!resultado.Exito)
                return Enumerable.Empty<SelectListItem>();

            return resultado.Datos.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.NombreBeneficiario} {b.ApellidoBeneficiario} - {b.NumeroCuentaBeneficiario}"
            });
        }
    }
}