using Assistant;
using CEasyUO;
using System;
using System.Collections.Generic;
using System.Threading;


namespace CEasyUO
{
    public class EUOInterpreter
    {
        private List<Stmt> m_Statements = new List<Stmt>();
        private Stmt CurrentStatment;

        private Dictionary<string,int> Labels = new Dictionary<string, int>();
        private Dictionary<string, int> Subs = new Dictionary<string, int>();

        public static Dictionary<string,object> Variables = new Dictionary<string, object>();
        public EUOInterpreter( string script )
        {
            var parser = new EUOParser( script );
            m_Statements = parser.GenerateAST();
        }

    

        public void Run()
        {
            for( int i = 0;i < m_Statements.Count; )
            {
                //if ( Step( m_Statements[i] ) )
               //     i++;
            }
          
            //throw new NotImplementedException();
        }
       

        internal void Step(Stmt st)
        {
           /* switch ( CurrentToken.TokenName )
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
            CurrentIndex++;*/
        }

        private void ParseFor()
        {
            throw new NotImplementedException();
        }

      

        private Stack<bool> IfStack = new Stack<bool>();
        
    /*    private void ParseIF()
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
    */
      

        private void EventMacro()
        {
          /*  if (CurrentToken.TokenName == Lexer.Tokens.IntLiteral)
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
            }*/
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
          /*  CurrentIndex++;
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
           
    */

        }
        
     
    }
}