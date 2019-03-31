using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlipCompiler
{
    class Stmt { }

    abstract class Expr
    {
        public abstract object GetValue();
       
    }

    class Block : Stmt
    {
        public List<Stmt> statements;

        public Block()
        {
            statements = new List<Stmt>();
        }

        public void AddStmt(Stmt stmt)
        {
            statements.Add(stmt);
        }
    }

    class Func : Block
    {
        public string ident;
        public List<string> vars;

        public Func(string i, List<string> v)
        {
            ident = i;
            vars = v;
        }
    }

    class IfBlock : Block
    {
        public Expr leftExpr;
        public Symbol op;
        public Expr rightExpr;

        public IfBlock(Expr lexpr, Symbol o, Expr rexpr)
        {
            leftExpr = lexpr;
            op = o;
            rightExpr = rexpr;
        }
    }

    class ElseIfBlock : Block
    {
        public Expr leftExpr;
        public Symbol op;
        public Expr rightExpr;

        public ElseIfBlock(Expr lexpr, Symbol o, Expr rexpr)
        {
            leftExpr = lexpr;
            op = o;
            rightExpr = rexpr;
        }
    }

    class ElseBlock : Block { }

    class EndIf : Block { }

    class RepeatBlock : Block { }

    class Assign : Stmt
    {
        public string ident;
        public Expr value;

        public Assign(string i, Expr v)
        {
            ident = i;
            value = v;
        }
    }

    class Call : Stmt
    {
        public string ident;
        public List<Expr> args;

        public Call(string i, List<Expr> a)
        {
            ident = i;
            args = a;
        }
    }

    class Return : Stmt
    {
        public Expr expr;

        public Return(Expr e)
        {
            expr = e;
        }
    }

    class IntLiteral : Expr
    {
        public int value;

        public IntLiteral(int v)
        {
            value = v;
        }

        public override object GetValue()
        {
            return value;
        }
    }

    class StringLiteral : Expr
    {
        public string value;

        public StringLiteral(string v)
        {
            value = v;
        }
        public override object GetValue()
        {
            return value;
        }
    }

    class Ident : Expr
    {
        public string value;

        public Ident(string v)
        {
            value = v;
        }

        public override object GetValue()
        {
            return EUOParser.Variables[value.ToLowerInvariant()];
        }
    }

    class MathExpr : Expr
    {
        public Expr leftExpr;
        public Symbol op;
        public Expr rightExpr;

        public MathExpr(Expr lexpr, Symbol o, Expr rexpr)
        {
            leftExpr = lexpr;
            op = o;
            rightExpr = rexpr;
        }
        public override object GetValue()
        {
            switch (op)
            {
                case Symbol.add:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValue() + (int)rightExpr.GetValue();
                    else
                        return (string)leftExpr.GetValue() + (string)rightExpr.GetValue();
                case Symbol.sub:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValue() - (int)rightExpr.GetValue();
                    break;
                case Symbol.mul:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValue() - (int)rightExpr.GetValue();
                    break;
                case Symbol.div:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValue() - (int)rightExpr.GetValue();
                    break;
                case Symbol.Concat:
                    return leftExpr.GetValue().ToString() + rightExpr.GetValue().ToString();

            }
         throw new NotSupportedException();   
        }
    }

    class ParanExpr : Expr
    {
        public Expr value;

        public ParanExpr(Expr v)
        {
            value = v;
        }

        public override object GetValue()
        {
            return value.GetValue();
        }
    }

    class CallExpr : Expr
    {
        public string ident;
        public List<Expr> args;

        public CallExpr(string i, List<Expr> a)
        {
            ident = i;
            args = a;
        }
        public override object GetValue()
        {
            throw new NotImplementedException();
        }
    }

    enum Symbol
    {
        add = 0,
        sub = 1,
        mul = 2,
        div = 3,
        equal = 4,
        doubleEqual = 5,
        notEqual = 6,
        leftParan = 7,
        rightParan = 8,
        leftBrace = 9,
        rightbrace = 10,
        Concat = 11
    }
}
