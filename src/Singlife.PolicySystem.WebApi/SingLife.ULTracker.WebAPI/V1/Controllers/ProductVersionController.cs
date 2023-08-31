using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.ProductVersion;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/product-versions")]
    public class ProductVersionController : ControllerBase
    {
        private readonly IMediator mediator;

        public ProductVersionController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("usage")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> CheckWhetherProductVersionIsInUse(string product, string version, CancellationToken cancellationToken)
        {
            var query = new CheckWhetherProductVersionIsInUseQuery
            {
                Product = product,
                Version = version
            };

            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        [Route("calculation-account-values")]
        public async Task CalculateAccountValues(string product, string version, CancellationToken cancellationToken)
        {
            var query = new CalculateAccountValuesCommand
            {
                Product = product,
                Version = version
            };

            await mediator.Send(query, cancellationToken);
        }
    }
}