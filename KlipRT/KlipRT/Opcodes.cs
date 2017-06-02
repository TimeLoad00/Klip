using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Opcodes
    {
        public static readonly int pushInt = 0;
        public static readonly int pushString = 1;
        public static readonly int pushVar = 2;
        public static readonly int print = 3;
        public static readonly int printLine = 4;
        public static readonly int read = 5;
        public static readonly int readLine = 6;
        public static readonly int halt = 7;
        public static readonly int inputInt32 = 8;
        public static readonly int inputString = 9;
        public static readonly int pop = 10;
        public static readonly int popa = 11;
        public static readonly int decVar = 12;
        public static readonly int setVar = 13;
        public static readonly int add = 14;
        public static readonly int sub = 15;
        public static readonly int mul = 16;
        public static readonly int div = 17;
        public static readonly int clear = 18;
        public static readonly int ife = 19;
        public static readonly int ifn = 20;
        public static readonly int elseife = 21;
        public static readonly int elseifn = 22;
        public static readonly int els = 23;
        public static readonly int endif = 24;
        public static readonly int call = 25;
        public static readonly int got = 26;
        public static readonly int ret = 27;
    }
}
