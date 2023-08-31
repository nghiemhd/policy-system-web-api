using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Singlife.ULTracker.WebAPI.Infrastructure;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Infrastructure.Common;
using SingLife.ULTracker.Infrastructure.EsbIntegration;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.WebAPI.Contracts.Compliances;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/compliances")]
    public class ComplianceController : ControllerBase
    {
        private readonly HttpClient esbApiClient;
        private readonly IEsbAccessTokenService esbAccessTokenService;
        private readonly IMapper mapper;

        public ComplianceController(
            [KeyFilter(HttpClients.EsbIntegration)] HttpClient esbApiClient,
            IEsbAccessTokenService esbAccessTokenService,
            IMapper mapper)
        {
            this.esbApiClient = esbApiClient;
            this.esbAccessTokenService = esbAccessTokenService;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("bankruptcies/upload")]
        [CorrelatedAuditApi("Compliance:UploadBankruptcy")]
        public async Task<IActionResult> UploadBankruptcies(UploadBankruptciesRequest request)
        {
            var bankruptcies = ParseBankruptciesFromCsv(request.BankruptciesFile);

            SetActionToBankruptcies(bankruptcies, request.AddOrDeleteAction);

            await UploadBankruptciesToEsbAsync(bankruptcies);

            return Ok();
        }

        private Bankruptcy[] ParseBankruptciesFromCsv(FileContract file)
        {
            var fileDto = mapper.Map<FileDto>(file);
            var bankruptcyDtos = ReadBankruptcyService.ReadCsvFile(fileDto);

            return mapper.Map<Bankruptcy[]>(bankruptcyDtos);
        }

        private void SetActionToBankruptcies(Bankruptcy[] backruptcies, string addOrDeleteAction)
        {
            foreach (Bankruptcy bankruptcy in backruptcies)
            {
                bankruptcy.Action = addOrDeleteAction;
            }
        }

        private async Task UploadBankruptciesToEsbAsync(Bankruptcy[] bankruptcies)
        {
            var request = await CreateUploadBankruptciesToEsbRequestAsync(bankruptcies);

            var response = await esbApiClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private async Task<HttpRequestMessage> CreateUploadBankruptciesToEsbRequestAsync(Bankruptcy[] bankruptcies)
        {
            var bankruptciesJson = JsonConvert.SerializeObject(bankruptcies);

            var request = new HttpRequestMessage(HttpMethod.Put, "compliance-service/compliance-file")
            {
                Content = new StringContent(bankruptciesJson, Encoding.UTF8)
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var accessToken = await esbAccessTokenService.GetAccessTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            return request;
        }
    }
}