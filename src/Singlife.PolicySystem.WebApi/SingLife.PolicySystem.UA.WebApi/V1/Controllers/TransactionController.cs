using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.UA.Model.Transactions;
using SingLife.PolicySystem.UA.UseCases.Transactions;
using SingLife.ULTracker.WebAPI.Contracts.UA.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.UA.WebApi.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ua-transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public TransactionController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Transaction))]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetTransactionQuery()
            {
                Id = id
            };

            var result = await mediator.Send(query, cancellationToken);
            var transactionContract = mapper.Map<Transaction>(result);

            return Ok(transactionContract);
        }

        [HttpPut]
        [CorrelatedAuditApi("Transaction:Edit")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit([FromBody] EditTransactionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<EditTransactionCommand>(request);

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (Exception ex) when (ex is TransactionNotFoundException || ex is TransactionModificationException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [CorrelatedAuditApi("Transaction:Delete")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromQuery] DeleteTransactionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<DeleteTransactionCommand>(request);

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (TransactionDeletionException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [CorrelatedAuditApi("Transaction:Create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<CreateTransactionCommand>(request);

                var result = await mediator.Send(command, cancellationToken);

                return Ok(result);
            }
            catch (TransactionCreationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}