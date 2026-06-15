using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class UtilisateurMapper : Profile
    {
        public UtilisateurMapper()
        {
            CreateMap<Utilisateur, UtilisateurCreateVM>();
            CreateMap<UtilisateurCreateVM, Utilisateur>();
            CreateMap<Utilisateur, UtilisateurEditVM>();
            CreateMap<UtilisateurEditVM, Utilisateur>();
        }
    }
}
