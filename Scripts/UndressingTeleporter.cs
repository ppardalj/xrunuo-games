using System;
using System.Collections;
using System.Linq;

using Server;
using Server.Items;
using Server.Gumps;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Items
{
	public class UndressingTeleporter : Teleporter
	{
		[Constructable]
		public UndressingTeleporter()
		{
			Name = "UndressingTeleporter";
		}

		public UndressingTeleporter( Serial serial )
			: base( serial )
		{
		}

		public override bool OnMoveOver( Mobile m )
		{
			// Undress and send the belongings to the bank box

			if ( Active )
			{
				Dismount( m );

				if ( !BankItems( m ) )
					return true; // dejamos mover pero no hacemos el teleport
			}

			return base.OnMoveOver( m );
		}

		private void Dismount( Mobile m )
		{
			foreach ( var mount in m.GetEquippedItems().OfType<IMountItem>().Select( mountItem => mountItem.Mount ) )
			{
				if ( mount != null )
					mount.Rider = null;
			}

			var item = m.FindItemOnLayer( Layer.Mount );
			if ( item != null )
				item.Delete();
		}

		private static bool BankItems( Mobile m )
		{
			if ( m == null || m.Backpack == null || m.BankBox == null )
				return false;

			try
			{
				m.DropHolding();

				Backpack bag = new Backpack();

				Container pack = m.Backpack;
				BankBox box = m.BankBox;

				foreach ( Item item in m.GetEquippedItems() )
				{
					if ( ( item.Layer != Layer.Bank ) && ( item.Layer != Layer.Backpack ) && ( item.Layer != Layer.Hair ) && ( item.Layer != Layer.FacialHair ) )
						pack.DropItem( item );
					else if ( item is Spells.Spellweaving.ArcaneFocus )
						item.Delete();
				}

				Container pouch = m.Backpack;

				ArrayList finalitems = new ArrayList( pouch.Items );
				ArrayList todelete = new ArrayList();

				foreach ( Item items in finalitems )
				{
					if ( items is Spells.Spellweaving.ArcaneFocus )
					{
						todelete.Add( items );
						continue;
					}

					bag.DropItem( items );
				}

				for ( int i = 0; i < todelete.Count; i++ )
					( (Item) todelete[i] ).Delete();

				box.DropItem( bag );
			}
			catch ( Exception e )
			{
				Scripts.Commands.CommandHandlers.BroadcastMessage( AccessLevel.Counselor, 0x32, String.Format( "Error en UndressingTeleport: {0}", e.ToString() ) );

				Logger.Error( "Error en UndressingTeleport: {0}", e.ToString() );

				return false;
			}

			return true;
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