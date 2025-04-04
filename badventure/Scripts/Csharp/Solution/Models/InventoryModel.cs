using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class InventoryModel
    {
        public int OwnerId { get; set; }

        public ItemModel[] Hotbar { get; set; } = new ItemModel[6];

        public ItemModel[] Backpack { get; set; } = new ItemModel[18];

        public int TotalItemCount => Hotbar.Count(x => x != null) + Backpack.Count(x => x != null);

        public bool IsSlotEmpty(bool isHotbar, int slotIndex)
        {
            var array = isHotbar ? Hotbar : Backpack;
            return array[slotIndex] == null;
        }
    }
}
