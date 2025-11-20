using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{

    /// API Controller para gestión de cuentas de ahorro

    [Route("api/savings-account")]
    [ApiController]
    [Authorize(Policy = "SoloAdministrador")]
    public class SavingsAccountController : ControllerBase
    {
        private readonly IServicioCuentaAhorro _servicioCuenta;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly ILogger<SavingsAccountController> _logger;

        public SavingsAccountController(
            IServicioCuentaAhorro servicioCuenta,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTransaccion repositorioTransaccion,
            ILogger<SavingsAccountController> logger)
        {
            _servicioCuenta = servicioCuenta;
            _repositorioCuenta = repositorioCuenta;
            _repositorioTransaccion = repositorioTransaccion;
            _logger = logger;
        }

    
        /// Obtiene un listado paginado de cuentas de ahorro
    
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDTO<CuentaAhorroApiResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerCuentas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? cedula = null,
            [FromQuery] bool? estado = null,
            [FromQuery] bool? tipo = null)
        {
            try
            {
                var (cuentas, total) = await _repositorioCuenta.ObtenerCuentasPaginadasAsync(
                    page,
                    pageSize,
                    cedula,
                    estado,
                    tipo);

                var cuentasDTO = cuentas.Select(c => new CuentaAhorroApiResponseDTO
                {
                    Id = c.Id,
                    NumeroCuenta = c.NumeroCuenta,
                    NombreCliente = c.Usuario.Nombre,
                    ApellidoCliente = c.Usuario.Apellido,
                    CedulaCliente = c.Usuario.Cedula,
                    Balance = c.Balance,
                    EsPrincipal = c.EsPrincipal,
                    TipoCuenta = c.EsPrincipal ? "Principal" : "Secundaria",
                    EstaActiva = c.EstaActiva,
                    FechaCreacion = c.FechaCreacion
                }).ToList();

                var response = new PaginatedResponseDTO<CuentaAhorroApiResponseDTO>
                {
                    Data = cuentasDTO,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Asigna una nueva cuenta de ahorro secundaria a un cliente
    
        [HttpPost]
        [ProducesResponseType(typeof(CuentaAhorroApiResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> AsignarCuentaSecundaria([FromBody] AsignarCuentaRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var dto = new CrearCuentaSecundariaDTO
                {
                    ClienteId = request.ClienteId,
                    AdministradorId = adminId,
                    BalanceInicial = request.BalanceInicial
                };

                var resultado = await _servicioCuenta.CrearCuentaSecundariaAsync(dto);

                if (!resultado.Exito)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(resultado.Datos.Id);

                var response = new CuentaAhorroApiResponseDTO
                {
                    Id = cuenta.Id,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    NombreCliente = cuenta.Usuario.Nombre,
                    ApellidoCliente = cuenta.Usuario.Apellido,
                    CedulaCliente = cuenta.Usuario.Cedula,
                    Balance = cuenta.Balance,
                    EsPrincipal = cuenta.EsPrincipal,
                    TipoCuenta = cuenta.EsPrincipal ? "Principal" : "Secundaria",
                    EstaActiva = cuenta.EstaActiva,
                    FechaCreacion = cuenta.FechaCreacion
                };

                return CreatedAtAction(nameof(ObtenerTransaccionesDeCuenta), new { accountNumber = cuenta.NumeroCuenta }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar cuenta");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Obtiene las transacciones de una cuenta por su número de cuenta
    
        [HttpGet("{accountNumber}/transactions")]
        [ProducesResponseType(typeof(DetalleCuentaApiResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTransaccionesDeCuenta(string accountNumber)
        {
            try
            {
                var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(accountNumber);

                if (cuenta == null)
                {
                    return NotFound(new { message = "Cuenta no encontrada" });
                }

                var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(cuenta.Id);

                var response = new DetalleCuentaApiResponseDTO
                {
                    Id = cuenta.Id,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    NombreCliente = cuenta.Usuario.Nombre,
                    ApellidoCliente = cuenta.Usuario.Apellido,
                    Balance = cuenta.Balance,
                    EsPrincipal = cuenta.EsPrincipal,
                    EstaActiva = cuenta.EstaActiva,
                    Transacciones = transacciones.Select(t => new TransaccionApiDTO
                    {
                        FechaTransaccion = t.FechaTransaccion,
                        Monto = t.Monto,
                        TipoTransaccion = t.TipoTransaccion,
                        Beneficiario = t.Beneficiario,
                        Origen = t.Origen,
                        EstadoTransaccion = t.EstadoTransaccion
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener transacciones de cuenta {accountNumber}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
