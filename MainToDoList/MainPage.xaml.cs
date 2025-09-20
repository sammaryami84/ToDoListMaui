using MainToDoList.Views;

namespace MainToDoList;
public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }



    private void ToDoList_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ItemListPage());
    }
}
