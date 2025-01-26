using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents.Services
{
    public interface IAppLifeCycleService   //AI
    {
        event Action Destroying;

        public void NotifyDestroying();

        public event Func<Task> DestroyingAsync;

        public Task NotifyDestroyingAsync();

        event Action LogOff;

        public void NotifyLogOff();


        public event Action<string>? OnFileChange;
        public void NotifyOnFileChange(string filename);
    }

    public class AppLifecycleService : IAppLifeCycleService     //AI
    {
        public event Action Destroying;

        public void NotifyDestroying() => Destroying?.Invoke();

        public event Func<Task> DestroyingAsync;

        public async Task NotifyDestroyingAsync()
        {
            if (DestroyingAsync != null)
            {
                var handlerTasks = DestroyingAsync.GetInvocationList()
                 .Cast<Func<Task>>()
                 .Select(handler => handler());

                await Task.WhenAll(handlerTasks);
            }
        }

        public event Action LogOff;

        public void NotifyLogOff() => LogOff?.Invoke();

        public event Action<string>? OnFileChange;
        public void NotifyOnFileChange(string filename) => OnFileChange?.Invoke(filename);
    }
}
