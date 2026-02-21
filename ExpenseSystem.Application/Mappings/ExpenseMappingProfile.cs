using AutoMapper;
using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Domain.Entities;

namespace ExpenseSystem.Application.Mappings;

public class ExpenseMappingProfile : Profile
{
    public ExpenseMappingProfile()
    {
        CreateMap<ExpenseClaim, ExpenseResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ExpenseClaim, PaymentRequestDto>()
            .ForMember(dest => dest.ExpenseId, opt => opt.MapFrom(src => src.Id));

        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}