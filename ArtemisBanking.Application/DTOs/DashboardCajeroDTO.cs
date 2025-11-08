using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class DashboardCajeroDTO
    {
        public int TransaccionesDelDia { get; set; }
        public int PagosDelDia { get; set; }
        public int DepositosDelDia { get; set; }
        public int RetirosDelDia { get; set; }
    }
}
