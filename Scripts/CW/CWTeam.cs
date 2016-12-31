using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Games
{
	public class CWTeam : Team
	{
		private CWTeamDefinition m_Definition;
		private CWHealer m_Healer;

		[CommandProperty( AccessLevel.Seer )]
		public CWHealer Healer
		{
			get { return m_Healer; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Point3D HealerHome
		{
			get { return m_Definition.HealerHome; }
		}

		public CWTeam( CWTeamDefinition definition, IGame game )
			: base( definition, game )
		{
			m_Definition = definition;
		}

		public override void OnStartGame()
		{
			base.OnStartGame();

			m_Healer = new CWHealer( this );
			m_Healer.MoveToWorld( HealerHome, Map );
		}
		
		public override void OnGameFinished()
		{
			base.OnGameFinished();

			if ( m_Healer != null )
				m_Healer.Delete();
		}

		public override bool IsAlive()
		{
			bool healerAlive = m_Healer != null && !m_Healer.Deleted;
			
			return healerAlive || base.IsAlive();
		}

		public void OnHealerDeath()
		{
			foreach ( Mobile m in GetMembers() )
			{
				if ( m.Alive )
					m.SendMessage( 0x28, "Move with caution! The healer of your team is dead, now you can not resurrect if you die!" );
				else
					m.SendMessage( 0x28, "The healer of your team has died, now you can only be resurrected by a teammate!" );
			}

			Game.BroadcastMessage( "{0} healer has fallen!", Name );

			m_Healer = null;
		}
	}
}
