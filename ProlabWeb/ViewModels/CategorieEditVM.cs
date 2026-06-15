using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class CategorieEditVM
    {

        public Guid Categorieid { get; set; }

        public string Nom { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Prix { get; set; }

        public bool Isactive { get; set; }

        public string[] IdsAnalyse { get; set; } = new string[0];

    }
}
