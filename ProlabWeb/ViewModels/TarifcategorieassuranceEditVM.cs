namespace ProlabWeb.ViewModels
{
    public class TarifcategorieassuranceEditVM
    {
        public string Codeassurance { get; set; } = null!;

        public int? Idlaboratoire { get; set; }

        public List<TarifCategorieVM> Tarif { get; set; } = new List<TarifCategorieVM>();
    }
}
