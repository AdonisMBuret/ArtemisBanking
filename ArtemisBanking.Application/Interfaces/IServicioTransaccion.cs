using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
        
    public interface IServicioTransaccion
    {
        // MÉTODOS PRIVADOS (USADOS INTERNAMENTE) 
         
        Task<(bool exito, string mensaje)> RealizarTransferenciaAsync(
            int cuentaOrigenId,
            string numeroCuentaDestino,
            decimal monto);
         
        Task<(bool exito, string mensaje, decimal montoPagado)> PagarTarjetaCreditoAsync(
            int cuentaOrigenId,
            int tarjetaId,
            decimal monto);

        Task<(bool exito, string mensaje, decimal montoAplicado)> PagarPrestamoAsync(
            int cuentaOrigenId,
            int prestamoId,
            decimal monto);
         
        Task<(bool exito, string mensaje)> RealizarAvanceEfectivoAsync(
            int tarjetaId,
            int cuentaDestinoId,
            decimal monto);
         
        Task<bool> TieneFondosSuficientesAsync(int cuentaId, decimal monto);
         
        Task<decimal> CalcularCreditoDisponibleAsync(int tarjetaId);

        // MÉTODOS PÚBLICOS (USADOS POR CONTROLADORES) 
         
        Task<ResultadoOperacion> RealizarTransaccionExpressAsync(TransaccionExpressDTO datos);
         
        Task<ResultadoOperacion> PagarTarjetaCreditoClienteAsync(PagoTarjetaClienteDTO datos);

        Task<ResultadoOperacion> PagarPrestamoClienteAsync(PagoPrestamoClienteDTO datos);

        Task<ResultadoOperacion> PagarBeneficiarioAsync(PagoBeneficiarioDTO datos);

        Task<ResultadoOperacion> RealizarAvanceEfectivoClienteAsync(AvanceEfectivoDTO datos);

        Task<ResultadoOperacion<(string nombre, string apellido)>> ObtenerInfoCuentaDestinoAsync(string numeroCuenta);
    }
}