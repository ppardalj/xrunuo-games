using System;

using Server;
using Server.Mobiles;

namespace Server.Engines.Games
{
	[CorpseName( "a horse corpse" )]
	public class GameHorse : SilverSteed
	{
		[Constructable]
		public GameHorse( Mobile m, int hue )
		{
			Name = "a horse";
			Hue = hue;
			
			SetControlMaster( m );
		}

		public GameHorse( Serial serial )
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
