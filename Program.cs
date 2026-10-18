using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    static void Main()
    {
        Collector collector = new Collector();
        Viewer viewer = new Viewer(collector);

        viewer.Start();
    }
}

public class Viewer
{
    private const string CommandAdd = "Add";
    private const string CommandDelete = "Delete";
    private const string CommandShow = "Show";
    private const string CommandExite = "Exite";

    private Collector _collector;
    private bool _isWork = true;

    public Viewer(Collector collector)
    {
        _collector = collector;
    }

    public void Start()
    {
        while (_isWork)
        {
            Console.WriteLine("Введите команду из нане существующих:");
            Console.WriteLine($"{CommandAdd} - добавить грусть");
            Console.WriteLine($"{CommandDelete} - преодолеть или смириться с грустью");
            Console.WriteLine($"{CommandShow} - показать все грусти");
            Console.WriteLine($"{CommandExite} - выйти");

            string command = Console.ReadLine();

            Console.Clear();

            switch (command)
            {
                case CommandAdd:
                    _collector.Add();
                    break;

                case CommandDelete:
                    _collector.Delete();
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

            Thread.Sleep(3000);
        }
    }
}

public class Collector
{
    private List<Problem> _myProblem;

    public Collector()
    {
        _myProblem = Tools.Load();
    }

    public void Add()
    {
        Console.Write("Введите описание для вашей грусти: ");
        string description = Console.ReadLine();

        _myProblem.Add(new Problem(DateOnly.FromDateTime(DateTime.Today), description));
        Console.WriteLine("Вы добавили новую грусть :(");

        Tools.Save(_myProblem);
    }

    public void Delete()
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

                Tools.Save(_myProblem);
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

    public static void Save(List<Problem> problems)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(problems, _options);

            File.WriteAllText(FilePath, jsonString);
            Console.WriteLine("Все грусти успешно сохранены");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при попытке сохранения проблем: {ex.Message}");
        }
    }

    public static List<Problem> Load()
    {
        try
        {
            if (File.Exists(FilePath) == false)
            {
                Console.WriteLine("Валера кто-то забрал твои проблемы теперь у тебя их нет)))");
                Console.WriteLine("......Была загружена база данных без проблем......");

                File.WriteAllText(FilePath, "[]");
                return new List<Problem>();
            }

            string jsonString = File.ReadAllText(FilePath);

            Console.WriteLine("......Была загружена базза данных c проблемами......");
            Console.WriteLine("А что если мы прошепчем: <<Мне больно смотреть на тебя Артас>>");

            return JsonSerializer.Deserialize<List<Problem>>(jsonString, _options) ?? new List<Problem>();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Валера кто-то забрал твои проблемы теперь у тебя их нет)))  {ex.Message}");
            Console.WriteLine("......Была загружена база данных без проблем......");

            return new List<Problem>();
        }
    }
}
