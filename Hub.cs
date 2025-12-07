using System.Collections.Generic;
using System.Linq;

namespace NewChatSystem
{
    public class Hub
    {
        public string Title { get; set; }
        public List<Participant> ActiveUsers { get; set; } = new();
        public List<LogEntry> Logs { get; set; } = new();

        public Hub(string title)
        {
            Title = title;
        }

        public void Join(Participant p)
            => ActiveUsers.Add(p ?? new Participant());

        public void PostLog(LogEntry entry)
            => Logs.Add(entry);

        public (string leadUser, int frequency, string maxContent) CalculateMetrics()
        {
            var leader = Logs
                .GroupBy(x => x.Writer.Username)
                .OrderByDescending(grp => grp.Count())
                .FirstOrDefault();

            string leadName = leader?.Key ?? "None";
            int total = leader?.Count() ?? 0;

            string heavyPayload = Logs
                .OrderByDescending(x => x.Content.Length)
                .FirstOrDefault()?.Content ?? "None";

            return (leadName, total, heavyPayload);
        }
    }
}