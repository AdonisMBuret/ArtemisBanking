using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{

    /// API Controller para gestión de usuarios

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SoloAdministrador")]
    public class UsersController : ControllerBase
    {
        private readonly IServicioUsuario _servicioUsuario;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IServicioUsuario servicioUsuario,
            ILogger<UsersController> logger)
        {
            _servicioUsuario = servicioUsuario;
            _logger = logger;
        }

    
        /// Obtiene un listado paginado de usuarios (excluyendo usuarios con rol "Comercio")
    
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDTO<UsuarioApiResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObtenerUsuarios(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? rol = null)
        {
            try
            {
                var resultado = await _servicioUsuario.ObtenerUsuariosPaginadosParaApiAsync(
                    page,
                    pageSize,
                    rol,
                    excludeComercio: true);

                if (!resultado.Exito)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return Ok(resultado.Datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Obtiene un listado paginado de usuarios con rol "Comercio"
    
        [HttpGet("commerce")]
        [ProducesResponseType(typeof(PaginatedResponseDTO<UsuarioApiResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerUsuariosComercio(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var resultado = await _servicioUsuario.ObtenerUsuariosComercioParaApiAsync(
                    page,
                    pageSize);

                if (!resultado.Exito)
                {
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return Ok(resultado.Datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios comercio");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Crea un nuevo usuario en el sistema
    
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioApiResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var dto = new CrearUsuarioDTO
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Cedula = request.Cedula,
                    Correo = request.Email,
                    NombreUsuario = request.UserName,
                    Contrasena = request.Password,
                    TipoUsuario = request.TipoUsuario,
                    MontoInicial = request.MontoInicial ?? 0
                };

                var resultado = await _servicioUsuario.CrearUsuarioParaApiAsync(dto);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("ya está en uso") || resultado.Mensaje.Contains("ya está registrado"))
                    {
                        return Conflict(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = resultado.Datos.Id }, resultado.Datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Crea un nuevo usuario con rol "Comercio" asociado a un comercio específico
    
        [HttpPost("commerce/{commerceId}")]
        [ProducesResponseType(typeof(UsuarioApiResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CrearUsuarioComercio(
            int commerceId,
            [FromBody] CrearUsuarioComercioRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // ? El servicio maneja la lógica de comercio
                var resultado = await _servicioUsuario.CrearUsuarioComercioParaApiAsync(
                    commerceId,
                    request.Nombre,
                    request.Apellido,
                    request.Cedula,
                    request.Email,
                    request.UserName,
                    request.Password);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("ya está en uso") || resultado.Mensaje.Contains("ya existe"))
                    {
                        return Conflict(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = resultado.Datos.Id }, resultado.Datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario comercio");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Actualiza los datos de un usuario existente
    
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ActualizarUsuario(string id, [FromBody] ActualizarUsuarioRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var dto = new ActualizarUsuarioDTO
                {
                    UsuarioId = id,
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Cedula = request.Cedula,
                    Correo = request.Email,
                    NombreUsuario = request.UserName,
                    NuevaContrasena = request.Password,
                    MontoAdicional = request.MontoAdicional ?? 0
                };

                var resultado = await _servicioUsuario.ActualizarUsuarioAsync(dto);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("no encontrado"))
                    {
                        return NotFound(new { message = resultado.Mensaje });
                    }
                    if (resultado.Mensaje.Contains("ya está en uso") || resultado.Mensaje.Contains("ya está registrado"))
                    {
                        return Conflict(new { message = resultado.Mensaje });
                    }
                    return BadRequest(new { message = resultado.Mensaje });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Cambia el estado de un usuario (activa o desactiva)
    
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstadoUsuario(string id, [FromBody] CambiarEstadoRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var resultado = await _servicioUsuario.CambiarEstadoAsync(id, usuarioActualId);

                if (!resultado.Exito)
                {
                    if (resultado.Mensaje.Contains("no puede cambiar su propio estado"))
                    {
                        return Forbid();
                    }
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
                _logger.LogError(ex, "Error al cambiar estado de usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

    
        /// Obtiene los detalles de un usuario específico
    
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioApiResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerUsuarioPorId(string id)
        {
            try
            {
                var resultado = await _servicioUsuario.ObtenerUsuarioPorIdParaApiAsync(id);

                if (!resultado.Exito)
                {
                    return NotFound(new { message = resultado.Mensaje });
                }

                return Ok(resultado.Datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
