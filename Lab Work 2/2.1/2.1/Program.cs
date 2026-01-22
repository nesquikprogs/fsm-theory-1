using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TA_Lab2Console
{
    internal class Program
    {
        private static readonly Random random = new Random();

        static void Main(string[] args)
        {
            // Грамматика зашита в программу
            // S -> S0 | S1 | P0 | P1
            // P -> N
            // N -> 0 | 1 | N0 | N1
            Dictionary<string, List<string>> grammarRules = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "S0", "S1", "P0", "P1" } },
                { "P", new List<string> { "N" } },
                { "N", new List<string> { "0", "1", "N0", "N1" } }
            };

            // Цепочки для проверки
            var chains = new string[] { "11.010", "0.1", "01.", "100" };
            List<string> results = new List<string>();

            // Вывод грамматики на консоль
            Console.WriteLine("Дана регулярная грамматика:");
            foreach (var rule in grammarRules)
            {
                Console.WriteLine($"{rule.Key} -> {string.Join(" | ", rule.Value)}");
            }
            Console.WriteLine();

            // Вывод цепочек на консоль
            Console.WriteLine("Цепочки для проверки:");
            foreach (var chain in chains)
            {
                Console.WriteLine(chain);
            }
            Console.WriteLine();

            // Проверка цепочек на принадлежность языку
            foreach (var chain in chains)
            {
                bool isValid = ValidateChain(chain);
                results.Add($"Цепочка '{chain}' {(isValid ? "принадлежит" : "не принадлежит")} языку.");
            }

            // Генерация пяти случайных цепочек по грамматике
            List<string> generatedChains = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                string word = GenerateWord();
                while (!IsEnded(word))
                {
                    word = GenerateWord();
                }
                generatedChains.Add(word);
            }

            // Диаграмма состояний автомата
            string stateDiagram = BuildStateDiagram();

            // Описание языка
            string languageDescription = "Грамматика порождает язык: L = {w.d | w ∈ {0,1}+, d ∈ {0,1}+}, где:\n" +
                                         "- w - непустая последовательность символов 0 и 1,\n" +
                                         "- d - непустая последовательность символов 0 и 1,\n" +
                                         "- символ '.' разделяет w и d.";

            // Вывод диаграммы переходов
            Console.WriteLine("Диаграмма состояний ДС:");
            Console.WriteLine(stateDiagram);
            Console.WriteLine();

            // Вывод результатов проверки цепочек
            Console.WriteLine("Результаты проверки цепочек:");
            foreach (var res in results)
            {
                Console.WriteLine(res);
            }
            Console.WriteLine();

            // Вывод пяти случайных цепочек
            Console.WriteLine("Пять случайных цепочек по грамматике:");
            foreach (var word in generatedChains)
            {
                Console.WriteLine(word);
            }
            Console.WriteLine();

            // Вывод описания языка
            Console.WriteLine("Описание языка:");
            Console.WriteLine(languageDescription);
        }

        // Проверка цепочки с помощью регулярного выражения
        // ^[01]+\.[01]+$ - начало строки, одна или более цифр 0 или 1, точка, одна или более цифр 0 или 1, конец строки
        private static bool ValidateChain(string chain)
        {
            string pattern = @"^[01]+\.[01]+$";
            return Regex.IsMatch(chain, pattern);
        }

        // Диаграмма состояний
        // S - начальное состояние, читаем первую часть цепочки
        // P - состояние для точки
        // N - состояние второй части цепочки
        private static string BuildStateDiagram()
        {
            return @"S --0--> S
S --1--> S
S --0--> P
S --1--> P
P --.--> N
N --0--> N
N --1--> N";
        }

        // Генерация цепочки по грамматике
        private static string GenerateWord()
        {
            string fin = "";
            return ApplyRule('S', ref fin);
        }

        // Рекурсивное применение правил грамматики
        private static string ApplyRule(char rule, ref string fin)
        {
            switch (rule)
            {
                case 'S':
                    // S -> S0 | S1 | P0 | P1
                    if (random.Next(0, 2) == 0)
                    {
                        fin += random.Next(0, 2);
                        ApplyRule('S', ref fin);
                    }
                    else
                    {
                        fin += random.Next(0, 2);
                        ApplyRule('P', ref fin);
                    }
                    break;
                case 'P':
                    fin += "."; // точка разделяет первую и вторую часть
                    ApplyRule('N', ref fin);
                    break;
                case 'N':
                    // N -> 0 | 1 | N0 | N1
                    if (random.Next(0, 2) == 0)
                    {
                        fin += random.Next(0, 2);
                        return fin;
                    }
                    else
                    {
                        fin += random.Next(0, 2);
                        ApplyRule('N', ref fin);
                    }
                    break;
            }
            return fin;
        }

        // Проверка корректности цепочки (только 0, 1 и точка)
        private static bool IsEnded(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            foreach (char c in str)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
