namespace ArtemisBanking.Application.DTOs
{
    /// <summary>
    /// DTO para el dashboard del cliente
    /// Contiene todos los productos financieros
    /// </summary>
    public class DashboardClienteDTO
    {
        public IEnumerable<CuentaAhorroDTO> CuentasAhorro { get; set; }
        public IEnumerable<PrestamoDTO> Prestamos { get; set; }
        public IEnumerable<TarjetaCreditoDTO> TarjetasCredito { get; set; }
    }

    /// <summary>
    /// DTO para el detalle de cuenta del cliente
    /// </summary>
    public class DetalleCuentaClienteDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public string TipoCuenta { get; set; }
        public IEnumerable<TransaccionDTO> Transacciones { get; set; }
    }

    /// <summary>
    /// DTO para el detalle de préstamo del cliente
    /// </summary>
    public class DetallePrestamoClienteDTO
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public IEnumerable<CuotaPrestamoDTO> TablaAmortizacion { get; set; }
    }

    /// <summary>
    /// DTO para el detalle de tarjeta del cliente
    /// </summary>
    public class DetalleTarjetaClienteDTO
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; }
        public IEnumerable<ConsumoTarjetaDTO> Consumos { get; set; }
    }
}
