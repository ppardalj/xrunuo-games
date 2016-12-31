using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;

namespace Server.Engines.Games
{
	public class Manager
	{
		private static Manager m_Instance;

		public static Manager Instance
		{
			get
			{
				if ( m_Instance == null )
					m_Instance = new Manager();

				return m_Instance;
			}
		}

		private int m_NextId;
		private Dictionary<int, GameDefinition> m_Definitions;

		public Dictionary<int, GameDefinition> Definitions
		{
			get { return m_Definitions; }
		}

		public Manager()
		{
			m_Definitions = new Dictionary<int, GameDefinition>();
			m_NextId = 1;
		}

		public int AddDefinition( GameDefinition definition )
		{
			int id = m_NextId++;
			m_Definitions[id] = definition;
			return id;
		}

		public void RemoveDefinition( int id )
		{
			m_Definitions.Remove( id );
			// TODO: handle existing join stones with this definition attached.
		}

		#region Persistence
		public void Serialize( GenericWriter writer )
		{
			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_NextId );
			writer.Write( (int) m_Definitions.Count );
			foreach ( var definition in m_Definitions )
			{
				writer.Write( definition.Key );
				writer.Write( definition.Value.GetType().FullName );
				definition.Value.Serialize( writer );
			}
		}

		public void Deserialize( GenericReader reader )
		{
			/*int version = */
			reader.ReadInt();

			m_NextId = reader.ReadInt();
			int definitionCount = reader.ReadInt();
			for ( int i = 0; i < definitionCount; i++ )
			{
				int index = reader.ReadInt();
				string fullName = reader.ReadString();
				Type type = ScriptCompiler.FindTypeByFullName( fullName );
				GameDefinition definition = (GameDefinition) Activator.CreateInstance( type );
				definition.Deserialize( reader );
				m_Definitions[index] = definition;
			}
		}
		#endregion
	}
}
