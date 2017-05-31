using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Opcodes
    {
        public static readonly byte pushInt32 = 0;
        public static readonly byte pushString = 1;
        public static readonly byte pushVar = 2;
        public static readonly byte print = 3;
        public static readonly byte printLine = 4;
        public static readonly byte read = 5;
        public static readonly byte readLine = 6;
        public static readonly byte halt = 7;
        public static readonly byte inputInt32 = 8;
        public static readonly byte inputString = 9;
        public static readonly byte pop = 10;
        public static readonly byte popa = 11;
        public static readonly byte decVar = 12;
        public static readonly byte setVar = 13;
        public static readonly byte add = 14;
        public static readonly byte sub = 15;
        public static readonly byte mul = 16;
        public static readonly byte div = 17;
        public static readonly byte clear = 18;
        public static readonly byte ife = 19;
        public static readonly byte ifn = 20;
        public static readonly byte elseife = 21;
        public static readonly byte elseifn = 22;
        public static readonly byte els = 23;
        public static readonly byte endif = 24;
        public static readonly byte call = 25;
        public static readonly byte got = 26;
        public static readonly byte ret = 27;
    }
}
