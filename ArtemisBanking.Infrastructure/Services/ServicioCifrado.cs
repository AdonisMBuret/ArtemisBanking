using ArtemisBanking.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBanking.Infrastructure.Services
{
    /// <summary>
    /// Servicio para cifrado de datos sensibles usando SHA-256
    /// </summary>
    public class ServicioCifrado : IServicioCifrado
    {
        /// <summary>
        /// Cifra el CVC de la tarjeta usando SHA-256
        /// </summary>
        public string CifrarCVC(string cvc)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(cvc);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifica si un CVC coincide con su versión cifrada
        /// </summary>
        public bool VerificarCVC(string cvc, string cvcCifrado)
        {
            var cvcCifradoIngresado = CifrarCVC(cvc);
            return cvcCifradoIngresado == cvcCifrado;
        }
    }
}