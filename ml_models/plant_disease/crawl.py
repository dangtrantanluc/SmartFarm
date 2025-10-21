from newspaper import Article
import requests
from bs4 import BeautifulSoup

base_url = "https://khuyennong.vn/trong-trot/ky-thuat-trong-trot"

# Lấy danh sách bài viết
html = requests.get(base_url).text
soup = BeautifulSoup(html, "html.parser")

links = [
    "https://khuyennong.vn" + a["href"]
    for a in soup.select(".news-item a")
    if a["href"].endswith(".html")
]

# Tải nội dung từng bài
data = []
for link in links:
    article = Article(link, language="vi")
    article.download()
    article.parse()
    data.append({
        "title": article.title,
        "url": link,
        "text": article.text
    })

print(f"Đã thu thập {len(data)} bài.")
