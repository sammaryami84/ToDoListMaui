using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using MainToDoList.Models;
using MainToDoList.Services;
using MainToDoList.Helpers;

namespace MainToDoList.Views;

public partial class ItemListView : ContentView
{
    public ObservableCollection<ItemModel> Items { get; set; } = new();
    public ObservableCollection<ItemModel> FilteredItems { get; set; } = new();
    public ObservableCollection<string> Categories { get; set; } = new() { "All", "Personal", "Wishlist", "Shopping", "Work" };
    public string SelectedCategory { get; set; } = "All";

    private readonly DatabaseService _db = new DatabaseService(AppDatabase.DbPath);
    private bool _isFiltering = false;

    public ItemListView()
    {
        InitializeComponent();
        BindingContext = this;
        ItemsView.ItemsSource = FilteredItems;
    }

    public async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        Items.Clear();

        try
        {
            var fetchedItems = await _db.GetItemsWithSubTasksAsync();

            if (fetchedItems?.Count > 0)
            {
                foreach (var item in fetchedItems)
                {
                    item.CategoryIconText = item.Category switch
                    {
                        "Personal" => "👤",
                        "Wishlist" => "🎁",
                        "Shopping" => "🛒",
                        "Work" => "💼",
                        _ => "❓"
                    };

                    Items.Add(item);
                }

                Debug.WriteLine($"✅ Loaded {Items.Count} tasks from local DB");
            }
            else
            {
                Debug.WriteLine("⚠️ No items in local DB.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ DB load error: {ex.Message}");
        }

        FilterTasks();
    }

    public void FilterTasks()
    {
        _isFiltering = true;

        Dispatcher.Dispatch(() =>
        {
            FilteredItems.Clear();

            var filtered = SelectedCategory == "All"
                ? Items
                : Items.Where(i => i.Category == SelectedCategory);

            foreach (var item in filtered)
                FilteredItems.Add(item);

            bool isAllEmpty = SelectedCategory == "All" && Items.Count == 0;
            bool isCategoryEmpty = SelectedCategory != "All" && !FilteredItems.Any();

            EmptyStateView.IsVisible = isAllEmpty || isCategoryEmpty;
            ItemsView.IsVisible = !EmptyStateView.IsVisible;

            if (isAllEmpty)
            {
                EmptyStateLabel.Text = "No tasks available. You can add one to get started.";
                ClickHint.IsVisible = true;
                AddButtonFrame.BorderColor = Color.FromArgb("#FF894F");
            }
            else if (isCategoryEmpty)
            {
                EmptyStateLabel.Text = $"No tasks found in '{SelectedCategory}' category.";
                ClickHint.IsVisible = false;
                AddButtonFrame.BorderColor = Colors.Transparent;
            }
            else
            {
                ClickHint.IsVisible = false;
                AddButtonFrame.BorderColor = Colors.Transparent;
            }

            _isFiltering = false;
        });
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        if (_isFiltering) return;
        FilterTasks();
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await _db.InitializeAsync();
        await Navigation.PushAsync(new AddTaskPage());
    }

    private async void OnTrashClicked(object sender, EventArgs e)
    {
        var toDelete = Items.Where(i => i.IsCompleted).ToList();

        foreach (var item in toDelete)
        {
            try
            {
                await _db.DeleteItemAsync(item);
                Items.Remove(item);
                Debug.WriteLine($"✅ Deleted item ID: {item.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to delete item {item.Id}: {ex.Message}");
            }
        }

        FilterTasks();
    }

    private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is ItemModel task)
        {
            task.IsCompleted = e.Value;

            try
            {
                await _db.UpdateItemAsync(task);
                Debug.WriteLine($"🔄 Task {task.Id} completion updated to: {task.IsCompleted}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to update task {task.Id}: {ex.Message}");
            }

            if (SelectedCategory != "All")
                FilterTasks();
        }
    }

    private async void OnTaskTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is ItemModel item)
        {
            await Navigation.PushModalAsync(new TaskDetailPage(item, Items), true);
        }
    }

    private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        Debug.WriteLine($"Scrolled: first={e.FirstVisibleItemIndex}, last={e.LastVisibleItemIndex}");
    }
}