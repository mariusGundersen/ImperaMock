using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public interface IContext
    {
        Task Action(int id);

        Task<string> Function(string id);
    }
}
