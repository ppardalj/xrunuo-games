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
	public class CWGameDefinitionGump : GameDefinitionGump
	{
		public override string GetGumpHeader()
		{
			return "CW GAME DEFINITION GUMP";
		}

		private CWTeamDefinition[] m_Teams;

		public CWGameDefinitionGump( Manager manager, int definitionId )
			: base( manager, definitionId )
		{
			m_Teams = Definition.GetTeamDefinitions().Cast<CWTeamDefinition>().ToArray();

			// Column header

			int offset = 255;

			AddLabelCropped( 312, offset, 120, 20, LabelHue, "Healer Home" );

			// Entry info

			offset += 30;

			int maxPage = ( m_Teams.Length - 1 ) / EntriesPerPage;

			for ( int i = 0; i < m_Teams.Length; i++ )
			{
				var team = m_Teams[i];

				AddLabelCropped( 312, offset, 120, 20, LabelHue, team.HealerHome.ToString() );

				offset += 20;
			}
		}
	}
}