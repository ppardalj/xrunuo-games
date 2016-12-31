using System;
using System.Collections;

using Server;
using Server.Accounting;
using Server.Engines.Games;
using Server.Items;
using Server.Gumps;

namespace Server.Items
{
	[FlipableAttribute( 0xEDC, 0xEDB )]
	public class JoinStone : Item
	{
		private bool m_Active;
		private Embryo m_Embryo;
		private GameDefinition m_Definition;
		private int m_DefinitionId;

		[CommandProperty( AccessLevel.GameMaster, AccessLevel.Seer )]
		public bool Active
		{
			get { return m_Active; }
			set { m_Active = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsOpened
		{
			get { return m_Embryo != null; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public string GameName
		{
			get { return m_Definition.Name; }
		}

		public JoinStone( int definitionId )
			: base( 0xEDC )
		{
			SetDefinition( definitionId );

			Movable = false;
		}

		private void SetDefinition( int definitionId )
		{
			var definitions = Manager.Instance.Definitions;

			if ( !definitions.ContainsKey( definitionId ) )
			{
				Delete();
			}
			else
			{
				m_Definition = definitions[definitionId];
				m_DefinitionId = definitionId;

				InvalidateProperties();
			}
		}

		public override LocalizedText GetNameProperty()
		{
			return new LocalizedText( String.Format( "{0} join stone", m_Definition.Name ) );
		}

		public void Open()
		{
			if ( m_Embryo != null )
				throw new Exception( "Tried to open join stone already opened" );

			m_Embryo = new Embryo();
		}

		public void JoinGame( Mobile m )
		{
			if ( m_Embryo == null )
				throw new Exception( "Tried to join the game of a closed stone" );

			m_Embryo.JoinWaitingList( m );

			// TODO: Raise player joined event to allow, for example, tracking.
		}

		public void StartGame()
		{
			if ( m_Embryo == null )
				throw new Exception( "Tried to start game of join stone not opened" );

			IGame game = m_Definition.CreateGame();
			game.StartGame( m_Embryo );

			m_Embryo = null;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Account acc = from.Account as Account;

			if ( acc == null )
				return;

			if ( !IsOpened )
			{
				from.SendMessage( 32, "This event is currently closed." );
			}
			// TODO: Raise event to allow other systems to vote for game join allowance, such as jail.
			else if ( from.Mounted == true )
			{
				from.SendMessage( 32, "You cannot join the game while mounted." );
			}
			else if ( from.IsBodyMod )
			{
				from.SendMessage( 32, "You cannot join the game while polymorphed." );
			}
			else
			{
				from.CloseGump( typeof( Spells.Ninjitsu.AnimalForm.AnimalFormGump ) );

				from.SendGump( new JoinGump( this ) );
			}
		}

		public JoinStone( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (bool) m_Active );
			writer.Write( (int) m_DefinitionId );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
					{
						m_Active = reader.ReadBool();
						goto case 0;
					}
				case 0:
					{
						int definitionId = reader.ReadInt();
						Timer.DelayCall( TimeSpan.Zero, () => SetDefinition( definitionId ) );

						break;
					}
			}

			if ( version < 1 )
				m_Active = true;
		}
	}
}
