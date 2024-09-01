
using DBLayer;
using HMS.Components.Services;

namespace HMS
{
    public partial class App : Application
    {
        private readonly IAppLifeCycleService _appLifeCycle;
        public App(IAppLifeCycleService appLifeCycle)
        {
            InitializeComponent();
            //Routing.RegisterRoute("Objednavka", typeof(SkladModul.Objednavka.Main));
            _appLifeCycle = appLifeCycle;
            MainPage = new MainPage();
            
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Destroying += (s, e) =>
            {
                _appLifeCycle.NotifyDestroying();
            };

            return window;
        }
    }
}
