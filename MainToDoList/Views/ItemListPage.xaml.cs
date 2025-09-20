using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace MainToDoList.Views;

public partial class ItemListPage : ContentPage
{
    public ItemListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ✅ تازه‌سازی کامل لیست تسک‌ها از دیتابیس
        if (itemListView != null)
        {
            await itemListView.RefreshAsync();
        }
    }

    private void OnHamburgerClicked(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = true;
    }
}