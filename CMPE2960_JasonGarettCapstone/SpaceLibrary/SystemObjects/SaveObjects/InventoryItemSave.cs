using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class InventoryItemSave : GameObjectSave
    {
        //What type of item this is
        public InventoryItem.ItemType m_ItemType;
        //How much the item is worth
        public uint m_Cost;

        public InventoryItemSave(InventoryItem ii)
            :base(ii)
        {
            m_ItemType = ii.m_ItemType;
            m_Cost = ii.m_Cost;
        }

        public InventoryItemSave()
        { }
    }
}
