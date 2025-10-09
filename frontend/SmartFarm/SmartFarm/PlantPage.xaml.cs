using System.IO;  // Thêm cho MemoryStream
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace SmartFarm;
public partial class PlantPage : ContentPage
{
    FileResult? photo;
    private readonly HttpClient _httpClient = new();

    public PlantPage()
    {
        InitializeComponent();

        var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

        double screenWidth = displayInfo.Width / displayInfo.Density; //Lấy kích thước chiều rộng màn hình (đơn vị logic)
        double screenHeight = displayInfo.Height / displayInfo.Density; // Lấy kích thước chiều cao màn hình (đơn vị logic)

        frameTakePicture.WidthRequest = screenWidth * 0.85;
        frameTakePicture.HeightRequest = screenHeight * 0.5;

        predictResult.WidthRequest = screenWidth * 0.85;

        bgPredict.WidthRequest = screenWidth;
        bgPredict.HeightRequest = screenHeight;

        titlePage.WidthRequest = screenWidth;



        _httpClient.BaseAddress = new Uri("http://192.168.88.209:8000");
        _httpClient.Timeout = TimeSpan.FromSeconds(180);

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
        await using var stream = await photo.OpenReadAsync();
        if (stream is null)
        {
            await DisplayAlert("Lỗi", "Không đọc được ảnh.", "OK");
            return;
        }

        using var form = new MultipartFormDataContent();
        using var content = new StreamContent(stream);
        //content.Add(new StreamContent(stream), "file", photo.FileName ?? "image.jpg");  // Handle null FileName

        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(content, "file", Path.GetFileName(photo.FileName));
        // Thêm tham số top_k
        form.Add(new StringContent("3"), "top_k");

        try
        {
            // Gửi request tới astAPI
            var resp = await _httpClient.PostAsync("/plant/predict", form);
            resp.EnsureSuccessStatusCode();

            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<DiseaseResp>(json, options);

                var predictLabel = result.predicted_label;
                var confidence = result.confidence;
                if (result.alternatives != null && result.alternatives.Count > 0)
                {
                    foreach (var p in result.alternatives)
                    {

                    }
                } 
                if(result.guide != null)
                {

                }

            }
            else
            {
                
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Looix", ex.ToString(), "OK");
        }
    }

    private async Task CallPredictAsync(Stream imageStream)
    {
        using var client = new HttpClient();

     
    }
}



public class DiseaseResp
{
    public string? predicted_label { get; set; } // Tên loại bệnh
    public string? confidence { get; set; } // độ chính xác
    public List<Alternative>? alternatives { get; set; } // Những dự đoán có thể liên quan
    public Guide? guide { get; set; } // Dấu hiệu, phòng và trị bệnh cho loại bệnh được dự đoán.
}
public class Guide
{
    public string? plant {  get; set; } //Tên cây trồng
    public string? symptoms { get; set; } // dấu hiệu của bệnh 
    public string? prevention  { get; set; } //Phòng ngừa bệnh cho  cây
    public string? treatment { get; set; } // Cách trị bệnh cho cây
}

public class Alternative
{
    public String? label { get; set; } // tên của loại cây
    public Double? score { get; set; } // độ chính xác
}