using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Games
{
	public abstract class Team
	{
		private TeamDefinition m_Definition;
		private IGame m_Game;
		private List<Mobile> m_Members;

		public IGame Game { get { return m_Game; } }

		[CommandProperty( AccessLevel.Seer )]
		public string Name
		{
			get { return m_Definition.Name; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public int Hue
		{
			get { return m_Definition.Hue; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Point3D Home
		{
			get { return m_Definition.Home; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Map Map
		{
			get { return Game.Map; }
		}

		public Team( TeamDefinition definition, IGame game )
		{
			m_Definition = definition;
			m_Game = game;
			m_Members = new List<Mobile>();
		}

		public virtual void OnStartGame()
		{
		}

		public virtual void OnGameFinished()
		{
			foreach ( var player in m_Members.ToArray() )
			{
				m_Game.LeaveGame( player );
			}
		}

		[CommandProperty( AccessLevel.Counselor )]
		public int ActiveMemberCount
		{
			get { return m_Members.Count( m => m.NetState != null ); }
		}

		public IEnumerable<Mobile> GetMembers()
		{
			return m_Members;
		}

		public bool IsMember( Mobile m )
		{
			return m_Members.Contains( m );
		}

		public void AddMember( Mobile m )
		{
			m_Members.Add( m );
		}

		public void RemoveMember( Mobile m )
		{
			m_Members.Remove( m );
		}

		public virtual bool IsAlive()
		{
			return m_Members.Where( m => m.Alive && m.NetState != null ).Any();
		}
	}
}
