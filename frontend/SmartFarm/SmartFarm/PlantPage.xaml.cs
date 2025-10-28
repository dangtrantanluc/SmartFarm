using System.IO;  // Thêm cho MemoryStream
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CommunityToolkit.Maui.Core.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace SmartFarm;
public partial class PlantPage : ContentPage
{
    FileResult? photo;
    private readonly HttpClient _httpClient = new();

    private bool isLoading;

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


        _httpClient.BaseAddress = new Uri("http://192.168.88.144:8000");
        _httpClient.Timeout = TimeSpan.FromSeconds(180);


        BindingContext = this; // ⚠️ Bắt buộc

    }

    public bool IsLoading
    {
        get => isLoading;
        set
        {
            if (isLoading == value)
                return;
            isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
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

        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(content, "file", System.IO.Path.GetFileName(photo.FileName));
        // Thêm tham số top_k
        form.Add(new StringContent("3"), "top_k");


        try
        {
            IsLoading = true;
            // Gửi request tới FastAPI
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

                var predictLabel = result.predicted;
                var confidence = result.confidence;
                MainResult.Children.Clear();


                Label diseaseNameLb = new Label { Text = $"Tên bệnh: {predictLabel}", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1B5E20") };
                Label predictScoreLb = new Label { Text = $"Độ chính xác: {confidence}", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#388E3C") };

                var newResultFrame = new Border
                {
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(15)
                    },
                    Background = new SolidColorBrush(Color.FromArgb("#E8F5E9")),
                    Padding = 15,
                    Shadow = new Shadow
                    {
                        Brush = new SolidColorBrush(Colors.Gray),
                        Opacity = 0.5f,     // Độ mờ của bóng
                        Offset = new Point(5, 5), // Vị trí bóng (ngang, dọc)
                        Radius = 10         // Độ tán của bóng
                    },
                    Content = new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Label{Text="🌿 Kết quả dự đoán", FontSize=22, FontAttributes=FontAttributes.Bold, TextColor=Color.FromArgb("#2E7D32")},
                            diseaseNameLb,
                            predictScoreLb,
                        }
                    }
                };

                Label plantLb;
                Label symtomsLb;
                Label preventionLb;
                Label treatmentLb;
                MainResult.Children.Add(newResultFrame);
                var newGuide = new VerticalStackLayout
                {
                    Spacing = 10
                };
                var newGuideFrame = new Border
                {
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(15)
                    },
                    Background = new SolidColorBrush(Color.FromArgb("#FFF3E0")),
                    Padding = 15,
                    Shadow = new Shadow
                    {
                        Brush = new SolidColorBrush(Colors.Gray),
                        Opacity = 0.5f,     // Độ mờ của bóng
                        Offset = new Point(5, 5), // Vị trí bóng (ngang, dọc)
                        Radius = 10         // Độ tán của bóng
                    },
                    Content = newGuide
                };
                newGuideFrame.IsVisible = false;
                

                if (result.guide != null)
                {
                    newGuide.Children.Clear();
                    newGuideFrame.IsVisible = true;
                    Label titleLb = new Label { Text = "📖 Hướng dẫn chăm sóc và trị bệnh", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#E65100") };
                    plantLb = new Label { Text = $"Cây trồng: {result.guide.plant}", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#BF360C") };
                    symtomsLb = new Label { Text = $"• Dấu hiệu: {result.guide.symptoms}", TextColor = Color.FromArgb("#000000") };
                    preventionLb = new Label { Text = $"• Phòng ngừa: {result.guide.prevention}", TextColor = Color.FromArgb("#000000") };
                    treatmentLb = new Label { Text = $"• Cách trị: {result.guide.treatment}", TextColor = Color.FromArgb("#000000") };
                       
                    newGuide.Children.Add(titleLb);
                    newGuide.Children.Add(plantLb);
                    newGuide.Children.Add(symtomsLb);
                    newGuide.Children.Add(preventionLb);
                    newGuide.Children.Add(treatmentLb);
                }

                MainResult.Children.Add(newGuideFrame);

                if (result.alternatives != null && result.alternatives.Count > 0)
                {
                    var alternativeCollection = new CollectionView
                    {
                        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
                        ItemTemplate = new DataTemplate(() =>
                        {
                            var frame = new Border
                            {
                                StrokeShape = new RoundRectangle
                                {
                                    CornerRadius = new CornerRadius(10)
                                },
                                Background = new SolidColorBrush(Color.FromArgb("#FFFFFF")),
                                Padding = 10,
                                Margin = 0.5,
                                Shadow = new Shadow
                                {
                                    Brush = new SolidColorBrush(Colors.Gray),
                                    Opacity = 0.5f,     // Độ mờ của bóng
                                    Offset = new Point(5, 5), // Vị trí bóng (ngang, dọc)
                                    Radius = 10         // Độ tán của bóng
                                }
                            };
                            var grid = new Grid
                            {
                                ColumnDefinitions = new ColumnDefinitionCollection
                                            {
                                                new ColumnDefinition { Width = GridLength.Star }, // *
                                                new ColumnDefinition { Width = GridLength.Auto }  // Auto
                                            }
                            };

                            var nameLabel = new Label
                            {
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#1565C0")
                            };
                            nameLabel.SetBinding(Label.TextProperty, "label");

                            var scoreLabel = new Label
                            {
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#0D47A1")
                            };
                            scoreLabel.SetBinding(Label.TextProperty, new Binding("score", stringFormat: "{0:P2}"));
                            Grid.SetColumn(scoreLabel, 1);


                            //Thêm vào grid
                            grid.Add(nameLabel);
                            grid.Add(scoreLabel);

                            //Gắn grid vào frame    
                            frame.Content = grid;

                            var tapGesture = new TapGestureRecognizer();
                            tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, new Binding("."));
                            try
                            {

                                tapGesture.Tapped += (s, e) =>
                                {
                                    // Xử lý khi Border được nhấn
                                    if (s is View view &&
                                        view.BindingContext is Alternative tappedItem)
                                    {
                                        diseaseNameLb.Text = $"Tên bệnh: {tappedItem.label}";
                                        predictScoreLb.Text = $"Độ chính xác: {tappedItem.score}";
                                        if (tappedItem.guide != null)
                                        {
                                            newGuide.Children.Clear();
                                            newGuideFrame.IsVisible = true;
                                            Label titleLb = new Label { Text = "📖 Hướng dẫn chăm sóc và trị bệnh", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#E65100") };
                                            plantLb = new Label { Text = $"Cây trồng: {tappedItem.guide.plant}", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#BF360C") };
                                            symtomsLb = new Label { Text = $"• Dấu hiệu: {tappedItem.guide.symptoms}", TextColor = Color.FromArgb("#000000") };
                                            preventionLb = new Label { Text = $"• Phòng ngừa: {tappedItem.guide.prevention}", TextColor = Color.FromArgb("#000000") };
                                            treatmentLb = new Label { Text = $"• Cách trị: {tappedItem.guide.treatment}", TextColor = Color.FromArgb("#000000") };


                                            newGuide.Children.Add(titleLb);
                                            newGuide.Children.Add(plantLb);
                                            newGuide.Children.Add(symtomsLb);
                                            newGuide.Children.Add(preventionLb);
                                            newGuide.Children.Add(treatmentLb);
                                        }
                                        else
                                        {
                                            newGuide.Children.Clear();
                                            newGuideFrame.IsVisible = false;
                                        }
                                    };
                                };
                            }
                            catch (Exception ex)
                            {
                                DisplayAlert("Lỗi", ex.ToString(), "OK");
                            }
                            frame.GestureRecognizers.Add(tapGesture);

                            return frame;
                        })
                    };
                    alternativeCollection.ItemsSource = result.alternatives;

                    var alternativeLabel = new Label { Text = "🔍 Các khả năng khác", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0D47A1"), Margin = new Thickness(0, 0, 0, 10) };

                    var temp = new VerticalStackLayout { };
                    temp.Children.Add(alternativeLabel);
                    temp.Children.Add(alternativeCollection);

                    var newAlternativesFrame = new Border
                    {
                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = new CornerRadius(15)
                        },
                        Background = new SolidColorBrush(Color.FromArgb("#E3F2FD")),
                        Padding = 15
                    };

                    newAlternativesFrame.Content = temp;

                    MainResult.Children.Add(newAlternativesFrame);
                }
            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", ex.ToString(), "OK");
        }
        finally
        {
            IsLoading = false; 
        }
    }

}



public class DiseaseResp
{
    public string? predicted { get; set; } // Tên loại bệnh
    public double? confidence { get; set; } // độ chính xác
    public List<Alternative>? alternatives { get; set; } // Những dự đoán có thể liên quan
    public Guide? guide { get; set; } // Dấu hiệu, phòng và trị bệnh cho loại bệnh được dự đoán.
}
public class Guide
{
    public string? plant { get; set; } //Tên cây trồng
    public string? symptoms { get; set; } // dấu hiệu của bệnh 
    public string? prevention { get; set; } //Phòng ngừa bệnh cho  cây
    public string? treatment { get; set; } // Cách trị bệnh cho cây
}

public class Alternative
{
    public String? label { get; set; } // tên của loại cây
    public Guide? guide { get; set; } // Dấu hiệu, phòng và trị bệnh cho loại bệnh được dự đoán.
    public Double? score { get; set; } // độ chính xác
}