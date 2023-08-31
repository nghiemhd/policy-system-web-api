using Autofac.Features.AttributeFilters;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Singlife.ULTracker.WebAPI.Infrastructure;
using SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.Vul;
using SingLife.PolicySystem.Shared.ApiClient;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.Common.Policies.Documents;
using SingLife.ULTracker.Shared;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.PolicyAccountValues;
using SingLife.ULTracker.UseCases.Vul.V1.Documents;
using SingLife.ULTracker.UseCases.Vul.V1.Policies;
using SingLife.ULTracker.UseCases.Vul.V1.PolicyDetails;
using SingLife.ULTracker.UseCases.VulPolicies;
using SingLife.ULTracker.WebAPI.Contracts.Common.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.VulPolicies.Documents;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;
using PolicyIdentityContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.PolicyIdentity;
using VulPolicyContract = SingLife.ULTracker.WebAPI.Contracts.VulPolicies.VulPolicy;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/vul-policies")]
    public class VulPolicyController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly WebApiClient documentApiClient;
        private readonly WebApiClient quotationEngineApiClient;

        public VulPolicyController(
            IMediator mediator,
            IMapper mapper,
            [KeyFilter(WebApiClients.SingLifeDMS)] WebApiClient documentApiClient,
            [KeyFilter(WebApiClients.QuotationEngine)] WebApiClient quotationEngineApiClient)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.documentApiClient = documentApiClient;
            this.quotationEngineApiClient = quotationEngineApiClient;
        }

        [HttpPost]
        [CorrelatedAuditApi("Policy:Create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(VulPolicyContract vulPolicy, CancellationToken cancellationToken)
        {
            try
            {
                return await CreatePolicy();
            }
            catch (DuplicatePolicyNumberException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DuplicatePolicyOwnerException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> CreatePolicy()
            {
                var createVulPolicyCommand = new CreateVulPolicyCommand
                {
                    Policy = mapper.Map<VulPolicyDTO>(vulPolicy),
                    UniqueCorrelationId = this.GetUniqueCorrelationId(),
                    AuthenticatedUser = this.GetAuthenticatedUser()
                };

                var vulPolicyIdentity = await this.mediator.Send(createVulPolicyCommand, cancellationToken);

                var vulPolicyIdentityContract = mapper.Map<PolicyIdentityContract>(vulPolicyIdentity);

                this.SetAuditCorrelationId(vulPolicyIdentityContract.PolicyId.ToString());

                return Created($"vul-policies/{vulPolicyIdentityContract.PolicyId}", vulPolicyIdentityContract);
            }
        }

        [HttpPut]
        [CorrelatedAuditApi("Policy:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(VulPolicyContract policy, CancellationToken cancellationToken)
        {
            try
            {
                return await EditPolicy();
            }
            catch (DuplicatePolicyNumberException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DuplicatePolicyOwnerException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> EditPolicy()
            {
                var editVulPolicyCommand = new EditVulPolicyCommand
                {
                    ModifiedPolicy = mapper.Map<VulPolicyDTO>(policy),
                    UniqueCorrelationId = this.GetUniqueCorrelationId(),
                    AuthenticatedUser = this.GetAuthenticatedUser()
                };

                await mediator.Send(editVulPolicyCommand, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VulPolicyContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var getVulPolicyQuery = new GetVulPolicyQuery
            {
                PolicyId = id
            };

            var vulPolicyDto = await mediator.Send(getVulPolicyQuery, cancellationToken);

            return vulPolicyDto == null
                ? NotFound()
                : Ok(mapper.Map<VulPolicyContract>(vulPolicyDto));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VulPolicyContract))]
        public async Task<IActionResult> GetByPolicyNumber(string policyNumber, CancellationToken cancellationToken)
        {
            var getPolicyQuery = new GetVulPolicyQuery
            {
                PolicyNumber = policyNumber
            };

            var result = await mediator.Send(getPolicyQuery, cancellationToken);

            return Ok(mapper.Map<VulPolicyContract>(result));
        }

        [HttpGet]
        [Route("{id}/documents/terms-of-acceptance")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> PrintPolicyTermOfAcceptance([FromQuery] PrintTermsOfAcceptanceRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetTermOfAcceptanceDocumentDataQuery
                {
                    PolicyId = request.PolicyId
                };

                var documentData = await mediator.Send(query, cancellationToken);

                var documentContent = await PrintDocument(TemplateIds.VulTermOfAcceptance, documentData);

                var now = DateTime.UtcNow.ToSingaporeTime();
                var documentContract = new FileContract
                {
                    FileName = $"{documentData.PolicyNumber} Terms of Acceptance {now:yyyy-MM-dd HH-mm-ss}.pdf",
                    ContentType = "application/pdf",
                    FileContent = documentContent
                };

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("{policyId:guid}/documents/pi")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPIDocument(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                var piDocument = await PrintPIDocumentAsync(policyId, cancellationToken);

                return Ok(piDocument);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (PrintPIException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        private async Task<FileContract> PrintPIDocumentAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var piDocumentData = await GetPIDocumentDataAsync(policyId);
            var piDocument = await PrintPIDocumentAsync(piDocumentData, cancellationToken);

            if (piDocument.HasError)
            {
                throw new PrintPIException(piDocument.Errors[0].Message);
            }

            return new FileContract
            {
                FileName = $"Variable Universal Life Calculator {DateTime.Now:dd-MMM-yyyy-hhmmss}.pdf",
                ContentType = "application/pdf",
                FileContent = piDocument.FileContent
            };
        }

        private async Task<PIDocumentDataDto> GetPIDocumentDataAsync(Guid policyId)
        {
            var query = new GetPIDocumentDataQuery { PolicyId = policyId };

            return await mediator.Send(query);
        }

        private async Task<BIDocument> PrintPIDocumentAsync(PIDocumentDataDto piDocumentData, CancellationToken cancellationToken)
        {
            var request = mapper.Map<GenerateBIDocumentRequest>(piDocumentData);

            var response = await quotationEngineApiClient.PostAsBsonAsync("vul/pi-documents", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await quotationEngineApiClient.ReadBsonContentAsync<BIDocument>(response, cancellationToken);
        }

        [HttpGet]
        [Route("{id}/documents/welcome-letter-and-policy-schedules")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WelcomeLetterAndPolicyScheduleDocuments))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintWelcomeLetterAndPolicyScheduleDocuments([FromQuery] PrintWelcomeLetterAndPolicySchedulesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var query = mapper.Map<GetWelcomeLetterAndSchedulesDocumentDataQuery>(request);
                var documentData = await mediator.Send(query, cancellationToken);

                var documents = await Task.WhenAll(
                    PrintDocument(TemplateIds.VulWelcomeLetter, documentData.WelcomeLetter),
                    PrintDocument(TemplateIds.VulPolicySchedule1, documentData.PolicySchedule1),
                    PrintDocument(TemplateIds.VulPolicySchedule2, documentData.PolicySchedule2)
                    );

                var now = DateTime.UtcNow.ToSingaporeTime().ToString("yyyy-MM-dd HH-mm-ss");

                var documentsContract = new WelcomeLetterAndPolicyScheduleDocuments
                {
                    PolicyId = request.PolicyId,
                    PolicyNumber = documentData.WelcomeLetter.PolicyNumber,
                    WelcomeLetter = new FileContract
                    {
                        FileName = $"{documentData.WelcomeLetter.PolicyNumber} Welcome Letter {now}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documents[0]
                    },
                    PolicySchedule1 = new FileContract
                    {
                        FileName = $"{documentData.WelcomeLetter.PolicyNumber} Policy Schedule 1 {now}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documents[1]
                    },
                    PolicySchedule2 = new FileContract
                    {
                        FileName = $"{documentData.WelcomeLetter.PolicyNumber} Policy Schedule 2 {now}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documents[2]
                    }
                };

                return Ok(documentsContract);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}/documents/charges-letter")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintChargesLetter([FromQuery] PrintChargesLetterDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetChargesLetterDocumentDataQuery { PolicyId = request.PolicyId };
                var data = await mediator.Send(query, cancellationToken);

                var chargesLetterRequest = new DocumentRequestData
                {
                    TemplateId = TemplateIds.VulChargeLetter,
                    TemplateVersion = TemplateIds.TemplateVersion,
                    EncryptionKey = string.Empty,
                    TemplateData = data
                };

                var response = await documentApiClient.PostAsJsonAsync("", chargesLetterRequest, cancellationToken);
                response.EnsureSuccessStatusCode();

                var documentContent = await response.Content.ReadAsByteArrayAsync();

                var now = DateTime.UtcNow.ToSingaporeTime();
                var documentContract = new FileContract
                {
                    FileName = $"{data.PolicyNumber} Charges Letter {now:yyyy-MM-dd HH-mm-ss}.pdf",
                    ContentType = "application/pdf",
                    FileContent = documentContent
                };

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}/documents/policy-statement")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPolicyStatement([FromQuery] PrintPolicyStatementRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await PrintDocumentAsync();
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }

            async Task<IActionResult> PrintDocumentAsync()
            {
                var query = new GetPolicyStatementDocumentDataQuery { PolicyId = request.PolicyId };
                var documentData = await mediator.Send(query, cancellationToken);

                var documentContent = await PrintDocument(TemplateIds.VulPolicyStatement, documentData);
                var documentContract = CreateDocumentContract();

                return Ok(documentContract);

                FileContract CreateDocumentContract()
                {
                    var now = DateTime.UtcNow.ToSingaporeTime();

                    return new FileContract
                    {
                        FileName = $"{documentData.PolicyNumber} Policy Statement {now:yyyy-MM-dd HH-mm-ss}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documentContent
                    };
                }
            }
        }

        [HttpPut]
        [Route("{id}/special-quote")]
        [CorrelatedAuditApi("Policy:EditSpecialQuoteFactors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SaveSpecialQuoteFactors(
            SaveSpecialQuoteFactorsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<SaveSpecialQuoteFactorsCommand>(request);

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("policy-details/excel")]
        [CorrelatedAuditApi("Policy:ExportPolicyDetails")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportPolicyDetails(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                return await ExportPolicyDetails();
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            async Task<IActionResult> ExportPolicyDetails()
            {
                var query = new GetExportedPolicyDetailsQuery { PolicyId = policyId };

                var exportPolicyDetailsCommand = new ExportPolicyDetailsCommand
                {
                    PolicyDetails = await mediator.Send(query, cancellationToken),
                    TemplateFileContent = ApplicationSettings.V1.VulPolicyDetailsTemplate
                };

                var file = await mediator.Send(exportPolicyDetailsCommand);

                return Ok(mapper.Map<FileContract>(file));
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountValuesRecalculationRecordResult))]
        [Route("get-account-values-recalculation-record")]
        public async Task<IActionResult> GetAccountValuesRecalculationRecordByPolicyNumber([FromQuery] GetAccountValuesRecalculationRecordRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetAccountValuesRecalculationRecordsQuery>(request);
            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<AccountValuesRecalculationRecordResult>(result));
        }

        [HttpPut]
        [Route("account-values")]
        [CorrelatedAuditApi("Policy:RecalculateAccountValues")]
        public async Task<IActionResult> RecalculateAccountValues(RecalculateAccountValuesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<RecalculateAccountValuesWithSpecificDateCommand>(request);
            command.Product = Products.Vul;

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut]
        [Route("{policyId}/policy-account-values")]
        [CorrelatedAuditApi("Policy:RefreshPolicyAccountValues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshPolicyAccountValues(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                var command = new RefreshPolicyAccountValuesCommand
                {
                    PolicyId = policyId
                };

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private async Task<byte[]> PrintDocument(string templateId, object templateData)
        {
            var printRequest = new DocumentRequestData()
            {
                TemplateId = templateId,
                TemplateVersion = TemplateIds.TemplateVersion,
                EncryptionKey = string.Empty,
                TemplateData = templateData
            };

            var response = await documentApiClient.PostAsJsonAsync("", printRequest);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}