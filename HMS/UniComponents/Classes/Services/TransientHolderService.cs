using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents.Services
{
    /// <summary>
    /// Spravuje objekty (najlepšie DI), ktoré sú transient a chceme ich podržať dlhšie ako životnosť jednej url stránky. Postup: <para/>
    /// - Treba mať zaregistrovaný tento service v DI ako Singleton <para/>
    /// - Treba mať zaregistrovaný patričný ITransientHolder ako transient. napr. TransientPageHolder<para/>
    /// - Získať injected ITransientHolder a priradiť mu TransientObject + ostatné nastavenia ak potreba<para/>
    /// - Zavolať AddOnceOrRetrieve na začiatku behu stránky pred renderom<para/>
    /// - Zavolať CheckOutScope na konci behu stránky, najlepšie cez Dispose()<para/>
    /// </summary>
    public class TransientHolderService
    {
        public List<ITransientHolder> ZoznamTransients { get; set; } = new();

        /// <summary>
        /// Prejde všetky uložené Transienty a skontroluje ich podmienku pre životnosť. Ak áno tak maže transient a odstráni ho zo zoznamu (out of scope).
        /// </summary>
        public void CheckOutScope()
        {
            List<ITransientHolder> toRemove = new();
            foreach (var item in ZoznamTransients)
            {
                if (item.CheckOutScope())
                {
                    toRemove.Add(item);
                }
            }
            foreach (var item in toRemove)
            {
                ZoznamTransients.Remove(item);
            }
        }

        /// <summary>
        /// Rovnako ako CheckOutScope, ale asynchrónne.
        /// </summary>
        /// <returns></returns>
        public async Task CheckOutScopeAsync()
        {
            List<ITransientHolder> toRemove = new();
            foreach (var item in ZoznamTransients)
            {
                if (await item.CheckOutScopeAsync())
                {
                    toRemove.Add(item);
                }
            }
            foreach (var item in toRemove)
            {
                ZoznamTransients.Remove(item);
            }
        }

        public async Task CheckStartConditionAsync()
        {
            foreach (var item in ZoznamTransients)
            {
                await item.CheckStartConditionAsync();
            }
        }

        /// <summary>
        /// Pridá TransientHolder do zoznamu ak v ňom ešte nie je. Ak tam už je, tak sa skopírujú všetky properties TransientObject.<para/>
        /// true = už existuje holder, kopíruje sa TransientObject<para/>
        /// false = nový holder, pridá sa do zoznamu, nekopíruje sa nič do TransientObject<para/>
        /// </summary>
        /// <param name="transientHolder">Treba získať z DI a priradiť TransientObject minimálne</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Ak je TransientObject null</exception>
        public bool AddOnceOrRetrieve(ITransientHolder transientHolder)
        {
            if (transientHolder.TransientObject == null)
            {
                throw new ArgumentNullException($"TransientObject is null {transientHolder.GetType()}");
            }

            var found = ZoznamTransients.FirstOrDefault(x => x.TransientObject.GetType() == transientHolder.TransientObject.GetType());
            if (found == null)
            {
                ZoznamTransients.Add(transientHolder);
                return false;
            }
            else
            {
                found.CopyProperties(transientHolder.TransientObject);
                found.TransientObject = transientHolder.TransientObject;
                return true;
            }
        }

        /// <summary>
        /// Kontroluje, či už je v zozname TransientHolder s rovnakým typom TransientObject.
        /// </summary>
        /// <param name="transientHolder"></param>
        /// <returns></returns>
        public bool ContainsHolder(ITransientHolder transientHolder)
        {
            return ZoznamTransients.Any(x => x.TransientObject.GetType() == transientHolder.TransientObject.GetType());
        }

        /// <summary>
        /// Získa TransientObject podľa typu. Ak sa nenašiel, tak vyhodí výnimku. Stačí pretypovať na požadovaný typ.
        /// </summary>
        /// <param name="typeTransient"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public object GetTransient(Type typeTransient)
        {
            return ZoznamTransients.FirstOrDefault(x => x.TransientObject.GetType() == typeTransient)?.TransientObject 
                ?? throw new ArgumentNullException("TransientObject sa nenašiel, chýba prvotná inicializácia.");
        }


    }

    /// <summary>
    /// Hovorí o tom ako sa majú kopírovať prop/fieldy v Transient objekte. <para/>
    /// Ak property má atribút [CopyProperties], tak sa kopúruje jeho "telo" rekurzívne <para/>
    /// Ak property má atribút [IgnoreCopy], tak sa nekopíruje <para/>
    /// </summary>
    

    /*public interface ITransientHolderService
    {
        public List<ITransientHolder> ZoznamTransients { get; set; }
        public void CheckOutScope();
        public Task CheckOutScopeAsync();
        public void AddOnceOrRetrieve(ITransientHolder transientHolder);
        public Task AddOnceOrRetrieveAsync(ITransientHolder transientHolder);
    }*/
}
