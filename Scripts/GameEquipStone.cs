using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Mobiles;

namespace Server.Items
{
	public class GameEquipStone : Item
	{
		private static HashSet<Mobile> m_AlreadyUsed = new HashSet<Mobile>();

		[Constructable]
		public GameEquipStone()
			: base( 0x1183 )
		{
			this.Movable = false;
			this.Name = "Equip stone";
		}

		public GameEquipStone( Serial serial )
			: base( serial )
		{
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !from.InLOS( this.GetWorldLocation() ) )
			{
				from.SendLocalizedMessage( 502800 ); // You can't see that.
			}
			else if ( from.GetDistanceToSqrt( this.GetWorldLocation() ) > 4 )
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
			}
			else if ( m_AlreadyUsed.Contains( from ) )
			{
				from.SendMessage( "You have already used this stone." );
			}
			else
			{
				GiveEquipTo( from );
				m_AlreadyUsed.Add( from );
			}
		}

		public static void Reset()
		{
			m_AlreadyUsed.Clear();
		}

		public static void GiveEquipTo( Mobile m )
		{
			GiveLeatherArmor( m );
			GivePotions( m );
			GiveSpellbooksAndReagents( m );
			GiveBandages( m );
			GiveShield( m );
			GiveWeapons( m );
		}

		private static void GivePotions( Mobile m )
		{
			Container pouch = new Pouch() { Label1 = "Potions", Hue = 0x25 };

			pouch.DropItem( new GreaterExplosionPotion() { Amount = 10 } );
			pouch.DropItem( new GreaterRefreshPotion() { Amount = 10 } );
			pouch.DropItem( new GreaterCurePotion() { Amount = 10 } );
			pouch.DropItem( new GreaterHealPotion() { Amount = 10 } );
			pouch.DropItem( new GreaterStrengthPotion() { Amount = 10 } );
			pouch.DropItem( new GreaterAgilityPotion() { Amount = 10 } );

			GiveItem( m, pouch );

			if ( m.Skills[SkillName.Poisoning].Value >= 30.0 )
				GiveItem( m, new DeadlyPoisonPotion() { Amount = 5 } );
		}

		public static void GiveLeatherArmor( Mobile m )
		{
			if ( m.Race == Race.Human )
				GiveHumanLeatherArmor( m );
			else if ( m.Race == Race.Elf )
				GiveElvenLeatherArmor( m );
			else if ( m.Race == Race.Gargoyle )
				GiveGargishLeatherArmor( m );
		}

		private static void GiveHumanLeatherArmor( Mobile m )
		{
			GiveLeatherArmorPiece( m, new LeatherArms(), lmc: 4 );
			GiveLeatherArmorPiece( m, new LeatherChest(), lmc: 4 );
			GiveLeatherArmorPiece( m, new LeatherGloves(), lmc: 3 );
			GiveLeatherArmorPiece( m, new LeatherGorget(), lmc: 3 );
			GiveLeatherArmorPiece( m, new LeatherLegs(), lmc: 3 );
			GiveLeatherArmorPiece( m, new LeatherCap(), lmc: 3 );
		}

		private static void GiveElvenLeatherArmor( Mobile m )
		{
			GiveLeatherArmorPiece( m, new LeafArms(), lmc: 4 );
			GiveLeatherArmorPiece( m, new LeafTunic(), lmc: 4 );
			GiveLeatherArmorPiece( m, new LeafGloves(), lmc: 4 );
			GiveLeatherArmorPiece( m, new LeafGorget(), lmc: 4 );
			GiveLeatherArmorPiece( m, new LeafLeggings(), lmc: 4 );
		}

		private static void GiveGargishLeatherArmor( Mobile m )
		{
			GiveLeatherArmorPiece( m, new GargishLeatherArms(), lmc: 5 );
			GiveLeatherArmorPiece( m, new GargishLeatherChest(), lmc: 5 );
			GiveLeatherArmorPiece( m, new GargishLeatherLeggings(), lmc: 5 );
			GiveLeatherArmorPiece( m, new GargishLeatherKilt(), lmc: 5 );
		}

		private static void GiveLeatherArmorPiece( Mobile m, BaseArmor armor, int lmc = 0 )
		{
			armor.Exceptional = true;
			armor.Attributes.LowerManaCost = lmc;
			EquipItem( m, armor );
		}

		private static void GiveSpellbooksAndReagents( Mobile m )
		{
			bool giveMageRegs = false, giveNecroRegs = false, giveMysticRegs = false;

			if ( m.Skills[SkillName.Magery].Value >= 30.0 )
			{
				giveMageRegs = true;
				GiveItem( m, new Spellbook() { Content = ulong.MaxValue } );
			}

			if ( m.Skills[SkillName.Chivalry].Value >= 30.0 )
			{
				GiveItem( m, new BookOfChivalry() );
			}

			if ( m.Skills[SkillName.Necromancy].Value >= 30.0 )
			{
				giveNecroRegs = true;
				GiveItem( m, new NecromancerSpellbook() { Content = 65535 } );
			}

			if ( m.Skills[SkillName.Bushido].Value >= 30.0 )
			{
				GiveItem( m, new BookOfBushido() );
			}

			if ( m.Skills[SkillName.Ninjitsu].Value >= 30.0 )
			{
				GiveItem( m, new BookOfNinjitsu() );
			}

			if ( m.Skills[SkillName.Spellweaving].Value >= 30.0 && m is PlayerMobile && ( (PlayerMobile) m ).Arcanist )
			{
				GiveItem( m, new SpellweavingSpellbook() { Content = 65535 } );
			}

			if ( m.Skills[SkillName.Mysticism].Value >= 30.0 )
			{
				giveMageRegs = true;
				giveMysticRegs = true;
				GiveItem( m, new MysticSpellbook() { Content = 65535 } );
			}

			Bag bag = new Bag() { Label1 = "Reagents" };
			int amount = 100;

			if ( giveMageRegs )
			{
				bag.DropItem( new BlackPearl( amount ) );
				bag.DropItem( new Bloodmoss( amount ) );
				bag.DropItem( new Garlic( amount ) );
				bag.DropItem( new Ginseng( amount ) );
				bag.DropItem( new MandrakeRoot( amount ) );
				bag.DropItem( new Nightshade( amount ) );
				bag.DropItem( new SulfurousAsh( amount ) );
				bag.DropItem( new SpidersSilk( amount ) );
			}

			if ( giveNecroRegs )
			{
				bag.DropItem( new BatWing( amount ) );
				bag.DropItem( new GraveDust( amount ) );
				bag.DropItem( new DaemonBlood( amount ) );
				bag.DropItem( new NoxCrystal( amount ) );
				bag.DropItem( new PigIron( amount ) );
			}

			if ( giveMysticRegs )
			{
				bag.DropItem( new DaemonBone( amount ) );
				bag.DropItem( new Bone( amount ) );
				bag.DropItem( new FertileDirt( amount ) );
				bag.DropItem( new DragonsBlood( amount ) );
			}

			if ( bag.Items.Count > 0 )
				GiveItem( m, bag );
			else
				bag.Delete();
		}

		private static void GiveBandages( Mobile m )
		{
			if ( m.Skills[SkillName.Healing].Value >= 30.0 )
			{
				GiveItem( m, new Bandage( 100 ) );
			}
		}

		private static void GiveShield( Mobile m )
		{
			if ( m.Skills[SkillName.Parry].Value >= 30.0 )
			{
				BaseShield shield = null;
				if ( m.Race != Race.Gargoyle )
					shield = new MetalKiteShield();
				else
					shield = new GargishKiteShield();

				shield.Attributes.SpellChanneling = 1;
				shield.Attributes.CastSpeed = 1;

				GiveItem( m, shield );
			}
		}

		private static void GiveWeapons( Mobile m )
		{
			if ( m.Skills[SkillName.Archery].Value >= 30.0 )
			{
				GiveWeapon( m, new Bow() );
				GiveWeapon( m, new Crossbow() );
				GiveWeapon( m, new HeavyCrossbow() );
				GiveWeapon( m, new CompositeBow() );

				GiveItem( m, new Bolt( 100 ) );
				GiveItem( m, new Arrow( 100 ) );
			}

			if ( m.Skills[SkillName.Throwing].Value >= 30.0 )
			{
				GiveWeapon( m, new Boomerang() );
				GiveWeapon( m, new Cyclone() );
				GiveWeapon( m, new SoulGlaive() );
			}

			if ( m.Skills[SkillName.Macing].Value >= 30.0 )
			{
				if ( m.Race != Race.Gargoyle )
				{
					GiveWeapon( m, new WarAxe() );
					GiveWeapon( m, new WarHammer() );
				}
				else
				{
					GiveWeapon( m, new DiscMace() );
					GiveWeapon( m, new GargishWarHammer() );
				}
			}

			if ( m.Skills[SkillName.Swords].Value >= 30.0 )
			{
				if ( m.Skills[SkillName.Lumberjacking].Value >= 30.0 )
				{
					if ( m.Race != Race.Gargoyle )
					{
						GiveWeapon( m, new Hatchet() );
						GiveWeapon( m, new LargeBattleAxe() );
					}
					else
					{
						GiveWeapon( m, new GargishAxe() );
						GiveWeapon( m, new DualShortAxes() );
					}
				}

				if ( m.Race != Race.Gargoyle )
				{
					GiveWeapon( m, new Halberd() );
					GiveWeapon( m, new Katana() );
				}
				else
				{
					GiveWeapon( m, new GargishTalwar() );
					GiveWeapon( m, new GargishKatana() );
				}

				if ( m.Skills[SkillName.Poisoning].Value >= 30.0 )
				{
					if ( m.Race != Race.Gargoyle )
						GiveWeapon( m, new Cleaver() );
					else
						GiveWeapon( m, new GargishCleaver() );
				}
			}

			if ( m.Skills[SkillName.Fencing].Value >= 30.0 )
			{
				if ( m.Race != Race.Gargoyle )
					GiveWeapon( m, new ShortSpear() );
				else
					GiveWeapon( m, new DualPointedSpear() );

				if ( m.Skills[SkillName.Parry].Value >= 30.0 || m.Skills[SkillName.Poisoning].Value >= 30.0 )
				{
					if ( m.Race != Race.Gargoyle )
						GiveWeapon( m, new Kryss() );
					else
						GiveWeapon( m, new GargishKryss() );
				}
				else
				{
					if ( m.Race != Race.Gargoyle )
						GiveWeapon( m, new Spear() );
					else
						GiveWeapon( m, new GargishPike() );
				}
			}
		}

		private static void GiveWeapon( Mobile m, BaseWeapon weapon )
		{
			weapon.Exceptional = true;
			weapon.Attributes.SpellChanneling = 1;
			weapon.Attributes.CastSpeed = 1; // Avoid Faster Casting -1.
			weapon.Attributes.WeaponDamage = 30;

			if ( weapon.Speed > 18 )
				weapon.Attributes.WeaponSpeed = 25;
			else if ( weapon.Speed > 16 )
				weapon.Attributes.WeaponSpeed = 20;
			else if ( weapon.Speed > 14 )
				weapon.Attributes.WeaponSpeed = 15;
			else if ( weapon.Speed > 10 )
				weapon.Attributes.WeaponSpeed = 5;

			GiveItem( m, weapon );
		}

		private static void GiveItem( Mobile m, Item item )
		{
			if ( !m.PlaceInBackpack( item ) )
				item.Delete();
		}

		private static void EquipItem( Mobile m, Item item )
		{
			if ( !m.EquipItem( item ) && !m.PlaceInBackpack( item ) )
				item.Delete();
		}
	}
}
