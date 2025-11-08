using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class ResultadoPaginadoDTO<T>
    {
        public IEnumerable<T> Datos { get; set; }
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamañoPagina);
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
