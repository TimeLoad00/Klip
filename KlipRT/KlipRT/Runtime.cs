using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipRT
{
    class Runtime
    {
        static List<Func> funcs = new List<Func>();
        static List<Block> blocks = new List<Block>();
        static List<Var> vars = new List<Var>();
        static Stack<object> stack = new Stack<object>();
        static Buffer code = new Buffer();
        static bool running = true;
        static Func currentFunc = null;
        static Stack<Call> callstack = new Stack<Call>();
        static bool ifWorked = false;

        public Runtime(string c)
        {
            Lexer lexer = new Lexer(c);
            funcs = lexer.funcs;
            blocks = lexer.blocks;
            code = lexer.code;

            Run(GetFunc("Main"));
        }

        static void Run(Func func)
        {
            byte opcode = 0;
            code.pos = func.location;
            currentFunc = func;
            Block currentBlock = null;
            Stack<Block> blockstack = new Stack<Block>();

            while (running)
            {
                try
                {
                    opcode = code.Read();
                }
                catch { }
                
                if (opcode == Opcodes.pushInt32)
                {
                    stack.Push(code.ReadInt32());
                }
                else if (opcode == Opcodes.pushString)
                {
                    stack.Push(code.ReadString());
                }
                else if (opcode == Opcodes.pushVar)
                {
                    stack.Push(GetVarValue(code.ReadString()));
                }
                else if (opcode == Opcodes.print)
                {
                    Console.Write(stack.Pop());
                }
                else if (opcode == Opcodes.printLine)
                {
                    Console.WriteLine(stack.Pop());
                }
                else if (opcode == Opcodes.read)
                {
                    Console.Read();
                }
                else if (opcode == Opcodes.readLine)
                {
                    Console.ReadLine();
                }
                else if (opcode == Opcodes.halt)
                {
                    while (true) { }
                }
                else if (opcode == Opcodes.inputInt32)
                {
                    stack.Push(Convert.ToInt32(Console.ReadLine()));
                }
                else if (opcode == Opcodes.inputString)
                {
                    stack.Push(Console.ReadLine());
                }
                else if (opcode == Opcodes.pop)
                {
                    stack.Pop();
                }
                else if (opcode == Opcodes.popa)
                {
                    stack.Clear();
                }
                else if (opcode == Opcodes.decVar)
                {
                    vars.Add(new Var(code.ReadString()));
                }
                else if (opcode == Opcodes.setVar)
                {
                    SetVarValue(code.ReadString(), stack.Pop());
                }
                else if (opcode == Opcodes.add)
                {
                    object value1 = stack.Pop();
                    object value2 = stack.Pop();

                    if (value1 is string && value2 is string)
                    {
                        string value = ((string)value2) + ((string)value1);
                        stack.Push(value);
                    }
                    else if (value1 is int && value2 is int)
                    {
                        int value = ((int)value1) + ((int)value2);
                        stack.Push(value);
                    }
                }
                else if (opcode == Opcodes.sub)
                {
                    int value1 = (int)stack.Pop();
                    int value2 = (int)stack.Pop();
                    stack.Push(value1 + value2);
                }
                else if (opcode == Opcodes.mul)
                {
                    int value1 = (int)stack.Pop();
                    int value2 = (int)stack.Pop();
                    stack.Push(value1 * value2);
                }
                else if (opcode == Opcodes.div)
                {
                    int value1 = (int)stack.Pop();
                    int value2 = (int)stack.Pop();
                    stack.Push(value1 / value2);
                }
                else if (opcode == Opcodes.clear)
                {
                    Console.Clear();
                }
                else if (opcode == Opcodes.ife)
                {
                    int blockNumber = code.ReadInt32();
                    IfBlock ifblock = GetIf(blockNumber);

                    object value1 = stack.Pop();
                    object value2 = stack.Pop();

                    if (IfEqual(value1, value2))
                    {
                        if (currentBlock == null)
                        {
                            currentBlock = ifblock;
                        }
                        else
                        {
                            blockstack.Push(currentBlock);
                            currentBlock = ifblock;
                        }
                        IncVars();
                        ifWorked = true;
                    }
                    else
                    {
                        code.pos = ifblock.endBlock;
                        ifWorked = false;
                    }
                }
                else if (opcode == Opcodes.ifn)
                {
                    int blockNumber = code.ReadInt32();
                    IfBlock ifblock = GetIf(blockNumber);

                    object value1 = stack.Pop();
                    object value2 = stack.Pop();

                    if (IfNotEqual(value1, value2))
                    {
                        if (currentBlock == null)
                        {
                            currentBlock = ifblock;
                        }
                        else
                        {
                            blockstack.Push(currentBlock);
                            currentBlock = ifblock;
                        }
                        IncVars();
                        ifWorked = true;
                    }
                    else
                    {
                        code.pos = ifblock.endBlock;
                        ifWorked = false;
                    }
                }
                else if (opcode == Opcodes.elseife)
                {
                    int blockNumber = code.ReadInt32();
                    ElseIfBlock elseifblock = GetElseIf(blockNumber);

                    if (!ifWorked)
                    {
                        object value1 = stack.Pop();
                        object value2 = stack.Pop();

                        if (IfEqual(value1, value2))
                        {
                            if (currentBlock == null)
                            {
                                currentBlock = elseifblock;
                            }
                            else
                            {
                                blockstack.Push(currentBlock);
                                currentBlock = elseifblock;
                            }
                            IncVars();
                            ifWorked = true;
                        }
                        else
                        {
                            code.pos = elseifblock.endBlock;
                            ifWorked = false;
                        }
                    }
                    else
                    {
                        code.pos = elseifblock.endBlock;
                    }
                }
                else if (opcode == Opcodes.elseifn)
                {
                    int blockNumber = code.ReadInt32();
                    ElseIfBlock elseifblock = GetElseIf(blockNumber);

                    if (!ifWorked)
                    {
                        object value1 = stack.Pop();
                        object value2 = stack.Pop();

                        if (IfNotEqual(value1, value2))
                        {
                            if (currentBlock == null)
                            {
                                currentBlock = elseifblock;
                            }
                            else
                            {
                                blockstack.Push(currentBlock);
                                currentBlock = elseifblock;
                            }
                            IncVars();
                            ifWorked = true;
                        }
                        else
                        {
                            code.pos = elseifblock.endBlock;
                            ifWorked = false;
                        }
                    }
                    else
                    {
                        code.pos = elseifblock.endBlock;
                    }
                }
                else if (opcode == Opcodes.els)
                {
                    int blockNumber = code.ReadInt32();
                    ElseBlock elseblock = GetElse(blockNumber);

                    if (!ifWorked)
                    {
                        if (currentBlock == null)
                        {
                            currentBlock = elseblock;
                        }
                        else
                        {
                            blockstack.Push(currentBlock);
                            currentBlock = elseblock;
                        }
                        IncVars();
                    }
                    else
                    {
                        code.pos = elseblock.endBlock;
                    }
                }
                else if (opcode == Opcodes.endif)
                {
                    if (blockstack.Count > 0)
                    {
                        currentBlock = blockstack.Pop();
                    }
                    else
                    {
                        currentBlock = null;
                    }
                    DecVars();
                    DestroyVars();
                }
                else if (opcode == Opcodes.call)
                {
                    string name = code.ReadString();
                    Func f = GetFunc(name);
                    Call c = new Call(currentFunc, code.pos, vars);
                    callstack.Push(c);
                    currentFunc = f;
                    code.pos = f.location;
                    vars.Clear();
                }
                else if (opcode == Opcodes.got)
                {
                    string name = code.ReadString();
                    int location = GetLabel(name);
                    code.pos = location;
                }
                else if (opcode == Opcodes.ret)
                {
                    if (callstack.Count > 0)
                    {
                        Call c = callstack.Pop();
                        currentFunc = c.func;
                        code.pos = c.ret;
                        vars = c.vars;
                    }
                    else
                    {
                        running = false;
                    }
                }
            }
        }

        static Func GetFunc(string name)
        {
            Func func = null;

            foreach (Func f in funcs)
            {
                if (f.name == name)
                {
                    func = f;
                }
            }

            return func;
        }

        static int GetLabel(string name)
        {
            int location = 0;

            foreach (Label l in currentFunc.labels)
            {
                if (l.name == name)
                {
                    location = l.location;
                }
            }

            return location;
        }

        static object GetVarValue(string name)
        {
            object value = 0;

            foreach (Var v in vars)
            {
                if (v.name == name)
                {
                    value = v.value;
                }
            }

            return value;
        }

        static void SetVarValue(string name, object value)
        {
            bool found = false;

            foreach (Var v in vars)
            {
                if (v.name == name)
                {
                    v.value = value;
                    found = true;
                }
            }

            if (!found)
            {
                Var v = new Var(name);
                v.value = value;
                vars.Add(v);
            }
        }

        static IfBlock GetIf(int blockNumber)
        {
            IfBlock ifblock = null;

            foreach (Block b in blocks)
            {
                if (b is IfBlock)
                {
                    IfBlock bl = (IfBlock)b;

                    if (bl.blockNumber == blockNumber)
                    {
                        ifblock = bl;
                    }
                }
            }

            return ifblock;
        }

        static ElseIfBlock GetElseIf(int blockNumber)
        {
            ElseIfBlock elseifblock = null;

            foreach (Block b in blocks)
            {
                if (b is ElseIfBlock)
                {
                    ElseIfBlock bl = (ElseIfBlock)b;

                    if (bl.blockNumber == blockNumber)
                    {
                        elseifblock = bl;
                    }
                }
            }

            return elseifblock;
        }

        static ElseBlock GetElse(int blockNumber)
        {
            ElseBlock elseblock = null;

            foreach (Block b in blocks)
            {
                if (b is ElseBlock)
                {
                    ElseBlock bl = (ElseBlock)b;

                    if (bl.blockNumber == blockNumber)
                    {
                        elseblock = bl;
                    }
                }
            }

            return elseblock;
        }

        static bool IfEqual(object value1, object value2)
        {
            bool ifequal = false;

            if ((value1 is int && value2 is int) && ((int)value1 == (int)value2))
            {
                ifequal = true;
            }
            else if ((value1 is string && value2 is string) && ((string)value1 == (string)value2))
            {
                ifequal = true;
            }

            return ifequal;
        }

        static bool IfNotEqual(object value1, object value2)
        {
            bool ifequal = false;

            if ((value1 is int && value2 is int) && ((int)value1 != (int)value2))
            {
                ifequal = true;
            }
            else if ((value1 is string && value2 is string) && ((string)value1 != (string)value2))
            {
                ifequal = true;
            }

            return ifequal;
        }

        static void IncVars()
        {
            foreach (Var v in vars)
            {
                v.scope++;
            }
        }

        static void DecVars()
        {
            foreach (Var v in vars)
            {
                v.scope--;
            }
        }

        static void DestroyVars()
        {
            foreach (Var v in vars)
            {
                if (v.scope == 1)
                {
                    vars.Remove(v);
                }
            }
        }
    }
}
