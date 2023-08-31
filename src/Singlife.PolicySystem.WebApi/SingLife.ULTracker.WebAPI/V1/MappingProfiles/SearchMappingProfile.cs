using AutoMapper;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.Search;
using SingLife.ULTracker.WebAPI.Contracts.Search;
using System;
using System.Linq;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class SearchMappingProfile : Profile
    {
        public SearchMappingProfile()
        {
            CreateMap<SearchPoliciesRequest, SearchPoliciesQuery>();
            CreateMap<SearchPredicate, SearchPredicateDto>()
                .ForMember(dest => dest.Operands, opt => opt.ResolveUsing(ConvertOperands));

            CreateMap<SearchPoliciesResultDto, SearchPoliciesResponse>();
            CreateMap<MatchedPolicyDto, MatchedPolicy>();
        }

        private object[] ConvertOperands(SearchPredicate predicate)
        {
            if (predicate.Operands?.Length == 0)
                return new object[0];

            switch (predicate.FieldType)
            {
                case FieldType.Bool:
                    return ConvertOperands(predicate, OperandParser.ParseBoolean);

                case FieldType.DateTime:
                    return ConvertOperands(predicate, OperandParser.ParseDateTime);

                case FieldType.Decimal:
                    return ConvertOperands(predicate, OperandParser.ParseDecimal);

                case FieldType.Integer:
                    return ConvertOperands(predicate, OperandParser.ParseInt32);

                case FieldType.Text:
                default:
                    return ConvertOperands(predicate, text => text);
            }
        }

        private static object[] ConvertOperands<T>(SearchPredicate predicate, Func<string, T> conversionFunction) =>
            predicate.Operands
                .Select(conversionFunction)
                .Cast<object>()
                .ToArray();
    }
}