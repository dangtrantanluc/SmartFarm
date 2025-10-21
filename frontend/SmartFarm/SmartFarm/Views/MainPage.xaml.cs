//using System.Net.Http;
//using System.Text.Json;
//using System.Xml.Linq;

//namespace SmartFarm.Views;

//public partial class MainPage : ContentPage
//{
//    private const string ApiKey = "46da9b97f4b96d4a11c23de7f08f8b96";
//    //private const string GNewsUrl =
//    // "https://gnews.io/api/v4/search?q=vietnam&lang=vi&country=vn&max=5&apikey=" + ApiKey;
//    private const string GNewsUrl =
//    "https://gnews.io/api/v4/search?q=nông%20nghiệp%20OR%20agriculture&lang=vi&country=vn&max=10&apikey=" + ApiKey;


//    // Fallback RSS (Vietnamnet - Nông nghiệp)
//    private const string RssFallbackUrl = "https://danviet.vn/rss/nong-nghiep-1001.rss";

//    public MainPage()
//    {
//        InitializeComponent();
//        _ = LoadNewsAsync();
//    }

//    private async Task LoadNewsAsync()
//    {
//        NewsContainer.Children.Clear();
//        NewsContainer.Children.Add(new Label
//        {
//            Text = "📰 Đang tải tin tức...",
//            TextColor = Colors.Gray
//        });

//        bool ok = await TryLoadFromGNews();
//        if (!ok)
//            ok = await TryLoadFromRss();

//        if (!ok)
//        {
//            NewsContainer.Children.Clear();
//            NewsContainer.Children.Add(new Label
//            {
//                Text = "⚠️ Không tải được tin nào. Vui lòng kiểm tra mạng hoặc API key.",
//                TextColor = Colors.Red
//            });
//        }
//    }

//    // ================= GNEWS API =================
//    private async Task<bool> TryLoadFromGNews()
//    {
//        try
//        {
//            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
//            var response = await http.GetAsync(GNewsUrl);

//            // Hiển thị mã phản hồi
//            NewsContainer.Children.Add(new Label
//            {
//                Text = $"[GNews] HTTP {response.StatusCode}",
//                FontSize = 12,
//                TextColor = Colors.Gray
//            });

//            if (!response.IsSuccessStatusCode)
//            {
//                NewsContainer.Children.Add(new Label
//                {
//                    Text = $"Lỗi GNews: {response.ReasonPhrase}",
//                    TextColor = Colors.OrangeRed
//                });
//                return false;
//            }

//            var body = await response.Content.ReadAsStringAsync();
//            if (string.IsNullOrWhiteSpace(body))
//            {
//                NewsContainer.Children.Add(new Label
//                {
//                    Text = "Phản hồi GNews rỗng.",
//                    TextColor = Colors.OrangeRed
//                });
//                return false;
//            }

//            var json = JsonDocument.Parse(body);
//            if (!json.RootElement.TryGetProperty("articles", out var articles))
//            {
//                NewsContainer.Children.Add(new Label
//                {
//                    Text = "Không có trường 'articles' trong JSON GNews.",
//                    TextColor = Colors.OrangeRed
//                });
//                return false;
//            }

//            var list = articles.EnumerateArray().ToList();
//            if (list.Count == 0)
//            {
//                NewsContainer.Children.Add(new Label
//                {
//                    Text = "Không có bài nào từ GNews.",
//                    TextColor = Colors.OrangeRed
//                });
//                return false;
//            }

//            RenderArticles(list.Select(a =>
//            {
//                string title = a.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
//                string desc = a.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
//                string url = a.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
//                string image = a.TryGetProperty("image", out var i) ? i.GetString() ?? "" : "";
//                string source = (a.TryGetProperty("source", out var s) &&
//                                 s.TryGetProperty("name", out var sn)) ? sn.GetString() ?? "" : "";
//                return new Article(title, desc, url, image, source);
//            }));

//            return true;
//        }
//        catch (Exception ex)
//        {
//            NewsContainer.Children.Add(new Label
//            {
//                Text = $"[GNews Exception] {ex.GetType().Name}: {ex.Message}",
//                TextColor = Colors.Red,
//                FontSize = 12
//            });
//            return false;
//        }
//    }

//    // ================= RSS FALLBACK =================
//    private async Task<bool> TryLoadFromRss()
//    {
//        try
//        {
//            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
//            var xml = await http.GetStringAsync(RssFallbackUrl);

//            var doc = XDocument.Parse(xml);
//            var items = doc.Descendants("item").Take(5).Select(item =>
//            {
//                string title = item.Element("title")?.Value ?? "";
//                string link = item.Element("link")?.Value ?? "";
//                string desc = (item.Element("description")?.Value ?? "")
//                    .Replace("<![CDATA[", "").Replace("]]>", "");
//                return new Article(title, desc, link, "", "Vietnamnet RSS");
//            }).ToList();

//            if (items.Count == 0)
//            {
//                NewsContainer.Children.Add(new Label
//                {
//                    Text = "[RSS] Không tìm thấy <item> nào trong feed.",
//                    TextColor = Colors.OrangeRed
//                });
//                return false;
//            }

//            RenderArticles(items);
//            return true;
//        }
//        catch (Exception ex)
//        {
//            NewsContainer.Children.Add(new Label
//            {
//                Text = $"[RSS Exception] {ex.GetType().Name}: {ex.Message}",
//                TextColor = Colors.Red,
//                FontSize = 12
//            });
//            return false;
//        }
//    }

//    // ================= Render UI =================
//    private void RenderArticles(IEnumerable<Article> articles)
//    {
//        NewsContainer.Children.Clear();

//        foreach (var a in articles)
//        {
//            var layout = new VerticalStackLayout { Spacing = 6 };

//            if (!string.IsNullOrEmpty(a.Image))
//            {
//                layout.Children.Add(new Image
//                {
//                    Source = ImageSource.FromUri(new Uri(a.Image)),
//                    HeightRequest = 120,
//                    Aspect = Aspect.AspectFit
//                });
//            }

//            layout.Children.Add(new Label
//            {
//                Text = a.Title,
//                FontAttributes = FontAttributes.Bold,
//                FontSize = 18,
//                TextColor = Colors.DarkGreen
//            });

//            if (!string.IsNullOrEmpty(a.Description))
//            {
//                layout.Children.Add(new Label
//                {
//                    Text = a.Description,
//                    FontSize = 14,
//                    TextColor = Colors.Black
//                });
//            }

//            layout.Children.Add(new Label
//            {
//                Text = $"Nguồn: {a.Source}",
//                FontSize = 12,
//                TextColor = Colors.Gray
//            });

//            var link = new Label
//            {
//                Text = "👉 Xem chi tiết",
//                TextColor = Colors.Blue,
//                FontAttributes = FontAttributes.Bold
//            };

//            var tap = new TapGestureRecognizer();
//            tap.Tapped += async (_, __) =>
//            {
//                if (!string.IsNullOrEmpty(a.Url))
//                    await Launcher.Default.OpenAsync(a.Url);
//            };
//            link.GestureRecognizers.Add(tap);

//            layout.Children.Add(link);

//            NewsContainer.Children.Add(new Frame
//            {
//                BorderColor = Colors.LightGray,
//                CornerRadius = 10,
//                Margin = new Thickness(0, 10),
//                HasShadow = true,
//                Padding = 10,
//                Content = layout
//            });
//        }
//    }

//    private record Article(string Title, string Description, string Url, string Image, string Source);
//}
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace SmartFarm.Views;

public partial class MainPage : ContentPage
{
    private const string ApiKey = "46da9b97f4b96d4a11c23de7f08f8b96";

    // GNews lấy tin tiếng Việt về nông nghiệp (dự phòng)
    private const string GNewsUrl =
        "https://gnews.io/api/v4/search?q=nông%20nghiệp%20OR%20agriculture&lang=vi&country=vn&max=10&apikey=" + ApiKey;

    // RSS Dân Việt (feed tổng)
    private const string RssFallbackUrl = "https://danviet.vn/dv-tin-tuc.rss";

    public MainPage()
    {
        InitializeComponent();
        _ = LoadNewsAsync();
    }

    private async Task LoadNewsAsync()
    {
        NewsContainer.Children.Clear();
        NewsContainer.Children.Add(new Label
        {
            Text = "📰 Đang tải tin tức...",
            TextColor = Colors.Gray
        });

        bool ok = await TryLoadFromGNews();
        if (!ok)
            ok = await TryLoadFromRss();

        if (!ok)
        {
            NewsContainer.Children.Clear();
            NewsContainer.Children.Add(new Label
            {
                Text = "⚠️ Không tải được tin nào. Vui lòng kiểm tra mạng hoặc API key.",
                TextColor = Colors.Red
            });
        }
    }

    // ================= GNEWS API =================
    private async Task<bool> TryLoadFromGNews()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await http.GetAsync(GNewsUrl);

            NewsContainer.Children.Add(new Label
            {
                Text = $"[GNews] HTTP {response.StatusCode}",
                FontSize = 12,
                TextColor = Colors.Gray
            });

            if (!response.IsSuccessStatusCode) return false;

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) return false;

            var json = JsonDocument.Parse(body);
            if (!json.RootElement.TryGetProperty("articles", out var articles))
                return false;

            var list = articles.EnumerateArray().ToList();
            if (list.Count == 0) return false;

            RenderArticles(list.Select(a =>
            {
                string title = a.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                string desc = a.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                string url = a.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
                string image = a.TryGetProperty("image", out var i) ? i.GetString() ?? "" : "";
                string source = (a.TryGetProperty("source", out var s) &&
                                 s.TryGetProperty("name", out var sn)) ? sn.GetString() ?? "" : "";
                return new Article(title, desc, url, image, source);
            }));

            return true;
        }
        catch (Exception ex)
        {
            NewsContainer.Children.Add(new Label
            {
                Text = $"[GNews Exception] {ex.GetType().Name}: {ex.Message}",
                TextColor = Colors.Red,
                FontSize = 12
            });
            return false;
        }
    }

    // ================= RSS FALLBACK =================
    private async Task<bool> TryLoadFromRss()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var xml = await http.GetStringAsync(RssFallbackUrl);
            var doc = XDocument.Parse(xml);

            var items = doc.Descendants("item")
                .Select(item =>
                {
                    string title = item.Element("title")?.Value ?? "";
                    string link = item.Element("link")?.Value ?? "";
                    string desc = item.Element("description")?.Value ?? "";
                    string image = item.Element("image")?.Value ?? "";

                    // Nếu chưa có ảnh, tìm ảnh trong mô tả HTML
                    if (string.IsNullOrEmpty(image))
                    {
                        var match = Regex.Match(desc, @"https://[^""']+\.(jpg|png)");
                        if (match.Success)
                            image = match.Value;
                    }

                    // Làm sạch mô tả
                    desc = Regex.Replace(desc, "<.*?>", string.Empty)
                                .Replace("<![CDATA[", "")
                                .Replace("]]>", "")
                                .Trim();

                    return new Article(title, desc, link, image, "Dân Việt RSS");
                })
                // 🔍 Chỉ lấy tin có từ khóa "nông nghiệp" hoặc "nông sản"
                .Where(a =>
                    a.Title.Contains("nông nghiệp", StringComparison.OrdinalIgnoreCase) ||
                    a.Description.Contains("nông nghiệp", StringComparison.OrdinalIgnoreCase) ||
                    a.Title.Contains("nông sản", StringComparison.OrdinalIgnoreCase) ||
                    a.Description.Contains("nông sản", StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            if (items.Count == 0)
            {
                NewsContainer.Children.Add(new Label
                {
                    Text = "[RSS] Không có bài nào liên quan đến nông nghiệp.",
                    TextColor = Colors.OrangeRed
                });
                return false;
            }

            RenderArticles(items);
            return true;
        }
        catch (Exception ex)
        {
            NewsContainer.Children.Add(new Label
            {
                Text = $"[RSS Exception] {ex.GetType().Name}: {ex.Message}",
                TextColor = Colors.Red,
                FontSize = 12
            });
            return false;
        }
    }

    // ================= Render UI =================
    private void RenderArticles(IEnumerable<Article> articles)
    {
        NewsContainer.Children.Clear();

        foreach (var a in articles)
        {
            var layout = new VerticalStackLayout { Spacing = 6 };

            // Ảnh (nếu có)
            if (!string.IsNullOrEmpty(a.Image))
            {
                layout.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(new Uri(a.Image)),
                    HeightRequest = 140,
                    Aspect = Aspect.AspectFill,
                    Margin = new Thickness(0, 0, 0, 5)
                });
            }

            // Tiêu đề
            layout.Children.Add(new Label
            {
                Text = a.Title,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                TextColor = Colors.DarkGreen
            });

            // Mô tả
            if (!string.IsNullOrEmpty(a.Description))
            {
                layout.Children.Add(new Label
                {
                    Text = a.Description,
                    FontSize = 14,
                    TextColor = Colors.Black
                });
            }

            // Nguồn
            layout.Children.Add(new Label
            {
                Text = $"Nguồn: {a.Source}",
                FontSize = 12,
                TextColor = Colors.Gray
            });

            // Nút xem chi tiết
            var link = new Label
            {
                Text = "👉 Xem chi tiết",
                TextColor = Colors.Blue,
                FontAttributes = FontAttributes.Bold
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) =>
            {
                if (!string.IsNullOrEmpty(a.Url))
                    await Launcher.Default.OpenAsync(a.Url);
            };
            link.GestureRecognizers.Add(tap);
            layout.Children.Add(link);

            // Gói trong frame
            NewsContainer.Children.Add(new Frame
            {
                BorderColor = Colors.LightGray,
                CornerRadius = 10,
                Margin = new Thickness(0, 10),
                HasShadow = true,
                Padding = 10,
                Content = layout
            });
        }
    }

    private record Article(string Title, string Description, string Url, string Image, string Source);
}
