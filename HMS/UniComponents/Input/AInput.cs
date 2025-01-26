using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public abstract class AInput<T> : ComponentBase
    {
        /// <summary>
        /// ID inputu, Default "Ainput"
        /// </summary>
        [Parameter]
        public string ID { get => _id + Salt; set => _id = value; }
        private string _id = "Ainput";

        /// <summary>
        /// Pridá k ID další text pre jednoduché oddelenie IDčiek, Default ""
        /// </summary>
        [Parameter]
        public string Salt { get; set; } = "";

        /// <summary>
        /// Zobrazovaná hlavička, Default ""
        /// </summary>
        [Parameter]
        public string Header { get; set; } = "";

        /// <summary>
        /// Prvotna hodnota inputu, Default null
        /// </summary>
        [Parameter]
        public T? Value { get; set; } = default(T);

        /// <summary>
        /// Disablovanie inputu, Default false
        /// </summary>
        [Parameter]
        public bool Disabled { get; set; } = false;

        /// <summary>
        /// Doplnok pre input class, default ""
        /// </summary>
        [Parameter]
        public string ClassInput { get; set; } = "";

        /// <summary>
        /// Event callback aby sme zmenili hodnotu v parent komponente. Napr. <para/>
        /// ValueChanged="@(val => Property = val)"
        /// </summary>
        [Parameter]
        public EventCallback<T?> ValueChanged { get; set; }


        /// <summary>
        /// Zabezpečuje aby sa zavolal event ValueChanged a nastavila hodnota Value, možno upraviť podľa nutnosti cez override
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnInputChange(ChangeEventArgs e)
        {
            Value = (T?)e.Value;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
