namespace ArtemisBanking.Web.ViewModels
{
    /// <summary>
    /// ViewModel para el dashboard del cajero
    /// Muestra indicadores de las operaciones del día
    /// </summary>
    public class DashboardCajeroViewModel
    {
        public int TransaccionesDelDia { get; set; }
        public int PagosDelDia { get; set; }
        public int DepositosDelDia { get; set; }
        public int RetirosDelDia { get; set; }
    }
}