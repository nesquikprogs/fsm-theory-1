using System;
using System.IO;
using System.Text;

public class JuliaCommentRemover
{
    // Состояния конечного автомата 
    private enum State
    {
        Normal,               // Обычном коде (по умолчанию)
        InSingleLineComment,  // Находимся внутри однострочного комментария (# ...)
        InBlockComment,       // Находимся внутри многострочного комментария (#= ... =#)
        InString,             // Находимся внутри строкового литерала ("..." или """...""")
        InCharLiteral         // Находимся внутри символьного литерала ('a' или '\n')
    }

    // RemoveComments удаляет все комментарии из входного файла и
    // записывает результат в выходной файл
    public static void RemoveComments(string inputFile, string outputFile)
    {
        try
        {
            // Открываем входной файл для чтения и выходной для записи
            using (StreamReader reader = new StreamReader(inputFile))
            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                State currentState = State.Normal;         // Начинаем с обычного состояния
                StringBuilder currentLine = new StringBuilder(); // Накопитель для текущей строки

                string line;
                // Читаем входной файл построчно
                while ((line = reader.ReadLine()) != null)
                {
                    // Посимвольно обрабатываем каждую строку
                    for (int i = 0; i < line.Length; i++)
                    {
                        char currentChar = line[i];  // Текущий символ
                        char nextChar = (i + 1 < line.Length) ? line[i + 1] : '\0'; // Следующий символ

                        // В зависимости от состояния принимаем решение
                        switch (currentState)
                        {
                            case State.Normal:
                                // Если встретили символ # — это начало комментария
                                if (currentChar == '#')
                                {
                                    // Если после # идёт =, значит начался блочный комментарий
                                    if (nextChar == '=')
                                    {
                                        currentState = State.InBlockComment;
                                        i++; // Пропускаем '='
                                    }
                                    else
                                    {
                                        // Иначе — обычный однострочный комментарий
                                        currentState = State.InSingleLineComment;
                                        break; // Игнорируем остаток строки
                                    }
                                }
                                // Если встретили двойную кавычку — начинаем строковый литерал
                                else if (currentChar == '"')
                                {
                                    currentState = State.InString;
                                    currentLine.Append(currentChar);
                                }
                                // Если встретили одинарную кавычку — начинаем символьный литерал
                                else if (currentChar == '\'')
                                {
                                    currentState = State.InCharLiteral;
                                    currentLine.Append(currentChar);
                                }
                                else
                                {
                                    // Обычный символ программы — просто добавляем в результат
                                    currentLine.Append(currentChar);
                                }
                                break;

                            case State.InSingleLineComment:
                                // Однострочный комментарий — пропускаем символы до конца строки
                                break;

                            case State.InBlockComment:
                                // Проверяем, не встретилось ли окончание многострочного комментария (=#)
                                if (currentChar == '=' && nextChar == '#')
                                {
                                    currentState = State.Normal; // Выходим в обычный режим
                                    i++; // Пропускаем '#'
                                }
                                break;

                            case State.InString:
                                // Внутри строки копируем все символы как есть
                                currentLine.Append(currentChar);
                                // Проверяем конец строки (") и учитываем экранирование (\")
                                if (currentChar == '"' && (i == 0 || line[i - 1] != '\\'))
                                {
                                    currentState = State.Normal; // Закрыли строку
                                }
                                break;

                            case State.InCharLiteral:
                                // Внутри символьного литерала тоже копируем символы
                                currentLine.Append(currentChar);
                                // Конец символьного литерала (') с учётом экранирования
                                if (currentChar == '\'' && (i == 0 || line[i - 1] != '\\'))
                                {
                                    currentState = State.Normal; // Закрыли символ
                                }
                                break;
                        }
                    }

                    // В конце строки однострочный комментарий обнуляется
                    if (currentState == State.InSingleLineComment)
                    {
                        currentState = State.Normal;
                    }

                    // Если строка не пустая и мы не внутри блочного комментария —
                    // записываем её в выходной файл
                    if (currentState != State.InBlockComment && currentLine.Length > 0)
                    {
                        writer.WriteLine(currentLine.ToString());
                    }

                    // Очищаем накопитель для следующей строки
                    currentLine.Clear();
                }
            }

            Console.WriteLine("Комментарии успешно удалены. Результат записан в " + outputFile);
        }
        catch (Exception ex)
        {
            // Если произошла ошибка (например, нет файла), выводим сообщение
            Console.WriteLine("Произошла ошибка: " + ex.Message);
        }
    }

    // Точка входа в программу
    public static void Main(string[] args)
    {
        string inputFile = "input.jl";   // Входной файл на Julia
        string outputFile = "output.jl"; // Файл-результат без комментариев

        // Если входного файла ещё нет — создаём тестовый пример
        if (!File.Exists(inputFile))
        {
            File.WriteAllText(inputFile,
                "# Однострочный комментарий\n" +
                "println(\"Hello, world!\") # Комментарий после кода\n\n" +
                "\"\"\"\n" +
                "Многострочный строковый литерал\n" +
                "не должен удаляться\n" +
                "\"\"\"\n\n" +
                "#= Это многострочный комментарий,\n" +
                "который должен быть удален =#\n\n" +
                "x = 1 #= встроенный комментарий =# + 2\n" +
                "c = '\\'' # Символ с экранированием\n");
        }

        // Запускаем основной метод удаления комментариев
        RemoveComments(inputFile, outputFile);
    }
}
