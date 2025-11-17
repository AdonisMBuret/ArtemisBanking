using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    // ==================== DASHBOARD ADMIN ====================

    /// ViewModel para mostrar los indicadores en el dashboard del administrador
    public class DashboardAdminViewModel
    {
        [Display(Name = "Total de Transacciones")]
        public int TotalTransacciones { get; set; }

        [Display(Name = "Transacciones del Día")]
        public int TransaccionesDelDia { get; set; }

        [Display(Name = "Total de Pagos")]
        public int TotalPagos { get; set; }

        [Display(Name = "Pagos del Día")]
        public int PagosDelDia { get; set; }

        [Display(Name = "Clientes Activos")]
        public int ClientesActivos { get; set; }

        [Display(Name = "Clientes Inactivos")]
        public int ClientesInactivos { get; set; }

        [Display(Name = "Total de Productos Financieros")]
        public int TotalProductosFinancieros { get; set; }

        [Display(Name = "Préstamos Vigentes")]
        public int PrestamosVigentes { get; set; }

        [Display(Name = "Tarjetas Activas")]
        public int TarjetasActivas { get; set; }

        [Display(Name = "Cuentas de Ahorro")]
        public int CuentasAhorro { get; set; }

        [Display(Name = "Deuda Promedio por Cliente")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaPromedioCliente { get; set; }
    }
}