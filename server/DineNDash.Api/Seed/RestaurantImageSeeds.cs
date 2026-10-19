namespace DineNDash.Api.Seed;

/// <summary>
/// Which photo in Seed/Images belongs to which restaurant, matched on the restaurant's name in the database
/// </summary>
public static class RestaurantImageSeeds
{
    public static readonly IReadOnlyDictionary<string, string> FileToRestaurantName = new Dictionary<string, string>
    {
        ["bar-tottis.jpg"] = "Bar Totti's",
        ["basax.jpg"] = "Basax",
        ["belles-hot-chicken.jpg"] = "Belles Hot Chicken Circular Quay",
        ["burger-point.jpg"] = "Burger Point Circular Quay",
        ["chat-thai.jpg"] = "Chat Thai",
        ["chicken-V.jpg"] = "Chicken V",
        ["fratelli-paradiso.jpg"] = "Fratelli Paradiso",
        ["gumshara.jpg"] = "Gumshara",
        ["ken-chan-curry.jpg"] = "Ken-Chan Curry",
        ["mappen.jpg"] = "Mappen",
        ["marys.jpg"] = "Mary's",
        ["my-zakaya.jpg"] = "My Zakaya",
        ["ni-hao-bar.jpg"] = "Ni Hao Bar & Dining",
        ["nour.jpg"] = "Nour",
        ["pepper-lunch.jpg"] = "Pepper Lunch",
        ["pho-pasteur.jpg"] = "Pho Pasteur",
        ["ramen-ippudo.jpg"] = "Ramen IPPUDO Westfield Sydney",
        ["yappari-steak.jpg"] = "Yappari Steak Regent Place",
        ["yayoi-galeries.jpg"] = "YAYOI Galeries",
        ["yunn.jpg"] = "Yunn"
    };
}
