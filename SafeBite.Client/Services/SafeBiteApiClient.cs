using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SafeBite.Client.Services
{
    // The one class in the client that talks to the SafeBite API.
    // It is a "typed HttpClient": Program.cs registers it with AddHttpClient, which hands it
    // an HttpClient that already has the base address (Apigee or the local API) and the API key header.
    // Each public method below is one of the six operations in the marking scheme.
    public class SafeBiteApiClient
    {
        // Resource names = the routes in the API's controllers ([Route("api/...")])
        public const string Establishments = "establishments";
        public const string Inspections = "inspections";
        public const string OwnerResponses = "owner-responses";

        private readonly HttpClient _http;

        // Web defaults: camelCase property names, case-insensitive reading (matches the API's JSON)
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public SafeBiteApiClient(HttpClient http) => _http = http;

        // 1. GET (GetAll)   GET api/{resource}
        public async Task<List<T>> GetAllAsync<T>(string resource)
        {
            using var response = await _http.GetAsync($"api/{resource}");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<List<T>>(Json) ?? new List<T>();
        }

        // 2. GET (GetById)  GET api/{resource}/{id}   returns null when the API answers 404
        public Task<T?> GetByIdAsync<T>(string resource, int id) => GetOrNullAsync<T>($"api/{resource}/{id}");

        // Extra read endpoint: GET api/establishments/{id}/badge
        public Task<Models.BadgeDto?> GetBadgeAsync(int establishmentId) =>
            GetOrNullAsync<Models.BadgeDto>($"api/{Establishments}/{establishmentId}/badge");

        // 3. POST  POST api/{resource}   body = CreateDto, returns the created record (201 Created)
        public async Task<TRead?> CreateAsync<TRead>(string resource, object createDto)
        {
            using var content = JsonBody(createDto);
            using var response = await _http.PostAsync($"api/{resource}", content);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<TRead>(Json);
        }

        // 4. PUT  PUT api/{resource}/{id}   body = the complete UpdateDto (204 No Content)
        public async Task ReplaceAsync(string resource, int id, object updateDto)
        {
            using var content = JsonBody(updateDto);
            using var response = await _http.PutAsync($"api/{resource}/{id}", content);
            await EnsureSuccessAsync(response);
        }

        // 5. PATCH  PATCH api/{resource}/{id}   body = a JSON Patch document (204 No Content)
        // JSON Patch is a list of operations, for example:
        //   [ { "op": "replace", "path": "/phone", "value": "416-555-0100" } ]
        // The content type must be application/json-patch+json, or the API's
        // Newtonsoft JSON Patch formatter won't read the body.
        public async Task PatchAsync(string resource, int id, IEnumerable<JsonPatchOperation> operations)
        {
            using var content = JsonBody(operations, "application/json-patch+json");
            using var response = await _http.PatchAsync($"api/{resource}/{id}", content);
            await EnsureSuccessAsync(response);
        }

        // 6. DELETE  DELETE api/{resource}/{id}   (204 No Content)
        public async Task DeleteAsync(string resource, int id)
        {
            using var response = await _http.DeleteAsync($"api/{resource}/{id}");
            await EnsureSuccessAsync(response);
        }

        // ---------- helpers ----------
        // Serializes a DTO to a JSON string body (with a Content-Length, which proxies like Apigee prefer)
        private static StringContent JsonBody(object value, string mediaType = "application/json") =>
            new(JsonSerializer.Serialize(value, Json), Encoding.UTF8, mediaType);

        private async Task<T?> GetOrNullAsync<T>(string path)
        {
            using var response = await _http.GetAsync(path);
            if (response.StatusCode == HttpStatusCode.NotFound) return default;
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<T>(Json);
        }

        // Turns any non-2xx answer into an ApiException that carries the status code and the
        // API's (or Apigee's) error body, so the page can show what went wrong.
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiException(response.StatusCode, body);
        }
    }

    // One JSON Patch operation. "op" is usually "replace"; "path" is "/" + the camelCase property name.
    public record JsonPatchOperation(string Op, string Path, object? Value);

    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Body { get; }

        public ApiException(HttpStatusCode statusCode, string body)
            : base(Describe(statusCode, body))
        {
            StatusCode = statusCode;
            Body = body;
        }

        private static string Describe(HttpStatusCode code, string body)
        {
            var hint = code switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    " Apigee rejected the request: check the API key and the header name.",
                HttpStatusCode.NotFound => " The record or route was not found.",
                HttpStatusCode.BadRequest => " The API rejected the data.",
                _ => ""
            };
            var detail = string.IsNullOrWhiteSpace(body) ? "" : $" Details: {(body.Length > 400 ? body[..400] + "…" : body)}";
            return $"API returned {(int)code} {code}.{hint}{detail}";
        }
    }
}
