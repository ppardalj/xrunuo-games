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
	public class CTFGameDefinitionGump : GameDefinitionGump
	{
		public override string GetGumpHeader()
		{
			return "CTF GAME DEFINITION GUMP";
		}

		private CTFGameDefinition m_Definition;
		private CTFTeamDefinition[] m_Teams;

		public CTFGameDefinitionGump( Manager manager, int definitionId )
			: base( manager, definitionId )
		{
			m_Definition = (CTFGameDefinition) Definition;
			m_Teams = Definition.GetTeamDefinitions().Cast<CTFTeamDefinition>().ToArray();

			// Game Info

			AddLabel( 22, 160, 1259, "Max score" );
			AddLabel( 106, 160, 1271, m_Definition.MaxScore.ToString() );
			AddButton( 276, 160, 0x15E1, 0x15E5, 2005, GumpButtonType.Reply, 0 );

			// Column header

			int offset = 255;

			AddLabelCropped( 312, offset, 120, 20, LabelHue, "Flag Home" );

			// Entry info

			offset += 30;

			int maxPage = ( m_Teams.Length - 1 ) / EntriesPerPage;

			for ( int i = 0; i < m_Teams.Length; i++ )
			{
				var team = m_Teams[i];

				AddLabelCropped( 312, offset, 120, 20, LabelHue, team.FlagHome.ToString() );

				offset += 20;
			}
		}

		public override void OnResponse( GameClient sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			switch ( info.ButtonID )
			{
				case 2005:
					{
						var property = m_Definition.GetType().GetProperty( "MaxScore" );
						var stack = new Stack();
						stack.Push( new GamePropertyStackEntry( Manager, DefinitionId ) );
						from.SendGump( new SetGump( property, from, m_Definition, stack, 0, new ArrayList() ) );

						break;
					}
				default:
					{
						base.OnResponse( sender, info );
						break;
					}
			}
		}
	}
}