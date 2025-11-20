using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{
    [Route("api/commerce")]
    [ApiController]
    [Authorize(Policy = "SoloAdministrador")]
    public class CommerceController : ControllerBase
    {
        private readonly IServicioComercio _servicioComercio;
        private readonly ILogger<CommerceController> _logger;

        public CommerceController(
            IServicioComercio servicioComercio,
            ILogger<CommerceController> logger)
        {
            _servicioComercio = servicioComercio;
            _logger = logger;
        }

    
        /// Obtiene todos los comercios paginados o todos si no se especifican parámetros
    
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDTO<ComercioResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<ComercioResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            try
            {
                if (!page.HasValue || !pageSize.HasValue)
                {
                    var comercios = await _servicioComercio.ObtenerComerciosActivosAsync();
                    return Ok(comercios);
                }

                var resultado = await _servicioComercio.ObtenerComerciosPaginadosAsync(page.Value, pageSize.Value);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comercios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Obtiene un comercio por su ID
    
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ComercioResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var comercio = await _servicioComercio.ObtenerComercioPorIdAsync(id);
                
                if (comercio == null)
                {
                    return NotFound(new { message = "Comercio no encontrado" });
                }

                return Ok(comercio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comercio {ComercioId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Crea un nuevo comercio
    
        [HttpPost]
        [ProducesResponseType(typeof(ComercioResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CrearComercioRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var comercio = await _servicioComercio.CrearComercioAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = comercio.Id }, comercio);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comercio");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Actualiza un comercio existente
    
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarComercioRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var actualizado = await _servicioComercio.ActualizarComercioAsync(id, request);
                
                if (!actualizado)
                {
                    return NotFound(new { message = "Comercio no encontrado" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comercio {ComercioId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Cambia el estado de un comercio (activa/desactiva)
    
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] CambiarEstadoComercioRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var actualizado = await _servicioComercio.CambiarEstadoComercioAsync(id, request.Status);
                
                if (!actualizado)
                {
                    return NotFound(new { message = "Comercio no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del comercio {ComercioId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
