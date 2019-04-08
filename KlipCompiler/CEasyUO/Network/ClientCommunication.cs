using CUO_API;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Assistant
{
    public unsafe sealed class ClientCommunication
    {
        public static DateTime ConnectionStart { get; private set; }
        public static IPAddress LastConnection { get; }

        public static IntPtr ClientWindow { get; private set; } = IntPtr.Zero;

        private static OnPacketSendRecv _sendToClient, _sendToServer, _recv, _send;
        private static OnGetPacketLength _getPacketLength;
        private static OnGetPlayerPosition _getPlayerPosition;
        private static OnCastSpell _castSpell;
        private static OnGetStaticImage _getStaticImage;

        private static OnHotkey _onHotkeyPressed;
        private static OnMouse _onMouse;
        private static OnUpdatePlayerPosition _onUpdatePlayerPosition;
        private static OnClientClose _onClientClose;
        private static OnInitialize _onInitialize;
        private static OnConnected _onConnected;
        private static OnDisconnected _onDisconnected;
        private static OnFocusGained _onFocusGained;
        private static OnFocusLost _onFocusLost;

        internal static bool InstallHooks( PluginHeader* header )
        {
            _sendToClient = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( header->Recv );
            _sendToServer = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>( header->Send );
            _getPacketLength = Marshal.GetDelegateForFunctionPointer<OnGetPacketLength>( header->GetPacketLength );
            _getPlayerPosition = Marshal.GetDelegateForFunctionPointer<OnGetPlayerPosition>( header->GetPlayerPosition );
            _castSpell = Marshal.GetDelegateForFunctionPointer<OnCastSpell>( header->CastSpell );
            _getStaticImage = Marshal.GetDelegateForFunctionPointer<OnGetStaticImage>( header->GetStaticImage );

            ClientWindow = header->HWND;

            _recv = OnRecv;
            _send = OnSend;
            _onHotkeyPressed = OnHotKeyHandler;
            _onMouse = OnMouseHandler;
            _onUpdatePlayerPosition = OnPlayerPositionChanged;
            _onClientClose = OnClientClosing;
            _onInitialize = OnInitialize;
            _onConnected = OnConnected;
            _onDisconnected = OnDisconnected;
           // _onFocusGained = OnFocusGained;
           // _onFocusLost = OnFocusLost;

            header->OnRecv = Marshal.GetFunctionPointerForDelegate( _recv );
            header->OnSend = Marshal.GetFunctionPointerForDelegate( _send );
            header->OnHotkeyPressed = Marshal.GetFunctionPointerForDelegate( _onHotkeyPressed );
            header->OnMouse = Marshal.GetFunctionPointerForDelegate( _onMouse );
            header->OnPlayerPositionChanged = Marshal.GetFunctionPointerForDelegate( _onUpdatePlayerPosition );
            header->OnClientClosing = Marshal.GetFunctionPointerForDelegate( _onClientClose );
            header->OnInitialize = Marshal.GetFunctionPointerForDelegate( _onInitialize );
            header->OnConnected = Marshal.GetFunctionPointerForDelegate( _onConnected );
            header->OnDisconnected = Marshal.GetFunctionPointerForDelegate( _onDisconnected );
            header->OnFocusGained = Marshal.GetFunctionPointerForDelegate( _onFocusGained );
            header->OnFocusLost = Marshal.GetFunctionPointerForDelegate( _onFocusLost );

            return true;
        }

        private static void OnClientClosing()
        {
            var last = Console.BackgroundColor;
            var lastFore = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine( "Closing EasyUO instance" );
            Console.BackgroundColor = last;
            Console.ForegroundColor = lastFore;

        }

        private static void OnInitialize()
        {
            var last = Console.BackgroundColor;
            var lastFore = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine( "Initialized EasyUO instance" );
            Console.BackgroundColor = last;
            Console.ForegroundColor = lastFore;
        }

        private static void OnConnected()
        {
            ConnectionStart = DateTime.Now;
            try
            {
            }
            catch
            {
            }
        }

        private static void OnDisconnected()
        {
            PacketHandlers.Party.Clear();

        

            World.Player = null;
            World.Items.Clear();
            World.Mobiles.Clear();
            ActionQueue.Stop();
          
            BuffsTimer.Stop();
            
            PacketHandlers.Party.Clear();
            PacketHandlers.IgnoreGumps.Clear();
        }

        


        private static bool OnHotKeyHandler( int key, int mod, bool ispressed )
        {
            if ( ispressed )
            {
                // bool code = HotKey.OnKeyDown( (int)( key | mod ) );

                return true;// code;
            }

            return true;
        }

        private static void OnMouseHandler( int button, int wheel )
        {
            if ( button > 4 )
                button = 3;
            else if ( button > 3 )
                button = 2;
            else if ( button > 2 )
                button = 2;
            else if ( button > 1 )
                button = 1;

            //HotKey.OnMouse( button, wheel );
        }

        private static void OnPlayerPositionChanged( int x, int y, int z )
        {
            if(World.Player != null)
                World.Player.Position = new Point3D( x, y, z );
        }

        private static bool OnRecv( byte[] data, int length )
        {
            fixed ( byte* ptr = data )
            {
                PacketReader p = new PacketReader( ptr, length, PacketsTable.GetPacketLength( data[0] ) < 0 );
                Packet packet = new Packet( data, length, p.DynamicLength );

                return !PacketHandler.OnServerPacket( p.PacketID, p, packet );
            }
        }

        private static bool OnSend( byte[] data, int length )
        {
            fixed ( byte* ptr = data )
            {
                PacketReader p = new PacketReader( ptr, length, PacketsTable.GetPacketLength( data[0] ) < 0 );
                Packet packet = new Packet( data, length, p.DynamicLength );

                return !PacketHandler.OnClientPacket( p.PacketID, p, packet );
            }
        }

        public static void CastSpell( int idx ) => _castSpell( idx );

        public static bool GetPlayerPosition( out int x, out int y, out int z )
            => _getPlayerPosition( out x, out y, out z );

        internal static void SendToServer( Packet p )
        {
            var len = p.Length;
            _sendToServer( p.Compile(), (int)len );
        }

        internal static void SendToClient( Packet p )
        {
            var len = p.Length;
            _sendToClient( p.Compile(), (int)len );
        }
    }

}
