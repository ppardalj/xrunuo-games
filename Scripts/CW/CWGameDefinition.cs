using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Gumps;

namespace Server.Engines.Games
{
	public class CWGameDefinition : GameDefinition
	{
		public override IGame CreateGame()
		{
			return new CWGame( this );
		}

		public override TeamDefinition CreateTeamDefinition()
		{
			return new CWTeamDefinition();
		}

		public override GameDefinitionGump CreateDefinitionGump( Manager manager, int gameId )
		{
			return new CWGameDefinitionGump( manager, gameId );
		}
	}
}
