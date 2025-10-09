using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace SmartFarm;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Message> Messages { get; set; }
    private readonly HttpClient _httpClient = new();

    public MainPage()
    {
        InitializeComponent();
        Messages = new ObservableCollection<Message>();
        MessagesView.ItemsSource = Messages;

        _httpClient.BaseAddress = new Uri("http://192.168.88.251:8000");
        _httpClient.Timeout = TimeSpan.FromSeconds(180); 

    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        string userMessage = MessageEntry.Text?.Trim();
        if (string.IsNullOrEmpty(userMessage))
            return;

        // Thêm tin nhắn người dùng
        Messages.Add(new Message { Text = userMessage, IsUser = true });
        MessageEntry.Text = string.Empty;

        try
        {
            var request = new { query = userMessage };

            // Gửi request tới FastAPI
            var response = await _httpClient.PostAsJsonAsync("/chat/chat", request);
            response.EnsureSuccessStatusCode();

            // Đọc raw JSON để debug
            string rawJson = await response.Content.ReadAsStringAsync();
            //Messages.Add(new Message { Text = $"📥 Raw JSON: {rawJson}", IsUser = false });

            // Parse thành object
            var result = System.Text.Json.JsonSerializer.Deserialize<ChatResponse>(rawJson);

            if (result != null && !string.IsNullOrEmpty(result.response))
            {
                Messages.Add(new Message { Text = result.response, IsUser = false });
            }
            else
            {
                Messages.Add(new Message { Text = "⚠️ Không nhận được phản hồi từ server (reply null).", IsUser = false });
            }
        }
        catch (Exception ex)
        {
            Messages.Add(new Message { Text = $"❌ Lỗi kết nối: {ex.Message}", IsUser = false });
        }
    

    // Cuộn xuống tin nhắn mới nhất
    //if (Messages.Count > 0)
    //    MessagesView.ScrollTo(Messages.Count - 1, ScrollToPosition.End, animate: true);
}
}

public class Message
{
    public string Text { get; set; }
    public bool IsUser { get; set; }
}

public class ChatResponse
{
    public string response { get; set; }
}
