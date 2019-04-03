using Assistant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEasyUO
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();

            SetupVarsTimer();
        }

        private void SetupVarsTimer()
        {
            Thread t = new Thread( new ThreadStart( () => {
                while ( true )
                {
                    Thread.Sleep( 250 );
                    if(this.IsHandleCreated)
                    if ( InvokeRequired )
                    {
                        BeginInvoke( new MethodInvoker( UpdateVars ) );
                    }
                    else
                    {
                        UpdateVars();
                        // Do things
                    }
                }
                
                

            } ) );
            t.IsBackground = true;
            t.Start();
            treeVarTree.Nodes.Add( new TreeNode( "Character Info" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Status Bar" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Container Info" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Last Action" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Find Item" ) );
        }

        private void UpdateVars()
        {
            try
            {
                //txtDebug.Text = "CurrentLine: " + Parser?.CurrentLine ?? "0";
                Dictionary<string, object> charinfo = new Dictionary<string, object>();
                if ( !Engine.IsInstalled || World.Player == null )
                {
                    return;
                }

                charinfo.Add( "#CHARPOSX", World.Player.Position.X );
                charinfo.Add( "#CHARPOSY", World.Player.Position.Y );
                charinfo.Add( "#CHARPOSZ", World.Player.Position.Z );
                charinfo.Add( "#CHARDIR", World.Player.Direction );
                charinfo.Add( "#CHARSTATUS", World.Player.GetStatusCode() );
                charinfo.Add( "#CHARID", World.Player.Serial );
                charinfo.Add( "#CHARTYPE", World.Player.Body );
                charinfo.Add( "#CHARGHOST", World.Player.IsGhost );
                charinfo.Add( "#CHARBACKPACKID", uintToEUO( World.Player?.Backpack?.Serial ?? 0) );

                Dictionary<string, object> last = new Dictionary<string, object>();
                last.Add( "#LOBJECTID", EUOInterpreter.GetVariable<string>( "#LOBJECTID" ) );
                last.Add( "#LOBJECTTYPE", EUOInterpreter.GetVariable<string>( "#LOBJECTTYPE" ) );
                last.Add( "#LTARGETID", EUOInterpreter.GetVariable<string>( "#LTARGETID" ) );
                last.Add( "#LTARGETX", EUOInterpreter.GetVariable<string>( "#LTARGETX" ) );
                last.Add( "#LTARGETY", EUOInterpreter.GetVariable<string>( "#LTARGETY" ) );
                last.Add( "#LTARGETZ", EUOInterpreter.GetVariable<string>( "#LTARGETZ" ) );
                last.Add( "#LTARGETKIND", EUOInterpreter.GetVariable<string>( "#LTARGETKIND" ) );
                last.Add( "#LTARGETTILE", EUOInterpreter.GetVariable<string>( "#LTARGETTILE" ) );
                last.Add( "#LSKILL", EUOInterpreter.GetVariable<string>( "#LSKILL" ) );
                last.Add( "#LSPELL", EUOInterpreter.GetVariable<string>( "#LSPELL" ) );



                Dictionary<string, object> find = new Dictionary<string, object>();
                find.Add( "#FINDID", EUOInterpreter.GetVariable<string>( "#findid" ) );
                find.Add( "#FINDTYPE", EUOInterpreter.GetVariable<string>( "#FINDTYPE" ) );
                find.Add( "#FINDX", EUOInterpreter.GetVariable<string>( "#FINDX" ) );
                find.Add( "#FINDY", EUOInterpreter.GetVariable<string>( "#FINDY" ) );
                find.Add( "#FINDZ", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDDIST", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDKIND", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDSTACK", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDBAGID", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDMOD", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDREP", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDCOL", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDINDEX", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDCNT", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );


                foreach ( TreeNode n in treeVarTree.Nodes )
                {
                    if ( n.Text == "Last Action" )
                        UpdateChildren( (TreeNode)n, last );
                    if ( n.Text == "Character Info" )
                        UpdateChildren( (TreeNode)n, charinfo );
                    if ( n.Text == "Find Item" )
                        UpdateChildren( (TreeNode)n, find );
                }

            }catch(Exception e)
            {
                Debugger.Break();
                Console.WriteLine( e.Message );
            }
        }

        private void UpdateChildren( TreeNode n, Dictionary<string, object> dict )
        {
            foreach(var c in dict)
            {
                if ( n.Nodes.ContainsKey( c.Key ) )
                    n.Nodes[c.Key].Text = c.Key + ": " + c.Value.ToString();
                else
                    n.Nodes.Add(c.Key,c.Key + ": " + c.Value.ToString() );
            }
        }
      
public static string uintToEUO(uint Num)
        {
            uint bSys = 26;
            uint bNum, cnt, cnt2;
            string res = "";
            char[] arr = new char[7];
            bNum = ( Num ^ 69 ) + 7;
            cnt = 0;
            do
            {
                arr[cnt] = (char)((bNum % bSys) + 65);
                if ( bNum < bSys ) break;
                bNum = bNum / bSys;
                cnt++;

            } while ( true );

            return new string(arr).Trim('\0');
        }

        public static ushort EUO2StealthType( string EUO )
        {
            uint a = 0;
            uint i = 1;
            foreach ( var c in EUO )
            {
                a += ( ( c - (uint)65 ) * i );
                i *= 26;
            }
            a = ( a - 7 ) ^ 0x45;
            if ( a > 0xFFFF )
                return 0;

            return (ushort)a;
        }

        public static uint EUO2StealthID( string EUO )
        {
            uint ret = 0;
            uint i = 1;
            foreach ( var c in EUO )
            {
                ret += ( c - (uint)65 ) * i;
                i *= 26;
            }
            return ( ret - 7 ) ^ 0x45;
        }

        public EUOInterpreter Parser;
        private void btnPlayClicked( object sender, EventArgs e )
        {
            //if ( Parser == null || Parser.Script != txtScriptEntry.Text )
                Parser = new EUOInterpreter( txtScriptEntry.Text );
                
            Parser.Run();
        }

        private void btnPauseClicked( object sender, EventArgs e )
        {

        }

        private void btnStopClicked( object sender, EventArgs e )
        {

        }

        private void btnStepClicked( object sender, EventArgs e )
        {
            try
            {
               // if ( Parser == null || Parser.Script != txtScriptEntry.Text )
                //    Parser = new EUOParser( txtScriptEntry.Text );
                //Parser.Line();
               // txtDebug.Text = "Current Line: " + Parser.CurrentLine;
            } catch(Exception ee )
            {
                txtDebug.Text = "E: " + ee.Message;
            }
           
            
        }
    }
}
