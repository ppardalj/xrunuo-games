using System;
using System.Collections.Generic;

using Server;
using Server.Engines.Games;

namespace Server.Items
{
	public class ManagerPersistance : Item
	{
		private static ManagerPersistance m_Instance;

		public static ManagerPersistance Instance { get { return m_Instance; } }

		private ManagerPersistance()
			: base( 1 )
		{
			Name = "Game Manager Stone - Internal";
			Movable = false;

			if ( m_Instance == null || m_Instance.Deleted )
				m_Instance = this;
			else
				base.Delete();
		}

		public override void Delete()
		{
		}

		public static void Ensure()
		{
			if ( m_Instance == null )
				new ManagerPersistance();
		}

		public ManagerPersistance( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			Manager.Instance.Serialize( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();

			Manager.Instance.Deserialize( reader );
		}
	}
}
