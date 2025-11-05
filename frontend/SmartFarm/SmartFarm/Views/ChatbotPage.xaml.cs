using System.Collections.ObjectModel;
using System.Net.Http.Json;
using SmartFarm.Models;

namespace SmartFarm.Views
{
    public partial class ChatbotPage : ContentPage
    {
        public ObservableCollection<Message> Messages { get; set; }
        private readonly HttpClient _httpClient = new();

        public ChatbotPage()
        {
            InitializeComponent();
            Messages = new ObservableCollection<Message>();
            MessagesView.ItemsSource = Messages;

            _httpClient.BaseAddress = new Uri("http://192.168.1.102:8000");
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

                //Gửi request tới FastAPI
                var response = await _httpClient.PostAsJsonAsync("/chat/chat", request);
                response.EnsureSuccessStatusCode();

                //Đọc phản hồi dạng text (chuỗi)
                string botReply = await response.Content.ReadAsStringAsync();

                // Nếu server trả về "Human:" hoặc "Assistant:" thì loại bỏ
                if (botReply.StartsWith("Human:"))
                    botReply = botReply.Substring(6).Trim();
                if (botReply.StartsWith("Assistant:"))
                    botReply = botReply.Substring(10).Trim();

                botReply = botReply.Trim('"');


                //Hiển thị phản hồi bot
                Messages.Add(new Message { Text = botReply, IsUser = false });
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Text = $"❌ Lỗi kết nối: {ex.Message}", IsUser = false });
            }

          
            //if (Messages.Count > 0)
            //    MessagesView.ScrollTo(Messages.Count - 1, ScrollToPosition.End, animate: true);
        }
    }
}
