using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace UniComponents
{
    public class ObservableEntitySaverVM<T> : ObservableObject where T : class, ICloneable, new()
    {
        /// <summary>
        /// Spravuje ukladanie pre danu entitu
        /// </summary>
        public EntitySaver<T> EntitySaver { get; set; }

        /// <summary>
        /// Podmnožina entit, ktoré sa vyskytujú v property danej entity a nižšie (alebo iné entity) -> treba naplnit v konstruktori najlepšie podľa hierarchie
        /// </summary>
        public List<IEntitySaver> TrackingList { get; set; } = new();

        /// <summary>
        /// Skratka na EntitySaver.Input
        /// </summary>
        public T Entity
        {
            get => EntitySaver.Input;
            set => EntitySaver.Input = value;
        }

        /// <summary>
        /// Ulozí všetky Inputy z tracking entít a potom Input z hlavnej entity
        /// </summary>
        /// <returns></returns>
        public virtual ValidationResult Save()
        {
            foreach (var entitySaver in TrackingList.Reverse<IEntitySaver>())
            {
                var res = entitySaver.SaveEntity();
                if (res != ValidationResult.Success) {
                    return res;
                }
            }

            var res2 = EntitySaver.SaveEntity();
            if (res2 != ValidationResult.Success)
            {
                return res2;
            }
            return ValidationResult.Success;
        }

        /// <summary>
        /// Prejde všetky tracking entity + hlavnu entitu a ak v niektorej existuje Modified na true tak vráti true
        /// </summary>
        /// <returns></returns>
        public virtual bool AnyModify()
        {
            return EntitySaver.Modified || TrackingList.Any(x => x.Modified);
        }
    }
}
