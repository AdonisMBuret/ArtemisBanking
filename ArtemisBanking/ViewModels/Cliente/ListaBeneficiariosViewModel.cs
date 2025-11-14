
namespace ArtemisBanking.ViewModels.Cliente
{
    /// ==================== BENEFICIARIOS ====================
    
    /// <summary>
    /// ViewModel para el listado de beneficiarios del cliente
    /// </summary>
    public class ListaBeneficiariosViewModel
    {
        public IEnumerable<BeneficiarioItemViewModel> Beneficiarios { get; set; }
    }
}
