namespace InventoryAPI.Models;

public class Item
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ItemGroupId { get; set; }
    public ItemGroup ItemGroup { get; set; } = null!;
    public int UnitOfMeasureId { get; set; } 
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
}