namespace ProlabWeb.ViewModels
{
    public class PrescripteurEditVM
    {
        public Guid Prescripteurid { get; set; }

        public string Nom { get; set; } = null!;

        public string? Description { get; set; }

        public bool Isactive { get; set; }

        public string? Adresse { get; set; }

        public string? Tel { get; set; }
    }
}
