namespace SwissLohnSystem.UI.Options
{
    public class ApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        // Routes configurable: senin API farklıysa sadece burayı değiştir.
        public string SettingsBasePath { get; set; } = "/api/Settings";
        public string SettingsByCompanyPath { get; set; } = "/by-company"; // GET {base}/by-company/{companyId}
    }
}
