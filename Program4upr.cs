using System;
using System.Linq;

namespace NewChatSystem
{
    class Program4upr
    {
        static void Main()
        {
            Console.WriteLine("╔════════════════════════════════╗");
            Console.WriteLine("║      SYSTEM V2.0 STARTUP       ║");
            Console.WriteLine("╚════════════════════════════════╝\n");

            var channel = new Hub("General");

            void TryJoin(object entry)
            {
                switch (entry)
                {
                    case string str when !string.IsNullOrWhiteSpace(str):
                        channel.Join(new Participant(str));
                        Console.WriteLine($"[LOG] New manual participant: {str}");
                        break;

                    case string str when string.IsNullOrWhiteSpace(str):
                        var auto = new Participant(null);
                        channel.Join(auto);
                        Console.WriteLine($"[LOG] New auto-generated participant: {auto.Username}");
                        break;

                    case Participant p:
                        channel.Join(p);
                        Console.WriteLine($"[LOG] Existing object joined: {p.Username}");
                        break;

                    case null:
                        Console.WriteLine("[ERROR] Null input rejected.");
                        break;

                    default:
                        Console.WriteLine("[ERROR] Unknown data type.");
                        break;
                }
            }

            TryJoin("Alice");
            TryJoin("Bob");
            TryJoin(new Participant("UnknownAgent"));

            var p1 = channel.ActiveUsers.First();
            var p2 = channel.ActiveUsers.Skip(1).First();

            channel.PostLog(new LogEntry(p1, "System check..."));
            channel.PostLog(new LogEntry(p1, "Status report?"));
            channel.PostLog(new LogEntry(p2, "All green."));
            channel.PostLog(new LogEntry(p2, "Ready to launch sequence."));

            var targetLog = channel.Logs.FirstOrDefault(l => l.Writer == p1 && l.Content == "Status report?");
            targetLog?.Modify("Status report: Update requested.");

            Console.WriteLine("\n╔══════════ LOG HISTORY ══════════╗");
            foreach (var log in channel.Logs)
                Console.WriteLine(log);
            Console.WriteLine("╚═════════════════════════════════╝");

            Console.WriteLine("\n=== DATA EXTRACTION TEST ===");
            var referenceEntry = channel.Logs.First();

            (string u, string c, DateTime d) = referenceEntry;
            Console.WriteLine($" >> Basic: {u} said '{c}' at {d}");

            (string u2, string c2, DateTime dOnly, TimeSpan tOnly) = referenceEntry;
            Console.WriteLine($" >> Detailed: User {u2} | Date: {dOnly:yyyy-MM-dd} | Time: {tOnly}");

            var metrics = channel.CalculateMetrics();
            Console.WriteLine("\n=== CHANNEL METRICS ===");
            (string activeUser, int msgCount, string maxLenMsg) = metrics;

            Console.WriteLine($"Most Active: {activeUser}");
            Console.WriteLine($"Total Posts: {msgCount}");
            Console.WriteLine($"Longest Post: {maxLenMsg}");
        }
    }
}