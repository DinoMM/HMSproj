using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public interface IEntitySaver
    {
        ValidationResult SaveEntity();
        bool Modified { get; }
        bool Exist { get; }
    }
}
