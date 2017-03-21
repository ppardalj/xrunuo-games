using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Games
{
	public struct AddressStruct
	{
		public IPAddress LocalIP;
		public IPAddress PublicIP;

		public AddressStruct( IPAddress publicIP, IPAddress localIP )
		{
			LocalIP = localIP;
			PublicIP = publicIP;
		}
	}

	public static class GameHelper
	{
		#region Anti multiclient
		private static List<AddressStruct> m_AddressesInEvent = new List<AddressStruct>();

		public static void ClearAddressList()
		{
			m_AddressesInEvent.Clear();
		}

		public static void AddToAddressList( Mobile m )
		{
			m_AddressesInEvent.Add( new AddressStruct( m.NetState.Address, m.NetState.ClientAddress ) );
		}

		public static bool IsUsingMulticlient( Mobile m )
		{
			NetState state = m.NetState;

			if ( state == null )
				return false;

			foreach ( AddressStruct address in m_AddressesInEvent )
			{
				if ( address.LocalIP.Equals( state.ClientAddress ) && address.PublicIP.Equals( state.Address ) )
					return true;
			}

			return false;
		}
		#endregion

		#region Simultaneous games
		private static bool m_IsGameRunning;

		public static bool IsGameRunning
		{
			get { return m_IsGameRunning; }
			set { m_IsGameRunning = value; }
		}
		#endregion

		#region Static getters
		public static Team FindTeamFor( Mobile m )
		{
			if ( m is CWHealer )
				return ( (CWHealer) m ).Team;
			
			foreach ( var game in Registry.Instance.GetActiveGames().OfType<ITeamGame>() )
			{
				var team = game.GetTeamFor( m );

				if ( team != null )
					return team;
			}

			return null;
		}

		public static IGame FindGameFor( Mobile m )
		{
			return Registry.Instance.GetActiveGames().FirstOrDefault( game => game.IsPlaying( m ) );
		}
		#endregion
	}
}
