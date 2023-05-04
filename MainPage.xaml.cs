using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



namespace FifteenPuzzleUWP
{
    public sealed partial class MainPage : Page
    {
        private readonly int gridSize = 4;
        private Button[,] buttons;
        private int emptyRow, emptyColumn;
        private Stack<Tuple<int, int, int, int>> moveHistory;

        public MainPage()
        {
            this.InitializeComponent();
            buttons = new Button[gridSize, gridSize];
            moveHistory = new Stack<Tuple<int, int, int, int>>();
            InitializeGame();
            ShuffleGame();
        }

        private void InitializeGame()
        {
            for (int i = 0; i < gridSize; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int j = 0; j < gridSize; j++)
                {
                    int number = i * gridSize + j + 1;
                    if (number < gridSize * gridSize)
                    {
                        Button button = new Button
                        {
                            Content = number.ToString(),
                            FontSize = 24,
                            Width = 100,
                            Height = 100
                        };
                        button.Click += Button_Click;
                        Grid.SetRow(button, i);
                        Grid.SetColumn(button, j);
                        GameGrid.Children.Add(button);
                        buttons[i, j] = button;
                    }
                    else
                    {
                        emptyRow = i;
                        emptyColumn = j;
                    }
                }
            }
        }

        private void ShuffleGame()
        {
            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                int rowOffset = random.Next(-1, 2);
                int newRow = emptyRow + rowOffset;
                int newColumn = emptyColumn + (1 - Math.Abs(rowOffset));

                if (IsValidPosition(newRow, newColumn))
                {
                    SwapButtons(emptyRow, emptyColumn, newRow, newColumn);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int row = Grid.GetRow(clickedButton);
            int column = Grid.GetColumn(clickedButton);

            if (IsValidMove(row, column))
            {
                moveHistory.Push(Tuple.Create(row, column, emptyRow, emptyColumn));
                SwapButtons(row, column, emptyRow, emptyColumn);

                if (IsGameWon())
                {
                    ShowWinMessage();
                }
            }
        }

        private void SwapButtons(int row1, int column1, int row2, int column2)
        {
            buttons[row1, column1] = buttons[row2, column2];
            buttons[row2, column2] = null;
            Grid.SetRow(buttons[row1, column1], row1);
            Grid.SetColumn(buttons[row1, column1], column1);

            emptyRow = row2;
            emptyColumn = column2;
        }

        private bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < gridSize && column >= 0 && column < gridSize;
        }

        private bool IsValidMove(int row, int column)
        {
            return Math.Abs(emptyRow - row) + Math.Abs(emptyColumn - column) == 1;
        }

        private bool IsGameWon()
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (buttons[i, j] != null)
                    {
                        int correctNumber = i * gridSize + j + 1;
                        if (correctNumber < gridSize * gridSize && buttons[i, j].Content.ToString() != correctNumber.ToString())
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private async void ShowWinMessage()
        {
            ContentDialog winDialog = new ContentDialog()
            {
                Title = "Переможець",
                Content = "Ви виграли гру!",
                CloseButtonText = "ОК"
            };

            await winDialog.ShowAsync();
            InitializeGame();
            ShuffleGame();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (moveHistory.Any())
            {
                var move = moveHistory.Pop();
                SwapButtons(move.Item1, move.Item2, move.Item3, move.Item4);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            ShuffleGame();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Збереження результатів тут. Зауважте, що ви можете використовувати різні методи збереження,
            // такі як локальне збереження, збереження у хмарі тощо. Нижче наведений приклад збереження
            // результатів гри у текстовий файл на локальному диску.

            SaveResultsToFile();
        }

        private async void SaveResultsToFile()
        {
            // Імпортуйте простір імен Windows.Storage та Windows.Storage.Pickers
            // до файлу MainPage.xaml.cs

            Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            savePicker.FileTypeChoices.Add("Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "FifteenPuzzleResults";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                string content = GenerateResultsString();
                await Windows.Storage.FileIO.WriteTextAsync(file, content);
            }
        }

        private string GenerateResultsString()
        {
            string content = "Результати гри \"П'ятнашки\":\n\n";

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    content += buttons[i, j]?.Content?.ToString() ?? " ";
                    content += "\t";
                }
                content += "\n";
            }

            return content;
        }
    }
}


