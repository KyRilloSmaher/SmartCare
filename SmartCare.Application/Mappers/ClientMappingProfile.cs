using AutoMapper;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Domain.Entities;

public class ClientProfile : Profile
{
    public ClientProfile()
    {

        CreateMap<SignUpRequest, Client>()
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.Addresses, opt => opt.Ignore()) // We'll map manually
            .ForMember(dest => dest.User, opt => opt.Ignore()); // Will set after creation

        CreateMap<CreateAddressRequestDto, Address>();
        CreateMap<Client, ClientResponseDto>()
             .ForMember(dest => dest.FirstName,
                 opt => opt.MapFrom(src => src.User.FirstName))

             .ForMember(dest => dest.LastName,
                 opt => opt.MapFrom(src => src.User.LastName))

             .ForMember(dest => dest.UserName,
                 opt => opt.MapFrom(src => src.User.UserName))

             .ForMember(dest => dest.Email,
                 opt => opt.MapFrom(src => src.User.Email))

             .ForMember(dest => dest.PhoneNumber,
                 opt => opt.MapFrom(src => src.User.PhoneNumber))

             .ForMember(dest => dest.Gender,
                 opt => opt.MapFrom(src => src.User.Gender))

             .ForMember(dest => dest.ProfileImageUrl,
                 opt => opt.MapFrom(src => src.User.ProfileImageUrl))

             .ForMember(dest => dest.AccountType,
                 opt => opt.MapFrom(src => src.AccountType.ToString()))

             .ForMember(dest => dest.Addresses,
                 opt => opt.MapFrom(src => src.Addresses));
    }
    }