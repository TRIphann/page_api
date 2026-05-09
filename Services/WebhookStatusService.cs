using System.Collections.Concurrent;
using facbook_page_api.Models;

namespace facbook_page_api.Services
{
    /// <summary>
    /// Quản lý trạng thái webhook: registered hay chưa, URL tunnel, thời gian.
    /// Dùng chung giữa WebhookController và UI để hiển thị trạng thái.
    /// </summary>
    public static class WebhookStatusService
    {
        public static bool IsRegistered { get; private set; }
        public static string TunnelUrl { get; private set; } = "";
        public static DateTime? LastEventReceived { get; private set; }
        public static int TotalEventsReceived { get; private set; }
        public static string WebhookMode { get; private set; } = "webhook"; // "webhook"

        private static readonly ConcurrentDictionary<string, int> _eventTypeCounts = new();

        public static void SetRegistered(bool registered, string? tunnelUrl = null)
        {
            IsRegistered = registered;
            if (tunnelUrl != null) TunnelUrl = tunnelUrl;
            WebhookMode = registered ? "webhook" : "polling";
        }

        public static void RecordEvent(string? eventType = null)
        {
            LastEventReceived = DateTime.Now;
            TotalEventsReceived++;
            if (eventType != null)
            {
                _eventTypeCounts.AddOrUpdate(eventType, 1, (_, c) => c + 1);
            }
        }

        public static object GetStatus()
        {
            return new
            {
                mode = WebhookMode,
                webhook_registered = IsRegistered,
                tunnel_url = TunnelUrl,
                last_event = LastEventReceived?.ToString("yyyy-MM-dd HH:mm:ss"),
                total_events = TotalEventsReceived,
                event_counts = _eventTypeCounts.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }
}
