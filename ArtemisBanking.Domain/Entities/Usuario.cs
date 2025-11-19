using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Domain.Entities
{
    public class Usuario : IdentityUser
    {
        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Cedula { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool EstaActivo { get; set; } = false; 

        public int? ComercioId { get; set; }

        public virtual Comercio? Comercio { get; set; }

        public virtual ICollection<CuentaAhorro> CuentasAhorro { get; set; }

        public virtual ICollection<Prestamo> Prestamos { get; set; }

        public virtual ICollection<TarjetaCredito> TarjetasCredito { get; set; }

        public virtual ICollection<Beneficiario> Beneficiarios { get; set; }

        public Usuario()
        {
            CuentasAhorro = new List<CuentaAhorro>();
            Prestamos = new List<Prestamo>();
            TarjetasCredito = new List<TarjetaCredito>();
            Beneficiarios = new List<Beneficiario>();
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
