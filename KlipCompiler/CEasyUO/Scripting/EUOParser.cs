using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEasyUO
{
    class EUOParser
    {
        private List<Token> m_Tokens;
        private int CurrentIndex = 0;
        public int CurrentLine => m_Tokens != null ? CurrentToken.Line : 0;

        private Token CurrentToken => m_Tokens.Count >= CurrentIndex ? m_Tokens[CurrentIndex] : null;
        private Token NextToken => m_Tokens[CurrentIndex + 1];
        private Token LastToken => m_Tokens[CurrentIndex - 1];
        public string Script { get; internal set; }

        public EUOParser(string script )
        {
            Script = script;
            Lexer lexer = new CEasyUO.Lexer() { InputString = script };

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

                t.Line = line;
                if ( t.TokenName == Lexer.Tokens.NewLine )
                    line++;
                if ( t.TokenName.ToString() == "Undefined" )
                {
                    var rrr = tokens.Last();
                    throw new Exception( $"Undefined token: {t.TokenValue} " );
                }
                if ( t.TokenName.ToString() != "Whitespace" && t.TokenName.ToString() != "Undefined" )
                {
                    tokens.Add( t );
                }
                if ( t.TokenName == Lexer.Tokens.Label )
                {
                    //Labels.Add( t.TokenValue.TrimEnd( new[]{':'} ).ToLowerInvariant(), tok + 1 );
                }
                if ( t.TokenName == Lexer.Tokens.Function )
                {
                    //Subs.Add( t.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant(), tok + 1 );
                }
                tok++;
            }
            m_Tokens = tokens;
            m_Tokens.Add( new Token( Lexer.Tokens.EOF, "" ) );
            if ( m_Tokens == null || m_Tokens.Count <= 1 )
                throw new Exception( "Parse Error" );
            // Player = PlayerMobile.GetPlayer();
           // GenerateAST();
        }

        public List<Stmt> GenerateAST()
        {
            Block currentBlock = new Block();
            var blockstack = new Stack<Block>();
            Token tok = null;
            var tree = new List<Stmt>();
            var running = true;

            while ( running )
            {
                try
                {
                    tok = CurrentToken;
                    CurrentIndex++;
                    if ( tok == null )
                        break;
                }
                catch { }
                if ( currentBlock is IfBlock ifb )
                {
                    if ( IfEndsAtThisLine != -1 && tok.Line > IfEndsAtThisLine )
                    {
                        currentBlock.AddStmt( new EndIf() );
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                }

                switch ( tok.TokenName )
                {
                    case Lexer.Tokens.Call:
                        currentBlock.AddStmt( ParseCall() );
                        break;
                    case Lexer.Tokens.Label:
                        currentBlock.AddStmt( new Label( tok.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant() ) { Line = tok.Line } );
                        CurrentIndex++;
                        break;
                    case Lexer.Tokens.Goto:
                        {
                            currentBlock.AddStmt( new Goto( CurrentToken.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant() ) { Line = tok.Line } );
                            CurrentIndex++;

                            Block block = currentBlock;

                           /* if ( blockstack.Count > 0 && block is Func )
                            {
                                currentBlock = blockstack.Pop();
                                currentBlock.AddStmt( block );
                            }*/

                        }
                        break;
                    case Lexer.Tokens.Event:
                        currentBlock.AddStmt( ParseEvent() );
                        break;
                    case Lexer.Tokens.FindItem:
                        currentBlock.AddStmt( ParseFindItem() );
                        break;
                }

                if ( tok.TokenName == Lexer.Tokens.Import )
                {
                    //  Program.imports.Add( ParseImport() );
                }

                else if ( tok.TokenName == Lexer.Tokens.Function )
                {
                    Block block = currentBlock;
                    if ( blockstack.Count > 0 && block is Func )
                    {
                        currentBlock = blockstack.Pop();
                        currentBlock.AddStmt( block );
                    }

                    Func func = ParseFunc();
                   
                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = func;
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.If )
                {
                    IfBlock ifblock = ParseIf();

                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = ifblock;
                    }
                }
                /* else if ( tok.TokenName == Lexer.Tokens.ElseIf )
                 {
                     ElseIfBlock elseifblock = ParseElseIf();

                     if ( currentBlock != null )
                     {
                         blockstack.Push( currentBlock );
                         currentBlock = elseifblock;
                     }
                 }*/
                else if ( tok.TokenName == Lexer.Tokens.Else )
                {
                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = new ElseBlock() { Line = tok.Line };
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.Repeat )
                {
                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = new RepeatBlock() { Line = tok.Line };
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.Set )
                {
                    Assign a = ParseAssign();
                    currentBlock.AddStmt( a );
                }
                /*else if ( tok.TokenName == Lexer.Tokens.Ident )
                {
                    if ( tokens.Peek().TokenName == Lexer.Tokens.Equal )
                    {
                        tokens.pos--;
                        Assign a = ParseAssign();
                        currentBlock.AddStmt( a );
                    }
                    else if ( tokens.Peek().TokenName == Lexer.Tokens.LeftParan )
                    {
                        tokens.pos--;
                        Call c = ParseCall();
                        currentBlock.AddStmt( c );
                    }
                }*/
                else if ( tok.TokenName == Lexer.Tokens.Return )
                {
                    Return r = ParseReturn();
                    currentBlock.AddStmt( r );
                    Block block = currentBlock;

                    if ( blockstack.Count > 0 && block is Func )
                    {
                        currentBlock = blockstack.Pop();
                        currentBlock.AddStmt( block );
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.RightBrace )
                {
                    if ( currentBlock is Func )
                    {
                        currentBlock.AddStmt( new Return( null ) );
                        //tree.Add( currentBlock );
                        //currentBlock = null;
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    else if ( currentBlock is IfBlock || currentBlock is ElseIfBlock || currentBlock is ElseBlock )
                    {
                        currentBlock.AddStmt( new EndIf() );
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    else if ( currentBlock is RepeatBlock )
                    {
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.EOF )
                {
                    if ( currentBlock is Func )
                    {
                        currentBlock.AddStmt( new Return( null ) );
                        //tree.Add( currentBlock );
                        //currentBlock = null;
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    tree.Add( currentBlock );
                    running = false;
                }
            }
            return tree;
        }

        private EventStmt ParseEvent()
        {
            if ( CurrentToken.TokenName != Lexer.Tokens.StringLiteral )
            {
                throw new ParseException();
            }
            var type = CurrentToken.TokenValue;
            CurrentIndex++;
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );

            return new EventStmt( type, exps ) { Line = CurrentToken.Line };
        }

        private FindItemStmt ParseFindItem()
        {
            if ( CurrentToken.TokenName != Lexer.Tokens.StringLiteral )
            {
               // throw new Exception();
            }
            var ids = ParseExpr(); ;// CurrentToken.TokenValue;
            IntLiteral index = null;
            Expr filter = null;
            if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
            {
                index = new IntLiteral(int.Parse(CurrentToken.TokenValue));
                CurrentIndex++;
            }
            if(CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
            {
                filter = ParseExpr();
            }

            return new FindItemStmt( ids, index, filter ) { Line = CurrentToken.Line };
          /*  foreach ( var idStr in ids )
            {
                if ( idStr.Length == 3 )
                {
                    var id = Form1.EUO2StealthType( idStr );
                    foreach ( var i in World.Items.Values )
                    {
                        if ( i.ItemID == id && ( ( container != 0 && i.ContainerID == container ) || ( ground && i.ContainerID == 0 ) ) )
                        {
                            Setvariable( "#findid", Form1.uintToEUO( i.Serial.Value ) );
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

            }*/
        }
 



        private Call ParseCall()
        {
            var subname = "";
            //if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
            subname = CurrentToken.TokenValue;
            var exrps = new List<Expr>();
            CurrentIndex++;
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exrps.Add( ParseExpr() );
            return new Call( subname, exrps ) { Line = CurrentToken.Line };
        }

        private Assign ParseAssign()
        {
            var variableName = ParseExpr();
            string varName = "";
            var value = ParseExpr();
            return new Assign( variableName, value ) { Line = CurrentToken.Line };

        }

        private Return ParseReturn()
        {
            var exrps = new List<Expr>();
            if ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                return new Return( ParseExpr() ) { Line = CurrentToken.Line };
            return new Return( null ) { Line = CurrentToken.Line };
        }

        private int IfEndsAtThisLine = -1;
        private IfBlock ParseIf()
        {
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
            IfEndsAtThisLine = -1;
            if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
            {
                IfEndsAtThisLine = int.Parse( CurrentToken.TokenValue ) + CurrentToken.Line;
            }
            else if ( CurrentToken.TokenName == Lexer.Tokens.LeftBrace || NextToken.TokenName == Lexer.Tokens.LeftBrace )
            {

            }
            else
            {
                IfEndsAtThisLine = CurrentToken.Line + 1;
            }
            return new IfBlock( lexpr, op.TokenName, rexpr ) { Line = CurrentToken.Line };


        }

        private Func ParseFunc()
        {
            string ident = "";
            List<string> vars = new List<string>();

            if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
            {
                ident = CurrentToken.TokenValue.ToString();
            }
            else
                throw new ParseException( CurrentToken );



            return new Func( ident, null ) { Line = CurrentToken.Line };
        }
        Expr ParseExpr()
        {
            Expr ret = null;
            Token t = CurrentToken;

            if ( NextToken.TokenName == Lexer.Tokens.LeftParan )
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
            else if ( t.TokenName == Lexer.Tokens.IntLiteral )
            {
                IntLiteral i = new IntLiteral( Convert.ToInt32( t.TokenValue.ToString() ) );
                ret = i;
            }
            else if ( t.TokenName == Lexer.Tokens.StringLiteral )
            {
                StringLiteral s = new StringLiteral( t.TokenValue.ToString() );
                ret = s;
            }
            else if ( t.TokenName == Lexer.Tokens.Ident || t.TokenName == Lexer.Tokens.BuildInIdent )
            {
                string ident = t.TokenValue.ToString();

                Ident i = new Ident( ident );
                ret = i;
            }
            else if ( t.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
                Expr e = ParseExpr();

                if ( NextToken.TokenName == Lexer.Tokens.RightParan )
                {
                    CurrentIndex++;
                }

                ParanExpr p = new ParanExpr( e );

                if ( NextToken.TokenName == Lexer.Tokens.Add )
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.add, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Sub )
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.sub, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Mul )
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.mul, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Div )
                {
                    CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.div, expr );
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

            if ( NextToken.TokenName == Lexer.Tokens.Add || NextToken.TokenName == Lexer.Tokens.Sub || NextToken.TokenName == Lexer.Tokens.Mul || NextToken.TokenName == Lexer.Tokens.Div || NextToken.TokenName == Lexer.Tokens.Comma )
            {
                Expr lexpr = ret;
                Symbol op = 0;

                if ( NextToken.TokenName == Lexer.Tokens.Add )
                {
                    op = Symbol.add;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Sub )
                {
                    op = Symbol.sub;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Mul )
                {
                    op = Symbol.mul;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Div )
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

                ret = new MathExpr( lexpr, op, rexpr );
            }

            CurrentIndex++;
            return ret;
        }

    }
}
