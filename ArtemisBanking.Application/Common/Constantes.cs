namespace ArtemisBanking.Application.Common
{
    public static class Constantes
    {
        // ROLES DEL SISTEMA 
        public const string RolAdministrador = "Administrador";
        public const string RolCajero = "Cajero";
        public const string RolCliente = "Cliente";
        public const string RolComercio = "Comercio"; 

        // TIPOS DE TRANSACCIÓN 
        public const string TipoDebito = "DEBITO";
        public const string TipoCredito = "CREDITO";

        // ESTADOS DE TRANSACCIÓN 
        public const string EstadoAprobada = "APROBADA";
        public const string EstadoRechazada = "RECHAZADA";

        // ESTADOS DE CONSUMO 
        public const string ConsumoAprobado = "APROBADO";
        public const string ConsumoRechazado = "RECHAZADO";

        // TEXTOS ESPECIALES 
        public const string TextoDeposito = "DEPOSITO";
        public const string TextoRetiro = "RETIRO";
        public const string TextoAvance = "AVANCE";

        // CONFIGURACIONES 
        public const int TamanoPaginaPorDefecto = 20;
        public const decimal InteresAvanceEfectivo = 6.25m;
    }
}