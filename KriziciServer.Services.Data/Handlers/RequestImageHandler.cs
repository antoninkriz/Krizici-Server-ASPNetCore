using System.Text;
using System.Threading.Tasks;
using KriziciServer.Common.Exceptions;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using KriziciServer.Services.Data.Services;
using Microsoft.Extensions.Logging;

namespace KriziciServer.Services.Data.Handlers
{
    public class RequestImageHandler : IRequestHandler<ImageRequest, ImageResponse>
    {
        private readonly IDataService _dataService;
        private readonly ILogger _logger;

        public RequestImageHandler(IDataService dataService, ILogger<RequestImageHandler> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async Task<ImageResponse> HandleAsync(ImageRequest request)
        {
            _logger.LogInformation($"Requesting file: '{request.Type}-{request.Id}.png'.");

            var response = new ImageResponse();

            try
            {
                response.Response = await _dataService.GetContentAsync(request.Type, request.Id);
                response.Success = true;
            }
            catch (AuthException e)
            {
                response.Success = false;
                response.Error = e.Code;
            }

            _logger.LogInformation($"Request for file '{request.Type}-{request.Id}.png' successful");

            return response;
        }
    }
}