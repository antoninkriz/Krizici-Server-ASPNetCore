using System;
using System.Threading.Tasks;
using KriziciServer.Api.Requests;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RawRabbit;

namespace KriziciServer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IBusClient _busClient;
        private readonly ILogger<DataController> _logger;

        public DataController(IMemoryCache cache, IBusClient busClient, ILogger<DataController> logger)
        {
            _cache = cache;
            _busClient = busClient;
            _logger = logger;
        }

        [HttpPost("jwt")]
        public async Task<IActionResult> Jwt([FromBody] JwtRequest jwtrq)
        {
            var response = await _busClient.RequestAsync<LoginRequest, LoginResponse>(new LoginRequest
            {
                IdToken = jwtrq.idtoken
            });

            _logger.LogInformation(response.Success
                ? $"Successfully generated JWT '{response.Response}' for GoogleTokenId '{jwtrq.idtoken?.Substring(0, 8)}...'"
                : $"JWT could not be generated for GoogleTokenId '{jwtrq.idtoken?.Substring(0, 8)}...'");

            return Ok(JsonConvert.SerializeObject(response));
        }

        [HttpGet("{json}")]
        [ResponseCache(Duration = 180)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Get(string json)
        {
            var resp = _cache.Get<DataResponse>($"{json}");
            if (resp != null)
                return new JsonResult(JsonConvert.DeserializeObject(resp.Response));
            
            resp = await _busClient.RequestAsync<DataRequest, DataResponse>(new DataRequest
            {
                What = json
            });
            
            _cache.Set($"{json}",
                resp,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = (DateTime.Now.AddMinutes(60) - DateTime.Now)
                });

            return new JsonResult(JsonConvert.DeserializeObject(resp.Response));
        }

        [HttpGet("{type}/{id}")]
        [ResponseCache(Duration = 60)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Get(string type, int id)
        {
            var resp = _cache.Get<ImageResponse>($"{type}-{id}");
            if (resp != null)
            {
                if (resp.Success)
                    return File(resp.Response, "image/png");
                return BadRequest(resp.Error);
            }

            resp = await _busClient.RequestAsync<ImageRequest, ImageResponse>(new ImageRequest
            {
                Type = type,
                Id = id
            });

            _cache.Set($"{type}-{id}",
                resp,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = (DateTime.Now.AddMinutes(60) - DateTime.Now)
                });

            if (resp.Success)
                return File(resp.Response, "image/png");
            return BadRequest(resp.Error);
        }
    }
}