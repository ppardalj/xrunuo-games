using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Gumps;

namespace Server.Engines.Games
{
	public class CTFGameDefinition : GameDefinition
	{
		private int m_MaxScore;

		[CommandProperty( AccessLevel.Seer )]
		public int MaxScore
		{
			get { return m_MaxScore; }
			set { m_MaxScore = value; }
		}

		public override IGame CreateGame()
		{
			return new CTFGame( this );
		}

		public override TeamDefinition CreateTeamDefinition()
		{
			return new CTFTeamDefinition();
		}

		public override GameDefinitionGump CreateDefinitionGump( Manager manager, int gameId )
		{
			return new CTFGameDefinitionGump( manager, gameId );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_MaxScore );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();

			m_MaxScore = reader.ReadInt();
		}
	}
}
