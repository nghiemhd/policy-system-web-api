using AutoMapper;
using SingLife.ULTracker.UseCases.Audit;
using SingLife.ULTracker.WebAPI.Contracts.Audits;
using SingLife.ULTracker.WebAPI.Contracts.Common;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class AuditMappingProfile : Profile
    {
        public AuditMappingProfile()
        {
            CreateMap<UseCases.Audit.ValueChange, Contracts.Audits.ValueChange>();
            CreateMap<UseCases.Audit.ValueChangesGroup, Contracts.Audits.ValueChangesGroup>();
            CreateMap<GetValueChangesRequest, GetValueChangesQuery>();
            CreateMap<FailedAuditEventDto, FailedAuditEvent>();
            CreateMap<AuditEventDto, AuditEvent>();
        }
    }
}