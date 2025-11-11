using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class AvanceEfectivoDTO
    {
        [Required]
        public int TarjetaId { get; set; }

        [Required]
        public int CuentaDestinoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public string UsuarioId { get; set; }
    }
}
