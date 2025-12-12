using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace SwissLohnSystem.UI.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public string BaseUrl { get; }

        public ApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            BaseUrl = config["Api:BaseUrl"]?.TrimEnd('/') ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(BaseUrl))
            {
                _http.BaseAddress = new System.Uri(BaseUrl);
            }
        }

        public async Task<(bool ok, T? data, string? message)> GetAsync<T>(string url) where T : class
        {
            var resMsg = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return await ParseEnvelope<T>(resMsg);
        }

        public async Task<(bool ok, T? data, string? message)> PostAsync<T>(string url, object body) where T : class
        {
            var resMsg = await _http.PostAsJsonAsync(url, body);
            return await ParseEnvelope<T>(resMsg);
        }

        public async Task<(bool ok, T? data, string? message)> PutAsync<T>(string url, object body) where T : class
        {
            var resMsg = await _http.PutAsJsonAsync(url, body);
            return await ParseEnvelope<T>(resMsg);
        }

        public async Task<(bool ok, T? data, string? message)> DeleteAsync<T>(string url) where T : class
        {
            var resMsg = await _http.DeleteAsync(url);
            return await ParseEnvelope<T>(resMsg);
        }

        /// <summary>
        /// CSV import gibi multipart/form-data upload işlemleri için.
        /// </summary>
        public async Task<(bool ok, T? data, string? message)> PostMultipartAsync<T>(
            string url,
            IFormFile file,
            string formFieldName = "File") where T : class
        {
            if (file is null || file.Length == 0)
                return (false, null, "Datei ist leer oder fehlt.");

            using var form = new MultipartFormDataContent();

            await using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "text/csv");

            form.Add(fileContent, formFieldName, file.FileName);

            var resMsg = await _http.PostAsync(url, form);
            return await ParseEnvelope<T>(resMsg);
        }

        private static async Task<(bool ok, T? data, string? message)> ParseEnvelope<T>(HttpResponseMessage resMsg) where T : class
        {
            string? raw = null;

            try
            {
                raw = await resMsg.Content.ReadAsStringAsync();
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                var ok = resMsg.IsSuccessStatusCode;
                return (ok, null, resMsg.ReasonPhrase);
            }

            var trimmed = raw.TrimStart();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (trimmed.StartsWith("["))
            {
                try
                {
                    var direct = JsonSerializer.Deserialize<T>(raw, jsonOptions);
                    return (resMsg.IsSuccessStatusCode, direct, resMsg.IsSuccessStatusCode ? null : resMsg.ReasonPhrase);
                }
                catch (Exception ex)
                {
                    return (false, null, $"JSON parse error (array): {ex.Message}");
                }
            }

            try
            {
                var env = JsonSerializer.Deserialize<ApiEnvelope<T>>(raw, jsonOptions);

                if (env is not null)
                {
                    var success = resMsg.IsSuccessStatusCode && env.Success;
                    var msg = env.Message ?? resMsg.ReasonPhrase;
                    return (success, env.Data, msg);
                }
            }
            catch
            {
            }

            try
            {
                var direct = JsonSerializer.Deserialize<T>(raw, jsonOptions);
                return (resMsg.IsSuccessStatusCode, direct, resMsg.IsSuccessStatusCode ? null : resMsg.ReasonPhrase);
            }
            catch (Exception ex)
            {
                return (false, null, $"JSON parse error: {ex.Message}");
            }
        }

        private sealed class ApiEnvelope<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}
