using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class AsignarPrestamoDTO
    {
        [Required]
        public string ClienteId { get; set; }

        [Required]
        public string AdministradorId { get; set; }

        [Required]
        [Range(100, double.MaxValue)]
        public decimal MontoCapital { get; set; }

        [Required]
        public int PlazoMeses { get; set; }

        [Required]
        [Range(0.01, 100)]
        public decimal TasaInteresAnual { get; set; }
    }
}
