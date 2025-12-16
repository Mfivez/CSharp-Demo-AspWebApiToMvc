using System.Text.Json;
using System.Text;

namespace DemoUser.ASP.Clients
{
    public class AuthApiClient
    {
        private readonly HttpClient _http;

        public AuthApiClient(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("http://localhost:5062");
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { username, password }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync("/api/token", content);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("token").GetString();
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { username, password }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync("/api/users/register", content);
            return response.IsSuccessStatusCode;
        }
    }
}
