using Assistant;
using CEasyUO;
using System;
using System.Collections.Generic;
using System.Threading;


namespace KlipCompiler
{
    public class EUOParser
    {
        private List<Token> m_Tokens;
        private int CurrentIndex = 0;
        public int CurrentLine => m_Tokens != null ? CurrentToken.Line : 0;

        private Token CurrentToken => m_Tokens != null ? m_Tokens[CurrentIndex] : null;
        private Token NextToken => m_Tokens[CurrentIndex + 1];
        private Token LastToken => m_Tokens[CurrentIndex - 1];

        public string Script { get; internal set; }

        private Dictionary<string,int> Labels = new Dictionary<string, int>();
        private Dictionary<string, int> Subs = new Dictionary<string, int>();

        public static Dictionary<string,object> Variables = new Dictionary<string, object>();
        public EUOParser(string script )
        {
            Script = script;
            Lexer lexer = new KlipCompiler.Lexer() { InputString = script };

            List<Token> tokens = new List<Token>();
            int line = 0;
            int tok = 0;
            while ( true )
            {

                Token t = lexer.GetToken();
                if ( t == null )
                {
                    break;
                }
                if ( t.TokenName == Lexer.Tokens.NewLine )
                    line++;
                t.Line = line;
                if ( t.TokenName.ToString() == "Undefined" )
                {
                    throw new Exception( $"Undefined token: {t.TokenValue} " );
                }
                if ( t.TokenName.ToString() != "Whitespace" && t.TokenName.ToString() != "Undefined" )
                {
                    tokens.Add( t );
                }
                if(t.TokenName == Lexer.Tokens.Label)
                {
                    Labels.Add( t.TokenValue.TrimEnd( new[]{':'} ).ToLowerInvariant(), tok + 1 );
                }
                if ( t.TokenName == Lexer.Tokens.Function )
                {
                    Subs.Add( t.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant(), tok + 1 );
                }
                tok++;
            }
            m_Tokens = tokens;
            if ( m_Tokens == null )
                throw new Exception( "Parse Error" );
           // Player = PlayerMobile.GetPlayer();
        }

        public void Run()
        {
            while (CurrentIndex != m_Tokens.Count - 1)
            {
                if ( !Line() )
                    break;
              
                
            }
            //throw new NotImplementedException();
        }
        public bool Line()
        {
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                Step();
            if ( NextToken == null )
                return false;
            CurrentIndex++;
            return true;
        }

        public void Step()
        {
            switch ( CurrentToken.TokenName )
            {
                case Lexer.Tokens.Function:
                    //skip to end return;

                    break;
                case Lexer.Tokens.NewLine:
                    break;
                case Lexer.Tokens.Set:
                    Set();
                    break;
                case Lexer.Tokens.Label:

                    break;
                case Lexer.Tokens.Goto: // todo scan forward for labels.
                    CurrentIndex++;
                    if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
                        CurrentIndex = Labels[CurrentToken.TokenValue.ToLowerInvariant()];
                    else
                        throw new Exception( "Unhandled goto " + CurrentToken.TokenValue + " on line " + CurrentLine );
                    break;
                case Lexer.Tokens.Event:
                    CurrentIndex++;
                    if ( CurrentToken.TokenValue == "macro" )
                    {
                        CurrentIndex++;
                        EventMacro();
                    }
                    break;
                case Lexer.Tokens.Target:
                    CurrentIndex++;
                    if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
                    {
                        CurrentIndex++;
                    }
                    else
                        while ( !Targeting.HasTarget ) Thread.Sleep( 55 );

                    break;

                case Lexer.Tokens.Wait:
                    CurrentIndex++;
                    if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
                    {
                        Thread.Sleep( int.Parse( CurrentToken.TokenValue ) * 50 );
                        CurrentIndex++;
                    }
                    else if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
                    {
                        Thread.Sleep( int.Parse( CurrentToken.TokenValue.TrimEnd( new char[] { 's' } ) ) * 1000 );
                        CurrentIndex++;
                    }
                    else
                    {
                        Thread.Sleep( 1000 );
                    }

                    break;

                case Lexer.Tokens.If:
                    ParseIF();
                    break;
                case Lexer.Tokens.Else:
                    ParseElse();
                    break;
                case Lexer.Tokens.FindItem:
                    ParseFindItem();
                    break;
                case Lexer.Tokens.For:
                    ParseFor();
                    break;
                case Lexer.Tokens.Comment:
                    break;
                default:
                    Console.WriteLine( "Unhandled Token: " + CurrentToken.TokenName );
                    break;
            }
            CurrentIndex++;
        }

        private void ParseFor()
        {
            throw new NotImplementedException();
        }

        private void ParseFindItem()
        {
            CurrentIndex++;
            if(CurrentToken.TokenName != Lexer.Tokens.StringLiteral)
            {
                throw new Exception();
            }
            var ids = CurrentToken.TokenValue.Split( '_' );
            CurrentIndex++;
            uint container = 0;
            bool ground = false;
            if( CurrentToken.TokenName == Lexer.Tokens.StringLiteral ) {
                if ( CurrentToken.TokenValue.ToLower() == "g" || CurrentToken.TokenValue.ToLower() == "g_" )
                    ground = true;
                else if( CurrentToken.TokenValue.ToLower() == "c_" )
                {
                    CurrentIndex++;
                    if(CurrentToken.TokenName == Lexer.Tokens.Comma )
                    {
                        CurrentIndex++;
                        if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
                        {
                            container = Form1.EUO2StealthID( CurrentToken.TokenValue );  
                        }

                    }

                }
            }
           foreach(var idStr in ids )
            {
                if ( idStr.Length == 3 )
                {
                    var id = Form1.EUO2StealthType( idStr );
                    foreach ( var i in World.Items.Values )
                    {
                        if(i.ItemID == id && ((container != 0 && i.ContainerID == container ) || (ground && i.ContainerID == 0)) )
                        {
                            Setvariable( "#findid", Form1.uintToEUO(i.Serial.Value) );
                            Setvariable( "#findtype", Form1.uintToEUO( i.ItemID.Value ) );

                            Setvariable( "#findx", i.Position.X );
                            Setvariable( "#findy", i.Position.Y );
                            Setvariable( "#findz", i.Position.Z );

                        }
                    }
                    foreach ( var i in World.Mobiles.Values )
                    {
                        if ( i.Body == id )
                        {
                            Setvariable( "#findid", Form1.uintToEUO( i.Serial.Value ) );
                            Setvariable( "#findtype", Form1.uintToEUO( i.Body ) );

                            Setvariable( "#findx", i.Position.X );
                            Setvariable( "#findy", i.Position.Y );
                            Setvariable( "#findz", i.Position.Z );

                        }
                    }
                }
                else
                {
                    var id = Form1.EUO2StealthID( idStr );
                    if ( World.Items.ContainsKey( id ) )
                    {
                        Setvariable( "#findid", id );
                    }
                    else if ( World.Mobiles.ContainsKey( id ) )
                    {

                    }
                }
               
            }
        }
        private void ParseElse()
        {
            CurrentIndex++;
            (var startIndex, var EndIndex) = parseBlock();
            if ( IfStack.Pop() )
                CurrentIndex = EndIndex;
            else
                CurrentIndex = startIndex;
        }

        private Stack<bool> IfStack = new Stack<bool>();
        
        private void ParseIF()
        {
            CurrentIndex++;
            if ( CurrentToken.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
            }
            var lexpr = ParseExpr();

            var op = CurrentToken;  
            CurrentIndex++;
            var rexpr = ParseExpr();
            if ( CurrentToken.TokenName == Lexer.Tokens.RightParan )
            {
                CurrentIndex++;
            }
            (var startIf, var endIf) = parseBlock();
            /* if(CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else )
             {
                 (var startIndex, var EndIndex) = parseBlock();
             }*/
            var cnt = IfStack.Count;

            if(op.TokenName == Lexer.Tokens.Equal )
            {
                if ( lexpr.GetValue().Equals( rexpr.GetValue() ) )
                {
                    CurrentIndex = startIf;
                    if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.NotEqual )
            {
                if ( !lexpr.GetValue().Equals( rexpr.GetValue() ) )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.MoreOrEqual || op.TokenName == Lexer.Tokens.MoreOrEqual2 )
            {
                if ( (int)lexpr.GetValue() >= (int)rexpr.GetValue()  )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.LessOrEqual || op.TokenName == Lexer.Tokens.LessOrEqual2 )
            {
                if ( (int)lexpr.GetValue() <= (int)rexpr.GetValue() )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.More )
            {
                if ( (int)lexpr.GetValue() > (int)rexpr.GetValue() )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.Less )
            {
                if ( (int)lexpr.GetValue() < (int)rexpr.GetValue() )
                {
                    CurrentIndex = startIf; if( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }

            if ( cnt == IfStack.Count ) // false
                if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( false );

        }

        private (int,int) parseBlock()
        {
            //look for Int on current line or LeftBrace
            //Look for left brace as next statement
            // if neither found just next line

            var start = 0;
            var end = 0;
            int lineCnt = -1;
            var ifIndex = 0;
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
            {
                if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
                {
                    lineCnt = int.Parse( CurrentToken.TokenValue ) + CurrentLine;
                    start = CurrentIndex + 1;
                    while ( CurrentLine != lineCnt ) CurrentIndex++;
                    end = CurrentIndex;
                    break;
                }
                else if ( CurrentToken.TokenName == Lexer.Tokens.LeftBrace )
                {
                    start = CurrentIndex + 1;
                    while ( CurrentToken.TokenName != Lexer.Tokens.RightBrace )
                    {
                        CurrentIndex++;
                    }
                    CurrentIndex++;
                    end = CurrentIndex;
                    break;
                }
            }
            while ( CurrentToken.TokenName == Lexer.Tokens.NewLine ) CurrentIndex++;

            if ( ifIndex == 0 )
            {
                if ( CurrentToken.TokenName == Lexer.Tokens.LeftBrace )
                {
                    start = CurrentIndex + 1;
                    while ( CurrentToken.TokenName != Lexer.Tokens.RightBrace )
                    {
                        CurrentIndex++;
                    }
                    CurrentIndex++;
                    end = CurrentIndex;

                }
                else
                {
                    start = CurrentIndex + 1;

                    while ( CurrentToken.TokenName != Lexer.Tokens.NewLine ) CurrentIndex++;
                    end = CurrentIndex;

                }
            }
            return (start, end);
        }

        private void EventMacro()
        {
            if (CurrentToken.TokenName == Lexer.Tokens.IntLiteral)
            {
                int idOne = int.Parse(CurrentToken.TokenValue);
                int idTwo = 0;
                if (NextToken.TokenName == Lexer.Tokens.IntLiteral)
                {
                    CurrentIndex++;
                    idTwo = int.Parse(CurrentToken.TokenValue);
                }

                switch (idOne)
                {
                    case 22: // last target
                        var targ = Form1.EUO2StealthID(GetVariable<string>("#ltargetid"));
                         Targeting.Target( targ );
                        //Player.Targeting.TargetTo(targ);
                        break;
                    case 13:
                        ClientCommunication.SendToServer( new UseSkill( idTwo ) );
                        // Player.UseSkill(idTwo);
                        break;
                        
                }
                    
            }
            else
            {
                throw new Exception($"Unhandled event {CurrentToken.TokenValue} at line {CurrentLine}");
            }
        }
        public static T GetVariable<T>(string name)
        {
            name = name.ToLowerInvariant();

            switch ( name )
            {
                case "#lobjectid":
                    return (T)(object)Form1.uintToEUO( World.Player.LastObject.Value ).ToString();
                case "#lobjecttype":
                    return (T)(object)Form1.uintToEUO( World.Player.LastObjectAsEntity?.GraphicID ?? 0 ).ToString();
                case "#ltargetid":
                    return (T)(object)Form1.uintToEUO( Targeting.LastTarget?.Serial ?? 0 ).ToString();
                case "#ltargetx":
                    return (T)(object)(Targeting.LastTarget?.X.ToString() ?? "X");
                case "#ltargety":
                    return (T)(object)Targeting.LastTarget?.Y.ToString();
                case "#ltargetz":
                    return (T)(object)Targeting.LastTarget?.Z.ToString();
                case "#ltargetkind":
                    return (T)(object)( Targeting.LastTarget?.Gfx ?? 0 ).ToString();
                case "#ltargettile":
                    return (T)(object)(Targeting.LastTarget?.Gfx ?? 0 ).ToString();
                case "#lskill":
                    return (T)(object)World.Player.LastSkill.ToString();
                case "#lspell":
                    return (T)(object)World.Player.LastSpell.ToString();
            }
         

            if (! Variables.ContainsKey( name ) )
            {
                Setvariable( name, "X" );
            }
            var res = Variables[name];
            if ( res is T result )
                return result;
           if(typeof(T) == typeof(string) )
            {
                return (T)(object)res.ToString();
            }
            return (T)res ;
        }

        public static void Setvariable(string key, object value )
        {
            key = key.ToLowerInvariant();
            if ( Variables.ContainsKey( key ) )
                Variables[key] = value;
            else
                Variables.Add( key, value );
            if(value.ToString() != "x")
                switch ( key )
                {
                    case "#lastobjectid":
                        World.Player.LastObject = Form1.EUO2StealthID( value.ToString() );
                        break;
                    case "#lasttargetid":
                        Targeting.LastTarget.TargID = Form1.EUO2StealthID( value.ToString() );
                        break;
                }
        }
        private void Set()
        {
            CurrentIndex++;
            var variableName = ParseExpr();
            string varName = "";
            var value = ParseExpr().GetValue();
            if ( variableName is Ident i )
            {
                varName = i.value.ToLowerInvariant();
                Setvariable( varName, value );
            }
            else
            {
                varName = variableName.GetValue().ToString().ToLowerInvariant();
                Setvariable( varName, value );
            }
           


        }
        
        Expr ParseExpr()
        {
            Expr ret = null;
            Token t = CurrentToken;

            if (NextToken.TokenName == Lexer.Tokens.LeftParan)
            {
               /* string ident = "";

                if (t.TokenName == Lexer.Tokens.Ident || t.TokenName == Lexer.Tokens.BuildInIdent)
                {
                    ident = t.TokenValue.ToString();
                }

                CurrentIndex++;

                if (NextToken.TokenName == Lexer.Tokens.RightParan)
                {
                    ret = new CallExpr(ident, new List<Expr>());
                }
                else
                {
                    //ret = new CallExpr(ident, ParseCallArgs());
                }*/
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
            else if (t.TokenName == Lexer.Tokens.Ident || t.TokenName == Lexer.Tokens.BuildInIdent)
            {
                string ident = t.TokenValue.ToString();

                Ident i = new Ident(ident);
                ret = i;
            }
            else if (t.TokenName == Lexer.Tokens.LeftParan)
            {
                Expr e = ParseExpr();

                if (NextToken.TokenName == Lexer.Tokens.RightParan)
                {
                    CurrentIndex++;
                }

                ParanExpr p = new ParanExpr(e);

                if (NextToken.TokenName == Lexer.Tokens.Add)
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.add, expr);
                }
                else if (NextToken.TokenName == Lexer.Tokens.Sub)
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.sub, expr);
                }
                else if (NextToken.TokenName == Lexer.Tokens.Mul)
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.mul, expr);
                }
                else if (NextToken.TokenName == Lexer.Tokens.Div)
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr(p, Symbol.div, expr);
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Comma )
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.Concat, expr );
                }
                else
                {
                    ret = p;
                }
            }

            if (NextToken.TokenName == Lexer.Tokens.Add || NextToken.TokenName == Lexer.Tokens.Sub || NextToken.TokenName == Lexer.Tokens.Mul || NextToken.TokenName == Lexer.Tokens.Div || NextToken.TokenName == Lexer.Tokens.Comma )
            {
                Expr lexpr = ret;
                Symbol op = 0;

                if (NextToken.TokenName == Lexer.Tokens.Add)
                {
                    op = Symbol.add;
                }
                else if (NextToken.TokenName == Lexer.Tokens.Sub)
                {
                    op = Symbol.sub;
                }
                else if (NextToken.TokenName == Lexer.Tokens.Mul)
                {
                    op = Symbol.mul;
                }
                else if (NextToken.TokenName == Lexer.Tokens.Div)
                {
                    op = Symbol.div;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Comma )
                {
                    op = Symbol.Concat;
                }
                CurrentIndex++;
                CurrentIndex++;
                Expr rexpr = ParseExpr();

                ret = new MathExpr(lexpr, op, rexpr);
            }

            CurrentIndex++;
            return ret;
        }
    }
}