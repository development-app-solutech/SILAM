using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System.Globalization;

namespace ProlabWeb.Mappers
{
    public class AnalyseAvecOngletMapper : Profile
    {
        public AnalyseAvecOngletMapper()
        {
            CreateMap<Analyse, AnalyseAvecOngletCreateVM>()
                .ForMember(dest => dest.Parametres, opt => opt.Ignore())
                .ForMember(dest => dest.ValeursReference, opt => opt.Ignore())
                .ForMember(dest => dest.TarifsAssurance, opt => opt.Ignore());

            CreateMap<AnalyseAvecOngletCreateVM, Analyse>()
                .ForMember(dest => dest.Parametres, opt => opt.Ignore())
                .ForMember(dest => dest.Valeurreferences, opt => opt.Ignore())
                .ForMember(dest => dest.Tarifanalyseassurances, opt => opt.Ignore());

            CreateMap<Analyse, AnalyseAvecOngletEditVM>()
                .ForMember(dest => dest.Parametres, opt => opt.Ignore())
                .ForMember(dest => dest.ValeursReference, opt => opt.Ignore())
                .ForMember(dest => dest.TarifsAssurance, opt => opt.Ignore());

            CreateMap<AnalyseAvecOngletEditVM, Analyse>()
                .ForMember(dest => dest.Parametres, opt => opt.Ignore())
                .ForMember(dest => dest.Valeurreferences, opt => opt.Ignore())
                .ForMember(dest => dest.Tarifanalyseassurances, opt => opt.Ignore());
        }
    }
}
