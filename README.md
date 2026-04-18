# 📘 Facebook Page API - ASP.NET Core MVC

Ứng dụng quản lý Facebook Page thông qua Graph API v25.0, được xây dựng bằng ASP.NET Core MVC với giao diện Dashboard hiện đại.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)
![Facebook](https://img.shields.io/badge/Facebook_Graph_API-v25.0-1877F2?style=flat&logo=facebook)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

## 🚀 Tính năng

| Chức năng | Endpoint | Method |
|---|---|---|
| Xem thông tin trang | `/api/page/info` | GET |
| Lấy danh sách bài viết | `/api/page/posts` | GET |
| Đăng bài viết mới | `/api/page/posts` | POST |
| Xóa bài viết | `/api/page/post/{postId}` | DELETE |
| Xem comments bài viết | `/api/page/post/{postId}/comments` | GET |
| Xem likes bài viết | `/api/page/post/{postId}/likes` | GET |
| Thống kê trang | `/api/page/insights` | GET |

## 📋 Yêu cầu

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- [Facebook Developer Account](https://developers.facebook.com/)
- Facebook Page với quyền quản trị

## ⚙️ Cài đặt & Cấu hình

### 1. Clone repository

```bash
git clone https://github.com/TRIphann/page_api.git
cd page_api
```

### 2. Lấy Facebook Page Access Token

1. Truy cập [Graph API Explorer](https://developers.facebook.com/tools/explorer/)
2. Chọn App trong dropdown **"Ứng dụng trên Meta"**
3. Cấp quyền (Permissions):
   - `read_insights`
   - `pages_show_list`
   - `pages_read_engagement`
   - `pages_manage_metadata`
   - `pages_read_user_content`
   - `pages_manage_posts`
4. **Quan trọng:** Chọn dropdown **"Người dùng hoặc Trang"** → chọn **tên Page** (KHÔNG chọn "Mã người dùng")
5. Copy Access Token

### 3. Cập nhật thông tin

Mở file `Controllers/PageApiController.cs` và thay đổi:

```csharp
private const string PAGE_ID = "YOUR_PAGE_ID";
private const string ACCESS_TOKEN = "YOUR_PAGE_ACCESS_TOKEN";
```

### 4. Chạy ứng dụng

```bash
dotnet run
```

Truy cập: **http://localhost:5051**

## 🏗️ Kiến trúc dự án

```
facbook_page_api/
├── Controllers/
│   ├── HomeController.cs          # Điều hướng trang chủ
│   └── PageApiController.cs       # REST API endpoints
├── Models/
│   └── FacebookModels.cs          # Data models (Page, Post, Comment,...)
├── Services/
│   ├── IFacebookGraphService.cs   # Interface service
│   └── FacebookGraphService.cs    # Gọi Facebook Graph API
├── Views/
│   └── Home/
│       └── Index.cshtml           # Dashboard UI
├── Program.cs                     # Cấu hình ứng dụng
└── appsettings.json               # Cấu hình chung
```

### Luồng hoạt động

```
Browser → Controller → FacebookGraphService → Facebook Graph API v25.0
                ↓
          Response JSON → Dashboard hiển thị kết quả
```

## 📖 Hướng dẫn sử dụng

### Lấy thông tin trang
Bấm **GET /api/page/info** → Hiển thị tên, category, số fan, ảnh đại diện.

### Lấy bài viết
Bấm **GET /api/page/posts** → Danh sách bài viết với `id`, `message`, `created_time`.

### Đăng bài mới
1. Bấm **POST /api/page/posts**
2. Nhập nội dung bài viết vào ô "Nội dung bài viết"
3. Bấm lại nút POST → Bài viết được đăng lên Facebook Page

### Xóa bài viết
1. Copy Post ID từ kết quả GET posts (dạng: `pageId_postId`)
2. Dán vào ô **Post ID**
3. Bấm **DEL /api/page/post/{postId}**

### Xem comments / likes
1. Nhập Post ID
2. Bấm **GET .../comments** hoặc **GET .../likes**

## ⚠️ Lưu ý

- **Access Token** hết hạn sau ~1-2 giờ → cần lấy lại từ Graph API Explorer
- **Page Token ≠ User Token**: Phải chọn tên Page trong dropdown, không chọn "Mã người dùng"
- **Post ID** có dạng `pageId_postId` (vd: `1046712038534955_122093172452504915`)
- Dùng endpoint `/posts` thay vì `/feed` cho Trải nghiệm Trang mới (New Pages Experience)
- Nhiều metrics insights cũ đã bị deprecated trong API v25.0

## 🛠️ Công nghệ sử dụng

- **Backend:** ASP.NET Core 9.0 MVC
- **HTTP Client:** `HttpClient` (Dependency Injection)
- **Facebook API:** Graph API v25.0
- **Frontend:** HTML, CSS, JavaScript (Vanilla)
- **Serialization:** `System.Text.Json`

## 👤 Tác giả

**TRIphann** - [GitHub](https://github.com/TRIphann)
