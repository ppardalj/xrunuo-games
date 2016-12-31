using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Engines.Games
{
	public class Registry
	{
		private static Registry m_Instance;

		public static Registry Instance
		{
			get
			{
				if ( m_Instance == null )
					m_Instance = new Registry();

				return m_Instance;
			}
		}

		private HashSet<IGame> m_ActiveGames;

		private Registry()
		{
			m_ActiveGames = new HashSet<IGame>();
		}

		public void OnGameStarted( IGame game )
		{
			m_ActiveGames.Add( game );

			// TODO: Raise an event when a game starts to notify game started to other systems.
		}

		public void OnGameFinished( IGame game )
		{
			m_ActiveGames.Remove( game );
		}

		public IEnumerable<IGame> GetActiveGames()
		{
			return m_ActiveGames;
		}
	}
}
