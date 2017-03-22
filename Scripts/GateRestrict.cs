using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;

namespace Server.Engines.Games
{
	public class GateRestrict : Moongate
	{
		public override bool ForceShowProperties { get { return true; } }
		public override bool TeleportPets { get { return false; } }

		[Constructable()]
		public GateRestrict()
		{
			Hue = 1161;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !UsarGate( from ) )
				return;
		}

		public override bool OnMoveOver( Mobile m )
		{
			return !m.Player || UsarGate( m );
		}

		public bool UsarGate( Mobile m )
		{
			if ( Server.Spells.SpellHelper.CheckCombat( m ) )
			{
				m.SendLocalizedMessage( 1005564, "", 0x22 ); // Wouldst thou flee during the heat of battle??
				return false;
			}
			else if ( m.Mounted )
			{
				m.SendMessage( 0x22, "You shall not pass while mounted." );
				return false;
			}
			else if ( m.IsBodyMod )
			{
				m.SendMessage( 0x22, "You shall not pass while polymorphed." );
				return false;
			}
			else
			{
				CheckGate( m, 0 );
				return true;
			}
		}

		public GateRestrict( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );//version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();
		}
	}
}
