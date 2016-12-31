using System;
using Server;

namespace Server.Items
{
	public class SurvivalInvisPotion : InvisibilityPotion
	{
		[Constructable]
		public SurvivalInvisPotion()
		{
		}

		public override TimeSpan ComputeDelay( Mobile from )
		{
			return TimeSpan.Zero;
		}

		public override TimeSpan ComputeDuration( Mobile from )
		{
			return TimeSpan.FromSeconds( 2.0 );
		}

		public override void PlayDrinkEffect( Mobile from )
		{
			// Smoke bomb effect
			from.FixedParticles( 0x3709, 1, 30, 9904, 1108, 6, EffectLayer.RightFoot );
			from.PlaySound( 0x22F );
		}

		public SurvivalInvisPotion( Serial serial )
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