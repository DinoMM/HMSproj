using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    /// <summary>
    /// Poskytuje možnosť vytvoriť komponent dynamicky.
    /// </summary>
    public abstract class DynamicLoadedComponent2 : ComponentBase
    {
        /// <summary>
        /// Hodnota ukazuje, či sa má komponent renderovať (Treba spraviť @if(Rendered)). true => renderovať, default false
        /// </summary>
        public bool Rendered { get; private set; } = false;

        /// <summary>
        /// Funkcia, ktorá sa vykoná po zrenderovaní komponentu
        /// </summary>
        [Parameter]
        public Func<Task>? AfterRender { get; set; }

        /// <summary>
        /// Funkcia, ktorá sa vykoná po ukončení komponentu (jeho deštrukcí)
        /// </summary>
        [Parameter]
        public Func<Task>? AfterDispose { get; set; }

        private bool onlyOne = false;       //keby sme vypli dvojite renderovanie tak nepotrebujeme toto, pravdepodobne

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Rendered && !onlyOne)        //pri renderovani sa vykona určená metóda AfterRender
            {
                onlyOne = true;
                if (AfterRender != null)
                {
                    await AfterRender();
                }
            }
        }

        /// <summary>
        /// Nastavuje Rendered = true, bez zastavenia programu
        /// </summary>
        protected virtual void StartRender()
        {
            Rendered = true;
        }

        /// <summary>
        /// nastavuje Rendered = false, spúšťa AfterComplete
        /// </summary>
        protected async Task Dispose()
        {
            Rendered = false;
            onlyOne = false;
            if (AfterDispose != null)
            {
                await AfterDispose();
            }
        }
    }
}
