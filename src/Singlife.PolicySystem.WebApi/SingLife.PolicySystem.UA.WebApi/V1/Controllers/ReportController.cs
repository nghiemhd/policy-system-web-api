using MediatR;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.UA.UseCases.Reports.Accounting;
using SingLife.PolicySystem.UA.UseCases.Reports.ActuarialTransaction;
using SingLife.PolicySystem.UA.UseCases.Reports.Lia;
using SingLife.PolicySystem.UA.UseCases.Reports.Transaction;
using SingLife.PolicySystem.UA.UseCases.Reports.Valuation;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Accounting;
using SingLife.ULTracker.WebAPI.Contracts.Reports.ActuarialTransaction;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Lia;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Transaction;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Valuation;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.UA.WebApi.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/sa-reports")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator mediator;

        public ReportController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [Route("valuation")]
        [CorrelatedAuditApi("Report:ExportValuationReport")]
        public async Task<IActionResult> ExportValuationReport([FromBody] ExportValuationReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGenerateValuationReportCommand
            {
                ReportDate = request.Month,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("accounting")]
        [CorrelatedAuditApi("Report:ExportSAAccountingReport")]
        public async Task<IActionResult> ExportAccountingReport([FromBody] ExportSAAccountingReportRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGenerateAccountingReportCommand
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("ifrs17-valuation")]
        [CorrelatedAuditApi("Report:ExportIfrs17ValuationReport")]
        public async Task<IActionResult> ExportIfrs17ValuationReport([FromBody] ExportIfrs17ValuationReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGenerateIfrs17ValuationReportCommand
            {
                ReportDate = request.Month,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("lia")]
        [CorrelatedAuditApi("Report:ExportLiaReport")]
        public async Task<IActionResult> ExportLiaReport([FromBody] ExportLiaReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGenerateLiaReportByChannelCommand
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Channels = ExportLiaByValues.Channels,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("transaction")]
        [CorrelatedAuditApi("Report:ExportTransactionReport")]
        public async Task<IActionResult> ExportTransactionReport([FromBody] ExportTransactionReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGenerateTransactionReportCommand
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("actuarial-transaction")]
        [CorrelatedAuditApi("Report:ExportActuarialTransactionReport")]
        public async Task<IActionResult> ExportActuarialTransactionReport([FromBody] ExportActuarialTransactionReportRequest request, CancellationToken cancellationToken)
        {
            var command = new NotifyGenerateActuarialTransactionReportCommand
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}