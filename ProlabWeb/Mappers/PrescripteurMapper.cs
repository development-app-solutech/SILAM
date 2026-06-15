using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class PrescripteurMapper : Profile
    {
        public PrescripteurMapper()
        {
            CreateMap<Prescripteur, PrescripteurCreateVM>();
            CreateMap<PrescripteurCreateVM, Prescripteur>();

            CreateMap<Prescripteur, PrescripteurEditVM>();
            CreateMap<PrescripteurEditVM, Prescripteur>();

            CreateMap<Prescripteur, EntetedemandePrescripteurVM>();
            CreateMap<EntetedemandePrescripteurVM, Prescripteur>();
        }
    }
}
