using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioAutenticacion
    {
        // LOGIN 
                        
        Task<ResultadoOperacion<string>> LoginAsync(string nombreUsuario, string contrasena, bool recordarme);

         
        Task<ResultadoOperacion> LogoutAsync();

        // CONFIRMACIÓN DE CUENTA 
                 
        Task<ResultadoOperacion> ConfirmarCuentaAsync(string usuarioId, string token);

        // RESETEO DE CONTRASEÑA 
                 
        Task<ResultadoOperacion<string>> SolicitarReseteoContrasenaAsync(string nombreUsuario);

          
        Task<ResultadoOperacion> RestablecerContrasenaAsync(string usuarioId, string token, string nuevaContrasena);

        // UTILIDADES 
                 
        Task<string> ObtenerRolUsuarioAsync(string usuarioId);

        Task<bool> EstaAutenticadoAsync(string usuarioId);
    }
}