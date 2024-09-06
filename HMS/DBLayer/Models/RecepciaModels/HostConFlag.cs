using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class HostConFlag
    {
        [ForeignKey("HostX")]
        public long Host { get; set; }
        public Host HostX { get; set; }
        [ForeignKey("HostFlagX")]
        public string HostFlag { get; set; }
        public HostFlag HostFlagX { get; set; }
    }
}
