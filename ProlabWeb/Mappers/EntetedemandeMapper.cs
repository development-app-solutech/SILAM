using AutoMapper;
using Elfie.Serialization;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class EntetedemandeMapper : Profile
    {
        public EntetedemandeMapper()
        {
            CreateMap<Entetedemande, EntetedemandeCreateVM>()
                .ForMember(dest => dest.Patientid, opt => opt.MapFrom(src => src.Patientid))
                .ForMember(dest => dest.Prescripteurid, opt => opt.MapFrom(src => src.Prescripteurid));

            CreateMap<EntetedemandeCreateVM, Entetedemande>()
                .ForMember(dest => dest.Patientid, opt => opt.MapFrom(src => src.Patientid))
                .ForMember(dest => dest.Prescripteurid, opt => opt.MapFrom(src => src.Prescripteurid));

            CreateMap<Entetedemande, EnteteDemandeEditVM>()
                .ForMember(dest => dest.Patientid, opt => opt.MapFrom(src => src.Patientid))
                .ForMember(dest => dest.Prescripteurid, opt => opt.MapFrom(src => src.Prescripteurid));

            CreateMap<EnteteDemandeEditVM, Entetedemande>()
                .ForMember(dest => dest.Patientid, opt => opt.MapFrom(src => src.Patientid))
                .ForMember(dest => dest.Prescripteurid, opt => opt.MapFrom(src => src.Prescripteurid));
            

        }
    }
}
