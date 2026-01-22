using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TA_Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Задание: Построить регулярную грамматику, эквивалентную грамматике:");
            Console.WriteLine("S → A.A\nA → B | BA\nB → 0 | 1\n");

            // Исходная грамматика в виде правил
            Dictionary<string, List<string>> grammarRules = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "A.A" } },
                { "A", new List<string> { "B", "BA" } },
                { "B", new List<string> { "0", "1" } }
            };

            // Отчёт для пользователя
            var report = new StringBuilder();
            report.AppendLine("Исходная грамматика:");
            report.AppendLine("S → A.A");
            report.AppendLine("A → B | BA");
            report.AppendLine("B → 0 | 1");
            report.AppendLine();

            // Генерация цепочек для исходной грамматики
            var results = GenerateChains(grammarRules, "S", 5);
            report.AppendLine("Примеры цепочек, порождаемых исходной грамматикой:");
            foreach (var res in results) report.AppendLine(res);
            report.AppendLine();

            report.AppendLine("Грамматика порождает язык L = {w | w ∈ {0, 1}+}");
            report.AppendLine("То есть все непустые строки из нулей и единиц с точкой.\n");

            // Новая (регулярная) грамматика — выводим правила по одному
            report.AppendLine("Эквивалентная регулярная(один нетерминал справа) грамматика");
            report.AppendLine("S -> A0");
            report.AppendLine("S -> A1");
            report.AppendLine("S -> S0");
            report.AppendLine("S -> S1");
            report.AppendLine("A -> B");
            report.AppendLine("B -> 0");
            report.AppendLine("B -> 1");
            report.AppendLine("B -> B0");
            report.AppendLine("B -> B1");
            report.AppendLine();

            // Определяем правила для новой грамматики
            Dictionary<string, List<string>> newGrammarRules = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "A0", "A1", "S0", "S1" } },
                { "A", new List<string> { "B" } },
                { "B", new List<string> { "0", "1", "B0", "B1" } }
            };

            // Генерация цепочек для новой грамматики
            var newResults = GenerateChains(newGrammarRules, "S", 5);
            report.AppendLine("Примеры цепочек, порождаемых регулярной грамматикой:");
            foreach (var res in newResults) report.AppendLine(res);
            report.AppendLine();

            report.AppendLine("Грамматика порождает все возможные непустые строки над алфавитом {0, 1}.");

            // Вывод отчёта на экран
            Console.WriteLine(report.ToString());

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// Метод генерации цепочек по грамматике.
        /// Начиная с начального символа, постепенно заменяем нетерминалы
        /// на правые части правил, пока не получим терминальные строки
        private static List<string> GenerateChains(Dictionary<string, List<string>> grammarRules, string startSymbol, int limit)
        {
            List<string> chains = new List<string> { startSymbol }; // начальная строка
            List<string> results = new List<string>(); // готовые терминальные строки

            while (results.Count < limit)
            {
                List<string> newChains = new List<string>();
                foreach (var chain in chains)
                {
                    string nonTerminal = FindFirstNonTerminal(chain, grammarRules);
                    if (nonTerminal != null)
                    {
                        // заменяем первый встретившийся нетерминал всеми возможными правилами
                        foreach (var rule in grammarRules[nonTerminal])
                        {
                            string newChain = ReplaceFirstOccurrence(chain, nonTerminal, rule);
                            newChains.Add(newChain);
                        }
                    }
                    else
                    {
                        // если нет нетерминалов — цепочка терминальная
                        results.Add(chain);
                    }
                }
                chains = newChains;

                if (chains.Count == 0) break; // если больше нечего разворачивать
            }

            return results;
        }

        /// Поиск первого нетерминала в строке.
        /// Нетерминал — это символ, для которого есть правила в грамматике
        private static string FindFirstNonTerminal(string chain, Dictionary<string, List<string>> grammarRules)
        {
            foreach (var key in grammarRules.Keys)
            {
                if (chain.Contains(key))
                {
                    return key;
                }
            }
            return null;
        }

        /// Замена первого вхождения подстроки (нетерминала) на правило
        private static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int index = source.IndexOf(find);
            if (index < 0) return source;
            return source.Substring(0, index) + replace + source.Substring(index + find.Length);
        }
    }
}
