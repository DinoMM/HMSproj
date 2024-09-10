using DBLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingTemplates
{
    public interface I_ValidationVM
    {
        public abstract bool ValidateUser();
        public abstract bool ValidateUserCRU();
        
    }
}
