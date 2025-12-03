using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization; // Важно за JSON атрибутите
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DataProcessingApp
{
    // ----------------------
    // Задача 1: Координати
    // ----------------------
    public struct MapLocation
    {
        [JsonPropertyName("latitude")]
        public double Lat { get; set; }

        [JsonPropertyName("longitude")]
        public double Lng { get; set; }
    }

    // ----------------------
    // Задача 2: Контакти
    // ----------------------
    [XmlType("ContactPerson")]
    public class Subscriber
    {
        [XmlElement("Name")]
        public string Names { get; set; }

        [XmlAttribute("ID")]
        public string PersonalId { get; set; }

        [XmlElement("Phone")]
        public string MobileNumber { get; set; }
    }

    public class PhoneBook
    {
        public List<Subscriber> Subscribers { get; set; } = new List<Subscriber>();

        public string SearchContact(string key)
        {
            var result = Subscribers.FirstOrDefault(s => s.Names.Contains(key) || s.PersonalId == key);
            return result != null ? result.MobileNumber : "Няма намерен запис.";
        }
    }

    // ----------------------
    // Задача 3: Граф / Връзки
    // ----------------------
    public class LinkNode
    {
        public string ElementTag { get; set; }
        public string ElementId { get; set; }
        public string LinkedId { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Настройка на енкодинга за коректно четене на кирилица
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            bool appRunning = true;

            while (appRunning)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n=== СИСТЕМА ЗА ОБРАБОТКА НА ДАННИ v2.0 ===");
                Console.ResetColor();
                Console.WriteLine("1. [CSV/TXT] -> Конвертиране на координати в JSON");
                Console.WriteLine("2. [TXT/Regex] -> Извличане на телефони в XML");
                Console.WriteLine("3. [XML/DAE] -> Анализ на връзки в 3D сцена");
                Console.WriteLine("0. Изход");
                Console.Write("\nМоля, изберете операция: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            RunGpsConverter();
                            break;
                        case "2":
                            RunPhoneExtractor();
                            break;
                        case "3":
                            RunSceneAnalyzer();
                            break;
                        case "0":
                            Console.WriteLine("Довиждане!");
                            appRunning = false;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Невалидна команда! Опитайте отново.");
                            Console.ResetColor();
                            break;
                    }
                }
                catch (Exception globalEx)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"\n(!) Критична грешка в главното меню: {globalEx.Message}");
                    Console.ResetColor();
                }
            }
        }

        // --- Логика за Задача 1 ---
        static void RunGpsConverter()
        {
            string path = "input-01.txt";
            Console.WriteLine($"\n--- Четене на файл: {path} ---");

            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException("Входният файл липсва!");

                string rawContent = File.ReadAllText(path);

                // Разделяме по ';' за отделните записи
                var tokens = rawContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var locations = new List<MapLocation>();

                foreach (var token in tokens)
                {
                    try
                    {
                        var coords = token.Split(',');
                        if (coords.Length >= 2)
                        {
                            // Използваме double за по-добра прецизност
                            double lat = double.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
                            double lng = double.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);

                            locations.Add(new MapLocation { Lat = lat, Lng = lng });
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"(!) Пропуснат некоректен запис: {token}");
                    }
                }

                string json = JsonSerializer.Serialize(locations, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("locations.json", json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Успешно конвертирани {locations.Count} локации.");
                Console.WriteLine("Резултатът е записан в 'locations.json'.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при обработка на координати: {ex.Message}");
            }
        }

        // --- Логика за Задача 2 ---
        static void RunPhoneExtractor()
        {
            string path = "input-02.txt";
            Console.WriteLine($"\n--- Анализ на телефонен указател: {path} ---");

            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException("Файлът с контакти липсва!");

                string content = File.ReadAllText(path);

                // Regex дефиниции
                var regName = new Regex(@"\b[А-Я][а-я]+\b");
                var regId = new Regex(@"\b\d{6}\b"); // Търси точно 6 цифри
                var regPhone = new Regex(@"\+395(\s+|-)\d{3}(\s+|-)\d{2}(\s+|-)\d{2}");

                var matchesName = regName.Matches(content);
                var matchesId = regId.Matches(content);
                var matchesPhone = regPhone.Matches(content);

                // Проверка за цялост на данните
                if (matchesName.Count != matchesId.Count || matchesId.Count != matchesPhone.Count)
                {
                    throw new InvalidDataException("Несъответствие в броя на имената, ID-тата и телефоните. Проверете входните данни.");
                }

                PhoneBook phoneBook = new PhoneBook();

                for (int i = 0; i < matchesName.Count; i++)
                {
                    // Изчистване на излишни интервали в телефона
                    string cleanPhone = Regex.Replace(matchesPhone[i].Value, @"\s+|-", " ");

                    phoneBook.Subscribers.Add(new Subscriber
                    {
                        Names = matchesName[i].Value,
                        PersonalId = matchesId[i].Value,
                        MobileNumber = cleanPhone
                    });
                }

                // Запис в XML
                XmlSerializer serializer = new XmlSerializer(typeof(List<Subscriber>), new XmlRootAttribute("PhoneDirectory"));
                using (StreamWriter sw = new StreamWriter("contacts.xml"))
                {
                    serializer.Serialize(sw, phoneBook.Subscribers);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Обработени са {phoneBook.Subscribers.Count} контакта.");
                Console.WriteLine("Данните са експортирани в 'contacts.xml'.");
                Console.ResetColor();

                // Опция за бързо търсене
                Console.Write("Търсене по Име или ID (Enter за пропуск): ");
                string searchKey = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(searchKey))
                {
                    Console.WriteLine($"Резултат: {phoneBook.SearchContact(searchKey)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при обработка на контакти: {ex.Message}");
            }
        }

        // --- Логика за Задача 3 ---
        static void RunSceneAnalyzer()
        {
            string path = "input-03.dae";
            Console.WriteLine($"\n--- Анализ на 3D файл: {path} ---");

            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException("XML/DAE файлът липсва!");

                XDocument doc = XDocument.Load(path);
                List<LinkNode> links = new List<LinkNode>();

                // Търсене на всички елементи в документа
                foreach (var el in doc.Descendants())
                {
                    // Проверка на всички атрибути на елемента
                    foreach (var attr in el.Attributes())
                    {
                        // Логика: ако атрибутът започва с #, значи е референция
                        if (attr.Value.StartsWith("#") && attr.Value.Length > 1)
                        {
                            links.Add(new LinkNode
                            {
                                ElementTag = el.Name.LocalName,
                                ElementId = el.Attribute("id")?.Value ?? "N/A",
                                LinkedId = attr.Value.Substring(1) // Махаме диеза (#)
                            });
                        }
                    }
                }

                string json = JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("links.json", json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Открити са {links.Count} вътрешни връзки.");
                Console.WriteLine("Графът е записан в 'links.json'.");
                Console.ResetColor();
            }
            catch (System.Xml.XmlException xmlEx)
            {
                Console.WriteLine($"Невалиден XML формат: {xmlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Обща грешка при сцена: {ex.Message}");
            }
        }
    }
}