using AutoMapper;
using Application.DTOs;
using Application.DTOs.Expenses;
using Domain.Entities;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Expense, ExpenseResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.Icon))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color));

        CreateMap<Alert, AlertDto>();
    }
}
