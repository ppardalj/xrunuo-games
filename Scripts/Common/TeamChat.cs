using System;

using Server.Commands;

namespace Server.Engines.Games
{
	public static class TeamChat
	{
		public static void Initialize()
		{
			CommandSystem.Register( new string[] { "eq", "equipo", "t", "team" }, AccessLevel.Player, new CommandEventHandler( TeamChat_Command ) );
		}

		[Usage( "t <message>" )]
		[Description( "Game team chat." )]
		[Aliases( "team, eq, equipo" )]
		private static void TeamChat_Command( CommandEventArgs e )
		{
			string message = e.ArgString;

			if ( !String.IsNullOrWhiteSpace( message ) )
			{
				message = message.Trim();

				Team team = GameHelper.FindTeamFor( e.Mobile );
				if ( team != null )
				{
					message = String.Format( "<{0} Team> {1}: {2}", team.Name, e.Mobile.Name, message );

					foreach ( Mobile m in team.GetMembers() )
						m.SendMessage( 64, message );
				}
			}
		}
	}
}
