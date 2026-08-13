namespace KoneMoshapoRetail.Models
{
    public class LogEntry
    {
        public string LogId { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "Information";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Category { get; set; } = "Application";
        public string Action { get; set; } = string.Empty;

        public string ToFormattedString() =>
            $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{Source}] {Message}";
    }
}