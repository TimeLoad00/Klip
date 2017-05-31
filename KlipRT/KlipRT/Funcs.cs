using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Func
    {
        public string name;
        public int location;
        public List<Label> labels = new List<Label>();

        public Func(string n, int loc)
        {
            name = n;
            location = loc;
        }
    }

    class Label
    {
        public string name;
        public int location;

        public Label(string n, int loc)
        {
            name = n;
            location = loc;
        }
    }
}
