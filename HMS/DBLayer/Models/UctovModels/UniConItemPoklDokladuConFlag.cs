using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models.UctovModels
{
    public partial class UniConItemPoklDokladuConFlag
    {
        [ForeignKey("UniConItemPoklDokladuX")]
        public long UniConItemPoklDokladu { get; set; }
        public UniConItemPoklDokladu UniConItemPoklDokladuX { get; set; }
        [ForeignKey("UniConItemPoklDokladuFlagX")]
        public long UniConItemPoklDokladuFlag { get; set; }
        public UniConItemPoklDokladuFlag UniConItemPoklDokladuFlagX { get; set; }
    }
}
