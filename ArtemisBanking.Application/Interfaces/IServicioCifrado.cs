
namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCifrado
    {
        string CifrarCVC(string cvc);
        bool VerificarCVC(string cvc, string cvcCifrado);
    }
}