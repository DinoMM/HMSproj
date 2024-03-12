namespace HMS
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //Routing.RegisterRoute("Objednavka", typeof(SkladModul.Objednavka.Main));
            MainPage = new MainPage();
            
        }
    }
}
