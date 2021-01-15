using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VRNotifier.DTO;
using VRNotifier.Extensions;
using VRNotifier.Services;

namespace VRNotifier.Controllers
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController
    {
        private readonly DiscordService _discordService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(DiscordService discordService, ILogger<NotificationController> logger)
        {
            _discordService = discordService;
            _logger = logger;
        }
        
        [HttpPost]
        public async Task<JsonResult> Notify(NotificationDTO notificationDto)
        {
            try
            {
                var result = await _discordService.Notify(notificationDto);
                return new JsonResult(result.AsSerializableResult());
            }
            catch (Exception e)
            {
                _logger.LogError("Something went wrong. {exceptionMessage}", e.Message);
                return new JsonResult(Result.Failure("Notifying Endpoints failed.").AsSerializableResult());
            }
            
        }
        
    }
}