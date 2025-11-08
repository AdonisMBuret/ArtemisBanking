using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class BeneficiarioDTO
    {
        public int Id { get; set; }
        public string NumeroCuentaBeneficiario { get; set; }
        public string NombreBeneficiario { get; set; }
        public string ApellidoBeneficiario { get; set; }
    }
}
