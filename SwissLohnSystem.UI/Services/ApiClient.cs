using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SwissLohnSystem.UI.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public string BaseUrl { get; }

        public ApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;

            // appsettings.json:  "Api": { "BaseUrl": "https://localhost:7090" }
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

        private static async Task<(bool ok, T? data, string? message)> ParseEnvelope<T>(HttpResponseMessage resMsg) where T : class
        {
            ApiEnvelope<T>? env = null;
            string? raw = null;

            try
            {
                // Önce JSON dene
                env = await resMsg.Content.ReadFromJsonAsync<ApiEnvelope<T>>();
            }
            catch
            {
                // JSON değilse düz metni al
                try { raw = await resMsg.Content.ReadAsStringAsync(); } catch { /* ignore */ }
            }

            var success = resMsg.IsSuccessStatusCode && env?.Success == true;
            if (success) return (true, env!.Data, env.Message);

            // Hata mesajı üret
            var msg =
                env?.Message
                ?? raw
                ?? $"{(int)resMsg.StatusCode} {resMsg.ReasonPhrase}";

            return (false, env?.Data, msg);
        }

        // API'nin standardı
        private sealed class ApiEnvelope<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}
