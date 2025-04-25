using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingTemplates
{
    public interface I_RD_TableVM<T>
    {
        public bool NacitavaniePoloziek { get; }

        public abstract Task NacitajZoznamy();
        public abstract bool MoznoVymazat(T item);
        public abstract void Vymazat(T item);


    }
}
