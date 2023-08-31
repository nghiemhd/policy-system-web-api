using Autofac.Features.AttributeFilters;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Singlife.ULTracker.WebAPI.Infrastructure;
using SingLife.PolicySystem.Shared.ApiClient;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.Common.Policies.Documents;
using SingLife.ULTracker.Model.Transactions;
using SingLife.ULTracker.UseCases.Transactions;
using SingLife.ULTracker.UseCases.Transactions.DataExport;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Common.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;
using TransactionAttachment = SingLife.ULTracker.WebAPI.Contracts.Transactions.TransactionAttachment;
using TransactionAttachmentDto = SingLife.ULTracker.UseCases.Transactions.TransactionAttachmentDto;
using TransactionContract = SingLife.ULTracker.WebAPI.Contracts.Transactions.Transaction;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly WebApiClient dmsApiClient;

        public TransactionController(
            IMediator mediator,
            IMapper mapper,
            [KeyFilter(WebApiClients.SingLifeDMS)] WebApiClient dmsApiClient)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.dmsApiClient = dmsApiClient;
        }

        [HttpGet]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<TransactionContract>))]
        public async Task<IActionResult> Search([FromQuery] TransactionPagedSearch pagedSearch, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchTransactionsQuery>(pagedSearch);

            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<TransactionContract>>(result));
        }

        [HttpGet]
        [Route("search-by-dates")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<TransactionContract>))]
        public async Task<IActionResult> SearchByDates([FromQuery] TransactionPagedSearch pagedSearch, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchTransactionsByDatesQuery>(pagedSearch);

            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<TransactionContract>>(result));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionContract))]
        public async Task<IActionResult> Get([FromQuery] TransactionRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetTransactionQuery>(request);

            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<TransactionContract>(result));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [CorrelatedAuditApi("Transaction:Create")]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<CreateTransactionCommand>(request);

                var result = await mediator.Send(command, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [CorrelatedAuditApi("Transaction:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateTransactionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<UpdateTransactionCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (TransactionNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpDelete]
        [CorrelatedAuditApi("Transaction:Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromQuery] TransactionRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<DeleteTransactionCommand>(request);

            var result = await mediator.Send(command, cancellationToken);

            return result ? Ok() : BadRequest();
        }

        [HttpGet]
        [Route("{transactionId:guid}/attachments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionAttachment[]))]
        public async Task<IActionResult> GetTransactionAttachments(Guid transactionId, CancellationToken cancellationToken)
        {
            var query = new GetTransactionAttachmentsQuery { TransactionId = transactionId };
            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<TransactionAttachment[]>(result));
        }

        [HttpPost]
        [Route("attachments")]
        [CorrelatedAuditApi("Transaction:AddAttachments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAttachments(CreateTransactionAttachmentsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (RequestNotHaveAnyAttachment())
                    return BadRequest("Request contains no attachment.");

                var command = new CreateTransactionAttachmentsCommand
                {
                    TransactionAttachments = mapper.Map<TransactionAttachmentDto[]>(request.TransactionAttachments),
                    TransactionId = request.TransactionId
                };

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (DuplicateAttachmentException exception)
            {
                return BadRequest(exception.Message);
            }

            bool RequestNotHaveAnyAttachment()
            {
                return (request.TransactionAttachments == null || request.TransactionAttachments.Length == 0);
            }
        }

        [HttpGet]
        [Route("attachments/{attachmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactionAttachment(Guid attachmentId, CancellationToken cancellationToken)
        {
            var query = new GetTransactionAttachmentQuery() { AttachmentId = attachmentId };
            var attachment = await mediator.Send(query, cancellationToken);

            if (attachment == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<FileContract>(attachment));
        }

        [HttpDelete]
        [Route("attachments")]
        [CorrelatedAuditApi("Transaction:DeleteAttachment")]
        public async Task<IActionResult> DeleteAttachment([FromQuery] TransactionAttachmentRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<DeleteTransactionAttachmentCommand>(request);

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [Route("export")]
        [CorrelatedAuditApi("Transaction:Export")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> Export([FromQuery] ExportTransactionRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetTransactionsForExportingQuery>(request);

            var transactions = await mediator.Send(query, cancellationToken);

            var command = new ExportTransactionsDataCommand
            {
                Transactions = transactions,
                ReportTemplate = ApplicationSettings.V1.TransactionReportTemplate
            };

            var result = await mediator.Send(command);

            return Ok(mapper.Map<FileContract>(result));
        }

        [HttpGet]
        [Route("{id}/receipt/pdf")]
        [CorrelatedAuditApi("Transaction:PrintReceipt")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintReceiptDocument(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetReceiptDocumentDataQuery() { TransactionId = id };
                var receiptDocumentData = await mediator.Send(query);

                var documentRequestData = new DocumentRequestData()
                {
                    TemplateId = TemplateIds.Receipt,
                    TemplateVersion = TemplateIds.TemplateVersion,
                    EncryptionKey = string.Empty,
                    TemplateData = receiptDocumentData
                };

                var response = await dmsApiClient.PostAsJsonAsync("", documentRequestData, cancellationToken);
                response.EnsureSuccessStatusCode();
                var documentContent = await response.Content.ReadAsByteArrayAsync();

                var documentContract = new FileContract()
                {
                    FileName = $"Receipt Document-{DateTime.UtcNow.ToSingaporeTime():yyyy-MM-dd HH-mm-ss}.pdf",
                    ContentType = "application/pdf",
                    FileContent = documentContent
                };

                return Ok(documentContract);
            }
            catch (PolicyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPost]
        [Route("{transactionId:guid}/discounts")]
        [CorrelatedAuditApi("TransactionDiscount:Create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransactionDiscount([FromBody] CreateTransactionDiscountRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await CreateTransactionDiscountAsync();
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }

            async Task<IActionResult> CreateTransactionDiscountAsync()
            {
                var command = mapper.Map<CreateTransactionDiscountCommand>(request);
                var result = await mediator.Send(command, cancellationToken);

                return Ok(result);
            }
        }

        [HttpDelete]
        [Route("{transactionId:guid}/discounts")]
        [CorrelatedAuditApi("TransactionDiscount:Delete")]
        public async Task<IActionResult> DeleteTransactionDiscount(DeleteTransactionDiscountRequest request, CancellationToken cancellationToken)
        {
            var command = new DeleteTransactionDiscountCommand()
            {
                TransactionId = request.TransactionId,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut]
        [Route("{transactionId:guid}/discounts")]
        [CorrelatedAuditApi("TransactionDiscount:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTransactionDiscount([FromBody] EditTransactionDiscountRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await UpdateTransactionDiscountAsync();
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }

            async Task<IActionResult> UpdateTransactionDiscountAsync()
            {
                var command = mapper.Map<UpdateTransactionDiscountCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }
    }
}