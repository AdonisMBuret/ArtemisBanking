using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{

    public interface IRepositorioComercio : IRepositorioGenerico<Comercio>
    {
        Task<Comercio?> ObtenerPorRNCAsync(string rnc);

        Task<IEnumerable<Comercio>> ObtenerActivosAsync();

        Task<(IEnumerable<Comercio> Comercios, int TotalRegistros)> ObtenerPaginadoAsync(int pagina, int registrosPorPagina);
   
        Task<Comercio?> ObtenerConUsuarioAsync(int id);
   
        Task<bool> TieneUsuarioAsociadoAsync(int comercioId);

        Task<Comercio?> ObtenerPorUsuarioIdAsync(string usuarioId);
    }
}
