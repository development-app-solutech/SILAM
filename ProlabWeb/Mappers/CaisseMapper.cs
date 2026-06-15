using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class CaisseMapper : Profile
    {
        public CaisseMapper()
        {
            CreateMap<Caisse, CaisseCreateVM>().ReverseMap();
            CreateMap<Caisse, CaisseEditVM>().ReverseMap();
        }
    }
}