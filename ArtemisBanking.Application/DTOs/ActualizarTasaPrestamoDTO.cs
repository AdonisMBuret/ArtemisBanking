using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class ActualizarTasaPrestamoDTO
    {
        [Required]
        public int PrestamoId { get; set; }

        [Required]
        [Range(0.01, 100)]
        public decimal NuevaTasaInteres { get; set; }
    }
}
