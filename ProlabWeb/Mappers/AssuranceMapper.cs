using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class AssuranceMapper : Profile
    {
        public AssuranceMapper()
        {
            CreateMap<AssuranceCreateVM, Assurance>();
            CreateMap<AssuranceEditVM, Assurance>();
            CreateMap<Assurance, AssuranceEditVM>();
        }
    }
} 