
namespace ArtemisBanking.Application.Common
{
    /// <summary>
    /// Clase que contiene todas las constantes usadas en el sistema
    /// Esto nos ayuda a no repetir valores en todo el código
    /// </summary>
    public static class Constantes
    {
        // ==================== ROLES DEL SISTEMA ====================
        // Estos son los 3 tipos de usuarios que puede haber
        public const string RolAdministrador = "Administrador";
        public const string RolCajero = "Cajero";
        public const string RolCliente = "Cliente";

        // ==================== TIPOS DE TRANSACCIÓN ====================
        // Cuando el dinero SALE de una cuenta
        public const string TipoDebito = "DEBITO";
        // Cuando el dinero ENTRA a una cuenta
        public const string TipoCredito = "CREDITO";

        // ==================== ESTADOS DE TRANSACCIÓN ====================
        // Cuando la transacción se hizo correctamente
        public const string EstadoAprobada = "APROBADA";
        // Cuando la transacción fue rechazada (sin fondos, etc.)
        public const string EstadoRechazada = "RECHAZADA";

        // ==================== ESTADOS DE CONSUMO ====================
        // Cuando el consumo en tarjeta fue exitoso
        public const string ConsumoAprobado = "APROBADO";
        // Cuando el consumo fue rechazado (sin crédito disponible)
        public const string ConsumoRechazado = "RECHAZADO";

        // ==================== TEXTOS ESPECIALES ====================
        // Cuando se hace un depósito por cajero
        public const string TextoDeposito = "DEPOSITO";
        // Cuando se hace un retiro por cajero
        public const string TextoRetiro = "RETIRO";
        // Cuando se hace un avance de efectivo desde tarjeta
        public const string TextoAvance = "AVANCE";

        // ==================== CONFIGURACIONES ====================
        // Cuántos registros se muestran por página en los listados
        public const int TamanoPaginaPorDefecto = 20;
        
        // Interés que se cobra en avances de efectivo (6.25%)
        public const decimal InteresAvanceEfectivo = 6.25m;
    }
}