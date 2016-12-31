using System;

namespace Server.Engines.Games
{
	public class CWTeamDefinition : TeamDefinition
	{
		private Point3D m_HealerHome;

		[CommandProperty( AccessLevel.Seer )]
		public Point3D HealerHome
		{
			get { return m_HealerHome; }
			set { m_HealerHome = value; }
		}

		public CWTeamDefinition()
		{
		}

		public override Team CreateTeam( IGame game )
		{
			return new CWTeam( this, game );
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_HealerHome );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();

			m_HealerHome = reader.ReadPoint3D();
		}
	}
}
