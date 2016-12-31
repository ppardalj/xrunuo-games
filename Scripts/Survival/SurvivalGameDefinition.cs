using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Gumps;

namespace Server.Engines.Games
{
	public class SurvivalGameDefinition : GameDefinition
	{
		public override IGame CreateGame()
		{
			return new SurvivalGame( this );
		}

		public override TeamDefinition CreateTeamDefinition()
		{
			throw new NotSupportedException();
		}

		public override GameDefinitionGump CreateDefinitionGump( Manager manager, int gameId )
		{
			return new SurvivalGameDefinitionGump( manager, gameId );
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
