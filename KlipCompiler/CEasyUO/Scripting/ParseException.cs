using System;
using System.Runtime.Serialization;

namespace CEasyUO
{
    [Serializable]
    internal class ParseException : Exception
    {
        private Token currentToken;

        public ParseException()
        {
        }

        public ParseException( Token c ) : base( $"Error Parsing Token at: {c.Line} Token identified as: {c.TokenName} value: {c.TokenValue}" )
        {
            this.currentToken = currentToken;
        }

       
        protected ParseException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}