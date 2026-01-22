// Program.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TA_Lab6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Устанавливаем кодировку UTF-8 для корректного отображения кириллицы
            Console.OutputEncoding = Encoding.UTF8;



            // Исходная грамматика
            string inputGrammar = @"S -> AB | ABS
AB -> BA
BA -> AB
A -> a
B -> b";

            // Преобразуем грамматику и формируем отчет
            string conversionReport = ConvertGrammar(inputGrammar);

            // Выводим результат на экран
            Console.WriteLine(conversionReport);

            Console.WriteLine("Для выхода нажмите любую клавишу...");
            Console.ReadKey();
        }

        /// Метод для преобразования грамматики и генерации цепочек
        private static string ConvertGrammar(string inputGrammar)
        {
            var report = new StringBuilder();

            // Добавляем исходную грамматику в отчет (только здесь один раз)
            report.AppendLine("Исходная грамматика:");
            report.AppendLine(inputGrammar);
            report.AppendLine();

            // Генерация цепочек исходной грамматики
            report.AppendLine("Сгенерированные цепочки исходной грамматики (максимум 5):");
            List<string> chains = new List<string> { "S" }; // Начальная цепочка
            List<string> results = new List<string>();

            // Определяем правила исходной грамматики
            Dictionary<string, List<string>> grammarRules = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "AB", "ABS" } },
                { "A", new List<string> { "a" } },
                { "B", new List<string> { "b" } }
            };

            // Генерация терминальных цепочек
            while (results.Count < 5)
            {
                List<string> newChains = new List<string>();
                foreach (var chain in chains)
                {
                    string nonTerminal = FindFirstNonTerminal(chain, grammarRules);
                    if (nonTerminal != null)
                    {
                        foreach (var rule in grammarRules[nonTerminal])
                        {
                            string newChain = ReplaceFirstOccurrence(chain, nonTerminal, rule);
                            newChains.Add(newChain);
                        }
                    }
                    else
                    {
                        if (IsTerminal(chain, grammarRules))
                            results.Add(chain);
                    }
                }
                chains = newChains;
                if (chains.Count == 0) break;
            }

            foreach (var r in results)
                report.AppendLine(r);

            report.AppendLine();
            report.AppendLine("Описание языка:");
            report.AppendLine("Грамматика порождает язык L = { w | w содержит одинаковое количество символов 'a' и 'b' }");
            report.AppendLine();

            // Преобразуем грамматику в КС-грамматику (контекстно-свободную)
            report.AppendLine("Преобразованная КС-грамматика (эквивалентная):");
            report.AppendLine("S -> aS | bS | a | b");
            report.AppendLine();

            // Генерация цепочек для КС-грамматики
            List<string> newChainsList = new List<string> { "S" };
            List<string> newResults = new List<string>();
            Dictionary<string, List<string>> newGrammarRules = new Dictionary<string, List<string>>()
            {
                { "S", new List<string> { "aS", "bS", "a", "b" } }
            };

            while (newResults.Count < 5)
            {
                List<string> tempChains = new List<string>();
                foreach (var chain in newChainsList)
                {
                    string nonTerminal = FindFirstNonTerminal(chain, newGrammarRules);
                    if (nonTerminal != null)
                    {
                        foreach (var rule in newGrammarRules[nonTerminal])
                        {
                            string newChain = ReplaceFirstOccurrence(chain, nonTerminal, rule);
                            tempChains.Add(newChain);
                        }
                    }
                    else
                    {
                        if (IsTerminal(chain, newGrammarRules))
                            newResults.Add(chain);
                    }
                }
                newChainsList = tempChains;
                if (newChainsList.Count == 0) break;
            }

            report.AppendLine("Сгенерированные цепочки для КС-грамматики:");
            foreach (var r in newResults)
                report.AppendLine(r);

            report.AppendLine();
            report.AppendLine("Описание языка для КС-грамматики:");
            report.AppendLine("Грамматика порождает все возможные непустые строки над алфавитом {a, b}.");

            return report.ToString();
        }

        /// Поиск первого нетерминала в цепочке
        private static string FindFirstNonTerminal(string chain, Dictionary<string, List<string>> grammarRules)
        {
            foreach (var key in grammarRules.Keys)
                if (chain.Contains(key)) return key;
            return null;
        }

        /// Проверка, является ли цепочка полностью терминальной
        private static bool IsTerminal(string chain, Dictionary<string, List<string>> grammarRules)
        {
            foreach (var key in grammarRules.Keys)
                if (chain.Contains(key)) return false;
            return true;
        }

        /// Замена первого вхождения подстроки
        private static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int index = source.IndexOf(find);
            if (index < 0) return source;
            return source.Substring(0, index) + replace + source.Substring(index + find.Length);
        }
    }
}
