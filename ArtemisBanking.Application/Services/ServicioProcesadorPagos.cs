using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Application.Services
{
    public class ServicioProcesadorPagos : IServicioProcesadorPagos
    {
        private readonly IRepositorioComercio _repositorioComercio;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumo;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IServicioCifrado _servicioCifrado;
        private readonly IServicioCorreo _servicioCorreo;

        public ServicioProcesadorPagos(
            IRepositorioComercio repositorioComercio,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioConsumoTarjeta repositorioConsumo,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTransaccion repositorioTransaccion,
            IServicioCifrado servicioCifrado,
            IServicioCorreo servicioCorreo)
        {
            _repositorioComercio = repositorioComercio;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumo = repositorioConsumo;
            _repositorioCuenta = repositorioCuenta;
            _repositorioTransaccion = repositorioTransaccion;
            _servicioCifrado = servicioCifrado;
            _servicioCorreo = servicioCorreo;
        }

        public async Task<ResultadoOperacion> ProcesarPagoAsync(int comercioId, ProcesarPagoRequestDTO request)
        {
            var comercio = await _repositorioComercio.ObtenerConUsuarioAsync(comercioId);
            if (comercio == null || !comercio.EstaActivo)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El comercio no existe o está inactivo"
                };
            }

            if (comercio.Usuario == null)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El comercio no tiene un usuario asociado"
                };
            }

            var tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(request.CardNumber);
            if (tarjeta == null || !tarjeta.EstaActiva)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "La tarjeta no existe o está inactiva"
                };
            }

            var mesExpiracion = int.Parse(request.MonthExpirationCard);
            var anoExpiracion = int.Parse(request.YearExpirationCard);
            var fechaExpiracion = new DateTime(anoExpiracion, mesExpiracion, 1).AddMonths(1).AddDays(-1);

            if (fechaExpiracion < DateTime.Now)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "La tarjeta está vencida"
                };
            }

            var cvcHash = _servicioCifrado.CifrarCVC(request.CVC);
            if (tarjeta.CVC != cvcHash)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El CVC es incorrecto"
                };
            }

            var creditoDisponible = tarjeta.LimiteCredito - tarjeta.DeudaActual;
            if (request.TransactionAmount > creditoDisponible)
            {
                var consumoRechazado = new ConsumoTarjeta
                {
                    FechaConsumo = DateTime.Now,
                    Monto = request.TransactionAmount,
                    NombreComercio = comercio.Nombre,
                    EstadoConsumo = Constantes.ConsumoRechazado,
                    TarjetaId = tarjeta.Id,
                    ComercioId = comercio.Id
                };

                await _repositorioConsumo.AgregarAsync(consumoRechazado);
                await _repositorioConsumo.GuardarCambiosAsync();

                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "Fondos insuficientes. Límite disponible: " + creditoDisponible.ToString("C")
                };
            }

          
            tarjeta.DeudaActual += request.TransactionAmount;
            await _repositorioTarjeta.ActualizarAsync(tarjeta);

            var consumo = new ConsumoTarjeta
            {
                FechaConsumo = DateTime.Now,
                Monto = request.TransactionAmount,
                NombreComercio = comercio.Nombre,
                EstadoConsumo = Constantes.ConsumoAprobado,
                TarjetaId = tarjeta.Id,
                ComercioId = comercio.Id
            };

            await _repositorioConsumo.AgregarAsync(consumo);

            var cuentaComercio = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(comercio.UsuarioId!);
            if (cuentaComercio == null)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El comercio no tiene una cuenta de ahorro principal"
                };
            }

            cuentaComercio.Balance += request.TransactionAmount;
            await _repositorioCuenta.ActualizarAsync(cuentaComercio);

            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = request.TransactionAmount,
                TipoTransaccion = Constantes.TipoCredito,
                EstadoTransaccion = Constantes.EstadoAprobada,
                Origen = $"Pago con tarjeta ****{tarjeta.NumeroTarjeta.Substring(12)}",
                Beneficiario = cuentaComercio.NumeroCuenta,
                CuentaAhorroId = cuentaComercio.Id
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

    
            var asuntoCliente = $"Consumo realizado con la tarjeta ****{tarjeta.NumeroTarjeta.Substring(12)}";
            var cuerpoCliente = $@"
                <h2>Consumo Realizado</h2>
                <p>Estimado {tarjeta.Cliente.NombreCompleto},</p>
                <p>Se ha realizado un consumo con su tarjeta de crédito:</p>
                <ul>
                    <li><strong>Monto:</strong> {request.TransactionAmount:C}</li>
                    <li><strong>Tarjeta:</strong> ****{tarjeta.NumeroTarjeta.Substring(12)}</li>
                    <li><strong>Comercio:</strong> {comercio.Nombre}</li>
                    <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                </ul>
                <p>Gracias por usar Artemis Banking.</p>
            ";

            await _servicioCorreo.EnviarNotificacionConsumoTarjetaAsync(
                tarjeta.Cliente.Email!,
                tarjeta.Cliente.NombreCompleto,
                request.TransactionAmount,
                tarjeta.NumeroTarjeta.Substring(12),
                comercio.Nombre,
                DateTime.Now);

            var asuntoComercio = $"Pago recibido a través de tarjeta ****{tarjeta.NumeroTarjeta.Substring(12)}";
            var cuerpoComercio = $@"
                <h2>Pago Recibido</h2>
                <p>Estimado {comercio.Nombre},</p>
                <p>Ha recibido un pago:</p>
                <ul>
                    <li><strong>Monto:</strong> {request.TransactionAmount:C}</li>
                    <li><strong>Tarjeta:</strong> ****{tarjeta.NumeroTarjeta.Substring(12)}</li>
                    <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                </ul>
                <p>El monto ha sido acreditado a su cuenta.</p>
            ";

            await _servicioCorreo.EnviarNotificacionTransaccionRecibidaAsync(
                comercio.Usuario.Email!,
                comercio.Nombre,
                request.TransactionAmount,
                tarjeta.NumeroTarjeta.Substring(12),
                DateTime.Now);

            return new ResultadoOperacion
            {
                Exito = true,
                Mensaje = "Pago procesado exitosamente"
            };
        }

        public async Task<PaginatedResponseDTO<TransaccionComercioDTO>> ObtenerTransaccionesComercioAsync(
            int comercioId, 
            int page = 1, 
            int pageSize = 20)
        {
            var comercio = await _repositorioComercio.ObtenerConUsuarioAsync(comercioId);
            if (comercio == null || comercio.Usuario == null)
            {
                return new PaginatedResponseDTO<TransaccionComercioDTO>
                {
                    Data = new List<TransaccionComercioDTO>(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = 0,
                    TotalPages = 0
                };
            }

            var cuentaComercio = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(comercio.UsuarioId!);
            if (cuentaComercio == null)
            {
                return new PaginatedResponseDTO<TransaccionComercioDTO>
                {
                    Data = new List<TransaccionComercioDTO>(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = 0,
                    TotalPages = 0
                };
            }

            (IEnumerable<Transaccion> transacciones, int totalRegistros) = await _repositorioTransaccion.ObtenerPorCuentaPaginadoAsync(
                cuentaComercio.Id, 
                page, 
                pageSize);

            var transaccionesDTO = transacciones.Select(t => new TransaccionComercioDTO
            {
                Id = t.Id,
                Fecha = t.FechaTransaccion,
                Monto = t.Monto,
                TipoTransaccion = t.TipoTransaccion,
                Estado = t.EstadoTransaccion,
                Origen = t.Origen,
                Beneficiario = t.Beneficiario
            });

            return new PaginatedResponseDTO<TransaccionComercioDTO>
            {
                Data = transaccionesDTO,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRegistros,
                TotalPages = (int)Math.Ceiling(totalRegistros / (double)pageSize)
            };
        }
    }
}
