using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Net.Http;
using System.IO;  // Thêm cho MemoryStream

namespace SmartFarm;
public partial class PlantPage : ContentPage
{
    FileResult? photo;

    public PlantPage()
    {
        InitializeComponent();
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Lỗi", "Không có quyền camera.", "OK");
                return;
            }
        }

        photo = await MediaPicker.CapturePhotoAsync();
        if (photo is null) return;

        await LoadPhotoToImage();
    }

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Lỗi", "Không có quyền thư viện ảnh.", "OK");
                return;
            }
        }

        photo = await MediaPicker.PickPhotoAsync();
        if (photo is null) return;

        await LoadPhotoToImage();
    }

    private async Task LoadPhotoToImage()
    {
        await using var originalStream = await photo!.OpenReadAsync();
        if (originalStream is null) return;

        using var memoryStream = new MemoryStream();
        await originalStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;  // Reset position để read từ đầu

        SelectedImage.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
        ClearButton.IsVisible = true;  // Hiện nút X khi có ảnh  
    }
    private void OnClearClicked(object sender, EventArgs e)
    {
        SelectedImage.Source = null;
        photo = null;
        ClearButton.IsVisible = false;  // Ẩn nút X sau khi clear
    }

    private async void OnUploadClicked(object sender, EventArgs e)
    {
        if (photo is null)
        {
            await DisplayAlert("Lỗi", "Chưa có ảnh nào được chọn.", "OK");
            return;
        }

        using var content = new MultipartFormDataContent();
        await using var stream = await photo.OpenReadAsync();
        if (stream is null)
        {
            await DisplayAlert("Lỗi", "Không đọc được ảnh.", "OK");
            return;
        }
        content.Add(new StreamContent(stream), "file", photo.FileName ?? "image.jpg");  // Handle null FileName

        var client = new HttpClient();
        var response = await client.PostAsync("http://10.0.2.2:8000/disease/predict", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PredictionResult?>();
            if (result is not null)
            {
                ResultLabel.Text = $"Kết quả: {result.Prediction}";
            }
            else
            {
                ResultLabel.Text = "Không nhận được kết quả.";
            }
        }
        else
        {
            ResultLabel.Text = "Lỗi kết nối server AI.";
        }
    }
}

public class PredictionResult
{
    public string? Prediction { get; set; }  // Nullable để an toàn
}
