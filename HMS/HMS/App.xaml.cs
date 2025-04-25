
using DBLayer;
using UniComponents.Services;

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

            if (window != null)
            {
                window.Title = "HMS";   //nastavi nazov okna
            }

            window.Destroying += (s, e) =>
            {
                _appLifeCycle.NotifyDestroying();
            };

            window.Destroying += async (s, e) =>
            {
                await _appLifeCycle.NotifyDestroyingAsync();
            };

#if ANDROID
            window.Deactivated += async (s, e) =>
            {
                await _appLifeCycle.NotifyDestroyingAsync();
            };

            _appLifeCycle.OnFileChange += (filename) =>
            {
                HMS.Platforms.Android.FileManagerHelper.NotifyFileAdded(filename);
            };
#else
            window.Destroying += async (s, e) =>
            {
                await _appLifeCycle.NotifyDestroyingAsync();
            };
#endif


            return window;
        }

    }
}
