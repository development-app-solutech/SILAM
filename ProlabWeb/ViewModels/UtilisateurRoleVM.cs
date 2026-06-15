namespace ProlabWeb.ViewModels
{
    public class UtilisateurRoleVM
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NormalizedName { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsSelected { get; set; } // pour checkbox
    }
}
