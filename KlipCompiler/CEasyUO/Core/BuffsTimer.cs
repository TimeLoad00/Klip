using System;

namespace Assistant
{
	public class BuffsTimer
	{
		//private static int m_Count;
		private static Timer m_Timer;

		
		static BuffsTimer()
		{
		}

		/*public static int Count
		{
			get
			{
				return m_Count;
			}
		}*/

		public static bool Running
		{
			get
			{
				return m_Timer.Running;
			}
		}

		public static void Start()
		{
		

		}

		public static void Stop()
		{
			
		}

		
	}
}
