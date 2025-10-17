using AutoMapper;
using SmartCare.Domain.Entities;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
namespace SmartCare.Application.Mappers
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile()
        {
            CreateCompanyRequestDtoToCompany();
            UpdateCompanyRequestDtoToCompany();
            CompanyToCompanyResponseDto();
            CompanyToCompanyResponseForAdminDto();

        }
        void CreateCompanyRequestDtoToCompany()
        {
            CreateMap<CreateCompanyRequestDto, Company>()
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        }
        void UpdateCompanyRequestDtoToCompany()
        {
            CreateMap<UpdateCompanyRequest, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
        void CompanyToCompanyResponseDto()
        {
            CreateMap<Company, CompanyResponseDto>();
        }
        void CompanyToCompanyResponseForAdminDto()
        {
            CreateMap<Company, CompanyResponseForAdminDto>();
        }
    }
}
