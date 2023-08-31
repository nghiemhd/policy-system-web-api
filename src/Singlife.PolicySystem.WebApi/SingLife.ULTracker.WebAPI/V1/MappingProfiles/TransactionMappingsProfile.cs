using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Transactions;
using SingLife.ULTracker.UseCases.Transactions.DataExport;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Transactions;
using System;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class TransactionMappingsProfile : Profile
    {
        public TransactionMappingsProfile()
        {
            CreateMap<TransactionDTO, Transaction>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => ConvertFromEnumToString(src.TransactionType)));

            CreateMap<TransactionPagedSearch, SearchTransactionsQuery>();

            CreateMap<TransactionPagedSearch, SearchTransactionsByDatesQuery>();

            CreateMap<UpdateTransactionRequest, UpdateTransactionCommand>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => ConvertFromStringToEnum(src.TransactionType)));

            CreateMap<CreateTransactionRequest, CreateTransactionCommand>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => ConvertFromStringToEnum(src.TransactionType)));

            CreateMap<TransactionAttachmentRequest, DeleteTransactionAttachmentCommand>();

            CreateMap<TransactionRequest, DeleteTransactionCommand>();

            CreateMap<TransactionRequest, GetTransactionQuery>();

            CreateMap<CreateTransactionDiscountRequest, CreateTransactionDiscountCommand>();

            CreateMap<EditTransactionDiscountRequest, UpdateTransactionDiscountCommand>();

            CreateMap<TransactionAttachment, TransactionAttachmentDto>();

            CreateMap<TransactionAttachmentDto, TransactionAttachment>();

            CreateMap<ExportTransactionRequest, GetTransactionsForExportingQuery>();

            CreateMap<Page<TransactionDTO>, PagedSearchResult<Transaction>>();
        }

        public static string ConvertFromEnumToString(Model.Transactions.TransactionTypes transactionType)
        {
            switch (transactionType)
            {
                case Model.Transactions.TransactionTypes.InitialPremium:
                    return TransactionTypes.InitialPremium;

                case Model.Transactions.TransactionTypes.Premium:
                    return TransactionTypes.Premium;

                case Model.Transactions.TransactionTypes.Withdrawal:
                    return TransactionTypes.Withdrawal;

                default:
                    throw new ArgumentException($"Unsupported transaction type {transactionType}");
            }
        }

        public static Model.Transactions.TransactionTypes ConvertFromStringToEnum(string transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypes.InitialPremium:
                    return Model.Transactions.TransactionTypes.InitialPremium;

                case TransactionTypes.Premium:
                    return Model.Transactions.TransactionTypes.Premium;

                case TransactionTypes.Withdrawal:
                    return Model.Transactions.TransactionTypes.Withdrawal;

                default:
                    throw new ArgumentException($"Unsupported transaction type {transactionType}");
            }
        }
    }
}