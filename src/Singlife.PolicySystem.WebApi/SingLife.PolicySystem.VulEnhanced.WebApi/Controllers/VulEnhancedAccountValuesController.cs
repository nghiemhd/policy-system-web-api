using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.VulEnhanced.UseCases.AccountValues;
using SingLife.ULTracker.WebAPI.Contracts.VulEnhanced.AccountValues;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.VulEnhanced.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/vul-enhanced-account-values")]
    public class VulEnhancedAccountValuesController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public VulEnhancedAccountValuesController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("imported-files")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustodianResult))]
        public async Task<IActionResult> GetAccountValuesImportFiles([FromQuery] GetCustodiansRequest request)
        {
            var query = mapper.Map<GetCustodiansQuery>(request);
            var accountValuesFilesDto = await mediator.Send(query);
            var accountValuesFiles = mapper.Map<CustodianResult>(accountValuesFilesDto);

            return Ok(accountValuesFiles);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImportAccountValuesResult))]
        public async Task<IActionResult> ImportAccountValues([FromBody] ImportAccountValuesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<ImportAccountValuesCommand>(request);
            var result = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<ImportAccountValuesResult>(result));
        }

        [HttpGet]
        [Route("imported-files/{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ULTracker.WebAPI.Contracts.Common.File))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAccountValuesExcelFile(Guid id, string key, CancellationToken cancellationToken)
        {
            var command = new GetCustodianFileQuery()
            {
                Id = id,
                Key = key
            };

            var fileDto = await mediator.Send(command, cancellationToken);
            if (fileDto == null)
            {
                return NotFound();
            }

            var excelFile = mapper.Map<ULTracker.WebAPI.Contracts.Common.File>(fileDto);

            return Ok(excelFile);
        }
    }
}