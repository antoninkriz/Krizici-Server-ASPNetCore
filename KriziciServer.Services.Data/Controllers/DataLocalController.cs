using System;
using System.Net;
using System.Threading.Tasks;
using KriziciServer.Services.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace KriziciServer.Services.Data.Controllers
{
    [Route("api")]
    [ApiController]
    public class DataLocalController : ControllerBase
    {
        private readonly IDataService _dataService;

        public DataLocalController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet("update")]
        public async Task<IActionResult> Get()
        {
            var ipadress = Request.HttpContext.Connection.RemoteIpAddress;
            if (!(ipadress.Equals(IPAddress.Loopback) || ipadress.Equals(IPAddress.IPv6Loopback)))
                return Forbid();


            var timeStart = DateTime.UtcNow;
            await _dataService.UpdateDataAsync();

            return Ok((DateTime.UtcNow - timeStart).TotalSeconds);
        }
    }
}