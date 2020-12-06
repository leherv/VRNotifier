using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VRNotifier.DTO;
using VRNotifier.Extensions;

namespace VRNotifier.Services
{
    public class VRPersistenceClient : IVRPersistenceClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<VRPersistenceClient> _logger;

        public VRPersistenceClient(HttpClient client, ILogger<VRPersistenceClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        private async Task<Result<HttpResponseMessage>> SendRequest(HttpRequestMessage requestMessage)
        {
            try
            {
                var response = await _client.SendAsync(requestMessage);
                return Result.Success(response);
            }
            catch (ArgumentNullException)
            {
                return Result.Failure<HttpResponseMessage>("Message was null");
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<HttpResponseMessage>("Message was already sent");
            }
            catch (HttpRequestException)
            {
                return Result.Failure<HttpResponseMessage>("HttpRequest failed");
            }
        }

        public async Task<Result<IEnumerable<MediaDTO>>> GetSubscribedToMedia(string notificationEndpointIdentifier)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"subscription/{notificationEndpointIdentifier}");
            var response = await SendRequest(message);
            if (response.IsSuccess)
            {
                var deserializeResult =
                    JsonHandler.Deserialize<SerializableResult<IEnumerable<MediaDTO>>>(await response.Value.Content
                        .ReadAsStringAsync());
                if (deserializeResult.IsFailure)
                {
                    return Result.Failure<IEnumerable<MediaDTO>>("Deserialization failed.");
                }
    
                return Result.Success(deserializeResult.Value.Value);
            }

            return Result.Failure<IEnumerable<MediaDTO>>(response.Error);
        }

        public async Task<Result> Subscribe(AddSubscriptionsDTO addSubscriptionsDto)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, "subscription");
            message.Content = new StringContent(JsonHandler.Serialize(addSubscriptionsDto), Encoding.UTF8,
                "application/json");
            var response = await SendRequest(message);
            if (response.IsFailure)
            {
                return Result.Failure("Unsubscribe failed.");
            }

            var deserializeResult =
                JsonHandler.Deserialize<List<SerializableResult>>(await response.Value.Content.ReadAsStringAsync());
            if (deserializeResult.IsFailure)
            {
                _logger.LogError("Deserialization failed due to: {errorDetails}.", deserializeResult.Error);
                return Result.Failure("Deserialization failed.");
            }

            var failedSubscriptions = deserializeResult.Value.Where(r => !r.IsSuccess).ToList();
            if (failedSubscriptions.Any())
            {
                _logger.LogError("Subscribe failed due to: {errorDetails}", string.Join('\n', failedSubscriptions.Select(fSub => fSub.Error)));
                return Result.Failure("Subscribe failed.");
            }

            return Result.Success();
        }

        public async Task<Result> Unsubscribe(DeleteSubscriptionsDTO deleteSubscriptionsDto)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, "subscription");
            message.Content = new StringContent(JsonHandler.Serialize(deleteSubscriptionsDto), Encoding.UTF8,
                "application/json");
            var response = await SendRequest(message);
            if (response.IsFailure)
            {
                return Result.Failure("Unsubscribe failed.");
            }
            var deserializeResult =
                JsonHandler.Deserialize<List<SerializableResult>>(await response.Value.Content.ReadAsStringAsync());
            if (deserializeResult.IsFailure)
            {
                _logger.LogError("Deserialization failed due to: {errorDetails}.", deserializeResult.Error);
                return Result.Failure("Deserialization failed.");
            }

            var failedSubscriptionCancellations = deserializeResult.Value.Where(r => !r.IsSuccess).ToList();
            if (failedSubscriptionCancellations.Any())
            {
                _logger.LogError("Unsubscribe failed due to: {errorDetails}", string.Join('\n', failedSubscriptionCancellations.Select(fSub => fSub.Error)));
                return Result.Failure("Unsubscribe failed.");
            }

            return Result.Success();
        }
    }
}