// Program.cs
using System;
using System.Collections.Generic;

namespace TA_Lab1_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Определяем две грамматики

            // 1. Стандартная грамматика
            Dictionary<string, List<string>> grammar1 = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "aaCFD" } },
                { "AD", new List<string> { "D" } },
                { "F", new List<string> { "AFB", "AB" } },
                { "Cb", new List<string> { "bC" } },
                { "AB", new List<string> { "bBA" } },
                { "CB", new List<string> { "C" } },
                { "Ab", new List<string> { "bA" } },
                { "bCD", new List<string> { "" } } // ε - пустая строка
            };
            string description1 = "Строки вида a^n b^n, где n >= 2";

            // 2. Альтернативная грамматика: все строки из {a,b} с символом ⊥
            Dictionary<string, List<string>> grammar2 = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "A⊥", "B⊥" } },
                { "A", new List<string> { "a", "Ba" } },
                { "B", new List<string> { "b", "Bb", "Ab" } }
            };
            string description2 = "Все строки из {a,b} с символом ⊥ в конце";

            // Меню выбора грамматики
            Console.WriteLine("Выбор грамматики");
            Console.WriteLine("1 - Стандартная грамматика");
            Console.WriteLine("2 - Альтернативная грамматика");
            Console.Write("Ваш выбор: ");
            string choice = Console.ReadLine();

            Dictionary<string, List<string>> selectedGrammar;
            string selectedDescription;

            if (choice == "2")
            {
                selectedGrammar = grammar2;
                selectedDescription = description2;
            }
            else
            {
                selectedGrammar = grammar1;
                selectedDescription = description1;
            }

            // Генерация цепочек и вывод
            GenerateChainsAndExplain(selectedGrammar, selectedDescription);

            Console.WriteLine("\nПрограмма завершила работу. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// Генерация цепочек по грамматике, вывод языка и пояснение 
        private static void GenerateChainsAndExplain(Dictionary<string, List<string>> grammar, string languageDescription)
        {
            List<string> chains = new List<string> { "S" };
            List<string> results = new List<string>();

            // Генерация до 5 цепочек
            while (results.Count < 5)
            {
                List<string> newChains = new List<string>();

                foreach (var chain in chains)
                {
                    string nonTerminal = FindFirstNonTerminal(chain, grammar);

                    if (nonTerminal != null)
                    {
                        foreach (var rule in grammar[nonTerminal])
                        {
                            string newChain = ReplaceFirstOccurrence(chain, nonTerminal, rule);
                            newChains.Add(newChain);
                        }
                    }
                    else
                    {
                        if (IsTerminal(chain, grammar))
                        {
                            results.Add(chain);
                        }
                    }
                }

                chains = newChains;
                if (chains.Count == 0) break;
            }

            // Вывод цепочек
            Console.WriteLine("\n=== Сгенерированные цепочки ===");
            foreach (var r in results)
            {
                Console.WriteLine(r);
            }

            // Вывод языка
            Console.WriteLine($"\n=== Язык, порождаемый грамматикой ===");
            Console.WriteLine($"L = {languageDescription}");

            // Пояснение
            Console.WriteLine("\n=== Пояснение ===");
            if (languageDescription.Contains("a^n b^n"))
            {
                Console.WriteLine("- Строки начинаются с двух символов 'a'.");
                Console.WriteLine("- Рекурсивные правила F -> AFB | AB добавляют одинаковое количество 'a' и 'b'.");
                Console.WriteLine("- Минимальное количество символов a и b: n >= 2.");
                
            }
            else
            {
                Console.WriteLine("- Строки могут состоять из произвольной комбинации символов 'a' и 'b'.");
                Console.WriteLine("- Все строки заканчиваются символом ⊥ (маркер конца цепочки).");
                Console.WriteLine("- Множество цепочек: { w⊥ | w ∈ {a,b}* }");
                
            }
        }

        // Поиск первого нетерминала
        private static string FindFirstNonTerminal(string chain, Dictionary<string, List<string>> grammar)
        {
            foreach (var key in grammar.Keys)
            {
                if (chain.Contains(key))
                    return key;
            }
            return null;
        }

        // Проверка, что цепочка состоит только из терминалов
        private static bool IsTerminal(string chain, Dictionary<string, List<string>> grammar)
        {
            foreach (var key in grammar.Keys)
            {
                if (chain.Contains(key))
                    return false;
            }
            return true;
        }

        // Замена первого вхождения нетерминала на правило грамматики
        private static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int index = source.IndexOf(find);
            if (index < 0) return source;
            return source.Substring(0, index) + replace + source.Substring(index + find.Length);
        }
    }
}
