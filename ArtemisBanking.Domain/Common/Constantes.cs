using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Common
{
    public static class Constantes
    {
        // Roles del sistema
        public const string RolAdministrador = "Administrador";
        public const string RolCajero = "Cajero";
        public const string RolCliente = "Cliente";

        // Plazos de préstamo permitidos (en meses)
        public static readonly int[] PlazosPermitidos = { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 };

        // Interés de avance de efectivo
        public const decimal InteresAvanceEfectivo = 6.25m;

        // Paginación
        public const int TamanoPaginaPorDefecto = 20;

        // Tipos de transacción
        public const string TipoDebito = "DÉBITO";
        public const string TipoCredito = "CRÉDITO";

        // Estados de transacción
        public const string EstadoAprobada = "APROBADA";
        public const string EstadoRechazada = "RECHAZADA";

        // Estados de consumo
        public const string ConsumoAprobado = "APROBADO";
        public const string ConsumoRechazado = "RECHAZADO";

        // Texto para avances de efectivo
        public const string TextoAvance = "AVANCE";
        public const string TextoDeposito = "DEPÓSITO";
        public const string TextoRetiro = "RETIRO";

        // Longitud de números
        public const int LongitudNumeroCuenta = 9;
        public const int LongitudNumeroTarjeta = 16;
        public const int LongitudCVC = 3;
    }
}
