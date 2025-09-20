using CommunityToolkit.Maui.Views;
using MainToDoList.Models;
using MainToDoList.Services;
using System.Collections.ObjectModel;

namespace MainToDoList.Views;

public partial class EditPopup : Popup
{
    private readonly ItemModel _task;
    private readonly DatabaseService _db;

    public ObservableCollection<CategoryModel> Categories { get; set; }
    public CategoryModel SelectedCategoryModel { get; set; }

    public EditPopup(ItemModel task)
    {
        InitializeComponent();
        _task = task;
        _db = new DatabaseService(AppDatabase.DbPath);

        Categories = new ObservableCollection<CategoryModel>
        {
            new CategoryModel { Name = "other" },
            new CategoryModel { Name = "Personal" },
            new CategoryModel { Name = "Wishlist" },
            new CategoryModel { Name = "Shopping" },
            new CategoryModel { Name = "Work" }
        };

        SelectedCategoryModel = Categories.FirstOrDefault(c => c.Name == _task.Category);
        BindingContext = this;

        // مقداردهی اولیه کنترل‌هایی که Binding ندارن
        TaskEntry.Text = _task.Name;
        TaskDatePicker.Date = _task.TaskDate;
        TaskTimePicker.Time = _task.TaskTime;
        ReminderCheckBox.IsChecked = _task.IsCompleted;

        // اجرای انیمیشن هنگام باز شدن
        this.Opened += OnPopupOpened;
    }

    private async void OnPopupOpened(object sender, EventArgs e)
    {
        PopupContent.Opacity = 0;
        PopupContent.Scale = 0.8;
        PopupContent.TranslationY = 100;

        // مرحله اول: ورود از پایین و ظاهر شدن
        await Task.WhenAll(
            PopupContent.FadeTo(1, 300, Easing.CubicInOut),
            PopupContent.ScaleTo(1.05, 300, Easing.CubicOut),
            PopupContent.TranslateTo(0, 0, 300, Easing.CubicOut)
        );

        // مرحله دوم: برگشت به اندازه‌ی نهایی
        await PopupContent.ScaleTo(1.0, 150, Easing.CubicIn);

        // مرحله سوم: لرزش نرم افقی
        await PopupContent.TranslateTo(-10, 0, 50);
        await PopupContent.TranslateTo(10, 0, 50);
        await PopupContent.TranslateTo(0, 0, 50);
    } 

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _task.Name = TaskEntry.Text;
        _task.TaskDate = TaskDatePicker.Date;
        _task.TaskTime = TaskTimePicker.Time;
        _task.IsCompleted = ReminderCheckBox.IsChecked;
        _task.Category = SelectedCategoryModel?.Name ?? _task.Category;

        await _db.UpdateItemAsync(_task);
        await CloseAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}