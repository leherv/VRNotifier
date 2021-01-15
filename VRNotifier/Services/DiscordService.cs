using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VRNotifier.DTO;
using VRPersistence.Config;

namespace VRNotifier.Services
{
    public class DiscordService: BackgroundService, INotificationService
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordSettings _discordSettings;
        private readonly CommandHandlingService _commandHandlingService;
        private readonly ILogger<DiscordService> _logger;
        private const string NOTIFICATION_CHANNEL_NAME = "notifications";

        public DiscordService(CommandHandlingService commandHandlingService, DiscordSocketClient client, IOptions<DiscordSettings> discordSettings, IServiceProvider serviceProvider, ILogger<DiscordService> logger)
        {
            _commandHandlingService = commandHandlingService;
            _client = client;
            _logger = logger;
            _discordSettings = discordSettings.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Connecting to Discord");
            await _client.LoginAsync(TokenType.Bot, _discordSettings.ApiKey);
            await _client.StartAsync();
            await _commandHandlingService.InitializeAsync();
            _logger.LogInformation("Connection successful.");
        }

        public async Task<Result> Notify(NotificationDTO notificationDto)
        {
            try
            {
                var notifiableChannelInformation = FilterNotifiableEndpointInformation(notificationDto);
                var notifyChannelTasks = notifiableChannelInformation.Select(notifiableChannelInfo =>
                    NotifyChannel(notifiableChannelInfo, notificationDto.Message));
                var executionResult = await Task.WhenAll(notifyChannelTasks);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed during notification for message {message} due to: {errorMessage}", notificationDto.Message, e.Message);
                return Result.Failure("Something went wrong.");
            }
        }

        private async Task<Result> NotifyChannel((SocketGuild socketGuild, SocketTextChannel socketTextChannel) endpointInformation, string message)
        {
            try
            {
                await endpointInformation.socketTextChannel?.SendMessageAsync(message);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to notify channel with name {channelName} due to {errorMessage}", endpointInformation.socketTextChannel?.Name ?? "Something went wrong", e.Message);
                return Result.Failure($"Failed to notify channel with name {endpointInformation.socketTextChannel?.Name ?? "Something went wrong"} due to {e.Message}");
            }
           
        }
        
        private IEnumerable<(SocketGuild socketGuild, SocketTextChannel socketTextChannel)> FilterNotifiableEndpointInformation(NotificationDTO notificationDto)
        {
            foreach (var notificationEndpointIdentifier in notificationDto.NotificationEndpointIdentifiers)
            {
                var guildResult = FetchGuild(notificationEndpointIdentifier);
                if (guildResult.IsFailure)
                {
                    _logger.LogError("Could not notify guild with identifier {identifier} due to {error}", notificationEndpointIdentifier, guildResult.Error);
                    continue;
                }

                var channelResult = FetchChannel(guildResult.Value);
                if (channelResult.IsFailure)
                {
                    _logger.LogError("Could not notify guild with name {guildName} and identifier {identifier} due to {error}", guildResult.Value.Name, notificationEndpointIdentifier, channelResult.Error);
                    continue;
                }
                yield return (guildResult.Value, channelResult.Value);
            }
        }

        private Result<SocketGuild> FetchGuild(string notificationEndpointIdentifier)
        {
            var guild = _client.Guilds.FirstOrDefault(g =>
                g.Id.ToString().Equals(notificationEndpointIdentifier));
            return guild == null
                ? Result.Failure<SocketGuild>($"No Guild for notificationEndpointIdentifier {notificationEndpointIdentifier} found.")
                : Result.Success(guild);
        }

        private Result<SocketTextChannel> FetchChannel(SocketGuild socketGuild)
        {
            var channel = socketGuild.TextChannels.FirstOrDefault(c => c.Name.ToLower().Equals(NOTIFICATION_CHANNEL_NAME));
            return channel == null
                ? Result.Failure<SocketTextChannel>($"No notification channel with name {NOTIFICATION_CHANNEL_NAME} found.")
                : Result.Success(channel);
        }
        
    }
}