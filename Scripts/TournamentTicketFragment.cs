using System;
using System.Collections;
using Server.Spells;

namespace Server.Items
{
	public class TournamentTicketFragment : Item
	{
		[Constructable]
		public TournamentTicketFragment()
			: this( 1 )
		{
		}

		[Constructable]
		public TournamentTicketFragment( int amount )
			: base( 5154 )
		{
			Stackable = true;
			Weight = 0.1;
			Hue = 1161;
			Name = "Tournament Ticket Fragment";
			Amount = amount;
			Label1 = "Collect 100 to claim a Tournament Ticket";
		}

		public override void OnDoubleClick( Mobile from )
		{
			base.OnDoubleClick( from );

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
				return;
			}

			if ( this.Amount >= 100 )
			{
				if ( this.Amount == 100 )
				{
					this.Delete();
				}
				else
				{
					this.Amount -= 100;
				}
				from.Backpack.AddItem( new TournamentTicket() );
				from.SendMessage( "You combine 100 fragments and obtain a full Tournament Ticket!" );
			}
		}

		public TournamentTicketFragment( Serial serial )
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