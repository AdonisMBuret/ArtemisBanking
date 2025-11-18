using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Domain.Entities
{
    public class Usuario : IdentityUser
    {
        // Datos personales del usuario
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }

        // Fecha de cuando se creó el usuario
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Indica si el usuario está activo o inactivo
        public bool EstaActivo { get; set; } = false; // Por defecto inactivo hasta confirmar correo

        // Relación opcional con comercio (solo para usuarios con rol comercio)
        public int? ComercioId { get; set; }
        public virtual Comercio? Comercio { get; set; }

        // Relación: Un usuario puede tener muchas cuentas de ahorro
        public virtual ICollection<CuentaAhorro> CuentasAhorro { get; set; }

        // Relación: Un usuario puede tener muchos préstamos
        public virtual ICollection<Prestamo> Prestamos { get; set; }

        // Relación: Un usuario puede tener muchas tarjetas de crédito
        public virtual ICollection<TarjetaCredito> TarjetasCredito { get; set; }

        // Relación: Un usuario puede tener muchos beneficiarios registrados
        public virtual ICollection<Beneficiario> Beneficiarios { get; set; }

        // Constructor para inicializar las colecciones
        public Usuario()
        {
            CuentasAhorro = new List<CuentaAhorro>();
            Prestamos = new List<Prestamo>();
            TarjetasCredito = new List<TarjetaCredito>();
            Beneficiarios = new List<Beneficiario>();
        }

        // Propiedad calculada para obtener el nombre completo
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
