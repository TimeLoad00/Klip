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

                if (tok.TokenName == Lexer.Tokens.Import)
                {
                    Program.imports.Add(ParseImport());
                }
                else if (tok.TokenName == Lexer.Tokens.Function)
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
                else if (tok.TokenName == Lexer.Tokens.If)
                {
                    IfBlock ifblock = ParseIf();

                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = ifblock;
                    }
                }
                else if (tok.TokenName == Lexer.Tokens.ElseIf)
                {
                    ElseIfBlock elseifblock = ParseElseIf();

                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = elseifblock;
                    }
                }
                else if (tok.TokenName == Lexer.Tokens.Else)
                {
                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new ElseBlock();
                    }
                }
                else if (tok.TokenName == Lexer.Tokens.Repeat)
                {
                    if (currentBlock != null)
                    {
                        blockstack.Push(currentBlock);
                        currentBlock = new RepeatBlock();
                    }
                }
                else if (tok.TokenName == Lexer.Tokens.Ident)
                {
                    if (tokens.Peek().TokenName == Lexer.Tokens.Equal)
                    {
                        tokens.pos--;
                        Assign a = ParseAssign();
                        currentBlock.AddStmt(a);
                    }
                    else if (tokens.Peek().TokenName == Lexer.Tokens.LeftParan)
                    {
                        tokens.pos--;
                        Call c = ParseCall();
                        currentBlock.AddStmt(c);
                    }
                }
                else if (tok.TokenName == Lexer.Tokens.Return)
                {
                    Return r = ParseReturn();
                    currentBlock.AddStmt(r);
                }
                else if (tok.TokenName == Lexer.Tokens.RightParan)
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
                else if (tok.TokenName == Lexer.Tokens.EOF)
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

            if (t.TokenName == Lexer.Tokens.Ident)
            {
                ret = t.TokenValue;
            }

            return ret;
        }

        static Func ParseFunc()
        {
            string ident = "";
            List<string> vars = new List<string>();

            if (tokens.Peek().TokenName == Lexer.Tokens.Ident)
            {
                ident = tokens.GetToken().TokenValue.ToString();
            }
            
            if (tokens.Peek().TokenName == Lexer.Tokens.LeftParan)
            {
                tokens.pos++;
            }

            if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
            {
                tokens.pos++;
            }
            else
            {
                vars = ParseFuncArgs();
            }

            if (tokens.Peek().TokenName == Lexer.Tokens.LeftBrace)
            {
                tokens.pos++;
            }

            return new Func(ident, vars);
        }

        static IfBlock ParseIf()
        {
            IfBlock ret = null;
            Symbol op = 0;

            if (tokens.Peek().TokenName == Lexer.Tokens.LeftParan)
            {
                tokens.pos++;
            }

            Expr lexpr = ParseExpr();

            if (tokens.Peek().TokenName == Lexer.Tokens.DoubleEqual)
            {
                op = Symbol.doubleEqual;
                tokens.pos++;
            }
            else if (tokens.Peek().TokenName == Lexer.Tokens.NotEqual)
            {
                op = Symbol.notEqual;
                tokens.pos++;
            }

            Expr rexpr = ParseExpr();

            if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
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

            if (tokens.Peek().TokenName == Lexer.Tokens.LeftParan)
            {
                tokens.pos++;
            }

            Expr lexpr = ParseExpr();

            if (tokens.Peek().TokenName == Lexer.Tokens.DoubleEqual)
            {
                op = Symbol.doubleEqual;
                tokens.pos++;
            }
            else if (tokens.Peek().TokenName == Lexer.Tokens.NotEqual)
            {
                op = Symbol.notEqual;
                tokens.pos++;
            }

            Expr rexpr = ParseExpr();

            if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
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

            if (tok.TokenName == Lexer.Tokens.Ident)
            {
                ident = tok.TokenValue.ToString();
            }

            if (tokens.Peek().TokenName == Lexer.Tokens.LeftParan)
            {
                tokens.pos++;
            }

            if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
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

            if (tokens.Peek().TokenName == Lexer.Tokens.LeftParan)
            {
                string ident = "";

                if (t.TokenName == Lexer.Tokens.Ident)
                {
                    ident = t.TokenValue.ToString();
                }

                tokens.pos++;

                if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
                {
                    ret = new CallExpr(ident, new List<Expr>());
                }
                else
                {
                    ret = new CallExpr(ident, ParseCallArgs());
                }
            }
            else if (t.TokenName == Lexer.Tokens.IntLiteral)
            {
                IntLiteral i = new IntLiteral(Convert.ToInt32(t.TokenValue.ToString()));
                ret = i;
            }
            else if (t.TokenName == Lexer.Tokens.StringLiteral)
            {
                StringLiteral s = new StringLiteral(t.TokenValue.ToString());
                ret = s;
            }
            else if (t.TokenName == Lexer.Tokens.Ident)
            {
                string ident = t.TokenValue.ToString();

                Ident i = new Ident(ident);
                ret = i;
            }
            else if (t.TokenName == Lexer.Tokens.LeftParan)
            {
                Expr e = ParseExpr();

                if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
                {
                    tokens.pos++;
                }

                ParanExpr p = new ParanExpr(e);

                if (tokens.Peek().TokenName == Lexer.Tokens.Add)
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.add, expr);
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.Sub)
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.sub, expr);
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.Mul)
                {
                    tokens.pos++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.mul, expr);
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.Div)
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

            if (tokens.Peek().TokenName == Lexer.Tokens.Add || tokens.Peek().TokenName == Lexer.Tokens.Sub || tokens.Peek().TokenName == Lexer.Tokens.Mul || tokens.Peek().TokenName == Lexer.Tokens.Div)
            {
                Expr lexpr = ret;
                Symbol op = 0;

                if (tokens.Peek().TokenName == Lexer.Tokens.Add)
                {
                    op = Symbol.add;
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.Sub)
                {
                    op = Symbol.sub;
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.Mul)
                {
                    op = Symbol.mul;
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.Div)
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

                if (tok.TokenName == Lexer.Tokens.Ident)
                {
                    ret.Add(tok.TokenValue.ToString());
                }

                if (tokens.Peek().TokenName == Lexer.Tokens.Comma)
                {
                    tokens.pos++;
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
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

                if (tokens.Peek().TokenName == Lexer.Tokens.Comma)
                {
                    tokens.pos++;
                }
                else if (tokens.Peek().TokenName == Lexer.Tokens.RightParan)
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
