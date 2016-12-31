using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Games
{
	public abstract class TeamDefinition
	{
		public abstract Team CreateTeam( IGame game );

		private string m_Name;
		private int m_Hue;
		private Point3D m_Home;

		[CommandProperty( AccessLevel.Seer )]
		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public int Hue
		{
			get { return m_Hue; }
			set { m_Hue = value; }
		}

		[CommandProperty( AccessLevel.Seer )]
		public Point3D Home
		{
			get { return m_Home; }
			set { m_Home = value; }
		}

		public TeamDefinition()
		{
		}
		
		public virtual void Serialize( GenericWriter writer )
		{
			writer.Write( (int) 0 ); // version

			writer.Write( m_Hue );
			writer.Write( m_Name );
			writer.Write( m_Home );
		}

		public virtual void Deserialize( GenericReader reader )
		{
			/*int version = */
			reader.ReadInt();

			m_Hue = reader.ReadInt();
			m_Name = reader.ReadString();
			m_Home = reader.ReadPoint3D();
		}
	}
}
