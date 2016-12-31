using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Server;
using Server.Engines.Games;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Gumps
{
	public class TotalWarGameDefinitionGump : GameDefinitionGump
	{
		public override string GetGumpHeader()
		{
			return "TOTAL WAR GAME DEFINITION GUMP";
		}

		private TotalWarGameDefinition m_Definition;

		public TotalWarGameDefinitionGump( Manager manager, int definitionId )
			: base( manager, definitionId )
		{
			m_Definition = (TotalWarGameDefinition) Definition;
		}

		public override void OnResponse( GameClient sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			switch ( info.ButtonID )
			{
				default:
					{
						base.OnResponse( sender, info );
						break;
					}
			}
		}
	}
}