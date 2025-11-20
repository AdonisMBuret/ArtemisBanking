using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{

    /// API Controller para gestión de préstamos

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SoloAdministrador")]
    public class LoanController : ControllerBase
    {
        private readonly IServicioPrestamo _servicioPrestamo;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly ILogger<LoanController> _logger;

        public LoanController(
            IServicioPrestamo servicioPrestamo,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            ILogger<LoanController> logger)
        {
            _servicioPrestamo = servicioPrestamo;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _logger = logger;
        }

    
        /// Obtiene un listado paginado de préstamos
   
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDTO<PrestamoApiResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObtenerPrestamos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? cedula = null,
            [FromQuery] bool? estado = null)
        {
            try
            {
                var (prestamos, total) = await _repositorioPrestamo.ObtenerPrestamosPaginadosAsync(
                    page,
                    pageSize,
                    cedula,
                    estado);

                var prestamosDTO = prestamos.Select(p => new PrestamoApiResponseDTO
                {
                    Id = p.Id,
                    NumeroPrestamo = p.NumeroPrestamo,
                    NombreCliente = p.Cliente.Nombre,
                    ApellidoCliente = p.Cliente.Apellido,
                    CedulaCliente = p.Cliente.Cedula,
                    MontoCapital = p.MontoCapital,
                    TotalCuotas = p.PlazoMeses,
                    CuotasPagadas = p.CuotasPagadas,
                    MontoPendiente = p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota),
                    TasaInteresAnual = p.TasaInteresAnual,
                    PlazoMeses = p.PlazoMeses,
                    CuotaMensual = p.CuotaMensual,
                    EstaAlDia = p.EstaAlDia,
                    EstaActivo = p.EstaActivo,
                    FechaCreacion = p.FechaCreacion
                }).ToList();

                var response = new PaginatedResponseDTO<PrestamoApiResponseDTO>
                {
                    Data = prestamosDTO,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener préstamos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Asigna un nuevo préstamo a un cliente

        [HttpPost]
        [ProducesResponseType(typeof(PrestamoApiResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AsignarPrestamo([FromBody] AsignarPrestamoRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var dto = new AsignarPrestamoDTO
                {
                    ClienteId = request.ClienteId,
                    AdministradorId = adminId,
                    MontoCapital = request.MontoCapital,
                    PlazoMeses = request.PlazoMeses,
                    TasaInteresAnual = request.TasaInteresAnual
                };

                var resultado = await _servicioPrestamo.AsignarPrestamoAsync(dto);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("préstamo activo"))
                    {
                        return Conflict(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(resultado.Datos.Id);

                var response = new PrestamoApiResponseDTO
                {
                    Id = prestamo.Id,
                    NumeroPrestamo = prestamo.NumeroPrestamo,
                    NombreCliente = prestamo.Cliente.Nombre,
                    ApellidoCliente = prestamo.Cliente.Apellido,
                    CedulaCliente = prestamo.Cliente.Cedula,
                    MontoCapital = prestamo.MontoCapital,
                    TotalCuotas = prestamo.PlazoMeses,
                    CuotasPagadas = prestamo.CuotasPagadas,
                    MontoPendiente = prestamo.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota),
                    TasaInteresAnual = prestamo.TasaInteresAnual,
                    PlazoMeses = prestamo.PlazoMeses,
                    CuotaMensual = prestamo.CuotaMensual,
                    EstaAlDia = prestamo.EstaAlDia,
                    EstaActivo = prestamo.EstaActivo,
                    FechaCreacion = prestamo.FechaCreacion
                };

                return CreatedAtAction(nameof(ObtenerPrestamoPorId), new { id = prestamo.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar préstamo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Obtiene el detalle de un préstamo con su tabla de amortización

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DetallePrestamoApiResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObtenerPrestamoPorId(int id)
        {
            try
            {
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

                if (prestamo == null)
                {
                    return NotFound(new { message = "Préstamo no encontrado" });
                }

                var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(id);

                var response = new DetallePrestamoApiResponseDTO
                {
                    Id = prestamo.Id,
                    NumeroPrestamo = prestamo.NumeroPrestamo,
                    NombreCliente = prestamo.Cliente.Nombre,
                    ApellidoCliente = prestamo.Cliente.Apellido,
                    CedulaCliente = prestamo.Cliente.Cedula,
                    MontoCapital = prestamo.MontoCapital,
                    TasaInteresAnual = prestamo.TasaInteresAnual,
                    PlazoMeses = prestamo.PlazoMeses,
                    CuotaMensual = prestamo.CuotaMensual,
                    EstaActivo = prestamo.EstaActivo,
                    EstaAlDia = prestamo.EstaAlDia,
                    FechaCreacion = prestamo.FechaCreacion,
                    TablaAmortizacion = cuotas.Select(c => new CuotaPrestamoApiDTO
                    {
                        FechaPago = c.FechaPago,
                        MontoCuota = c.MontoCuota,
                        EstaPagada = c.EstaPagada,
                        EstaAtrasada = c.EstaAtrasada
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener préstamo {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Actualiza la tasa de interés de un préstamo activo
    
        [HttpPatch("{id}/rate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ActualizarTasaInteres(int id, [FromBody] ActualizarTasaRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var dto = new ActualizarTasaPrestamoDTO
                {
                    PrestamoId = id,
                    NuevaTasaInteres = request.NuevaTasaInteres
                };

                var resultado = await _servicioPrestamo.ActualizarTasaInteresAsync(dto);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("no encontrado"))
                    {
                        return NotFound(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar tasa del préstamo {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
