using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.Reports.Accounting;
using SingLife.ULTracker.UseCases.Reports.Lia;
using SingLife.ULTracker.UseCases.Reports.ME;
using SingLife.ULTracker.UseCases.Reports.NewBusiness;
using SingLife.ULTracker.UseCases.Reports.PendingNewBusiness;
using SingLife.ULTracker.UseCases.Reports.Valuation;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Accounting;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Lia;
using SingLife.ULTracker.WebAPI.Contracts.Reports.ME;
using SingLife.ULTracker.WebAPI.Contracts.Reports.NewBusiness;
using SingLife.ULTracker.WebAPI.Contracts.Reports.PendingNewBusiness;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Valuation;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public ReportController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("accounting")]
        [CorrelatedAuditApi("Report:ExportAccountingReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportAccountingReport([FromBody] ExportAccountingReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetAccountingReportInputQuery>(request);
            var reportInput = await mediator.Send(query, cancellationToken);

            var command = new ExportAccountingReportCommand
            {
                ReportInput = reportInput,
                TemplateFileContent = ApplicationSettings.V1.AccountingReportTemplate,
                ToMonth = request.ToMonth
            };
            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }

        [HttpPost]
        [Route("accounting/csv")]
        [CorrelatedAuditApi("Report:ExportAccountingReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportAccountingReportInCsv([FromBody] ExportAccountingReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetAccountingReportInputQuery>(request);
            var reportInput = await mediator.Send(query, cancellationToken);

            var command = new ExportAccountingReportInCsvCommand
            {
                ReportInput = reportInput,
                ToMonth = request.ToMonth
            };
            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }

        [HttpPost]
        [Route("valuation")]
        [CorrelatedAuditApi("Report:ExportValuationReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportValuationReport([FromBody] ExportValuationReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var command = new ExportValuationReportCommand
            {
                ReportDate = request.Month,
                TemplateFileContent = ApplicationSettings.V1.ValuationReportTemplate
            };

            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }

        [HttpPost]
        [Route("me")]
        [CorrelatedAuditApi("Report:ExportMeReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportMeReport([FromBody] ExportMEReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetMEReportInputQuery>(request);
            var reportInput = await mediator.Send(query);

            var command = new ExportMEReportCommand
            {
                ReportInput = reportInput,
                TemplateFileContent = ApplicationSettings.V1.MeReportTemplate
            };

            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }

        [HttpPost]
        [Route("new-business")]
        [CorrelatedAuditApi("Report:ExportNewBusinessReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportNewBusinessReport([FromBody] ExportNewBusinessReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var query = new GetNewBusinessReportQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Product = request.Product,
                PolicyStatuses = request.PolicyStatuses
            };

            var reportInput = await mediator.Send(query);

            var command = new ExportNewBusinessReportCommand
            {
                ReportInput = reportInput,
                TemplateFileContent = ApplicationSettings.V1.NewBusinessReportTemplate
            };
            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }

        [HttpPost]
        [Route("pending-new-business")]
        [CorrelatedAuditApi("Report:ExportPendingNewBusinessReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportPendingNewBusinessReport([FromBody] ExportPendingNewBusinessReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var query = new GetPendingNewBusinessReportQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
            };
            var reportInput = await mediator.Send(query);

            var command = new ExportPendingNewBusinessReportCommand
            {
                ReportInput = reportInput,
                TemplateFileContent = ApplicationSettings.V1.PendingNewBusinessReportTemplate
            };
            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }

        [HttpPost]
        [Route("lia")]
        [CorrelatedAuditApi("Report:ExportLiaReport")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportLiaReport([FromBody] ExportLiaReportToExcelRequest request, CancellationToken cancellationToken)
        {
            var query = new GetLiaReportQuery
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
            };

            var reportData = await mediator.Send(query, cancellationToken);

            var command = new ExportLiaReportCommand
            {
                ReportData = reportData,
                TemplateFileContent = ApplicationSettings.V1.LiaReportTemplate
            };
            var reportFileDto = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<FileContract>(reportFileDto));
        }
    }
}