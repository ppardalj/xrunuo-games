using System;
using System.Collections;

using Server;
using Server.Items;

namespace Server.Engines.Games
{
	public class CTFTeam : Team
	{
		private CTFGame m_Game;
		private CTFTeamDefinition m_Definition;
		private CTFFlag m_Flag;
		private int m_Score;

		public CTFFlag Flag
		{
			get { return m_Flag; }
			set { m_Flag = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public int Score
		{
			get { return m_Score; }
			set { m_Score = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Point3D FlagHome
		{
			get { return m_Definition.FlagHome; }
		}

		public CTFTeam( CTFTeamDefinition definition, CTFGame game )
			: base( definition, game )
		{
			m_Game = game;
			m_Definition = definition;
		}

		public override void OnStartGame()
		{
			base.OnStartGame();

			m_Flag = new CTFFlag( m_Game, this );
			m_Flag.MoveToWorld( FlagHome, Map );
		}

		public override void OnGameFinished()
		{
			base.OnGameFinished();

			if ( m_Flag != null )
			{
				m_Flag.Delete();
				m_Flag = null;
			}
		}
	}
}
