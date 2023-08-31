using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.VulEnhanced.Infrastructure.Documents;
using SingLife.PolicySystem.VulEnhanced.UseCases.Documents;
using SingLife.PolicySystem.VulEnhanced.UseCases.Policies;
using SingLife.PolicySystem.VulEnhanced.UseCases.PolicyDetails;
using SingLife.PolicySystem.VulEnhanced.WebApi.V1;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.SystemTime;
using SingLife.ULTracker.Shared;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.PolicyAccountValues;
using SingLife.ULTracker.WebAPI.Contracts.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.VulEnhanced.Documents;
using SingLife.ULTracker.WebAPI.Contracts.VulEnhanced.Policies;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;
using PolicyIdentityContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.PolicyIdentity;

namespace SingLife.PolicySystem.VulEnhanced.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/vul-enhanced-policies")]
    public class VulEnhancedPolicyController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly DocumentGenerationService documentGenerationService;

        public VulEnhancedPolicyController(
            IMediator mediator,
            IMapper mapper,
            DocumentGenerationService documentGenerationService)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.documentGenerationService = documentGenerationService;
        }

        [HttpPost]
        [CorrelatedAuditApi("Policy:Create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyIdentityContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePolicyRequest request, CancellationToken cancellationToken)
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
                var command = mapper.Map<CreatePolicyCommand>(request);
                command.UniqueCorrelationId = this.GetUniqueCorrelationId();
                command.AuthenticatedUser = this.GetAuthenticatedUser();

                var policyIdentity = await this.mediator.Send(command, cancellationToken);

                var policyIdentityContract = mapper.Map<PolicyIdentityContract>(policyIdentity);

                this.SetAuditCorrelationId(policyIdentityContract.PolicyId.ToString());

                return Created($"vul-enhanced-policies/{policyIdentityContract.PolicyId}", policyIdentityContract);
            }
        }

        [HttpPut]
        [CorrelatedAuditApi("Policy:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit([FromBody] EditPolicyRequest request, CancellationToken cancellationToken)
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
                var command = mapper.Map<EditPolicyCommand>(request);
                command.UniqueCorrelationId = this.GetUniqueCorrelationId();
                command.AuthenticatedUser = this.GetAuthenticatedUser();

                await this.mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VulEnhancedPolicy))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetPolicyQuery { PolicyId = id };
            var policyDto = await mediator.Send(query, cancellationToken);

            if (policyDto == null)
                return NotFound();

            return Ok(mapper.Map<VulEnhancedPolicy>(policyDto));
        }

        [HttpPut]
        [Route("{id}/special-quote")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [CorrelatedAuditApi("Policy:EditSpecialQuoteFactors")]
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
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        private async Task<FileContract> PrintPIDocumentAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var piDocumentData = await GetPIDocumentDataAsync(policyId, cancellationToken);
            var piDocument = await documentGenerationService.GeneratePI(piDocumentData, User.Identity.Name, cancellationToken);

            return new FileContract
            {
                FileName = $"Policy Illustration {piDocumentData.PolicyNumber}.pdf",
                ContentType = "application/pdf",
                FileContent = piDocument
            };
        }

        private async Task<PIDocumentDataDto> GetPIDocumentDataAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetPIDocumentDataQuery { PolicyId = policyId };
            return await mediator.Send(query, cancellationToken);
        }

        [HttpGet]
        [Route("{policyId:guid}/documents/welcome-letter-and-policy-schedules")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WelcomeLetterAndPolicyScheduleDocuments))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintWelcomeLetterAndPolicySchedules(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetWelcomeLetterAndSchedulesDocumentDataQuery { PolicyId = policyId };
                var documentData = await mediator.Send(query, cancellationToken);

                var documents = await Task.WhenAll(
                    documentGenerationService.GenerateWelcomeLetter(documentData.WelcomeLetter, User.Identity.Name, cancellationToken),
                    documentGenerationService.GeneratePolicySchedule1(documentData.PolicySchedule1, User.Identity.Name, cancellationToken),
                    documentGenerationService.GeneratePolicySchedule2(documentData.PolicySchedule2, User.Identity.Name, cancellationToken)
                );

                var documentsContract = new WelcomeLetterAndPolicyScheduleDocuments
                {
                    PolicyId = policyId,
                    PolicyNumber = documentData.PolicyNumber,
                    WelcomeLetter = new FileContract
                    {
                        FileName = $"Welcome Letter {documentData.PolicyNumber}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documents[0]
                    },
                    PolicySchedule1 = new FileContract
                    {
                        FileName = $"Policy Schedule 1 {documentData.PolicyNumber}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documents[1]
                    },
                    PolicySchedule2 = new FileContract
                    {
                        FileName = $"Policy Schedule 2 {documentData.PolicyNumber}.pdf",
                        ContentType = "application/pdf",
                        FileContent = documents[2]
                    }
                };

                return Ok(documentsContract);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("{policyId:guid}/documents/charges-letter")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintChargesLetter(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                var document = await PrintChargesLetterAsync();
                return Ok(document);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }

            async Task<FileContract> PrintChargesLetterAsync()
            {
                var query = new GetChargesLetterDocumentDataQuery { PolicyId = policyId };
                var data = await mediator.Send(query, cancellationToken);

                return new FileContract
                {
                    FileName = $"Charges Letter {data.PolicyNumber}.pdf",
                    ContentType = "application/pdf",
                    FileContent = await documentGenerationService.GenerateChargesLetter(data, User.Identity.Name, cancellationToken)
                };
            }
        }

        [HttpGet]
        [Route("{policyId:guid}/documents/terms-of-acceptance")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintTOADocument(Guid policyId, CancellationToken cancellationToken)
        {
            try
            {
                var toaDocument = await PrintTOADocumentAsync();
                return Ok(toaDocument);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }

            async Task<FileContract> PrintTOADocumentAsync()
            {
                var query = new GetTOADocumentDataQuery { PolicyId = policyId };
                var termsOfAcceptanceDocumentData = await mediator.Send(query, cancellationToken);
                var termsOfAcceptanceDocument = await documentGenerationService.GenerateTOADocument(termsOfAcceptanceDocumentData, User.Identity.Name, cancellationToken);

                return new FileContract
                {
                    FileName = $"{termsOfAcceptanceDocumentData.ApplicationNumber} Terms of Acceptance {DateTimeHelper.UtcNow.ToSingaporeTime():yyyy-MM-dd HH-mm-ss}.pdf",
                    ContentType = "application/pdf",
                    FileContent = termsOfAcceptanceDocument
                };
            }
        }

        [HttpPut]
        [Route("account-values")]
        [CorrelatedAuditApi("Policy:RecalculateAccountValues")]
        public async Task<IActionResult> RecalculateAccountValues(RecalculateAccountValuesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<RecalculateAccountValuesWithSpecificDateCommand>(request);
            command.Product = Products.VulEnhanced;

            await mediator.Send(command, cancellationToken);

            return Ok();
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

            async Task<IActionResult> ExportPolicyDetails()
            {
                var query = new GetExportedPolicyDetailsQuery { PolicyId = policyId };

                var exportPolicyDetailsCommand = new ExportPolicyDetailsCommand
                {
                    PolicyDetails = await mediator.Send(query, cancellationToken),
                    TemplateFileContent = Resources.VULEnhancedPolicyDetailsTemplate
                };

                var file = await mediator.Send(exportPolicyDetailsCommand);

                return Ok(mapper.Map<FileContract>(file));
            }
        }
    }
}