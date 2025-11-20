
namespace ArtemisBanking.Application.DTOs
{
    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public List<string> Errores { get; set; } = new();

        public static ResultadoOperacion Ok(string mensaje = "Operación exitosa")
            => new() { Exito = true, Mensaje = mensaje };

        public static ResultadoOperacion Fallo(string mensaje)
            => new() { Exito = false, Mensaje = mensaje };

        public static ResultadoOperacion FalloConErrores(string mensaje, List<string> errores)
            => new() { Exito = false, Mensaje = mensaje, Errores = errores };
    }
}
