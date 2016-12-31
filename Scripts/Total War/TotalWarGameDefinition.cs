using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Gumps;

namespace Server.Engines.Games
{
	public class TotalWarGameDefinition : GameDefinition
	{
		public override IGame CreateGame()
		{
			return new TotalWarGame( this );
		}

		public override TeamDefinition CreateTeamDefinition()
		{
			return new TotalWarTeamDefinition();
		}

		public override GameDefinitionGump CreateDefinitionGump( Manager manager, int gameId )
		{
			return new TotalWarGameDefinitionGump( manager, gameId );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( version < 1 )
				reader.ReadInt();
		}
	}
}
