using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioUsuario
    {
        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// Si es cliente, crea también su cuenta de ahorro principal
        /// </summary>
        Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos);

        /// <summary>
        /// Actualiza los datos de un usuario existente
        /// Si es cliente y hay monto adicional, lo suma a la cuenta principal
        /// </summary>
        Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos);

        /// <summary>
        /// Activa o desactiva un usuario
        /// El administrador no puede cambiar su propio estado
        /// </summary>
        Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId);

        /// <summary>
        /// Obtiene los datos del dashboard del administrador
        /// Incluye indicadores de transacciones, clientes y productos
        /// </summary>
        Task<ResultadoOperacion<DashboardAdminDTO>> ObtenerDashboardAdminAsync();

        /// <summary>
        /// Obtiene los datos del dashboard del cajero
        /// Incluye indicadores del día actual
        /// </summary>
    }
}
