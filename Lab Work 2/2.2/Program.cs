using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Lab2Console
{
    internal class Program
    {
        private static readonly Random random = new Random();

        public enum State
        {
            H,  // Начальное состояние, принимает первый символ цепочки (0, 1, +, -)
            A,  // Состояние для обработки последовательностей 0 и 1
            B,  // Состояние после знака + или -, перед возвратом к A
            S   // Конечное состояние, символ ⊥ завершает цепочку
        }

        static void Main(string[] args)
        {
            // Настраиваем консоль на вывод Unicode для символа ⊥
            Console.OutputEncoding = Encoding.UTF8;
            char bottomSymbol = '\u22A5'; // Символ конца цепочки ⊥

            // Регулярная грамматика для ДС
            string grammarText =
$@"H -> 0A | 1A | +A | -A
A -> 0A | 1A | {bottomSymbol}S
B -> 0A | 1A
S -> {bottomSymbol}";

            // Цепочки для проверки
            string[] inputChains = { $"1011{bottomSymbol}", $"10+011{bottomSymbol}", $"0-101+1{bottomSymbol}" };

            // Парсим грамматику
            Dictionary<string, List<string>> grammarRules = ParseGrammar(grammarText);

            // Строим диаграмму переходов ДС
            StringBuilder diagram = BuildDiagram(grammarRules, bottomSymbol);

            Console.WriteLine("Диаграмма переходов ДС:");
            Console.WriteLine(diagram);

            Console.WriteLine("Регулярная грамматика:");
            Console.WriteLine(grammarText);
            Console.WriteLine();

            // Проверяем цепочки на принадлежность языку
            Console.WriteLine("Результаты проверки цепочек:");
            foreach (var chain in inputChains)
            {
                bool isValid = ValidateChain(chain);
                Console.WriteLine($"Цепочка \"{chain}\" {(isValid ? "принадлежит" : "не принадлежит")} языку.");
            }
            Console.WriteLine();

            // Описание языка
            string languageDescription = $"Грамматика порождает язык: L = ({{0,1,-,+}}^n {bottomSymbol} | n >= 1), где строка начинается с 0,1 или знака (+/-), " +
                                         $"далее произвольное количество 0 и 1, и завершается специальной меткой {bottomSymbol}.";
            Console.WriteLine("Порождаемый язык:");
            Console.WriteLine(languageDescription);
            Console.WriteLine();

            // Генерация пяти случайных цепочек по ДС
            Console.WriteLine("Пять случайных цепочек, построенных на ДС:");
            for (int i = 0; i < 5; i++)
            {
                string generatedString = GenerateRandomString(bottomSymbol);
                Console.WriteLine($"Цепочка {i + 1}: {generatedString}");
            }
        }

        // Парсинг грамматики в словарь
        private static Dictionary<string, List<string>> ParseGrammar(string grammarText)
        {
            var grammar = new Dictionary<string, List<string>>();
            var lines = grammarText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;

                var lhs = parts[0].Trim();
                var rhs = parts[1].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                grammar[lhs] = new List<string>();
                foreach (var rule in rhs)
                    grammar[lhs].Add(rule.Trim());
            }
            return grammar;
        }

        // Построение диаграммы переходов ДС для наглядного отображения
        private static StringBuilder BuildDiagram(Dictionary<string, List<string>> rules, char bottomSymbol)
        {
            StringBuilder diagram = new StringBuilder();
            foreach (var rule in rules)
            {
                string left = rule.Key;
                foreach (var production in rule.Value)
                {
                    string firstSymbol = production.Length > 0 ? production[0].ToString() : bottomSymbol.ToString();
                    string target = production.Length > 1 ? production.Substring(1) : "S";
                    diagram.AppendLine($"{left} --{firstSymbol}--> {target}");
                }
            }
            return diagram;
        }


        /// Ответ на вопрос о взаимосвязи: один и тот же автомат, но предназназначенный для разных задач - ValidateChain автомат распознавания(проверки на принадлежность языку) 
        ///                                                                                                проходит по каждому символу входной цепочки и проверяет, 
        ///                                                                                                соответствует ли она правилам переходов
        ///                                                                                              - GenerateRandomString автомат генерации (создания рандомных цепочек) 
        ///                                                                                                начиная с начального состояния H, он случайным образом выбирает допустимые переходы, 
        ///                                                                                                добавляя символы, пока не достигнет S
        // Проверка цепочки на принадлежность языку по ДС
        private static bool ValidateChain(string chain)
        {
            char bottomSymbol = '\u22A5';
            State currentState = State.H;

            for (int i = 0; i < chain.Length; i++)
            {
                char symbol = chain[i];

                switch (currentState)
                {
                    case State.H:
                        if (symbol == '0' || symbol == '1')
                            currentState = State.A;
                        else if (symbol == '+' || symbol == '-')
                            currentState = State.A;
                        else
                            return false;
                        break;

                    case State.A:
                        if (symbol == '0' || symbol == '1')
                            currentState = State.A;
                        else if (symbol == '+' || symbol == '-')
                            currentState = State.B;
                        else if (symbol == bottomSymbol)
                            currentState = State.S;
                        else
                            return false;
                        break;

                    case State.B:
                        if (symbol == '0' || symbol == '1')
                            currentState = State.A;
                        else
                            return false;
                        break;

                    case State.S:
                        // После ⊥ ничего не должно идти
                        return i == chain.Length - 1;
                }
            }

            return currentState == State.S;
        }

        // Генерация случайной цепочки по ДС
        public static string GenerateRandomString(char bottomSymbol)
        {
            char[] binarySymbols = { '0', '1' };
            char[] operationSymbols = { '+', '-' };
            State currentState = State.H;
            StringBuilder result = new StringBuilder();
            Random random = new Random();

            while (true)
            {
                switch (currentState)
                {
                    case State.H:
                        result.Append(binarySymbols[random.Next(binarySymbols.Length)]);
                        currentState = State.A;
                        break;
                    case State.A:
                        int choiceA = random.Next(3);
                        if (choiceA == 0)
                            result.Append(binarySymbols[random.Next(binarySymbols.Length)]);
                        else if (choiceA == 1)
                        {
                            result.Append(operationSymbols[random.Next(operationSymbols.Length)]);
                            currentState = State.B;
                        }
                        else
                        {
                            result.Append(bottomSymbol);
                            currentState = State.S;
                        }
                        break;
                    case State.B:
                        result.Append(binarySymbols[random.Next(binarySymbols.Length)]);
                        currentState = State.A;
                        break;
                    case State.S:
                        return result.ToString();
                }

                if (result.Length > 20)
                {
                    result.Append(bottomSymbol);
                    return result.ToString();
                }
            }
        }
    }
}

