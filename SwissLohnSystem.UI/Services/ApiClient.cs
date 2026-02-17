using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace SwissLohnSystem.UI.Services
{
    public sealed class ApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public string BaseUrl { get; }

        public ApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            BaseUrl = config["Api:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(BaseUrl))
                _http.BaseAddress = new System.Uri(BaseUrl);
        }

        public async Task<(bool ok, T? data, string? message)> GetAsync<T>(string url)
        {
            var res = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return await ParseEnvelope<T>(res);
        }

        public async Task<(bool ok, T? data, string? message)> PostAsync<T>(string url, object body)
        {
            var res = await _http.PostAsJsonAsync(url, body);
            return await ParseEnvelope<T>(res);
        }

        public async Task<(bool ok, T? data, string? message)> PutAsync<T>(string url, object body)
        {
            var res = await _http.PutAsJsonAsync(url, body);
            return await ParseEnvelope<T>(res);
        }

        public async Task<(bool ok, T? data, string? message)> DeleteAsync<T>(string url)
        {
            var res = await _http.DeleteAsync(url);
            return await ParseEnvelope<T>(res);
        }

        public async Task<(bool ok, T? data, string? message)> PostMultipartAsync<T>(
            string url,
            IFormFile file,
            string formFieldName = "File")
        {
            if (file is null || file.Length == 0)
                return (false, default, "Datei ist leer oder fehlt.");

            using var form = new MultipartFormDataContent();

            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "text/csv");
            form.Add(fileContent, formFieldName, file.FileName);

            var res = await _http.PostAsync(url, form);
            return await ParseEnvelope<T>(res);
        }

        private static async Task<(bool ok, T? data, string? message)> ParseEnvelope<T>(HttpResponseMessage res)
        {
            string raw;
            try { raw = await res.Content.ReadAsStringAsync(); }
            catch { return (res.IsSuccessStatusCode, default, res.ReasonPhrase); }

            if (string.IsNullOrWhiteSpace(raw))
                return (res.IsSuccessStatusCode, default, res.ReasonPhrase);

            // 1) ApiResponse<T> envelope dene
            try
            {
                var env = JsonSerializer.Deserialize<ApiEnvelope<T>>(raw, JsonOpts);
                if (env is not null && (env.Success || !string.IsNullOrWhiteSpace(env.Message) || env.Data is not null))
                {
                    // success flag + http status birlikte
                    var ok = res.IsSuccessStatusCode && env.Success;
                    var msg = env.Message ?? (ok ? null : res.ReasonPhrase);
                    return (ok, env.Data, msg);
                }
            }
            catch
            {
                // ignore -> direct parse dene
            }

            // 2) Direct T parse
            try
            {
                var direct = JsonSerializer.Deserialize<T>(raw, JsonOpts);
                return (res.IsSuccessStatusCode, direct, res.IsSuccessStatusCode ? null : res.ReasonPhrase);
            }
            catch (System.Exception ex)
            {
                return (false, default, $"JSON parse error: {ex.Message}");
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
