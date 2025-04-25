using Microsoft.AspNetCore.Components.WebView.Maui;

namespace HMS
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            //CreateWebView(typeof(Other), 2, 0);
            //RemoveWebView(2, 1);

        }

        //public void CreateWebView(Type page, int Row, int Col, int RowSpan = 0, int ColSpan = 0 ) { //mozno vymazat
        //    var blazorWebView = new BlazorWebView
        //    {
        //        HostPage = "wwwroot/index.html",
        //    };
        //    blazorWebView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = page });

        //    Grid.SetRow(blazorWebView, Row);
        //    Grid.SetColumn(blazorWebView, Col);
        //    Grid.SetRowSpan(blazorWebView, RowSpan);
        //    Grid.SetColumnSpan(blazorWebView, ColSpan);


        //    MainGrid.Children.Add(blazorWebView);
        //}

        //public void RemoveWebView(int Row, int Col) {       //mozno vymazat
        //    var childToRemove = MainGrid.Children.FirstOrDefault(child =>
        //    MainGrid.GetRow(child) == Row && MainGrid.GetColumn(child) == Col && child is BlazorWebView);

        //    if (childToRemove != null)
        //    {
        //        MainGrid.Children.Remove(childToRemove);
        //    }
        //}
    }
}
