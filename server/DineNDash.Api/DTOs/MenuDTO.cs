namespace DineNDash.Api.DTOs;

/// <summary>
/// Name for a new or renamed menu category
/// </summary>
public class MenuCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Details for a new or edited menu item, including which category it sits in
/// </summary>
public class MenuItemRequest
{
    public int MenuCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Menu item as shown to the client, with a link to its image when it has one
/// </summary>
public class MenuItemResponse
{
    public int Id { get; set; }
    public int MenuCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Menu category with its items
/// </summary>
public class MenuCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<MenuItemResponse> Items { get; set; } = [];
}
