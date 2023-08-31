using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.VulAccountValuesImports;
using SingLife.ULTracker.WebAPI.Contracts.VulAccountValuesImports;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/vul-account-values")]
    public class VulAccountValuesImportsController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public VulAccountValuesImportsController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("imported-files")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountValuesImportFile[]))]
        public async Task<IActionResult> GetAccountValuesImportFiles()
        {
            var query = new GetAccountValuesFilesQuery();
            var accountValuesFilesDto = await mediator.Send(query);

            return Ok(mapper.Map<AccountValuesImportFile[]>(accountValuesFilesDto));
        }

        [HttpPost]
        public async Task<IActionResult> ImportAccountValues([FromBody] ImportAccountValuesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<ImportAccountValuesCommand>(request);
            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [Route("imported-files/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAccountValuesExcelFile(Guid id, CancellationToken cancellationToken)
        {
            var command = new ExportAccountValuesAsExcelFileCommand()
            {
                Id = id
            };

            var fileDto = await mediator.Send(command, cancellationToken);
            if (fileDto == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<FileContract>(fileDto));
        }
    }
}