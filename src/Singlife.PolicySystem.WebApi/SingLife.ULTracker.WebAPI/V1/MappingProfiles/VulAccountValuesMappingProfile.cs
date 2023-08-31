using AutoMapper;
using SingLife.ULTracker.UseCases.VulAccountValuesImports;
using SingLife.ULTracker.WebAPI.Contracts.VulAccountValuesImports;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class VulAccountValuesMappingProfile : Profile
    {
        public VulAccountValuesMappingProfile()
        {
            CreateMap<AccountValuesImportFileDto, AccountValuesImportFile>();

            CreateMap<ImportAccountValuesRequest, ImportAccountValuesCommand>();
        }
    }
}