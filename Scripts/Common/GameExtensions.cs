using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Scripts.Commands;

namespace Server.Engines.Games
{
	public static class GameExtensions
	{
		public static void BroadcastMessage( this IGame game, string message )
		{
			message = String.Format( "Game: {0}", message );

			foreach ( var player in game.GetPlayers() )
				player.SendMessage( 0x489, message );

			CommandHandlers.BroadcastMessage( AccessLevel.Counselor, 0x489, message );
		}

		public static void BroadcastMessage( this IGame game, string message, params object[] args )
		{
			game.BroadcastMessage( String.Format( message, args ) );
		}
	}
}
