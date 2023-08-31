using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.UA.UseCases.Policies;
using SingLife.PolicySystem.UA.UseCases.Policies.Documents;
using SingLife.PolicySystem.UA.UseCases.Transactions;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.SystemTime;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.UA.Policies;
using SingLife.ULTracker.WebAPI.Contracts.UA.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.UA.WebApi.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ua-policies")]
    public class UAPolicyController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UAPolicyController(
          IMediator mediator,
          IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UAPolicySummary))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetPolicySummaryQuery
            {
                PolicyId = policyId
            };

            var policySummaryDto = await mediator.Send(query, cancellationToken);

            if (policySummaryDto == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<UAPolicySummary>(policySummaryDto));
        }

        [HttpGet]
        [Route("details/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UAPolicyDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetUAPolicyDetailsQuery
            {
                PolicyId = policyId
            };

            var policyDetailsDto = await mediator.Send(query, cancellationToken);

            if (policyDetailsDto == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<UAPolicyDetails>(policyDetailsDto));
        }

        [HttpPost]
        [Route("policy-details-export/{policyNumber}/excel")]
        public async Task<IActionResult> ExportPolicyDetails([FromBody] ExportPolicyDetailsRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGeneratePolicyDetailsCommand
            {
                PolicyNumber = request.PolicyNumber,
                UserName = request.UserName,
                ExportDate = DateTimeHelper.Today
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("policy-details-download/excel")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(File))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadPolicyDetails([FromBody] DownloadPolicyDetailsRequest request, CancellationToken cancellationToken)
        {
            var command = new DownloadPolicyDetailsCommand
            {
                FileKey = request.FileKey,
                FileName = request.FileName
            };

            var fileDto = await mediator.Send(command, cancellationToken);

            if (fileDto == null)
            {
                return NotFound();
            }

            var fileContract = mapper.Map<File>(fileDto);

            return Ok(fileContract);
        }

        [HttpPut]
        [Route("documents/{policyNumber}/e-application")]
        public async Task<IActionResult> GenerateEApplication(string policyNumber, CancellationToken cancellationToken)
        {
            var query = new GetPolicyAndApplicationIdentitiesQuery { PolicyNumber = policyNumber };

            var policyApplicationIdentitiesDto = await mediator.Send(query, cancellationToken);

            var command = new GenerateEAppCommand { ApplicationNumber = policyApplicationIdentitiesDto.ApplicationNumber.Value };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut]
        [Route("documents/{policyNumber}/policy-cover")]
        public async Task<IActionResult> GeneratePolicyCoverPage(string policyNumber, CancellationToken cancellationToken)
        {
            var query = new GetPolicyAndApplicationIdentitiesQuery { PolicyNumber = policyNumber };

            var policyApplicationIdentitiesDto = await mediator.Send(query, cancellationToken);

            var command = new GenerateCoverPageCommand { ApplicationNumber = policyApplicationIdentitiesDto.ApplicationNumber.Value };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut]
        [Route("documents/{policyNumber}/policy-schedule")]
        public async Task<IActionResult> GeneratePolicySchedule(string policyNumber, CancellationToken cancellationToken)
        {
            var query = new GetPolicyAndApplicationIdentitiesQuery { PolicyNumber = policyNumber };

            var policyApplicationIdentitiesDto = await mediator.Send(query, cancellationToken);

            var command = new GeneratePolicyScheduleCommand { ApplicationNumber = policyApplicationIdentitiesDto.ApplicationNumber.Value };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut]
        [CorrelatedAuditApi("Policy:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([FromBody] EditPolicyRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await EditPolicy();
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            async Task<IActionResult> EditPolicy()
            {
                var command = mapper.Map<EditPolicyCommand>(request);
                command.UniqueCorrelationId = this.GetUniqueCorrelationId();
                command.AuthenticatedUser = this.GetAuthenticatedUser();

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpPost]
        [Route("{policyId}/transactions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchPolicyTransactionResult))]
        public async Task<IActionResult> SearchPolicyTransactionsAsync([FromBody] SearchPolicyTransactionsRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchPolicyTransactionsQuery>(request);

            var result = await mediator.Send(query, cancellationToken);

            var searchPolicyTransactionResult = new SearchPolicyTransactionResult
            {
                Currency = result.Currency,
                TransactionPage = mapper.Map<PagedSearchResult<MatchedTransaction>>(result.TransactionPage)
            };

            return Ok(searchPolicyTransactionResult);
        }

        [HttpPut]
        [Route("account-values")]
        [CorrelatedAuditApi("Policy:RecalculateAccountValues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RecalculateAccountValues([FromBody] RecalculateAccountValuesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new RecalculateAccountValuesCommand()
                {
                    PolicyId = request.PolicyId,
                    UserName = this.User.Identity.Name
                };

                await mediator.Send(command, cancellationToken);
            }
            catch (PolicyNotFoundException exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok();
        }
    }
}