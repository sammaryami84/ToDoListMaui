using MainToDoList.Services;
using MainToDoList.ViewModels;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Calendar;
using MainToDoList.Models;
using System;
using System.Linq;

namespace MainToDoList.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;
    private DateTime _lastMonthDisplayed;

    public CalendarPage()
    {
        InitializeComponent();

        var dbService = new DatabaseService(AppDatabase.DbPath); // مسیر یکپارچه

        _viewModel = new CalendarViewModel(dbService);
        BindingContext = _viewModel;

        calendar.DisplayDate = DateTime.Today;
        _lastMonthDisplayed = calendar.DisplayDate;

        // رویداد انتخاب تاریخ برای نمایش تسک‌های روز
        calendar.SelectionChanged += OnSelectionChanged;

        // بررسی تغییر ماه برای به‌روزرسانی لیست روزهای دارای تسک
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            var currentMonth = calendar.DisplayDate;

            if (currentMonth.Month != _lastMonthDisplayed.Month ||
                currentMonth.Year != _lastMonthDisplayed.Year)
            {
                _lastMonthDisplayed = currentMonth;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _viewModel.UpdateMonthTasksAsync(currentMonth);
                });
            }

            return true;
        });

        // مقداردهی اولیه
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await dbService.InitializeAsync(); // ساخت جدول‌ها
            await _viewModel.UpdateMonthTasksAsync(_lastMonthDisplayed); // نمایش تسک‌های ماه جاری
            await _viewModel.FilterTasksByDateAsync(DateTime.Today); // نمایش تسک‌های امروز
        });
    }

    // ✅ وقتی صفحه دوباره ظاهر می‌شه، داده‌ها رو تازه کن
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var selectedDate = calendar.SelectedDate ?? DateTime.Today;
        await _viewModel.UpdateMonthTasksAsync(calendar.DisplayDate);
        await _viewModel.FilterTasksByDateAsync(selectedDate);
    }

    private async void OnCalendarTaskTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is ItemModel item)
        {
            await Navigation.PushModalAsync(new TaskDetailPage(item, _viewModel.TasksForSelectedDay), true);
        }
    }
    private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is ItemModel task)
        {
            task.IsCompleted = e.Value;

            var dbService = new DatabaseService(AppDatabase.DbPath);
            await dbService.UpdateItemAsync(task);
        }
    }
    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }

    private async void OnSelectionChanged(object sender, CalendarSelectionChangedEventArgs e)
    {
        var selectedDate = calendar.SelectedDate;
        if (selectedDate != null)
        {
            await _viewModel.FilterTasksByDateAsync(selectedDate.Value);
        }
    }
}