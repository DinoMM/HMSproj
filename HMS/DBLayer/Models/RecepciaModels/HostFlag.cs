using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class HostFlag : Flag
    {

        public override HostFlag Clon()
        {
            var clon = base.Clon();
            return new HostFlag() { 
                ID = clon.ID,
                NumericValue = clon.NumericValue,
                StringValue = clon.StringValue,
                DateValue = clon.DateValue
            };
        }

        public override void SetFromOther(Flag other)
        {
            base.SetFromOther(other);
        }
    }
    
}
