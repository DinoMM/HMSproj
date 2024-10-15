using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class ObjectHolder
    {
        public List<object> Container { get; set; } = new();

        public void Add(object obj)
        {
            Container.Add(obj);
        }
        /// <summary>
        /// Najde podla typu prvy objekt, odstrani ho a zaroven vrati
        /// </summary>
        /// <typeparam name="T">Hľadaný typ</typeparam>
        /// <returns></returns>
        public T? Remove<T>()
        {
            T? founded = default;
            for (int i = 0; i < Container.Count; i++)
            {
                if (Container[i] is T)
                {
                    founded = (T)Container[i];
                    Container.RemoveAt(i);
                    break;
                }
            }
            return founded;
        }
        /// <summary>
        /// Najde podla typu prvy objekt a vrati ho
        /// </summary>
        /// <typeparam name="T">Hľadaný typ</typeparam>
        /// <returns></returns>
        public T? Find<T>()
        {
            T? founded = default;
            for (int i = 0; i < Container.Count; i++)
            {
                if (Container[i] is T)
                {
                    founded = (T)Container[i];
                    break;
                }
            }
            return founded;
        }
        /// <summary>
        /// Najde podla typu vsetky objekt a vrati ich v podobe listu
        /// null - žiaden prvok sa nenašiel
        /// </summary>
        /// <typeparam name="T">Hľadaný typ</typeparam>
        /// <returns></returns>
        public List<T>? FindList<T>()
        {
            List<T> founded = new();
            for (int i = 0; i < Container.Count; i++)
            {
                if (Container[i] is T)
                {
                    founded.Add((T)Container[i]);
                    break;
                }
            }
            if (founded.Count == 0) //ak neobsahuje ziaden prvok tak rovno vraitme null
            {
                return null;
            }
            return founded;
        }

        /// <summary>
        /// Zaručí, že v kontajneri bude len jeden objekt daného typu. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void AddOnlyOne<T>(T obj)
        {
            while (true)
            {
                var found = Remove<T>();
                if (found == null)
                {
                    Add(obj);
                    break;
                }
            }
        }
    }
}
