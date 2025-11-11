using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class PagoTarjetaCajeroDTO
    {
        [Required]
        public string NumeroCuentaOrigen { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public string NumeroTarjeta { get; set; }

        [Required]
        public string CajeroId { get; set; }
    }
}
