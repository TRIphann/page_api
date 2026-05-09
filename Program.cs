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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Logger.LogInformation("═══════════════════════════════════════════");
app.Logger.LogInformation("  Facebook Page API + Webhook Service");
app.Logger.LogInformation("  Kafka topic: {Topic}", app.Configuration["Kafka:Topic"] ?? "raw_events");
app.Logger.LogInformation("  Webhook endpoint: /webhook");
app.Logger.LogInformation("═══════════════════════════════════════════");

app.Run();

