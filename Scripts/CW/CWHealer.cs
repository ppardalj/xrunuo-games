using System;

using Server;
using Server.Engines.Games;
using Server.Mobiles;
using Server.Items;

namespace Server.Mobiles
{
	public class CWHealer : BaseHealer
	{
		public override bool CanTeach { get { return false; } }

		private CWTeam m_Team;

		[CommandProperty( AccessLevel.Counselor )]
		public string TeamName
		{
			get { return m_Team.Name; }
		}

		public new CWTeam Team
		{
			get { return m_Team; }
			set { m_Team = value; }
		}

		[Constructable]
		public CWHealer( CWTeam team )
		{
			CantWalk = true;

			m_Team = team;

			Name = String.Format( "{0} team healer", team.Name );

			Body = 0x190;
			Hue = 0x4001;

			SetResistance( ResistanceType.Physical, 50 );
			SetResistance( ResistanceType.Fire, 50 );
			SetResistance( ResistanceType.Cold, 50 );
			SetResistance( ResistanceType.Poison, 50 );
			SetResistance( ResistanceType.Energy, 50 );

			HoodedShroudOfShadows hooded = new HoodedShroudOfShadows();
			hooded.Hue = m_Team.Hue;
			AddItem( hooded );

			Delta( MobileDelta.Noto );
		}

		public override bool CheckResurrect( Mobile m )
		{
			if ( !m_Team.IsMember( m ) )
			{
				Say( "I won't resurrect people who are not on my team." );
				return false;
			}

			return true;
		}

		protected override bool OnBeforeDeath()
		{
			m_Team.OnHealerDeath();
			
			return base.OnBeforeDeath();
		}

		public override int GetRobeColor()
		{
			return 0;
		}

		public override void InitOutfit()
		{
		}

		public override void InitBody()
		{
		}

		public CWHealer( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();
		}
	}
}
