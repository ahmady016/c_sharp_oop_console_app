using System.Text.Json.Serialization;

namespace HotelManagement;

public record RankingWeights
(
    int NightWeight,
    int SpentDivider
);

public record HotelRoom
(
    int RoomNumber,
    double PricePerNight,
    HotelRoomType Type = HotelRoomType.Single,
    bool IsEmpty = true
);

public record HotelGuest
(
    [property: JsonConverter(typeof(GuestIdJsonConverter))]
    [property: JsonPropertyName("guest_id")]
    Guest Guest,
    int TotalNights,
    double TotalSpent,
    int Score,
    HotelTier Tier
);

public record HotelBooking
(
    string Id,
    [property: JsonConverter(typeof(GuestIdJsonConverter))]
    [property: JsonPropertyName("guest_id")]
    Guest Guest,
    int RoomNumber,
    DateTime CheckInDate,
    double Spent,
    int Nights = 1,
    DateTime? CheckOutDate = null,
    CheckedStatus Status = CheckedStatus.CheckedIn
);

public record HotelGuestHistory
(
    [property: JsonConverter(typeof(GuestIdJsonConverter))]
    [property: JsonPropertyName("guest_id")]
    Guest Guest,
    int TotalNights,
    double TotalSpent,
    int Score,
    HotelTier Tier,
    CheckedStatus CurrentStatus,
    int TotalBookings,
    IEnumerable<HotelBooking> Bookings
)
{
    public override string ToString() =>
        $"{Guest.FullName} is a {Guest.Gender} guest, stayed for ({TotalNights}) nights, spent $({TotalSpent:F2}), scored ({Score}), belongs to the [{Tier}] tier, has ({TotalBookings}) bookings and Current status: {CurrentStatus}.";
}
