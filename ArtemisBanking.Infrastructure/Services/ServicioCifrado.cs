using ArtemisBanking.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBanking.Infrastructure.Services
{
     
    /// Servicio para cifrado de datos sensibles usando SHA-256
     
    public class ServicioCifrado : IServicioCifrado
    {
         
        /// Cifra el CVC de la tarjeta usando SHA-256
         
        public string CifrarCVC(string cvc)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(cvc);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

         
        /// Verifica si un CVC coincide con su versión cifrada
         
        public bool VerificarCVC(string cvc, string cvcCifrado)
        {
            var cvcCifradoIngresado = CifrarCVC(cvc);
            return cvcCifradoIngresado == cvcCifrado;
        }
    }
}