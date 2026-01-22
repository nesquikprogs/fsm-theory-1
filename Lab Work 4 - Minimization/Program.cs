using System;
using System.Collections.Generic;
using System.Linq;

public class FiniteAutomaton
{
    public HashSet<string> States { get; set; } // Хранит все состояния автомата, типа A, S, L и т.д.
    public HashSet<string> Alphabet { get; set; } // Хранит алфавит, то есть символы вроде 0, -, c
    public Dictionary<(string, string), string> Transitions { get; set; } // Список переходов, кто куда идёт по какому символу
    public string InitialState { get; set; } // Начальное состояние, с которого всё начинается
    public HashSet<string> FinalStates { get; set; } // Список финальных состояний, куда надо прийти

    public FiniteAutomaton()
    {
        States = new HashSet<string>(); // Создаём пустой набор состояний
        Alphabet = new HashSet<string>(); // Создаём пустой набор символов
        Transitions = new Dictionary<(string, string), string>(); // Создаём пустой словарь переходов
        FinalStates = new HashSet<string>(); // Создаём пустой набор финальных состояний
    }

    public void AddTransition(string fromState, string symbol, string toState)
    {
        // Добавляем переход: откуда (fromState), по какому символу (symbol), куда (toState)
        States.Add(fromState); // Добавляем начальное состояние в набор
        States.Add(toState); // Добавляем конечное состояние в набор
        Alphabet.Add(symbol); // Добавляем символ в алфавит
        Transitions[(fromState, symbol)] = toState; // Записываем переход в словарь
    }

    public void SetFinalState(string state)
    {
        // Помечаем состояние как финальное
        FinalStates.Add(state); // Добавляем состояние в набор финальных
    }

    public void SetInitialState(string state)
    {
        // Устанавливаем начальное состояние
        InitialState = state; // Записываем его
        States.Add(state); // Добавляем в набор состояний
    }

    public FiniteAutomaton Minimize()
    {
        // Убираем состояния, до которых нельзя добраться
        var reachableStates = FindReachableStates(); // Находим все доступные состояния от начального
        var unreachableStates = States.Except(reachableStates).ToList(); // Выбираем недоступные

        foreach (var state in unreachableStates)
        {
            States.Remove(state); // Удаляем недоступное состояние
            FinalStates.Remove(state); // Удаляем его из финальных, если было

            var transitionsToRemove = Transitions
                .Where(t => t.Key.Item1 == state || t.Value == state) // Находим все переходы с этим состоянием
                .Select(t => t.Key)
                .ToList();

            foreach (var key in transitionsToRemove)
            {
                Transitions.Remove(key); // Удаляем эти переходы
            }
        }

        // Делим состояния на финальные и не финальные
        var partition = new List<HashSet<string>>();
        var nonFinalStates = new HashSet<string>(States.Except(FinalStates)); // Все, кроме финальных

        if (nonFinalStates.Count > 0)
            partition.Add(new HashSet<string>(nonFinalStates)); // Добавляем не финальные
        if (FinalStates.Count > 0)
            partition.Add(new HashSet<string>(FinalStates)); // Добавляем финальные

        // Разбиваем группы(эквивалентные состояния), пока можно
        bool changed;
        do
        {
            changed = false;
            var newPartition = new List<HashSet<string>>(); // Новая версия групп

            foreach (var group in partition)
            {
                if (group.Count == 1) // Если группа из одного состояния, оставляем как есть
                {
                    newPartition.Add(new HashSet<string>(group));
                    continue;
                }

                var splitGroups = new Dictionary<string, HashSet<string>>(); // Для новых подгрупп

                foreach (var state in group)
                {
                    var signature = new System.Text.StringBuilder(); // Строим "подпись" состояния

                    foreach (var symbol in Alphabet.OrderBy(s => s)) // Для каждого символа алфавита
                    {
                        if (Transitions.TryGetValue((state, symbol), out var targetState))
                        {
                            var targetGroupIndex = partition.FindIndex(g => g.Contains(targetState)); // Куда ведёт переход
                            signature.Append(targetGroupIndex).Append(","); // Добавляем индекс группы
                        }
                        else
                        {
                            signature.Append("-,"); // Если перехода нет, ставим "-"
                        }
                    }

                    var sigStr = signature.ToString(); // Полная подпись
                    if (!splitGroups.ContainsKey(sigStr))
                        splitGroups[sigStr] = new HashSet<string>(); // Создаём новую группу, если подпись новая

                    splitGroups[sigStr].Add(state); // Добавляем состояние в группу с такой подписью
                }

                if (splitGroups.Count > 1)
                    changed = true; // Если группа разделилась, отмечаем изменение

                newPartition.AddRange(splitGroups.Values); // Добавляем все подгруппы
            }

            partition = newPartition; // Обновляем группы
        } while (changed); // Повторяем, пока есть изменения

        // Создаём новый минимизированный автомат по одному представителю из группы
        var minimized = new FiniteAutomaton();
        minimized.Alphabet = new HashSet<string>(Alphabet); // Копируем алфавит

        var classRepresentatives = partition.Select(g => g.OrderBy(s => s).First()).ToList(); // Берем по одному представителю из каждой группы

        var stateToRepresentative = new Dictionary<string, string>(); // Связь старых состояний с новыми
        foreach (var group in partition)
        {
            var representative = group.OrderBy(s => s).First(); // Берем первого по алфавиту
            foreach (var state in group)
            {
                stateToRepresentative[state] = representative; // Записываем замену
            }
        }

        // Добавляем состояния в новый автомат
        foreach (var rep in classRepresentatives)
        {
            minimized.States.Add(rep); // Добавляем представителя

            if (partition.First(g => g.Contains(rep)).Contains(InitialState))
                minimized.InitialState = rep; // Устанавливаем начальное, если оно было в группе

            if (partition.First(g => g.Contains(rep)).Any(s => FinalStates.Contains(s)))
                minimized.FinalStates.Add(rep); // Устанавливаем финальное, если было в группе
        }

        // Добавляем переходы
        foreach (var group in partition)
        {
            var fromRep = group.OrderBy(s => s).First(); // Берем представителя группы

            foreach (var symbol in minimized.Alphabet)
            {
                if (Transitions.TryGetValue((fromRep, symbol), out var originalTarget))
                {
                    var toRep = stateToRepresentative[originalTarget]; // Находим представителя целевого состояния
                    minimized.Transitions[(fromRep, symbol)] = toRep; // Добавляем новый переход
                }
            }
        }

        return minimized; // Возвращаем минимизированный автомат
    }

    private HashSet<string> FindReachableStates()
    {
        var reachable = new HashSet<string>(); // Множество достижимых состояний
        if (!States.Contains(InitialState))
            return reachable; // Если начального нет, возвращаем пустое

        var queue = new Queue<string>(); // Очередь для обхода
        queue.Enqueue(InitialState); // Начинаем с начального
        reachable.Add(InitialState); // Добавляем его

        while (queue.Count > 0)
        {
            var current = queue.Dequeue(); // Берем текущее состояние

            foreach (var symbol in Alphabet) // Для каждого символа
            {
                if (Transitions.TryGetValue((current, symbol), out var target) && // Если есть переход
                    !reachable.Contains(target)) // И его ещё нет в достижимых
                {
                    reachable.Add(target); // Добавляем целевое состояние
                    queue.Enqueue(target); // Ставим в очередь для дальнейшего обхода
                }
            }
        }

        return reachable; // Возвращаем все достижимые состояния
    }

    public void PrintAutomat(string title)
    {
        // Выводим информацию об автомате с красивым заголовком
        Console.WriteLine($"=== {title} ===");
        Console.WriteLine($"Состояния: {string.Join(", ", States.OrderBy(s => s))}");
        Console.WriteLine($"Алфавит: {string.Join(", ", Alphabet.OrderBy(a => a))}");
        Console.WriteLine($"Начальное состояние: {InitialState}");
        Console.WriteLine($"Конечное состояние: {string.Join(", ", FinalStates.OrderBy(f => f))}");
        Console.WriteLine("Переходы:");

        foreach (var transition in Transitions.OrderBy(t => t.Key.Item1).ThenBy(t => t.Key.Item2))
        {
            Console.WriteLine($"  {transition.Key.Item1} --{transition.Key.Item2}--> {transition.Value}");
        }

        Console.WriteLine();
    }
}

public class Program
{
    public static void Main()
    {
        var automat = new FiniteAutomaton(); // Создаём новый автомат

        automat.SetInitialState("S"); // Устанавливаем начальное состояние

        automat.SetFinalState("L"); // Устанавливаем финальные состояния
        automat.SetFinalState("R");

        // Добавляем все переходы, которые были в задании
        automat.AddTransition("S", "0", "D");
        automat.AddTransition("S", "-", "A");
        automat.AddTransition("A", "c", "J");
        automat.AddTransition("J", "n", "H");
        automat.AddTransition("D", "c", "F");
        automat.AddTransition("F", "n", "G");
        automat.AddTransition("H", "1", "R");
        automat.AddTransition("G", "1", "L");
        automat.AddTransition("L", "c", "H");
        automat.AddTransition("R", "c", "G");
        automat.AddTransition("L", "-", "R");
        automat.AddTransition("R", "-", "L");
        automat.AddTransition("M", "c", "R");
        automat.AddTransition("M", "n", "L");

        // Показываем, как выглядит автомат до минимизации
        automat.PrintAutomat("Оригинальный автомат");

        // Минимизируем автомат
        var minimized = automat.Minimize();

        // Показываем результат после минимизации
        minimized.PrintAutomat("Минимизированный автомат");
    }
}