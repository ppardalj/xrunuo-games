using System;

using Server;
using Server.Engines.Games;

namespace Server.Gumps
{
	public class SurvivalGameDefinitionGump : GameDefinitionGump
	{
		public override string GetGumpHeader()
		{
			return "SURVIVAL GAME DEFINITION GUMP";
		}

		public SurvivalGameDefinitionGump( Manager manager, int definitionId )
			: base( manager, definitionId )
		{
		}
	}
}