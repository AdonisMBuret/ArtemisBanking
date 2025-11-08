using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class TarjetaCreditoDTO
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public string UltimosCuatroDigitos { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; }
        public bool EstaActiva { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoCliente { get; set; }
        public string CedulaCliente { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
