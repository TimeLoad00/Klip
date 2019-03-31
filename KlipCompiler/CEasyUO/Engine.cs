using CEasyUO;
using CUO_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assistant
{
    public class Engine
    {

        public Engine()
        {

        }
        public static ClientVersions ClientVersion { get; private set; }

        public static bool UseNewMobileIncoming => ClientVersion >= ClientVersions.CV_70331;

        public static bool UsePostHSChanges => ClientVersion >= ClientVersions.CV_7090;

        public static bool UsePostSAChanges => ClientVersion >= ClientVersions.CV_7000;

        public static bool UsePostKRPackets => ClientVersion >= ClientVersions.CV_6017;

        public static string UOFilePath { get; internal set; }
        public static bool IsInstalled { get; internal set; }
        public static unsafe void Install( PluginHeader* header )
        {
            Console.WriteLine( "Install Invoked CEasyUO" );
            ClientVersion = (ClientVersions)header->ClientVersion;
            try
            {
                PacketHandlers.Initialize();
                Targeting.Initialize();
                Spell.Initialize();
                if ( !ClientCommunication.InstallHooks( header ) )
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return;
                }
                string clientPath = Marshal.GetDelegateForFunctionPointer<OnGetUOFilePath>( header->GetUOFilePath )();
                Ultima.Files.SetMulPath( clientPath );
                Ultima.Multis.PostHSFormat = UsePostHSChanges;
            }
            catch (Exception e)
            {

            }
          
            Thread t = new Thread( () =>
            {
                Thread.CurrentThread.Name = "EasyUO Main Thread";
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new Form1() );
            } );
            t.SetApartmentState( ApartmentState.STA );

            t.IsBackground = true;

            t.Start();
            IsInstalled = true;

        }


        internal static void LogCrash( Exception e )
        {
        }
    }
}
