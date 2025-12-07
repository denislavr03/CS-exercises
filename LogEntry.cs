using System;

namespace NewChatSystem
{
    public class LogEntry
    {
        public Participant Writer { get; set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.Now;
        public bool IsModified { get; private set; } = false;
        public DateTime? ModifiedTimestamp { get; private set; } = null;

        public LogEntry(Participant writer, string content)
        {
            Writer = writer ?? new Participant();
            Content = content ?? string.Empty;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            var flag = IsModified ? "*" : "";
            return $"[{Timestamp:HH:mm}] <{Writer}>{flag}: {Content}";
        }

        public void Modify(string updatedContent)
        {
            Content = updatedContent ?? Content;
            IsModified = true;
            ModifiedTimestamp = DateTime.Now;
        }

        public void Deconstruct(out string author, out string body, out DateTime time)
        {
            author = Writer?.Username ?? "Unknown";
            body = Content;
            time = Timestamp;
        }

        public void Deconstruct(out string author, out string body, out DateTime datePart, out TimeSpan timePart)
        {
            author = Writer?.Username ?? "Unknown";
            body = Content;
            datePart = Timestamp.Date;
            timePart = Timestamp.TimeOfDay;
        }
    }
}