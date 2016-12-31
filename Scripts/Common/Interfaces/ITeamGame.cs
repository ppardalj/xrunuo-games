using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Games
{
	public interface ITeamGame : IGame
	{
		/// <summary>
		/// Gets the team for the given mobile in this game, or null if any.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		Team GetTeamFor( Mobile m );
	}
}
