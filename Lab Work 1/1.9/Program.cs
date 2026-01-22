using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TA_Lab2
{
    internal class Program
    {
        // Класс для представления узла дерева вывода
        public class TreeNode
        {
            public string Value { get; set; }              // Символ (нетерминал или терминал)
            public List<TreeNode> Children { get; } = new List<TreeNode>(); // Дети в дереве

            public TreeNode(string value)
            {
                Value = value;
            }
        }

        // Класс для хранения правила грамматики
        public class GrammarRule
        {
            public string Left { get; }   // Левая часть (нетерминал)
            public List<string> Right { get; } // Правая часть (список символов)

            public GrammarRule(string left, List<string> right)
            {
                Left = left;
                Right = right;
            }
        }

        // Грамматика хранится как список правил
        private static List<GrammarRule> grammar = new List<GrammarRule>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Задание: дана грамматика G:");
            Console.WriteLine("S → a S b S | b S a S | ε");
            Console.WriteLine("а) Построить все возможные деревья вывода для цепочки abab");
            Console.WriteLine("б) Определить, является ли грамматика неоднозначной\n");

            // Загружаем грамматику
            LoadGrammar("S -> a S b S\nS -> b S a S\nS -> ε");

            string input = "abab"; // Цепочка для проверки

            // Получаем все возможные деревья вывода
            List<TreeNode> trees = Parse("S", input);

            Console.WriteLine("Все возможные деревья вывода для цепочки abab:\n");

            if (trees.Count == 0)
            {
                Console.WriteLine("Нет деревьев вывода.");
            }
            else
            {
                int index = 1;
                foreach (var tree in trees)
                {
                    Console.WriteLine($"Дерево {index}: (пошаговый вывод)");
                    Console.WriteLine(GetDerivationString(tree));
                    Console.WriteLine();

                    Console.WriteLine($"Дерево {index}: (структура)");
                    PrintTree(tree, "");
                    Console.WriteLine(new string('=', 40));
                    index++;
                }
            }

            // Пояснение про неоднозначность
            Console.WriteLine("\nВывод:");
            if (trees.Count > 1)
            {
                Console.WriteLine("Так как для цепочки abab существуют несколько различных деревьев вывода,");
                Console.WriteLine("данная грамматика является неоднозначной.");
            }
            else
            {
                Console.WriteLine("Для цепочки abab существует только одно дерево вывода, грамматика однозначная.");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// Загрузка грамматики из текстового описания
        private static void LoadGrammar(string text)
        {
            grammar.Clear();
            foreach (var line in text.Split('\n'))
            {
                var parts = line.Trim().Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string left = parts[0].Trim();
                    var right = parts[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    grammar.Add(new GrammarRule(left, right));
                }
            }
        }

        /// Построение всех возможных деревьев вывода для заданного нетерминала
        private static List<TreeNode> Parse(string nonTerminal, string input)
        {
            List<TreeNode> results = new List<TreeNode>();

            foreach (var rule in grammar)
            {
                if (!rule.Left.Equals(nonTerminal)) continue;

                // Обработка ε-правила
                if (rule.Right.Count == 1 && rule.Right[0] == "ε")
                {
                    if (string.IsNullOrEmpty(input))
                    {
                        TreeNode node = new TreeNode(nonTerminal);
                        node.Children.Add(new TreeNode("ε"));
                        results.Add(node);
                    }
                }
                else
                {
                    MatchRule(rule.Right, input, parsedChildren =>
                    {
                        TreeNode node = new TreeNode(nonTerminal);
                        node.Children.AddRange(parsedChildren);

                        // Если дерево даёт правильную строку — сохраняем
                        if (Reconstruct(node).Equals(input))
                            results.Add(node);
                    });
                }
            }

            return results;
        }

        /// Сопоставление правила с входной 
        private static void MatchRule(List<string> symbols, string input, Action<List<TreeNode>> onMatch)
        {
            MatchRecursive(symbols, input, 0, new List<TreeNode>(), onMatch);
        }

        /// Рекурсивное сопоставление символов правила с подстрокой 
        private static void MatchRecursive(List<string> symbols, string input, int pos, List<TreeNode> partial, Action<List<TreeNode>> onMatch)
        {
            if (symbols.Count == 0)
            {
                if (pos == input.Length) onMatch(new List<TreeNode>(partial));
                return;
            }

            string first = symbols[0];
            var rest = symbols.Skip(1).ToList();

            // Если символ — нетерминал
            if (grammar.Any(r => r.Left.Equals(first)))
            {
                for (int i = pos; i <= input.Length; i++)
                {
                    string leftInput = input.Substring(pos, i - pos);
                    var trees = Parse(first, leftInput);
                    foreach (var t in trees)
                    {
                        partial.Add(t);
                        MatchRecursive(rest, input, i, partial, onMatch);
                        partial.RemoveAt(partial.Count - 1);
                    }
                }
            }
            else // Если символ — терминал (a или b)
            {
                if (pos < input.Length && input.Substring(pos, 1) == first)
                {
                    TreeNode terminal = new TreeNode(first);
                    partial.Add(terminal);
                    MatchRecursive(rest, input, pos + 1, partial, onMatch);
                    partial.RemoveAt(partial.Count - 1);
                }
            }
        }

        /// Восстановление строки из дерева
        private static string Reconstruct(TreeNode node)
        {
            if (node.Value == "a" || node.Value == "b") return node.Value;
            if (node.Value == "ε") return "";
            StringBuilder sb = new StringBuilder();
            foreach (var child in node.Children)
            {
                sb.Append(Reconstruct(child));
            }
            return sb.ToString();
        }

        /// Получение строкового представления вывода (пошаговое)
        private static string GetDerivationString(TreeNode node)
        {
            if (node.Children.Count == 0) return node.Value;

            var sb = new StringBuilder();
            sb.Append(node.Value);
            sb.Append(" -> ");

            for (int i = 0; i < node.Children.Count; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(node.Children[i].Value);
            }

            foreach (var child in node.Children)
            {
                var childDerivation = GetDerivationString(child);
                if (!string.IsNullOrEmpty(childDerivation))
                {
                    sb.AppendLine();
                    sb.Append(childDerivation);
                }
            }

            return sb.ToString();
        }

        /// Текстовая визуализация дерева с отступами
        private static void PrintTree(TreeNode node, string indent, bool isLast = true)
        {
            Console.Write(indent);
            Console.Write(isLast ? "└─" : "├─");
            Console.WriteLine(node.Value);

            for (int i = 0; i < node.Children.Count; i++)
            {
                PrintTree(node.Children[i], indent + (isLast ? "  " : "│ "), i == node.Children.Count - 1);
            }
        }
    }
}
