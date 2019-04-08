using System;
using System.Collections;
using System.Collections.Generic;

namespace Assistant
{
    public class TargetInfo
    {
        public byte Type;
        public uint TargID;
        public byte Flags;
        public Serial Serial;
        public int X, Y;
        public int Z;
        public ushort Gfx;
    }

    public class Targeting
    {
        public const uint LocalTargID = 0x7FFFFFFF; // uid for target sent from razor

        public delegate void TargetResponseCallback(bool location, Serial serial, Point3D p, ushort gfxid);
        public delegate void CancelTargetCallback();

        private static CancelTargetCallback m_OnCancel;
        private static TargetResponseCallback m_OnTarget;

        private static bool m_Intercept;
        private static bool m_HasTarget;
        private static bool m_ClientTarget;
        private static TargetInfo m_LastTarget;
        public static TargetInfo LastTarget => m_LastTarget != null ? m_LastTarget : m_LastGroundTarg;
        private static TargetInfo m_LastGroundTarg;
        private static TargetInfo m_LastBeneTarg;
        private static TargetInfo m_LastHarmTarg;

        private static bool m_AllowGround;
        private static uint m_CurrentID;
        private static byte m_CurFlags;

        private static uint m_PreviousID;
        private static bool m_PreviousGround;
        private static byte m_PrevFlags;

        private static Serial m_LastCombatant;

        private delegate bool QueueTarget();
        private static QueueTarget TargetSelfAction = new QueueTarget(DoTargetSelf);
        private static QueueTarget LastTargetAction = new QueueTarget(DoLastTarget);
        private static QueueTarget m_QueueTarget;


        private static uint m_SpellTargID = 0;
        public static uint SpellTargetID { get { return m_SpellTargID; } set { m_SpellTargID = value; } }

        private static List<uint> m_FilterCancel = new List<uint>();

        public static bool HasTarget { get { return m_HasTarget; } }

        
        private static List<ushort> m_MonsterIds = new List<ushort>()
         {
             0x1, 0x2, 0x3, 0x4, 0x7, 0x8, 0x9, 0xC, 0xD, 0xE, 0xF,
             0x10, 0x11, 0x12, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C,
             0x1E, 0x1F, 0x21, 0x23, 0x24, 0x25, 0x27, 0x29, 0x2A, 0x2C,
             0x2D, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
             0x39, 0x3B, 0x3C, 0x3D, 0x42, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
             0x4B, 0x4F, 0x50, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x59, 0x5A,
             0x5B, 0x5C, 0x5D, 0x5E, 0x60, 0x61, 0x62, 0x69, 0x6A, 0x6B, 0x6C,
             0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x87, 0x88, 0x89,
             0x8A, 0x8B, 0x8C, 0x8E, 0x8F, 0x91, 0x93, 0x96, 0x99, 0x9B, 0x9E,
             0x9F, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xB4, 0x4C, 0x4D, 0x3D
         };

        public static void Initialize()
        {
            PacketHandler.RegisterClientToServerViewer(0x6C, new PacketViewerCallback(TargetResponse));
            PacketHandler.RegisterServerToClientViewer(0x6C, new PacketViewerCallback(NewTarget));
            PacketHandler.RegisterServerToClientViewer(0xAA, new PacketViewerCallback(CombatantChange));

         
        }

        private static void CombatantChange(PacketReader p, PacketHandlerEventArgs e)
        {
            Serial ser = p.ReadUInt32();
            if (ser.IsMobile && ser != World.Player.Serial && ser != Serial.Zero && ser != Serial.MinusOne)
                m_LastCombatant = ser;
        }

        private static void AttackLastComb()
        {
            if (m_LastCombatant.IsMobile)
                ClientCommunication.SendToServer(new AttackReq(m_LastCombatant));
        }

        private static void AttackLastTarg()
        {
            if (m_LastTarget != null && m_LastTarget.Serial.IsMobile)
                ClientCommunication.SendToServer(new AttackReq(m_LastTarget.Serial));
        }

        private static void OnClearQueue()
        {
            ClearQueue();

       
        }

        internal static void OneTimeTarget(TargetResponseCallback onTarget)
        {
            OneTimeTarget(false, onTarget, null);
        }

        internal static void OneTimeTarget(bool ground, TargetResponseCallback onTarget)
        {
            OneTimeTarget(ground, onTarget, null);
        }

        internal static void OneTimeTarget(TargetResponseCallback onTarget, CancelTargetCallback onCancel)
        {
            OneTimeTarget(false, onTarget, onCancel);
        }

        internal static void OneTimeTarget(bool ground, TargetResponseCallback onTarget, CancelTargetCallback onCancel)
        {
            if (m_Intercept && m_OnCancel != null)
            {
                m_OnCancel();
                CancelOneTimeTarget();
            }

            if (m_HasTarget && m_CurrentID != 0 && m_CurrentID != LocalTargID)
            {
                m_PreviousID = m_CurrentID;
                m_PreviousGround = m_AllowGround;
                m_PrevFlags = m_CurFlags;

                m_FilterCancel.Add(m_PreviousID);
            }

            m_Intercept = true;
            m_CurrentID = LocalTargID;
            m_OnTarget = onTarget;
            m_OnCancel = onCancel;

            m_ClientTarget = m_HasTarget = true;
            ClientCommunication.SendToClient(new Target(LocalTargID, ground));
            ClearQueue();
        }

        internal static void CancelOneTimeTarget()
        {
            m_ClientTarget = m_HasTarget = false;

            ClientCommunication.SendToClient(new CancelTarget(LocalTargID));
            EndIntercept();
        }

        private static bool m_LTWasSet;
        public static void TargetSetLastTarget()
        {
            if (World.Player != null)
            {
                m_LTWasSet = false;
                OneTimeTarget(false, new TargetResponseCallback(OnSetLastTarget), new CancelTargetCallback(OnSLTCancel));
            }
        }

        private static void OnSLTCancel()
        {
            if (m_LastTarget != null)
                m_LTWasSet = true;
        }

        private static void OnSetLastTarget(bool location, Serial serial, Point3D p, ushort gfxid)
        {
            if (serial == World.Player.Serial)
            {
                OnSLTCancel();
                return;
            }

            m_LastBeneTarg = m_LastHarmTarg = m_LastGroundTarg = m_LastTarget = new TargetInfo();
            m_LastTarget.Flags = 0;
            m_LastTarget.Gfx = gfxid;
            m_LastTarget.Serial = serial;
            m_LastTarget.Type = (byte)(location ? 1 : 0);
            m_LastTarget.X = p.X;
            m_LastTarget.Y = p.Y;
            m_LastTarget.Z = p.Z;

            m_LTWasSet = true;

            if (serial.IsMobile)
            {
                LastTargetChanged();
                ClientCommunication.SendToClient(new ChangeCombatant(serial));
                m_LastCombatant = serial;
            }
        }

        private static Serial m_OldLT = Serial.Zero;

        private static void RemoveTextFlags(UOEntity m)
        {
            if (m != null)
            {
                bool oplchanged = false;

                if (oplchanged)
                    m.OPLChanged();
            }
        }

        private static void AddTextFlags(UOEntity m)
        {
        
        }

        private static void LastTargetChanged()
        {
            if (m_LastTarget != null)
            {
                bool lth = Config.GetInt("LTHilight") != 0;

                if (m_OldLT.IsItem)
                {
                    RemoveTextFlags(World.FindItem(m_OldLT));
                }
                else
                {
                    Mobile m = World.FindMobile(m_OldLT);
                    if (m != null)
                    {
                        if (lth)
                            ClientCommunication.SendToClient(new MobileIncoming(m));

                        RemoveTextFlags(m);
                    }
                }

                if (m_LastTarget.Serial.IsItem)
                {
                    AddTextFlags(World.FindItem(m_LastTarget.Serial));
                }
                else
                {
                    Mobile m = World.FindMobile(m_LastTarget.Serial);
                    if (m != null)
                    {
                        if (IsLastTarget(m) && lth)
                            ClientCommunication.SendToClient(new MobileIncoming(m));

                        CheckLastTargetRange(m);

                        AddTextFlags(m);
                    }
                }

                m_OldLT = m_LastTarget.Serial;
            }
        }

        public static bool LTWasSet { get { return m_LTWasSet; } }

        public static void TargetRandNonFriendly()
        {
            RandomTarget(3, 4, 5, 6);
        }

        public static void TargetRandFriendly()
        {
            RandomTarget(0, 1, 2);
        }

        public static void TargetRandEnemy()
        {
            RandomTarget(5);
        }

        public static void TargetRandEnemyMonster()
        {
            RandomMonsterTarget(5);
        }

        public static void TargetRandEnemyHumanoid()
        {
            RandomHumanoidTarget(5);
        }

        public static void TargetRandRed()
        {
            RandomTarget(6);
        }

        public static void TargetRandGrey()
        {
            RandomTarget(3, 4);
        }

        public static void TargetRandGreyMonster()
        {
            RandomMonsterTarget(3, 4);
        }

        public static void TargetRandGreyHumanoid()
        {
            RandomHumanoidTarget(3, 4);
        }

        public static void TargetRandCriminal()
        {
            RandomTarget(4);
        }

        public static void TargetRandCriminalHumanoid()
        {
            RandomHumanoidTarget(4);
        }

        public static void TargetRandInnocent()
        {
            RandomTarget(1);
        }

        public static void TargetRandInnocentHumanoid()
        {
            RandomHumanoidTarget(1);
        }

        public static void TargetRandAnyone()
        {
            RandomTarget();
        }

        public static void RandomTarget(params int[] noto)
        {
 

            List<Mobile> list = new List<Mobile>();
            foreach (Mobile m in World.MobilesInRange(12))
            {
                if ((noto.Length > 0 && noto[0] == 0) &&
                     !m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                     Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                {
                    for (int i = 0; i < noto.Length; i++)
                    {
                        if (noto[i] == m.Notoriety)
                        {
                            list.Add(m);
                            break;
                        }
                    }

                    if (noto.Length == 0)
                        list.Add(m);
                }
            }

            if (list.Count > 0)
                SetLastTargetTo((Mobile)list[Utility.Random(list.Count)]);

        }

        public static void RandomHumanoidTarget(params int[] noto)
        {


            List<Mobile> list = new List<Mobile>();
            foreach (Mobile m in World.MobilesInRange(12))
            {
                if (m.Body != 0x0190 && m.Body != 0x0191 && m.Body != 0x025D && m.Body != 0x025E)
                    continue;

                if ((noto.Length > 0 && noto[0] == 0) &&
                     !m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                     Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                {
                    for (int i = 0; i < noto.Length; i++)
                    {
                        if (noto[i] == m.Notoriety)
                        {
                            list.Add(m);
                            break;
                        }
                    }

                    if (noto.Length == 0)
                        list.Add(m);
                }
            }

            if (list.Count > 0)
                SetLastTargetTo(list[Utility.Random(list.Count)]);

        }

        public static void RandomMonsterTarget(params int[] noto)
        {


            List<Mobile> list = new List<Mobile>();
            foreach (Mobile m in World.MobilesInRange(12))
            {
                if (!m.IsMonster)
                    continue;

                if ((noto.Length > 0 && noto[0] == 0) &&
                    !m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                    Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                {
                    for (int i = 0; i < noto.Length; i++)
                    {
                        if (noto[i] == m.Notoriety)
                        {
                            list.Add(m);
                            break;
                        }
                    }

                    if (noto.Length == 0)
                        list.Add(m);
                }
            }

            if (list.Count > 0)
                SetLastTargetTo(list[Utility.Random(list.Count)]);

        }


        public static void TargetCloseNonFriendly()
        {
            ClosestTarget(3, 4, 5, 6);
        }

        public static void TargetCloseFriendly()
        {
            ClosestTarget(0, 1, 2);
        }

        public static void TargetCloseEnemy()
        {
            ClosestTarget(5);
        }

        public static void TargetCloseEnemyHumanoid()
        {
            ClosestHumanoidTarget(5);
        }

        public static void TargetCloseEnemyMonster()
        {
            ClosestMonsterTarget(5);
        }

        public static void TargetCloseRed()
        {
            ClosestTarget(6);
        }

        public static void TargetCloseGrey()
        {
            ClosestTarget(3, 4);
        }

        public static void TargetCloseGreyMonster()
        {
            ClosestMonsterTarget(3, 4);
        }

        public static void TargetCloseGreyHumanoid()
        {
            ClosestHumanoidTarget(3, 4);
        }

        public static void TargetCloseCriminal()
        {
            ClosestTarget(4);
        }

        public static void TargetCloseCriminalHumanoid()
        {
            ClosestHumanoidTarget(4);
        }

        public static void TargetCloseInnocent()
        {
            ClosestTarget(1);
        }

        public static void TargetCloseInnocentHumanoid()
        {
            ClosestHumanoidTarget(1);
        }

        public static void TargetClosest()
        {
            ClosestTarget();
        }

        public static void ClosestTarget(params int[] noto)
        {
   

            List<Mobile> list = new List<Mobile>();
            foreach (Mobile m in World.MobilesInRange(12))
            {
                if ((noto.Length > 0 && noto[0] == 0) &&
                     !m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                     Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                {
                    for (int i = 0; i < noto.Length; i++)
                    {
                        if (noto[i] == m.Notoriety)
                        {
                            list.Add(m);
                            break;
                        }
                    }

                    if (noto.Length == 0)
                        list.Add(m);
                }
            }

            Mobile closest = null;
            double closestDist = double.MaxValue;

            foreach (Mobile m in list)
            {
                double dist = Utility.DistanceSqrt(m.Position, World.Player.Position);

                if (dist < closestDist || closest == null)
                {
                    closestDist = dist;
                    closest = m;
                }
            }

            if (closest != null)
                SetLastTargetTo(closest);
;
        }

        public static void ClosestHumanoidTarget(params int[] noto)
        {
   

            List<Mobile> list = new List<Mobile>();
            foreach (Mobile m in World.MobilesInRange(12))
            {
                if (m.Body != 0x0190 && m.Body != 0x0191 && m.Body != 0x025D && m.Body != 0x025E)
                    continue;

                if (((noto.Length > 0 && noto[0] == 0)) &&
                     !m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                     Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                {
                    for (int i = 0; i < noto.Length; i++)
                    {
                        if (noto[i] == m.Notoriety)
                        {
                            list.Add(m);
                            break;
                        }
                    }

                    if (noto.Length == 0)
                        list.Add(m);
                }
            }

            Mobile closest = null;
            double closestDist = double.MaxValue;

            foreach (Mobile m in list)
            {
                double dist = Utility.DistanceSqrt(m.Position, World.Player.Position);

                if (dist < closestDist || closest == null)
                {
                    closestDist = dist;
                    closest = m;
                }
            }

            if (closest != null)
                SetLastTargetTo(closest);

        }

        public static void ClosestMonsterTarget(params int[] noto)
        {


            List<Mobile> list = new List<Mobile>();
            foreach (Mobile m in World.MobilesInRange(12))
            {
                if (!m.IsMonster)
                    continue;

                if (((noto.Length > 0 && noto[0] == 0)) &&
                    !m.Blessed && !m.IsGhost && m.Serial != World.Player.Serial &&
                    Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                {
                    for (int i = 0; i < noto.Length; i++)
                    {
                        if (noto[i] == m.Notoriety)
                        {
                            list.Add(m);
                            break;
                        }
                    }

                    if (noto.Length == 0)
                        list.Add(m);
                }
            }

            Mobile closest = null;
            double closestDist = double.MaxValue;

            foreach (Mobile m in list)
            {
                double dist = Utility.DistanceSqrt(m.Position, World.Player.Position);

                if (dist < closestDist || closest == null)
                {
                    closestDist = dist;
                    closest = m;
                }
            }

            if (closest != null)
                SetLastTargetTo(closest);

        }

        public static void SetLastTargetTo(Mobile m)
        {
            SetLastTargetTo(m, 0);
        }

        public static void SetLastTargetTo(Mobile m, byte flagType)
        {
            TargetInfo targ = new TargetInfo();
            m_LastGroundTarg = m_LastTarget = targ;

            if ((m_HasTarget && m_CurFlags == 1) || flagType == 1)
                m_LastHarmTarg = targ;
            else if ((m_HasTarget && m_CurFlags == 2) || flagType == 2)
                m_LastBeneTarg = targ;
            else if (flagType == 0)
                m_LastHarmTarg = m_LastBeneTarg = targ;

            targ.Type = 0;
            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Flags = flagType;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;


            TargetLast();
            LastTargetChanged();
        }

        private static void EndIntercept()
        {
            m_Intercept = false;
            m_OnTarget = null;
            m_OnCancel = null;
        }

        public static void TargetSelf()
        {
            TargetSelf(false);
        }

        public static void TargetSelf(bool forceQ)
        {
            if (World.Player == null)
                return;

            //if ( Macros.MacroManager.AcceptActions )
            //	MacroManager.Action( new TargetSelfAction() );

            if (m_HasTarget)
            {
                if (!DoTargetSelf())
                    ResendTarget();
            }
            else if (forceQ || Config.GetBool("QueueTargets"))
            {
 
                m_QueueTarget = TargetSelfAction;
            }
        }

        public static bool DoTargetSelf()
        {
            if (World.Player == null)
                return false;

            if (CheckHealPoisonTarg(m_CurrentID, World.Player.Serial))
                return false;

            CancelClientTarget();
            m_HasTarget = false;

            if (m_Intercept)
            {
                TargetInfo targ = new TargetInfo();
                targ.Serial = World.Player.Serial;
                targ.Gfx = World.Player.Body;
                targ.Type = 0;
                targ.X = World.Player.Position.X;
                targ.Y = World.Player.Position.Y;
                targ.Z = World.Player.Position.Z;
                targ.TargID = LocalTargID;
                targ.Flags = 0;

                OneTimeResponse(targ);
            }
            else
            {
                ClientCommunication.SendToServer(new TargetResponse(m_CurrentID, World.Player));
            }

            return true;
        }

        public static void TargetLast()
        {
            TargetLast( false);
        }

        public static void TargetLast( bool forceQ)
        {
            //if ( Macros.MacroManager.AcceptActions )
            //	MacroManager.Action( new LastTargetAction() );

            if (m_HasTarget)
            {
                if (!DoLastTarget())
                    ResendTarget();
            }
            else if (forceQ || Config.GetBool("QueueTargets"))
            {


                m_QueueTarget = LastTargetAction;
            }
        }

        public static bool DoLastTarget()
        {
            TargetInfo targ;
            if (Config.GetBool("SmartLastTarget"))
            {
                if (m_AllowGround && m_LastGroundTarg != null)
                    targ = m_LastGroundTarg;
                else if (m_CurFlags == 1)
                    targ = m_LastHarmTarg;
                else if (m_CurFlags == 2)
                    targ = m_LastBeneTarg;
                else
                    targ = m_LastTarget;

                if (targ == null)
                    targ = m_LastTarget;
            }
            else
            {
                if (m_AllowGround && m_LastGroundTarg != null)
                    targ = m_LastGroundTarg;
                else
                    targ = m_LastTarget;
            }

            if (targ == null)
                return false;

            Point3D pos = Point3D.Zero;
            if (targ.Serial.IsMobile)
            {
                Mobile m = World.FindMobile(targ.Serial);
                if (m != null)
                {
                    pos = m.Position;

                    targ.X = pos.X;
                    targ.Y = pos.Y;
                    targ.Z = pos.Z;
                }
                else
                {
                    pos = Point3D.Zero;
                }
            }
            else if (targ.Serial.IsItem)
            {
                Item i = World.FindItem(targ.Serial);
                if (i != null)
                {
                    pos = i.GetWorldPosition();

                    targ.X = i.Position.X;
                    targ.Y = i.Position.Y;
                    targ.Z = i.Position.Z;
                }
                else
                {
                    pos = Point3D.Zero;
                    targ.X = targ.Y = targ.Z = 0;
                }
            }
            else
            {
                if (!m_AllowGround && (targ.Serial == Serial.Zero || targ.Serial >= 0x80000000))
                {
                    return false;
                }
                else
                {
                    pos = new Point3D(targ.X, targ.Y, targ.Z);
                }
            }

            if (Config.GetBool("RangeCheckLT") && (pos == Point3D.Zero || !Utility.InRange(World.Player.Position, pos, Config.GetInt("LTRange"))))
            {
                if (Config.GetBool("QueueTargets"))
                    m_QueueTarget = LastTargetAction;
                return false;
            }

            if (CheckHealPoisonTarg(m_CurrentID, targ.Serial))
                return false;

            CancelClientTarget();
            m_HasTarget = false;

            targ.TargID = m_CurrentID;

            if (m_Intercept)
                OneTimeResponse(targ);
            else
                ClientCommunication.SendToServer(new TargetResponse(targ));
            return true;
        }

        public static void ClearQueue()
        {
            m_QueueTarget = null;
        }

        private static TimerCallbackState m_OneTimeRespCallback = new TimerCallbackState(OneTimeResponse);

        private static void OneTimeResponse(object state)
        {
            TargetInfo info = state as TargetInfo;

            if (info != null)
            {
                if ((info.X == 0xFFFF && info.X == 0xFFFF) && (info.Serial == 0 || info.Serial >= 0x80000000))
                {
                    if (m_OnCancel != null)
                        m_OnCancel();
                }
                else
                {

                    if (m_OnTarget != null)
                        m_OnTarget(info.Type == 1 ? true : false, info.Serial, new Point3D(info.X, info.Y, info.Z), info.Gfx);
                }
            }

            EndIntercept();
        }

        private static void CancelTarget()
        {
            OnClearQueue();
            CancelClientTarget();

            if (m_HasTarget)
            {
                ClientCommunication.SendToServer(new TargetCancelResponse(m_CurrentID));
                m_HasTarget = false;
            }
        }

        private static void CancelClientTarget()
        {
            if (m_ClientTarget)
            {
                m_FilterCancel.Add((uint)m_CurrentID);
                ClientCommunication.SendToClient(new CancelTarget(m_CurrentID));
                m_ClientTarget = false;
            }
        }

        public static void Target(TargetInfo info)
        {
            if (m_Intercept)
            {
                OneTimeResponse(info);
            }
            else if (m_HasTarget)
            {
                info.TargID = m_CurrentID;
                m_LastGroundTarg = m_LastTarget = info;
                ClientCommunication.SendToServer(new TargetResponse(info));
            }

            CancelClientTarget();
            m_HasTarget = false;
        }

        public static void Target(Point3D pt)
        {
            TargetInfo info = new TargetInfo();
            info.Type = 1;
            info.Flags = 0;
            info.Serial = 0;
            info.X = pt.X;
            info.Y = pt.Y;
            info.Z = pt.Z;
            info.Gfx = 0;

            Target(info);
        }

        public static void Target(Point3D pt, int gfx)
        {
            TargetInfo info = new TargetInfo();
            info.Type = 1;
            info.Flags = 0;
            info.Serial = 0;
            info.X = pt.X;
            info.Y = pt.Y;
            info.Z = pt.Z;
            info.Gfx = (ushort)(gfx & 0x3FFF);

            Target(info);
        }

        public static void Target(Serial s)
        {
            TargetInfo info = new TargetInfo();
            info.Type = 0;
            info.Flags = 0;
            info.Serial = s;

            if (s.IsItem)
            {
                Item item = World.FindItem(s);
                if (item != null)
                {
                    info.X = item.Position.X;
                    info.Y = item.Position.Y;
                    info.Z = item.Position.Z;
                    info.Gfx = item.ItemID;
                }
            }
            else if (s.IsMobile)
            {
                Mobile m = World.FindMobile(s);
                if (m != null)
                {
                    info.X = m.Position.X;
                    info.Y = m.Position.Y;
                    info.Z = m.Position.Z;
                    info.Gfx = m.Body;
                }
            }

            Target(info);
        }

        public static void Target(object o)
        {
            if (o is Item)
            {
                Item item = (Item)o;
                TargetInfo info = new TargetInfo();
                info.Type = 0;
                info.Flags = 0;
                info.Serial = item.Serial;
                info.X = item.Position.X;
                info.Y = item.Position.Y;
                info.Z = item.Position.Z;
                info.Gfx = item.ItemID;
                Target(info);
            }
            else if (o is Mobile)
            {
                Mobile m = (Mobile)o;
                TargetInfo info = new TargetInfo();
                info.Type = 0;
                info.Flags = 0;
                info.Serial = m.Serial;
                info.X = m.Position.X;
                info.Y = m.Position.Y;
                info.Z = m.Position.Z;
                info.Gfx = m.Body;
                Target(info);
            }
            else if (o is Serial)
            {
                Target((Serial)o);
            }
            else if (o is TargetInfo)
            {
                Target((TargetInfo)o);
            }
        }

        public static void CheckTextFlags(Mobile m)
        {

   
        }

        public static bool IsLastTarget(Mobile m)
        {
            if (m != null)
            {
                if (Config.GetBool("SmartLastTarget"))
                {
                    if (m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial)
                        return true;
                }
                else
                {
                    if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
                        return true;
                }
            }

            return false;
        }

        private static int m_NextPrevTargIdx = 0;
        public static void NextTarget()
        {
            List<Mobile> list = World.MobilesInRange(12);
            TargetInfo targ = new TargetInfo();
            Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

            if (list.Count <= 0)
            {

                return;
            }

            for (int i = 0; i < 3; i++)
            {
                m_NextPrevTargIdx++;

                if (m_NextPrevTargIdx >= list.Count)
                    m_NextPrevTargIdx = 0;

                m = (Mobile)list[m_NextPrevTargIdx];

                if (m != null && m != World.Player && m != old)
                    break;
                else
                    m = null;
            }

            if (m == null)
                m = old;

            if (m == null)
            {

                return;
            }

            m_LastGroundTarg = m_LastTarget = targ;

            m_LastHarmTarg = m_LastBeneTarg = targ;

            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Type = 0;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;

            /*if ( m_HasTarget )
            {
                 DoLastTarget();
                 ClearQueue();
            }*/
        }

        private static int m_NextPrevTargHumanoidIdx = 0;
        public static void NextTargetHumanoid()
        {
            List<Mobile> mobiles = World.MobilesInRange(12);
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile mob in mobiles)
            {
                if (mob.Body == 0x0190 || mob.Body == 0x0191 || mob.Body == 0x025D || mob.Body == 0x025E)
                    list.Add(mob);
            }

            if (list.Count <= 0)
            {
                return;
            }

            TargetInfo targ = new TargetInfo();
            Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

            for (int i = 0; i < 3; i++)
            {
                m_NextPrevTargHumanoidIdx++;

                if (m_NextPrevTargHumanoidIdx >= list.Count)
                    m_NextPrevTargHumanoidIdx = 0;

                m = (Mobile)list[m_NextPrevTargHumanoidIdx];

                if (m != null && m != World.Player && m != old)
                    break;
                else
                    m = null;
            }

            if (m == null)
                m = old;

            if (m == null)
            {
                return;
            }

            m_LastGroundTarg = m_LastTarget = targ;

            m_LastHarmTarg = m_LastBeneTarg = targ;

            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Type = 0;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;

            /*if ( m_HasTarget )
            {
                 DoLastTarget();
                 ClearQueue();
            }*/
        }

        private static int m_NextPrevTargEnemyHumanoidIdx = 0;
        public static void NextTargetEnemyHumanoid()
        {
            List<Mobile> mobiles = World.MobilesInRange(12);
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile mob in mobiles)
            {
                if (mob.Body == 0x0190 || mob.Body == 0x0191 || mob.Body == 0x025D || mob.Body == 0x025E)
                {
                    if (mob.Notoriety == 5) // Check if they are red
                    {
                        list.Add(mob);
                    }
                }
            }

            if (list.Count <= 0)
            {
                return;
            }

            TargetInfo targ = new TargetInfo();
            Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

            for (int i = 0; i < 3; i++)
            {
                m_NextPrevTargEnemyHumanoidIdx++;

                if (m_NextPrevTargEnemyHumanoidIdx >= list.Count)
                    m_NextPrevTargEnemyHumanoidIdx = 0;

                m = list[m_NextPrevTargEnemyHumanoidIdx];

                if (m != null && m != World.Player && m != old)
                    break;
                else
                    m = null;
            }

            if (m == null)
                m = old;

            if (m == null)
            {
                return;
            }

            m_LastGroundTarg = m_LastTarget = targ;

            m_LastHarmTarg = m_LastBeneTarg = targ;

            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Type = 0;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;

            /*if ( m_HasTarget )
           {
               DoLastTarget();
               ClearQueue();
           }*/
        }
        
        public static void PrevTarget()
        {
            List<Mobile> list = World.MobilesInRange(12);
            TargetInfo targ = new TargetInfo();
            Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

            if (list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                m_NextPrevTargIdx--;

                if (m_NextPrevTargIdx < 0)
                    m_NextPrevTargIdx = list.Count - 1;

                m = (Mobile)list[m_NextPrevTargIdx];

                if (m != null && m != World.Player && m != old)
                    break;
                else
                    m = null;
            }

            if (m == null)
                m = old;

            if (m == null)
            {
                return;
            }

            m_LastGroundTarg = m_LastTarget = targ;

            m_LastHarmTarg = m_LastBeneTarg = targ;

            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Type = 0;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;
        }
        
        public static void PrevTargetHumanoid()
        {
            List<Mobile> mobiles = World.MobilesInRange(12);
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile mob in mobiles)
            {
                if (mob.Body == 0x0190 || mob.Body == 0x0191 || mob.Body == 0x025D || mob.Body == 0x025E)
                    list.Add(mob);
            }

            if (list.Count <= 0)
            {
                return;
            }

            TargetInfo targ = new TargetInfo();
            Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

            for (int i = 0; i < 3; i++)
            {
                m_NextPrevTargHumanoidIdx--;

                if (m_NextPrevTargHumanoidIdx < 0)
                    m_NextPrevTargHumanoidIdx = list.Count - 1;

                m = (Mobile)list[m_NextPrevTargHumanoidIdx];

                if (m != null && m != World.Player && m != old)
                    break;
                else
                    m = null;
            }

            if (m == null)
                m = old;

            if (m == null)
            {
                return;
            }

            m_LastGroundTarg = m_LastTarget = targ;

            m_LastHarmTarg = m_LastBeneTarg = targ;

            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Type = 0;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;

            /*if ( m_HasTarget )
            {
                 DoLastTarget();
                 ClearQueue();
            }*/
        }
        
        public static void PrevTargetEnemyHumanoid()
        {
            List<Mobile> mobiles = World.MobilesInRange(12);
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile mob in mobiles)
            {
                if (mob.Body == 0x0190 || mob.Body == 0x0191 || mob.Body == 0x025D || mob.Body == 0x025E)
                {
                    if (mob.Notoriety == 5) // Check if they are red
                    {
                        list.Add(mob);
                    }
                }
            }

            if (list.Count <= 0)
            {
                return;
            }

            TargetInfo targ = new TargetInfo();
            Mobile m = null, old = World.FindMobile(m_LastTarget == null ? Serial.Zero : m_LastTarget.Serial);

            for (int i = 0; i < 3; i++)
            {
                m_NextPrevTargEnemyHumanoidIdx--;

                if (m_NextPrevTargEnemyHumanoidIdx < 0)
                    m_NextPrevTargEnemyHumanoidIdx = list.Count - 1;

                m = list[m_NextPrevTargEnemyHumanoidIdx];

                if (m != null && m != World.Player && m != old)
                    break;
                else
                    m = null;
            }

            if (m == null)
                m = old;

            if (m == null)
            {
                return;
            }

            m_LastGroundTarg = m_LastTarget = targ;

            m_LastHarmTarg = m_LastBeneTarg = targ;

            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Type = 0;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            ClientCommunication.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;
        }

        public static void CheckLastTargetRange(Mobile m)
        {
            if (World.Player == null)
                return;

            if (m_HasTarget && m != null && m_LastTarget != null && m.Serial == m_LastTarget.Serial && m_QueueTarget == LastTargetAction)
            {
                if (Config.GetBool("RangeCheckLT") )
                {
                    if (Utility.InRange(World.Player.Position, m.Position, Config.GetInt("LTRange")))
                    {
                        if (m_QueueTarget())
                            ClearQueue();
                    }
                }
            }
        }

        private static bool CheckHealPoisonTarg(uint targID, Serial ser)
        {
            if (World.Player == null)
                return false;

            if (targID == m_SpellTargID && ser.IsMobile && (World.Player.LastSpell == Spell.ToID(1, 4) || World.Player.LastSpell == Spell.ToID(4, 5)) && Config.GetBool("BlockHealPoison"))
            {
                Mobile m = World.FindMobile(ser);

                if (m != null && m.Poisoned)
                {
                    return true;
                }
            }

            return false;
        }

        private static void TargetResponse(PacketReader p, PacketHandlerEventArgs args)
        {
            TargetInfo info = new TargetInfo();
            info.Type = p.ReadByte();
            info.TargID = p.ReadUInt32();
            info.Flags = p.ReadByte();
            info.Serial = p.ReadUInt32();
            info.X = p.ReadUInt16();
            info.Y = p.ReadUInt16();
            info.Z = p.ReadInt16();
            info.Gfx = p.ReadUInt16();

            m_ClientTarget = false;

            //if (Config.GetBool("ShowAttackTargetOverhead"))
            //{
            //    Mobile m = World.FindMobile(info.Serial);

            //    if (m != null)
            //    {
            //        if (FriendsAgent.IsFriend(m))
            //        {
            //            World.Player.OverheadMessage(63, $"Target: {m.Name}");
            //        }
            //        else
            //        {
            //            World.Player.OverheadMessage(m.GetNotorietyColorInt(), $"Target: {m.Name}");
            //        }
            //    }
            //}            

            // check for cancel
            if (info.X == 0xFFFF && info.X == 0xFFFF && (info.Serial <= 0 || info.Serial >= 0x80000000))
            {
                m_HasTarget = false;

                if (m_Intercept)
                {
                    args.Block = true;
                    Timer.DelayedCallbackState(TimeSpan.Zero, m_OneTimeRespCallback, info).Start();
                    EndIntercept();

                    if (m_PreviousID != 0)
                    {
                        m_CurrentID = m_PreviousID;
                        m_AllowGround = m_PreviousGround;
                        m_CurFlags = m_PrevFlags;

                        m_PreviousID = 0;

                        ResendTarget();
                    }
                }
                else if (m_FilterCancel.Contains((uint)info.TargID) || info.TargID == LocalTargID)
                {
                    args.Block = true;
                }

                m_FilterCancel.Clear();
                return;
            }

            ClearQueue();

            if (m_Intercept)
            {
                if (info.TargID == LocalTargID)
                {
                    Timer.DelayedCallbackState(TimeSpan.Zero, m_OneTimeRespCallback, info).Start();

                    m_HasTarget = false;
                    args.Block = true;

                    if (m_PreviousID != 0)
                    {
                        m_CurrentID = m_PreviousID;
                        m_AllowGround = m_PreviousGround;
                        m_CurFlags = m_PrevFlags;

                        m_PreviousID = 0;

                        ResendTarget();
                    }

                    m_FilterCancel.Clear();

                    return;
                }
                else
                {
                    EndIntercept();
                }
            }

            m_HasTarget = false;

            if (CheckHealPoisonTarg(m_CurrentID, info.Serial))
            {
                ResendTarget();
                args.Block = true;
            }

            if (info.Serial != World.Player.Serial)
            {
                if (info.Serial.IsValid)
                {
                    // only let lasttarget be a non-ground target

                    m_LastTarget = info;
                    if (info.Flags == 1)
                        m_LastHarmTarg = info;
                    else if (info.Flags == 2)
                        m_LastBeneTarg = info;

                    LastTargetChanged();
                }

                m_LastGroundTarg = info; // ground target is the true last target

               
            }
            else
            {
             
            }

          

            m_FilterCancel.Clear();
        }

        private static void NewTarget(PacketReader p, PacketHandlerEventArgs args)
        {
            bool prevAllowGround = m_AllowGround;
            uint prevID = m_CurrentID;
            byte prevFlags = m_CurFlags;
            bool prevClientTarget = m_ClientTarget;

            m_AllowGround = p.ReadBoolean(); // allow ground
            m_CurrentID = p.ReadUInt32(); // target uid
            m_CurFlags = p.ReadByte(); // flags
                                       // the rest of the packet is 0s
  //         if (m_CurrentID == LocalTargID)
  //             return;
            // check for a server cancel command
            if (!m_AllowGround && m_CurrentID == 0 && m_CurFlags == 3)
            {
                m_HasTarget = false;
                m_ClientTarget = false;
                if (m_Intercept)
                {
                    EndIntercept();
                }
                return;
            }

            if (Spell.LastCastTime + TimeSpan.FromSeconds(3.0) > DateTime.UtcNow && Spell.LastCastTime + TimeSpan.FromSeconds(0.5) <= DateTime.UtcNow && m_SpellTargID == 0)
                m_SpellTargID = m_CurrentID;

            m_HasTarget = true;
            m_ClientTarget = false;

if (m_QueueTarget != null && m_QueueTarget())
            {
                ClearQueue();
                args.Block = true;
            }

            if (args.Block)
            {
                if (prevClientTarget)
                {
                    m_AllowGround = prevAllowGround;
                    m_CurrentID = prevID;
                    m_CurFlags = prevFlags;

                    m_ClientTarget = true;

                    if (!m_Intercept)
                        CancelClientTarget();
                }
            }
            else
            {
                m_ClientTarget = true;

                if (m_Intercept)
                {
                    if (m_OnCancel != null)
                        m_OnCancel();
                    EndIntercept();

                    m_FilterCancel.Add((uint)prevID);
                }
            }
        }

        public static void ResendTarget()
        {
            if (!m_ClientTarget || !m_HasTarget)
            {
                CancelClientTarget();
                m_ClientTarget = m_HasTarget = true;
                ClientCommunication.SendToClient(new Target(m_CurrentID, m_AllowGround, m_CurFlags));
            }
        }
    }
}
