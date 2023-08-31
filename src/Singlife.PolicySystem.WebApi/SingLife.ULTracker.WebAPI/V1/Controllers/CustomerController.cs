using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Common.Customers;
using SingLife.ULTracker.UseCases.Customers;
using SingLife.ULTracker.UseCases.Customers.DataExports;
using SingLife.ULTracker.WebAPI.Contracts.Customers;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public CustomerController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerDetails(Guid id, [FromQuery] string[] products, CancellationToken cancellationToken)
        {
            var query = new GetCustomerDetailsQuery { CustomerId = id, Products = products };
            var customer = await mediator.Send(query, cancellationToken);

            if (customer == null)
                return NotFound();
            
            return Ok(mapper.Map<CustomerDetails>(customer));
        }

        [HttpGet]
        [Route("individuals/{idNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerDetails[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerDetailsByIdNumber(string idNumber, CancellationToken cancellationToken)
        {
            var query = new GetCustomerDetailsByIdNumberQuery
            {
                IdNumber = idNumber
            };
            var customers = await mediator.Send(query, cancellationToken);

            if (customers != null)
            {
                var customerContracts = mapper.Map<CustomerDetails[]>(customers);

                return this.Ok(customerContracts);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [CorrelatedAuditApi("Customer:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditCustomer(EditCustomerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await EditCustomerAsync();
            }
            catch (CustomerIdNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (DuplicateCustomerIdentityException exception)
            {
                return BadRequest(exception);
            }

            async Task<IActionResult> EditCustomerAsync()
            {
                var command = mapper.Map<UpdateCustomerCommand>(request);
                command.UniqueCorrelationId = this.GetUniqueCorrelationId();
                command.AuthenticatedUser = this.GetAuthenticatedUser();

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("others/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OtherCustomerDetails))]
        public async Task<IActionResult> GetOtherCustomerById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetOtherCustomerByIdQuery
            {
                Id = id
            };

            var otherCustomerDto = await mediator.Send(query, cancellationToken);
            var otherCustomerContract = mapper.Map<OtherCustomerDetails>(otherCustomerDto);

            return Ok(otherCustomerContract);
        }

        [HttpPost]
        [Route("others")]
        [CorrelatedAuditApi("OtherCustomer:Create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOtherCustomer(CreateOtherCustomerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<CreateOtherCustomerCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (DuplicateOtherCustomerSnapshotException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PolicyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch]
        [Route("others")]
        [CorrelatedAuditApi("OtherCustomer:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditOtherCustomer(EditOtherCustomerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await EditOtherCustomerAsync();
            }
            catch (CustomerSnapshotNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicateOtherCustomerSnapshotException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> EditOtherCustomerAsync()
            {
                var command = mapper.Map<EditOtherCustomerCommand>(request);
                command.UniqueCorrelationId = this.GetUniqueCorrelationId();
                command.AuthenticatedUser = this.GetAuthenticatedUser();

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpDelete]
        [Route("others")]
        [CorrelatedAuditApi("OtherCustomer:Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOtherCustomer([FromQuery] DeleteOtherCustomerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await DeleteOtherCustomerAsync();
            }
            catch (CustomerSnapshotNotFoundException)
            {
                return NotFound();
            }

            async Task<IActionResult> DeleteOtherCustomerAsync()
            {
                var command = mapper.Map<DeleteCustomerSnapshotCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpPost]
        [Route("searches")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchCustomerResult))]
        public async Task<SearchCustomerResult> SearchCustomers(SearchCustomerRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchCustomersQuery>(request);
            var customerPage = await mediator.Send(query, cancellationToken);

            return mapper.Map<SearchCustomerResult>(customerPage);
        }

        [HttpPost]
        [Route("export")]
        [CorrelatedAuditApi("Customer:ExportCustomers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        public async Task<IActionResult> ExportCustomersData([FromBody] ExportCustomersRequest request, CancellationToken cancellationToken)
        {
            ExportCustomerDTO[] exportCustomerDto;
            if (request.SearchTerm == SearchTerm.WildCard)
            {
                var queryByWildCard = new GetExportAllCustomersDataQuery();
                exportCustomerDto = await mediator.Send(queryByWildCard);
            }
            else
            {
                var queryBySearchTerm = mapper.Map<GetExportCustomersDataQuery>(request);
                exportCustomerDto = await mediator.Send(queryBySearchTerm, cancellationToken);
            }

            var command = new ExportCustomersDataCommand()
            {
                CustomerTemplateContent = ApplicationSettings.V1.CustomerTemplate,
                Customers = exportCustomerDto
            };

            var result = await mediator.Send(command);
            var fileContract = mapper.Map<FileContract>(result);

            return Ok(fileContract);
        }

        [HttpPatch]
        [Route("update-identity")]
        [CorrelatedAuditApi("Customer:UpdateIdentity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateIdentity([FromBody] EditCustomerIdentityRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await UpdateCustomerIdentityAsync();
            }
            catch (CustomerNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicateCustomerException ex)
            {
                return BadRequest(ex.Message);
            }

            async Task<IActionResult> UpdateCustomerIdentityAsync()
            {
                var command = mapper.Map<UpdateCustomerIdentityCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("tax-residences")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaxResidence[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerTaxResidences([FromQuery] GetTaxResidencesDetailsRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetTaxResidencesQuery>(request);
            var taxResidencesDto = await mediator.Send(query, cancellationToken);

            if (taxResidencesDto == null)
                return NotFound();

            return Ok(mapper.Map<TaxResidence[]>(taxResidencesDto));
        }
    }
}