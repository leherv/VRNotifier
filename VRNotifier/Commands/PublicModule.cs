using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VRNotifier.Config;
using VRNotifier.DTO;
using VRNotifier.Services;

namespace VRNotifier.Commands
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly TrackedMediaSettings _trackedMediaSettings;
        private readonly IVRPersistenceClient _vrPersistenceClient;
        private readonly ILogger<PublicModule> _logger;

        private const string HelpText =
            "Welcome to Vik Release Notifier (VRN)!\n" +
            "The following commands are available:\n" +
            "!subscribe [mediaName1], [mediaName2], ...\n" +
            "!unsubscribe [mediaName1], [mediaName2], ...\n" +
            "!listAvailable \n" +
            "!listSubscribed";

        public PublicModule(IOptions<TrackedMediaSettings> trackedMediaSettings,
            IVRPersistenceClient vrPersistenceClient, ILogger<PublicModule> logger)
        {
            _trackedMediaSettings = trackedMediaSettings.Value;
            _vrPersistenceClient = vrPersistenceClient;
            _logger = logger;
        }

        [Command("help")]
        [Alias("h")]
        public Task Help() => ReplyAsync(HelpText);

        [Command("listAvailable")]
        public Task ListAvailable()
        {
            var message = "Available Media: \n" +
                          $"{string.Join("\n", _trackedMediaSettings.MediaNames)}";
            return ReplyAsync(message);
        }

        [Command("listSubscribed")]
        public async Task ListSubscribed()
        {
            var result = await _vrPersistenceClient.GetSubscribedToMedia(GetNotificationEndpointNotifierIdentifier(Context));
            var message = "";
            if (result.IsSuccess)
            {
                message = result.Value == null || !result.Value.Any() 
                    ? "No Subscriptions yet."
                    : $"Subscribed To: \n {string.Join("\n", result.Value .Select(m => m.MediaName))}";
            }
            else
            {
                message = "Something went wrong.";
            }
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("subscribe")]
        public async Task Subscribe(params String[] mediaNames)
        {
            if (mediaNames == null || mediaNames.Length < 1)
            {
                await Context.Channel.SendMessageAsync("Nothing to do.");
            }
            var notificationEndpointNotifierIdentifier = GetNotificationEndpointNotifierIdentifier(Context); 
            var addSubscriptionDtos = mediaNames.Select(mediaName => new AddSubscriptionDTO(mediaName)).ToList();
            var addSubscriptionsDto = new AddSubscriptionsDTO(notificationEndpointNotifierIdentifier, addSubscriptionDtos);
            var result = await _vrPersistenceClient.Subscribe(addSubscriptionsDto);
            var message = result.IsFailure
                ? "Something went wrong."
                : "Successfully subscribed.";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("unsubscribe")]
        public async Task Unsubscribe(params string[] mediaNames)
        {
            if (mediaNames == null || mediaNames.Length < 1)
            {
                await Context.Channel.SendMessageAsync("Nothing to do.");
            }
            var notificationEndpointNotifierIdentifier = GetNotificationEndpointNotifierIdentifier(Context); 
            var deleteSubscriptionDtos = mediaNames.Select(mediaName => new DeleteSubscriptionDTO(mediaName)).ToList();
            var deleteSubscriptionsDto = new DeleteSubscriptionsDTO(notificationEndpointNotifierIdentifier, deleteSubscriptionDtos);
            var result = await _vrPersistenceClient.Unsubscribe(deleteSubscriptionsDto);
            var message = result.IsFailure
                ? "Something went wrong."
                : "Successfully unsubscribed.";
            await Context.Channel.SendMessageAsync(message);
        }

        private string GetNotificationEndpointNotifierIdentifier(ICommandContext context)
        {
            return context.Guild.Id.ToString();
        }
    }
}