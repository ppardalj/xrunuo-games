using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Engines.Games;
using Server.Network;
using Server.Items;

namespace Server.Gumps
{
	public class GameManagerGump : Gump
	{
		public static void Initialize()
		{
			CommandSystem.Register( "GameManager", AccessLevel.Seer, new CommandEventHandler( GameManager_OnCommand ) );
		}

		private static void GameManager_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			ManagerPersistance.Ensure();

			from.SendGump( new GameManagerGump() );
		}

		private const int EntriesPerPage = 16;

		private const int FontColor = 0xFFFFFF;

		private const int LabelHue = 0x480;

		private void AddHtmlColor( int x, int y, int width, int height, string text, int color, bool background, bool scrollbar )
		{
			AddHtml( x, y, width, height, String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text ), background, scrollbar );
		}

		private Manager m_Manager;
		private KeyValuePair<int, GameDefinition>[] m_Definitions;

		public GameManagerGump()
			: this( Manager.Instance )
		{
		}

		public GameManagerGump( Manager manager )
			: base( 25, 25 )
		{
			m_Manager = manager;
			m_Definitions = manager.Definitions.ToArray();

			AddPage( 0 );

			// Gump Structure

			AddBackground( 0, 0, 520, 510, 0x13BE );
			AddImageTiled( 10, 10, 500, 30, 0xA40 );
			AddImageTiled( 10, 50, 500, 370, 0xA40 );
			AddImageTiled( 10, 430, 500, 65, 0xA40 );
			AddAlphaRegion( 10, 10, 500, 485 );

			// Title

			AddHtmlColor( 10, 14, 500, 20, "<CENTER>GAME MANAGER GUMP</CENTER>", FontColor, false, false );

			// Footer options

			AddButton( 15, 435, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0 );
			AddHtmlColor( 50, 435, 300, 20, "Add a new game definition", FontColor, false, false );

			// Column header

			int offset = 60;

			AddLabelCropped( 22, offset, 100, 20, LabelHue, "Name" );
			AddLabelCropped( 102, offset, 100, 20, LabelHue, "Type" );

			// Definitions info

			offset += 30;

			int maxPage = ( m_Definitions.Length - 1 ) / EntriesPerPage;

			int i = 0;
			foreach ( var definition in m_Definitions )
			{
				int page = i / EntriesPerPage;
				int entry = i % EntriesPerPage;

				if ( entry == 0 )
				{
					AddPage( page + 1 );

					offset = 90;

					if ( page > 0 )
						AddButton( 465, 62, 0x15E3, 0x15E7, 0, GumpButtonType.Page, page );
					else
						AddImage( 465, 62, 0x25EA );

					if ( page < maxPage )
						AddButton( 482, 62, 0x15E1, 0x15E5, 0, GumpButtonType.Page, page + 2 );
					else
						AddImage( 482, 62, 0x25E6 );
				}

				AddLabelCropped( 22, offset, 120, 20, LabelHue, definition.Value.Name );
				AddLabelCropped( 102, offset, 120, 20, LabelHue, definition.Value.GetType().Name.Replace( "GameDefinition", "" ) );
				AddButton( 470, offset - 1, 0xFA5, 0xFA7, 100 + definition.Key * 10 + 1, GumpButtonType.Reply, 0 );

				offset += 20;
				i++;
			}
		}

		public override void OnResponse( GameClient sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( info.ButtonID < 100 )
			{
				switch ( info.ButtonID )
				{
					case 1: // Add new definition
						{
							from.SendGump( new SelectGameTypeGump( m_Manager ) );
							break;
						}
				}
			}
			else
			{
				int index = info.ButtonID - 100;

				int gameId = index / 10;
				int option = index % 10;

				if ( m_Manager.Definitions.ContainsKey( gameId ) )
				{
					var definition = m_Manager.Definitions[gameId];

					switch ( option )
					{
						case 1: // Edit
							from.SendGump( definition.CreateDefinitionGump( m_Manager, gameId ) );
							break;
					}
				}
			}
		}
	}
}
