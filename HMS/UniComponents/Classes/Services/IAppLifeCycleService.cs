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
    }

    public class AppLifecycleService : IAppLifeCycleService     //AI
    {
        public event Action Destroying;

        public void NotifyDestroying() => Destroying?.Invoke();
    }
}
