using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Games
{
	public interface IGame
	{
		/// <summary>
		/// Returns a listener attached to the game of the given type, or null if any.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetListener<T>() where T : GameListener;

		/// <summary>
		/// Returns all players participating in the game.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Mobile> GetPlayers();
		
		/// <summary>
		/// Gets whether the given mobile is playing in this game or not.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		bool IsPlaying( Mobile m );

		/// <summary>
		/// The map where the game is being played.
		/// </summary>
		Map Map { get; }

		/// <summary>
		/// The area where the game is being played.
		/// </summary>
		Rectangle2D Area { get; }

		/// <summary>
		/// Starts this game.
		/// </summary>
		/// <param name="embryo"></param>
		void StartGame( Embryo embryo );

		/// <summary>
		/// Returns whether a mobile is allowed to perform beneficial acts to another in the game region.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		bool AllowBeneficial( Mobile from, Mobile target );

		/// <summary>
		/// Returns whether a mobile is allowed to perform harmful acts to another in the game region.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		bool AllowHarmful( Mobile from, Mobile target );

		/// <summary>
		/// Forces the given movile to leave this game.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="legal"></param>
		void LeaveGame( Mobile m, LeaveMode leavemode = LeaveMode.Legal );

		/// <summary>
		/// Invoked when a mobile got healed.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="healer"></param>
		/// <param name="amount"></param>
		void OnHeal( Mobile m, Mobile healer, int amount );

		/// <summary>
		/// Invoked when a mobile got resurrected.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="healer"></param>
		void OnResurrect( Mobile m, Mobile healer );

		/// <summary>
		/// Returns the given player to their home.
		/// </summary>
		/// <param name="m"></param>
		void ReturnToHome( Mobile m );
	}
}
