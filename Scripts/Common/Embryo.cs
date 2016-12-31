using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Accounting;

namespace Server.Engines.Games
{
	public class Embryo
	{
		private List<Mobile> m_WaitingPlayers;

		public Embryo()
		{
			m_WaitingPlayers = new List<Mobile>();
		}

		public void JoinWaitingList( Mobile m )
		{
			if ( !m.Alive )
				m.Resurrect();

			BankHelper.BankItems( m );

			// Move to the "pre-event room", the big red one in ilshenar.
			m.MoveToWorld( new Point3D( 775, 1479, -28 ), Map.Maps[34] );

			GameHelper.AddToAddressList( m );

			m_WaitingPlayers.Add( m );
		}

		public IEnumerable<Mobile> GetWaitingPlayers()
		{
			return m_WaitingPlayers.Where(
				( m ) =>
				{
					// TODO: Raise event to notify other systems (f.i. jail) about game elegibility.
					return true;
				} );
		}
	}
}
