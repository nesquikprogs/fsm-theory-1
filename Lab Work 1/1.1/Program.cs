// Program.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarDerivationConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Ввод данных            
            // Вариант 1
            string grammarInput1 = @"S -> T | T+S | T-S
T -> F | F*T
F -> a | b";
            string chain1 = "a-b*a+b";

            // Вариант 2
            string grammarInput2 = @"S -> aSBC | abC
CB -> BC
bB -> bb
bC -> bc
cC -> cc";
            string chain2 = "aaabbbccc";

            string grammarInput = grammarInput1;
            string chain = chain1;

            //Меню выбора варианта грамматики и цепочки 
            Console.WriteLine("=== Выбор исходных данных ===");
            Console.WriteLine("1 - Стандартная грамматика и цепочка");
            Console.WriteLine("2 - Альтернативная грамматика и цепочка");
            Console.Write("Ваш выбор: ");
            string choice = Console.ReadLine();

            if (choice == "2")
            {
                grammarInput = grammarInput2;
                chain = chain2;
            }

            // Вывод выбранных данных
            Console.WriteLine("\n=== Используемые данные ===");
            Console.WriteLine("Грамматика:");
            Console.WriteLine(grammarInput);
            Console.WriteLine($"Цепочка: {chain}\n");

            // Парсинг грамматики
            // Разбиваем текст грамматики на словарь <Нетерминал, Список правил>
            var parsedGrammar = ParseGrammar(grammarInput);

            // Генерация цепочек (рекурсивный алгоритм построения дерева вывода)
            // FindDerivation пытается "развернуть" S в целевую цепочку
            var derivationTree = FindDerivation("S", chain, parsedGrammar);

            // Вывод результатов 
            Console.WriteLine("=== Результаты ===");
            if (derivationTree != null)
            {
                Console.WriteLine("Вывод найден:");
                Console.WriteLine(ConvertDerivationTreeToText(derivationTree, 0));
            }
            else
            {
                Console.WriteLine("Вывод не найден.");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Многострочный ввод грамматики 
        static string ReadMultilineInput()
        {
            StringBuilder sb = new StringBuilder();
            string line;
            while (true)
            {
                line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;
                sb.AppendLine(line);
            }
            return sb.ToString().Trim();
        }

        // Класс узла дерева вывода 
        public class DerivationNode
        {
            public string Value { get; set; }
            public string Rule { get; set; }
            public List<DerivationNode> Children { get; set; } = new List<DerivationNode>();
        }

        // Парсинг грамматики 
        // Преобразует текстовое представление правил в словарь для удобного поиска.
        static Dictionary<string, List<string>> ParseGrammar(string grammarText)
        {
            return grammarText
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(rule => rule.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length == 2)
                .ToDictionary(
                    parts => parts[0].Trim(),
                    parts => parts[1].Split('|').Select(p => p.Trim()).ToList()
                );
        }

        // Построение дерева вывода (генерация цепочек) 
        // Рекурсивно заменяет нетерминалы на правые части правил грамматики
        // до тех пор, пока цепочка не совпадёт с целевой (targetChain).
        static DerivationNode FindDerivation(string currentChain, string targetChain, Dictionary<string, List<string>> grammar)
        {
            // Если текущая цепочка совпала с целевой — базовый случай, генерация завершена
            if (currentChain == targetChain)
            {
                return new DerivationNode { Value = currentChain };
            }

            // Если текущая цепочка длиннее целевой, дальнейшие шаги невозможны
            if (currentChain.Length > targetChain.Length)
            {
                return null;
            }

            // Находим все нетерминалы в текущей цепочке, чтобы попробовать применить правила
            var nonTerminals = FindNonTerminals(currentChain, grammar.Keys);

            foreach (var (nonTerminal, index) in nonTerminals)
            {
                foreach (string production in grammar[nonTerminal])
                {
                    // Генерируем новую цепочку, заменяя нетерминал на продукцию
                    string newChain = currentChain.Substring(0, index) + production + currentChain.Substring(index + nonTerminal.Length);

                    // Рекурсивно пробуем развернуть новую цепочку
                    var childNode = FindDerivation(newChain, targetChain, grammar);
                    if (childNode != null)
                    {
                        // Если цепочка сработала, формируем узел дерева вывода с применённым правилом
                        return new DerivationNode
                        {
                            Value = currentChain,
                            Rule = $"{currentChain} -> {newChain} (по правилу {nonTerminal} -> {production})",
                            Children = { childNode }
                        };
                    }
                }
            }
            return null;
        }

        // Поиск нетерминалов в цепочке 
        static List<(string, int)> FindNonTerminals(string chain, IEnumerable<string> nonTerminals)
        {
            return nonTerminals
                .Where(nonTerminal => nonTerminal.Length <= chain.Length)
                .SelectMany(nonTerminal =>
                    Enumerable.Range(0, chain.Length - nonTerminal.Length + 1)
                        .Where(i => chain.Substring(i, nonTerminal.Length) == nonTerminal)
                        .Select(i => (nonTerminal, i))
                ).ToList();
        }

        // Вывод дерева вывода в консоль 
        static string ConvertDerivationTreeToText(DerivationNode node, int level)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(new string(' ', level * 4));
            sb.AppendLine(node.Value);

            foreach (var child in node.Children)
            {
                sb.Append(ConvertDerivationTreeToText(child, level + 1));
            }

            return sb.ToString();
        }
    }
}
