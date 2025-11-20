using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IServicioJwt _servicioJwt;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IServicioJwt servicioJwt,
            IServicioCorreo servicioCorreo,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _servicioJwt = servicioJwt;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }

    
        /// Autentica un usuario y devuelve un token JWT

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _userManager.FindByNameAsync(request.UserName);
            if (usuario == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            if (!usuario.EstaActivo)
            {
                return Unauthorized(new { message = "La cuenta está inactiva. Por favor active su cuenta mediante el enlace enviado a su correo electrónico." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(usuario, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            var token = await _servicioJwt.GenerarTokenAsync(usuario);

            return Ok(new LoginResponseDTO { Jwt = token });
        }

    
        /// Confirma la cuenta de un usuario mediante un token

        [HttpPost("confirm")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmarCuenta([FromBody] ConfirmarCuentaRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var decodedToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Token));
                var parts = decodedToken.Split('|');
                
                if (parts.Length != 2)
                {
                    return BadRequest(new { message = "Token inválido" });
                }

                var userId = parts[0];
                var token = parts[1];

                var usuario = await _userManager.FindByIdAsync(userId);
                if (usuario == null)
                {
                    return BadRequest(new { message = "Token inválido" });
                }

                var result = await _userManager.ConfirmEmailAsync(usuario, token);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Token inválido o expirado" });
                }

                usuario.EstaActivo = true;
                await _userManager.UpdateAsync(usuario);

                return NoContent();
            }
            catch
            {
                return BadRequest(new { message = "Token inválido" });
            }
        }

    
        /// Solicita un token para resetear la contraseña
    
        [HttpPost("get-reset-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SolicitarResetToken([FromBody] SolicitarResetTokenRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _userManager.FindByNameAsync(request.UserName);
            if (usuario == null)
            {
                return BadRequest(new { message = "El nombre de usuario no existe" });
            }

            usuario.EstaActivo = false;
            await _userManager.UpdateAsync(usuario);

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            var asunto = "Reseteo de contraseña - Artemis Banking";
            var cuerpo = $@"
                <h2>Reseteo de Contraseña</h2>
                <p>Hola {usuario.NombreCompleto},</p>
                <p>Has solicitado resetear tu contraseña. Utiliza el siguiente token para completar el proceso:</p>
                <p><strong>{token}</strong></p>
                <p>Este token expirará en 24 horas.</p>
                <p>Si no solicitaste este cambio, ignora este correo.</p>
            ";

            await _servicioCorreo.EnviarCorreoGenericoAsync(usuario.Email!, asunto, cuerpo);

            return NoContent();
        }

    
        /// Resetea la contraseña de un usuario

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _userManager.FindByIdAsync(request.UserId);
            if (usuario == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            var result = await _userManager.ResetPasswordAsync(usuario, request.Token, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Token inválido o contraseña no cumple con los requisitos", errors = result.Errors });
            }

            usuario.EstaActivo = true;
            await _userManager.UpdateAsync(usuario);

            return NoContent();
        }
    }
}
