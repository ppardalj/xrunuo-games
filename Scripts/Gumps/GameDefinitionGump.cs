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
	public abstract class GameDefinitionGump : Gump
	{
		protected const int EntriesPerPage = 7;

		protected const int FontColor = 0xFFFFFF;

		protected const int LabelHue = 0x480;
		protected const int GreenHue = 0x40;
		protected const int RedHue = 0x20;

		private void AddHtmlColor( int x, int y, int width, int height, string text, int color, bool background, bool scrollbar )
		{
			AddHtml( x, y, width, height, String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text ), background, scrollbar );
		}

		public abstract string GetGumpHeader();
		
		private Manager m_Manager;
		private int m_DefinitionId;
		private GameDefinition m_Definition;
		private TeamDefinition[] m_Teams;

		protected Manager Manager { get { return m_Manager; } }
		protected GameDefinition Definition { get { return m_Definition; } }
		protected int DefinitionId { get { return m_DefinitionId; } }

		public GameDefinitionGump( Manager manager, int definitionId )
			: base( 25, 25 )
		{
			m_Manager = manager;
			m_DefinitionId = definitionId;
			m_Definition = m_Manager.Definitions[definitionId];
			m_Teams = m_Definition.GetTeamDefinitions().ToArray();

			AddPage( 0 );

			// Gump Structure

			AddBackground( 0, 0, 520, 510, 0x13BE );
			AddImageTiled( 10, 10, 500, 30, 0xA40 );
			AddImageTiled( 10, 50, 500, 190, 0xA40 );
			AddImageTiled( 10, 250, 500, 190, 0xA40 );
			AddImageTiled( 10, 450, 500, 45, 0xA40 );
			AddAlphaRegion( 10, 10, 500, 485 );

			// Title

			AddHtmlColor( 10, 14, 500, 20, String.Format("<CENTER>{0}</CENTER>", GetGumpHeader()), FontColor, false, false );

			// Game Info

			AddLabel( 22, 60, 1259, "Game Name" );
			AddLabel( 106, 60, 1271, m_Definition.Name );
			AddButton( 276, 60, 0x15E1, 0x15E5, 2001, GumpButtonType.Reply, 0 );

			AddLabel( 22, 85, 1259, "Map" );
			AddLabel( 106, 85, 1271, m_Definition.Map.ToString() );
			AddButton( 276, 85, 0x15E1, 0x15E5, 2002, GumpButtonType.Reply, 0 );

			AddLabel( 22, 110, 1259, "Area" );
			AddLabel( 106, 110, 1271, m_Definition.Area.ToString() );
			AddButton( 276, 110, 0x15E1, 0x15E5, 2003, GumpButtonType.Reply, 0 );

			AddLabel( 22, 135, 1259, "Duration" );
			AddLabel( 106, 135, 1271, m_Definition.Duration.ToString() );
			AddButton( 276, 135, 0x15E1, 0x15E5, 2004, GumpButtonType.Reply, 0 );

			// Footer options

			AddButton( 15, 453, 0xFA5, 0xFA7, 1001, GumpButtonType.Reply, 0 );
			AddHtmlColor( 50, 453, 300, 20, "Add a new team definition", FontColor, false, false );

			AddButton( 15, 473, 0xFA5, 0xFA7, 1002, GumpButtonType.Reply, 0 );
			AddHtmlColor( 50, 473, 300, 20, "Create a player join stone", FontColor, false, false );

			AddButton( 285, 453, 0xFB7, 0xFB9, 1003, GumpButtonType.Reply, 0 );
			AddHtmlColor( 320, 453, 300, 20, "Save changes", FontColor, false, false );

			AddButton( 285, 473, 0xFB1, 0xFB3, 1004, GumpButtonType.Reply, 0 );
			AddHtmlColor( 320, 473, 300, 20, "Remove this game", FontColor, false, false );

			// Column header

			int offset = 255;

			AddLabelCropped( 22, offset, 100, 20, LabelHue, "Team" );
			AddLabelCropped( 122, offset, 120, 20, LabelHue, "Hue" );
			AddLabelCropped( 182, offset, 120, 20, LabelHue, "Home" );
			
			// Entry info

			offset += 30;

			int maxPage = ( m_Teams.Length - 1 ) / EntriesPerPage;

			for ( int i = 0; i < m_Teams.Length; i++ )
			{
				var team = m_Teams[i];

				int page = i / EntriesPerPage;
				int entry = i % EntriesPerPage;

				if ( entry == 0 )
				{
					AddPage( page + 1 );

					offset = 285;

					if ( page > 0 )
						AddButton( 465, 252, 0x15E3, 0x15E7, 0, GumpButtonType.Page, page );
					else
						AddImage( 465, 252, 0x25EA );

					if ( page < maxPage )
						AddButton( 482, 252, 0x15E1, 0x15E5, 0, GumpButtonType.Page, page + 2 );
					else
						AddImage( 482, 252, 0x25E6 );
				}

				AddLabelCropped( 22, offset, 120, 20, team.Hue - 1, team.Name );
				AddLabelCropped( 122, offset, 120, 20, LabelHue, team.Hue.ToString() );
				AddLabelCropped( 182, offset, 120, 20, LabelHue, team.Home.ToString() );
				
				AddButton( 435, offset - 1, 0xFA5, 0xFA7, 10 * i + 1, GumpButtonType.Reply, 0 );
				AddButton( 470, offset - 1, 0xFB1, 0xFB3, 10 * i + 2, GumpButtonType.Reply, 0 );

				offset += 20;
			}
		}

		public override void OnResponse( GameClient sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( info.ButtonID > 1000 )
			{
				switch ( info.ButtonID )
				{
					case 1001: // Add a new team definition
						{
							var team = m_Definition.CreateTeamDefinition();
							team.Name = "unnamed";
							team.Hue = 0x481;

							m_Definition.AddTeamDefinition( team );
					
							from.SendMessage( "You added a new team definition. Now, press the edit button to customize it!" );
							from.SendGump( m_Definition.CreateDefinitionGump( m_Manager, m_DefinitionId ) );

							break;
						}
					case 1002: // Place a join stone
						{
							from.SendMessage( "Select a location to place the join stone." );
							from.BeginTarget( -1, true, TargetFlags.None, new TargetCallback(
								( m, targeted ) =>
								{
									IPoint3D loc = null;
									Map map = null;

									if ( targeted is LandTarget )
									{
										loc = ( (LandTarget) targeted ).Location;
										map = from.Map;
									}
									else if ( targeted is StaticTarget )
									{
										loc = ( (StaticTarget) targeted ).Location;
										map = from.Map;
									}

									if ( loc != null && map != null )
									{
										var joinStone = new JoinStone( m_DefinitionId );
										// TODO: track a join stone was created?
										joinStone.MoveToWorld( new Point3D( loc ), map );

										m.SendMessage( "You place the join stone." );
									}
									else
									{
										m.SendMessage( "A join stone cannot be placed there." );
									}

									m.SendGump( m_Definition.CreateDefinitionGump( m_Manager, m_DefinitionId ) );
								} ) );

							break;
						}
					case 1003: // Save changes
						{
							from.SendMessage( 0x64, "* Cambios Guardados Correctamente *" );
							from.SendGump( new GameManagerGump() );

							break;
						}
					case 1004: // Remove definition
						{
							SendWarningGump( sender.Mobile,
								( m, okay, state ) =>
								{
									if ( okay )
									{
										m_Manager.RemoveDefinition( m_DefinitionId );

										m.SendGump( new GameManagerGump() );
										m.SendMessage( "You removed the game definition." );
									}
									else
									{
										m.SendGump( m_Definition.CreateDefinitionGump( m_Manager, m_DefinitionId ) );
										m.SendMessage( "You choose not to remove this game definition." );
									}
								} );

							break;
						}
					case 2001: // Change game name
						{
							var property = m_Definition.GetType().GetProperty( "Name" );
							var stack = new Stack();
							stack.Push( new GamePropertyStackEntry( m_Manager, m_DefinitionId, property ) );
							from.SendGump( new SetGump( property, from, m_Definition, stack, 0, new ArrayList() ) );

							break;
						}
					case 2002: // Change game map
						{
							var property = m_Definition.GetType().GetProperty( "Map" );
							var stack = new Stack();
							stack.Push( new GamePropertyStackEntry( m_Manager, m_DefinitionId, property ) );
							from.SendGump( new SetListOptionGump( property, from, m_Definition, stack, 0, new ArrayList(), Map.GetMapNames(), Map.GetMapValues() ) );

							break;
						}
					case 2003: // Change game area
						{
							var property = m_Definition.GetType().GetProperty( "Area" );
							from.SendGump( new PropertiesGump( from, m_Definition.Area, new Stack(), new GamePropertyStackEntry( m_Manager, m_DefinitionId, property ) ) );

							break;
						}
					case 2004: // Change game duration
						{
							var property = m_Definition.GetType().GetProperty( "Duration" );
							var stack = new Stack();
							stack.Push( new GamePropertyStackEntry( m_Manager, m_DefinitionId ) );
							from.SendGump( new SetTimeSpanGump( property, from, m_Definition, stack, 0, new ArrayList() ) );

							break;
						}
				}
			}
			else
			{
				int teamId = info.ButtonID / 10;
				int option = info.ButtonID % 10;

				if ( teamId >= 0 && teamId < m_Teams.Length )
				{
					var team = m_Teams[teamId];

					switch ( option )
					{
						case 1: // Edit
							{
								from.SendGump( new PropertiesGump( from, team, new Stack(), new GamePropertyStackEntry( m_Manager, m_DefinitionId ) ) );
								break;
							}

						case 2: // Remove
							{
								SendWarningGump( sender.Mobile,
									( m, okay, state ) =>
									{
										if ( okay )
										{
											m_Definition.RemoveTeamDefinition( team );
											m.SendMessage( "You removed the team definition." );
										}
										else
										{
											m.SendMessage( "You choose not to remove this team definition." );
										}

										m.SendGump( m_Definition.CreateDefinitionGump( m_Manager, m_DefinitionId ) );
									} );
								break;
							}
					}
				}
			}
		}

		private void SendWarningGump( Mobile from, WarningGumpCallback callback )
		{
			from.SendGump( new WarningGump(
				"Are you sure?",
				30720,
				"If you delete this definition you will lose all the changes. Are you sure you wish to continue?",
				0xFFC000,
				280,
				200,
				callback,
				null
			) );
		}

		public class GamePropertyStackEntry : PropertiesGump.IStackEntry
		{
			private Manager m_Manager;
			private int m_DefinitionId;
			private PropertyInfo m_Property;

			private GameDefinition m_Definition;

			public GamePropertyStackEntry( Manager manager, int definitionId, PropertyInfo prop = null )
			{
				m_Manager = manager;
				m_DefinitionId = definitionId;
				m_Property = prop;

				m_Definition = manager.Definitions[definitionId];

			}
			public void SendGump( Mobile from, Stack stack )
			{
				from.SendGump( m_Definition.CreateDefinitionGump( m_Manager, m_DefinitionId ) );
			}

			public void OnValueChanged( object obj )
			{
				if ( m_Property != null && m_Property.CanWrite )
					m_Property.SetValue( m_Definition, obj, null );
			}
		}
	}
}