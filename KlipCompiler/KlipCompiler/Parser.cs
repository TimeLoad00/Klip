using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipCompiler
{
    class Parser
    {
        static TokenList tokens;
        static Block currentBlock;
        static Stack<Block> blockstack;
        static List<Stmt> tree;
        static bool running;

        public Parser(TokenList t)
        {
            tokens = t;
            currentBlock = null;
            blockstack = new Stack<Block>();
            Token tok = null;
            tree = new List<Stmt>();
            running = true;

            while (running)
            {
                try
                {
                    tok = tokens.GetToken();
                }
                catch { }

                if (tok.TokenName.ToString() == "Import")
                {
                    Program.imports.Add(ParseImport());
                }
                else if (tok.TokenName.ToString() == "Function")
                {
                    Func func = ParseFunc();

                    if (currentBlock == null)
                    {
                        currentBlock = func;
                    }
                    else
                    {
                        currentBlock.AddStmt(new Return(null));
                        tree.Add(currentBlock);
                        currentBlock = func;
                    }
                }
                else if (tok.TokenName.ToString() == "If")
                {
                    IfBlock ifblock = ParseIf();

                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = ifblock;
                    }
                }
                else if (tok.TokenName.ToString() == "ElseIf")
                {
                    ElseIfBlock elseifblock = ParseElseIf();

                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = elseifblock;
                    }
                }
                else if (tok.TokenName.ToString() == "Else")
                {
                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new ElseBlock();
                    }
                }
                else if (tok.TokenName.ToString() == "Repeat")
                {
                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new RepeatBlock();
                    }
                }
                else if (tok.TokenName.ToString() == "Ident")
                {
                    if (tokens.Peek().TokenName.ToString() == "Equal")
                    {
                        tokens.pos--;
                        Assign a = ParseAssign();
                        currentBlock.AddStmt(a);
                    }
                    else if (tokens.Peek().TokenName.ToString() == "LeftParan")
                    {
                        tokens.pos--;
                        Call c = ParseCall();
                        currentBlock.AddStmt(c);
                    }
                }
                else if (tok.TokenName.ToString() == "Return")
                {
                    Return r = ParseReturn();
                    currentBlock.AddStmt(r);
                }
                else if (tok.TokenName.ToString() == "RightBrace")
                {
                    if (currentBlock is Func)
                    {
                        currentBlock.AddStmt(new Return(null));
                        tree.Add(currentBlock);
                        currentBlock = null;
                    }
                    else if (currentBlock is IfBlock || currentBlock is ElseIfBlock || currentBlock is ElseBlock)
                    {
                        currentBlock.AddStmt(new EndIf());
                        Block block = currentBlock;

                        if (blockstack.Count > 0)
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt(block);
                        }
                    }
                    else if (currentBlock is RepeatBlock)
                    {
                        Block block = currentBlock;

                        if (blockstack.Count > 0)
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt(block);
                        }
                    }
                }
                else if (tok.TokenName.ToString() == "EOF")
                {
                    tree.Add(currentBlock);
                    running = false;
                }
            }
        }

        static string ParseImport()
        {
            string ret = "";
            Token t = tokens.GetToken();

            if (t.TokenName.ToString() == "Ident")
            {
                ret = t.TokenValue;
            }

            return ret;
        }

        static Func ParseFunc()
        {
            string ident = "";
            List<string> vars = new List<string>();

            if (tokens.Peek().TokenName.ToString() == "Ident")
            {
                ident = tokens.GetToken().TokenValue.ToString();
            }
            
            if (tokens.Peek().TokenName.ToString() == "LeftParan")
            {
                tokens.pos++;
            }

            if (tokens.Peek().TokenName.ToString() == "RightParan")
            {
                tokens.pos++;
            }
            else
            {
                vars = ParseFuncArgs();
            }

            if (tokens.Peek().TokenName.ToString() == "LeftBrace")
            {
                tokens.pos++;
            }

            return new Func(ident, vars);
        }

        static IfBlock ParseIf()
        {
            IfBlock ret = null;
            Symbol op = 0;

            if (tokens.Peek().TokenName.ToString() == "LeftParan")
            {
                tokens.pos++;
            }

            Expr lexpr = ParseExpr();

            if (tokens.Peek().TokenName.ToString() == "DoubleEqual")
            {
                op = Symbol.doubleEqual;
                tokens.pos++;
            }
            else if (tokens.Peek().TokenName.ToString() == "NotEqual")
            {
                op = Symbol.notEqual;
                tokens.pos++;
            }

            Expr rexpr = ParseExpr();

            if (tokens.Peek().TokenName.ToString() == "RightParan")
            {
                tokens.pos++;
            }

            ret = new IfBlock(lexpr, op, rexpr);

            return ret;
        }

        static ElseIfBlock ParseElseIf()
        {
            ElseIfBlock ret = null;
            Symbol op = 0;

            if (tokens.Peek().TokenName.ToString() == "LeftParan")
            {
                tokens.pos++;
            }

            Expr lexpr = ParseExpr();

            if (tokens.Peek().TokenName.ToString() == "DoubleEqual")
            {
                op = Symbol.doubleEqual;
                tokens.pos++;
            }
            else if (tokens.Peek().TokenName.ToString() == "NotEqual")
            {
                op = Symbol.notEqual;
                tokens.pos++;
            }

            Expr rexpr = ParseExpr();

            if (tokens.Peek().TokenName.ToString() == "RightParan")
            {
                tokens.pos++;
            }

            ret = new ElseIfBlock(lexpr, op, rexpr);

            return ret;
        }

        static Assign ParseAssign()
        {
            Assign ret = null;
            string ident = "";

            Token t = tokens.GetToken();
            ident = t.TokenValue.ToString();

            tokens.pos++;

            Expr value = ParseExpr();

            ret = new Assign(ident, value);

            return ret;
        }

        static Call ParseCall()
        {
            string ident = "";
            Token tok = tokens.GetToken();
            List<Expr> args = new List<Expr>();

            if (tok.TokenName.ToString() == "Ident")
            {
                ident = tok.TokenValue.ToString();
            }

            if (tokens.Peek().TokenName.ToString() == "LeftParan")
            {
                tokens.pos++;
            }

            if (tokens.Peek().TokenName.ToString() == "RightParan")
            {
                tokens.pos++;
            }
            else
            {
                args = ParseCallArgs();
            }

            return new Call(ident, args);
        }

        static Return ParseReturn()
        {
            return new Return(ParseExpr());
        }

        static Expr ParseExpr()
        {
            Expr ret = null;
            Token t = tokens.GetToken();

            if (tokens.Peek().TokenName.ToString() == "LeftParan")
            {
                string ident = "";

                if (t.TokenName.ToString() == "Ident")
                {
                    ident = t.TokenValue.ToString();
                }

                tokens.pos++;

                if (tokens.Peek().TokenName.ToString() == "RightParan")
                {
                    ret = new CallExpr(ident, new List<Expr>());
                }
                else
                {
                    ret = new CallExpr(ident, ParseCallArgs());
                }
            }
            else if (t.TokenName.ToString() == "IntLiteral")
            {
                IntLiteral i = new IntLiteral(Convert.ToInt32(t.TokenValue.ToString()));
                ret = i;
            }
            else if (t.TokenName.ToString() == "StringLiteral")
            {
                StringLiteral s = new StringLiteral(t.TokenValue.ToString());
                ret = s;
            }
            else if (t.TokenName.ToString() == "Ident")
            {
                string ident = t.TokenValue.ToString();

                Ident i = new Ident(ident);
                ret = i;
            }
            else if (t.TokenName.ToString() == "LeftParan")
            {
                Expr e = ParseExpr();

                if (tokens.Peek().TokenName.ToString() == "RightParan")
                {
                    tokens.pos++;
                }

                ParanExpr p = new ParanExpr(e);

                if (tokens.Peek().TokenName.ToString() == "Add")
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.add, expr);
                }
                else if (tokens.Peek().TokenName.ToString() == "Sub")
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.sub, expr);
                }
                else if (tokens.Peek().TokenName.ToString() == "Mul")
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.mul, expr);
                }
                else if (tokens.Peek().TokenName.ToString() == "Div")
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.div, expr);
                }
                else
                {
                    ret = p;
                }
            }

            if (tokens.Peek().TokenName.ToString() == "Add" || tokens.Peek().TokenName.ToString() == "Sub" || tokens.Peek().TokenName.ToString() == "Mul" || tokens.Peek().TokenName.ToString() == "Div")
            {
                Expr lexpr = ret;
                Symbol op = 0;

                if (tokens.Peek().TokenName.ToString() == "Add")
                {
                    op = Symbol.add;
                }
                else if (tokens.Peek().TokenName.ToString() == "Sub")
                {
                    op = Symbol.sub;
                }
                else if (tokens.Peek().TokenName.ToString() == "Mul")
                {
                    op = Symbol.mul;
                }
                else if (tokens.Peek().TokenName.ToString() == "Div")
                {
                    op = Symbol.div;
                }

                tokens.pos++;

                Expr rexpr = ParseExpr();

                ret = new MathExpr(lexpr, op, rexpr);
            }

            return ret;
        }

        static List<string> ParseFuncArgs()
        {
            List<string> ret = new List<string>();

            while (true)
            {
                Token tok = tokens.GetToken();

                if (tok.TokenName.ToString() == "Ident")
                {
                    ret.Add(tok.TokenValue.ToString());
                }

                if (tokens.Peek().TokenName.ToString() == "Comma")
                {
                    tokens.pos++;
                }
                else if (tokens.Peek().TokenName.ToString() == "RightParan")
                {
                    tokens.pos++;
                    break;
                }
            }

            return ret;
        }

        static List<Expr> ParseCallArgs()
        {
            List<Expr> ret = new List<Expr>();

            while (true)
            {
                ret.Add(ParseExpr());

                if (tokens.Peek().TokenName.ToString() == "Comma")
                {
                    tokens.pos++;
                }
                else if (tokens.Peek().TokenName.ToString() == "RightParan")
                {
                    tokens.pos++;
                    break;
                }
            }

            return ret;
        }

        public List<Stmt> GetTree()
        {
            return tree;
        }
    }
}
