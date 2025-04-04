using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Badventure.Scripts.Csharp.Solution.Models;
using Badventure.Scripts.Csharp.Solution.Enums;
using System.Xml.Linq;

namespace Badventure.Scripts.Csharp.Solution.Contollers
{
    public class InventoryController
    {
        private const int STACK_SIZE = 16;
 
        private Dictionary<int, InventoryModel> _inventories = new Dictionary<int, InventoryModel>();

        public InventoryModel CreateInventory(int ownerId)
        {
            if (_inventories.ContainsKey(ownerId))
            {
                throw new InvalidOperationException("No inventory has been found for this entity");
            }

            var inventory = new InventoryModel { OwnerId = ownerId };
            _inventories[ownerId] = inventory;
            return inventory;
        }

        public bool AddItem(int ownerId, ItemModel item)
        {
            if (!_inventories.TryGetValue(ownerId, out var inventory))
            {
                throw new InvalidOperationException("No inventory has been found with similar id!");
            }

            if (item.ItemType == ItemType.Consumables || item.ItemType == ItemType.Deployable)
            {
                var partialStackItem = FindPartialStackItem(inventory, item);

                if (partialStackItem != null)
                {
                    return MergeItems(ownerId, item.ItemId, partialStackItem.ItemId);
                }
            }

            for (int i = 0; i < inventory.Hotbar.Length; i++)
            {
                if (inventory.Hotbar[i] == null)
                {
                    inventory.Hotbar[i] = item;
                    return true;
                }
            }

            for (int i = 0; i < inventory.Backpack.Length; i++)
            {
                if (inventory.Backpack[i] == null)
                {
                    inventory.Backpack[i] = item;
                    return true;
                }
            }

            return false;
        }

        private ItemModel FindPartialStackItem(InventoryModel inventory, ItemModel item)
        {
            var hotbarPartialStack = inventory.Hotbar.FirstOrDefault(stackItem => stackItem != null && AreItemsMergeable(stackItem, item) && GetItemQuantity(stackItem) < STACK_SIZE);

            if (hotbarPartialStack != null) return hotbarPartialStack;
            
            return inventory.Backpack.FirstOrDefault(stackItem => stackItem != null && AreItemsMergeable(stackItem, item) && GetItemQuantity(stackItem) < STACK_SIZE);
        }

        public bool RemoveItem(int ownerId, int itemId)
        {
            if (!_inventories.TryGetValue(ownerId, out var inventory)) throw new InvalidOperationException("No inventory has been found with similar id!");


            for (int i = 0; i < inventory.Hotbar.Length; i++)
            {
                if (inventory.Hotbar[i]?.ItemId == itemId)
                {
                    inventory.Hotbar[i] = null;
                    return true;
                }
            }
            
            for (int i = 0; i < inventory.Backpack.Length; i++)
            {
                if (inventory.Backpack[i]?.ItemId == itemId)
                {
                    inventory.Backpack[i] = null;
                    return true;
                }
            }

            return false;
        }

        public bool MoveItem(int ownerId, bool fromHotbar, int fromSlot, bool toHotbar, int toSlot)
        {
            if (!_inventories.TryGetValue(ownerId, out var inventory)) throw new InvalidOperationException("No inventory has been found with similar id!");


            var sourceArray = fromHotbar ? inventory.Hotbar : inventory.Backpack;
            var destinationArray = toHotbar ? inventory.Hotbar : inventory.Backpack;
            
            if (fromSlot < 0 || fromSlot >= sourceArray.Length || toSlot < 0 || toSlot >= destinationArray.Length) return false;
            

            if (sourceArray[fromSlot] == null) return false;
            

            if (destinationArray[toSlot] != null)
            {
                var sourceItem = sourceArray[fromSlot];
                var targetItem = destinationArray[toSlot];

                if (sourceItem.ItemType == ItemType.Consumables || sourceItem.ItemType == ItemType.Deployable)
                    if (AreItemsMergeable(sourceItem, targetItem)) return MergeItems(ownerId, sourceItem.ItemId, targetItem.ItemId); 

                var temp = destinationArray[toSlot];
                destinationArray[toSlot] = sourceArray[fromSlot];
                sourceArray[fromSlot] = temp;
            }
            else
            {
                destinationArray[toSlot] = sourceArray[fromSlot];
                sourceArray[fromSlot] = null;
            }

            return true;
        }

        public bool MergeItems(int ownerId, int sourceItemId, int targetItemId)
        {
            if (!_inventories.TryGetValue(ownerId, out var inventory)) throw new InvalidOperationException("No inventory has been found with similar id!");


            var sourceItem = FindItemById(inventory, sourceItemId);
            var targetItem = FindItemById(inventory, targetItemId);

            if (sourceItem == null || targetItem == null) return false;

            if (sourceItem.ItemType != ItemType.Consumables &&sourceItem.ItemType != ItemType.Deployable) return false;
            if (!AreItemsMergeable(sourceItem, targetItem)) return false;

            int sourceQuantity = GetItemQuantity(sourceItem);
            int targetQuantity = GetItemQuantity(targetItem);
            int totalQuantity = sourceQuantity + targetQuantity;

            if (totalQuantity > STACK_SIZE)
            { 
                SetItemQuantity(targetItem, STACK_SIZE);
                SetItemQuantity(sourceItem, totalQuantity - STACK_SIZE);
                return false;
            }

            MergeCharacteristics(sourceItem, targetItem);
  
            SetItemQuantity(targetItem, totalQuantity);

            RemoveItem(ownerId, sourceItemId);

            return true;
        }

        public List<ItemModel> GetAllItems(int ownerId)
        {
            if (!_inventories.TryGetValue(ownerId, out var inventory)) throw new InvalidOperationException("No inventory has been found with similar id!");

            return inventory.Hotbar.Concat(inventory.Backpack).Where(item => item != null).ToList();
        }

        public int CountItemsByType(int ownerId, ItemType itemType) => GetAllItems(ownerId).Count(item => item.ItemType == itemType);

        private ItemModel FindItemById(InventoryModel inventory, int itemId)
        {
            
            var hotbarItem = inventory.Hotbar.FirstOrDefault(i => i?.ItemId == itemId);

            if (hotbarItem != null) return hotbarItem;

              
            return inventory.Backpack.FirstOrDefault(i => i?.ItemId == itemId);
        }

        private ItemModel FindSimilarStackItem(InventoryModel inventory, ItemModel item)
        {
            var hotbarItem = inventory.Hotbar.FirstOrDefault(i => i != null && AreItemsMergeable(i, item) && GetItemQuantity(i) < STACK_SIZE);

            if (hotbarItem != null) return hotbarItem;

            return inventory.Backpack.FirstOrDefault(i => i != null && AreItemsMergeable(i, item) && GetItemQuantity(i) < STACK_SIZE);
        }

        private bool AreItemsMergeable(ItemModel sourceItem, ItemModel targetItem)
        {
            if (sourceItem.ItemType != targetItem.ItemType) return false;
           
 
            return sourceItem.ItemName == targetItem.ItemName && sourceItem.ItemDescription == targetItem.ItemDescription && sourceItem.ItemRarity == targetItem.ItemRarity;
        }
        private void MergeCharacteristics(ItemModel sourceItem, ItemModel targetItem)
        {
            if (sourceItem.Attributes == null || targetItem.Attributes == null) return;
            

            var mergedCharacteristics = targetItem.Attributes.GroupBy(c => c.Attribute).ToDictionary(g => g.Key, g => g.Sum(c => c.Value));

            targetItem.Attributes = mergedCharacteristics.Select(kvp => new AttributeModel{ Attribute = kvp.Key,Value = kvp.Value}).ToList();
        }

        private int GetItemQuantity(ItemModel item)
        {
            if (item.Attributes == null) 
                return 1;

            var quantityChar = item.Attributes.FirstOrDefault(c => c.Attribute == AttributeType.Quanity);

            return quantityChar?.Value ?? 1;
        }

        private void SetItemQuantity(ItemModel item, int quantity)
        {
            if (item.Attributes == null) item.Attributes = new List<AttributeModel>();
            

            var quantityChar = item.Attributes.FirstOrDefault(c => c.Attribute == AttributeType.Quanity);

            if (quantityChar != null)
            {
                quantityChar.Value = quantity;
            }
            else
            {
                item.Attributes.Add(new AttributeModel(AttributeType.Quanity, quantity));
            }
        }

        public ItemModel CreateItemStack(ItemModel baseItem, int quantity)
        {
            if (baseItem.ItemType != ItemType.Consumables && baseItem.ItemType != ItemType.Deployable) throw new InvalidOperationException("Stack can be created only for items of type consumbales or deployable");

            quantity = Math.Min(quantity, STACK_SIZE);

            var stackItem = new ItemModel(
                baseItem.ItemId,
                baseItem.ItemName,
                baseItem.ItemDescription,
                baseItem.ItemRarity,
                baseItem.ItemType,
                baseItem.Attributes?.ToList()
            );
            stackItem.Attributes ??= new List<AttributeModel>();

            var quantityCharacteristic = stackItem.Attributes.FirstOrDefault(c => c.Attribute == AttributeType.Quanity);

            if (quantityCharacteristic != null)
            {
                quantityCharacteristic.Value = quantity;
            }    
            else
            {
                stackItem.Attributes.Add(new AttributeModel
                {
                    Attribute = AttributeType.Quanity,
                    Value = quantity
                });
            }

            return stackItem;
        }

        public ItemModel ExtractItemFromStack(int ownerId, int itemId)
        {
            if (!_inventories.TryGetValue(ownerId, out var inventory)) throw new InvalidOperationException("No inventory has been found with similar id!");

            var stackItem = FindItemById(inventory, itemId);

            if (stackItem == null) 
                return null;
            
            if (stackItem.ItemType != ItemType.Consumables && stackItem.ItemType != ItemType.Deployable) 
                return null;

            int currentQuantity = GetItemQuantity(stackItem);

            if (currentQuantity <= 1)
            {
                RemoveItem(ownerId, itemId);
                return stackItem;
            }

            var extractedItem = new ItemModel(
                stackItem.ItemId,
                stackItem.ItemName,
                stackItem.ItemDescription,
                stackItem.ItemRarity,
                stackItem.ItemType,
                stackItem.Attributes?.Select(c => new AttributeModel(c.Attribute, c.Value)).ToList()
            );

            SetItemQuantity(stackItem, currentQuantity - 1);

            return extractedItem;
        }
    }
}
