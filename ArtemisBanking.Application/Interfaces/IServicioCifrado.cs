namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de cifrado
    /// Define los métodos para cifrar y verificar datos sensibles (como el CVC de las tarjetas)
    /// </summary>
    public interface IServicioCifrado
    {
        // Cifrar el CVC de la tarjeta con SHA-256
        string CifrarCVC(string cvc);

        // Verificar si un CVC coincide con el cifrado
        bool VerificarCVC(string cvc, string cvcCifrado);
    }
}