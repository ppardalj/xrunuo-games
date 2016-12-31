using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.Engines.Games
{
	public class SurvivalGame : BaseIndividualGame<SurvivalGameDefinition>
	{
		private static readonly TimeSpan VortexRespawnTime = TimeSpan.FromMinutes( 1.0 );

		private DateTime m_StartTime;
		private Timer m_RespawnTimer;
		private List<Mobile> m_Vortices;

		public Timer RespawnTimer
		{
			get { return m_RespawnTimer; }
		}

		public SurvivalGame( SurvivalGameDefinition definition )
			: base( definition )
		{
			m_Vortices = new List<Mobile>();
		}

		protected override GameRegion CreateRegion()
		{
			return new SurvivalGameRegion( this );
		}

		public override void OnJoin( Mobile m )
		{
			base.OnJoin( m );

			m.AddItem( new Robe() { Hue = 1194 } );

			m.PlaceInBackpack( new SurvivalInvisPotion() );
			m.PlaceInBackpack( new SurvivalInvisPotion() );
		}

		public override void OnStarted()
		{
			base.OnStarted();

			m_StartTime = DateTime.Now;

			SpawnVortex();

			m_RespawnTimer = Timer.DelayCall( VortexRespawnTime, VortexRespawnTime, () =>
			{
				SpawnVortex();
				this.BroadcastMessage( "The sands of time swirl creating another vortex!" );
			} );
		}

		private void SpawnVortex()
		{
			var vortex = new SurvivalVortex();
			vortex.MoveToWorld( GetSpawnableLocation(), Map );

			m_Vortices.Add( vortex );
		}

		public override void OnSlice()
		{
			base.OnSlice();

			CheckAlive();
		}

		public override void OnDeath( Mobile m )
		{
			base.OnDeath( m );

			LeaveGame( m );
			GiveConsolationReward( m );

			CheckAlive();
		}

		public override void OnWin( Mobile m )
		{
			base.OnWin( m );

			int seconds = (int) ( DateTime.Now - m_StartTime ).TotalSeconds;
			int fragments = seconds / ( 10 * 4 );

			if ( fragments > 0 )
				m.BankBox.AddItem( new TournamentTicketFragment( fragments ) );
			m.SendMessage( 64, String.Format( "You have been given {0} Tournament Ticket Fragments for lasting {1} seconds alive.", fragments, seconds ) );

		}

		private void GiveConsolationReward( Mobile m )
		{
			int seconds = (int) ( DateTime.Now - m_StartTime ).TotalSeconds;

			int reward = Math.Min( 20000, seconds * 15 );

			int fragments = seconds / ( 10 * 2 );

			if ( reward > 0 )
			{
				m.BankBox.AddItem( new BankCheck( reward ) );
				m.BankBox.AddItem( new TournamentTicketFragment( fragments ) );

				Timer.DelayCall( TimeSpan.FromSeconds( 3.0 ), () =>
				{
					m.SendMessage( 64, String.Format( "You have been given {0} gold coins and {1} Tournament Ticket Fragments for lasting {2} seconds alive.", reward, fragments, seconds ) );
				} );
			}
		}

		private void CheckAlive()
		{
			if ( GetPlayers().Count() <= 1 )
				FinishGame();
		}

		public override void OnFinished()
		{
			base.OnFinished();

			if ( m_RespawnTimer != null )
			{
				m_RespawnTimer.Stop();
				m_RespawnTimer = null;
			}

			foreach ( var vortex in m_Vortices.ToArray() )
				vortex.Delete();
		}

		public override int ComputeNotoriety( Mobile from, Mobile target )
		{
			return Notoriety.Innocent;
		}

		public override bool AllowHarmful( Mobile from, Mobile target )
		{
			if ( from is SurvivalVortex || target is SurvivalVortex )
				return true;

			return base.AllowHarmful( from, target );
		}
	}
}
