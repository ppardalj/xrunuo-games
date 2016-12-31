using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Games
{
	public class CWGame : BaseTeamGame<CWGameDefinition>
	{
		private static readonly TimeSpan HealerDeathTime = TimeSpan.FromMinutes( 10.0 );
		private static readonly TimeSpan SuddenDeathTime = TimeSpan.FromMinutes( 15.0 );

		private Timer m_StateTimer;
		private bool m_InSuddenDeath;

		public Timer StateTimer
		{
			get { return m_StateTimer; }
		}

		public bool InSuddenDeath
		{
			get { return m_InSuddenDeath; }
		}

		public CWGame( CWGameDefinition definition )
			: base( definition )
		{
		}

		public override bool OverridesNotoriety( Mobile from, Mobile target )
		{
			if ( target is CWHealer )
				return true;

			return base.OverridesNotoriety( from, target );
		}

		public override Team GetTeamFor( Mobile m )
		{
			var healer = m as CWHealer;

			if ( healer != null )
				return healer.Team;

			return base.GetTeamFor( m );
		}

		protected override void AttachEventListeners()
		{
			base.AttachEventListeners();

			AttachEventListener( new EquipOnJoinListener( this ) );
			AttachEventListener( new GiveHorsesListener( this ) );
			AttachEventListener( new DetectSkillListener( this ) );
			AttachEventListener( new KillReportListener( this ) );
		}

		private void KickDefeatedTeams()
		{
			foreach ( var team in GetTeams().ToArray() )
			{
				if ( !team.IsAlive() )
					KickTeam( team );
			}
		}

		private void KillHealers()
		{
			bool message = false;

			foreach ( CWTeam team in GetTeams() )
			{
				if ( team.Healer != null )
				{
					team.Healer.Kill();

					message = true;
				}
			}

			if ( message )
				this.BroadcastMessage( "After 10 minutes, the healers of all teams have reached their time limit and have fallen!" );

			m_StateTimer = Timer.DelayCall( SuddenDeathTime - HealerDeathTime, new TimerCallback( TurnToSuddenDeath ) );
		}

		private void TurnToSuddenDeath()
		{
			this.BroadcastMessage( "After 15 minutes, the game has gone into sudden death! If you die now you will be automatically disqualified!" );

			m_InSuddenDeath = true;
		}

		private void KickTeam( Team team )
		{
			foreach ( var m in team.GetMembers().ToArray() )
			{
				LeaveGame( m, LeaveMode.Loser );
				m.SendMessage( 0x28, "All members of your team and your healer died. Your team has been disqualified from the game." );
			}

			RemoveTeam( team );

			this.BroadcastMessage( "The {0} has been disqualified from the game.", team.Name );
		}

		public override void OnStarted()
		{
			base.OnStarted();

			m_StateTimer = Timer.DelayCall( HealerDeathTime, new TimerCallback( KillHealers ) );
		}

		public override void OnFinished()
		{
			base.OnFinished();

			if ( m_StateTimer != null )
			{
				m_StateTimer.Stop();
				m_StateTimer = null;
			}
		}

		public override void OnDeath( Mobile m )
		{
			base.OnDeath( m );

			if ( InSuddenDeath )
				LeaveGame( m, LeaveMode.Loser );
		}

		public override void OnSlice()
		{
			base.OnSlice();

			KickDefeatedTeams();

			if ( TeamCount <= 1 )
				FinishGame();
		}
	}
}
