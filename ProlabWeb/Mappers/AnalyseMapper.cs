using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class AnalyseMapper : Profile
    {
        public AnalyseMapper()
        {
            CreateMap<Analyse, AnalyseCreateVM>();
            CreateMap<AnalyseCreateVM, Analyse>();

            CreateMap<Analyse, AnalyseEditVM>();
            CreateMap<AnalyseEditVM, Analyse>();

            CreateMap<ValeurreferenceCreateVM, Valeurreference>();
            CreateMap<Valeurreference, ValeurreferenceCreateVM>();
            CreateMap<ValeurreferenceEditVM, Valeurreference>();
            CreateMap<Valeurreference, ValeurreferenceEditVM>();
        }
    }
}
