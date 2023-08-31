using Autofac.Features.AttributeFilters;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Singlife.ULTracker.WebAPI.Infrastructure;
using SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1.Common;
using SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.UL;
using SingLife.PolicySystem.Shared.ApiClient;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Shared;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.Policies;
using SingLife.ULTracker.UseCases.PolicyAccountValues;
using SingLife.ULTracker.UseCases.Transactions;
using SingLife.ULTracker.UseCases.UL.V1.Documents;
using SingLife.ULTracker.UseCases.UL.V1.PolicyDetails;
using SingLife.ULTracker.WebAPI.Contracts.Common.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Policies.Documents;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;
using PolicyIdentityContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.PolicyIdentity;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/policy")]
    public class PolicyController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly WebApiClient documentApiClient;
        private readonly WebApiClient quotationEngineApiClient;

        public PolicyController(
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyIdentityContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [CorrelatedAuditApi("Policy:Create")]
        public async Task<IActionResult> Create(Policy policy, CancellationToken cancellationToken)
        {
            try
            {
                return await CreatePolicy();
            }
            catch (DuplicatePolicyNumberException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> CreatePolicy()
            {
                var createPolicyCommand = new CreatePolicyCommand
                {
                    Policy = mapper.Map<PolicyDTO>(policy),
                    UniqueCorrelationId = this.GetUniqueCorrelationId(),
                    AuthenticatedUser = this.GetAuthenticatedUser()
                };

                var policyIdentity = await this.mediator.Send(createPolicyCommand, cancellationToken);

                var policyIdentityContract = mapper.Map<PolicyIdentityContract>(policyIdentity);

                this.SetAuditCorrelationId(policyIdentityContract.PolicyId.ToString());

                return Created($"policy/{policyIdentityContract.PolicyId}", policyIdentityContract);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Policy))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var getPolicyQuery = new GetPolicyQuery
            {
                PolicyId = id
            };

            var policyDto = await mediator.Send(getPolicyQuery, cancellationToken);

            return policyDto == null
                ? NotFound()
                : Ok(mapper.Map<Policy>(policyDto));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Policy))]
        public async Task<IActionResult> GetByPolicyNumber(string policyNumber, CancellationToken cancellationToken)
        {
            var getPolicyQuery = new GetPolicyQuery
            {
                PolicyNumber = policyNumber
            };

            var policy = await mediator.Send(getPolicyQuery, cancellationToken);

            return Ok(mapper.Map<Policy>(policy));
        }

        [HttpPut]
        [CorrelatedAuditApi("Policy:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditPolicy([FromBody] Policy policy, CancellationToken cancellationToken)
        {
            try
            {
                return await EditPolicy();
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicatePolicyNumberException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> EditPolicy()
            {
                var command = new EditPolicyCommand()
                {
                    ModifiedPolicy = mapper.Map<PolicyDTO>(policy),
                    UniqueCorrelationId = this.GetUniqueCorrelationId(),
                    AuthenticatedUser = this.GetAuthenticatedUser()
                };

                await this.mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("{id}/documents/welcome-letter-and-schedules")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WelcomeLetterAndScheduleDocuments))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintWelcomeLetterAndSchedule12([FromQuery] PrintWelcomeLetterAndSchedulesDocumentsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await PrintDocumentsAsync();
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            async Task<IActionResult> PrintDocumentsAsync()
            {
                var query = mapper.Map<GetWelcomeLetterAndSchedulesDocumentDataQuery>(request);
                var documentData = await mediator.Send(query, cancellationToken);

                var documents = await Task.WhenAll(
                    PrintWelcomeLetter(documentData.WelcomeLetter, documentData.Version),
                    PrintPolicySchedule1(documentData.PolicySchedule1, documentData.Version),
                    PrintPolicySchedule2(documentData.PolicySchedule2, documentData.Version));

                var documentsContract = BuildDocumentContract();

                return Ok(documentsContract);

                WelcomeLetterAndScheduleDocuments BuildDocumentContract()
                {
                    var now = DateTime.UtcNow.ToSingaporeTime().ToString("yyyy-MM-dd HH-mm-ss");

                    return new WelcomeLetterAndScheduleDocuments()
                    {
                        PolicyId = request.PolicyId,
                        PolicyNumber = documentData.WelcomeLetter.PolicyNumber,
                        WelcomeLetterDocument = new FileContract()
                        {
                            FileName = $"{documentData.WelcomeLetter.PolicyNumber} Welcome Letter {now}.pdf",
                            ContentType = "application/pdf",
                            FileContent = documents[0]
                        },
                        PolicySchedule1 = new FileContract()
                        {
                            FileName = $"{documentData.WelcomeLetter.PolicyNumber} Policy Schedule 1 {now}.pdf",
                            ContentType = "application/pdf",
                            FileContent = documents[1]
                        },
                        PolicySchedule2 = new FileContract()
                        {
                            FileName = $"{documentData.WelcomeLetter.PolicyNumber} Policy Schedule 2 {now}.pdf",
                            ContentType = "application/pdf",
                            FileContent = documents[2]
                        }
                    };
                }
            }
        }

        private Task<byte[]> PrintWelcomeLetter(WelcomeLetterData data, string version)
        {
            if (version == Products.ULVersions.ULSeriesOne)
            {
                return PrintDocument(TemplateIds.ULSeriesOneWelcomeLetter, data);
            }
            else
            {
                return PrintDocument(TemplateIds.ULWelcomeLetter, data);
            }
        }

        private Task<byte[]> PrintPolicySchedule1(PolicySchedule1Data data, string version)
        {
            if (version == Products.ULVersions.ULSeriesOne)
            {
                return PrintDocument(TemplateIds.ULSeriesOnePolicySchedule1, data);
            }
            else
            {
                return PrintDocument(TemplateIds.ULPolicySchedule1, data);
            }
        }

        private Task<byte[]> PrintPolicySchedule2(PolicySchedule2Data data, string version)
        {
            if (version == Products.ULVersions.ULSeriesOne)
            {
                return PrintDocument(TemplateIds.ULSeriesOnePolicySchedule2, data);
            }
            else
            {
                return PrintDocument(TemplateIds.ULPolicySchedule2, data);
            }
        }

        private async Task<byte[]> PrintDocument(string templateId, object templateData)
        {
            var request = new DocumentRequestData()
            {
                TemplateId = templateId,
                TemplateVersion = TemplateIds.TemplateVersion,
                EncryptionKey = string.Empty,
                TemplateData = templateData
            };

            var response = await documentApiClient.PostAsJsonAsync("", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        [HttpGet]
        [Route("{id}/documents/policy-statement")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPolicyStatement([FromQuery] PrintPolicyStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var documentContract = await PrintPolicyStatementInternal(request, cancellationToken);

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidPolicyStatementPeriodException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        private async Task<FileContract> PrintPolicyStatementInternal(PrintPolicyStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            var policyStatementDocumentData = await GetPolicyStatementDocumentData(request, cancellationToken);
            var documentContent = await PrintPolicyStatement(policyStatementDocumentData);

            return CreateDocumentContract(policyStatementDocumentData.PolicySummary.PolicyNumber, documentContent);
        }

        private async Task<PolicyStatementDocumentData> GetPolicyStatementDocumentData(
            PrintPolicyStatementDocumentRequest request,
            CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetPolicyStatementDocumentDataQuery>(request);

            return await mediator.Send(query, cancellationToken);
        }

        private async Task<byte[]> PrintPolicyStatement(PolicyStatementDocumentData documentData)
        {
            var templateId = DetermineDocumentTemplate();

            return await PrintDocument(templateId, documentData);

            string DetermineDocumentTemplate() => documentData.Version == Products.ULVersions.ULSeriesOne
                ? TemplateIds.ULSeriesOnePolicyStatement
                : TemplateIds.ULPolicyStatement;
        }

        private FileContract CreateDocumentContract(string policyNumber, byte[] fileContent)
        {
            var now = DateTime.UtcNow.ToSingaporeTime();

            return new FileContract
            {
                FileName = $"{policyNumber} Policy Statement {now:yyyy-MM-dd HH-mm-ss}.pdf",
                ContentType = "application/pdf",
                FileContent = fileContent
            };
        }

        [HttpGet]
        [Route("{id}/documents/terms-of-acceptance")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPolicyTermsOfAcceptance([FromQuery] PrintTermsOfAcceptanceDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var termsOfAcceptanceDocument = await PrintTermsOfAcceptanceDocumentAsync();

                return Ok(termsOfAcceptanceDocument);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            async Task<FileContract> PrintTermsOfAcceptanceDocumentAsync()
            {
                var query = mapper.Map<GetTermsOfAcceptanceDocumentDataQuery>(request);
                var documentData = await mediator.Send(query, cancellationToken);

                var fileContent = documentData.Version == Products.ULVersions.ULSeriesOne
                    ? await PrintDocument(TemplateIds.ULSeriesOneTermsOfAcceptance, documentData)
                    : await PrintDocument(TemplateIds.ULTermsOfAcceptance, documentData);

                var now = DateTime.UtcNow.ToSingaporeTime();
                var fileName = $"{documentData.PolicyNumber} Terms of Acceptance {now:yyyy-MM-dd HH-mm-ss}.pdf";

                return new FileContract
                {
                    FileName = fileName,
                    ContentType = "application/pdf",
                    FileContent = fileContent
                };
            }
        }

        [HttpGet]
        [Route("{id}/documents/commission-statement")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintCommissionStatement([FromQuery] PrintCommissionStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var query = mapper.Map<GetCommissionStatementDocumentDataQuery>(request);
                var documentData = await mediator.Send(query, cancellationToken);

                var documentContent = documentData.Version == Products.ULVersions.ULSeriesOne
                    ? await PrintDocument(TemplateIds.ULSeriesOneCommissionStatement, documentData)
                    : await PrintDocument(TemplateIds.ULCommissionStatement, documentData);

                var documentContract = CreateCommissionStatementDocumentContract(documentData, documentContent);

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private FileContract CreateCommissionStatementDocumentContract(CommissionStatementDocumentData documentData, byte[] documentContent)
        {
            var now = DateTime.UtcNow.ToSingaporeTime();

            return new FileContract
            {
                FileName = $"{documentData.PolicyNumber} Commission Statement {now:yyyy-MM-dd HH-mm-ss}.pdf",
                ContentType = "application/pdf",
                FileContent = documentContent
            };
        }

        [HttpPost]
        [Route("policies-exports")]
        [CorrelatedAuditApi("Policy:ExportPolicies")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportPolicies([FromBody] ExportPoliciesRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetPoliciesForExportQuery>(request);
            var policies = await mediator.Send(query, cancellationToken);

            var command = new ExportPolicyListCommand
            {
                TemplateFileContent = ApplicationSettings.V1.ExportPoliciesTemplateFileContent,
                Policies = policies
            };

            var fileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(fileDto));
        }

        [HttpGet]
        [Route("policy-details/excel")]
        [CorrelatedAuditApi("Policy:ExportPolicyDetails")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportPolicyDetails(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                return await ExportPolicyDetails();
            }
            catch (PolicyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AccountValueNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> ExportPolicyDetails()
            {
                var getPolicyDetailsQuery = new GetExportedPolicyDetailsQuery { PolicyId = policyId };

                var exportPolicyDetailsCommand = new ExportPolicyDetailsCommand
                {
                    PolicyDetails = await mediator.Send(getPolicyDetailsQuery, cancellationToken),
                    TemplateFileContent = ApplicationSettings.V1.ULPolicyDetailsTemplate
                };

                var file = await mediator.Send(exportPolicyDetailsCommand);

                return Ok(mapper.Map<FileContract>(file));
            }
        }

        [HttpPost]
        [Route("crediting-rate-calculation-strategy")]
        [CorrelatedAuditApi("Policy:ChangeCreditingRateCalculationStrategy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeCreditingRateCalculationStrategy(
            [FromBody] ChangeCreditingRateCalculationStrategyRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await ChangeCreditingRateCalculationStrategy();
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }

            return Ok();

            async Task ChangeCreditingRateCalculationStrategy()
            {
                var command = new ChangeCreditingRateCalculationStrategyCommand
                {
                    PolicyId = request.PolicyId,
                    NewStrategy = (UseCases.Policies.CreditingRateCalculationStrategy)request.NewStrategy
                };

                await mediator.Send(command, cancellationToken);
            }
        }

        [HttpPut]
        [Route("{id}/special-quote")]
        [CorrelatedAuditApi("Policy:EditSpecialQuoteFactors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SaveSpecialQuoteFactors(
            [FromBody] SaveSpecialQuoteFactorsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<SaveSpecialQuoteFactorsCommand>(request);

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("get-account-values-recalculation-record")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountValuesRecalculationRecordResult))]
        public async Task<IActionResult> GetAccountValuesRecalculationRecordByPolicyNumber([FromQuery] GetAccountValuesRecalculationRecordRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetAccountValuesRecalculationRecordsQuery>(request);
            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<AccountValuesRecalculationRecordResult>(result));
        }

        [HttpPut]
        [Route("account-values")]
        [CorrelatedAuditApi("AccountValue:Recalculate")]
        public async Task<IActionResult> RecalculateAccountValues([FromBody] RecalculateAccountValuesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<RecalculateAccountValuesWithSpecificDateCommand>(request);
            command.Product = Products.UL;

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut]
        [Route("{policyId}/policy-account-values")]
        [CorrelatedAuditApi("AccountValue:Refresh")]
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
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{policyId}/documents/charges")]
        [CorrelatedAuditApi("Document:PrintPolicyChargesDocument")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPolicyChargesDocument(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                var document = await PrintPolicyChargesDocumentInternal(policyId, cancellationToken);

                return Ok(document);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private async Task<FileContract> PrintPolicyChargesDocumentInternal(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetPolicyChargesDocumentDataQuery { PolicyId = policyId };
            var documentData = await mediator.Send(query, cancellationToken);

            var document = await GenerateChargesDocumentAsync(documentData);

            return new FileContract
            {
                FileName = $"Policy Charges {documentData.PolicyNumber}.pdf",
                ContentType = "application/pdf",
                FileContent = document.Content
            };
        }

        private async Task<File> GenerateChargesDocumentAsync(PolicyChargesDocumentDataDto documentData)
        {
            var request = mapper.Map<GenerateChargesDocumentRequest>(documentData);

            var response = await quotationEngineApiClient.PostAsBsonAsync("ul/charges-document", request);
            response.EnsureSuccessStatusCode();

            return await quotationEngineApiClient.ReadBsonContentAsync<File>(response);
        }

        [HttpGet]
        [Route("{id:guid}/documents/inforce-illustration")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintInforceIllustration([FromQuery] PrintPolicyInforceIllustrationDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.IllustrationDate == null)
                {
                    return BadRequest("Illustration Date can not be null.");
                }

                var document = await PrintInforceIllustrationDocument(request.PolicyId, request.IllustrationDate.Value, cancellationToken);

                return Ok(document);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InitialPremiumTransactionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (PrintInforceIllustrationException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        private async Task<FileContract> PrintInforceIllustrationDocument(Guid policyId, DateTime illustrationDate, CancellationToken cancellationToken)
        {
            var query = new GetInforceIllustrationDocumentDataQuery
            {
                PolicyId = policyId,
                IllustrationDate = illustrationDate
            };

            var documentData = await mediator.Send(query, cancellationToken);

            var document = await PrintInforceIllustrationDocumentAsync(documentData);

            return new FileContract
            {
                FileName = $"Inforce Illustration {documentData.PolicyNumber}.pdf",
                ContentType = "application/pdf",
                FileContent = document
            };
        }

        private async Task<byte[]> PrintInforceIllustrationDocumentAsync(InforceIllustrationDocumentData documentData)
        {
            var request = new DocumentRequestData()
            {
                TemplateId = TemplateIds.ULInforceIllustration,
                TemplateVersion = TemplateIds.TemplateVersion,
                EncryptionKey = string.Empty,
                TemplateData = documentData
            };

            var response = await documentApiClient.PostAsJsonAsync("", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        [HttpGet]
        [Route("{policyId:guid}/documents/pi")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
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
        }

        private async Task<FileContract> PrintPIDocumentAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var piDocumentData = await GetPIDocumentDataAsync(policyId);
            var piDocument = await PrintPIDocumentAsync(piDocumentData, cancellationToken);

            return new FileContract
            {
                FileName = $"Policy Illustration {piDocumentData.PolicyNumber}.pdf",
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

            var response = await quotationEngineApiClient.PostAsBsonAsync("ul/pi-documents", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await quotationEngineApiClient.ReadBsonContentAsync<BIDocument>(response, cancellationToken);
        }
    }
}