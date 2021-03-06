using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;
using ChatApp.Services;
using Microsoft.ApplicationInsights;

namespace ChatApp.Controllers
{
    [Produces("application/json")]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IChatService ChatService;
        private readonly TelemetryClient Telemetry;

        public ChatController(IChatService chatService, TelemetryClient telemetry)
        {
            ChatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            // Telemetry
            Telemetry.TrackEvent("GetAsync");

            // Action
            var entries = await ChatService.GetAsync();
            return Json(entries.OrderBy(x => x.CreatedDate));
        }

        [HttpGet("from/{loadFrom}")]
        public async Task<IActionResult> GetAsync(DateTime loadFrom)
        {
            // Telemetry
            Telemetry.TrackEvent("GetAsync", new Dictionary<string, string> { { "loadFrom", loadFrom.ToString() } });

            // Action
            var entries = await ChatService.GetAsync();
            return Json(entries
                .Where(x => x.CreatedDate > loadFrom)
                .OrderBy(x => x.CreatedDate));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody]CreateChatEntryDto entry)
        {
            // Telemetry
            Telemetry.TrackEvent("CreateAsync", new Dictionary<string, string> {
                { "entry", entry?.ToJson() }
            });

            // Action
            if (ModelState.IsValid)
            {
                try
                {
                    var dto = await ChatService.CreateAsync(entry);
                    return CreatedAtAction(nameof(GetAsync), dto);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Error = ex.Message
                    });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}