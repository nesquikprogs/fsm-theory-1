// Program.cs
using System;
using System.Text;

namespace TA_Lab1_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Устанавливаем кодировку UTF-8 для корректного отображения символов
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Лабораторная работа: Теория автоматов");
            Console.WriteLine("Тема: Преобразование праволинейной грамматики в леволинейную\n");

            // Меню выбора грамматики
            Console.WriteLine("Выберите грамматику:");
            Console.WriteLine("1 - Первая грамматика (S -> 0S | 0B ...)");
            Console.WriteLine("2 - Вторая грамматика (S -> aA | aB | ba ...)");
            Console.Write("Ваш выбор: ");
            string choice = Console.ReadLine();

            string[] rules;

            if (choice == "2")
            {
                // Вторая грамматика
                rules = new string[]
                {
                    "S -> aA | aB | ba",
                    "A -> bs",
                    "B -> aS | bB | ⊥"
                };
            }
            else
            {
                // Первая грамматика по умолчанию
                rules = new string[]
                {
                    "S -> 0S | 0B",
                    "B -> 1B | 1C",
                    "C -> 1C | ⊥"
                };
            }

            Console.WriteLine("\nВыбранная праволинейная грамматика:(нетерминал (если есть) всегда стоит справа от терминала в правой части правила)");
            foreach (var rule in rules)
            {
                Console.WriteLine(rule);
            }

            // Строка для хранения леволинейной грамматики
            StringBuilder leftLinearGrammar = new StringBuilder();

            try
            {
                // Обрабатываем каждое правило грамматики
                foreach (string rule in rules)
                {
                    string trimmedRule = rule.Trim();

                    // Проверяем наличие символа "->" для разделения левой и правой части
                    if (trimmedRule.Contains("->"))
                    {
                        string[] parts = trimmedRule.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            string lhs = parts[0].Trim(); // Левый нетерминал
                            string rhs = parts[1].Trim(); // Правая часть с альтернативами

                            // Разделяем альтернативы через "|"
                            string[] alternatives = rhs.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string alternative in alternatives)
                            {
                                string trimmedAlternative = alternative.Trim();
                                if (trimmedAlternative.Length > 0)
                                {
                                    // Если правило имеет терминал + нетерминал справа (праволинейное)
                                    if (trimmedAlternative.Length > 1 && trimmedAlternative != "⊥")
                                    {
                                        char terminal = trimmedAlternative[0];          // первый символ — терминал
                                        string nonTerminal = trimmedAlternative.Substring(1); // оставшаяся часть — нетерминал

                                        // Формируем леволинейное правило: нетерминал -> терминал + исходный нетерминал
                                        leftLinearGrammar.AppendLine($"{nonTerminal} -> {terminal}{lhs}");
                                    }
                                    else
                                    {
                                        // Только терминал или символ конца строки
                                        leftLinearGrammar.AppendLine($"{lhs} -> {trimmedAlternative}");
                                    }
                                }
                            }
                        }
                    }
                }

                // Вывод результата
                Console.WriteLine("\nЭквивалентная леволинейная грамматика:(нетерминал слева, терминал + исходный левый нетерминал справа)");
                Console.WriteLine(leftLinearGrammar.ToString());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке грамматики: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
