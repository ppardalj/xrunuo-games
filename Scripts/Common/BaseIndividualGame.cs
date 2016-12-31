using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Games
{
	public abstract class BaseIndividualGame<TDefinition> : BaseGame<TDefinition> where TDefinition : GameDefinition
	{
		private HashSet<Mobile> m_Players;

		public BaseIndividualGame( TDefinition definition )
			: base( definition )
		{
		}

		public override IEnumerable<Mobile> GetPlayers()
		{
			return m_Players;
		}

		public override bool IsPlaying( Mobile m )
		{
			return m_Players.Contains( m );
		}

		protected override void Initialize( Embryo embryo )
		{
			m_Players = new HashSet<Mobile>( embryo.GetWaitingPlayers() );
		}

		protected override GameVictoryInfo ComputeVictory()
		{
			string message;
			IEnumerable<Mobile> winners;
			
			if ( m_Players.Count == 1 )
			{
				var winner = m_Players.First();

				message = String.Format( "And the winner is... {0}!!", winner.Name );
				winners = new Mobile[] { winner };
			}
			else
			{
				message = "The game has finished with no winners.";
				winners = Enumerable.Empty<Mobile>();
			}

			return new GameVictoryInfo( message, winners );
		}

		public override int ComputeNotoriety( Mobile from, Mobile target )
		{
			return from == target ?	Notoriety.Ally : Notoriety.Enemy;
		}

		protected override int GetSolidOverrideHue( Mobile m )
		{
			return -1;
		}

		protected override Point3D GetHomeLocation( Mobile m )
		{
			return GetSpawnableLocation();
		}

		protected Point3D GetSpawnableLocation()
		{
			// Try 20 times to find a spawnable location.
			for ( int i = 0; i < 20; i++ )
			{
				var p = GetRandomLocation();

				if ( Map.CanSpawnMobile( p ) )
					return p;
			}

			// Unlikely, do not care whether it is valid or not.
			return GetRandomLocation();
		}

		private Point3D GetRandomLocation()
		{
			int x = Utility.Random( Area.X, Area.Width );
			int y = Utility.Random( Area.Y, Area.Height );

			int z = Map.GetAverageZ( x, y );

			return new Point3D( x, y, z );
		}

		public override void OnLeave( Mobile m, LeaveMode leavemode )
		{
			m_Players.Remove( m );

			base.OnLeave( m, leavemode );
		}
	}
}
