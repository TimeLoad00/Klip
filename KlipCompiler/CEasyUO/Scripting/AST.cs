using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEasyUO
{
    public class Stmt {
        public int Line = 0;
        public virtual bool Execute() {
            return true;
        }
    }

    abstract class Expr
    {
        public abstract object GetValue();
       
    }

    public class Block : Stmt
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

        public override bool Execute()
        {
            
            return base.Execute();
        }
    }
    class FindItemStmt : Stmt
    {
        public List<uint> FindIDs = new List<uint>();
        public List<ushort> FindTypes = new List<ushort>();
        public int Index;
        public bool GroundOnly = false;
        public bool ContainerOnly = false;
        public uint ContainerSerial;

        private Expr Filter;
        private Expr Find;

        public FindItemStmt( Expr idType, IntLiteral index, Expr filter )
        {
            Find = idType;
          
            if(index != null)
                Index = index.value;
            Filter = filter;
        }
        public override bool Execute()
        {
            var ids = Find.GetValue().ToString().Split( new[] { '_' } );
            foreach ( var id in ids )
            {
                if ( id.Length == 3 )
                    FindTypes.Add( Form1.EUO2StealthType( id ) );
                else
                    FindTypes.Add( Form1.EUO2StealthType( id ) );
            }
            if ( Filter != null )
            {
                var str = Filter.GetValue().ToString().Trim();
                ContainerOnly = ( str.Contains( "C" ) );
                GroundOnly = ( str.Contains( "G" ) );
                try
                {
                    var id = str.Split( '_' )[1];
                    ContainerSerial = Form1.EUO2StealthID( id );
                }
                catch { }
            }

            return base.Execute();
        }
    }
    class EventStmt : Stmt
    {
        public string EventType;
        public List<Expr> Params;
        public EventStmt( string eventType, List<Expr> paras )
        {
            EventType = eventType;
            Params = paras;
        }
    }

    class Goto : Stmt
    {
        public string Name;
        public Goto( string name )
        {
            Name = name;
        }
    }

    class Label : Stmt
    {
        public string Name;
        public Label(string name)
        {
            Name = name;
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

    class ForBlock : Block
    {
        public Expr From;
        public Ident Var;
        public Expr To;


        public ForBlock( Ident var, Expr from, Expr to )
        {
            From = from;
            Var = var;
            To = to;
        }
        public override bool Execute()
        {
            return base.Execute();
        }
    }

    class IfBlock : Block
    {
        public Expr leftExpr;
        public Lexer.Tokens op;
        public Expr rightExpr;
        

        public IfBlock(Expr lexpr, Lexer.Tokens o, Expr rexpr)
        {
            leftExpr = lexpr;
            op = o;
            rightExpr = rexpr;
        }
        public override bool Execute()
        {
            return base.Execute();
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
        public Expr ident;
        public Expr value;

        public Assign( Expr i, Expr v)
        {
            ident = i;
            value = v;
        }
        public override bool Execute()
        {
            string varName = "";
            if ( ident is Ident i )
            {
                varName = i.value.ToLowerInvariant();
                EUOInterpreter.Setvariable( varName, value );
            }
            else
            {
                varName = ident.GetValue().ToString().ToLowerInvariant();
                EUOInterpreter.Setvariable( varName, value );
            }
            return base.Execute();
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
            return EUOInterpreter.GetVariable<string>(value.ToLowerInvariant());
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
