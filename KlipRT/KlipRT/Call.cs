using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Call
    {
        public Func func;
        public int ret;
        public List<Var> vars;

        public Call(Func f, int loc, List<Var> v)
        {
            func = f;
            ret = loc;
            vars = v;
        }
    }
}
