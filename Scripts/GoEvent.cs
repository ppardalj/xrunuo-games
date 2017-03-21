using System;
using System.Linq;
using Server;
using Server.Commands;
using Server.Items;

namespace Server.Scripts.Commands
{
	public class GoEventCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register( "GoEvent", AccessLevel.Player, new CommandEventHandler( GoEvent_Command ) );
		}

		private static bool CheckCombat( Mobile m, TimeSpan time )
		{
			for ( int i = 0; i < m.Aggressed.Count; ++i )
			{
				AggressorInfo info = (AggressorInfo) m.Aggressed[i];

				if ( DateTime.Now - info.LastCombatTime < time )
					return true;
			}

			return false;
		}

		[Usage( "GoEvent" )]
		[Description( "Teleports the player to the event waiting room." )]
		private static void GoEvent_Command( CommandEventArgs e )
		{
			Mobile m = (Mobile) e.Mobile;

			if ( m != null )
			{
				bool isAnyGameOpened = false;

				if ( World.Instance.Items.OfType<JoinStone>().Any( stone => stone.IsOpened ) )
					isAnyGameOpened = true;

				if ( !isAnyGameOpened )
				{
					m.SendMessage( 38, "There are no events active at the moment." );
					return;
				}

				if ( m.X > 5180 && m.X < 5197 && m.Y > 1071 && m.Y < 1093 && m.Map == Map.Trammel )
				{
					m.SendMessage( 38, "You are already in the waiting room, do not need to use this command!" );
					return;
				}
				else if ( m.Region.GetLogoutDelay( m ) > TimeSpan.Zero )
				{
					m.SendMessage( 38, "You must be in a safe logout area to use this command." );
					return;
				}
				else if ( m.Criminal )
				{
					m.SendMessage( 38, "You must wait two minutes after committing a criminal act before using this command." );
					return;
				}
				else if ( CheckCombat( m, TimeSpan.FromMinutes( 2.0 ) ) )
				{
					m.SendMessage( 38, "You must wait two minutes after leaving a combat before using this command." );
					return;
				}
				else if ( Factions.Sigil.ExistsOn( m ) )
				{
					m.SendMessage( 38, "You cannot use this command while carrying a Sigil." );
					return;
				}
				else if ( m.Spell != null && m.Spell.IsCasting )
				{
					m.SendMessage( 38, "You cannot use this command while casting a spell." );
					return;
				}
				else if ( m.Poisoned || m.Paralyzed )
				{
					m.SendMessage( 38, "You cannot use this command while paralyzed or poisoned." );
					return;
				}
				else if ( m.IsBodyMod )
				{
					m.SendMessage( 38, "You cannot use this command while polymorphed." );
					return;
				}
				else if ( m.Mounted )
				{
					m.SendMessage( 38, "You cannot use this command while mounted." );
					return;
				}
				else
				{
					Effects.SendLocationParticles( EffectItem.Create( m.Location, m.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
					m.PlaySound( 0x1FE );

					m.MoveToWorld( new Point3D( 5140, 1773, 0 ), Map.Trammel );

					m.PlaySound( 0x1FE );
					Effects.SendLocationParticles( EffectItem.Create( m.Location, m.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );

					m.SendMessage( 64, "You have been teleported successfully to the Waiting Room! Enjoy the event!" );
				}
			}
		}
	}
}
