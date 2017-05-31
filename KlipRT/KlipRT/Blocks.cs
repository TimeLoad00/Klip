using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Block
    {
        public int blockNumber;
        public int endBlock;
    }

    class IfBlock : Block
    {
        public IfBlock(int number)
        {
            blockNumber = number;
            endBlock = 0;
        }
    }

    class ElseIfBlock : Block
    {
        public ElseIfBlock(int number)
        {
            blockNumber = number;
            endBlock = 0;
        }
    }

    class ElseBlock : Block
    {
        public ElseBlock(int number)
        {
            blockNumber = number;
            endBlock = 0;
        }
    }
}
