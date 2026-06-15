using AutoMapper;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Mappers
{
    public class MaterielMapper  : Profile 
    {
        public MaterielMapper()
        {
            CreateMap<Materiel, MaterielCreateVM>();
            CreateMap<MaterielCreateVM, Materiel>();
        }
    }
}
