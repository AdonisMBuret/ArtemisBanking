namespace ArtemisBanking.Web.ViewModels
{
    /// <summary>
    /// ViewModel para el dashboard del administrador
    /// Muestra todos los indicadores del sistema
    /// </summary>
    public class DashboardAdminViewModel
    {
        public int TotalTransacciones { get; set; }
        public int TransaccionesDelDia { get; set; }
        public int TotalPagos { get; set; }
        public int PagosDelDia { get; set; }
        public int ClientesActivos { get; set; }
        public int ClientesInactivos { get; set; }
        public int TotalProductosFinancieros { get; set; }
        public int PrestamosVigentes { get; set; }
        public int TarjetasActivas { get; set; }
        public int CuentasAhorro { get; set; }
        public decimal DeudaPromedioCliente { get; set; }
    }
}