using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace TuringMachineWPF
{
    /// <summary>
    /// Главный класс окна WPF-приложения для визуализации машины Тьюринга
    /// </summary>
    public partial class MainWindow : Window
    {
        private TuringMachine tm; // Экземпляр машины Тьюринга
        private DispatcherTimer animationTimer; // Таймер для анимации автоматического выполнения шагов
        private bool isFirstStep = true; // Флаг первого шага для корректного отображения
        private bool isLastStep = false; // Флаг окончания вычислений

        /// <summary>
        /// Конструктор главного окна
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Настраиваем таймер анимации: каждый шаг выполняется через 500 мс
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(500);
            animationTimer.Tick += AnimationTimer_Tick;
        }

        /// <summary>
        /// Обработчик кнопки "Анимация"
        /// Запускает автоматическое выполнение машины Тьюринга
        /// </summary>
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text.Trim();

            // Проверка корректности введённого числа
            if (input == "0")
            {
                MessageBox.Show("Ошибка: введите число больше 0");
                return;
            }

            if (input.Length == 0 || !input.All(char.IsDigit) || (input.Length > 1 && input[0] == '0'))
            {
                MessageBox.Show("Ошибка: введите корректное натуральное число без ведущих нулей");
                return;
            }

            // Создаём машину Тьюринга с введённым числом
            tm = new TuringMachine(input);
            isFirstStep = true;
            isLastStep = false;

            UpdateVisualization(); // Обновляем визуализацию начального состояния
            animationTimer.Start(); // Запускаем анимацию
        }

        /// <summary>
        /// Обработчик кнопки "Шаг"
        /// Выполняет один шаг машины Тьюринга вручную
        /// </summary>
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            // Если машина ещё не создана, создаём её
            if (tm == null)
            {
                string input = InputTextBox.Text.Trim();

                if (input == "0")
                {
                    MessageBox.Show("Ошибка: введите число больше 0");
                    return;
                }

                if (input.Length == 0 || !input.All(char.IsDigit) || (input.Length > 1 && input[0] == '0'))
                {
                    MessageBox.Show("Ошибка: введите корректное натуральное число без ведущих нулей");
                    return;
                }

                tm = new TuringMachine(input);
                isFirstStep = true;
                isLastStep = false;
                UpdateVisualization();
                return;
            }

            // Если машина ещё не дошла до конечного состояния
            if (tm.CurrentState != "q_final")
            {
                if (isFirstStep)
                {
                    // На первом шаге просто отображаем начальное состояние
                    isFirstStep = false;
                }
                else
                {
                    tm.Step(); // Выполняем один шаг машины Тьюринга
                    isLastStep = (tm.CurrentState == "q_final"); // Проверяем, достигли ли конца
                }
                UpdateVisualization(); // Обновляем визуализацию после шага
            }
            else
            {
                // Если машина уже завершила работу
                MessageBox.Show("Вычисление завершено!");
            }
        }

        /// <summary>
        /// Таймер для автоматической анимации
        /// Каждый тик выполняет один шаг машины Тьюринга
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (tm.CurrentState == "q_final")
            {
                // Если машина достигла конечного состояния — останавливаем таймер
                animationTimer.Stop();
                isLastStep = true;
                UpdateVisualization();
                ResultText.Text = $"Результат: {tm.GetResult()}"; // Показываем результат
                return;
            }

            if (isFirstStep)
            {
                isFirstStep = false; // Первый шаг — просто отображение начального состояния
            }
            else
            {
                tm.Step(); // Выполняем шаг машины
                isLastStep = (tm.CurrentState == "q_final");
            }
            UpdateVisualization(); // Обновляем визуализацию
        }

        /// <summary>
        /// Обновление визуализации ленты и информации о машине
        /// </summary>
        private void UpdateVisualization()
        {
            if (tm == null) return;

            // Получаем ленту с подсветкой текущей позиции головки
            var tapeWithHighlight = tm.GetTapeWithHighlight(isLastStep);
            TapeItemsControl.ItemsSource = tapeWithHighlight;

            // Определяем текущий символ под головкой
            var highlightedCell = tapeWithHighlight.FirstOrDefault(c => c.Background == Brushes.LightBlue);
            char currentSymbol = highlightedCell?.Symbol ?? ' ';

            // Отображаем текущее состояние и позицию головки
            StatusText.Text = $"Текущее состояние: {tm.CurrentState} | Позиция головки: {tm.HeadPosition}";

            if (isFirstStep)
            {
                // На первом шаге показываем начальное состояние и возможные переходы
                TransitionText.Text = "Начальное состояние q_start";
                NextStatesText.Text = tm.GetPossibleTransitions();
            }
            else if (isLastStep)
            {
                // В финальном состоянии показываем сообщение о завершении
                TransitionText.Text = "Финальное состояние q_final";
                NextStatesText.Text = "Нет возможных переходов";
            }
            else
            {
                // Показываем последний переход машины и текущий символ
                TransitionText.Text = $"Текущий символ: '{currentSymbol}' | " +
                                    $"Последний переход: {tm.LastTransition?.State ?? "-"}, " +
                                    $"'{tm.LastTransition?.Symbol ?? '-'}' → " +
                                    $"'{tm.LastTransition?.NewSymbol ?? '-'}', " +
                                    $"{tm.LastTransition?.Move ?? 0}, " +
                                    $"{tm.LastTransition?.NewState ?? "-"}";

                // Показываем все возможные переходы из текущего состояния
                NextStatesText.Text = tm.GetPossibleTransitions();
            }
        }
    }

    /// <summary>
    /// Класс для визуального отображения одной ячейки ленты
    /// </summary>
    public class TapeCell
    {
        public char Symbol { get; set; } // Символ в ячейке
        public Brush Background { get; set; } = Brushes.White; // Цвет фона ячейки
    }

    /// <summary>
    /// Информация о последнем переходе машины Тьюринга
    /// </summary>
    public class TransitionInfo
    {
        public string State { get; set; } // Текущее состояние
        public char Symbol { get; set; } // Символ, который прочитала головка
        public char NewSymbol { get; set; } // Новый символ, который записывается
        public int Move { get; set; } // Движение головки: -1 влево, 0 на месте, 1 вправо
        public string NewState { get; set; } // Новое состояние после перехода
    }

    /// <summary>
    /// Класс, реализующий машину Тьюринга для вычитания единицы
    /// </summary>
    public class TuringMachine
    {
        private List<char> tape; // Лента машины
        private int headPosition; // Позиция головки на ленте
        private string currentState; // Текущее состояние
        private Dictionary<(string, char), (char, int, string)> transitionTable; // Таблица переходов
        public TransitionInfo LastTransition { get; private set; } // Последний выполненный переход

        public string CurrentState => currentState;
        public int HeadPosition => headPosition;
        public char CurrentSymbol => headPosition >= 0 && headPosition < tape.Count ? tape[headPosition] : ' ';

        /// <summary>
        /// Конструктор машины Тьюринга
        /// </summary>
        public TuringMachine(string input)
        {
            // Создаём ленту: пробел + число + пробел
            tape = new List<char>((" " + input + " ").ToCharArray());
            headPosition = 0; // Начинаем с пробела перед числом
            currentState = "q_start";

            // Определяем таблицу переходов для алгоритма "вычесть 1"
            transitionTable = new Dictionary<(string, char), (char, int, string)>
            {
                // Начальное состояние: идём вправо до конца числа
                {("q_start", ' '), (' ', 1, "q_find_end")},

                // Состояние поиска конца числа
                {("q_find_end", '0'), ('0', 1, "q_find_end")},
                {("q_find_end", '1'), ('1', 1, "q_find_end")},
                {("q_find_end", '2'), ('2', 1, "q_find_end")},
                {("q_find_end", '3'), ('3', 1, "q_find_end")},
                {("q_find_end", '4'), ('4', 1, "q_find_end")},
                {("q_find_end", '5'), ('5', 1, "q_find_end")},
                {("q_find_end", '6'), ('6', 1, "q_find_end")},
                {("q_find_end", '7'), ('7', 1, "q_find_end")},
                {("q_find_end", '8'), ('8', 1, "q_find_end")},
                {("q_find_end", '9'), ('9', 1, "q_find_end")},
                {("q_find_end", ' '), (' ', -1, "q_subtract")}, // Конец числа → начинаем вычитание

                // Состояние вычитания единицы
                {("q_subtract", '0'), ('9', -1, "q_subtract")}, // Перенос разряда
                {("q_subtract", '1'), ('0', 0, "q_final")},
                {("q_subtract", '2'), ('1', 0, "q_final")},
                {("q_subtract", '3'), ('2', 0, "q_final")},
                {("q_subtract", '4'), ('3', 0, "q_final")},
                {("q_subtract", '5'), ('4', 0, "q_final")},
                {("q_subtract", '6'), ('5', 0, "q_final")},
                {("q_subtract", '7'), ('6', 0, "q_final")},
                {("q_subtract", '8'), ('7', 0, "q_final")},
                {("q_subtract", '9'), ('8', 0, "q_final")},
                {("q_subtract", ' '), (' ', 0, "q_error")} // Число было 0 → ошибка
            };
        }

        /// <summary>
        /// Выполняет один шаг машины Тьюринга
        /// </summary>
        public void Step()
        {
            if (currentState == "q_final" || currentState == "q_error") return;

            // Если головка вышла за границы ленты, расширяем ленту пробелами
            if (headPosition < 0)
            {
                tape.Insert(0, ' ');
                headPosition = 0;
            }
            else if (headPosition >= tape.Count)
            {
                tape.Add(' ');
            }

            char currentSymbol = tape[headPosition];

            // Выполняем переход согласно таблице переходов
            if (transitionTable.TryGetValue((currentState, currentSymbol), out var transition))
            {
                LastTransition = new TransitionInfo
                {
                    State = currentState,
                    Symbol = currentSymbol,
                    NewSymbol = transition.Item1,
                    Move = transition.Item2,
                    NewState = transition.Item3
                };

                // Обновляем символ на ленте, положение головки и состояние машины
                tape[headPosition] = transition.Item1;
                headPosition += transition.Item2;
                currentState = transition.Item3;
            }
            else
            {
                throw new InvalidOperationException($"Неопределенный переход: состояние {currentState}, символ {currentSymbol}");
            }
        }

        /// <summary>
        /// Выполняет машину полностью до конечного состояния
        /// </summary>
        public void Run()
        {
            while (currentState != "q_final" && currentState != "q_error")
            {
                Step();
            }
        }

        /// <summary>
        /// Получение результата работы машины
        /// </summary>
        public string GetResult()
        {
            if (currentState == "q_error")
            {
                return "Ошибка: введен ноль"; // Если число было 0
            }

            // Формируем строку из ленты и убираем пробелы и ведущие нули
            string result = new string(tape.ToArray()).Trim();
            result = result.TrimStart('0');
            return string.IsNullOrEmpty(result) ? "0" : result;
        }

        /// <summary>
        /// Получение визуальной ленты с подсветкой текущей позиции головки
        /// </summary>
        public List<TapeCell> GetTapeWithHighlight(bool isFinalState = false)
        {
            var tapeCells = new List<TapeCell>();
            for (int i = 0; i < tape.Count; i++)
            {
                bool highlight = isFinalState ? (i == headPosition && LastTransition != null) : (i == headPosition);

                tapeCells.Add(new TapeCell
                {
                    Symbol = tape[i],
                    Background = highlight ? Brushes.LightBlue : Brushes.White
                });
            }
            return tapeCells;
        }

        /// <summary>
        /// Получение всех возможных переходов из текущего состояния
        /// </summary>
        public string GetPossibleTransitions()
        {
            var possibleTransitions = transitionTable
                .Where(t => t.Key.Item1 == currentState)
                .Select(t => $"Если символ '{t.Key.Item2}': → '{t.Value.Item1}', {GetMoveDescription(t.Value.Item2)}, {t.Value.Item3}")
                .ToList();

            return possibleTransitions.Any()
                ? "Возможные переходы:\n" + string.Join("\n", possibleTransitions)
                : "Нет возможных переходов";
        }

        /// <summary>
        /// Перевод направления движения головки в текст
        /// </summary>
        private string GetMoveDescription(int move)
        {
            return move switch
            {
                -1 => "влево",
                0 => "на месте",
                1 => "вправо",
                _ => move.ToString()
            };
        }
    }
}
