using MainToDoList.Views; // اطمینان از دسترسی به صفحات
using System;

namespace MainToDoList
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            this.Navigated += (s, e) =>
            {
                if (FlyoutIsPresented)
                    FlyoutIsPresented = false;
            };


            // ثبت مسیر صفحه Calendar برای ناوبری
            Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));
        }

        // هندلر کلیک برای MenuItem مربوط به Calendar
        private async void OnCalendarClicked(object sender, EventArgs e)
        {

            await Shell.Current.GoToAsync(nameof(CalendarPage));
        }
    }
}