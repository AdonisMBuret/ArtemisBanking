
namespace ArtemisBanking.Application.DTOs
{
    public class ResultadoOperacion<T> : ResultadoOperacion
    {
        public T Datos { get; set; }

        public static ResultadoOperacion<T> Ok(T datos, string mensaje = "Operación exitosa")
            => new() { Exito = true, Mensaje = mensaje, Datos = datos };

        public new static ResultadoOperacion<T> Fallo(string mensaje)
            => new() { Exito = false, Mensaje = mensaje };
    }

}
