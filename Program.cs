using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Collector collector = new Collector();
        await collector.InitializeAsync();

        Viewer viewer = new Viewer(collector);
        await viewer.StartAsync();
    }
}

public class Viewer
{
    private const string CommandAdd = "Add";
    private const string CommandDelete = "Delete";
    private const string CommandShow = "Show";
    private const string CommandExite = "Exite";

    private readonly Collector _collector;
    private bool _isWork = true;

    public Viewer(Collector collector)
    {
        _collector = collector;
    }

    public async Task StartAsync()
    {
        while (_isWork)
        {
            Console.WriteLine("Введите команду из ныне существующих:");
            Console.WriteLine($"{CommandAdd} - добавить грусть");
            Console.WriteLine($"{CommandDelete} - преодолеть или смириться с грустью");
            Console.WriteLine($"{CommandShow} - показать все грусти");
            Console.WriteLine($"{CommandExite} - выйти");

            string command = Console.ReadLine();

            Console.Clear();

            switch (command)
            {
                case CommandAdd:
                    await _collector.AddAsync();
                    break;

                case CommandDelete:
                    await _collector.DeleteAsync();
                    break;

                case CommandShow:
                    _collector.Show();
                    break;

                case CommandExite:
                    Console.Clear();
                    Console.WriteLine("Пока-пока");
                    _isWork = false;
                    break;

                default:
                    Console.WriteLine("Данной команды не существует");
                    break;
            }

            await Task.Delay(3000);
            Console.Clear();
        }
    }
}

public class Collector
{
    private List<Problem> _myProblem = new List<Problem>();

    public async Task InitializeAsync()
    {
        _myProblem = await Tools.LoadAsync();
    }

    public async Task AddAsync()
    {
        Console.Write("Введите описание для вашей грусти: ");
        string description = Console.ReadLine();

        _myProblem.Add(new Problem(DateOnly.FromDateTime(DateTime.Today), description));
        Console.WriteLine("Вы добавили новую грусть :(");

        await Tools.SaveAsync(_myProblem);
    }

    public async Task DeleteAsync()
    {
        if (_myProblem.Count == 0)
        {
            Console.WriteLine("Список пуст");
            return;
        }

        Console.WriteLine("Выбери грусть которую ты преодолел или смирился с ней: ");

        for (int i = 0; i < _myProblem.Count; i++)
        {
            Console.WriteLine($"{i + 1}. Описание: {_myProblem[i].Description}");
        }

        if (int.TryParse(Console.ReadLine(), out int index))
        {
            index -= 1;

            if (_myProblem.Count > index && index >= 0)
            {
                _myProblem.RemoveAt(index);
                Console.WriteLine("Грусть была преодолена или стала частью тебя");

                await Tools.SaveAsync(_myProblem);
            }
            else
            {
                Console.WriteLine("Грусти под таким номером не существует");
            }
        }
        else
        {
            Console.WriteLine("Некорректный ввод");
        }
    }

    public void Show()
    {
        if (_myProblem.Count > 0)
        {
            for (int i = 0; i < _myProblem.Count; i++)
            {
                Console.WriteLine($"Грусть №{i + 1}");
                Console.WriteLine($"Дата: {_myProblem[i].Date}");
                Console.WriteLine($"Описание: {_myProblem[i].Description}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("В вас нет ни капли грусти ;) (Пока)");
        }
    }
}

public struct Problem
{
    [JsonConstructor]
    public Problem(DateOnly date, string description)
    {
        Date = date;
        Description = description;
    }

    public DateOnly Date { get; private set; }
    public string Description { get; private set; }
}

public static class Tools
{
    public static readonly string FilePath = "Problems.json";

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        IncludeFields = true,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static async Task SaveAsync(List<Problem> problems)
    {
        try
        {
            using (FileStream createStream = File.Create(FilePath))
            {
                await JsonSerializer.SerializeAsync(createStream, problems, _options);
            }
            Console.WriteLine("Все грусти успешно сохранены");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при попытке сохранения проблем: {ex.Message}");
        }
    }

    public static async Task<List<Problem>> LoadAsync()
    {
        try
        {
            if (File.Exists(FilePath) == false)
            {
                Console.WriteLine("Валера кто-то забрал твои проблемы теперь у тебя их нет)))");
                Console.WriteLine("......Была загружена база данных без проблем......");

                await File.WriteAllTextAsync(FilePath, "[]");
                return new List<Problem>();
            }

            using (FileStream openStream = File.OpenRead(FilePath))
            {
                Console.WriteLine("......Была загружена база данных c проблемами......");
                Console.WriteLine("А что если мы прошепчем: <<Мне больно смотреть на тебя Артас>>");

                var result = await JsonSerializer.DeserializeAsync<List<Problem>>(openStream, _options);
                return result ?? new List<Problem>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Валера кто-то забрал твои проблемы теперь у тебя их нет)))  {ex.Message}");
            Console.WriteLine("......Была загружена база данных без проблем......");

            return new List<Problem>();
        }
    }
}
