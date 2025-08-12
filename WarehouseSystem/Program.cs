using System;
using System.Collections.Generic;
using System.Linq;

namespace WareHouseInventory
{
    // ===== Marker interface =====
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }

    // ===== Product classes =====
    public class ElectronicItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public string Brand { get; }
        public int WarrantyMonths { get; }

        public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            Brand = brand;
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString()
        {
            return $"[Electronic] Id={Id}, Name={Name}, Qty={Quantity}, Brand={Brand}, Warranty={WarrantyMonths}m";
        }
    }

    public class GroceryItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            ExpiryDate = expiryDate;
        }

        public override string ToString()
        {
            return $"[Grocery] Id={Id}, Name={Name}, Qty={Quantity}, Expires={ExpiryDate:yyyy-MM-dd}";
        }
    }

    // ===== Custom exceptions =====
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message) { }
    }

    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }

    public class InvalidQuantityException : Exception
    {
        public InvalidQuantityException(string message) : base(message) { }
    }

    // ===== Generic Inventory Repository =====
    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new();

        public void AddItem(T item)
        {
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"Item with ID {item.Id} already exists.");

            _items[item.Id] = item;
        }

        public T GetItemById(int id)
        {
            if (!_items.TryGetValue(id, out var item))
                throw new ItemNotFoundException($"Item with ID {id} not found.");

            return item;
        }

        public void RemoveItem(int id)
        {
            if (!_items.Remove(id))
                throw new ItemNotFoundException($"Cannot remove: item with ID {id} not found.");
        }

        public List<T> GetAllItems()
        {
            return _items.Values.ToList();
        }

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative.");

            if (!_items.TryGetValue(id, out var item))
                throw new ItemNotFoundException($"Cannot update: item with ID {id} not found.");

            item.Quantity = newQuantity;
        }
    }

    // ===== Warehouse manager =====
    public class WareHouseManager
    {
        public InventoryRepository<ElectronicItem> _electronics { get; } = new();
        public InventoryRepository<GroceryItem> _groceries { get; } = new();

        // Seed with sample items
        public void SeedData()
        {
            try
            {
                _electronics.AddItem(new ElectronicItem(1, "Laptop", 10, "BrandA", 24));
                _electronics.AddItem(new ElectronicItem(2, "Smartphone", 25, "BrandB", 12));
                _electronics.AddItem(new ElectronicItem(3, "Router", 15, "BrandC", 18));

                _groceries.AddItem(new GroceryItem(101, "Rice (5kg)", 50, DateTime.Now.AddMonths(12)));
                _groceries.AddItem(new GroceryItem(102, "Palm Oil (1L)", 30, DateTime.Now.AddMonths(6)));
                _groceries.AddItem(new GroceryItem(103, "Canned Beans", 40, DateTime.Now.AddMonths(18)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while seeding data: {ex.Message}");
            }
        }

        // Generic printer
        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            var items = repo.GetAllItems();
            if (items.Count == 0)
            {
                Console.WriteLine("No items found.");
                return;
            }

            foreach (var item in items)
            {
                Console.WriteLine(item.ToString());
            }
        }

        // Increase stock (wrapping errors)
        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            try
            {
                var item = repo.GetItemById(id);
                if (quantity < 0)
                {
                    throw new InvalidQuantityException("Increase quantity must be non-negative.");
                }

                int newQty = item.Quantity + quantity;
                repo.UpdateQuantity(id, newQty);
                Console.WriteLine($"Updated Item ID {id}: New Quantity = {newQty}");
            }
            catch (DuplicateItemException dex)
            {
                Console.WriteLine($"Duplicate error: {dex.Message}");
            }
            catch (ItemNotFoundException inf)
            {
                Console.WriteLine($"Not found: {inf.Message}");
            }
            catch (InvalidQuantityException iq)
            {
                Console.WriteLine($"Invalid quantity: {iq.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while increasing stock: {ex.Message}");
            }
        }

        // Remove item wrapper
        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            try
            {
                repo.RemoveItem(id);
                Console.WriteLine($"Item with ID {id} removed successfully.");
            }
            catch (ItemNotFoundException inf)
            {
                Console.WriteLine($"Cannot remove: {inf.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while removing item: {ex.Message}");
            }
        }
    }

    // ===== Program Main =====
    public class Program
    {
        static void Main(string[] args)
        {
            var manager = new WareHouseManager();

            // Seed data
            manager.SeedData();

            Console.WriteLine("=== Grocery Items ===");
            manager.PrintAllItems(manager._groceries);
            Console.WriteLine();

            Console.WriteLine("=== Electronic Items ===");
            manager.PrintAllItems(manager._electronics);
            Console.WriteLine();

            // 1) Try to add a duplicate item (should throw DuplicateItemException)
            Console.WriteLine("Attempting to add duplicate electronic item (ID = 1)");
            try
            {
                manager._electronics.AddItem(new ElectronicItem(1, "Laptop Duplicate", 5, "BrandX", 12));
            }
            catch (DuplicateItemException dex)
            {
                Console.WriteLine($"Caught expected duplicate error: {dex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            Console.WriteLine();

            // 2) Try to remove a non-existent item
            Console.WriteLine("Attempting to remove non-existent grocery item (ID = 999)");
            manager.RemoveItemById(manager._groceries, 999);
            Console.WriteLine();

            // 3) Try to update with invalid (negative) quantity
            Console.WriteLine("Attempting to set invalid negative quantity for electronic item (ID = 2)");
            try
            {
                manager._electronics.UpdateQuantity(2, -5); // direct call to repo to show exception
            }
            catch (InvalidQuantityException iq)
            {
                Console.WriteLine($"Caught expected invalid quantity error: {iq.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            Console.WriteLine();

            // 4) Demonstrate IncreaseStock and successful Remove
            Console.WriteLine("Increasing stock of Grocery ID 101 by 20");
            manager.IncreaseStock(manager._groceries, 101, 20);
            Console.WriteLine();

            Console.WriteLine("Removing Electronic item ID 3");
            manager.RemoveItemById(manager._electronics, 3);
            Console.WriteLine();

            Console.WriteLine("Final lists after operations:");
            Console.WriteLine("Grocery Items:");
            manager.PrintAllItems(manager._groceries);
            Console.WriteLine();
            Console.WriteLine("Electronic Items:");
            manager.PrintAllItems(manager._electronics);
            Console.WriteLine();

            Console.WriteLine("Program finished. Press any key to exit");
            Console.ReadKey();
        }
    }
}
