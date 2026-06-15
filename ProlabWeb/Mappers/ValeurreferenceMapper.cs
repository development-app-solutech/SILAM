using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class ValeurreferenceMapper : Profile
    {
        public ValeurreferenceMapper()
        {
            CreateMap<Valeurreference, ValeurreferenceCreateVM>();
            CreateMap<ValeurreferenceCreateVM, Valeurreference>();

            CreateMap<Valeurreference, ValeurreferenceEditVM>();
            CreateMap<ValeurreferenceEditVM, Valeurreference>();

            CreateMap<Valeurreference, ValeurreferenceCreateAvecOngletVM>();
            CreateMap<ValeurreferenceCreateAvecOngletVM, Valeurreference>();

            CreateMap<Valeurreference, ValeurreferenceEditAvecOngletVM>();
            CreateMap<ValeurreferenceEditAvecOngletVM, Valeurreference>();
        }
    }
}
