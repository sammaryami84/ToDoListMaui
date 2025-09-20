using CommunityToolkit.Maui.Extensions;
using MainToDoList.Models;
using MainToDoList.Services;
using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.ApplicationModel.DataTransfer;

using System.Collections.ObjectModel;

namespace MainToDoList.Views;

public partial class TaskDetailPage : ContentPage
{
    private readonly ItemModel _task;
    private readonly ObservableCollection<ItemModel> _items;
    private readonly DatabaseService _db = new DatabaseService(AppDatabase.DbPath);

    public TaskDetailPage(ItemModel task, ObservableCollection<ItemModel> items)
    {
        InitializeComponent();
        _task = task;
        _items = items;
        BindingContext = _task;
    }

    // برگشت از صفحه
    private async void OnImageTapped(object sender, EventArgs e)
    {
        if (Navigation.ModalStack.Contains(this))
            await Navigation.PopModalAsync(true);
        else
            await Navigation.PopAsync(true);
    }

    // حذف تسک
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Delete", $"Delete task '{_task.Name}'?", "Yes", "Cancel");
        if (confirm)
        {
            try
            {
                // حذف از دیتابیس
                await _db.DeleteItemAsync(_task);

                // حذف از لیست محلی
                _items.Remove(_task);

                // اطلاع‌رسانی به صفحه‌ی اصلی برای تازه‌سازی
                MessagingCenter.Send(this, "TaskDeleted", _task.Id);

                // بستن صفحه
                if (Navigation.ModalStack.Contains(this))
                    await Navigation.PopModalAsync(true);
                else
                    await Navigation.PopAsync(true);

                Console.WriteLine($"🗑️ Deleted task ID: {_task.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete task: {ex.Message}", "OK");
            }
        }
    }
    private async void OnShareTapped(object sender, EventArgs e)
    {
        if (_task == null) return;

        string message = $"📝 Task: {_task.Name}\n📅 Date: {_task.TaskDate:yyyy/MM/dd}\n⏰ Time: {_task.TaskTime:hh\\:mm}\n📌 Category: {_task.Category}";

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = message,
            Title = "Share Task"
        });
    }
 

    // باز کردن BottomSheet هنگام کلیک روی مداد
    private void OnEditTapped(object sender, EventArgs e)
    {
        var popup = new EditPopup(_task); // ✅ آرگومان داده شده
        this.ShowPopup(popup);
    }



}