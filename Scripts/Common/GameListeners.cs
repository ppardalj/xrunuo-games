using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Commands;
using Server.Items;
using Server.Events;

namespace Server.Engines.Games
{
	public abstract class GameListener
	{
		private IGame m_Game;

		protected IGame Game { get { return m_Game; } }

		protected GameListener( IGame game )
		{
			m_Game = game;
		}

		/// <summary>
		/// Event invoked when the game starts.
		/// </summary>
		public virtual void OnStarted()
		{
		}

		/// <summary>
		/// Event invoked when the game ends.
		/// </summary>
		public virtual void OnFinished()
		{
		}

		/// <summary>
		/// Event invoked while the game is running at regular intervals.
		/// </summary>
		public virtual void OnSlice()
		{
		}

		/// <summary>
		/// Event invoked when a mobile joins the game in the given team.
		/// </summary>
		public virtual void OnJoin( Mobile m )
		{
		}

		/// <summary>
		/// Event invoked when a mobile leaves the game.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="leaveMode"></param>
		public virtual void OnLeave( Mobile m, LeaveMode leaveMode )
		{
		}

		/// <summary>
		/// Event invoked when the team of the given mobile wins the game.
		/// </summary>
		/// <param name="m"></param>
		public virtual void OnWin( Mobile m )
		{
		}

		/// <summary>
		/// Event invoked when the team of the given mobile loses the game.
		/// </summary>
		/// <param name="m"></param>
		public virtual void OnLose( Mobile m )
		{
		}

		/// <summary>
		/// Event invoked when <paramref name="from"/> is killed by
		/// <paramref name="killer"/>, meaning that <paramref name="killer"/>
		/// is the most recent damager and the most total damager.
		/// </summary>
		public virtual void OnKill( Mobile from, Mobile killer )
		{
		}

		/// <summary>
		/// Event invoked when <paramref name="from"/> death has been
		/// assisted by <paramref name="assistant"/>, meaning that
		/// they did at least a 25% of the target max hits as damage.
		/// </summary>
		public virtual void OnAssist( Mobile from, Mobile assistant )
		{
		}

		/// <summary>
		/// Event invoked when mob <paramref name="healer"/> heals
		/// mob <paramref name="m"/> in <paramref name="amount"/> points.
		/// </summary>
		public virtual void OnHeal( Mobile m, Mobile healer, int amount )
		{
		}

		/// <summary>
		/// Event invoked when mob <paramref name="healer"/> resurrects
		/// mob <paramref name="m"/>.
		/// </summary>
		public virtual void OnResurrect( Mobile m, Mobile healer )
		{
		}
	}

	public class LogoutKicker : GameListener
	{
		public static readonly TimeSpan LogoutLimitTime = TimeSpan.FromMinutes( 1.0 );

		private readonly Dictionary<Mobile, DateTime> m_Table;

		public LogoutKicker( IGame game )
			: base( game )
		{
			m_Table = new Dictionary<Mobile, DateTime>();
		}

		public override void OnStarted()
		{
			EventSink.Login += OnLogin;
			EventSink.Disconnected += OnDisconnected;
		}

		public override void OnFinished()
		{
			EventSink.Login -= OnLogin;
			EventSink.Disconnected -= OnDisconnected;
		}

		private void OnLogin( LoginEventArgs e )
		{
			var from = e.Mobile;

			if ( m_Table.ContainsKey( from ) )
				m_Table.Remove( from );
		}

		private void OnDisconnected( DisconnectedEventArgs e )
		{
			var from = e.Mobile;

			if ( Game.IsPlaying( from ) )
			{
				m_Table[from] = DateTime.Now;
			}
		}

		public override void OnSlice()
		{
			foreach ( Mobile m in m_Table.Keys.ToArray() )
			{
				TimeSpan elapsedSinceLogout = DateTime.Now - m_Table[m];

				if ( elapsedSinceLogout > LogoutLimitTime )
				{
					if ( m.Client == null )
						Game.LeaveGame( m );
				}
			}
		}
	}

	public class ReequipOnDeathListener : GameListener
	{
		private double m_StatsRestoredPercent;

		public ReequipOnDeathListener( IGame game, double statsRestoredPercent = 0.5 )
			: base( game )
		{
			m_StatsRestoredPercent = statsRestoredPercent;
		}

		public override void OnSlice()
		{
			foreach ( var player in Game.GetPlayers().Where( player => !player.Alive ) )
			{
				DeleteCorpse( player );
				ReturnToHome( player );
				RestoreHealth( player );
				Unequip( player );
				Reequip( player );
			}
		}

		private void DeleteCorpse( Mobile m )
		{
			var corpse = m.Corpse;

			if ( corpse != null && !corpse.Deleted )
				corpse.Delete();
		}

		private void ReturnToHome( Mobile m )
		{
			Game.ReturnToHome( m );
		}

		private void RestoreHealth( Mobile m )
		{
			m.Resurrect();

			m.Hits = Math.Max( 10, (int) ( m.HitsMax * m_StatsRestoredPercent ) );
			m.Stam = Math.Max( 10, (int) ( m.StamMax * m_StatsRestoredPercent ) );
			m.Mana = Math.Max( 10, (int) ( m.ManaMax * m_StatsRestoredPercent ) );
		}

		private void Unequip( Mobile m )
		{
			foreach ( var item in m.GetInventoryItems().ToArray() )
			{
				item.Delete();
			}
		}

		private void Reequip( Mobile m )
		{
			GameEquipStone.GiveEquipTo( m );
		}
	}

	public class DebugListener : GameListener
	{
		public DebugListener( IGame game )
			: base( game )
		{
		}

		public override void OnStarted()
		{
			Game.BroadcastMessage( "Debug: OnStarted()" );
		}

		public override void OnFinished()
		{
			Game.BroadcastMessage( "Debug: OnFinished()" );
		}

		public override void OnSlice()
		{
			// Do not spam with OnSlice() debug.
			// Game.BroadcastMessage( "Debug: OnSlice()" );
		}

		public override void OnJoin( Mobile from )
		{
			Game.BroadcastMessage( "Debug: OnJoin(from={0})", from );
		}

		public override void OnLeave( Mobile from, LeaveMode leaveMode )
		{
			Game.BroadcastMessage( "Debug: OnLeave(from={0}, leavemode={1})", from, leaveMode );
		}

		public override void OnWin( Mobile from )
		{
			Game.BroadcastMessage( "Debug: OnWin(from={0})", from );
		}

		public override void OnLose( Mobile from )
		{
			Game.BroadcastMessage( "Debug: OnLose(from={0})", from );
		}

		public override void OnKill( Mobile from, Mobile killer )
		{
			Game.BroadcastMessage( "Debug: OnKill(from={0}, killer={1})", from, killer );
		}

		public override void OnAssist( Mobile from, Mobile assistant )
		{
			Game.BroadcastMessage( "Debug: OnAssist(from={0}, assistant={1})", from, assistant );
		}

		public override void OnHeal( Mobile from, Mobile healer, int amount )
		{
			Game.BroadcastMessage( "Debug: OnHeal(from={0}, healer={1}, amount={2})", from, healer, amount );
		}

		public override void OnResurrect( Mobile from, Mobile healer )
		{
			Game.BroadcastMessage( "Debug: OnResurrect(from={0}, healer={1})", from, healer );
		}
	}

	public class DetectSkillListener : GameListener
	{
		public static void Initialize()
		{
			CommandSystem.Register( "gamestats", AccessLevel.Player, new CommandEventHandler( GameStats_OnCommand ) );
		}

		private static void GameStats_OnCommand( CommandEventArgs e )
		{
			var from = e.Mobile;

			var game = GameHelper.FindGameFor( from );
			if ( game != null )
			{
				var listener = game.GetListener<DetectSkillListener>();
				if ( listener != null )
				{
					var context = listener.GetContext( from );
					context.SendInfo();
				}
				else
				{
					from.SendMessage( "This game has no stats to show." );
				}
			}
			else
			{
				from.SendMessage( "You are not playing." );
			}
		}

		private Dictionary<Mobile, DetectSkillContext> m_Table;

		private DetectSkillContext GetContext( Mobile m )
		{
			DetectSkillContext context;

			if ( m_Table.ContainsKey( m ) )
				context = m_Table[m];
			else
				context = m_Table[m] = new DetectSkillContext( m );

			return context;
		}

		public DetectSkillListener( IGame game )
			: base( game )
		{
			m_Table = new Dictionary<Mobile, DetectSkillContext>();
		}

		public override void OnKill( Mobile from, Mobile killer )
		{
			DetectSkillContext context = GetContext( killer );
			context.Kills++;
		}

		public override void OnAssist( Mobile from, Mobile assistant )
		{
			DetectSkillContext context = GetContext( assistant );
			context.Assists++;
		}

		public override void OnHeal( Mobile from, Mobile healer, int amount )
		{
			if ( healer != null && from != healer )
			{
				double oldHitsPercentage = (double) from.Hits / from.HitsMax;
				double newHitsPercentage = (double) ( from.Hits + amount ) / from.HitsMax;

				if ( oldHitsPercentage < 0.2 && newHitsPercentage >= 0.2 )
				{
					DetectSkillContext context = GetContext( healer );
					context.Teamplay++;
				}
			}
		}

		public override void OnResurrect( Mobile from, Mobile healer )
		{
			if ( healer != null && from != healer )
			{
				DetectSkillContext context = GetContext( healer );
				context.Teamplay++;
			}
		}

		public override void OnLose( Mobile from )
		{
			if ( from.Client != null )
			{
				DetectSkillContext context = GetContext( from );

				int fragments = ( context.Kills * 10 + context.Assists * 5 + context.Teamplay * 5 ) / 2;
				if ( fragments > 0 )
				{
					from.BankBox.DropItem( new TournamentTicketFragment( fragments ) );
					from.SendMessage( 64, "You receive {0} Tournament Ticket Fragments for scoring {1} kills, {2} assists & {3} teamplay points!", fragments, context.Kills, context.Assists, context.Teamplay );
				}
			}
		}

		public override void OnWin( Mobile from )
		{
			if ( from.Client != null )
			{
				DetectSkillContext context = GetContext( from );

				if ( context.SkillScore > 0 )
					from.BankBox.DropItem( new BankCheck( Math.Min( context.SkillScore, 5 ) * 10000 ) );

				int fragments = ( context.Kills * 10 + context.Assists * 5 + context.Teamplay * 5 ) / 4;
				if ( fragments > 0 )
				{
					from.BankBox.DropItem( new TournamentTicketFragment( fragments ) );
					from.SendMessage( 64, "You receive {0} Tournament Ticket Fragments for scoring {1} kills, {2} assists & {3} teamplay points!", fragments, context.Kills, context.Assists, context.Teamplay );
				}
			}
		}

		private class DetectSkillContext
		{
			public Mobile Owner { get; private set; }

			public int Kills { get; set; }
			public int Assists { get; set; }
			public int Teamplay { get; set; }

			public DetectSkillContext( Mobile owner )
			{
				Owner = owner;
			}

			public int SkillScore
			{
				get { return Kills + Assists + Teamplay; }
			}

			public void SendInfo()
			{
				Owner.SendMessage( "Kills: {0}", Kills );
				Owner.SendMessage( "Assist: {0}", Assists );
				Owner.SendMessage( "Teamplay: {0}", Teamplay );
			}
		}
	}

	public class KillReportListener : GameListener
	{
		public static readonly TimeSpan KillBurstDuration = TimeSpan.FromSeconds( 30.0 );
		public static readonly int MinBurstLengthForPrice = 5;

		private Dictionary<Mobile, KillRecordCollection> m_Table;

		private KillRecordCollection GetCollection( Mobile m )
		{
			KillRecordCollection context;

			if ( m_Table.ContainsKey( m ) )
				context = m_Table[m];
			else
				context = m_Table[m] = new KillRecordCollection( m );

			return context;
		}

		public KillReportListener( IGame game )
			: base( game )
		{
			m_Table = new Dictionary<Mobile, KillRecordCollection>();
		}

		public override void OnKill( Mobile from, Mobile killer )
		{
			KillRecordCollection collection = GetCollection( killer );
			collection.AddRecord( new KillRecord( killer, from ) );

			string cardinal = null;
			switch ( collection.Count() )
			{
				case 2:
					cardinal = "double";
					break;
				case 3:
					cardinal = "triple";
					break;
				case 4:
					cardinal = "quadra";
					break;
				case 5:
					cardinal = "penta";
					break;
			}

			if ( cardinal != null )
				Game.BroadcastMessage( "{0} has scored a {1} kill!", killer.Name, cardinal );
		}

		public override void OnLeave( Mobile m, LeaveMode leaveMode )
		{
			base.OnLeave( m, leaveMode );

			if ( leaveMode != LeaveMode.Illegal )
			{
				var collection = GetCollection( m );
				if ( collection.MaxBurst >= MinBurstLengthForPrice )
				{
					if ( m.BankBox != null )
					{
						m.SendMessage( 64, "Due to your extra achievements, you have been awarded an extra prize!" );
						m.BankBox.DropItem( new TournamentTicket() );
					}
				}
			}
		}

		private class KillRecordCollection
		{
			public Mobile Owner { get; private set; }
			public List<KillRecord> Records { get; private set; }
			public int MaxBurst { get; private set; }

			public KillRecordCollection( Mobile owner )
			{
				Owner = owner;
				Records = new List<KillRecord>();
			}

			public void AddRecord( KillRecord record )
			{
				Records.Add( record );

				Records.RemoveAll( r => r.IsExpired );

				int count = Count();

				if ( MaxBurst < count )
					MaxBurst = count;
			}

			public int Count()
			{
				return Records.Where( r => !r.IsExpired ).Select( r => r.Victim ).Distinct().Count();
			}
		}

		private class KillRecord
		{
			public Mobile Owner { get; private set; }
			public Mobile Victim { get; private set; }
			public DateTime End { get; private set; }

			public KillRecord( Mobile owner, Mobile victim )
			{
				Owner = owner;
				Victim = victim;
				End = DateTime.Now + KillBurstDuration;
			}

			public bool IsExpired
			{
				get { return DateTime.Now > End; }
			}
		}
	}

	public class GiveHorsesListener : GameListener
	{
		private bool m_ReturnHorseOnResurrect;

		public GiveHorsesListener( IGame game, bool returnHorseOnResurrect = false )
			: base( game )
		{
			m_ReturnHorseOnResurrect = returnHorseOnResurrect;
		}

		public override void OnJoin( Mobile from )
		{
			if ( from.Race != Race.Gargoyle )
			{
				var horse = new GameHorse( from, from.SolidHueOverride );
				horse.Rider = from;
			}
		}

		public override void OnLeave( Mobile from, LeaveMode leaveMode )
		{
			var horse = GetHorseFor( from );

			if ( horse != null )
				horse.Delete();
		}

		public override void OnResurrect( Mobile m, Mobile healer )
		{
			if ( !m_ReturnHorseOnResurrect )
				return;

			if ( m.Race != Race.Gargoyle )
			{
				var horse = GetHorseFor( m );

				if ( horse == null )
					horse = new GameHorse( m, m.SolidHueOverride );

				horse.Hits = horse.HitsMax;
				horse.Rider = m;
			}
		}

		private GameHorse GetHorseFor( Mobile m )
		{
			return World.Instance.Mobiles.OfType<GameHorse>().Where( horse => horse.ControlMaster == m ).FirstOrDefault();
		}

		public override void OnFinished()
		{
			CleanHorses();
		}

		private static void CleanHorses()
		{
			foreach ( var horse in World.Instance.Mobiles.OfType<GameHorse>().ToArray() )
				horse.Delete();
		}
	}

	public class EquipOnJoinListener : GameListener
	{
		public EquipOnJoinListener( IGame game )
			: base( game )
		{
		}

		public override void OnJoin( Mobile m )
		{
			var earrings = new GoldEarrings();
			earrings.Name = "Game Earrings";
			earrings.Weight = 0.0;
			earrings.Movable = false;
			earrings.LootType = LootType.Cursed;
			earrings.Attributes.CastSpeed = 2;
			earrings.Attributes.CastRecovery = 4;
			m.AddItem( earrings );

			GameEquipStone.GiveEquipTo( m );
		}

		public override void OnFinished()
		{
			GameEquipStone.Reset();
		}
	}
}
