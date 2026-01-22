using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Lab1_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Устанавливаем кодировку UTF-8
            Console.OutputEncoding = Encoding.UTF8;



            // ЯЗЫК a) L = { a^n b^m c^k | n, m, k > 0 }
            Dictionary<string, List<string>> grammarA = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "ABC" } },   
                { "A", new List<string> { "aA", "a" } }, 
                { "B", new List<string> { "bB", "b" } }, 
                { "C", new List<string> { "cC", "c" } }  
            };

            Console.WriteLine("Язык a) L = {a^n b^m c^k | n, m, k > 0}");
            Console.WriteLine("Правила языка:");
            Console.WriteLine("S -> ABC");
            Console.WriteLine("A -> aA");
            Console.WriteLine("A -> a");
            Console.WriteLine("B -> bB");
            Console.WriteLine("B -> b");
            Console.WriteLine("C -> cC");
            Console.WriteLine("C -> c");
            Console.WriteLine("Примеры цепочек:");
            List<string> resultA = GenerateChains(grammarA, "S");
            Console.WriteLine(string.Join(", ", resultA) + "\n");

            // ЯЗЫК b) L = { 0^n (10)^m | n, m ≥ 0 }
            Dictionary<string, List<string>> grammarB = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "AB" } },     
                { "A", new List<string> { "0A", "e" } }, 
                { "B", new List<string> { "10B", "e" } } 
            };

            Console.WriteLine("Язык b) L = {0^n10^m | n, m ≥ 0}");
            Console.WriteLine("Правила языка:");
            Console.WriteLine("S -> AB");
            Console.WriteLine("A -> 0A");
            Console.WriteLine("A -> e");
            Console.WriteLine("B -> 10B");
            Console.WriteLine("B -> e");
            Console.WriteLine("Примеры цепочек:");
            List<string> resultB = GenerateChains(grammarB, "S");
            Console.WriteLine(string.Join(", ", resultB) + "\n");


            // ЯЗЫК c) L = { w w^R | w ∈ {0,1}* }
            Dictionary<string, List<string>> grammarC = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "0S0", "1S1", "e" } } // Рекурсивное построение палиндромов
            };

            Console.WriteLine("Язык c) L = {ww^R | w ∈ {0,1}*}");
            Console.WriteLine("Правила языка:");
            Console.WriteLine("S -> 0S0");
            Console.WriteLine("S -> 1S1");
            Console.WriteLine("S -> e");
            Console.WriteLine("Примеры цепочек:");
            List<string> resultC = GenerateChains(grammarC, "S");
            Console.WriteLine(string.Join(", ", resultC) + "\n");

            Console.WriteLine("Генерация цепочек завершена. Нажмите любую клавишу для выхода.");
            Console.ReadKey();
        }

        /// Метод для генерации цепочек на основе грамматики.
        /// Идея: начиная с начального символа, рекурсивно заменяем нетерминалы по правилам.
        /// Генерация продолжается, пока не получим терминальные цепочки или не достигнем лимита.
        private static List<string> GenerateChains(Dictionary<string, List<string>> grammarRules, string startSymbol)
        {
            List<string> chains = new List<string> { startSymbol }; // список цепочек, с которых начинаем
            List<string> results = new List<string>();              // список результатов
            int maxResults = 6;                                     // ограничение для наглядности

            while (results.Count < maxResults)
            {
                List<string> newChains = new List<string>();

                foreach (var chain in chains)
                {
                    // ищем первый нетерминал
                    string nonTerminal = FindFirstNonTerminal(chain, grammarRules);

                    if (nonTerminal != null)
                    {
                        // заменяем его всеми возможными правилами
                        foreach (var rule in grammarRules[nonTerminal])
                        {
                            string newChain = ReplaceFirstOccurrence(chain, nonTerminal, rule);
                            newChains.Add(newChain);
                        }
                    }
                    else
                    {
                        // если нетерминалов больше нет — цепочка считается результатом
                        results.Add(chain);
                    }
                }

                chains = newChains;

                if (chains.Count == 0)
                    break;
            }

            return results;
        }


        /// Метод находит первый нетерминал в цепочке (например S, A, B, C).
        private static string FindFirstNonTerminal(string chain, Dictionary<string, List<string>> grammarRules)
        {
            foreach (var key in grammarRules.Keys)
            {
                if (chain.Contains(key))
                    return key;
            }
            return null;
        }

        /// Метод заменяет первое вхождение нетерминала на правило.
        private static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int index = source.IndexOf(find);
            if (index < 0) return source;
            return source.Substring(0, index) + replace + source.Substring(index + find.Length);
        }
    }
}
