using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;

namespace API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Appointment, AppointmentToReturnDto>();
            CreateMap<CreateOrUpdateAppointmentDto, Appointment>();
            CreateMap<AppUser, UserDto>();
            CreateMap<RegisterDto, AppUser>();
        }        
    }
}