using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SwissLohnSystem.UI.DTOs.Common;
using SwissLohnSystem.UI.DTOs.Setting;
using SwissLohnSystem.UI.Options;

namespace SwissLohnSystem.UI.Services
{
    public class SettingsApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiOptions _opt;

        public SettingsApiClient(IHttpClientFactory httpClientFactory, IOptions<ApiOptions> opt)
        {
            _httpClientFactory = httpClientFactory;
            _opt = opt.Value;
        }

        private HttpClient Client => _httpClientFactory.CreateClient("ApiClient");

        public async Task<ApiResponse<List<SettingDto>>> GetByCompanyAsync(int companyId, CancellationToken ct = default)
        {
            var url = $"{_opt.SettingsBasePath}{_opt.SettingsByCompanyPath}/{companyId}";
            var res = await Client.GetAsync(url, ct);

            // ApiResponse wrapper bekliyoruz:
            var payload = await res.Content.ReadFromJsonAsync<ApiResponse<List<SettingDto>>>(cancellationToken: ct);

            if (payload is not null) return payload;

            return new ApiResponse<List<SettingDto>>
            {
                Success = false,
                Message = $"Settings okunamadı. HTTP {(int)res.StatusCode}"
            };
        }

        public async Task<ApiResponse<object>> UpsertByCompanyAsync(int companyId, List<SettingUpsertDto> items, CancellationToken ct = default)
        {
            var url = $"{_opt.SettingsBasePath}{_opt.SettingsByCompanyPath}/{companyId}";
            var res = await Client.PutAsJsonAsync(url, items, ct);

            // Bazı API'ler ApiResponse<object> döndürür, bazıları ApiResponse<List<SettingDto>>.
            // Burada object alıp UI’da sadece Success/Message kullanıyoruz.
            var payload = await res.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);

            if (payload is not null) return payload;

            return new ApiResponse<object>
            {
                Success = false,
                Message = $"Settings kaydedilemedi. HTTP {(int)res.StatusCode}"
            };
        }
    }
}
