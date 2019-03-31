using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KlipCompiler
{
    public class Lexer
    {
        private readonly Dictionary<Tokens, string> _tokens;
        private readonly Dictionary<Tokens, MatchCollection> _regExMatchCollection;
        private string _inputString;
        private int _index;

        public enum Tokens
        {
            Undefined = 0,
            Import = 1,
            Function = 2,
            If = 3,
            ElseIf = 4,
            Else = 5,
            While = 6,
            Repeat = 7,
            Return = 8,
            IntLiteral = 9,
            StringLiteral = 10,
            Ident = 11,
            Whitespace = 12,
            NewLine = 13,
            Add = 14,
            Sub = 15,
            Mul = 16,
            Div = 17,
            Equal = 18,
            DoubleEqual = 19,
            NotEqual = 20,
            LeftParan = 21,
            RightParan = 22,
            LeftBrace = 23,
            RightBrace = 24,
            Comma = 25,
            Period = 26,
            EOF = 27,
            Set,
            Label,
            Event,
            Goto,
            Target,
            Wait,
            BuildInIdent,
            Comment,
            LessOrEqual,
            MoreOrEqual,
            LessOrEqual2,
            MoreOrEqual2,
            IgnoreItem,
            Call,
            FindItem,
            Msg,
            Dollar,
            Less,
            More,
            For
        }

        public string InputString
        {
            set
            {
                _inputString = value.ToLowerInvariant();
                PrepareRegex();
            }
        }

        public Lexer()
        {
            _tokens = new Dictionary<Tokens, string>();
            _regExMatchCollection = new Dictionary<Tokens, MatchCollection>();
            _index = 0;
            _inputString = string.Empty;


            _tokens.Add( Tokens.Set, "set" );
            _tokens.Add( Tokens.Event, "event " );
            _tokens.Add( Tokens.Goto, "goto" );
            _tokens.Add( Tokens.Call, "gosub" );
            _tokens.Add( Tokens.Function, "sub " );
            _tokens.Add( Tokens.FindItem, "finditem" );
            _tokens.Add( Tokens.Msg, "msg" );


            _tokens.Add( Tokens.Target, "target" );
            _tokens.Add( Tokens.Wait, "wait" );
            _tokens.Add( Tokens.IgnoreItem, "ignoreitem" );
            //_tokens.Add(Tokens.Fu, "import");
            _tokens.Add(Tokens.For, "for");
            _tokens.Add( Tokens.If, "if" );
            _tokens.Add( Tokens.ElseIf, "elseif" );
            _tokens.Add( Tokens.Else, "else" );
            _tokens.Add( Tokens.Repeat, "repeat" );
            _tokens.Add( Tokens.Return, "return" );
            _tokens.Add( Tokens.IntLiteral, "[0-9][0-9]*" );

            _tokens.Add( Tokens.Label, "[a-zA-Z_][a-zA-Z0-9_]*?:" );
            _tokens.Add( Tokens.Comment, ";[^\n\r]*" );

            _tokens.Add( Tokens.Ident, "%[a-zA-Z0-9_]*" );
            _tokens.Add( Tokens.BuildInIdent, "#[a-zA-Z_][a-zA-Z0-9_]*" );
            _tokens.Add( Tokens.StringLiteral, "[a-zA-Z_][a-zA-Z0-9_]*" );

            _tokens.Add( Tokens.Whitespace, "[ \\t]+" );
            _tokens.Add( Tokens.NewLine, "\\r\\n" );
            _tokens.Add( Tokens.Add, "\\+" );
            _tokens.Add( Tokens.Sub, "\\-" );
            _tokens.Add( Tokens.Mul, "\\*" );
            _tokens.Add( Tokens.Div, "\\/" );
            _tokens.Add( Tokens.NotEqual, "\\<>" );
            _tokens.Add( Tokens.LessOrEqual, "\\<=" );
            _tokens.Add( Tokens.MoreOrEqual, "\\>=" );
            _tokens.Add( Tokens.LessOrEqual2, "\\=<" );
            _tokens.Add( Tokens.MoreOrEqual2, "\\=>" );
            _tokens.Add( Tokens.Less, "\\<" );
            _tokens.Add( Tokens.More, "\\>" );

            _tokens.Add( Tokens.Equal, "\\=" );
            _tokens.Add( Tokens.LeftParan, "\\(" );
            _tokens.Add( Tokens.RightParan, "\\)" );
            _tokens.Add( Tokens.LeftBrace, "\\{" );
            _tokens.Add( Tokens.RightBrace, "\\}" );
            _tokens.Add( Tokens.Comma, "\\," );
            _tokens.Add( Tokens.Period, "\\." );
            _tokens.Add( Tokens.Dollar, "\\$" );
        }

        private void PrepareRegex()
        {
            _regExMatchCollection.Clear();
            foreach ( KeyValuePair<Tokens, string> pair in _tokens )
            {
                _regExMatchCollection.Add( pair.Key, Regex.Matches( _inputString, pair.Value ) );
            }
        }

        public void ResetParser()
        {
            _index = 0;
            _inputString = string.Empty;
            _regExMatchCollection.Clear();
        }

        public Token GetToken()
        {
            if ( _index >= _inputString.Length )
                return null;

            foreach ( KeyValuePair<Tokens, MatchCollection> pair in _regExMatchCollection )
            {
                foreach ( Match match in pair.Value )
                {
                    if ( match.Index == _index )
                    {
                        _index += match.Length;
                        return new Token( pair.Key, match.Value, match.Index );
                    }

                    if ( match.Index > _index )
                    {
                        break;
                    }
                }
            }
            _index++;
            return new Token( Tokens.Undefined, string.Empty );
        }

        public PeekToken Peek()
        {
            return Peek( new PeekToken( _index, new Token( Tokens.Undefined, string.Empty ) ) );
        }

        public PeekToken Peek( PeekToken peekToken )
        {
            int oldIndex = _index;

            _index = peekToken.TokenIndex;

            if ( _index >= _inputString.Length )
            {
                _index = oldIndex;
                return null;
            }

            foreach ( KeyValuePair<Tokens, string> pair in _tokens )
            {
                Regex r = new Regex( pair.Value );
                Match m = r.Match( _inputString, _index );

                if ( m.Success && m.Index == _index )
                {
                    _index += m.Length;
                    PeekToken pt = new PeekToken( _index, new Token( pair.Key, m.Value ) );
                    _index = oldIndex;
                    return pt;
                }
            }
            PeekToken pt2 = new PeekToken( _index + 1, new Token( Tokens.Undefined, string.Empty ) );
            _index = oldIndex;
            return pt2;
        }
    }

    public class PeekToken
    {
        public int TokenIndex { get; set; }

        public Token TokenPeek { get; set; }

        public PeekToken( int index, Token value )
        {
            TokenIndex = index;
            TokenPeek = value;
        }
    }

    public class Token
    {
        public Lexer.Tokens TokenName { get; set; }

        public string TokenValue { get; set; }
        public int Line { get; internal set; }

        public int Index;

        public Token( Lexer.Tokens name, string value, int index = -1 )
        {
            TokenName = name;
            TokenValue = value;
            Index = index;
        }

        public override string ToString()
        {
            return TokenName.ToString();
        }
    }

    public class TokenList
    {
        public List<Token> Tokens;
        public int pos = 0;

        public TokenList( List<Token> tokens )
        {
            Tokens = tokens;
        }

        public Token GetToken()
        {
            Token ret = Tokens[pos];
            pos++;
            return ret;
        }

        public Token Peek()
        {
            return Tokens[pos];
        }
    }
}
