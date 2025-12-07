using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace UPR3_InternetData
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("\n╔════════════════════════════════════╗");
                Console.WriteLine("║      СИСТЕМА ЗА ИНТЕРНЕТ ДАННИ     ║");
                Console.WriteLine("╠════════════════════════════════════╣");
                Console.WriteLine("║ [1] Проверка на IP (GeoLocation)   ║");
                Console.WriteLine("║ [2] Точен час в София (Online)     ║");
                Console.WriteLine("║ [3] Новини (без Covid теми)        ║");
                Console.WriteLine("║ [0] Затвори програмата             ║");
                Console.WriteLine("╚════════════════════════════════════╝");

                Console.Write(">> Въведете вашата команда: ");
                var userChoice = Console.ReadLine();

                try
                {
                    switch (userChoice)
                    {
                        case "1": await RunIpCheck(); break;
                        case "2": await RunTimeCheck(); break;
                        case "3": await RunNewsScraper(); break;
                        case "0":
                            Console.WriteLine("Довиждане!");
                            return;
                        default:
                            Console.WriteLine("❌ Неразпозната команда. Опитайте пак.");
                            break;
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine("⚠ Възникна грешка: " + error.Message);
                }
            }
        }

        static async Task RunIpCheck()
        {
            Console.Write("Въведи IP (Enter за САЩ - 8.8.8.8): ");
            string targetAddress = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(targetAddress))
            {
                targetAddress = "8.8.8.8";
            }

            string requestUrl = $"https://ipapi.co/{targetAddress}/country_name/";

            using var webClient = new HttpClient();
            string apiResponse = await webClient.GetStringAsync(requestUrl);

            Console.WriteLine($"Result: Държава за IP {targetAddress} е: {apiResponse}");
        }

        static async Task RunTimeCheck()
        {
            string timeSourceUrl = "https://www.timeanddate.com/worldclock/bulgaria/sofia";

            using var timeClient = new HttpClient();
            string rawHtml = await timeClient.GetStringAsync(timeSourceUrl);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            var clockNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@id='ct']");

            if (clockNode != null)
                Console.WriteLine("🕒 Текущо време (София): " + clockNode.InnerText.Trim());
            else
                Console.WriteLine("❌ Неуспешно извличане на часа.");
        }

        static async Task RunNewsScraper()
        {
            string newsSite = "https://www.mediapool.bg/";

            using var scraperClient = new HttpClient();

            scraperClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");

            var responseStream = await scraperClient.GetStreamAsync(newsSite);

            var pageDoc = new HtmlDocument();
            pageDoc.Load(responseStream);

            var headlines = pageDoc.DocumentNode.SelectNodes("//h1 | //h2 | //h3");

            Console.WriteLine("\n=== Последни заглавия (Филтрирани) ===\n");

            if (headlines != null)
            {
                foreach (var item in headlines)
                {
                    string headerText = item.InnerText.Trim();

                    if (headerText.Length < 10) continue;

                    if (headerText.Contains("Covid", StringComparison.OrdinalIgnoreCase) ||
                        headerText.Contains("Ковид") ||
                        headerText.Contains("пандем"))
                    {
                        continue;
                    }

                    headerText = System.Net.WebUtility.HtmlDecode(headerText);

                    Console.WriteLine("📰 " + headerText);
                }
            }
            else
            {
                Console.WriteLine("Няма намерени заглавия.");
            }
        }
    }
}