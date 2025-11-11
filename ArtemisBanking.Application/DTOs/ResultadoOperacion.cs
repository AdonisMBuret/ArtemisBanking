using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.DTOs
{
    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public List<string> Errores { get; set; } = new();

        // Métodos estáticos para crear resultados comunes
        public static ResultadoOperacion Ok(string mensaje = "Operación exitosa")
            => new() { Exito = true, Mensaje = mensaje };

        public static ResultadoOperacion Fallo(string mensaje)
            => new() { Exito = false, Mensaje = mensaje };

        public static ResultadoOperacion FalloConErrores(string mensaje, List<string> errores)
            => new() { Exito = false, Mensaje = mensaje, Errores = errores };
    }
    public class ResultadoOperacion<T> : ResultadoOperacion
    {
        public T Datos { get; set; }

        public static ResultadoOperacion<T> Ok(T datos, string mensaje = "Operación exitosa")
            => new() { Exito = true, Mensaje = mensaje, Datos = datos };

        public new static ResultadoOperacion<T> Fallo(string mensaje)
            => new() { Exito = false, Mensaje = mensaje };
    }
}
