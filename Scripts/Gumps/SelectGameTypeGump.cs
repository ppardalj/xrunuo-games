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
	public class SelectGameTypeGump : Gump
	{
		private const int FontColor = 0xFFFFFF;

		private const int LabelHue = 0x480;
		private const int GreenHue = 0x40;
		private const int RedHue = 0x20;

		private void AddHtmlColor( int x, int y, int width, int height, string text, int color, bool background, bool scrollbar )
		{
			AddHtml( x, y, width, height, String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text ), background, scrollbar );
		}

		private Manager m_Manager;

		private static Type[] m_Types = new Type[] {
			typeof( CWGameDefinition ),
			typeof( CTFGameDefinition ),
			typeof( TotalWarGameDefinition ),
			typeof( SurvivalGameDefinition )
		};

		public SelectGameTypeGump( Manager manager )
			: base( 25, 25 )
		{
			m_Manager = manager;

			AddPage( 0 );

			// Gump Structure

			AddBackground( 0, 0, 270, 510, 0x13BE );
			AddImageTiled( 10, 10, 250, 30, 0xA40 );
			AddImageTiled( 10, 50, 250, 370, 0xA40 );
			AddImageTiled( 10, 430, 250, 65, 0xA40 );
			AddAlphaRegion( 10, 10, 250, 485 );

			// Title

			AddHtmlColor( 10, 14, 250, 20, "Please select a game type:", FontColor, false, false );

			// Column header

			int offset = 60;

			for ( int i = 0; i < m_Types.Length; i++ )
			{
				AddLabelCropped( 22, offset, 120, 20, LabelHue, m_Types[i].Name.Replace( "GameDefinition", "" ) );
				AddButton( 220, offset - 1, 0xFA5, 0xFA7, i + 1, GumpButtonType.Reply, 0 );

				offset += 20;
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			int index = info.ButtonID - 1;

			if ( index >= 0 && index < m_Types.Length )
			{
				Type type = m_Types[index];

				GameDefinition def = (GameDefinition) Activator.CreateInstance( type );
				def.Name = "unnamed";

				int gameId = m_Manager.AddDefinition( def );

				from.SendMessage( "You added a new game definition." );
				from.SendGump( def.CreateDefinitionGump( m_Manager, gameId ) );
			}
		}
	}
}
