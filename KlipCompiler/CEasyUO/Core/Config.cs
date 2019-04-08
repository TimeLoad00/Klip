using System;

namespace Assistant
{
    internal class Config
    {
        internal static bool GetBool( string v )
        {
            switch ( v )
            {
                default:
                    return true;
                case "QueueActions":
                    return false;
            }
        }

        internal static int GetInt( string v )
        {
            return 0;
        }

        internal static string GetString( string v )
        {
            return "DEBUG";
        }
    }
}