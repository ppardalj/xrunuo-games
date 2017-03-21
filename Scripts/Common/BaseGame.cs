using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Scripts.Commands;
using Server.Misc;
using Server.Events;

namespace Server.Engines.Games
{
	public abstract class BaseGame<TDefinition> : IGame where TDefinition : GameDefinition
	{
		public readonly static Point3D EndLocation = new Point3D( 5152, 1761, 0 );

		protected TDefinition m_Definition;

		private Region m_Region;
		private DateTime m_StartTime;
		private Timer m_SliceTimer;
		private Timer m_GameTimer;
		private HashSet<GameListener> m_EventListeners;

		public string Name { get { return m_Definition.Name; } }
		public Map Map { get { return m_Definition.Map; } }
		public Rectangle2D Area { get { return m_Definition.Area; } }
		public TimeSpan Duration { get { return m_Definition.Duration; } }
		public DateTime StartTime { get { return m_StartTime; } }

		public BaseGame( TDefinition definition )
		{
			m_Definition = definition;
		}

		public abstract IEnumerable<Mobile> GetPlayers();

		public abstract bool IsPlaying( Mobile m );

		public virtual bool OverridesNotoriety( Mobile from, Mobile target )
		{
			return IsPlaying( from ) && IsPlaying( target );
		}

		public abstract int ComputeNotoriety( Mobile from, Mobile target );

		protected abstract void Initialize( Embryo embryo );

		protected abstract GameVictoryInfo ComputeVictory();

		protected abstract Point3D GetHomeLocation( Mobile m );

		protected abstract int GetSolidOverrideHue( Mobile m );

		public virtual bool AllowBeneficial( Mobile from, Mobile target )
		{
			return ComputeNotoriety( from, target ) == Notoriety.Ally;
		}

		public virtual bool AllowHarmful( Mobile from, Mobile target )
		{
			return ComputeNotoriety( from, target ) == Notoriety.Enemy;
		}

		public T GetListener<T>() where T : GameListener
		{
			return m_EventListeners.OfType<T>().FirstOrDefault();
		}

		protected virtual void AttachEventListeners()
		{
			AttachEventListener( new LogoutKicker( this ) );

			if ( Misc.TestCenter.Enabled )
				AttachEventListener( new DebugListener( this ) );
		}

		protected void AttachEventListener( GameListener listener )
		{
			m_EventListeners.Add( listener );
		}

		public void StartGame( Embryo embryo )
		{
			m_EventListeners = new HashSet<GameListener>();

			AttachEventListeners();

			EventSink.PlayerDeath += new PlayerDeathEventHandler( OnPlayerDeath );

			m_Region = CreateRegion();
			m_Region.Register();

			Initialize( embryo );

			foreach ( var player in GetPlayers() )
				OnJoin( player );

			m_StartTime = DateTime.Now;

			m_SliceTimer = Timer.DelayCall( TimeSpan.FromSeconds( 5.0 ), TimeSpan.FromSeconds( 5.0 ), this.OnSlice );

			if ( Duration > TimeSpan.Zero )
				m_GameTimer = Timer.DelayCall( Duration, new TimerCallback( FinishGame ) );

			OnStarted();

			Registry.Instance.OnGameStarted( this );
		}

		protected virtual GameRegion CreateRegion()
		{
			return new GameRegion( this );
		}

		private void OnPlayerDeath( PlayerDeathEventArgs e )
		{
			var game = GameHelper.FindGameFor( e.Mobile );

			if ( game == this )
				OnDeath( e.Mobile );
		}

		public void ReturnToHome( Mobile m )
		{
			m.Location = this.GetHomeLocation( m );
			m.Map = this.Map;
		}

		public void LeaveGame( Mobile m, LeaveMode leavemode = LeaveMode.Legal )
		{
			if ( !IsPlaying( m ) )
				return;

			OnLeave( m, leavemode );
		}

		public void FinishGame()
		{
			EventSink.PlayerDeath -= OnPlayerDeath;

			if ( m_SliceTimer != null )
			{
				m_SliceTimer.Stop();
				m_SliceTimer = null;
			}

			if ( m_GameTimer != null )
			{
				m_GameTimer.Stop();
				m_GameTimer = null;
			}

			var victoryInfo = ComputeVictory();

			if ( !string.IsNullOrEmpty( victoryInfo.Message ) )
			{
				this.BroadcastMessage( victoryInfo.Message );
				World.Broadcast( 0x489, true, victoryInfo.Message );
			}

			foreach ( var player in victoryInfo.Winners )
				OnWin( player );

			foreach ( var player in GetPlayers().Except( victoryInfo.Winners ) )
				OnLose( player );

			foreach ( var player in GetPlayers().ToArray() )
				LeaveGame( player );

			OnFinished();

			Registry.Instance.OnGameFinished( this );

			m_Region.Unregister();
			m_Region = null;
		}

		#region Game events

		public virtual void OnStarted()
		{
			#region Notoriety
			// Bypass the current notoriety handlers
			m_MobileHandler = NotorietyHandlers.MobileHandler;
			m_CorpseHandler = NotorietyHandlers.CorpseHandler;

			NotorietyHandlers.MobileHandler = MobileNotoriety;
			NotorietyHandlers.CorpseHandler = CorpseNotoriety;
			#endregion

			this.BroadcastMessage( "The game has started!!" );

			foreach ( var listener in m_EventListeners )
				listener.OnStarted();
		}

		public virtual void OnJoin( Mobile m )
		{
			if ( !m.Alive )
				m.Resurrect();

			var home = GetHomeLocation( m );

			m.LogoutLocation = home;
			m.Location = home;

			m.LogoutMap = this.Map;
			m.Map = this.Map;

			m.Hits = m.HitsMax;
			m.Mana = m.ManaMax;
			m.Stam = m.StamMax;

			m.Frozen = false;
			m.Hidden = false;
			m.Flying = false;

			if ( m is PlayerMobile )
				( (PlayerMobile) m ).BlocksFameAward = true;

			m.SolidHueOverride = GetSolidOverrideHue( m );

			if ( Spells.Ninjitsu.AnimalForm.UnderTransformation( m ) )
				Spells.Ninjitsu.AnimalForm.RemoveContext( m, true );

			#region Notoriety
			m.Delta( MobileDelta.Noto );
			#endregion

			foreach ( var listener in m_EventListeners )
				listener.OnJoin( m );
		}

		public virtual void OnLeave( Mobile m, LeaveMode leavemode )
		{
			#region Notoriety
			m.Delta( MobileDelta.Noto );
			#endregion

			m.DropHolding();

			foreach ( Item item in m.GetEquippedItems() )
			{
				if ( ( item.Layer != Layer.Bank ) && ( item.Layer != Layer.Backpack ) && ( item.Layer != Layer.Hair ) && ( item.Layer != Layer.FacialHair ) )
					item.Delete();
			}

			foreach ( Item item in m.Backpack.Items.ToArray() )
				item.Delete();

			if ( !m.Alive )
				m.Resurrect();

			m.Hits = m.HitsMax;
			m.Stam = m.StamMax;
			m.Mana = m.ManaMax;

			m.Frozen = false;
			m.Poison = null;

			if ( m is PlayerMobile )
				( (PlayerMobile) m ).BlocksFameAward = false;

			m.SolidHueOverride = -1;

			BankHelper.RestoreBankedItems( m );

			foreach ( var listener in m_EventListeners )
				listener.OnLeave( m, leavemode );

			m.MoveToWorld( EndLocation, Map.Trammel );
		}

		public virtual void OnWin( Mobile m )
		{
			if ( m.NetState != null )
				GiveReward( m );

			foreach ( var listener in m_EventListeners )
				listener.OnWin( m );
		}

		public virtual void OnLose( Mobile m )
		{
			if ( m.NetState != null )
			{
				foreach ( var listener in m_EventListeners )
					listener.OnLose( m );
			}
		}

		protected virtual void GiveReward( Mobile m )
		{
			int tickets = 1;
			int gold = 10000;

			m.BankBox.AddItem( new BankCheck( gold ) );
			m.BankBox.AddItem( new TournamentTicket( tickets ) );

			m.SendMessage( 64, "Your win the game and receive {0} gp and {1} Tournament Ticket.", gold, tickets );
		}

		public virtual void OnFinished()
		{
			#region Notoriety
			// Restore original notoriety handlers
			NotorietyHandlers.MobileHandler = m_MobileHandler;
			NotorietyHandlers.CorpseHandler = m_CorpseHandler;
			#endregion

			GameHelper.ClearAddressList();
			GameHelper.IsGameRunning = false;

			foreach ( var listener in m_EventListeners )
				listener.OnFinished();
		}

		public virtual void OnSlice()
		{
			foreach ( var listener in m_EventListeners )
				listener.OnSlice();
		}

		public virtual void OnHeal( Mobile m, Mobile healer, int amount )
		{
			foreach ( var listener in m_EventListeners )
				listener.OnHeal( m, healer, amount );
		}

		public virtual void OnResurrect( Mobile m, Mobile healer )
		{
			foreach ( var listener in m_EventListeners )
				listener.OnResurrect( m, healer );
		}

		public virtual void OnDeath( Mobile m )
		{
			Mobile mostRecentDamager = m.FindMostRecentDamager( false );
			if ( mostRecentDamager != null )
			{
				OnKill( m, mostRecentDamager );
			}

			foreach ( var damageEntry in m.DamageEntries )
			{
				if ( damageEntry.Damager != mostRecentDamager
					&& damageEntry.Damager != m
					&& damageEntry.LastDamage > DateTime.Now - TimeSpan.FromSeconds( 5.0 )
					&& (double) damageEntry.DamageGiven / m.HitsMax > 0.25 )
				{
					OnAssist( m, damageEntry.Damager );
				}
			}

			m.DamageEntries.Clear();
		}

		public virtual void OnKill( Mobile m, Mobile killer )
		{
			foreach ( var listener in m_EventListeners )
				listener.OnKill( m, killer );
		}

		public virtual void OnAssist( Mobile m, Mobile assistant )
		{
			foreach ( var listener in m_EventListeners )
				listener.OnAssist( m, assistant );
		}

		#endregion

		#region Notoriety
		private NotorietyHandler m_MobileHandler;
		private CorpseNotorietyHandler m_CorpseHandler;

		private int MobileNotoriety( Mobile source, Mobile target )
		{
			if ( OverridesNotoriety( source, target ) )
				return ComputeNotoriety( source, target );
			else
				return m_MobileHandler( source, target );
		}

		private int CorpseNotoriety( Mobile source, Corpse target )
		{
			if ( target.Owner != null && OverridesNotoriety( source, target.Owner ) )
				return ComputeNotoriety( source, target.Owner );
			else
				return m_CorpseHandler( source, target );
		}
		#endregion
	}
}
