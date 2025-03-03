using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents.Classes.Services
{
    public interface IAndroidHelper
    {
        /// <summary>
        /// Aktivita, ktorá sa má vykonať pri stlačení tlačidla späť
        /// </summary>
        event Action OnBackButtonPressed;

        /// <summary>
        /// Spracovanie stlačenia tlačidla späť
        /// </summary>
        void HandleBackButton();
    }
}
