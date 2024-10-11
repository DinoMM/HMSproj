using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class DruhPohybu : ICloneable
    {
        [Key]
        public string ID { get; set; }
        public string Nazov { get; set; } = default!;
        public string MD { get; set; } = default!;
        public string DAL { get; set; } = default!;

        public object Clone()
        {
            return new DruhPohybu
            {
                ID = this.ID,
                Nazov = this.Nazov,
                MD = this.MD,
                DAL = this.DAL
            };
        }
        public DruhPohybu Clon()
        {
            return (DruhPohybu)this.Clone();
        }
    }
}
