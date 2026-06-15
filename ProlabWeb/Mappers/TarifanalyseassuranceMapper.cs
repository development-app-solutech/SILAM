using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class TarifanalyseassuranceMapper : Profile
    {
        public TarifanalyseassuranceMapper()
        {
            CreateMap<TarifanalyseassuranceCreateVM, Tarifanalyseassurance>();
            CreateMap<Tarifanalyseassurance, TarifanalyseassuranceCreateVM>();

            CreateMap<TarifanalyseassuranceEditVM, Tarifanalyseassurance>();
            CreateMap<Tarifanalyseassurance, TarifanalyseassuranceEditVM>();

            CreateMap<TarifanalyseassuranceCreateAvecOngletVM, Tarifanalyseassurance>();

            CreateMap<Tarifanalyseassurance, TarifanalyseassuranceCreateAvecOngletVM>()
                .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.CodeassuranceNavigation.Nom));

            CreateMap<Tarifanalyseassurance, TarifanalyseassuranceEditAvecOngletVM>()
            .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.CodeassuranceNavigation.Nom));

            CreateMap<TarifanalyseassuranceEditAvecOngletVM, Tarifanalyseassurance>();

        }
    }
}
