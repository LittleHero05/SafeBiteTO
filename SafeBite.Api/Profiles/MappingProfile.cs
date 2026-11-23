using AutoMapper;
using SafeBite.Api.Dtos;
using SafeBite.Api.Models;

namespace SafeBite.Api.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Establishments
            CreateMap<Establishment, EstablishmentReadDto>();
            CreateMap<EstablishmentCreateDto, Establishment>();
            CreateMap<EstablishmentUpdateDto, Establishment>().ReverseMap();

            // Inspections (EstablishmentName is flattened automatically from Establishment.Name)
            CreateMap<Inspection, InspectionReadDto>();
            CreateMap<InspectionCreateDto, Inspection>();
            CreateMap<InspectionUpdateDto, Inspection>().ReverseMap();

            // Owner responses
            CreateMap<OwnerResponse, OwnerResponseReadDto>();
            CreateMap<OwnerResponseCreateDto, OwnerResponse>();
            CreateMap<OwnerResponseUpdateDto, OwnerResponse>().ReverseMap();

            // Badge: flatten an establishment and its inspections into one summary
            CreateMap<Establishment, BadgeDto>()
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Street + " " + s.PostalCode))
                .ForMember(d => d.CurrentStatus, o => o.MapFrom((s, d) => Latest(s)?.Status ?? "Not inspected"))
                .ForMember(d => d.LastInspected, o => o.MapFrom((s, d) => Latest(s)?.InspectionDate))
                .ForMember(d => d.LatestInfraction, o => o.MapFrom((s, d) => Latest(s)?.InfractionSummary))
                .ForMember(d => d.CrucialCount, o => o.MapFrom((s, d) => Latest(s)?.CrucialCount ?? 0))
                .ForMember(d => d.SignificantCount, o => o.MapFrom((s, d) => Latest(s)?.SignificantCount ?? 0))
                .ForMember(d => d.MinorCount, o => o.MapFrom((s, d) => Latest(s)?.MinorCount ?? 0))
                .ForMember(d => d.Last12Months, o => o.MapFrom((s, d) => s.Inspections
                    .Where(i => i.InspectionDate >= DateOnly.FromDateTime(DateTime.Today.AddMonths(-12)))
                    .OrderBy(i => i.InspectionDate)
                    .Select(i => i.Status)
                    .ToList()));
        }

        private static Inspection? Latest(Establishment e) =>
            e.Inspections.OrderByDescending(i => i.InspectionDate).FirstOrDefault();
    }
}