using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class PatientMapper : Profile
    {
        public PatientMapper()
        {
            CreateMap<Patient, PatientCreateVM>();
            CreateMap<PatientCreateVM, Patient>();

            CreateMap<Patient, PatientEditVM>();
            CreateMap<PatientEditVM, Patient>();

            CreateMap<Patient, EnteteDemandePatientVM>();
            CreateMap<EnteteDemandePatientVM, Patient>();

        }
    }
}
