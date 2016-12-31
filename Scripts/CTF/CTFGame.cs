using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Targeting;

namespace Server.Engines.Games
{
	public class CTFGame : BaseTeamGame<CTFGameDefinition>
	{
		private Timer m_ScoreTimer;

		public TimeSpan TimeLeft { get { return Duration - ( DateTime.Now - StartTime ); } }
		public int MaxScore { get { return m_Definition.MaxScore; } }

		public CTFGame( CTFGameDefinition definition )
			: base( definition )
		{
		}

		protected override void AttachEventListeners()
		{
			base.AttachEventListeners();

			AttachEventListener( new EquipOnJoinListener( this ) );
			AttachEventListener( new DetectSkillListener( this ) );
			AttachEventListener( new KillReportListener( this ) );
			AttachEventListener( new ReequipOnDeathListener( this ) );
		}

		protected override GameRegion CreateRegion()
		{
			return new CTFGameRegion( this );
		}

		public override void OnStarted()
		{
			base.OnStarted();

			m_ScoreTimer = new ScoreTimer( this );
			m_ScoreTimer.Start();
		}

		public override void OnFinished()
		{
			base.OnFinished();

			if ( m_ScoreTimer != null )
			{
				m_ScoreTimer.Stop();
				m_ScoreTimer = null;
			}
		}

		public override void OnWin( Mobile m )
		{
			var MyTeam = GetTeamFor( m ) as CTFTeam;
			int TeamPoints = MyTeam.Score / 3;

			m.BankBox.AddItem( new TournamentTicketFragment( TeamPoints / 4 ) );

			base.OnWin( m );
		}

		public override void OnLose( Mobile m )
		{
			if ( m.Client != null )
			{
				var MyTeam = GetTeamFor( m ) as CTFTeam;
				int TeamPoints = MyTeam.Score / 3;
				int fragments = TeamPoints / 2;

				if ( fragments > 0 )
				{
					m.BankBox.AddItem( new TournamentTicketFragment( fragments ) );
					m.SendMessage( 64, "You and your teammates receive {0} Tournament Ticket Fragments for scoring {1} game points!", fragments, TeamPoints );
				}
			}

			base.OnLose( m );
		}

		protected override string GetWinMessage( Team team )
		{
			return String.Format( "The game has finished! The winner team is {0} team with {1} points!", team.Name, ( (CTFTeam) team ).Score );
		}

		protected override Team SelectWinnerTeam()
		{
			var teams = GetTeams().OfType<CTFTeam>();

			var maxPoints = teams.Max( team => team.Score );
			var topTeams = teams.Where( team => team.Score == maxPoints );

			if ( topTeams.Count() == 1 )
				return topTeams.First();
			else
				return null;
		}

		private class ScoreTimer : Timer
		{
			private CTFGame m_Game;

			public ScoreTimer( CTFGame game )
				: base( TimeSpan.FromMinutes( 5.0 ), TimeSpan.FromMinutes( 5.0 ) )
			{
				m_Game = game;
			}

			protected override void OnTick()
			{
				int leftHours = (int) ( m_Game.TimeLeft.TotalSeconds / 60 / 60 );
				int leftMinutes = (int) ( m_Game.TimeLeft.TotalSeconds / 60 ) % 60;
				int leftSeconds = (int) ( m_Game.TimeLeft.TotalSeconds ) % 60;

				m_Game.BroadcastMessage( "Time left: {0:0}:{1:00}:{2:00}  <>  Scores:", leftHours, leftMinutes, leftSeconds );

				foreach ( CTFTeam team in m_Game.GetTeams() )
				{
					m_Game.BroadcastMessage( "{0}: {1} points", team.Name, team.Score );
				}
			}
		}

		public override void OnSlice()
		{
			base.OnSlice();

			if ( MaxScore > 0 )
			{
				var teams = GetTeams().OfType<CTFTeam>();

				var topTeamScore = teams.Max( team => team.Score );
				var possibleWinners = teams.Where( team => team.Score >= MaxScore && team.Score == topTeamScore );
				if ( possibleWinners.Count() == 1 )
					FinishGame();
			}
		}

		public override void OnLeave( Mobile m, LeaveMode leavemode )
		{
			var flag = m.Backpack.FindItemByType<CTFFlag>();
			if ( flag != null )
			{
				flag.Game.BroadcastMessage( "The {0} flag has been returned to base!", flag.Team.Name );
				flag.ReturnToHome();
			}

			base.OnLeave( m, leavemode );
		}

		public override void OnKill( Mobile victim, Mobile killer )
		{
			base.OnKill( victim, killer );

			var killerTeam = GetTeamFor( killer ) as CTFTeam;
			var victimTeam = GetTeamFor( victim ) as CTFTeam;

			if ( killerTeam != null && killerTeam != victimTeam )
				killerTeam.Score++;
		}
	}
}