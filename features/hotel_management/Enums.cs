namespace HotelManagement;

public enum HotelType : byte
{
    Resort = 1,
    Hotel = 2,
    Boutique = 3,
    Economy = 4
}

public enum HotelRoomType : byte
{
    Single = 1,
    Double = 2,
    Suite = 3,
}

public enum HotelTier : byte
{
    None = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4
}

public enum CheckedStatus : byte
{
    None = 0,
    CheckedIn = 1,
    CheckedOut = 2,
}
