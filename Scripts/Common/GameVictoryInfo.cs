using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Games
{
	public class GameVictoryInfo
	{
		private string m_Message;
		private IEnumerable<Mobile> m_Winners;

		public string Message { get { return m_Message; } }
		public IEnumerable<Mobile> Winners { get { return m_Winners; } }

		public GameVictoryInfo( string message, IEnumerable<Mobile> winners )
		{
			m_Message = message;
			m_Winners = winners;
		}
	}
}
