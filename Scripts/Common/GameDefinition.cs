using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Gumps;

namespace Server.Engines.Games
{
	public abstract class GameDefinition
	{
		public abstract IGame CreateGame();
		public abstract TeamDefinition CreateTeamDefinition();
		public abstract GameDefinitionGump CreateDefinitionGump( Manager manager, int gameId);

		private string m_Name;
		private Map m_Map;
		private Rectangle2D m_Area;
		private TimeSpan m_Duration;
		private ISet<TeamDefinition> m_TeamDefinitions;

		[CommandProperty( AccessLevel.Seer )]
		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Map Map
		{
			get { return m_Map; }
			set { m_Map = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Rectangle2D Area
		{
			get { return m_Area; }
			set { m_Area = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public TimeSpan Duration
		{
			get { return m_Duration; }
			set { m_Duration = value; }
		}

		public GameDefinition()
		{
			m_TeamDefinitions = new HashSet<TeamDefinition>();
			m_Map = Map.Internal;
			m_Duration = TimeSpan.FromMinutes( 30.0 );
		}

		public IEnumerable<TeamDefinition> GetTeamDefinitions()
		{
			return m_TeamDefinitions;
		}

		public void AddTeamDefinition( TeamDefinition def )
		{
			m_TeamDefinitions.Add( def );
		}

		public void RemoveTeamDefinition( TeamDefinition def )
		{
			m_TeamDefinitions.Remove( def );
		}

		public List<Team> CreateTeams( IGame game )
		{
			return m_TeamDefinitions.Select( def => def.CreateTeam( game ) ).ToList();
		}

		public virtual void Serialize( GenericWriter writer )
		{
			writer.Write( (int) 0 ); // version

			writer.Write( (string) m_Name );
			writer.Write( (Map) m_Map );
			writer.Write( (Rectangle2D) m_Area );
			writer.Write( (TimeSpan) m_Duration );
						
			writer.Write( (int) m_TeamDefinitions.Count );
			foreach ( var teamDefinition in m_TeamDefinitions )
				teamDefinition.Serialize( writer );
		}

		public virtual void Deserialize( GenericReader reader )
		{
			int version = reader.ReadInt();

			m_Name = reader.ReadString();
			m_Map = reader.ReadMap();
			m_Area = reader.ReadRect2D();
			m_Duration = reader.ReadTimeSpan();

			int teamDefinitionCount = reader.ReadInt();
			for ( int i = 0; i < teamDefinitionCount; i++ )
			{
				var teamDefinition = CreateTeamDefinition();
				teamDefinition.Deserialize( reader );
				m_TeamDefinitions.Add( teamDefinition );
			}
		}
	}
}
