using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using MainToDoList.Models;
using MainToDoList.Services;
using MainToDoList.Helpers;

namespace MainToDoList.Views
{
    public partial class AddTaskPage : ContentPage
    {
        private readonly DatabaseService _db = new DatabaseService(AppDatabase.DbPath); // ✅ مسیر یکپارچه

        public ObservableCollection<CategoryModel> Categories { get; } = new()
        {
            new CategoryModel { Name = "Other" },
            new CategoryModel { Name = "Personal" },
            new CategoryModel { Name = "Wishlist" },
            new CategoryModel { Name = "Shopping" },
            new CategoryModel { Name = "Work" }
        };

        public ObservableCollection<SubTaskModel> SubTasks { get; } = new();

        private CategoryModel _selectedCategory;
        public CategoryModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public AddTaskPage()
        {
            InitializeComponent();

            BindingContext = this;
            SelectedCategory = Categories.FirstOrDefault();

            TaskDatePicker.MinimumDate = DateTime.Today;
            TaskTimePicker.Time = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(5));
            TaskDatePicker.DateSelected += OnDateSelected;
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            TaskTimePicker.Time = TaskDatePicker.Date == DateTime.Today
                ? DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(5))
                : new TimeSpan(0, 0, 0);
        }

        private void OnAddSubTaskClicked(object sender, EventArgs e)
        {
            SubTasks.Add(new SubTaskModel
            {
                Name = string.Empty,
                IsCompleted = false,
                Order = SubTasks.Count
            });
        }

        private void OnRemoveSubTaskClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is SubTaskModel sub)
            {
                SubTasks.Remove(sub);
                for (int i = 0; i < SubTasks.Count; i++)
                    SubTasks[i].Order = i;
            }
        }

        private async void OnSaveTaskClicked(object sender, EventArgs e)
        {
            var taskName = TaskEntry.Text?.Trim();
            var taskDate = TaskDatePicker.Date;
            var taskTime = TaskTimePicker.Time;
            var finalCategoryName = SelectedCategory?.Name ?? "All";

            if (string.IsNullOrWhiteSpace(taskName))
            {
                await DisplayAlert("Warning", "Task name cannot be empty.", "OK");
                return;
            }

            if (SubTasks.Any(sub => string.IsNullOrWhiteSpace(sub.Name)))
            {
                await DisplayAlert("Warning", "Sub-task name cannot be empty.", "OK");
                return;
            }

            var selectedDateTime = taskDate.Add(taskTime);
            if (selectedDateTime <= DateTime.Now)
            {
                await DisplayAlert("Invalid Time", "Selected time must be in the future.", "OK");
                return;
            }

            var safeDateTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Utc);
            var scheduledTicks = safeDateTime.Ticks;

            var newTask = new ItemModel
            {
                Name = taskName,
                TaskDate = taskDate,
                TaskTime = taskTime,
                ScheduledAtTicks = scheduledTicks,
                Category = finalCategoryName,
                IsCompleted = false
            };

            try
            {
                await _db.InsertItemAsync(newTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ InsertItemAsync failed: {ex.Message}");
                await DisplayAlert("Error", "Failed to save task. Please try again.", "OK");
                return;
            }

            newTask.SubTasks ??= new ObservableCollection<SubTaskModel>();

            try
            {
                foreach (var sub in SubTasks)
                {
                    sub.ItemId = newTask.Id;
                    await _db.InsertSubTaskAsync(sub);
                    newTask.SubTasks.Add(sub);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ InsertSubTaskAsync failed: {ex.Message}");
                await DisplayAlert("Error", "Failed to save subtasks. Please try again.", "OK");
                return;
            }

            if (ReminderCheckBox?.IsChecked ?? false)
            {
                var dueLocal = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Local);
                var minFuture = DateTime.Now.AddSeconds(5);

                if (dueLocal > minFuture)
                {
                    AlarmScheduler.ScheduleAlarm(newTask.Name, dueLocal);
                }
                else
                {
                    await DisplayAlert("Reminder",
                        "Reminder time is too close or in the past. Task saved but no notification was scheduled.",
                        "OK");
                }
            }

            await Navigation.PopAsync();
        }
        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // یا Shell.Current.GoToAsync("..");
        }
    }
}