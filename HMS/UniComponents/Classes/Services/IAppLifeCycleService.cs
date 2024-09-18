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

        event Action LogOff;

        public void NotifyLogOff();
    }

    public class AppLifecycleService : IAppLifeCycleService     //AI
    {
        public event Action Destroying;

        public void NotifyDestroying() => Destroying?.Invoke();

        public event Action LogOff;

        public void NotifyLogOff() => LogOff?.Invoke();
    }
}
