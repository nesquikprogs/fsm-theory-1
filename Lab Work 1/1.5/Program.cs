using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Lab1_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;


            // Определяем первую грамматику
            // Грамматика №1
            Grammar g1 = new Grammar(
                new Dictionary<string, List<string>>
                {
                    ["S"] = new List<string> { "aSL", "aL" },
                    ["L"] = new List<string> { "Kc" },
                    ["K"] = new List<string> { "b" }
                },
                new List<(string LeftHandSide, string RightHandSide)>
                {
                    ("cK", "Kc"),
                });

            // Грамматика №2:
            Grammar g2 = new Grammar(
                new Dictionary<string, List<string>>
                {
                    ["S"] = new List<string> { "aSBc", "abc" },
                    ["B"] = new List<string>(),
                },
                new List<(string LeftHandSide, string RightHandSide)>
                {
                    ("cB", "Bc"),
                    ("bB", "bb"),
                });

            // Проверяем эквивалентность грамматик
            bool areEquivalent = AreGrammarsEquivalent(g1, g2);

            // Вывод анализа
            Console.WriteLine("Анализ грамматик");

            Console.WriteLine("Первая грамматика:");
            Console.WriteLine("S → aSL | aL");
            Console.WriteLine("L → Kc");
            Console.WriteLine("K → b");
            Console.WriteLine("cK → Kc (контекстное правило)");
            Console.WriteLine("→ Порождает язык: a^n b^m c^m (n > 0, m > 0)\n");

            Console.WriteLine("Вторая грамматика:");
            Console.WriteLine("S → aSBc | abc");
            Console.WriteLine("cB → Bc");
            Console.WriteLine("bB → bb");
            Console.WriteLine("→ Порождает язык: a^n b^n c^n (n > 0)\n");

            Console.WriteLine("Примеры строк из первой грамматики:");
            foreach (var s in GenerateLanguage(g1, 4, 10)) Console.WriteLine(s);

            Console.WriteLine("\nПримеры строк из второй грамматики:");
            foreach (var s in GenerateLanguage(g2, 4, 10)) Console.WriteLine(s);

            Console.WriteLine("Вывод");

            if (areEquivalent)
            {
                Console.WriteLine("Грамматики эквивалентны.");
                Console.WriteLine("Обе грамматики пораждают один и тот же язык.");
            }
            else
            {
                Console.WriteLine("Грамматики не эквивалентны, потому что пораждают разные языки.");
                Console.WriteLine("Первая грамматика позволяет независимое количество 'a' и пар 'b-c'.");
                Console.WriteLine("Вторая грамматика требует одинаковое количество 'a', 'b' и 'c'.");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Метод для проверки эквивалентности грамматик
        static bool AreGrammarsEquivalent(Grammar g1, Grammar g2)
        {
            const int maxDepth = 5;           // ограничение глубины вывода
            const int maxStringsToCheck = 20; // ограничение по количеству строк

            HashSet<string> lang1 = GenerateLanguage(g1, maxDepth, maxStringsToCheck);
            HashSet<string> lang2 = GenerateLanguage(g2, maxDepth, maxStringsToCheck);

            // сравниваем множества строк
            return lang1.SetEquals(lang2);
        }

        // Метод для генерации языка на основе грамматики
        static HashSet<string> GenerateLanguage(Grammar grammar, int maxDepth, int maxStrings)
        {
            HashSet<string> generated = new HashSet<string>();
            Queue<(string str, int depth)> queue = new Queue<(string, int)>();
            queue.Enqueue((grammar.StartSymbol, 0));

            while (queue.Count > 0 && generated.Count < maxStrings)
            {
                var (current, depth) = queue.Dequeue();

                if (depth > maxDepth) continue;

                bool hasNonTerminal = false;
                foreach (var nt in grammar.Productions.Keys)
                {
                    if (current.Contains(nt))
                    {
                        hasNonTerminal = true;
                        foreach (var prod in grammar.Productions[nt])
                        {
                            string newStr = ReplaceFirst(current, nt, prod);
                            queue.Enqueue((newStr, depth + 1));
                        }
                    }
                }

                // обработка контекстных правил
                foreach (var (lhs, rhs) in grammar.ContextualRules)
                {
                    if (current.Contains(lhs))
                    {
                        string newStr = current.Replace(lhs, rhs);
                        queue.Enqueue((newStr, depth + 1));
                    }
                }

                if (!hasNonTerminal && !string.IsNullOrEmpty(current))
                {
                    generated.Add(current);
                }
            }

            return generated;
        }

        // Замена первого вхождения нетерминала
        static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0) return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        // Класс для описания грамматики
        class Grammar
        {
            public readonly string StartSymbol;
            public readonly Dictionary<string, List<string>> Productions;
            public readonly List<(string LeftHandSide, string RightHandSide)> ContextualRules;

            public Grammar(Dictionary<string, List<string>> productions,
                         List<(string LeftHandSide, string RightHandSide)> contextualRules)
            {
                StartSymbol = "S"; // начальный символ всегда S
                Productions = productions;
                ContextualRules = contextualRules;
            }
        }
    }
}
