# Facebook Page API - ASP.NET Core MVC

A Facebook Page management application with real-time webhook event processing, built with ASP.NET Core MVC and integrated with Apache Kafka.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)
![Facebook](https://img.shields.io/badge/Facebook_Graph_API-v25.0-1877F2?style=flat&logo=facebook)
![Kafka](https://img.shields.io/badge/Apache_Kafka-Docker-231F20?style=flat&logo=apachekafka)

## Features

### Facebook Graph API Endpoints

| Feature | Endpoint | Method |
|---------|----------|--------|
| Get page info | `/api/page/info` | GET |
| List posts | `/api/page/posts` | GET |
| Create a post | `/api/page/posts` | POST |
| Delete a post | `/api/page/post/{postId}` | DELETE |
| Get post comments | `/api/page/post/{postId}/comments` | GET |
| Get post likes | `/api/page/post/{postId}/likes` | GET |
| Page insights | `/api/page/insights` | GET |

### Real-time Webhook (Event-Driven Architecture)

| Feature | Endpoint | Method |
|---------|----------|--------|
| Facebook webhook verification | `/webhook` | GET |
| Receive real-time events | `/webhook` | POST |
| Webhook health check | `/webhook/health` | GET |

When a new comment is posted on the Facebook Page, the webhook endpoint receives the event in real-time, normalizes it into a standard schema, and publishes it to the Kafka topic `raw_events`.

## Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- [Facebook Developer Account](https://developers.facebook.com/)
- Facebook Page with admin access
- Docker with Apache Kafka container running on port 9092
- Kafka topic `raw_events` created

## Setup and Configuration

### 1. Clone the repository

```bash
git clone https://github.com/TRIphann/page_api.git
cd page_api
```

### 2. Get a Facebook Page Access Token

1. Go to [Graph API Explorer](https://developers.facebook.com/tools/explorer/)
2. Select your App from the dropdown
3. Grant the following permissions:
   - `read_insights`
   - `pages_show_list`
   - `pages_read_engagement`
   - `pages_manage_metadata`
   - `pages_read_user_content`
   - `pages_manage_posts`
4. **Important:** In the "User or Page" dropdown, select the **Page name** (not "User Token")
5. Copy the Access Token

### 3. Update configuration

Edit `appsettings.json`:

```json
{
  "Facebook": {
    "AppId": "YOUR_APP_ID",
    "AppSecret": "YOUR_APP_SECRET",
    "VerifyToken": "my_verify_token",
    "PageId": "YOUR_PAGE_ID",
    "PageAccessToken": "YOUR_PAGE_ACCESS_TOKEN"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "Topic": "raw_events"
  }
}
```

- `AppSecret`: Required for webhook signature validation (leave empty to skip in development)
- `VerifyToken`: Token used during Facebook webhook subscription verification

### 4. Restore dependencies and run

```bash
dotnet restore
dotnet run
```

The application will start at: **http://localhost:5051**

## Project Architecture

```
facbook_page_api/
├── Controllers/
│   ├── HomeController.cs              # Home page routing
│   ├── PageApiController.cs           # Facebook Graph API endpoints
│   └── WebhookController.cs           # Webhook receiver (GET verify + POST events)
├── Models/
│   ├── FacebookModels.cs              # Data models (Page, Post, Comment, ...)
│   └── WebhookModels.cs               # Webhook payload + NormalizedEvent schema
├── Services/
│   ├── IFacebookGraphService.cs       # Graph API service interface
│   ├── FacebookGraphService.cs        # Facebook Graph API client
│   ├── KafkaProducerService.cs        # Kafka producer (publish to raw_events)
│   ├── EventNormalizerService.cs      # Normalize webhook payloads to standard schema
│   └── SignatureValidator.cs          # HMAC-SHA256 signature validation
├── Views/
│   └── Home/
│       └── Index.cshtml               # Dashboard UI
├── Program.cs                         # App configuration and DI registration
└── appsettings.json                   # Application settings
```

### Request Flow - Graph API

```
Browser --> PageApiController --> FacebookGraphService --> Facebook Graph API v25.0
                  |
                  v
            JSON Response --> Dashboard displays results
```

### Request Flow - Real-time Webhook

```
Facebook (new comment on Page)
       |
       v  HTTP POST
  WebhookController (/webhook)
       |
       v
  SignatureValidator         --> Verify X-Hub-Signature-256 (HMAC-SHA256)
       |
       v
  EventNormalizerService     --> Convert Comment/Message to NormalizedEvent
       |
       v
  KafkaProducerService       --> Publish to Kafka topic "raw_events"
       |
       v
  Apache Kafka (Docker, port 9092)
```

## Normalized Event Schema

All Facebook events (comments, messages, reactions) are normalized into this unified schema before publishing to Kafka:

```json
{
  "event_id": "dcc77bca-28a0-4989-ad0b-dbfdd6f109b9",
  "event_type": "comment",
  "verb": "add",
  "page_id": "1046712038534955",
  "object_id": "1046712038534955_888",
  "post_id": "1046712038534955_999",
  "parent_id": null,
  "content": "Hello! This is a new comment!",
  "sender": {
    "id": "123456789",
    "name": "Nguyen Van A"
  },
  "timestamp": 1714012800,
  "received_at": "2026-04-25T08:19:20Z",
  "metadata": {
    "source": "feed",
    "field": "feed"
  }
}
```

## Facebook Webhook Registration

To receive real-time events from Facebook:

1. Go to [Facebook Developers](https://developers.facebook.com) and select your App
2. Navigate to **Webhooks** and select **Page**
3. Configure:
   - **Callback URL**: `https://<your-domain>/webhook` (use [ngrok](https://ngrok.com/) for local testing: `ngrok http 5051`)
   - **Verify Token**: `my_verify_token` (must match `Facebook:VerifyToken` in `appsettings.json`)
   - **Subscriptions**: Select `feed` for comments and `messages` for direct messages
4. Facebook sends a GET request to verify the endpoint, and upon success the subscription is active

## Usage Guide

### Get Page Info
Click **GET /api/page/info** to display the page name, category, fan count, and profile picture.

### List Posts
Click **GET /api/page/posts** to view all posts with their `id`, `message`, and `created_time`.

### Create a Post
1. Click **POST /api/page/posts**
2. Enter the post content
3. Submit to publish the post to the Facebook Page

### Delete a Post
1. Copy the Post ID from the GET posts result (format: `pageId_postId`)
2. Paste it into the **Post ID** field
3. Click **DEL /api/page/post/{postId}**

### View Comments / Likes
1. Enter a Post ID
2. Click **GET .../comments** or **GET .../likes**

### Test Webhook Locally
Send a simulated comment event using PowerShell:

```powershell
$body = @'
{
  "object": "page",
  "entry": [{
    "id": "1046712038534955",
    "time": 1714012800,
    "changes": [{
      "field": "feed",
      "value": {
        "item": "comment",
        "verb": "add",
        "message": "Test comment",
        "from": {"id": "123", "name": "Test User"},
        "post_id": "1046712038534955_999",
        "comment_id": "1046712038534955_888",
        "created_time": 1714012800
      }
    }]
  }]
}
'@

Invoke-WebRequest -Uri "http://localhost:5051/webhook" -Method POST -Body $body -ContentType "application/json"
```

## Important Notes

- **Access Token** expires after approximately 1-2 hours. Regenerate it from the Graph API Explorer.
- **Page Token vs User Token**: Always select the Page name in the dropdown, not "User Token".
- **Post ID** format is `pageId_postId` (e.g., `1046712038534955_122093172452504915`).
- Use the `/posts` endpoint instead of `/feed` for the New Pages Experience.
- Some legacy insight metrics have been deprecated in Graph API v25.0.
- **Webhook signature validation** is skipped when `Facebook:AppSecret` is empty (development mode only).

## Technology Stack

- **Backend:** ASP.NET Core 9.0 MVC
- **HTTP Client:** HttpClient with Dependency Injection
- **Facebook API:** Graph API v25.0
- **Message Broker:** Apache Kafka (Confluent.Kafka)
- **Frontend:** HTML, CSS, JavaScript (Vanilla)
- **Serialization:** System.Text.Json
- **Containerization:** Docker (Kafka)

## Author

**TRIphann** - [GitHub](https://github.com/TRIphann)
