using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.VulEnhanced.UseCases.Transactions;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.SystemTime;
using SingLife.ULTracker.Model.Transactions;
using SingLife.ULTracker.UseCases.DocumentGeneration.V2;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.PolicySystem.VulEnhanced.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/vul-enhanced-transactions")]
    public class VulEnhancedTransactionsController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IDocumentGenerationClient documentGenerationClient;

        public VulEnhancedTransactionsController(IMediator mediator, IDocumentGenerationClient documentGenerationClient)
        {
            this.mediator = mediator;
            this.documentGenerationClient = documentGenerationClient;
        }

        [HttpGet]
        [Route("{transactionId:guid}/receipt/pdf")]
        [CorrelatedAuditApi("Transaction:PrintReceipt")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PrintReceipt(Guid transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var receiptData = await GetReceiptDocumentDataAsync(transactionId);
                var receipt = await PrintReceiptAsync(receiptData, cancellationToken);

                return Ok(receipt);
            }
            catch (TransactionNotFoundException)
            {
                return NotFound();
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        private async Task<ReceiptDocumentDataDto> GetReceiptDocumentDataAsync(Guid transactionId)
        {
            var query = new GetReceiptDocumentDataQuery()
            {
                TransactionId = transactionId
            };

            return await mediator.Send(query);
        }

        private async Task<FileContract> PrintReceiptAsync(ReceiptDocumentDataDto documentData, CancellationToken cancellationToken)
        {
            var request = new DocumentGenerationRequest
            {
                TemplateId = TemplateIds.VulEnhanced.Receipt,
                FileType = FileType.Pdf,
                ResponseType = ResponseType.URL,
                TemplateData = documentData,
                UserName = User.Identity.Name
            };

            var response = await documentGenerationClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var downloadUrl = await documentGenerationClient.GetDownloadUrlAsync(response, cancellationToken);

            return new FileContract
            {
                FileName = $"Receipt Document-{DateTimeHelper.UtcNow.ToSingaporeTime():yyyy-MM-dd HH-mm-ss}.pdf",
                ContentType = "application/pdf",
                FileContent = await documentGenerationClient.DownloadDocumentAsync(downloadUrl)
            };
        }
    }
}