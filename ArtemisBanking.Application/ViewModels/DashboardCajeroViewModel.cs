using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    // ==================== DASHBOARD DEL CAJERO ====================

    /// <summary>
    /// ViewModel para el dashboard del cajero
    /// Muestra las estadísticas del día actual
    /// </summary>
    public class DashboardCajeroViewModel
    {
        [Display(Name = "Transacciones del Día")]
        public int TransaccionesDelDia { get; set; }

        [Display(Name = "Pagos del Día")]
        public int PagosDelDia { get; set; }

        [Display(Name = "Depósitos del Día")]
        public int DepositosDelDia { get; set; }

        [Display(Name = "Retiros del Día")]
        public int RetirosDelDia { get; set; }
    }
}