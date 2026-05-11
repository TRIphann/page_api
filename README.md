# Facebook Page API Dashboard

A real-time Facebook Page management dashboard built with ASP.NET Core 8, Kafka, and Server-Sent Events (SSE).

## Features

- **Facebook Page Information** - View page details via Facebook Graph API
- **Post Management** - Create and delete posts directly from the dashboard
- **Comments & Reactions** - View comments and reactions on posts
- **Page Insights** - Display basic page statistics
- **Real-time Webhook** - Receive and display new comments instantly via Kafka and SSE

## Architecture

```
Browser (Dashboard UI)
    |
    |-- REST API --> Facebook Graph API
    |
    |-- SSE --> ASP.NET Core Webhook Controller
                     |
                     |-- Kafka Consumer Service (background)
                              |
                              |-- Kafka Topic: raw_events
                                       |
                                       |-- Kafka Webhook Producer Service
                                                |
                                                |-- Facebook Webhook Endpoint
```

### Components

- **ASP.NET Core 8** - Backend API and WebSocket/SSE server
- **Kafka** - Message broker for real-time event streaming
- **Facebook Graph API** - Official API for Facebook Page data
- **Server-Sent Events (SSE)** - Push real-time updates to the browser
- **HMAC-SHA256 Signature Validation** - Verifies webhook payloads from Facebook

## Prerequisites

- .NET 8 SDK
- Apache Kafka (local or cloud)
- Facebook Page with a registered App and Webhook subscription

## Configuration

Edit `appsettings.json` to set your credentials:

```json
{
  "Facebook": {
    "PageId": "YOUR_PAGE_ID",
    "AccessToken": "YOUR_PAGE_ACCESS_TOKEN",
    "AppSecret": "YOUR_APP_SECRET"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "Topic": "raw_events",
    "Username": "",
    "Password": ""
  }
}
```

### Kafka Authentication (Confluent Cloud)

If using Confluent Cloud, set username and password in `appsettings.json`:

```json
"Kafka": {
  "BootstrapServers": "YOUR_BOOTSTRAP_SERVERS",
  "Topic": "YOUR_TOPIC",
  "Username": "YOUR_API_KEY",
  "Password": "YOUR_API_SECRET"
}
```

### Facebook Webhook Setup

1. Create a Facebook App at [developers.facebook.com](https://developers.facebook.com)
2. Add the "Webhooks" product to your app
3. Set the callback URL to `https://YOUR_PUBLIC_URL/webhook`
4. Subscribe to the `page` webhook field
5. Verify the privacy policy page is accessible at `/privacy`
6. Use a tunneling tool like ngrok for local development:
   ```
   ngrok http 3001
   ```

## Running the Application

```bash
dotnet run
```

The application runs at `http://localhost:3001` by default.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/page/info` | Get Facebook Page information |
| GET | `/api/page/posts` | Get recent posts from the Page |
| POST | `/api/page/posts` | Create a new post |
| DELETE | `/api/page/post/{postId}` | Delete a post |
| GET | `/api/page/post/{postId}/comments` | Get comments on a post |
| GET | `/api/page/post/{postId}/likes` | Get reactions on a post |
| GET | `/api/page/insights` | Get Page insights |
| GET | `/webhook/stream` | SSE stream for real-time events |
| GET | `/webhook/status` | Get webhook connection status |
| GET | `/webhook/all-comments` | Get all received comments |

## Real-time Events Flow

1. A user comments on a Facebook Page post
2. Facebook sends a webhook POST to `/webhook`
3. The webhook validates the HMAC-SHA256 signature
4. The event is normalized and published to the Kafka topic `raw_events`
5. The Kafka consumer reads the event and broadcasts it to all connected SSE clients
6. The dashboard UI receives the event and displays the new comment immediately

## Webhook Security

All incoming webhook payloads from Facebook are validated using HMAC-SHA256 signature verification. The `X-Hub-Signature-256` header must match the expected signature computed with the App Secret.

## Privacy Policy

A privacy policy page is served at `/privacy` to comply with Facebook App Review requirements. Update the content in `Program.cs` with your actual contact information before submitting for review.

## Project Structure

```
facbook_page_api/
├── Controllers/           # API and Webhook controllers
├── Models/               # Data models for Facebook API responses
├── Services/            # Business logic services
│   ├── FacebookGraphService.cs      # Facebook Graph API client
│   ├── KafkaProducerService.cs     # Produces events to Kafka
│   ├── KafkaConsumerService.cs     # Consumes events from Kafka
│   ├── SignatureValidator.cs       # HMAC-SHA256 webhook validation
│   ├── EventNormalizerService.cs   # Normalizes Facebook payloads
│   └── WebhookStatusService.cs     # Tracks webhook status
├── Views/               # Razor views
├── Program.cs           # Application entry point and configuration
└── appsettings.json     # Configuration file
```

## Dependencies

- `Confluent.Kafka` - Apache Kafka client
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson` - JSON serialization for ASP.NET Core
