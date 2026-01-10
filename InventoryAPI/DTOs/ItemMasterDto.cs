namespace InventoryAPI.DTOs;
public class ItemMasterDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ItemGroupId { get; set; }
    public int UnitOfMeasureId { get; set; }
}