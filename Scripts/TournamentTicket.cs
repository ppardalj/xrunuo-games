using System;
using System.Collections;
using Server.Spells;

namespace Server.Items
{
	[TypeAlias( "Server.Items.TicketDeTorneo" )]
	public class TournamentTicket : Item
	{
		[Constructable]
		public TournamentTicket()
			: this( 1 )
		{
		}

		[Constructable]
		public TournamentTicket( int amount )
			: base( 0xEF3 )
		{
			Stackable = true;
			Weight = 1.0;
			Hue = 1161;
			Name = "Tournament Ticket";
			Amount = amount;
		}

		public TournamentTicket( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();
		}
	}
}
