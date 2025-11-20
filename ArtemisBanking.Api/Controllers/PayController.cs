using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.Api.Controllers
{
    [Route("pay")]
    [ApiController]
    [Authorize(Policy = "AdminOComercio")]
    public class PayController : ControllerBase
    {
        private readonly IServicioProcesadorPagos _servicioProcesador;
        private readonly ILogger<PayController> _logger;

        public PayController(
            IServicioProcesadorPagos servicioProcesador,
            ILogger<PayController> logger)
        {
            _servicioProcesador = servicioProcesador;
            _logger = logger;
        }

    
        /// Obtiene las transacciones de un comercio
    
        [HttpGet("get-transactions/{commerceId?}")]
        [ProducesResponseType(typeof(PaginatedResponseDTO<TransaccionComercioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTransactions(
            int? commerceId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                int comercioIdFinal = ObtenerComercioId(commerceId);

                if (comercioIdFinal == 0)
                {
                    return BadRequest(new { message = "Debe especificar el ID del comercio" });
                }

                var resultado = await _servicioProcesador.ObtenerTransaccionesComercioAsync(
                    comercioIdFinal,
                    page,
                    pageSize);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones del comercio");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Procesa un pago desde un comercio
    
        [HttpPost("process-payment/{commerceId?}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ProcessPayment(
            int? commerceId,
            [FromBody] ProcesarPagoRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int comercioIdFinal = ObtenerComercioId(commerceId);

                if (comercioIdFinal == 0)
                {
                    return BadRequest(new { message = "Debe especificar el ID del comercio" });
                }

                var resultado = await _servicioProcesador.ProcesarPagoAsync(comercioIdFinal, request);

                if (!resultado.Exito)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Método auxiliar para obtener el ID del comercio según el rol del usuario autenticado

        private int ObtenerComercioId(int? commerceIdFromUrl)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Comercio")
            {
                var comercioIdClaim = User.FindFirst("ComercioId")?.Value;
                
                if (int.TryParse(comercioIdClaim, out int comercioId))
                {
                    return comercioId;
                }
                
                return 0; 
            }
            else if (userRole == "Administrador")
            {
                return commerceIdFromUrl ?? 0;
            }

            return 0; 
        }
    }
}