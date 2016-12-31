using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Targeting;

namespace Server.Engines.Games
{
	public class TotalWarGame : BaseTeamGame<TotalWarGameDefinition>
	{
		private int m_MaxKills;
		private Timer m_KillBroadcastTimer;

		public TimeSpan TimeLeft { get { return Duration - ( DateTime.Now - StartTime ); } }
		public int MaxKills { get { return m_MaxKills; } }

		public TotalWarGame( TotalWarGameDefinition definition )
			: base( definition )
		{
		}

		protected override void AttachEventListeners()
		{
			base.AttachEventListeners();

			AttachEventListener( new EquipOnJoinListener( this ) );
			AttachEventListener( new ReequipOnDeathListener( this, statsRestoredPercent: 1.0 ) );
			AttachEventListener( new DetectSkillListener( this ) );
			AttachEventListener( new KillReportListener( this ) );
			AttachEventListener( new GiveHorsesListener( this, returnHorseOnResurrect: true ) );
		}

		public override void OnStarted()
		{
			base.OnStarted();

			m_MaxKills = ComputeMaxKills( GetPlayers().Count() );

			m_KillBroadcastTimer = new KillBroadcastTimer( this );
			m_KillBroadcastTimer.Start();
		}

		private int ComputeMaxKills( int playerCount )
		{
			if ( playerCount < 10 )
				return 25;
			
			if ( playerCount < 20 )
				return 35;

			return 50;
		}

		protected override string GetWinMessage( Team team )
		{
			return String.Format( "Game over! Winner team is {0} with {1} kills.", team.Name, ( (TotalWarTeam) team ).Kills );
		}

		protected override Team SelectWinnerTeam()
		{
			var teams = GetTeams().OfType<TotalWarTeam>();

			var maxKills = teams.Max( team => team.Kills );
			var topTeams = teams.Where( team => team.Kills == maxKills );

			if ( topTeams.Count() == 1 )
				return topTeams.First();
			else
				return null;
		}

		public override void OnKill( Mobile m, Mobile killer )
		{
			base.OnKill( m, killer );

			var team = GetTeamFor( killer ) as TotalWarTeam;
			if ( team != null )
			{
				team.Kills++;

				if ( team.Kills >= m_MaxKills )
					FinishGame();
				else
					SendNotifications();
			}
		}

		private void SendNotifications()
		{
			foreach ( TotalWarTeam team in GetTeams() )
			{
				int leftKills = m_MaxKills - team.Kills;

				if ( leftKills <= 5 )
					this.BroadcastMessage( "The {0} team has only left {1} kills!", team.Name, leftKills );
			}
		}

		public override void OnFinished()
		{
			base.OnFinished();

			if ( m_KillBroadcastTimer != null )
			{
				m_KillBroadcastTimer.Stop();
				m_KillBroadcastTimer = null;
			}
		}

		private class KillBroadcastTimer : Timer
		{
			private TotalWarGame m_Game;

			public KillBroadcastTimer( TotalWarGame game )
				: base( TimeSpan.Zero, TimeSpan.FromMinutes( 5.0 ) )
			{
				m_Game = game;
			}

			protected override void OnTick()
			{
				int leftHours = (int) ( m_Game.TimeLeft.TotalSeconds / 60 / 60 );
				int leftMinutes = (int) ( m_Game.TimeLeft.TotalSeconds / 60 ) % 60;
				int leftSeconds = (int) ( m_Game.TimeLeft.TotalSeconds ) % 60;

				m_Game.BroadcastMessage( "Time left: {0:0}:{1:00}:{2:00}  <>  Scores:", leftHours, leftMinutes, leftSeconds );

				foreach ( TotalWarTeam team in m_Game.GetTeams() )
				{
					m_Game.BroadcastMessage( "{0}: {1}/{2} kills", team.Name, team.Kills, m_Game.MaxKills );
				}
			}
		}
	}
}