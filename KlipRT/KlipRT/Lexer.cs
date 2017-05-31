using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Lexer
    {
        public List<Func> funcs;
        public List<Block> blocks;
        public Buffer code;

        public Lexer(string c)
        {
            c = c.Replace(((char)13).ToString(), "");

            Func currentFunc = null;
            funcs = new List<Func>();
            code = new KlipRT.Buffer();
            Block currentBlock = null;
            blocks = new List<Block>();
            int blockNumber = 0;
            Stack<Block> blockstack = new Stack<Block>();

            foreach (string a in c.Split('\n'))
            {
                if (a.StartsWith(":"))
                {
                    string op = a.Substring(1);

                    if (currentFunc == null)
                    {
                        currentFunc = new Func(op, code.buffer.Count);
                    }
                    else
                    {
                        code.Write(Opcodes.ret);
                        funcs.Add(currentFunc);
                        currentFunc = new Func(op, code.buffer.Count);
                    }
                }
                else if (a.StartsWith("."))
                {
                    string name = a.Substring(1);
                    Label l = new Label(name, code.pos);
                    currentFunc.labels.Add(l);
                }
                else if (a.StartsWith("pushInt32 "))
                {
                    int value = Convert.ToInt32(a.Substring(10));
                    code.Write(Opcodes.pushInt32);
                    code.Write(value);
                }
                else if (a.StartsWith("pushString "))
                {
                    string temp = a.Substring(11);
                    string value = temp.Substring(temp.IndexOf("\"") + 1, temp.LastIndexOf("\"") - 1);
                    code.Write(Opcodes.pushString);
                    code.Write(value);
                }
                else if (a.StartsWith("pushVar "))
                {
                    string name = a.Substring(8);
                    code.Write(Opcodes.pushVar);
                    code.Write(name);
                }
                else if (a == "print")
                {
                    code.Write(Opcodes.print);
                }
                else if (a == "printLine")
                {
                    code.Write(Opcodes.printLine);
                }
                else if (a == "read")
                {
                    code.Write(Opcodes.read);
                }
                else if (a == "readLine")
                {
                    code.Write(Opcodes.readLine);
                }
                else if (a == "halt")
                {
                    code.Write(Opcodes.halt);
                }
                else if (a == "inputInt32")
                {
                    code.Write(Opcodes.inputInt32);
                }
                else if (a == "inputString")
                {
                    code.Write(Opcodes.inputString);
                }
                else if (a == "pop")
                {
                    code.Write(Opcodes.pop);
                }
                else if (a == "popa")
                {
                    code.Write(Opcodes.popa);
                }
                else if (a.StartsWith("decVar "))
                {
                    string name = a.Substring(7);
                    code.Write(Opcodes.decVar);
                    code.Write(name);
                }
                else if (a.StartsWith("setVar "))
                {
                    string name = a.Substring(7);
                    code.Write(Opcodes.setVar);
                    code.Write(name);
                }
                else if (a == "add")
                {
                    code.Write(Opcodes.add);
                }
                else if (a == "sub")
                {
                    code.Write(Opcodes.sub);
                }
                else if (a == "mul")
                {
                    code.Write(Opcodes.mul);
                }
                else if (a == "div")
                {
                    code.Write(Opcodes.div);
                }
                else if (a == "clear")
                {
                    code.Write(Opcodes.clear);
                }
                else if (a == "ife")
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new IfBlock(blockNumber);
                        code.Write(Opcodes.ife);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                    else
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new IfBlock(blockNumber);
                        code.Write(Opcodes.ife);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                }
                else if (a == "ifn")
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new IfBlock(blockNumber);
                        code.Write(Opcodes.ifn);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                    else
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new IfBlock(blockNumber);
                        code.Write(Opcodes.ifn);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                }
                else if (a == "elseife")
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new ElseIfBlock(blockNumber);
                        code.Write(Opcodes.elseife);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                    else
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new IfBlock(blockNumber);
                        code.Write(Opcodes.elseife);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                }
                else if (a == "elseifn")
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new ElseIfBlock(blockNumber);
                        code.Write(Opcodes.elseifn);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                    else
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new IfBlock(blockNumber);
                        code.Write(Opcodes.elseifn);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                }
                else if (a == "else")
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new ElseBlock(blockNumber);
                        code.Write(Opcodes.els);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                    else
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new ElseBlock(blockNumber);
                        code.Write(Opcodes.els);
                        code.Write(blockNumber);
                        blockNumber++;
                    }
                }
                else if (a == "endif")
                {
                    if (blockstack.Count == 0)
                    {
                        code.Write(Opcodes.endif);
                        currentBlock.endBlock = code.buffer.Count();
                        blocks.Add(currentBlock);
                        currentBlock = null;
                    }
                    else if (blockstack.Count > 0)
                    {
                        code.Write(Opcodes.endif);
                        currentBlock.endBlock = code.buffer.Count();
                        blocks.Add(currentBlock);
                        currentBlock = blockstack.Pop();
                    }
                }
                else if (a.StartsWith("call "))
                {
                    string name = a.Substring(5);
                    code.Write(Opcodes.call);
                    code.Write(name);
                }
                else if (a.StartsWith("goto "))
                {
                    string name = a.Substring(5);
                    code.Write(Opcodes.got);
                    code.Write(name);
                }
                else if (a == "ret")
                {
                    code.Write(Opcodes.ret);
                }
            }

            code.Write(Opcodes.ret);
            funcs.Add(currentFunc);
        }
    }
}
