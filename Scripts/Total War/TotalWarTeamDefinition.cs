using System;

namespace Server.Engines.Games
{
	public class TotalWarTeamDefinition : TeamDefinition
	{
		public TotalWarTeamDefinition()
		{
		}

		public override Team CreateTeam( IGame game )
		{
			return new TotalWarTeam( this, game );
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
