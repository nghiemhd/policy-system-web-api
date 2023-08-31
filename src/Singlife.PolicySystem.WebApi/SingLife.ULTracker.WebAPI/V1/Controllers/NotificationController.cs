using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.Ulpb.V1.CustomerNotification;
using SingLife.ULTracker.UseCases.Ulpb.V1.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Notification;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public NotificationController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("send-emails/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SendingEmailResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendEmails(SendingEmailRequest request, CancellationToken cancellationToken)
        {
            var query = new GetPolicyQuery { PolicyId = request.PolicyId };
            var policy = await mediator.Send(query);

            if (policy == null)
            {
                return NotFound();
            }

            var command = new SendEmailToCustomerCommand
            {
                Policy = policy,
                AdditionalRecpientEmails = request.AdditionalEmails,
                CreatedBy = request.CreatedBy,
                CreatedOn = request.CreatedOn,
                EmailType = EmailType.OutstandingRequirementsEmail
            };

            var sendingEmailResponse = await mediator.Send(command, cancellationToken);

            return Ok(mapper.Map<SendingEmailResponse>(sendingEmailResponse));
        }
    }
}