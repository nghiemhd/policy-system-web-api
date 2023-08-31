using AutoMapper;
using SingLife.ULTracker.UseCases.SystemTime;
using SingLife.ULTracker.WebAPI.Contracts.Common;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class SystemTimeMappingsProflie : Profile
    {
        public SystemTimeMappingsProflie()
        {
            CreateMap<ClockOffsetDto, ClockOffset>();

            CreateMap<ClockOffset, UpdateClockOffsetCommand>();
        }
    }
}