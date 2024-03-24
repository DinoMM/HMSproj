using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public interface IModal
    {
        public Task<bool> OpenModal(bool stop = false);
        public void UpdateText(string newText);
    }
}
