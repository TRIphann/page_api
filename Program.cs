using facbook_page_api.Services;

// Remove polling references - now using Webhook only

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register HttpClient and Facebook Graph Service
builder.Services.AddHttpClient<IFacebookGraphService, FacebookGraphService>();

// ========== Webhook Services ==========

// Kafka Producer - Singleton để reuse producer instance
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

// Event Normalizer - chuẩn hóa payload Facebook → schema thống nhất
builder.Services.AddTransient<IEventNormalizerService, EventNormalizerService>();

// Signature Validator - xác thực chữ ký HMAC-SHA256
builder.Services.AddSingleton<ISignatureValidator, SignatureValidator>();

// ============================================================
// WEBHOOK ONLY - Realtime comments via Facebook Webhook
// ============================================================

// Kafka Consumer - đọc từ Kafka → push realtime tới browser qua SSE
builder.Services.AddHostedService<KafkaConsumerService>();

// Add CORS for API testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

// Privacy policy page - public HTTPS URL required by Facebook App Review
app.MapGet("/privacy", () =>
{
    const string html = @"<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Chinh Sach Bao Mat - Facebook Page API</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.8; color: #1a1a2e; background: #f8f9fa; padding: 40px 20px; }
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 50px; border-radius: 12px; box-shadow: 0 2px 20px rgba(0,0,0,0.08); }
        h1 { font-size: 28px; color: #1877f2; margin-bottom: 8px; border-bottom: 2px solid #e8e8e8; padding-bottom: 16px; }
        .meta { font-size: 13px; color: #666; margin-bottom: 30px; }
        h2 { font-size: 18px; color: #333; margin-top: 28px; margin-bottom: 10px; }
        p { margin-bottom: 12px; color: #444; }
        ul { margin: 10px 0 12px 24px; }
        li { margin-bottom: 6px; color: #444; }
        .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #e8e8e8; font-size: 13px; color: #888; text-align: center; }
        a { color: #1877f2; text-decoration: none; }
        a:hover { text-decoration: underline; }
        strong { color: #333; }
    </style>
</head>
<body>
<div class='container'>
    <h1>Chinh Sach Bao Mat</h1>
    <p class='meta'><strong>Ung dung:</strong> Facebook Page API Dashboard<br><strong>Ngay hieu luc:</strong> 10/05/2026<br><strong>Phien ban:</strong> 1.0</p>
    <h2>1. Muc dich thu thap thong tin</h2>
    <p>Ung dung nay duoc thiet ke de quan ly va hien thi noi dung tu Facebook Page cua nguoi dung, bao gom bai viet, binh luan va phan hoi theo thoi gian thuc.</p>
    <p>Chung toi thu thap cac thong tin sau tu Facebook Page cua ban:</p>
    <ul>
        <li>Thong tin cong khai cua Page (ten, anh dai dien, mo ta)</li>
        <li>Danh sach bai viet tren Page</li>
        <li>Binh luan tren cac bai viet</li>
        <li>Phan hoi (reactions) tren bai viet va binh luan</li>
    </ul>
    <h2>2. Cach su dung thong tin</h2>
    <p>Thong tin thu thap duoc su dung cho cac muc dich sau:</p>
    <ul>
        <li>Hien thi bai viet va binh luan tren giao dien web</li>
        <li>Cap nhat noi dung theo thoi gian thuc thong qua webhook</li>
        <li>Quan ly noi dung Page (dang bai, xoa bai viet)</li>
        <li>Phan tich va thong ke hoat dong cua Page</li>
    </ul>
    <h2>3. Luu tru du lieu</h2>
    <p>Du lieu duoc xu ly va hien thi trong thoi gian thuc. Ung dung khong luu tru vinh vien du lieu tu Facebook. Tat ca du lieu duoc xu ly tam thoi trong bo nho server va chi phuc vu muc dich hien thi.</p>
    <p>Ung dung su dung Apache Kafka lam message broker de truyen tai su kien realtime. Du lieu trong Kafka duoc cau hinh voi thoi gian luu tru gioi han.</p>
    <h2>4. Chia se thong tin</h2>
    <p>Chung toi cam ket khong chia se, ban hoac chuyen giao thong tin ca nhan cua nguoi dung cho bat ky ben thu ba nao. Tat ca du lieu chi duoc xu ly noi bo va phuc vu muc dich hien thi tren giao dien cua ung dung.</p>
    <p>Ung dung ket noi truc tiep voi Facebook Graph API thong qua Page Access Token do nguoi dung cung cap. Viec su dung du lieu Facebook tuan thu theo <a href='https://www.facebook.com/policies/' target='_blank'>Chinh sach nen tang Facebook</a>.</p>
    <h2>5. Bao mat du lieu</h2>
    <p>Chung toi ap dung cac bien phap bao mat sau:</p>
    <ul>
        <li>Xac thuc webhook bang chu ky HMAC-SHA256 tu Facebook</li>
        <li>Ket noi qua HTTPS/SSL de ma hoa du lieu truyen tai</li>
        <li>Page Access Token duoc luu tru cuc bo trong cau hinh ung dung, khong chia se cong khai</li>
        <li>Khong thu thap hay luu tru thong tin ca nhan cua nguoi dung Facebook ngoai noi dung binh luan cong khai</li>
    </ul>
    <h2>6. Quyen cua nguoi dung</h2>
    <p>Nguoi dung co quyen:</p>
    <ul>
        <li>Truy cap va xem tat ca du lieu duoc hien thi tu Facebook Page cua minh</li>
        <li>Xoa hoac go bai viet truc tiep tu giao dien ung dung</li>
        <li>Huy ket noi ung dung khoi Facebook Page bat ky luc nao thong qua cai dat Facebook</li>
        <li>Yeu cau xoa du lieu tam thoi dang duoc xu ly trong he thong</li>
    </ul>
    <h2>7. Cookies va cong nghe theo doi</h2>
    <p>Ung dung nay khong su dung cookies de theo doi nguoi dung. Chung toi khong su dung bat ky cong cu phan tich hoac quang cao cua ben thu ba nao.</p>
    <h2>8. Quyen truy cap Facebook</h2>
    <p>Ung dung yeu cau cac quyen sau tu Facebook:</p>
    <ul>
        <li><strong>pages_read_engagement</strong> - Doc noi dung va binh luan tren Page</li>
        <li><strong>pages_manage_posts</strong> - Dang va xoa bai viet tren Page</li>
        <li><strong>pages_manage_metadata</strong> - Quan ly webhook va subscription</li>
    </ul>
    <p>Cac quyen nay chi duoc su dung theo pham vi ma nguoi dung da cap phep va chi phuc vu muc dich duoc neu trong chinh sach nay.</p>
    <h2>9. Thay doi chinh sach</h2>
    <p>Chung toi co the cap nhat Chinh sach Bao Mat nay theo thoi gian. Moi thay doi se duoc thong bao thong qua viec cap nhat ngay hieu luc tren trang nay. Chung toi kuyen khich nguoi dung xem lai chinh sach nay dinh ky.</p>
    <h2>10. Lien he</h2>
    <p>Neu ban co bat ky cau hoi hoac lo ngai nao ve Chinh sach Bao Mat nay, vui long lien he:</p>
    <p>Email: <strong>support@example.com</strong></p>
    <div class='footer'>
        <p>Chinh Sach Bao Mat nay duoc tao nham dap ung yeu cau cua Facebook App Review cho ung dung Facebook Page API Dashboard.</p>
    </div>
</div>
</body>
</html>";
    return Results.Text(html, "text/html; charset=utf-8");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Logger.LogInformation("═══════════════════════════════════════════");
app.Logger.LogInformation("  Facebook Page API + Webhook Service");
app.Logger.LogInformation("  Kafka topic: {Topic}", app.Configuration["Kafka:Topic"] ?? "raw_events");
app.Logger.LogInformation("  Webhook endpoint: /webhook");
app.Logger.LogInformation("═══════════════════════════════════════════");

app.Urls.Add("http://localhost:3001");

app.Run();

