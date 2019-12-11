using Mehyaa.Arduino.OTAServer.Abstractions.Entities;
using Mehyaa.Arduino.OTAServer.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Mehyaa.Arduino.OTAServer.Web.Controllers
{
    [ApiController]
    [Route("Update")]
    public class UpdateController : ControllerBase
    {
        private const string Esp8266UserAgent = "ESP8266-http-Update";

        private readonly ILogger _logger;
        private readonly OTAContext _otaContext;

        public UpdateController(OTAContext otaContext, ILogger<UpdateController> logger)
        {
            _otaContext = otaContext;
            _logger = logger;
        }

        [HttpGet(nameof(Esp8266))]
        public async Task<IActionResult> Esp8266()
        {
            var userAgent = Request.Headers[HeaderNames.UserAgent].ToString();

            if (!string.Equals(userAgent, Esp8266UserAgent, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Device is not an ESP8266.");

                Response.StatusCode = (int) HttpStatusCode.Forbidden;

                return Content("Device is not an ESP8266.", "text/plain");
            }

            if (!Request.Headers.ContainsKey("x-ESP8266-Chip-ID") ||
                !Request.Headers.ContainsKey("x-ESP8266-STA-MAC") ||
                !Request.Headers.ContainsKey("x-ESP8266-AP-MAC") ||
                !Request.Headers.ContainsKey("x-ESP8266-chip-size") ||
                !Request.Headers.ContainsKey("x-ESP8266-free-space") ||
                !Request.Headers.ContainsKey("x-ESP8266-sketch-size") ||
                !Request.Headers.ContainsKey("x-ESP8266-sketch-md5") ||
                !Request.Headers.ContainsKey("x-ESP8266-sdk-version") ||
                !Request.Headers.ContainsKey("x-ESP8266-mode"))
            {
                _logger.LogWarning("Device headers are absent.");

                Response.StatusCode = (int) HttpStatusCode.Forbidden;

                return Content("Device headers are absent.", "text/plain");
            }

            var deviceMacAddress = Request.Headers["x-ESP8266-STA-MAC"].ToString();

            var device =
                await _otaContext.Set<Device>()
                    .Include(x => x.Firmwares)
                    .FirstOrDefaultAsync(x => x.MacAddress == deviceMacAddress);

            if (device == null)
            {
                _logger.LogWarning($"Device does not exist with MAC: {deviceMacAddress}");

                Response.StatusCode = (int) HttpStatusCode.BadRequest;

                return Content("Device does not exist.", "text/plain");
            }

            var firmware = device.Firmwares.OrderByDescending(x => x.Id).FirstOrDefault();

            if (firmware == null)
            {
                _logger.LogWarning($"Device firmware does not exist for DeviceId: {device.Id}");

                Response.StatusCode = (int) HttpStatusCode.BadRequest;

                return Content("Device firmware does not exist.", "text/plain");
            }

            var deviceSketchHash = Request.Headers["x-ESP8266-sketch-md5"].ToString();

            if (string.Equals(deviceSketchHash, firmware.Hash, StringComparison.InvariantCultureIgnoreCase))
            {
                Response.StatusCode = (int) HttpStatusCode.NotModified;

                return Content("304 Not Modified", "text/plain");
            }

            _logger.LogInformation($"FirmwareId: {firmware.Id} returned to the device with MAC: {deviceMacAddress}");

            Response.Headers["x-MD5"] = firmware.Hash;

            return PhysicalFile(firmware.Path, "application/octet-stream", firmware.Filename);
        }
    }
}