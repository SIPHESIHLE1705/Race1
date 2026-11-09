using System.Net;
using System.Text.Json;
using Race.MVC.Helpers;
using Race.MVC.Models;

namespace Race.MVC.Services
{
    // The ONLY class that talks to the Part 2 API. The MVC project has no
    // database access and no duplicated business rules.
    public class ApiClient : IApiClient
    {
        private const string ApiSessionCookieName = ".AspNetCore.Session";
        private const string UnavailableMessage =
            "The RaceDay service is currently unavailable. Please try again shortly.";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<ApiClient> logger)
        {
            _http = http;
            _accessor = accessor;
            _logger = logger;
        }

        private ISession? Session => _accessor.HttpContext?.Session;

        // ---------- GET ----------
        public async Task<ApiResult<T>> GetAsync<T>(string url)
        {
            try
            {
                using var response = await SendAsync(HttpMethod.Get, url);
                string body = await response.Content.ReadAsStringAsync();
                int status = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                    return ApiResult<T>.FailWith(status, ToMessage(response.StatusCode, body));

                T? data = JsonSerializer.Deserialize<T>(body, JsonOptions);
                return ApiResult<T>.PassWith(data, status);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "API GET {Url} failed", url);
                return ApiResult<T>.FailWith(503, UnavailableMessage);
            }
        }

        // ---------- POST / PUT / DELETE ----------
        public Task<ApiResult> PostAsync(string url) => SendWithoutDataAsync(HttpMethod.Post, url);
        public Task<ApiResult> PutAsync(string url) => SendWithoutDataAsync(HttpMethod.Put, url);
        public Task<ApiResult> DeleteAsync(string url) => SendWithoutDataAsync(HttpMethod.Delete, url);

        private async Task<ApiResult> SendWithoutDataAsync(HttpMethod method, string url)
        {
            try
            {
                using var response = await SendAsync(method, url);
                string body = await response.Content.ReadAsStringAsync();
                int status = (int)response.StatusCode;

                return response.IsSuccessStatusCode
                    ? ApiResult.Pass(status, ToMessage(response.StatusCode, body))
                    : ApiResult.Fail(status, ToMessage(response.StatusCode, body));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "API {Method} {Url} failed", method, url);
                return ApiResult.Fail(503, UnavailableMessage);
            }
        }

        // ---------- LOGIN: captures the API's session cookie ----------
        public async Task<ApiResult<LoginResponseDto>> LoginAsync(string email, string password)
        {
            try
            {
                string url = ApiUrl.Build("api/Auth/login", ("email", email), ("password", password));

                using var response = await SendAsync(HttpMethod.Post, url);
                string body = await response.Content.ReadAsStringAsync();
                int status = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode)
                    return ApiResult<LoginResponseDto>.FailWith(status, ToMessage(response.StatusCode, body));

                var user = JsonSerializer.Deserialize<LoginResponseDto>(body, JsonOptions);
                string? cookie = ExtractSessionCookie(response);

                if (user == null || cookie == null || Session == null)
                    return ApiResult<LoginResponseDto>.FailWith(502,
                        "Login worked but your session could not be started. Please try again.");

                // Keep the API cookie in OUR session so later calls stay logged in.
                Session.SetString(SessionKeys.ApiCookie, cookie);
                return ApiResult<LoginResponseDto>.PassWith(user, status);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "API login failed");
                return ApiResult<LoginResponseDto>.FailWith(503, UnavailableMessage);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                using var response = await SendAsync(HttpMethod.Post, "api/Auth/logout");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "API logout failed (the local session is still cleared)");
            }
        }

        // ---------- helpers ----------
        private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            string? cookie = Session?.GetString(SessionKeys.ApiCookie);
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.TryAddWithoutValidation("Cookie", cookie);

            return _http.SendAsync(request);
        }

        private static string? ExtractSessionCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return null;

            string? raw = values.FirstOrDefault(v =>
                v.StartsWith(ApiSessionCookieName + "=", StringComparison.Ordinal));

            return raw?.Split(';')[0];   // keep only  name=value
        }

        // Show the API's own short messages ("Email already registered.");
        // hide anything technical (JSON problem details, HTML, server errors).
        private static string ToMessage(HttpStatusCode status, string body)
        {
            string text = body.Trim().Trim('"');
            bool looksFriendly = text.Length is > 0 and <= 300
                && !text.StartsWith('{') && !text.StartsWith('[') && !text.StartsWith('<');

            if ((int)status < 500 && looksFriendly) return text;

            return (int)status switch
            {
                400 => "Please check the information you entered and try again.",
                401 => "Please log in first.",
                403 => "You do not have permission to do that.",
                404 => "The requested item could not be found.",
                409 => "That action conflicts with existing data.",
                _ => "Something went wrong on the server. Please try again."
            };
        }
    }
}
