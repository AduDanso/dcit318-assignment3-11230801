using System;
using System.Collections.Generic;
using System.IO;

// Immutable record for Inventory Item
public record InventoryItem(string ProductCode, string ProductName, int Quantity, decimal Price);

// Generic Inventory Logger
public class InventoryLogger<T>
{
    private readonly string _filePath;

    public InventoryLogger(string filePath)
    {
        _filePath = filePath;
    }

    // Save items to file
    public void Save(List<T> items)
    {
        using var writer = new StreamWriter(_filePath);
        foreach (var item in items)
        {
            writer.WriteLine(item);
        }
    }

    // Load items from file (as strings)
    public List<string> Load()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"File '{_filePath}' not found.");

        return new List<string>(File.ReadAllLines(_filePath));
    }
}

class Program
{
    static void Main()
    {
        string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.FullName;
        string csvFilePath = Path.Combine(basePath, "products.csv");
        string logFilePath = Path.Combine(basePath, "inventory_log.txt");

        var inventory = new List<InventoryItem>();

        try
        {
            // Load inventory from CSV
            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"Products file not found: {csvFilePath}");

            foreach (var line in File.ReadAllLines(csvFilePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 4)
                {
                    Console.WriteLine($"Skipping invalid line: {line}");
                    continue;
                }

                string code = parts[0].Trim();
                string name = parts[1].Trim();
                if (!int.TryParse(parts[2].Trim(), out int qty) ||
                    !decimal.TryParse(parts[3].Trim(), out decimal price))
                {
                    Console.WriteLine($"Skipping invalid data: {line}");
                    continue;
                }

                inventory.Add(new InventoryItem(code, name, qty, price));
            }

            // Log inventory
            var logger = new InventoryLogger<InventoryItem>(logFilePath);
            logger.Save(inventory);

            // Search feature
            Console.Write("Enter product code to search: ");
            string searchCode = Console.ReadLine()?.Trim() ?? "";

            var found = inventory.Find(item => item.ProductCode.Equals(searchCode, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                Console.WriteLine($"Found: {found.ProductName} - Qty: {found.Quantity}, Price: GHC{found.Price}");
            }
            else
            {
                Console.WriteLine("Product not found.");
            }

            Console.WriteLine($"\nInventory log saved to: {logFilePath}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
