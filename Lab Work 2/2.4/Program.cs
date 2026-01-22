using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace TA_Lab2
{
    internal class Program
    {
        // Главный метод консольного приложения, который запускает анализ грамматики
        // Здесь мы задаем грамматику и цепочки напрямую, имитируя ввод из WPF
        static void Main(string[] args)
        {
            try
            {
                // Жестко заданная грамматика (как в условии задачи)
                string grammarInput = "S -> 0S | S0 | D\nD -> DD | 1A | eps\nA -> 0B | eps\nB -> 0A | 0";
                // Жестко заданные цепочки (как в условии задачи)
                string chainInput = "1011,00100,0100";

                // Создаем экземпляр анализатора (аналогично оригинальному коду)
                var primer = new Primer4();

                // Вызываем метод расчета (аналогично клику кнопки в WPF)
                primer.Calculate(grammarInput, chainInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            // Ожидаем нажатия клавиши для завершения
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }

    internal class Primer4
    {
        private static readonly Random random = new Random();
        enum State { S, D, A, B }

        // Метод, заменяющий клик кнопки в WPF: выполняет весь анализ и выводит результаты в консоль
        public void Calculate(string grammarInput, string chainInput)
        {
            try
            {
                // Анализ грамматики
                var grammarAnalyzer = new GrammarAnalyzer(grammarInput);
                Console.WriteLine("Диаграмма переходов:");
                // Разбор грамматики
                Dictionary<string, List<string>> grammarRules = ParseGrammar(grammarInput);
                // Построение диаграммы переходов
                StringBuilder diagram = BuildDiagram(grammarRules);
                Console.WriteLine($"{diagram}");

                // a) Определение типа грамматики
                string grammarType = grammarAnalyzer.DetermineGrammarType();
                Console.WriteLine($"a) Тип грамматики: {grammarType}");
                Console.WriteLine("Левая часть правила должна состоять ровно из одного нетерминала.\n");

                // Генерируем 5 рандомных цепочек длиной от 5 до 7
                for (int i = 0; i < 5; i++)
                {
                    int targetLength = random.Next(5, 8); // Рандомная длина от 5 до 7 (8 не включительно)
                    string generatedString = "";

                    // Повторяем генерацию, пока не получим строку нужной длины
                    while (generatedString.Length != targetLength)
                    {
                        generatedString = GenerateRandomString();
                        if (generatedString.Length > targetLength)
                        {
                            generatedString = ""; // Если строка слишком длинная, пробуем снова
                        }
                    }

                    Console.WriteLine($"Цепочка {i + 1}: {generatedString}");
                }
                Console.WriteLine();

                // b) Определение языка, который порождает грамматика
                // Исправленное описание языка: грамматика позволяет несколько '1', с чётным количеством '0' между ними
                string languageDescription = "L = {0* (1 (00)*)* 0* | где между любыми двумя '1' чётное количество '0'}. Язык состоит из строк с произвольным количеством '0' и '1', где расстояние между '1' (количество '0') всегда чётное.";
                Console.WriteLine($"b) Язык, порождаемый грамматикой: {languageDescription}\n");

                // c) Построение P-грамматики
                string pGrammar = grammarAnalyzer.GeneratePGrammar();
                Console.WriteLine($"c) Регулярная грамматика($ - явный конец строки):\n{pGrammar}");

                // Разбираем цепочки по запятой
                var chains = chainInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> results = new List<string>();
                // Проверяем каждую цепочку на принадлежность языку
                foreach (var chain in chains)
                {
                    string trimmedChain = chain.Trim();
                    bool isValid = AnalyzeGrammar(trimmedChain);
                    results.Add($"Цепочка '{trimmedChain}' {(isValid ? "принадлежит" : "не принадлежит")} языку.");
                }
                Console.WriteLine("Результаты проверки цепочек:");
                Console.WriteLine(string.Join(Environment.NewLine, results));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        // Метод для парсинга грамматики: разбирает входную строку на правила
        static Dictionary<string, List<string>> ParseGrammar(string input)
        {
            Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();
            string[] lines = input.Split('\n');

            foreach (string line in lines)
            {
                // Разделяем правило на левую и правую части
                string[] parts = line.Split(new string[] { " -> " }, StringSplitOptions.None);
                if (parts.Length < 2) continue;
                string left = parts[0].Trim();
                string[] rightParts = parts[1].Split('|');

                List<string> productions = new List<string>();
                foreach (string part in rightParts)
                {
                    productions.Add(part.Trim());
                }

                rules[left] = productions;
            }
            return rules;
        }

        // Исправленный метод для построения диаграммы переходов
        static StringBuilder BuildDiagram(Dictionary<string, List<string>> rules)
        {
            StringBuilder diagram = new StringBuilder();
            HashSet<string> uniqueTransitions = new HashSet<string>(); // Для избежания дубликатов

            // Определяем возможные состояния и алфавит
            string[] states = rules.Keys.Concat(new[] { "T" }).ToArray(); // Добавляем T как состояние
            char[] alphabet = { '0', '1' }; // Алфавит грамматики

            foreach (var state in states)
            {
                foreach (char symbol in alphabet)
                {
                    string transition = null;
                    if (state == "T")
                    {
                        transition = $"T --{symbol}--> T"; // Ловушка для всех символов
                    }
                    else
                    {
                        foreach (var production in rules[state])
                        {
                            if (production.Length >= 1 && production[0] == symbol)
                            {
                                if (production.Length == 1 && char.IsDigit(production[0]))
                                {
                                    // Одиночный терминал (например, B -> 0)
                                    transition = $"{state} --{symbol}--> {state}"; // Цикл или конечное состояние
                                }
                                else if (production.Length == 2 && char.IsUpper(production[1]))
                                {
                                    // Переход по символу к следующему состоянию (например, D -> 1A)
                                    transition = $"{state} --{symbol}--> {production[1]}";
                                }
                                else if (production.Length >= 2 && production[1] == state[0])
                                {
                                    // Цикл (например, S -> 0S, D -> DD)
                                    transition = $"{state} --{symbol}--> {state}";
                                }
                            }
                            else if (production == "eps")
                            {
                                // Пропускаем eps для переходов, отмечаем как конечное состояние позже
                                continue;
                            }
                        }

                        // Если переход не найден, направляем в ловушку (T)
                        if (transition == null)
                        {
                            transition = $"{state} --{symbol}--> T";
                        }
                    }

                    // Добавляем только уникальные переходы
                    if (uniqueTransitions.Add(transition))
                    {
                        diagram.AppendLine(transition);
                    }
                }
            }

            // Добавляем заметку о конечных состояниях
            diagram.AppendLine("\nКонечные состояния: D, A, B (из правил eps)");

            return diagram;
        }
        public class GrammarParser
        {
            // Определяем словарь для хранения правил грамматики (с eps вместо ε)
            static Dictionary<char, List<string>> grammar = new Dictionary<char, List<string>>()
            {
                { 'S', new List<string> { "0S", "S0", "D" } },
                { 'D', new List<string> { "DD", "1A", "" } }, // eps обозначаем как пустую строку ""
                { 'A', new List<string> { "0B", "" } },
                { 'B', new List<string> { "0A", "0" } }
            };

            // Рекурсивный метод для генерации всех возможных строк языка (используется для демонстрации)
            static void GenerateLanguage(char symbol, string currentString)
            {
                // Если достигнута максимальная длина строки (например, 5 символов), выводим результат
                if (currentString.Length >= 5)
                {
                    Console.WriteLine(currentString);
                    return;
                }

                // Получаем правила для текущего нетерминала
                foreach (var rule in grammar[symbol])
                {
                    if (rule == "") // Если правило - eps (пустая строка)
                    {
                        GenerateLanguage(symbol, currentString); // Продолжаем с текущей строкой
                    }
                    else
                    {
                        // Проходим по каждому символу в правиле
                        string newString = currentString;
                        foreach (char c in rule)
                        {
                            if (Char.IsUpper(c)) // Если символ - нетерминал
                            {
                                GenerateLanguage(c, newString); // Рекурсивно обрабатываем нетерминал
                            }
                            else
                            {
                                newString += c; // Добавляем терминальный символ к текущей строке
                            }
                        }
                        GenerateLanguage(symbol, newString); // Продолжаем генерацию с новой строкой
                    }
                }
            }
        }

        // Класс для анализа грамматики: определяет тип и генерирует P-грамматику
        public class GrammarAnalyzer
        {
            private readonly string[] productions;

            public GrammarAnalyzer(string grammarInput)
            {
                // Разбор входных данных в массив правил
                productions = grammarInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            // Метод определяет тип грамматики (контекстно-свободная или общая)
            public string DetermineGrammarType()
            {

                bool isContextFree = true;
                foreach (var production in productions)
                {
                    var parts = production.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                        throw new FormatException("Неверный формат правила.");

                    string left = parts[0].Trim();
                    string right = parts[1].Trim();

                    // Для контекстно-свободной грамматики левая часть должна быть одним нетерминалом
                    if (!Regex.IsMatch(left, @"^[A-Z]$"))
                    {
                        isContextFree = false;
                        break;
                    }
                }
                return isContextFree ? "Контекстно-свободная грамматика (тип 2)" : "Грамматика общего вида";
            }

            // Метод генерирует P-грамматику, добавляя маркер завершения
            public string GeneratePGrammar()
            {
                // Преобразование в P-грамматику
                StringBuilder pGrammar = new StringBuilder();
                foreach (var production in productions)
                {
                    var parts = production.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                        throw new FormatException("Неверный формат правила.");

                    string left = parts[0].Trim();
                    string right = parts[1].Trim();

                    // Добавляем маркер завершения для P-грамматики
                    pGrammar.AppendLine($"{left} -> {right} $");
                }
                return pGrammar.ToString();
            }
        }

        // Метод проверяет, принадлежит ли цепочка языку грамматики (анализатор)

        public bool AnalyzeGrammar(string input)
        {
            int state1 = 0; // Начальное состояние
            int index = 0; // Текущая позиция в строке
            int length = input.Length;

            while (index < length)
            {
                char c = input[index];

                if (state1 == 0)
                {
                    // Обработка правил S -> 0S | S0 | D
                    if (c == '0')
                    {
                        // Переход для правила S -> 0S или S -> S0
                        state1 = 0;
                    }
                    else if (c == '1')
                    {
                        // Переход для правила D -> 1A
                        state1 = 1;
                    }
                    else
                    {
                        return false; // Недопустимый символ
                    }
                }
                else if (state1 == 1)
                {
                    // Обработка правила A -> 0B | eps
                    if (c == '0')
                    {
                        // Переход для правила A -> 0B
                        state1 = 2;
                    }
                    else
                    {
                        return false; // Если не '0', то недопустимый символ
                    }
                }
                else if (state1 == 2)
                {
                    // Обработка правила B -> 0A | 0
                    if (c == '0')
                    {
                        // Переход для правила B -> 0A
                        state1 = 1;
                    }
                    else
                    {
                        return false; // Недопустимый символ
                    }
                }

                index++; // Переход к следующему символу
            }

            // Проверка допустимых конечных состояний
            // state == 0: строка соответствует правилам S
            // state == 1: строка соответствует правилам A (eps)
            return state1 == 0 || state1 == 1;
        }

        // Метод генерирует случайную строку на основе грамматики 
        public static string GenerateRandomString()
        {
            // Создаем объект Random для генерации случайных чисел
            Random random = new Random();

            // Текущее состояние начинается с S (начало)
            State currentState = State.S;
            // Строка для хранения результата
            StringBuilder result = new StringBuilder();
            while (true)
            {
                switch (currentState)
                {
                    case State.S:
                        // Правило: S -> 0S | S0 | D
                        int choiceS = random.Next(3); // Выбираем один из трех вариантов
                        if (choiceS == 0)
                        {
                            // Генерируем 0 и остаемся в S
                            result.Append('0');
                            currentState = State.S;
                        }
                        else if (choiceS == 1)
                        {
                            // Генерируем 0 и остаемся в S
                            result.Append('0');
                            currentState = State.S;
                        }
                        else
                        {
                            // Переходим к D
                            currentState = State.D;
                        }
                        break;
                    case State.D:
                        // Правило: D -> DD | 1A | eps
                        int choiceD = random.Next(3); // Выбираем один из трех вариантов
                        if (choiceD == 0)
                        {
                            // Остаемся в D (дублируем состояние)
                            currentState = State.D;
                        }
                        else if (choiceD == 1)
                        {
                            // Генерируем 1 и переходим в A
                            result.Append('1');
                            currentState = State.A;
                        }
                        else
                        {
                            // Пустая строка (eps), завершаем
                            return result.ToString();
                        }
                        break;
                    case State.A:
                        // Правило: A -> 0B | eps
                        int choiceA = random.Next(2); // Выбираем один из двух вариантов
                        if (choiceA == 0)
                        {
                            // Генерируем 0 и переходим в B
                            result.Append('0');
                            currentState = State.B;
                        }
                        else
                        {
                            // Пустая строка (eps), завершаем
                            return result.ToString();
                        }
                        break;
                    case State.B:
                        // Правило: B -> 0A | 0
                        int choiceB = random.Next(2); // Выбираем один из двух вариантов
                        if (choiceB == 0)
                        {
                            // Генерируем 0 и переходим в A
                            result.Append('0');
                            currentState = State.A;
                        }
                        else
                        {
                            // Генерируем 0 и завершаем
                            result.Append('0');
                            return result.ToString();
                        }
                        break;
                }
                // Добавляем ограничение на длину строки, чтобы избежать бесконечного цикла
                if (result.Length > 20)
                {
                    return result.ToString();
                }
            }
        }
    }
}