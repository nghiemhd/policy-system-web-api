using Autofac.Features.AttributeFilters;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Singlife.ULTracker.WebAPI.Infrastructure;
using SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.ULPB;
using SingLife.PolicySystem.Shared.ApiClient;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.Common.Policies.Documents;
using SingLife.ULTracker.Model.Ulpb.V1.Applications;
using SingLife.ULTracker.Model.Underwritings;
using SingLife.ULTracker.Shared;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.Common.Policies.Documents;
using SingLife.ULTracker.UseCases.PolicyAccountValues;
using SingLife.ULTracker.UseCases.Transactions;
using SingLife.ULTracker.UseCases.Ulpb.V1.Applications;
using SingLife.ULTracker.UseCases.Ulpb.V1.Documents;
using SingLife.ULTracker.UseCases.Ulpb.V1.EApplications;
using SingLife.ULTracker.UseCases.Ulpb.V1.Policies;
using SingLife.ULTracker.UseCases.Ulpb.V1.PolicyDetails;
using SingLife.ULTracker.WebAPI.Contracts.Common.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Policies;
using SingLife.ULTracker.WebAPI.Contracts.UlpbPolicies.Documents;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;
using FundingOptions = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.FundingOptions;
using PolicyIdentityContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.PolicyIdentity;
using PrintWelcomeLetterAndSchedulesDocumentsContract = SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Documents.WelcomeLetterAndScheduleDocuments;
using TermsOfAcceptanceDocumentContract = SingLife.ULTracker.WebAPI.Contracts.UlpbPolicies.Documents.TermOfAcceptanceDocument;
using UlpbApplicationChecklist = SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.ApplicationChecklists.UlpbApplicationChecklist;
using UlpbPolicy = SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Policies.UlpbPolicy;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ulpb-policies")]
    public class UlpbPolicyController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly WebApiClient documentApiClient;
        private readonly WebApiClient quotationEngineApiClient;

        public UlpbPolicyController(
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

        [HttpGet]
        [Route("{id}/documents/welcome-letter-and-schedules")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PrintWelcomeLetterAndSchedulesDocumentsContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintUlpbWelcomeLetterAndSchedules([FromQuery] PrintWelcomeLetterAndSchedulesDocumentsRequest request, CancellationToken cancellationToken)
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
                    PrintWelcomeLetter(documentData.WelcomeLetter, cancellationToken),
                    PrintPolicySchedule1(documentData.PolicySchedule1, cancellationToken),
                    PrintPolicySchedule2(documentData.PolicySchedule2, cancellationToken));

                var documentsContract = CreateDocumentContract();

                await SaveUlpbWelcomeLetterAndSchedulesToS3(documentsContract);

                return Ok();

                PrintWelcomeLetterAndSchedulesDocumentsContract CreateDocumentContract()
                {
                    var now = GetCurrentTimeForDocumentName();

                    return new PrintWelcomeLetterAndSchedulesDocumentsContract()
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

        private string GetCurrentTimeForDocumentName() =>
            DateTime.UtcNow.ToSingaporeTime().ToString("yyyy-MM-dd HH-mm-ss");

        private async Task SaveUlpbWelcomeLetterAndSchedulesToS3(PrintWelcomeLetterAndSchedulesDocumentsContract welcomeLetterAndSchedulesDocumentsContract)
        {
            var command = new SavePolicyDocumentsCommand
            {
                Documents = new[]
               {
                    new PolicyDocumentDto
                    {
                        PolicyId = welcomeLetterAndSchedulesDocumentsContract.PolicyId,
                        PolicyNumber = welcomeLetterAndSchedulesDocumentsContract.PolicyNumber,
                        Category = DocumentCategory.Document,
                        FileName = UlpbDocumentNames.WelcomeLetter,
                        FileContent = welcomeLetterAndSchedulesDocumentsContract.WelcomeLetterDocument.FileContent
                    },
                    new PolicyDocumentDto
                    {
                        PolicyId = welcomeLetterAndSchedulesDocumentsContract.PolicyId,
                        PolicyNumber = welcomeLetterAndSchedulesDocumentsContract.PolicyNumber,
                        Category = DocumentCategory.Document,
                        FileName = UlpbDocumentNames.PolicySchedule1,
                        FileContent = welcomeLetterAndSchedulesDocumentsContract.PolicySchedule1.FileContent
                    },
                    new PolicyDocumentDto
                    {
                        PolicyId = welcomeLetterAndSchedulesDocumentsContract.PolicyId,
                        PolicyNumber = welcomeLetterAndSchedulesDocumentsContract.PolicyNumber,
                        Category = DocumentCategory.Document,
                        FileName = UlpbDocumentNames.PolicySchedule2,
                        FileContent = welcomeLetterAndSchedulesDocumentsContract.PolicySchedule2.FileContent
                    }
                }
            };

            await mediator.Send(command);
        }

        private Task<byte[]> PrintWelcomeLetter(WelcomeLetterData data, CancellationToken cancellationToken)
        {
            return PrintDocument(TemplateIds.UlpbWelcomeLetter, data, cancellationToken);
        }

        private Task<byte[]> PrintPolicySchedule1(PolicySchedule1Data data, CancellationToken cancellationToken)
        {
            return PrintDocument(TemplateIds.UlpbPolicySchedule1, data, cancellationToken);
        }

        private Task<byte[]> PrintPolicySchedule2(PolicySchedule2Data data, CancellationToken cancellationToken)
        {
            return PrintDocument(TemplateIds.UlpbPolicySchedule2, data, cancellationToken);
        }

        private async Task<byte[]> PrintDocument(string templateId, object templateData, CancellationToken cancellationToken)
        {
            var request = new DocumentRequestData
            {
                TemplateId = templateId,
                TemplateVersion = TemplateIds.TemplateVersion,
                EncryptionKey = string.Empty,
                TemplateData = templateData
            };

            var response = await documentApiClient.PostAsJsonAsync("", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        [HttpPost]
        [CorrelatedAuditApi("Policy:Create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyIdentityContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreatePolicyRequest request, CancellationToken cancellationToken)
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
                var createPolicyCommand = mapper.Map<CreateUlpbPolicyCommand>(request);
                createPolicyCommand.UniqueCorrelationId = this.GetUniqueCorrelationId();
                createPolicyCommand.AuthenticatedUser = this.GetAuthenticatedUser();

                var policyIdentity = await this.mediator.Send(createPolicyCommand, cancellationToken);

                var policyIdentityContract = mapper.Map<PolicyIdentityContract>(policyIdentity);

                this.SetAuditCorrelationId(policyIdentityContract.PolicyId.ToString());

                return Created($"ulpb-policies/{policyIdentityContract.PolicyId}", policyIdentityContract);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UlpbPolicy))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var policyDto = await GetPolicyByIdAsync(id, cancellationToken);

            if (policyDto == null)
            {
                return NotFound();
            }

            var policyContract = mapper.Map<UlpbPolicy>(policyDto);
            policyContract.ApplicationChecklist = await GetApplicationChecklistAsync(id, cancellationToken);

            return Ok(policyContract);
        }

        [HttpGet]
        [Route("summary/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UlpbPolicySummary))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSummaryById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetPolicySummaryQuery { PolicyId = id };
            var policyDto = await mediator.Send(query, cancellationToken);

            if (policyDto == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<UlpbPolicySummary>(policyDto));
        }

        private Task<QueriedUlpbPolicyDto> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetPolicyQuery { PolicyId = policyId };
            return mediator.Send(query, cancellationToken);
        }

        private async Task<UlpbApplicationChecklist> GetApplicationChecklistAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetApplicationChecklistByPolicyQuery { PolicyId = policyId };
            var applicationChecklistDto = await mediator.Send(query, cancellationToken);

            return mapper.Map<UlpbApplicationChecklist>(applicationChecklistDto);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UlpbPolicy))]
        public async Task<IActionResult> GetByPolicyNumber(string policyNumber, CancellationToken cancellationToken)
        {
            var getPolicyQuery = new GetPolicyQuery
            {
                PolicyNumber = policyNumber,
            };

            var policy = await mediator.Send(getPolicyQuery, cancellationToken);

            return Ok(mapper.Map<UlpbPolicy>(policy));
        }

        [HttpPut]
        [CorrelatedAuditApi("Policy:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditPolicy([FromBody] EditPolicyRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await EditPolicy();
            }
            catch (DuplicatePolicyNumberException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> EditPolicy()
            {
                var command = mapper.Map<EditUlpbPolicyCommand>(request);
                command.UniqueCorrelationId = this.GetUniqueCorrelationId();
                command.AuthenticatedUser = this.GetAuthenticatedUser();

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("{id}/documents/terms-of-acceptance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintUlpbPolicyTermsOfAcceptance([FromQuery] PrintUlpbPolicyTermOfAcceptanceRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var query = mapper.Map<GetTermOfAcceptanceDocumentDataQuery>(request);
                var termsOfAcceptanceDocumentData = await mediator.Send(query, cancellationToken);

                var fileContent = await PrintDocument(TemplateIds.UlpbTermOfAcceptance, termsOfAcceptanceDocumentData, cancellationToken);
                var documentContract = CreateDocumentContract();

                await SaveTermsOfAcceptanceToS3(documentContract, fileContent);

                return Ok();

                TermsOfAcceptanceDocumentContract CreateDocumentContract()
                {
                    return new TermsOfAcceptanceDocumentContract
                    {
                        PolicyId = termsOfAcceptanceDocumentData.PolicyId,
                        PolicyNumber = termsOfAcceptanceDocumentData.PolicyNumber,
                        TermOfAcceptanceFile = new FileContract
                        {
                            FileName = $"{termsOfAcceptanceDocumentData.PolicyNumber} Terms of Acceptance {GetCurrentTimeForDocumentName()}.pdf",
                            ContentType = "application/pdf",
                            FileContent = fileContent
                        }
                    };
                }
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private async Task SaveTermsOfAcceptanceToS3(TermsOfAcceptanceDocumentContract termsOfAcceptanceDocumentContract, byte[] documentContent)
        {
            var command = new SavePolicyDocumentsCommand
            {
                Documents = new[]
               {
                    new PolicyDocumentDto
                    {
                        PolicyId = termsOfAcceptanceDocumentContract.PolicyId,
                        PolicyNumber = termsOfAcceptanceDocumentContract.PolicyNumber,
                        Category = DocumentCategory.Document,
                        FileName = UlpbDocumentNames.TermOfAcceptance,
                        FileContent = documentContent
                    }
                }
            };

            await mediator.Send(command);
        }

        [HttpGet]
        [Route("policy-details/excel")]
        [CorrelatedAuditApi("Policy:ExportPolicyDetails")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            catch (AccountValueNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> ExportPolicyDetails()
            {
                var getPolicyDetailsQuery = new GetExportedPolicyDetailsQuery { PolicyId = policyId };

                var policyDetails = await mediator.Send(getPolicyDetailsQuery, cancellationToken);
                var exportPolicyDetailsCommand = new ExportPolicyDetailsCommand
                {
                    PolicyDetails = policyDetails,
                    TemplateFileContent = ApplicationSettings.V1.UlpbPolicyDetailsTemplate
                };

                var file = await mediator.Send(exportPolicyDetailsCommand);

                return Ok(mapper.Map<FileContract>(file));
            }
        }

        [HttpPatch]
        [Route("issuing-policy")]
        [CorrelatedAuditApi("Policy:IssuePolicy")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> IssuePolicy([FromBody] IssuePolicyRequest request, CancellationToken cancellationToken)
        {
            if (request.ApplicationId == null && request.PolicyId == null)
            {
                return BadRequest("Application Id and Policy Id must have value.");
            }

            var policyId = request.PolicyId ?? await GetPolicyIdFromApplicationAsync(request.ApplicationId.Value, cancellationToken);

            try
            {
                return await IssuePolicyAsync(policyId, cancellationToken);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ApplicationChecklistWasNotDoneException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PolicyHasNoInitialPremiumTransactionException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<Guid> GetPolicyIdFromApplicationAsync(Guid applicationId, CancellationToken cancellationToken)
        {
            var query = new GetPolicyAndApplicationIdentitiesQuery { ApplicationId = applicationId };

            var policyAndApplicationIdentities = await mediator.Send(query, cancellationToken);

            if (policyAndApplicationIdentities == null)
            {
                throw new BadHttpRequestException("Cannot get application from the specified application Id.");
            }

            if (!policyAndApplicationIdentities.PolicyId.HasValue)
            {
                throw new BadHttpRequestException("Cannot get the corresponding policy of application.");
            }

            return policyAndApplicationIdentities.PolicyId.Value;
        }

        private async Task<IActionResult> IssuePolicyAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var command = new IssuePolicyCommand { PolicyId = policyId };

            await mediator.Send(command, cancellationToken);

            return Ok(policyId);
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
        [CorrelatedAuditApi("Policy:RecalculateAccountValues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RecalculateAccountValues(RecalculateAccountValuesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<RecalculateAccountValuesWithSpecificDateCommand>(request);
            command.Product = Products.Ulpb;

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [Route("{id}/documents/policy-statement")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPolicyStatement([FromQuery] PrintUlpbPolicyStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var documentContract = await PrintPolicyStatementInternal(request, cancellationToken);

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (InvalidPolicyStatementPeriodException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        private async Task<FileContract> PrintPolicyStatementInternal(PrintUlpbPolicyStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            var documentData = await GetPolicyStatementDocumentData(request, cancellationToken);
            var document = await PrintDocument(TemplateIds.UlpbPolicyStatement, documentData, cancellationToken);
            return CreatePolicyStatementDocumentContract(documentData.PolicySummary.PolicyNumber, document);
        }

        private async Task<PolicyStatementDocumentData> GetPolicyStatementDocumentData(PrintUlpbPolicyStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetPolicyStatementDocumentDataQuery>(request);
            return await mediator.Send(query, cancellationToken);
        }

        private FileContract CreatePolicyStatementDocumentContract(string policyNumber, byte[] documentContent)
        {
            var fileName = $"{policyNumber} Policy Statement {GetCurrentTimeForDocumentName()}.pdf";

            return new FileContract
            {
                FileName = fileName,
                ContentType = "application/pdf",
                FileContent = documentContent
            };
        }

        [HttpGet]
        [Route("{id}/documents/commission-statement")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintCommissionStatement([FromQuery] PrintUlpbCommissionStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var documentContract = await PrintCommissionStatementInternal(request, cancellationToken);

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
        }

        private async Task<FileContract> PrintCommissionStatementInternal(PrintUlpbCommissionStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            var documentData = await GetCommissionStatementDocumentData(request, cancellationToken);
            var document = await PrintDocument(TemplateIds.UlpbCommissionStatement, documentData, cancellationToken);
            return CreateCommissionStatementDocumentContract(documentData.PolicyNumber, document);
        }

        private async Task<CommissionStatementDocumentData> GetCommissionStatementDocumentData(PrintUlpbCommissionStatementDocumentRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetCommissionStatementDocumentDataQuery>(request);
            return await mediator.Send(query, cancellationToken);
        }

        private FileContract CreateCommissionStatementDocumentContract(string policyNumber, byte[] documentContent)
        {
            var fileName = $"{policyNumber} Commission Statement {GetCurrentTimeForDocumentName()}.pdf";

            return new FileContract
            {
                FileName = fileName,
                ContentType = "application/pdf",
                FileContent = documentContent
            };
        }

        [HttpGet]
        [Route("{id}/documents/pi")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintPI([FromQuery] PrintUlpbPIDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await PerformSavingFinalPIDocument(request.PolicyId);
                return Ok();
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (PrintPIException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task PerformSavingFinalPIDocument(Guid policyId)
        {
            var piDocumentData = await GetFinalPIDocumentData(policyId);
            var piDocument = await GenerateFinalPIDocument(piDocumentData);
            await SavePIPolicyDocumentToS3(policyId, piDocumentData.PolicyNumber, piDocument, piDocumentData.IsFinalPI);
        }

        private async Task<FinalPIDocumentDataDto> GetFinalPIDocumentData(Guid policyId)
        {
            var query = new GetFinalPIDocumentDataQuery { PolicyId = policyId };
            return await mediator.Send(query);
        }

        private async Task<byte[]> GenerateFinalPIDocument(FinalPIDocumentDataDto documentData)
        {
            if (IsLumpSumPremium(documentData.ClientDetails.FundingOption))
            {
                return await GenerateFinalPIDocumentForLumpSum(documentData);
            }

            if (IsSingleAndTopUpPremium(documentData.ClientDetails.FundingOption))
            {
                return await GenerateFinalPIDocumentForTopUp(documentData);
            }

            throw new Exception("Funding option is not valid.");

            bool IsLumpSumPremium(string fundingOption)
            {
                return fundingOption == FundingOptions.LumpSumPremium;
            }

            bool IsSingleAndTopUpPremium(string fundingOption)
            {
                return fundingOption == FundingOptions.TopUpPremium;
            }
        }

        private async Task<byte[]> GenerateFinalPIDocumentForLumpSum(FinalPIDocumentDataDto documentData)
        {
            var request = mapper.Map<GenerateLumpsumPIDocumentRequest>(documentData);

            var response = await quotationEngineApiClient.PostAsBsonAsync("ulpb/pi-documents/lump-sum", request);
            response.EnsureSuccessStatusCode();

            var piDocument = await quotationEngineApiClient.ReadBsonContentAsync<PIDocument>(response);

            if (piDocument.LumpSumPremiumQuotation.HasError)
                throw new PrintPIException(piDocument.LumpSumPremiumQuotation.Errors[0].Message);

            return piDocument.FileContent;
        }

        private async Task<byte[]> GenerateFinalPIDocumentForTopUp(FinalPIDocumentDataDto documentData)
        {
            var request = mapper.Map<GenerateTopUpPIDocumentRequest>(documentData);

            var response = await quotationEngineApiClient.PostAsBsonAsync("ulpb/pi-documents/top-up", request);
            response.EnsureSuccessStatusCode();

            var piDocument = await quotationEngineApiClient.ReadBsonContentAsync<PIDocument>(response);

            if (piDocument.TopUpPremiumQuotation.HasError)
                throw new PrintPIException(piDocument.TopUpPremiumQuotation.Errors[0].Message);

            return piDocument.FileContent;
        }

        private async Task SavePIPolicyDocumentToS3(Guid policyId, string policyNumber, byte[] piDocument, bool isFinalPI)
        {
            var command = CreateSavePIDocumentCommand(policyId, policyNumber, piDocument, isFinalPI);
            await mediator.Send(command);
        }

        private SavePolicyDocumentsCommand CreateSavePIDocumentCommand(Guid policyId, string policyNumber, byte[] piDocumentData, bool isFinalPI)
        {
            var fileName = isFinalPI ? UlpbDocumentNames.FinalPolicyIllustration : UlpbDocumentNames.SubmittedPolicyIllustration;

            return CreateSavePolicyDocumentCommand(policyId, policyNumber, piDocumentData, fileName);
        }

        private SavePolicyDocumentsCommand CreateSavePolicyDocumentCommand(Guid policyId, string policyNumber, byte[] documentData, string fileName)
        {
            return new SavePolicyDocumentsCommand
            {
                Documents = new[]
                {
                    new PolicyDocumentDto
                    {
                        PolicyId = policyId,
                        PolicyNumber = policyNumber,
                        Category = DocumentCategory.Document,
                        FileName = fileName,
                        FileContent = documentData
                    }
                }
            };
        }

        [HttpGet]
        [Route("{id:guid}/documents/inforce-illustration")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintInforceIllustration([FromQuery] PrintUlpbInforceIllustrationDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.IllustrationDate == null)
                    return BadRequest("Illustration Date can not be null.");

                var document = await PrintInforceIllustrationDocumentAsync(request, cancellationToken);

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
        }

        private async Task<FileContract> PrintInforceIllustrationDocumentAsync(PrintUlpbInforceIllustrationDocumentRequest request, CancellationToken cancellationToken)
        {
            var query = new GetInforceIllustrationDocumentDataQuery
            {
                PolicyId = request.PolicyId,
                IllustrationDate = request.IllustrationDate.Value
            };
            var documentData = await mediator.Send(query, cancellationToken);

            var document = await PrintDocument(TemplateIds.UlpbInforceIllustration, documentData, cancellationToken);

            return new FileContract
            {
                FileName = $"Inforce Illustration {documentData.PolicyNumber}.pdf",
                ContentType = "application/pdf",
                FileContent = document
            };
        }

        [HttpGet]
        [Route("{id}/documents/eapplication")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintEApplication([FromQuery] PrintUlpbEApplicationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await PerformSavingEAppDocument(request.PolicyId, cancellationToken);

                return Ok();
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnderwriteMeSectionsNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task PerformSavingEAppDocument(Guid policyId, CancellationToken cancellationToken)
        {
            var eApplicationData = await GetEAppDocumentData(policyId);
            var documentContent = await PrintDocument(TemplateIds.UlpbEApplication, eApplicationData, cancellationToken);

            await SaveEAppDocumentToS3(policyId, eApplicationData.PolicyNumber, documentContent);
        }

        private async Task<QueriedEApplicationDataDto> GetEAppDocumentData(Guid policyId)
        {
            var query = new GetEApplicationDataQuery { PolicyId = policyId };
            return await mediator.Send(query);
        }

        private async Task SaveEAppDocumentToS3(Guid policyId, string policyNumber, byte[] documentContent)
        {
            var command = CreateSaveEAppDocumentCommand(policyId, policyNumber, documentContent);
            await mediator.Send(command);
        }

        private SavePolicyDocumentsCommand CreateSaveEAppDocumentCommand(Guid policyId, string policyNumber, byte[] documentContent)
        {
            var fileName = UlpbDocumentNames.EApplication;
            return CreateSavePolicyDocumentCommand(policyId, policyNumber, documentContent, fileName);
        }

        [HttpPut]
        [Route("{policyId}/policy-account-values")]
        [CorrelatedAuditApi("Policy:RefreshPolicyAccountValues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    }
}