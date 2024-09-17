using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    /// <summary>
    /// Poskytuje možnosť zastaviť program do doby, kým sa zrenderuje komponent (nie jeho detské komponenty)
    /// </summary>
    public abstract class DynamicLoaderComponent : ComponentBase
    {
        [Parameter]
        /// <summary>
        /// Hodnota ukazuje, či sa má komponent renderovať (Treba spraviť @if(Rendered)). true => renderovať, default false
        /// </summary>
        public bool Rendered { get; set; } = false;

        private bool loaded = false;

        [Inject]
        private ObjectHolder _objHolder { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Resume();       //ak sme zadali ze chceme vyslovene pockat do nacitania, tak od tohto bodu by mal byt komponent nacitany (detske nie)
            }

        }

        /// <summary>
        ///Spustí renderovanie a Fyzicky zastaví program do doby, keď sa zrenderuje tento komponent (nie jeho detské komponenty)
        /// </summary>
        /// <returns></returns>
        public async Task WaitForLoad(ObjectHolder objectHolder)
        {
            StartRender();
            if (!loaded)
            {
                var tcs = new TaskCompletionSource<bool>();
                objectHolder.Add(tcs);
                await tcs.Task; // Physically stop the program
            }

        }

        /// <summary>
        /// nastavuje Rendered = true, bez zastavenia programu
        /// </summary>
        public void StartRender()
        {
            Rendered = true;
        }

        /// <summary>
        /// nastavuje Rendered = false
        /// </summary>
        public async Task Dispose()
        {
            await Resume();
            Rendered = false;
        }

        /// <summary>
        /// Ak je nastavené WaitForLoad, tak tato metoda ho ukončí
        /// </summary>
        private async Task Resume()
        {
            if (loaded)
            {
                return;
            }
            var tcs = _objHolder.Remove<TaskCompletionSource<bool>>();
            if (tcs != null)
            {
                tcs.SetResult(true);
                tcs = null;
            }
            loaded = true;
        }
    }
}
