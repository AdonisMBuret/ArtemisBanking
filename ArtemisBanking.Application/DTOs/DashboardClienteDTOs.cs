
namespace ArtemisBanking.Application.DTOs
{
    public class DashboardClienteDTO
    {
        public IEnumerable<CuentaAhorroDTO> CuentasAhorro { get; set; }
        public IEnumerable<PrestamoDTO> Prestamos { get; set; }
        public IEnumerable<TarjetaCreditoDTO> TarjetasCredito { get; set; }
    }

}
