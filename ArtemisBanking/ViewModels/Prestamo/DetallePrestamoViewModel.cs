using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Prestamo
{
    /// <summary>
    /// ViewModel para ver el detalle del préstamo (tabla de amortización)
    /// </summary>
    public class DetallePrestamoViewModel
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public string NombreCliente { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public int PlazoMeses { get; set; }
        public decimal CuotaMensual { get; set; }
        public bool EstaActivo { get; set; }

        public IEnumerable<CuotaPrestamoViewModel> TablaAmortizacion { get; set; }
    }

    public class CuotaPrestamoViewModel
    {
        public int Id { get; set; }
        public string FechaPago { get; set; }
        public decimal MontoCuota { get; set; }
        public bool EstaPagada { get; set; }
        public bool EstaAtrasada { get; set; }
    }
}