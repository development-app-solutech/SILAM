using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class EntetematerielapproMapper : Profile
    {
        public EntetematerielapproMapper()
        {
            CreateMap<EntetematerielapproCreateVM, Entetematerielappro>();
            CreateMap<Entetematerielappro, EntetematerielapproEditVM>();
            CreateMap<EntetematerielapproEditVM, Entetematerielappro>();
            // Si besoin, ajouter le mapping inverse ou d'autres propriétés
        }
    }
} 