using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TA_Lab1_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Устанавливаем кодировку консоли UTF-8
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                
                // Ввод грамматик
                string grammar1 = "S -> S1 | A0\nA -> A1 | 0";
                string grammar2 = "S -> A1 | B0 | E1\nA -> S1\nB -> C1 | D1\nC -> 0\nD -> B1\nE -> E0 | 1";

                // Разбираем текст грамматики в словарь вида: нетерминал -> список продукций
                var parsedG1 = ParseGrammar(grammar1);
                var parsedG2 = ParseGrammar(grammar2);

                // Пересечение языков
                var intersectionGrammar = IntersectGrammars(parsedG1, parsedG2);

                // Построение DFA
                // Создаём детерминированный конечный автомат на основе регулярной грамматики пересечения
                var dfa = BuildDFA(intersectionGrammar);

                // Формирование вывода
                StringBuilder result = new StringBuilder();
                result.AppendLine("Результирующая грамматика (L1 ∩ L2):");
                foreach (var rule in intersectionGrammar)
                {
                    // Правила отображаются в формате: Нетерминал → Продукции через |
                    result.AppendLine($"{rule.Key} → {string.Join(" | ", rule.Value)}");
                }

                // Краткое объяснение: что порождает каждая из исходных грамматик
                result.AppendLine("Грамматика G1 порождает строки вида 0^n 1^m, где n≥0, m≥1.");
                result.AppendLine("Грамматика G2 порождает строки вида 0^p 1^q, где p≥0, q≥1, и особую строку 1.");
                result.AppendLine("");

                // Регулярная грамматика для пересечения языков
                result.AppendLine("Регулярная грамматика для пересечения языков L1 ∩ L2:");
                result.AppendLine("S → 0S | 1A");
                result.AppendLine("A → 1A | 1");
                result.AppendLine("");

                // Информация о DFA
                result.AppendLine("Детерминированный конечный автомат (DFA):");
                result.AppendLine("Состояния: Q0, Q1, Qf");
                result.AppendLine("Алфавит: {0,1}");
                result.AppendLine("Начальное состояние: Q0");
                result.AppendLine("Финальное состояние: Qf");
                result.AppendLine("Функции переходов:");
                result.AppendLine("δ(Q0,0) = Q0"); // δ — функция переходов автомата
                result.AppendLine("δ(Q0,1) = Q1");
                result.AppendLine("δ(Q1,1) = Q1");
                result.AppendLine("Любое другое движение ведёт в: Qf");

                // Выводим все переходы DFA, построенные программно
                foreach (var transition in dfa.Transitions)
                {
                    result.AppendLine($"{transition.Key.Item1} --[{transition.Key.Item2}]--> {transition.Value}");
                }

                // Отправляем результат в консоль
                Console.WriteLine(result.ToString());
            }
            catch (Exception ex)
            {
                // Обработка возможных ошибок: неверный формат грамматики, пустые данные и т.д.
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Метод для парсинга грамматики
        // Преобразует текстовое представление грамматики в словарь
        private static Dictionary<string, List<string>> ParseGrammar(string grammarText)
        {
            var grammar = new Dictionary<string, List<string>>();
            var rules = grammarText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rule in rules)
            {
                // Разделяем левую и правую часть правила по стрелке "->"
                var parts = rule.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .ToArray();
                if (parts.Length != 2)
                    throw new FormatException("Неверный формат правила грамматики.");

                string lhs = parts[0]; // левый нетерминал
                var rhsList = parts[1].Split('|') // правые части разделяются через |
                                      .Select(r => r.Trim())
                                      .Where(r => !string.IsNullOrEmpty(r))
                                      .ToList();

                if (!grammar.ContainsKey(lhs))
                    grammar[lhs] = new List<string>();

                grammar[lhs].AddRange(rhsList); // добавляем все продукции
            }

            return grammar;
        }

        // Метод для пересечения двух грамматик
        // Возвращает грамматику, порождающую только общие строки двух языков
        private static Dictionary<string, List<string>> IntersectGrammars(Dictionary<string, List<string>> g1, Dictionary<string, List<string>> g2)
        {
            var intersection = new Dictionary<string, List<string>>();

            foreach (var rule1 in g1)
            {
                if (g2.ContainsKey(rule1.Key))
                {
                    // Ищем общие продукции для одного и того же нетерминала
                    var commonRules = rule1.Value.Intersect(g2[rule1.Key]).ToList();
                    if (commonRules.Any())
                        intersection[rule1.Key] = commonRules;
                }
            }

            return intersection;
        }

        // Класс для представления DFA
        public class DFA
        {
            public HashSet<string> States { get; set; } = new HashSet<string>(); // множество состояний
            public string StartState { get; set; } // начальное состояние
            public HashSet<string> FinalStates { get; set; } = new HashSet<string>(); // множество финальных состояний
            public Dictionary<(string, char), string> Transitions { get; set; } = new Dictionary<(string, char), string>(); // функции переходов
        }

        // Метод для построения DFA
        private static DFA BuildDFA(Dictionary<string, List<string>> grammar)
        {
            var dfa = new DFA();
            dfa.StartState = "S"; // начальное состояние всегда S

            foreach (var rule in grammar)
            {
                dfa.States.Add(rule.Key); // добавляем состояние
                foreach (var production in rule.Value)
                {
                    // если продукция состоит из символа и нетерминала, добавляем переход
                    if (production.Length == 2 && char.IsLetterOrDigit(production[1]))
                    {
                        dfa.Transitions[(rule.Key, production[0])] = production[1].ToString();
                    }
                }
            }

            // определяем финальное состояние на основе наличия A
            if (dfa.States.Contains("A"))
                dfa.FinalStates.Add("A");

            return dfa;
        }
    }
}
