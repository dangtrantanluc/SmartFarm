using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using SmartFarm.Models;
namespace SmartFarm.Services
{
    internal class NotificationService
    {
        private readonly HttpClient _httpClient;
        private FirebaseClient _firebaseClient;
        private readonly string _baseUrl;
        public NotificationService()
        {
            _baseUrl  = "https://smartfarm-d4d95-default-rtdb.firebaseio.com/";  
            _firebaseClient = new FirebaseClient(_baseUrl);
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.88.51:8000")// địa chỉ API FastAPI
            };
        }

        public async Task<bool> SaveNotification(string userId, string message, DateTime time)
        {
            var data = new NotificationsInfo
            {
                userId = userId,
                message = message,
                timestamp = time
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/notifications/save", content);
            return response.IsSuccessStatusCode;    
        }
        public async Task<bool> RemoveNotification(string userId, string noteKey)
        {
            var response = await _httpClient.DeleteAsync($"/notifications/remove/{userId}/{noteKey}");
            return response.IsSuccessStatusCode;
        }

        public async Task updateNotification(string userId, string noteKey, string newMessage, DateTime newTime)
        {
            try
            {
                // Đọc key từ file service account
                var filePath = Path.Combine(FileSystem.AppDataDirectory, "serviceAccountKey.json");
                // Nếu file chưa tồn tại, copy từ asset ra
                if (!File.Exists(filePath))
                {
                    using var stream1 = await FileSystem.OpenAppPackageFileAsync("serviceAccountKey.json");
                    using var reader1 = new StreamReader(stream1);
                    string json = await reader1.ReadToEndAsync();
                    await File.WriteAllTextAsync(filePath, json);
                }
                var credential = GoogleCredential.FromFile(filePath)
                    .CreateScoped(new[] { "https://www.googleapis.com/auth/firebase.database", "https://www.googleapis.com/auth/userinfo.email" });
                var tokenAccess = credential as ITokenAccess;
                // Lấy access token
                var accessToken = await tokenAccess.GetAccessTokenForRequestAsync();

                // Tạo object chứa dữ liệu mới
                var updatedNotification = new
                {
                    message = newMessage,
                    timestamp = newTime.ToString("yyyy-MM-ddTHH:mm:sszzz")
                };
                var jsonData = JsonSerializer.Serialize(updatedNotification);

                // 4️⃣ Gửi PATCH request đến node notifications/{userId}/{noteKey}
                var request = new HttpRequestMessage(
                    HttpMethod.Patch,
                    $"{_baseUrl}/notifications/{userId}/{noteKey}.json"
                )
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };

                // Gắn access token vào header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var client = new HttpClient();
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Cập nhật thông báo thành công!");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Lỗi cập nhật: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật thông báo: {ex.Message}");
            }
        }

        public async Task getNotifications(string userId, ObservableCollection<Note> existingNotes) 
        {
            try
            {
                // Đọc key từ file service account
                var filePath = Path.Combine(FileSystem.AppDataDirectory, "serviceAccountKey.json");
                // Nếu file chưa tồn tại, copy từ asset ra
                if (!File.Exists(filePath))
                {
                    using var stream1 = await FileSystem.OpenAppPackageFileAsync("serviceAccountKey.json");
                    using var reader1 = new StreamReader(stream1);
                    string json = await reader1.ReadToEndAsync();
                    await File.WriteAllTextAsync(filePath, json);
                }
                var credential = GoogleCredential.FromFile(filePath)
                    .CreateScoped(new[] { "https://www.googleapis.com/auth/firebase.database", "https://www.googleapis.com/auth/userinfo.email" });
                var tokenAccess = credential as ITokenAccess;
                // Lấy access token
                var accessToken = await tokenAccess.GetAccessTokenForRequestAsync();
                var request = new HttpRequestMessage(HttpMethod.Get,$"{_baseUrl}/notifications/{userId}.json");

                // Gắn token vào header (không phải query)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var client = new HttpClient()
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };

                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                using var stream = await response.Content.ReadAsStreamAsync();

                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    Console.WriteLine("Line: "+ line);
                    if (string.IsNullOrWhiteSpace(line) || line == "null") continue;
                  
                    try
                    {
                        var noteDict = JsonSerializer.Deserialize<Dictionary<string, Note>>(line);
                        if(noteDict != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                existingNotes.Clear();
                                foreach (var note in noteDict)
                                {     
                                    existingNotes.Add(new Note
                                    {
                                        Key = note.Key,
                                        UserId = note.Value.UserId,
                                        Message = note.Value.Message,
                                        TimeStamp = note.Value.TimeStamp
                                    });
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Deserialize error: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gọi API: {ex.Message}");
            }
        }
    }
}
