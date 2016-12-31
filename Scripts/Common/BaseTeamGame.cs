using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;

namespace Server.Engines.Games
{
	public abstract class BaseTeamGame<TDefinition> : BaseGame<TDefinition>, ITeamGame where TDefinition : GameDefinition
	{
		private List<Team> m_Teams;

		public int TeamCount { get { return m_Teams.Count; } }

		public BaseTeamGame( TDefinition definition )
			: base( definition )
		{
			m_Teams = m_Definition.CreateTeams( this );
		}

		public IEnumerable<Team> GetTeams()
		{
			return m_Teams;
		}

		public override IEnumerable<Mobile> GetPlayers()
		{
			return GetTeams().SelectMany( team => team.GetMembers() );
		}

		public override bool IsPlaying( Mobile m )
		{
			return GetTeams().Any( team => team.IsMember( m ) );
		}

		public virtual Team GetTeamFor( Mobile m )
		{
			var clone = m as Clone;

			if ( clone != null )
				return GetTeamFor( clone.Caster );

			var creature = m as BaseCreature;

			if ( creature != null )
			{
				if ( creature.Summoned )
					return GetTeamFor( creature.SummonMaster );

				if ( creature.Controlled )
					return GetTeamFor( creature.ControlMaster );
			}

			return GetTeams().FirstOrDefault( team => team.IsMember( m ) );
		}

		protected override void Initialize( Embryo embryo )
		{
			var waitingPlayers = embryo.GetWaitingPlayers();
			foreach ( var player in waitingPlayers )
			{
				while ( true )
				{
					var team = PickRandomTeam();
					if ( team.ActiveMemberCount < GetTeamsMaxSize() )
					{
						team.AddMember( player );
						player.SendMessage( "You have joined {0} team!", team.Name );

						break;
					}
				}
			}
		}

		private Team PickRandomTeam()
		{
			var teams = GetTeams().ToArray();
			return teams[Utility.RandomMinMax( 0, teams.Length - 1 )];
		}

		private int GetTeamsMaxSize()
		{
			int averagePlayerCountPerTeam = GetTeams().Sum( team => team.ActiveMemberCount ) / TeamCount;
			return averagePlayerCountPerTeam + 1;
		}

		protected override Point3D GetHomeLocation( Mobile m )
		{
			var team = GetTeamFor( m );
			return team.Home;
		}

		protected override int GetSolidOverrideHue( Mobile m )
		{
			var team = GetTeamFor( m );
			return team.Hue;
		}

		protected void RemoveTeam( Team team )
		{
			m_Teams.Remove( team );
		}

		public override void OnStarted()
		{
			foreach ( var team in GetTeams() )
				team.OnStartGame();

			base.OnStarted();
		}

		public override int ComputeNotoriety( Mobile source, Mobile target )
		{
			var sourceTeam = GetTeamFor( source );
			var targetTeam = GetTeamFor( target );

			return sourceTeam == targetTeam ? Notoriety.Ally : Notoriety.Enemy;
		}

		public override void OnLeave( Mobile m, LeaveMode leavemode )
		{
			var team = GetTeamFor( m );

			if ( team != null )
				team.RemoveMember( m );

			if ( leavemode == LeaveMode.Loser )
				OnLose( m );
			
			base.OnLeave( m, leavemode );
		}

		protected override GameVictoryInfo ComputeVictory()
		{
			string message;
			IEnumerable<Mobile> winners;

			var winnerTeam = SelectWinnerTeam();

			if ( winnerTeam != null )
			{
				message = GetWinMessage( winnerTeam );
				winners = winnerTeam.GetMembers();
			}
			else
			{
				message = "The game has finished with no winners.";
				winners = Enumerable.Empty<Mobile>();
			}

			return new GameVictoryInfo( message, winners );
		}

		protected virtual Team SelectWinnerTeam()
		{
			if ( m_Teams.Count == 1 )
				return m_Teams.First();

			return null;
		}

		protected virtual string GetWinMessage( Team team )
		{
			return String.Format( "The game has finished! The winner team is {0} team!", team.Name );
		}

		public override void OnFinished()
		{
			foreach ( var team in GetTeams() )
				team.OnGameFinished();

			base.OnFinished();
		}
	}
}
