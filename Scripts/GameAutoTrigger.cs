using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Commands;
using Server.Items;
using Server.Targeting;

namespace Server.Engines.Games
{
	public class GameAutoTrigger
	{
		// Are automatic daily games enabled?
		public static readonly bool Enabled = true;

		// Configure there the starting times for automatic games.
		public static TimeSpan[] GameStartTimes = new TimeSpan[]
			{
				TimeSpan.FromHours( 20.0 ),
				TimeSpan.FromHours( 23.0 )
			};

		public static void Initialize()
		{
			CommandSystem.Register( "GameOpen", AccessLevel.Seer, new CommandEventHandler( GameOpen_OnCommand ) );
			CommandSystem.Register( "GameOpenRandom", AccessLevel.Seer, new CommandEventHandler( GameOpenRandom_OnCommand ) );

			if ( Enabled )
				CreateAutoTriggers();
		}

		private static void GameOpenRandom_OnCommand( CommandEventArgs e )
		{
			if ( GameHelper.IsGameRunning )
			{
				e.Mobile.SendMessage( "There is already another running game, aborted." );
				return;
			}

			bool success = TriggerRandomGame();
			if ( success )
				e.Mobile.SendMessage( "You have activated a random game." );
			else
				e.Mobile.SendMessage( "Couldn't activate a random game." );
		}

		public static void GameOpen_OnCommand( CommandEventArgs e )
		{
			e.Mobile.SendMessage( "Target a game join stone to open it." );
			e.Mobile.BeginTarget( -1, false, TargetFlags.None,
				( from, targeted ) =>
				{
					if ( GameHelper.IsGameRunning )
					{
						from.SendMessage( "There is already another running game, aborted." );
						return;
					}

					JoinStone stone = targeted as JoinStone;
					if ( stone != null )
					{
						if ( stone.IsOpened )
						{
							from.SendMessage( "That stone is already opened." );
						}
						else
						{
							from.SendMessage( "You triggered the selected game." );
							TriggerGame( stone );
						}
					}
					else
					{
						from.SendMessage( "That is not a join stone!" );
					}
				} );
		}

		private static void CreateAutoTriggers()
		{
			foreach ( var gameStartTime in GameStartTimes )
			{
				var triggerTime = DateTime.Now.Date + gameStartTime - GameSignupTimer.GetSignupPeriodDuration();
				var triggerDelay = triggerTime - DateTime.Now;
				if ( triggerDelay.Ticks >= 0 )
				{
					Timer.DelayCall( triggerDelay, () => { TriggerRandomGame(); } );
					Console.WriteLine( "GameAutoTimer: Scheduled random game for {0}, sign-up will be triggered in {1}", gameStartTime, triggerDelay );
				}
			}
		}

		private static bool TriggerRandomGame()
		{
			var joinStone = PickRandomStone();
			return TriggerGame( joinStone );
		}

		private static bool TriggerGame( JoinStone joinStone )
		{
			if ( joinStone == null )
				return false;

			new GameSignupTimer( joinStone ).Start();
			return true;
		}

		private static JoinStone PickRandomStone()
		{
			JoinStone[] stones = World.Instance.Items.OfType<JoinStone>().Where( s => s.Active ).ToArray();

			if ( stones.Length == 0 )
				return null;
			else
				return stones[Utility.Random( stones.Length )];
		}
	}

	public class GameSignupTimer : Timer
	{
		public static readonly TimeSpan SignupMessageInterval = TimeSpan.FromMinutes( 1.0 );
		public static readonly int TotalSignupMessageCount = 10;

		public static TimeSpan GetSignupPeriodDuration()
		{
			return TimeSpan.FromSeconds( SignupMessageInterval.TotalSeconds * TotalSignupMessageCount );
		}

		private JoinStone m_JoinStone;
		private int m_Countdown;
		private List<Item> m_Moongates;

		public GameSignupTimer( JoinStone joinStone )
			: base( TimeSpan.Zero, SignupMessageInterval )
		{
			if ( joinStone == null )
				throw new ArgumentNullException( "joinStone" );

			m_JoinStone = joinStone;
			m_Moongates = new List<Item>();
			m_Countdown = TotalSignupMessageCount;

			OpenGame();
		}

		private void OpenGame()
		{
			if ( GameHelper.IsGameRunning )
			{
				Console.WriteLine( "GameAuto Error: Ya hay otro juego activo en este momento, desactivando timer..." );
			}
			else
			{
				GameHelper.IsGameRunning = true;

				m_JoinStone.Open();

				CreateMoongates();
			}
		}

		protected override void OnTick()
		{
			if ( m_Countdown > 0 )
				BroadcastSignupMessage();
			else
				StartGame();
		}

		private void BroadcastSignupMessage()
		{
			string message = String.Format( "{0} will start within {1} minutes. Gates opened.", m_JoinStone.GameName, m_Countdown );
			World.Broadcast( 1161, true, message );

			m_Countdown--;
		}

		private void StartGame()
		{
			m_JoinStone.StartGame();

			RemoveMoongates();

			string message = String.Format( "{0} started!. Gates closed.", m_JoinStone.GameName );
			World.Broadcast( 1161, true, message );

			Stop();
		}

		private class MoongateEntry
		{
			public Point3D Location { get; private set; }
			public Map Map { get; private set; }

			public MoongateEntry( Point3D location, Map map )
			{
				Location = location;
				Map = map;
			}
		}

		private static readonly MoongateEntry[] m_MoongateEntries = new MoongateEntry[]
			{
				new MoongateEntry( new Point3D( 1020, 526, -70 ), Map.Malas ) // Luna
			};

		private void CreateMoongates()
		{
			RemoveMoongates(); // sanity

			foreach ( MoongateEntry entry in m_MoongateEntries )
			{
				var moongate = new GateRestrict();
				moongate.Location = entry.Location;
				moongate.Map = entry.Map;
				moongate.Dispellable = false;
				moongate.Target = new Point3D( 5146, 1775, 0 );
				moongate.TargetMap = Map.Trammel;
				moongate.Name = "Game Entrance";
				m_Moongates.Add( moongate );
			}
		}

		private void RemoveMoongates()
		{
			foreach ( var moongate in m_Moongates )
				moongate.Delete();

			m_Moongates.Clear();
		}
	}
}
