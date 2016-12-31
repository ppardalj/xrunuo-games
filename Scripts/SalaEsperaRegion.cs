using System;
using System.Collections;
using System.Text;
using System.Xml;
using Server;
using Server.Items;

namespace Server.Regions
{
	public class SalaEsperaRegion : BaseRegion
	{
		public SalaEsperaRegion( XmlElement xml, Map map, Region parent )
			: base( xml, map, parent )
		{
		}

		public override void OnEnter( Mobile m )
		{
			m.DropHolding();
			m.Target = null; // por si traemos un mark de fuera, o cualquier otra cosa con la que dar por saco
		}

		public override bool CanUseStuckMenu( Mobile m )
		{
			return false;
		}

		public override bool OnBeginSpellCast( Mobile m, ISpell s )
		{
			m.SendMessage( "You cannot cast a spell here." );
			return false;
		}

		public override bool AllowHousing( Mobile from, Point3D p )
		{
			return false;
		}

		public override bool AllowBeneficial( Mobile from, Mobile target )
		{
			return false;
		}

		public override bool AllowHarmful( Mobile from, Mobile target )
		{
			return false;
		}
	}
}
