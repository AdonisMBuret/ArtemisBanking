using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones con comercios
    /// </summary>
    public interface IRepositorioComercio : IRepositorioGenerico<Comercio>
    {
        /// <summary>
        /// Obtiene un comercio por su RNC
        /// </summary>
        Task<Comercio?> ObtenerPorRNCAsync(string rnc);

        /// <summary>
        /// Obtiene todos los comercios activos
        /// </summary>
        Task<IEnumerable<Comercio>> ObtenerActivosAsync();

        /// <summary>
        /// Obtiene todos los comercios paginados
        /// </summary>
        Task<(IEnumerable<Comercio> Comercios, int TotalRegistros)> ObtenerPaginadoAsync(int pagina, int registrosPorPagina);

        /// <summary>
        /// Obtiene un comercio con su usuario asociado
        /// </summary>
        Task<Comercio?> ObtenerConUsuarioAsync(int id);

        /// <summary>
        /// Verifica si un comercio ya tiene un usuario asociado
        /// </summary>
        Task<bool> TieneUsuarioAsociadoAsync(int comercioId);

        /// <summary>
        /// Obtiene un comercio por el ID del usuario asociado
        /// </summary>
        Task<Comercio?> ObtenerPorUsuarioIdAsync(string usuarioId);
    }
}
