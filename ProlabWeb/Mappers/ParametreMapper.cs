using AutoMapper;
using Newtonsoft.Json;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System.Globalization;

namespace ProlabWeb.Mappers
{
    public class ParametreMapper : Profile
    {
        public ParametreMapper()
        {
            CreateMap<Parametre, ParametreItemCreateVM>()
                .ForMember(dest => dest.Masquer, opt => opt.MapFrom(src => src.Masquerdansrapport))
                .ForMember(dest => dest.UniteStandard, opt => opt.MapFrom(src => src.Codeunite))
                .ForMember(dest => dest.UniteSI, opt => opt.MapFrom(src => src.Codeunitesi))
                .ForMember(dest => dest.FacteurConversion, opt => opt.MapFrom(src => src.Facteurconversionsi.HasValue ? src.Facteurconversionsi.Value.ToString() : null))
                .ForMember(dest => dest.ResultatStandard, opt => opt.MapFrom(src => src.Decimalresultatstandard.HasValue ? src.Decimalresultatstandard.Value.ToString() : null))
                .ForMember(dest => dest.ResultatSI, opt => opt.MapFrom(src => src.Decimalresultatsi.HasValue ? src.Decimalresultatsi.Value.ToString() : null));

            CreateMap<ParametreItemCreateVM, Parametre>()
                .ForMember(dest => dest.Parametreid, opt => opt.Ignore())
                .ForMember(dest => dest.Idanalyse, opt => opt.Ignore())
                .ForMember(dest => dest.Masquerdansrapport, opt => opt.MapFrom(src => src.Masquer))
                .ForMember(dest => dest.Codeunite, opt => opt.MapFrom(src => src.UniteStandard))
                .ForMember(dest => dest.Codeunitesi, opt => opt.MapFrom(src => src.UniteSI))
                .ForMember(dest => dest.Facteurconversionsi, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.FacteurConversion) ? (decimal?)null : decimal.Parse(src.FacteurConversion)))
                .ForMember(dest => dest.Decimalresultatstandard, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ResultatStandard) ? (int?)null : int.Parse(src.ResultatStandard)))
                .ForMember(dest => dest.Decimalresultatsi, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ResultatSI) ? (int?)null : int.Parse(src.ResultatSI)));

            CreateMap<ParametreItemEditVM, Parametre>()
                .ForMember(dest => dest.Parametreid, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Masquerdansrapport, opt => opt.MapFrom(src => src.Masquer))
                .ForMember(dest => dest.Codeunite, opt => opt.MapFrom(src => src.UniteStandard))
                .ForMember(dest => dest.Codeunitesi, opt => opt.MapFrom(src => src.UniteSI))
                .ForMember(dest => dest.Decimalresultatstandard, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ResultatStandard) ? (int?)null : int.Parse(src.ResultatStandard)))
                .ForMember(dest => dest.Decimalresultatsi, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ResultatSI) ? (int?)null : int.Parse(src.ResultatSI)))
                .ForMember(dest => dest.Facteurconversionsi, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.FacteurConversion) ? (decimal?)null : decimal.Parse(src.FacteurConversion)));

            CreateMap<Parametre, ParametreItemEditVM>()
                .ForMember(dest => dest.Masquer, opt => opt.MapFrom(src => src.Masquerdansrapport))
                .ForMember(dest => dest.UniteStandard, opt => opt.MapFrom(src => src.Codeunite))
                .ForMember(dest => dest.UniteSI, opt => opt.MapFrom(src => src.Codeunitesi))
                .ForMember(dest => dest.ResultatStandard, opt => opt.MapFrom(src => src.Decimalresultatstandard.HasValue ? src.Decimalresultatstandard.Value.ToString() : null))
                .ForMember(dest => dest.ResultatSI, opt => opt.MapFrom(src => src.Decimalresultatsi.HasValue ? src.Decimalresultatsi.Value.ToString() : null))
                .ForMember(dest => dest.FacteurConversion, opt => opt.MapFrom(src => src.Facteurconversionsi.HasValue ? src.Facteurconversionsi.Value.ToString() : null));

            // Entité → ViewModel
            CreateMap<Parametre, ParametreCreateAvecOngletVM>()
                .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.Nom))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Masquer, opt => opt.MapFrom(src => src.Masquerdansrapport))
                .ForMember(dest => dest.UniteStandard, opt => opt.MapFrom(src => src.Codeunite))
                .ForMember(dest => dest.UniteSI, opt => opt.MapFrom(src => src.Codeunitesi))
                .ForMember(dest => dest.ResultatStandard, opt => opt.MapFrom(src =>
                    src.Decimalresultatstandard.HasValue ? src.Decimalresultatstandard.Value.ToString() : null))
                .ForMember(dest => dest.ResultatSI, opt => opt.MapFrom(src =>
                    src.Decimalresultatsi.HasValue ? src.Decimalresultatsi.Value.ToString() : null))
                .ForMember(dest => dest.FacteurConversion, opt => opt.MapFrom(src =>
                    src.Facteurconversionsi.HasValue ? src.Facteurconversionsi.Value.ToString() : null))
                .ForMember(dest => dest.OrdreAffichage, opt => opt.MapFrom(src => src.Ordreaffichage))
                .ForMember(dest => dest.Builder, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.Valuebuilder)
                        ? new ParametreItemBuilderVM()
                        : JsonConvert.DeserializeObject<ParametreItemBuilderVM>(src.Valuebuilder)))
                .ForSourceMember(src => src.Formuleautomate, opt => opt.DoNotValidate());

            // ViewModel → Entité
            CreateMap<ParametreCreateAvecOngletVM, Parametre>()
                .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.Nom))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Masquerdansrapport, opt => opt.MapFrom(src => src.Masquer))
                .ForMember(dest => dest.Codeunite, opt => opt.MapFrom(src => src.UniteStandard))
                .ForMember(dest => dest.Codeunitesi, opt => opt.MapFrom(src => src.UniteSI))
                .ForMember(dest => dest.Decimalresultatstandard, opt => opt.MapFrom(src =>
                    MappingHelpers.ParseInt(src.ResultatStandard)))
                .ForMember(dest => dest.Decimalresultatsi, opt => opt.MapFrom(src =>
                    MappingHelpers.ParseInt(src.ResultatSI)))
                .ForMember(dest => dest.Facteurconversionsi, opt => opt.MapFrom(src =>
                    MappingHelpers.ParseDecimal(src.FacteurConversion)))
                .ForMember(dest => dest.Ordreaffichage, opt => opt.MapFrom(src => src.OrdreAffichage))
                .ForMember(dest => dest.Valuebuilder, opt => opt.MapFrom(src =>
                    JsonConvert.SerializeObject(src.Builder, Formatting.Indented)));

            // Entité → ViewModel
            CreateMap<Parametre, ParametreEditAvecOngletVM>()
                .ForMember(dest => dest.Parametreid, opt => opt.MapFrom(src => src.Parametreid))
                .ForMember(dest => dest.Idanalyse, opt => opt.MapFrom(src => src.Idanalyse))
                .ForMember(dest => dest.Masquer, opt => opt.MapFrom(src => src.Masquerdansrapport))
                .ForMember(dest => dest.UniteStandard, opt => opt.MapFrom(src => src.Codeunite))
                .ForMember(dest => dest.UniteSI, opt => opt.MapFrom(src => src.Codeunitesi))
                .ForMember(dest => dest.ResultatStandard, opt => opt.MapFrom(src =>
                    src.Decimalresultatstandard.HasValue ? src.Decimalresultatstandard.Value.ToString() : null))
                .ForMember(dest => dest.ResultatSI, opt => opt.MapFrom(src =>
                    src.Decimalresultatsi.HasValue ? src.Decimalresultatsi.Value.ToString() : null))
                .ForMember(dest => dest.FacteurConversion, opt => opt.MapFrom(src =>
                    src.Facteurconversionsi.HasValue ? src.Facteurconversionsi.Value.ToString() : null))
                .ForMember(dest => dest.OrdreAffichage, opt => opt.MapFrom(src => src.Ordreaffichage))
                .ForMember(dest => dest.Builder, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.Valuebuilder)
                        ? new ParametreItemBuilderVM()
                        : JsonConvert.DeserializeObject<ParametreItemBuilderVM>(src.Valuebuilder)));

            // ViewModel → Entité
            CreateMap<ParametreEditAvecOngletVM, Parametre>()
                .ForMember(dest => dest.Parametreid, opt => opt.MapFrom(src => src.Parametreid ?? Guid.Empty))
                .ForMember(dest => dest.Idanalyse, opt => opt.MapFrom(src => src.Idanalyse))
                .ForMember(dest => dest.Masquerdansrapport, opt => opt.MapFrom(src => src.Masquer))
                .ForMember(dest => dest.Codeunite, opt => opt.MapFrom(src => src.UniteStandard))
                .ForMember(dest => dest.Codeunitesi, opt => opt.MapFrom(src => src.UniteSI))
                .ForMember(dest => dest.Decimalresultatstandard, opt => opt.MapFrom(src => MappingHelpers.ParseInt(src.ResultatStandard)))
                .ForMember(dest => dest.Decimalresultatsi, opt => opt.MapFrom(src => MappingHelpers.ParseInt(src.ResultatSI)))
                .ForMember(dest => dest.Facteurconversionsi, opt => opt.MapFrom(src => MappingHelpers.ParseDecimal(src.FacteurConversion)))
                .ForMember(dest => dest.Ordreaffichage, opt => opt.MapFrom(src => src.OrdreAffichage))
                .ForMember(dest => dest.Valuebuilder, opt => opt.MapFrom(src =>
                    JsonConvert.SerializeObject(src.Builder, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    })));

        }
    }
} 
