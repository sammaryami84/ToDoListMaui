using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MainToDoList.Models;
using MainToDoList.Services;

namespace MainToDoList.ViewModels
{
    public class CalendarViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;

        public ObservableCollection<int> TaskDaysInMonth { get; set; } = new();
        public ObservableCollection<ItemModel> TasksForSelectedDay { get; set; } = new();

        private string _monthTitle;
        public string MonthTitle
        {
            get => _monthTitle;
            set => SetField(ref _monthTitle, value);
        }

        private string _noTaskMessage;
        public string NoTaskMessage
        {
            get => _noTaskMessage;
            set => SetField(ref _noTaskMessage, value);
        }

        private bool _hasTasksInMonth;
        public bool HasTasksInMonth
        {
            get => _hasTasksInMonth;
            set => SetField(ref _hasTasksInMonth, value);
        }

        private bool _hasTasksForDay;
        public bool HasTasksForDay
        {
            get => _hasTasksForDay;
            set => SetField(ref _hasTasksForDay, value);
        }

        private string _noTasksForDayMessage;
        public string NoTasksForDayMessage
        {
            get => _noTasksForDayMessage;
            set => SetField(ref _noTasksForDayMessage, value);
        }

        public string TaskDaysInMonthString => TaskDaysInMonth.Count > 0
            ? string.Join(", ", TaskDaysInMonth)
            : string.Empty;

        public CalendarViewModel(DatabaseService dbService)
        {
            _db = dbService;
        }

        public async Task UpdateMonthTasksAsync(DateTime referenceDate)
        {
            TaskDaysInMonth.Clear();

            MonthTitle = referenceDate.ToString("MMMM yyyy", new CultureInfo("en-US"));

            var allItems = await _db.GetItemsAsync();

            var tasksInMonth = allItems
                .Where(item => item.TaskDate.Month == referenceDate.Month &&
                               item.TaskDate.Year == referenceDate.Year)
                .Select(item => item.TaskDate.Day)
                .Distinct()
                .OrderBy(day => day)
                .ToList();

            if (tasksInMonth.Any())
            {
                HasTasksInMonth = true;
                NoTaskMessage = string.Empty;
                foreach (var day in tasksInMonth)
                    TaskDaysInMonth.Add(day);
            }
            else
            {
                HasTasksInMonth = false;
                NoTaskMessage = $"No tasks available in {MonthTitle}.";
            }

            OnPropertyChanged(nameof(TaskDaysInMonthString));
        }

        public async Task FilterTasksByDateAsync(DateTime selectedDate)
        {
            TasksForSelectedDay.Clear();

            var allItems = await _db.GetItemsWithSubTasksAsync();

            var tasks = allItems
                .Where(item => item.TaskDate.Date == selectedDate.Date)
                .ToList();

            if (tasks.Any())
            {
                HasTasksForDay = true;
                NoTasksForDayMessage = string.Empty;

                foreach (var task in tasks)
                {
                    task.CategoryIconText = task.Category switch
                    {
                        "Personal" => "👤",
                        "Wishlist" => "🎁",
                        "Shopping" => "🛒",
                        "Work" => "💼",
                        _ => "❓"
                    };

                    TasksForSelectedDay.Add(task);
                }
            }
            else
            {
                HasTasksForDay = false;
                NoTasksForDayMessage = "No tasks available for this day.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}