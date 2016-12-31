using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using Server.Spells.Eighth;
using Server.Spells.Fifth;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Third;
using Server.Spells.Spellweaving;
using Server.Spells.Mysticism;
using Server.Events;

namespace Server.Engines.Games
{
	public class GameRegion : BaseRegion
	{
		public static void Initialize()
		{
			EventSink.Instance.SetAbility += new SetAbilityEventHandler( EventSink_SetAbility );
		}

		private static void EventSink_SetAbility( SetAbilityEventArgs e )
		{
			if ( e.Mobile.Region.IsPartOf( typeof( GameRegion ) ) && e.Index == 12 ) // Shadow Strike
			{
				WeaponAbility.ClearCurrentAbility( e.Mobile );
				e.Mobile.SendMessage( "You cannot use that ability during the game!" );
			}
		}

		private IGame m_Game;

		public GameRegion( IGame game )
			: base( null, game.Map, null, game.Area )
		{
			m_Game = game;
		}

		public override bool AllowAutoClaim( Mobile from )
		{
			return false;
		}

		public override bool CanUseStuckMenu( Mobile m )
		{
			return false;
		}

		public override bool OnSkillUse( Mobile m, SkillName skill )
		{
			if ( skill == SkillName.Hiding )
			{
				m.SendMessage( "You cannot hide during the game!" );
				return false;
			}

			return true;
		}

		private static Type[] m_ForbiddenSpells = new Type[]
			{
				// Travel spells
				typeof( MarkSpell ),			typeof( RecallSpell ),
				typeof( GateTravelSpell ),
				
				// Polymorph spells
				typeof( PolymorphSpell ),		typeof( HorrificBeastSpell ),

				// Summon spells
				typeof( SummonDaemonSpell ),	typeof( AirElementalSpell ),
				typeof( EarthElementalSpell ),	typeof( EnergyVortexSpell ),
				typeof( FireElementalSpell ),	typeof( WaterElementalSpell ),
				typeof( BladeSpiritsSpell ),	typeof( SummonCreatureSpell ),
				typeof( VengefulSpiritSpell ),	typeof( NaturesFurySpell ),
				typeof( RisingColossusSpell ),	typeof( AnimatedWeaponSpell ),	

				// Magic fields
				typeof( EnergyFieldSpell ),		typeof( FireFieldSpell ),
				typeof( PoisonFieldSpell ),		typeof( ParalyzeFieldSpell ),
				typeof( WallOfStoneSpell ),		typeof( WildfireSpell ),

				// Hiding
				typeof( InvisibilitySpell )
			};

		public override bool OnBeginSpellCast( Mobile m, ISpell spell )
		{
			if ( m is CWHealer && spell is TeleportSpell )
				return false;

			if ( m.AccessLevel == AccessLevel.Player && IsForbidden( spell ) )
			{
				m.SendMessage( "That spell is not allowed in this game!" );
				return false;
			}

			return base.OnBeginSpellCast( m, spell );
		}

		protected virtual bool IsForbidden( ISpell s )
		{
			return m_ForbiddenSpells.Contains( s.GetType() );
		}

		public override void OnEnter( Mobile m )
		{
			WeaponAbility.ClearCurrentAbility( m );

			if ( m.Flying && !AllowFlying( m ) )
				m.Flying = false;
		}

		public override void OnExit( Mobile m )
		{
			if ( m_Game.IsPlaying( m ) )
				m_Game.LeaveGame( m, LeaveMode.Illegal );
		}

		public override bool AllowBeneficial( Mobile from, Mobile target )
		{
			return m_Game.AllowBeneficial( from, target );
		}

		public override bool AllowHarmful( Mobile from, Mobile target )
		{
			return m_Game.AllowHarmful( from, target );
		}

		public override bool AllowHousing( Mobile from, Point3D p )
		{
			return from.AccessLevel != AccessLevel.Player;
		}

		public override bool OnHeal( Mobile m, Mobile healer, ref int amount )
		{
			bool success = base.OnHeal( m, healer, ref amount );

			if ( success )
				m_Game.OnHeal( m, healer, amount );

			return success;
		}

		public override bool OnResurrect( Mobile m, Mobile healer )
		{
			bool success = base.OnResurrect( m, healer );

			if ( success )
				m_Game.OnResurrect( m, healer );

			return success;
		}
	}

	public class CTFGameRegion : GameRegion
	{
		public CTFGameRegion( CTFGame game )
			: base( game )
		{
		}

		private static Type[] m_ForbiddenSpells = new Type[]
			{
				typeof( AnimalForm ),
			};

		protected override bool IsForbidden( ISpell s )
		{
			return base.IsForbidden( s ) || m_ForbiddenSpells.Contains( s.GetType() );
		}

		public override bool AllowFlying( Mobile from )
		{
			return false;
		}
	}

	public class SurvivalGameRegion : GameRegion
	{
		public SurvivalGameRegion( SurvivalGame game )
			: base( game )
		{
		}

		public override bool OnSkillUse( Mobile m, SkillName skill )
		{
			if ( skill == SkillName.SpiritSpeak )
			{
				m.SendMessage( "You cannot heal during the game!" );
				return false;
			}

			return base.OnSkillUse( m, skill );
		}

		public override bool AllowFlying( Mobile from )
		{
			return false;
		}
	}
}
