using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Spells.Mysticism;
using Server.Spells.Spellweaving;

namespace Server.Engines.Games
{
	public static class BankHelper
	{
		private static Dictionary<Mobile, BankedItemsContext> m_Table = new Dictionary<Mobile, BankedItemsContext>();

		private static Type[] m_IllegalItemTypes = new Type[]
			{
				typeof( ArcaneFocus ),	typeof( HealingStoneSpell.HealingStone ),
				typeof( SpellStone )
			};

		private static bool IsIllegal( this Item item )
		{
			return m_IllegalItemTypes.Contains( item.GetType() );
		}

		public static void BankItems( Mobile m )
		{
			m.DropHolding();

			Bag bag = new Bag();

			Container backpack = m.Backpack;
			BankBox bankBox = m.BankBox;

			var equipped = new List<Item>();

			// Drop all equiped items to backpack
			foreach ( var item in m.GetEquippedItems().ToArray() )
			{
				if ( ( item.Layer != Layer.Bank ) && ( item.Layer != Layer.Backpack ) && ( item.Layer != Layer.Hair ) && ( item.Layer != Layer.FacialHair ) )
				{
					backpack.DropItem( item );
					equipped.Add( item );
				}
			}

			foreach ( var item in m.GetInventoryItems().ToArray() )
			{
				if ( item.IsIllegal() )
					item.Delete();
				else
					bag.DropItem( item );
			}

			bankBox.DropItem( bag );

			m_Table[m] = new BankedItemsContext( m, bag, equipped );
		}

		public static void RestoreBankedItems( Mobile m )
		{
			if ( m_Table.ContainsKey( m ) )
			{
				var context = m_Table[m];

				foreach ( var item in context.Equipped )
				{
					if ( item.IsChildOf( m ) )
						m.EquipItem( item );
				}

				if ( context.Container.IsChildOf( m ) && m.Backpack != null )
				{
					foreach ( var item in context.Container.Items.ToArray() )
					{
						m.PlaceInBackpack( item );
					}
				}

				if ( context.Container.Items.Count == 0 )
					context.Container.Delete();
			}
		}

		private class BankedItemsContext
		{
			public Mobile Mobile { get; private set; }
			public Container Container { get; private set; }
			public List<Item> Equipped { get; private set; }

			public BankedItemsContext( Mobile m, Container cont, List<Item> equipped )
			{
				this.Mobile = m;
				this.Container = cont;
				this.Equipped = equipped;
			}
		}
	}
}
