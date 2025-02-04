using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    /// <summary>
    /// Template pre zobrazenie stĺpca tabuľky, ktorý sa generuje z inštancie triedy T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TableColumnTemplate<T> : ICloneable
    {
        /// <summary>
        /// Názov property, napr. nameof(Student.ID), Null pre prázdnu hlavičku
        /// </summary>
        public string? ID_Prop { get; set; }

        /// <summary>
        /// Pre filtrovanie, ak uzivatel da dvojklik tak sa zobrazi okno s moznostami na sortovanie v stlpci, Default null
        /// </summary>
        public List<string>? SelectionList { get; set; } = null;

        /// <summary>
        /// Zobrazený text v hlavičke stĺpca
        /// </summary>
        public string HeaderValue { get; set; } = "";

        /// <summary>
        /// Doplňkový class pre Hlavičky stĺpcov th
        /// </summary>
        public string HeaderClass { get; set; } = "";

        /// <summary>
        /// Zobrazený text pri v buňke td z inštancie ktora vráti string. Napr. (x) => x.Meno
        /// </summary>
        public Func<T, string>? CellValue { get; set; }

        /// <summary>
        /// Doplňkový class pre buňky td
        /// </summary>
        public string CellClass { get; set; } = "";

        /// <summary>
        /// Vytvorí vlastný komponent v buňke td ak CellValue == null
        /// </summary>
        public Func<T, RenderFragmentTemplate>? CellComponent { get; set; }

        /// <summary>
        /// Konvertuje hodnotu z inštancie T na iný typ, napr. string na DateTime?, string na bool? a podobne. Tento výsledny typ treba specifikovat do Type. <para/>
        /// Slúži pre filtrovanie alebo Sortovanie
        /// </summary>
        public (Func<T, object?>, Type)? CellConvert { get; set; } = null;


        /// <summary>
        /// Zobrazenie sĺpca v tabuľke, default true
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Zobrazený text/component v td ak je: ID_Prop == null.<para/> Ak bude obsahovať Spúšťacie funkcie tak treba pridať v OnInitialized 
        /// Vstupuje T - iterovaný item
        /// </summary>
        public Func<T, RenderFragmentTemplate>? Placeholder { get; set; }

        public object Clone()
        {
            return new TableColumnTemplate<T>
            {
                ID_Prop = this.ID_Prop,
                SelectionList = this.SelectionList,
                HeaderValue = this.HeaderValue,
                HeaderClass = this.HeaderClass,
                CellValue = this.CellValue,
                CellClass = this.CellClass,
                CellConvert = this.CellConvert,
                CellComponent = this.CellComponent,
                Visible = this.Visible,
                Placeholder = this.Placeholder
            };
        }
        public TableColumnTemplate<T> Clon()
        {
            return (TableColumnTemplate<T>)Clone();
        }
    }

    /// <summary>
    /// Jednoduchý template pre vykreslenie vlastných .razor komponentov napr. <para/>
    /// CellComponent = (e) => new(typeof(SimpleCheckMarkDiv),new RenderFragmentAttribute("Checked", e.Accept == "A"))}
    /// </summary>
    public class RenderFragmentTemplate
    {
        /// <summary>
        /// Typ komponentu, ktorý sa má vykresliť, pre vlastné komponenty .razor. Napr. typeof(MyComponent)
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Zoznam atribútov, ktoré sa majú nastaviť na komponent, Default je vytvorený prázdny zoznam
        /// </summary>
        public List<RenderFragmentAttribute> Attributes { get; set; } = new();

        /// <summary>
        /// Vytvorí RenderFragment z textu
        /// </summary>
        /// <param name="text">zobrazovaný text v buňke</param>
        /// <returns></returns>
        public static RenderFragment GenerateRenderFragment(string text)
        {
            return builder => builder.AddContent(0, text);
        }

        public RenderFragmentTemplate(Type type, params RenderFragmentAttribute[]? attributes)
        {
            Type = type;
            if (attributes != null)
            {
                Attributes.AddRange(attributes);
            }
        }

        /// <summary>
        /// Vytvorí RenderFragment z nastavených property
        /// </summary>
        /// <returns></returns>
        public RenderFragment GenerateRenderFragment()
        {
            return builder =>
            {
                builder.OpenComponent(0, Type);
                for (int i = 1; i <= Attributes.Count; ++i)
                {
                    if (Attributes[i - 1].Value != null)
                    {
                        builder.AddAttribute(i, Attributes[i - 1].Name, Attributes[i - 1].Value);
                    }
                    else
                    {
                        builder.AddAttribute(i, Attributes[i - 1].Name, EventCallback.Factory.Create(this, Attributes[i - 1].Func));
                    }
                }
                builder.CloseComponent();

            };
        }

        public static RenderFragment GenerateRenderFragmentFromInstance<Comp>(Comp? instance) where Comp : ComponentBase
        {
            if (instance == null)
            {
                return GenerateRenderFragment("");
            }
            var type = typeof(Comp);
            var list = new List<RenderFragmentAttribute>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<ParameterAttribute>() != null)
                {
                    if (property.CanRead)
                    {
                        var value = property.GetValue(instance);
                        list.Add(new(property.Name, value));
                    }
                }
            }
            var render = new RenderFragmentTemplate(type, list.ToArray());
            return render.GenerateRenderFragment();
        }
    }

    /// <summary>
    /// Atribút, ktorý sa nastaví na komponent napr. Checked="true", disabled="false"
    /// </summary>
    public class RenderFragmentAttribute
    {
        /// <summary>
        /// Názov atribútu, napr "Checked", "value", "class"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Hodnota atribútu, napr true, "text", "btn-success"
        /// </summary>
        public object? Value { get; set; }

        public Func<Task> Func { get; set; } = () => Task.CompletedTask;

        public RenderFragmentAttribute(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public RenderFragmentAttribute(string name, Func<Task> func)
        {
            Name = name;
            Value = null;
            Func = func;
        }
    }



}
