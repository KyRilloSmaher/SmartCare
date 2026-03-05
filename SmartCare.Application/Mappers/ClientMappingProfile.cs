using AutoMapper;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Domain.Entities;

public class SignUpProfile : Profile
{
    public SignUpProfile()
    {
        CreateMap<SignUpRequest, ApplictionUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))

            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src)); // Map DTO to Client

        CreateMap<SignUpRequest, Client>()
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.Addresses, opt => opt.Ignore()) // We'll map manually
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Will set after creation

        CreateMap<CreateAddressRequestDto, Address>();
    }
}