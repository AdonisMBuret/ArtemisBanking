using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class DashboardAdminDTO
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
