using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class ActualizarLimiteTarjetaDTO
    {
        [Required]
        public int TarjetaId { get; set; }

        [Required]
        [Range(100, double.MaxValue)]
        public decimal NuevoLimite { get; set; }
    }
}
