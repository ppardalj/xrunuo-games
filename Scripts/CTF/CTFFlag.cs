using System;

using Server;
using Server.Engines.Games;
using Server.Gumps;
using Server.Targeting;

namespace Server.Items
{
	public class CTFFlag : Item
	{
		private CTFGame m_Game;
		private CTFTeam m_Team;
		private Timer m_ReturnTimer;
		private bool m_IsAtHome;

		public override bool DisplayLootType { get { return false; } }

		[Constructable]
		public CTFFlag( CTFGame game, CTFTeam team )
			: base( 0x1627 )
		{
			m_Game = game;
			m_Team = team;

			this.Movable = false;
			this.Weight = 1.0;
			this.Hue = team.Hue;

			m_IsAtHome = true;

			LootType = LootType.Cursed;
		}

		public CTFFlag( Serial serial )
			: base( serial )
		{
		}

		public CTFGame Game { get { return m_Game; } }
		public CTFTeam Team { get { return m_Team; } }
		public bool IsAtHome { get { return m_IsAtHome; } }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();

			Delete();
		}

		public void ReturnToHome()
		{
			if ( !m_IsAtHome )
			{
				MoveToWorld( m_Team.FlagHome, m_Team.Map );
				m_IsAtHome = true;
			}

			if ( m_ReturnTimer != null )
			{
				m_ReturnTimer.Stop();
				m_ReturnTimer = null;
			}
		}

		public void BeginCapture()
		{
			if ( m_ReturnTimer != null && m_ReturnTimer.Running )
				m_ReturnTimer.Stop();

			m_ReturnTimer = new ReturnTimer( this );
			m_ReturnTimer.Start();

			m_IsAtHome = false;
		}

		public override void OnAdded( object parent )
		{
			Mobile m = this.RootParent as Mobile;

			if ( m != null )
				m.SolidHueOverride = 0x496; // BRIGHT orange (brighter than blaze)
		}

		public override void OnRemoved( object oldParent )
		{
			Mobile m = null;

			if ( oldParent is Item )
				m = ( (Item) oldParent ).RootParent as Mobile;
			else
				m = oldParent as Mobile;

			if ( m != null )
			{
				var team = Game.GetTeamFor( m );
				m.SolidHueOverride = team != null ? team.Hue : -1;
			}
		}

		public override void OnParentDeleted( object parent )
		{
			ReturnToHome();
		}

		public override DeathMoveResult OnInventoryDeath( Mobile parent )
		{
			parent.SolidHueOverride = Game.GetTeamFor( parent ).Hue;

			Timer.DelayCall( TimeSpan.Zero,
				delegate
				{
					if ( !( RootParent is Mobile ) && !IsAtHome )
						MoveToWorld( GetWorldLocation(), Map );
				} );

			return DeathMoveResult.MoveToCorpse;
		}

		public override bool Decays { get { return false; } }

		public override bool OnMoveOver( Mobile from )
		{
			if ( !base.OnMoveOver( from ) )
				return false;

			if ( !from.Alive )
				return true;

			Team team = Game.GetTeamFor( from );

			if ( team != null && from.Backpack != null )
			{
				if ( team != m_Team )
				{
					from.RevealingAction();
					from.Backpack.DropItem( this );
					from.SendMessage( "You got the enemy flag!" );

					BeginCapture();

					Game.BroadcastMessage( "{0} ({1}) got the {2} flag!", from.Name, team.Name, m_Team.Name );
				}
				else
				{
					if ( !m_IsAtHome )
					{
						Game.BroadcastMessage( "{0} has returned the {1} flag!", from.Name, m_Team.Name );
						ReturnToHome();
					}
					else
					{
						CTFFlag flag = from.Backpack.FindItemByType<CTFFlag>();
						if ( flag != null )
						{
							// Capture the flag!
							from.SendMessage( "You captured the {0} flag!", flag.Team.Name );

							flag.Game.BroadcastMessage( "{0} ({1}) captured the {2} flag!", from.Name, team.Name, flag.Team.Name );
							flag.ReturnToHome();

							( (CTFTeam) team ).Score += 15;
						}
					}
				}
			}

			return true;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.Target = new PassTarget( this );
				from.SendMessage( "Target a team-mate to pass the flag." );
			}
		}

		private class PassTarget : Target
		{
			private CTFFlag m_Flag;

			public PassTarget( CTFFlag flag )
				: base( 3, false, TargetFlags.None )
			{
				m_Flag = flag;
			}

			protected override void OnTarget( Mobile from, object target )
			{
				CTFTeam fteam = m_Flag.Game.GetTeamFor( from ) as CTFTeam;
				if ( target is Mobile )
				{
					Mobile targ = (Mobile) target;
					Team tteam = m_Flag.Game.GetTeamFor( targ );
					if ( tteam == fteam && from != targ )
					{
						if ( targ.Backpack != null )
						{
							targ.Backpack.DropItem( m_Flag );
							targ.SendMessage( "{0} gave you the {1} flag!", from.Name, m_Flag.Team.Name );

							m_Flag.Game.BroadcastMessage( "{0} passed the {1} flag to {2}!", from.Name, m_Flag.Team.Name, targ.Name );
						}
					}
					else
					{
						from.SendMessage( "You cannot give the flag to them!" );
					}
				}
			}
		}

		private class ReturnTimer : Timer
		{
			public static readonly TimeSpan MaxFlagHoldTime = TimeSpan.FromMinutes( 3.0 );

			private CTFFlag m_Flag;
			private DateTime m_Start;

			public ReturnTimer( CTFFlag flag )
				: base( TimeSpan.Zero, TimeSpan.FromSeconds( 30.0 ) )
			{
				m_Flag = flag;
				m_Start = DateTime.Now;
				}

			protected override void OnTick()
			{
				Mobile owner = m_Flag.RootParent as Mobile;

				TimeSpan left = MaxFlagHoldTime - ( DateTime.Now - m_Start );

				if ( left >= TimeSpan.FromSeconds( 1.0 ) )
				{
					if ( left > TimeSpan.FromMinutes( 1.0 ) )
						Interval = TimeSpan.FromSeconds( 30.0 );
					else if ( left > TimeSpan.FromSeconds( 30.0 ) )
						Interval = TimeSpan.FromSeconds( 15.0 );
					else if ( left >= TimeSpan.FromSeconds( 10.0 ) )
						Interval = TimeSpan.FromSeconds( 5.0 );
					else
						Interval = TimeSpan.FromSeconds( 1.0 );

					if ( owner != null )
						owner.SendMessage( "You must take the {0} flag to your flag in {1} seconds or be killed!", m_Flag.Team.Name, (int) left.TotalSeconds );
				}
				else
				{
					if ( owner != null )
					{
						owner.BoltEffect( 0 );
						owner.PlaySound( 0xDC ); // snake hiss
						owner.Kill();
					}

					m_Flag.Game.BroadcastMessage( "The {0} flag has been returned to base!", m_Flag.Team.Name );
					m_Flag.ReturnToHome();

					Stop();
				}
			}
		}
	}
}

