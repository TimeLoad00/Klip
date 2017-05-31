using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipCompiler
{
    class Compiler
    {
        static string code;
        static int repeats = 0;

        public Compiler(List<Stmt> list)
        {
            code = "";
            CompileStmtList(list);
        }
        
        static void CompileStmtList(List<Stmt> statements)
        {
            foreach (Stmt s in statements)
            {
                if (s is Func)
                {
                    CompileFunc((Func)s);
                }
                else if (s is IfBlock)
                {
                    CompileIf((IfBlock)s);
                }
                else if (s is ElseIfBlock)
                {
                    CompileElseIf((ElseIfBlock)s);
                }
                else if (s is ElseBlock)
                {
                    CompileElse((ElseBlock)s);
                }
                else if (s is EndIf)
                {
                    Write("endif");
                }
                else if (s is RepeatBlock)
                {
                    CompileRepeat((RepeatBlock)s);
                }
                else if (s is Assign)
                {
                    CompileAssign((Assign)s);
                }
                else if (s is Call)
                {
                    CompileCall((Call)s);
                }
                else if (s is Return)
                {
                    if (((Return)s).expr == null)
                    {
                        Write("ret");
                    }
                    else
                    {
                        CompileExpr(((Return)s).expr);
                        Write("ret");
                    }
                }
            }
        }

        static void CompileFunc(Func data)
        {
            Write(":" + data.ident);

            foreach (string s in data.vars)
            {
                Write("setVar " + s);
            }

            CompileStmtList(data.statements);
        }

        static void CompileIf(IfBlock data)
        {
            CompileExpr(data.leftExpr);
            CompileExpr(data.rightExpr);

            if (data.op == Symbol.doubleEqual)
            {
                Write("ife");
            }
            else if (data.op == Symbol.notEqual)
            {
                Write("ifn");
            }

            CompileStmtList(data.statements);
        }

        static void CompileElseIf(ElseIfBlock data)
        {
            CompileExpr(data.leftExpr);
            CompileExpr(data.rightExpr);

            if (data.op == Symbol.doubleEqual)
            {
                Write("elseife");
            }
            else if (data.op == Symbol.notEqual)
            {
                Write("elseifn");
            }

            CompileStmtList(data.statements);
        }

        static void CompileElse(ElseBlock data)
        {
            Write("else");
            CompileStmtList(data.statements);
        }

        static void CompileRepeat(RepeatBlock data)
        {
            string name = ".repeat" + repeats.ToString();
            repeats++;
            Write(name);
            CompileStmtList(data.statements);
            Write("goto " + name);
        }

        static void CompileAssign(Assign data)
        {
            CompileExpr(data.value);
            Write("setVar " + data.ident);
        }

        static void CompileCall(Call data)
        {
            data.args.Reverse();

            foreach (Expr e in data.args)
            {
                CompileExpr(e);
            }

            Write("call " + data.ident);
        }

        static void CompileExpr(Expr data)
        {
            if (data is IntLiteral)
            {
                Write("pushInt " + ((IntLiteral)data).value);
            }
            else if (data is StringLiteral)
            {
                Write("pushString " + ((StringLiteral)data).value);
            }
            else if (data is Ident)
            {
                Write("pushVar " + ((Ident)data).value);
            }
            else if (data is CallExpr)
            {
                foreach (Expr e in ((CallExpr)data).args)
                {
                    CompileExpr(e);
                }

                Write("call " + ((CallExpr)data).ident);
            }
            else if (data is MathExpr)
            {
                CompileExpr(((MathExpr)data).leftExpr);
                CompileExpr(((MathExpr)data).rightExpr);

                if (((MathExpr)data).op == Symbol.add)
                {
                    Write("add");
                }
                else if (((MathExpr)data).op == Symbol.sub)
                {
                    Write("sub");
                }
                else if (((MathExpr)data).op == Symbol.mul)
                {
                    Write("mul");
                }
                else if (((MathExpr)data).op == Symbol.div)
                {
                    Write("div");
                }
            }
            else if (data is ParanExpr)
            {
                CompileExpr(((ParanExpr)data).value);
            }
        }

        static void Write(string data)
        {
            if (code == "")
            {
                code += data;
            }
            else
            {
                code += "\n" + data;
            }
        }

        public string GetCode()
        {
            return code;
        }
    }
}
