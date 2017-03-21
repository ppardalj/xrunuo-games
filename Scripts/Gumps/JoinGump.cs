using System;
using Server.Engines.Games;
using Server.Items;
using Server.Network;

namespace Server.Gumps
{
	public class JoinGump : Gump
	{
		private static readonly ClientVersion MinVersion = new ClientVersion( "7.0.24.0" );

		private JoinStone m_Stone;

		public JoinGump( JoinStone stone )
			: base( 20, 30 )
		{
			m_Stone = stone;

			AddPage( 0 );
			AddBackground( 0, 0, 550, 220, 5054 );
			AddBackground( 10, 10, 530, 200, 3000 );

			AddPage( 1 );
			AddLabel( 20, 20, 0, String.Format( "�Bienvenido a la {0}!", m_Stone.GameName ) );
			AddLabel( 20, 60, 0, "Todos los objetos que lleves encima seran automaticamente enviados" );
			AddLabel( 20, 80, 0, "a tu banco. Guarda tu montura en el establo antes de entrar. Todos los " );
			AddLabel( 20, 100, 0, "items que necesites se te daran dentro del Juego. �Diviertete!" );

			AddLabel( 55, 180, 0, "Cancelar" );
			AddButton( 20, 180, 4005, 4006, 0, GumpButtonType.Reply, 0 );
			AddLabel( 165, 180, 0, "�Si, entrar!" );
			AddButton( 130, 180, 4005, 4006, 1, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			from.CloseGump( typeof( JoinGump ) );

			if ( info.ButtonID == 1 )
			{
				if ( !from.InRange( m_Stone.Location, 2 ) )
				{
					from.SendLocalizedMessage( 500446 ); // That is too far away.
				}
				else if ( !Misc.TestCenter.Enabled && GameHelper.IsUsingMulticlient( from ) )
				{
					from.SendMessage( 32, "No se permite la entrada al juego con 2 clientes." );
				}
				else if ( from.IsBodyMod )
				{
					from.SendMessage( 32, "No se permite entrar al juego transformado." );
				}
				else if ( from.HasTrade )
				{
					from.SendMessage( 32, "No se permite entrar al juego mientras se comercia." );
				}
				else if ( from.Mounted )
				{
					from.SendMessage( 32, "No se permite entrar al juego con montura" );
				}
				else if ( from.SkillsTotal < 5000 )
				{
					from.SendMessage( 32, "No se permite la entrada al juego a personajes con menos de 500 puntos de skill." );
				}
				else if ( from.NetState.Version == null || from.NetState.Version < MinVersion )
				{
					from.SendMessage( 32, "Para jugar es necesario tener al menos el cliente {0}", MinVersion.ToString() );
				}
				else if ( !m_Stone.IsOpened )
				{
					from.SendMessage( 32, "La entrada al juego est� cerrada." );
				}
				else
				{
					from.CloseGump( typeof( Spells.Ninjitsu.AnimalForm.AnimalFormGump ) );

					m_Stone.JoinGame( from );
				}
			}
		}
	}
}
