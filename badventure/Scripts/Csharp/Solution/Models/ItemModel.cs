using System;
using System.Collections.Generic;
using Badventure.Scripts.Csharp.Solution.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class ItemModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public ItemRarity ItemRarity { get; set; }
        public ItemType ItemType { get; set; }
        public List<AttributeModel> Attributes { get; set; }

        public ItemModel(int id, string name, string description, ItemRarity rarity, ItemType type, IEnumerable<AttributeModel> characteristics)
        {
            this.ItemId = id;
            this.ItemName = name;
            this.ItemDescription = description;
            this.ItemType = type;
            foreach (var characteristic in characteristics)
            {
                this.Attributes.Add(characteristic);
            }
        }
    }
}
