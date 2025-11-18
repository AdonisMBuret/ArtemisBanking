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
            // 1. Validar que el comercio existe y está activo
            var comercio = await _repositorioComercio.ObtenerConUsuarioAsync(comercioId);
            if (comercio == null || !comercio.EstaActivo)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El comercio no existe o está inactivo"
                };
            }

            // 2. Validar que el comercio tenga usuario asociado
            if (comercio.Usuario == null)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El comercio no tiene un usuario asociado"
                };
            }

            // 3. Buscar la tarjeta por número
            var tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(request.CardNumber);
            if (tarjeta == null || !tarjeta.EstaActiva)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "La tarjeta no existe o está inactiva"
                };
            }

            // 4. Validar fecha de expiración
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

            // 5. Validar CVC (comparar con el hash almacenado)
            var cvcHash = _servicioCifrado.CifrarCVC(request.CVC);
            if (tarjeta.CVC != cvcHash)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = "El CVC es incorrecto"
                };
            }

            // 6. Validar límite de crédito disponible
            var creditoDisponible = tarjeta.LimiteCredito - tarjeta.DeudaActual;
            if (request.TransactionAmount > creditoDisponible)
            {
                // Registrar consumo RECHAZADO
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

            // 7. Procesar el pago
            // 7.1 Aumentar la deuda de la tarjeta
            tarjeta.DeudaActual += request.TransactionAmount;
            await _repositorioTarjeta.ActualizarAsync(tarjeta);

            // 7.2 Registrar el consumo APROBADO
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

            // 7.3 Acreditar el monto a la cuenta del comercio
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

            // 7.4 Registrar la transacción en la cuenta del comercio
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

            // Recargar tarjeta con cliente para obtener la información completa
            tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

            // 8. Enviar correos
            // 8.1 Correo al cliente
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

            // 8.2 Correo al comercio (usando método genérico ya que no hay uno específico)
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

            // Como no hay método específico para comercio, usamos el de transacción recibida
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
            // Obtener el comercio con su usuario
            var comercio = await _repositorioComercio.ObtenerConUsuarioAsync(comercioId);
            if (comercio == null || comercio.Usuario == null)
            {
                return new PaginatedResponseDTO<TransaccionComercioDTO>
                {
                    Data = new List<TransaccionComercioDTO>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = 0
                };
            }

            // Obtener la cuenta principal del comercio
            var cuentaComercio = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(comercio.UsuarioId!);
            if (cuentaComercio == null)
            {
                return new PaginatedResponseDTO<TransaccionComercioDTO>
                {
                    Data = new List<TransaccionComercioDTO>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = 0
                };
            }

            // Obtener transacciones paginadas
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
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRegistros
            };
        }
    }
}
