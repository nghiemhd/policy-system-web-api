using AutoMapper;
using SingLife.PolicySystem.UA.UseCases.Transactions;
using SingLife.ULTracker.WebAPI.Contracts.UA.Transactions;

namespace SingLife.PolicySystem.UA.WebApi.V1.MappingProfiles
{
    public class UATransactionMappingProfile : Profile
    {
        public UATransactionMappingProfile()
        {
            CreateMap<DeleteTransactionRequest, DeleteTransactionCommand>();

            CreateMap<EditTransactionRequest, EditTransactionCommand>();

            CreateMap<CreateTransactionRequest, CreateTransactionCommand>();
        }
    }
}