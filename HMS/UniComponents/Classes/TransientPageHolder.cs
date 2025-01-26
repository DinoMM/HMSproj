using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class TransientPageHolder<T> : ITransientHolder
    {
        public T? TransientObjectT { get; set; }
        public object? TransientObject { get => TransientObjectT; set => TransientObjectT = (T?)value; }

        /// <summary>
        /// Url, ktorá sa skontroluje s aktuálnou url stránky. Ak sa rovnajú, tak sa vykoná mazanie a spustenie ClearMethod. Default "/"
        /// </summary>
        public string ConditionEndUrl { get; set; } = "/";
        public Action? ClearMethod { get; set; }
        public Func<Task>? ClearMethodAsync { get; set; }

        /// <summary>
        /// Urls, ktoré sa skontrolujú s predošlou url stránky. Ak sa rovnajú, tak sa vykoná metóda StartMethodAsync. Default prazdny
        /// </summary>
        public string[] ConditionStartUrl { get; set; } = Array.Empty<string>();
        public bool NextVisitStart { get; set; } = false;
        public Func<Task>? StartMethodAsync { get; set; }

        /// <summary>
        /// Určuje ako sa majú kopírovať properties. Default TransientCopyMethod.PublicPrivateThenPublic
        /// </summary>
        public TransientCopyMethod CopyMethod { get; set; } = TransientCopyMethod.PublicPrivateThenPublic;

        private readonly Navigator _navigator;
        public TransientPageHolder(Navigator navigator)
        {
            _navigator = navigator;
        }

        public void CopyProperties(T toThisTransient)
        {
            if (TransientObjectT != null)
            {
                switch (CopyMethod)
                {
                    case TransientCopyMethod.PublicThenPublic:
                        toThisTransient.CopyPropertiesFromX(TransientObjectT);
                        break;
                    case TransientCopyMethod.PublicPrivateThenPublicPrivate:
                        toThisTransient.CopyAllPropertiesFromX(TransientObjectT);
                        break;
                    case TransientCopyMethod.PublicPrivateThenPublic:
                        toThisTransient.CopyAllPropertiesFromX_1level(TransientObjectT);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("CopyMethod doesnt exist  in TransientPageHolder");
                }
            }
        }

        public async Task CopyPropertiesAsync(T toThisTransient)
        {
            await Task.Run(() =>
            {
                CopyProperties(toThisTransient);
            });
        }
        public void CopyProperties(object toThisTransient)
        {
            if (toThisTransient is T tttransient)
            {
                CopyProperties(tttransient);
            }
        }

        public async Task CopyPropertiesAsync(object toThisTransient)
        {
            if (toThisTransient is T tttransient)
            {
                await CopyPropertiesAsync(tttransient);
            }
        }

        public bool CheckOutScope()
        {
            if (_navigator.ActualPageUrl == ConditionEndUrl)
            {
                ClearMethod?.Invoke();
                TransientObject = default(T);
                return true;
            }
            return false;
        }

        public async Task<bool> CheckOutScopeAsync()
        {
            if (_navigator.ActualPageUrl == ConditionEndUrl)
            {
                if (ClearMethodAsync != null)
                {
                    await ClearMethodAsync();
                }
                TransientObject = default(T);
                return true;
            }
            return false;
        }

        public async Task<bool> CheckStartConditionAsync()
        {
            if (ConditionStartUrl.Length == 0 && !NextVisitStart)
            {
                return false;
            }
            if (NextVisitStart || ConditionStartUrl.Contains(_navigator.PreviousPageUrl))
            {
                if (StartMethodAsync != null)
                {
                    await StartMethodAsync();
                }
                NextVisitStart = false;
                return true;
            }
            return false;
        }

        public T GetTransientFromAnotherHolder(ITransientHolder anotherHolder)
        {
            if (anotherHolder.TransientObject == null)
            {
                throw new ArgumentNullException($"TransientObject is null {anotherHolder.GetType()}");
            }
            if (anotherHolder is TransientPageHolder<T> anotherTransientHolder)
            {
                return anotherTransientHolder.TransientObjectT;
            }
            throw new ArgumentException($"TransientHolder is bad type {anotherHolder.GetType()}");
        }
    }

    public interface ITransientHolder
    {
        /// <summary>
        /// Objekt, ktory je pridaný ako AddTransient. Nutnosť inicializovať pri vytvorení
        /// </summary>
        public object? TransientObject { get; set; }

        /// <summary>
        /// Metóda, ktorá sa vykoná pri mazaní stránky, možno vyčistiť potrebné veci pred vymazaním
        /// </summary>
        public Action? ClearMethod { get; set; }

        /// <summary>
        /// Rovnaká metóda ako ClearMethod, ale asynchrónne
        /// </summary>
        public Func<Task>? ClearMethodAsync { get; set; }

        /// <summary>
        /// Metóda, ktorá sa vykoná pri vstup na stránku z inej stránky ak bola splnená podmienka, možno spustiť potrebné veci
        /// </summary>
        public Func<Task>? StartMethodAsync { get; set; }

        /// <summary>
        /// Ak true tak sa spustí StartMethodAsync pri návšteve stránky
        /// </summary>
        public bool NextVisitStart { get; set; }

        /// <summary>
        /// Skopíruje properties z TransientObject do toThisTransient
        /// </summary>
        /// <param name="toThisTransient"></param>
        public void CopyProperties(object toThisTransient);

        /// <summary>
        /// Rovnaká metóda ako CopyProperties, ale asynchrónne
        /// </summary>
        /// <param name="toThisTransient"></param>
        /// <returns></returns>
        public Task CopyPropertiesAsync(object toThisTransient);

        /// <summary>
        /// Vyčistí TransientObject a vykoná ClearMethod, ak je ConditionEndUrl rovnaká ako ActualPageUrl
        /// </summary>
        /// <returns></returns>
        public bool CheckOutScope();

        /// <summary>
        /// Rovnaká metóda ako CheckOutScope, ale asynchrónne
        /// </summary>
        /// <returns></returns>
        public Task<bool> CheckOutScopeAsync();

        /// <summary>
        ///Skontroluje či predošlá stránka nebola nájdena v zozname, ak áno tak spustí metódu StartMethodAsync, asynchrónne
        /// </summary>
        /// <returns></returns>
        public Task<bool> CheckStartConditionAsync();

        /// <summary>
        /// Určuje ako sa majú kopírovať properties.
        /// </summary>
        public TransientCopyMethod CopyMethod { get; set; }
    }

    public enum TransientCopyMethod
    {
        /// <summary>
        /// Kopírujú sa len Public properties a prop s atribútom [CopyProperties] tiež len Public properties
        /// </summary>
        PublicThenPublic,
        /// <summary>
        /// Kopírujú sa Public + Private properties a prop s atribútom [CopyProperties] tiež Public + Private properties
        /// </summary>
        PublicPrivateThenPublicPrivate,
        /// <summary>
        /// Kopírujú sa Public + Private properties a prop s atribútom [CopyProperties] už len Public properties
        /// </summary>
        PublicPrivateThenPublic
    }
}
