using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurAvecLaboratoiresVM
    {
        public Utilisateur Utilisateur { get; set; }

        public List<Loboratoire> Laboratoires { get; set; } = new();
    }
}
