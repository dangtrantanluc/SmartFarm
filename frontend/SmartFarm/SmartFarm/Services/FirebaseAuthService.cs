using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartFarm.Services
{
	public class FirebaseAuthService
	{
		private readonly string apiKey = "AIzaSyBi5JE3OPeMB9sftxE98_NWZ3TlExrcTas";
		private readonly HttpClient httpClient = new HttpClient();


		public async Task<(bool success, string message, string uid)> LoginAsync(string email, string password)
		{
			var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

			var payload = new
			{
				email = email,
				password = password,
				returnSecureToken = true
			};

			var json = JsonSerializer.Serialize(payload);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync(url, content);
			var responseString = await response.Content.ReadAsStringAsync();

			if (response.IsSuccessStatusCode)
			{
				using var doc = JsonDocument.Parse(responseString);
				var root = doc.RootElement;
				var uid = root.GetProperty("localId").GetString();
				return (true, "Đăng nhập thành công", uid);
			}
			else
			{
				using var doc = JsonDocument.Parse(responseString);
				var error = doc.RootElement.GetProperty("error").GetProperty("message").GetString();
				return (false, $"Lỗi đăng nhập: {error}", null);
			}
		}
	}
}
