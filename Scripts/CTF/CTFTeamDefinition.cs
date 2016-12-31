using System;

namespace Server.Engines.Games
{
	public class CTFTeamDefinition : TeamDefinition
	{
		private Point3D m_FlagHome;

		[CommandProperty( AccessLevel.Seer )]
		public Point3D FlagHome
		{
			get { return m_FlagHome; }
			set { m_FlagHome = value; }
		}

		public CTFTeamDefinition()
		{
		}

		public override Team CreateTeam( IGame game )
		{
			return new CTFTeam( this, game as CTFGame );
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_FlagHome );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();

			m_FlagHome = reader.ReadPoint3D();
		}
	}
}
