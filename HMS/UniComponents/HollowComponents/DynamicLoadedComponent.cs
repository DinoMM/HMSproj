﻿using Microsoft.AspNetCore.Components;
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
    public abstract class DynamicLoadedComponent : ComponentBase
    {
        /// <summary>
        /// Hodnota ukazuje, či sa má komponent renderovať (Treba spraviť @if(Rendered)). true => renderovať, default false
        /// </summary>
        public bool Rendered { get; private set; } = false;

        /// <summary>
        /// Funkcia, ktorá sa vykoná po zrenderovaní komponentu (jeho inicializácií)
        /// </summary>
        [Parameter]
        public Func<Task>? AfterRender { get; set; }

        /// <summary>
        /// Funkcia, ktorá sa vykoná po ukončení komponentu (jeho deštrukcí)
        /// </summary>
        [Parameter]
        public Func<Task>? AfterDispose { get; set; }

        /// <summary>
        /// Funkcia, ktorá sa vykoná 
        /// </summary>
        [Parameter]
        public Func<Task>? RenderChild { get; set; }

        private bool childRendered = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)        //pri inicializácii sa vykona určená metóda AfterRender
            {
                if (AfterRender != null)
                {
                    await AfterRender();
                }
            }
            if (!childRendered && RenderChild != null)      //ked je comp rendernuty a ma cast, ktoru treba vyrenderovat
            {
                childRendered = true;
                await RenderChild();
                RenderChild = null;
                childRendered = false;
            }

        }

        /// <summary>
        /// Nastavuje Rendered = true, bez zastavenia programu
        /// </summary>
        public virtual void StartRender()
        {
            Rendered = true;
        }

        /// <summary>
        /// nastavuje Rendered = false, spúšťa AfterComplete
        /// </summary>
        public async Task Dispose()
        {
            Rendered = false;
            if (AfterDispose != null)
            {
                await AfterDispose();
            }
        }
    }
}
