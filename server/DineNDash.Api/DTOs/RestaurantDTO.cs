using DineNDash.Core.Enums;

namespace DineNDash.Api.DTOs;

/// <summary>
/// Restaurant as shown to the client when browsing or searching
/// </summary>
public class RestaurantResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CuisineType Cuisine { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public int PriceLevel { get; set; }
    public string? ProfileImageUrl { get; set; }
}

/// <summary>
/// The owner's view of their restaurant, including details diners do not see
/// </summary>
public class RestaurantProfileResponse : RestaurantResponse
{
    public int Capacity { get; set; }
}

/// <summary>
/// Details an owner sets when creating or editing their restaurant
/// </summary>
public class RestaurantProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CuisineType Cuisine { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public int PriceLevel { get; set; } = 2;
    public int Capacity { get; set; } = 20;
}

/// <summary>
/// One page of results with the totals needed for page controls
/// </summary>
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
