using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Application.ViewModels.Cajero;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
 
    [Authorize(Policy = "SoloCajero")] 
    public class CajeroController : Controller
    {
        private readonly IServicioCajero _servicioCajero;
        private readonly IMapper _mapper;
        private readonly ILogger<CajeroController> _logger;

        public CajeroController(
            IServicioCajero servicioCajero,
            IMapper mapper,
            ILogger<CajeroController> logger)
        {
            _servicioCajero = servicioCajero;
            _mapper = mapper;
            _logger = logger;
        }

        private string ObtenerCajeroActualId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // HOME (DASHBOARD DEL CAJERO) 

         
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cajeroId = ObtenerCajeroActualId();

            var resultado = await _servicioCajero.ObtenerDashboardAsync(cajeroId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new DashboardCajeroViewModel());
            }

            var viewModel = _mapper.Map<DashboardCajeroViewModel>(resultado.Datos);

            return View(viewModel);
        }

        // DEPÓSITO 

         
        [HttpGet]
        public IActionResult Deposito()
        {
            return View(new DepositoCajeroViewModel());
        }

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposito(DepositoCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            var dto = new DepositoCajeroDTO
            {
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            var resultado = await _servicioCajero.RealizarDepositoAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        //  RETIRO 

        [HttpGet]
        public IActionResult Retiro()
        {
            return View(new RetiroCajeroViewModel());
        }

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retiro(RetiroCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            var dto = new RetiroCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

  
            var resultado = await _servicioCajero.RealizarRetiroAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        //  PAGO A TARJETA DE CRÉDITO 
         
        [HttpGet]
        public IActionResult PagarTarjeta()
        {
            return View(new PagoTarjetaCajeroViewModel());
        }

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(PagoTarjetaCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            var dto = new PagoTarjetaCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                NumeroTarjeta = model.NumeroTarjeta,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            var resultado = await _servicioCajero.PagarTarjetaCreditoAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        //  PAGO A PRÉSTAMO 

         
        [HttpGet]
        public IActionResult PagarPrestamo()
        {
            return View(new PagoPrestamoCajeroViewModel());
        }

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPrestamo(PagoPrestamoCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            var dto = new PagoPrestamoCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                NumeroPrestamo = model.NumeroPrestamo,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            var resultado = await _servicioCajero.PagarPrestamoAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // TRANSACCIÓN ENTRE CUENTAS DE TERCEROS 


         
        [HttpGet]
        public IActionResult TransaccionTerceros()
        {
            return View(new TransaccionTercerosCajeroViewModel());
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionTerceros(TransaccionTercerosCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            // Creamos el DTO
            var dto = new TransaccionTercerosCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            var resultado = await _servicioCajero.TransaccionEntreTercerosAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

    }
}