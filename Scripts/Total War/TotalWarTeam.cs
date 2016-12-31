using System;
using System.Collections;

using Server;
using Server.Items;

namespace Server.Engines.Games
{
	public class TotalWarTeam : Team
	{
		private int m_Points;

		[CommandProperty( AccessLevel.Seer )]
		public int Kills
		{
			get { return m_Points; }
			set { m_Points = value; }
		}

		public TotalWarTeam( TotalWarTeamDefinition definition, IGame game )
			: base( definition, game )
		{
		}
	}
}
