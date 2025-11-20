using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{
    /// API Controller para gestión de tarjetas de crédito

    [Route("api/credit-card")]
    [ApiController]
    [Authorize(Policy = "SoloAdministrador")]
    public class CreditCardController : ControllerBase
    {
        private readonly IServicioTarjetaCredito _servicioTarjeta;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly ILogger<CreditCardController> _logger;

        public CreditCardController(
            IServicioTarjetaCredito servicioTarjeta,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            ILogger<CreditCardController> logger)
        {
            _servicioTarjeta = servicioTarjeta;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _logger = logger;
        }

    
        /// Obtiene un listado paginado de tarjetas de crédito
    
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDTO<TarjetaCreditoApiResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTarjetas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? cedula = null,
            [FromQuery] bool? estado = null)
        {
            try
            {
                var (tarjetas, total) = await _repositorioTarjeta.ObtenerTarjetasPaginadasAsync(
                    page,
                    pageSize,
                    cedula,
                    estado);

                var tarjetasDTO = tarjetas.Select(t => new TarjetaCreditoApiResponseDTO
                {
                    Id = t.Id,
                    NumeroTarjeta = t.NumeroTarjeta,
                    NombreCliente = t.Cliente.Nombre,
                    ApellidoCliente = t.Cliente.Apellido,
                    CedulaCliente = t.Cliente.Cedula,
                    LimiteCredito = t.LimiteCredito,
                    DeudaActual = t.DeudaActual,
                    CreditoDisponible = t.CreditoDisponible,
                    FechaExpiracion = t.FechaExpiracion,
                    EstaActiva = t.EstaActiva,
                    FechaCreacion = t.FechaCreacion
                }).ToList();

                var response = new PaginatedResponseDTO<TarjetaCreditoApiResponseDTO>
                {
                    Data = tarjetasDTO,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tarjetas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Asigna una nueva tarjeta de crédito a un cliente
    
        [HttpPost]
        [ProducesResponseType(typeof(TarjetaCreditoApiResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> AsignarTarjeta([FromBody] AsignarTarjetaRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var dto = new AsignarTarjetaDTO
                {
                    ClienteId = request.ClienteId,
                    AdministradorId = adminId,
                    LimiteCredito = request.LimiteCredito
                };

                var resultado = await _servicioTarjeta.AsignarTarjetaAsync(dto);

                if (!resultado.Exito)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(resultado.Datos.Id);

                var response = new TarjetaCreditoApiResponseDTO
                {
                    Id = tarjeta.Id,
                    NumeroTarjeta = tarjeta.NumeroTarjeta,
                    NombreCliente = tarjeta.Cliente.Nombre,
                    ApellidoCliente = tarjeta.Cliente.Apellido,
                    CedulaCliente = tarjeta.Cliente.Cedula,
                    LimiteCredito = tarjeta.LimiteCredito,
                    DeudaActual = tarjeta.DeudaActual,
                    CreditoDisponible = tarjeta.CreditoDisponible,
                    FechaExpiracion = tarjeta.FechaExpiracion,
                    EstaActiva = tarjeta.EstaActiva,
                    FechaCreacion = tarjeta.FechaCreacion
                };

                return CreatedAtAction(nameof(ObtenerTarjetaPorId), new { id = tarjeta.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar tarjeta");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Obtiene el detalle de una tarjeta con sus consumos
    
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DetalleTarjetaApiResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTarjetaPorId(int id)
        {
            try
            {
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

                if (tarjeta == null)
                {
                    return NotFound(new { message = "Tarjeta no encontrada" });
                }

                var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(id);

                var response = new DetalleTarjetaApiResponseDTO
                {
                    Id = tarjeta.Id,
                    NumeroTarjeta = tarjeta.NumeroTarjeta,
                    NombreCliente = tarjeta.Cliente.Nombre,
                    ApellidoCliente = tarjeta.Cliente.Apellido,
                    LimiteCredito = tarjeta.LimiteCredito,
                    DeudaActual = tarjeta.DeudaActual,
                    CreditoDisponible = tarjeta.CreditoDisponible,
                    FechaExpiracion = tarjeta.FechaExpiracion,
                    EstaActiva = tarjeta.EstaActiva,
                    Consumos = consumos.Select(c => new ConsumoTarjetaApiDTO
                    {
                        FechaConsumo = c.FechaConsumo,
                        Monto = c.Monto,
                        NombreComercio = c.NombreComercio,
                        EstadoConsumo = c.EstadoConsumo
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tarjeta {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Actualiza el límite de crédito de una tarjeta
    
        [HttpPatch("{id}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ActualizarLimite(int id, [FromBody] ActualizarLimiteRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var dto = new ActualizarLimiteTarjetaDTO
                {
                    TarjetaId = id,
                    NuevoLimite = request.NuevoLimite
                };

                var resultado = await _servicioTarjeta.ActualizarLimiteAsync(dto);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("no encontrad"))
                    {
                        return NotFound(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar límite de tarjeta {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Cancela una tarjeta de crédito (solo si no tiene deuda)
    
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CancelarTarjeta(int id)
        {
            try
            {
                var resultado = await _servicioTarjeta.CancelarTarjetaAsync(id);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("no encontrad"))
                    {
                        return NotFound(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cancelar tarjeta {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
