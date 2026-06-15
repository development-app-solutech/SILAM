namespace ProlabWeb.ViewModels
{
    public class TarifCategorieVM
    {
        public Guid Categorieid { get; set; }

        public string Nom { get; set; } = null!;

        public string? Prix { get; set; }
    }
}
