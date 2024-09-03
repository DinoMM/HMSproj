using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DBLayer.Models
{
    public class Coupon
    {
        [Key]
        [MaxLength(10)]
        public string ID { get; set; }                  //nahodne 6 miestne cislo
        public string NameService { get; set; }         //nazov sluzby na ktoru sa bude kupon stahovat
        public int Discount { get; set; }               //percentualna zlava z povodnej ceny
        public bool IsUsed { get; set; } = false;               //ukazovatel ci bol kupon uz pouzity
        public string Description { get; set; }         //popis pre lepsie pochopenie 

        /// <summary>
        /// Počet generovanych znakov v kupóne
        /// </summary>
        [NotMapped]
        public readonly static int NUM_CHARS = 6;

        /// <summary>
        /// Vráti nové náhodné ID, ktoré sa môže použiť pri tvorbe nového kupóna
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static string GetUniID(DataContext database) {
            string newID = "";
            do
            {
                for (int i = 0; i < NUM_CHARS; i++)
                {
                    newID += Random.Shared.Next(10);
                }
            } while (database.Coupons.FirstOrDefault(x => x.ID == newID) != null);
            return newID;
        }
    }
}
