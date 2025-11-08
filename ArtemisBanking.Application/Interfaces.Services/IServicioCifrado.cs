using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IServicioCifrado
    {
        // Cifrar el CVC de la tarjeta con SHA-256
        string CifrarCVC(string cvc);

        // Verificar si un CVC coincide con el cifrado
        bool VerificarCVC(string cvc, string cvcCifrado);
    }
}
