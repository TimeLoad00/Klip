using System;

namespace Assistant
{
    internal class Language
    {
        public static string CliLocName { get; internal set; }

        internal static string GetString( LocString loc )
        {
            throw new NotImplementedException();
        }

        internal static LocString Format( LocString loc, object[] args )
        {
            throw new NotImplementedException();
        }

        internal static string GetString( int name )
        {
            throw new NotImplementedException();
        }

        internal static string ClilocFormat( int num, string ext_str )
        {
            throw new NotImplementedException();
        }

        internal static string GetCliloc( int v )
        {
            throw new NotImplementedException();
        }
    }
}