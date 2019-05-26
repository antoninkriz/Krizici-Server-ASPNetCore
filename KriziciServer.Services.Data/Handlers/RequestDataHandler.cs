using System.Threading.Tasks;
using KriziciServer.Common.Exceptions;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using KriziciServer.Services.Data.Services;
using Microsoft.Extensions.Logging;

namespace KriziciServer.Services.Data.Handlers
{
    public class RequestDataHandler : IRequestHandler<DataRequest, DataResponse>
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;

        public RequestDataHandler(IDataService dataService, ILogger<RequestDataHandler> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async Task<DataResponse> HandleAsync(DataRequest request)
        {
            _logger.LogInformation($"Requesting file: '{request.What}.json'.");

            var response = new DataResponse();

            try
            {
                response.Response = await _dataService.GetDataAsync(request.What == "contacts" ? Types.Contacts : Types.Data);
                response.Success = true;
            }
            catch (AuthException e)
            {
                response.Success = false;
                response.Response = e.Code;
            }

            _logger.LogInformation($"Request for file '{request.What}.json' successful");

            return response;
        }
    }
}