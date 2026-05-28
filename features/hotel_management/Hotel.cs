using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace HotelManagement;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HotelType : byte
{
    [JsonStringEnumMemberName("resort")]
    Resort = 1,
    [JsonStringEnumMemberName("hotel")]
    Hotel = 2,
    [JsonStringEnumMemberName("boutique")]
    Boutique = 3,
    [JsonStringEnumMemberName("economy")]
    Economy = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HotelTier : byte
{
    [JsonStringEnumMemberName("none")]
    None = 0,
    [JsonStringEnumMemberName("bronze")]
    Bronze = 1,
    [JsonStringEnumMemberName("silver")]
    Silver = 2,
    [JsonStringEnumMemberName("gold")]
    Gold = 3,
    [JsonStringEnumMemberName("platinum")]
    Platinum = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HotelRoomType : byte
{
    [JsonStringEnumMemberName("single")]
    Single = 1,
    [JsonStringEnumMemberName("double")]
    Double = 2,
    [JsonStringEnumMemberName("suite")]
    Suite = 3,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CheckedStatus : byte
{
    [JsonStringEnumMemberName("none")]
    None = 0,
    [JsonStringEnumMemberName("checkedin")]
    CheckedIn = 1,
    [JsonStringEnumMemberName("checkedout")]
    CheckedOut = 2,
}

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
    Guest Guest,
    int TotalNights,
    double TotalSpent,
    int Score,
    HotelTier Tier
);
public record HotelBooking
(
    string Id,
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

public class Hotel : ICloneable
{
    #region Static Fields used for calculate guest score and tier
    // static readonly dictionary to hold the ranking weights for each hotel type
    // these weights are used to calculate the score and tier for each guest
    // based on the number of nights stayed and the amount spent at the hotel
    private static readonly Dictionary<string, RankingWeights> _rankingWeights = new()
    {
        ["Resort"] = new RankingWeights(NightWeight: 5, SpentDivider: 5),
        ["Hotel"] = new RankingWeights(NightWeight: 25, SpentDivider: 50),
        ["Boutique"] = new RankingWeights(NightWeight: 15, SpentDivider: 15),
        ["Economy"] = new RankingWeights(NightWeight: 50, SpentDivider: 100)
    };
    // static readonly dictionary to hold the minimum room cost per night for each hotel type
    public static readonly Dictionary<string, double> MinRoomCostPerNight = new()
    {
        ["Resort"] = 250,
        ["Hotel"] = 125,
        ["Boutique"] = 75,
        ["Economy"] = 50
    };
    public static readonly Dictionary<string, double> MaxRoomCostPerNight = new()
    {
        ["Resort"] = 500,
        ["Hotel"] = 250,
        ["Boutique"] = 150,
        ["Economy"] = 100
    };
    #endregion

    #region private state Fields
    // private readonly fields, only settable in the constructor
    private readonly string _id;
    private readonly string _name;
    private readonly string _location;
    private readonly int _capacity;
    private readonly HotelType _type;
    private readonly Dictionary<int, HotelRoom> _rooms = [];
    private readonly Dictionary<string, HotelBooking> _bookings = [];
    private readonly Dictionary<string, HotelGuest> _guests = [];
    #endregion

    #region public state Properties
    // public readonly (getter only) properties
    public string Id => _id;
    public string Name => _name;
    public string Location => _location;
    public int Capacity => _capacity;
    public HotelType Type => _type;
    public IReadOnlyList<HotelRoom> Rooms => [.._rooms.Values];
    #endregion

    #region public computed getter only Properties
    // public aggregations properties to
    // get all hotel bookings ordered by check-in date
    public IReadOnlyList<HotelBooking> Bookings => [..
        from booking in _bookings.Values
        orderby booking.CheckInDate descending
        select booking
    ];
    // get all hotel guests ordered by score
    public IReadOnlyList<HotelGuest> Guests => [..
        from guest in _guests.Values
        orderby guest.Score descending
        select guest
    ];
    // get the all the hotel guests history ordered by score
    public IReadOnlyList<HotelGuestHistory> GuestsHistory => [..
        from guest in Guests
        let guestBookings = from booking in Bookings
                            where booking.Guest.Id == guest.Guest.Id
                            orderby booking.CheckInDate descending
                            select booking
        orderby guest.Score descending
        select new HotelGuestHistory(
            Guest: guest.Guest,
            TotalNights: guest.TotalNights,
            TotalSpent: guest.TotalSpent,
            Score: guest.Score,
            Tier: guest.Tier,
            CurrentStatus: guestBookings.FirstOrDefault()?.Status ?? CheckedStatus.None,
            TotalBookings: guestBookings.Count(),
            Bookings: guestBookings
        )
    ];
    // get only the guests who are checked-out at the hotel
    public IReadOnlyList<Guest> PastGuests => [..
        from guestHistory in GuestsHistory
        where guestHistory.CurrentStatus == CheckedStatus.CheckedOut
        orderby guestHistory.Score descending
        select guestHistory.Guest
    ];
    // get only the guests who are still checked-in at the hotel
    public IReadOnlyList<Guest> CurrentGuests => [..
        from guestHistory in GuestsHistory
        where guestHistory.CurrentStatus == CheckedStatus.CheckedIn
        orderby guestHistory.Score descending
        select guestHistory.Guest
    ];
    // get the guests counter by each tier in the hotel
    public FrozenDictionary<string, int> TierCounter => Guests.Aggregate(
        (
            from tier in Enum.GetValues<HotelTier>()
            where tier != HotelTier.None
            select KeyValuePair.Create(tier.ToString(), 0)
        ).ToDictionary(kv => kv.Key, kv => kv.Value),
        (dict, guest) => {
            dict[guest.Tier.ToString()]++;
            return dict;
        },
        dict => dict.ToFrozenDictionary()
    );
    // get the unique guests who have stayed at the hotel ordered by latest check-in date
    public HashSet<Guest> GuestsSet => [..
        from booking in Bookings
        orderby booking.CheckInDate descending
        select booking.Guest
    ];
    // get the expired bookings in hotel along with its expiration dates string
    public KeyValuePair<string, HotelBooking>[] ExpiredBookings => [..
        from booking in Bookings
        let expiredDate = booking.CheckInDate.AddDays(booking.Nights)
        where DateTime.Now > expiredDate && booking.Status == CheckedStatus.CheckedIn
        select KeyValuePair.Create(expiredDate.ToString("yyyy-MM-dd"), booking)
    ];
    // get the current checked-in bookings along with its checked-in dates
    public KeyValuePair<string, HotelBooking>[] CurrentBookings => [..
        from booking in Bookings
        let expiredDate = booking.CheckInDate.AddDays(booking.Nights)
        where DateTime.Now <= expiredDate && booking.Status == CheckedStatus.CheckedIn
        select KeyValuePair.Create(booking.CheckInDate.ToString("yyyy-MM-dd"), booking)
    ];
    // get the completed bookings along with its checked-in dates
    public KeyValuePair<string, HotelBooking>[] CompletedBookings => [..
        from booking in Bookings
        where booking.Status == CheckedStatus.CheckedOut
        select KeyValuePair.Create(booking.CheckInDate.ToString("yyyy-MM-dd"), booking)
    ];
    // calculate the total number of available rooms and occupied rooms
    public IReadOnlyList<HotelRoom> AvailableRooms =>
        [..from room in Rooms where room.IsEmpty select room];
    public IReadOnlyList<HotelRoom> OccupiedRooms =>
        [..from room in Rooms where !room.IsEmpty select room];
    // calculate the total number of nights stayed by all guests at the hotel
    public int TotalNights => Guests.Sum(b => b.TotalNights);
    // calculate the total amount spent by all guests at the hotel
    public double TotalSpent => Math.Round(Guests.Sum(b => b.TotalSpent), 2);
    // calculate the average number of nights stayed per guest booking at the hotel
    public double AverageNights => Guests.Count > 0
        ? Math.Round((double)TotalNights / Guests.Count, 2)
        : 0.0;
    // calculate the average amount spent per guest booking at the hotel
    public double AverageSpent => Guests.Count > 0
        ? Math.Round(TotalSpent / Guests.Count, 2)
        : 0.0;
    // calculate the average guests score in the hotel
    public double AverageScore => Guests.Count > 0
        ? Math.Round(Guests.Average(g => g.Score), 2)
        : 0.0;
    // calculate the average guests age in the hotel
    public double AverageAge => Guests.Count > 0
        ? Math.Round(Guests.Average(g => g.Guest.Age), 2)
        : 0.0;
    #endregion

    #region public constructors
    // constructor to initialize the hotel object with required properties and validations
    public Hotel(
        string name,
        string location,
        HotelType type = HotelType.Economy,
        int capacity = 50
    )
    {
        name = name.Trim();
        location = location.Trim();

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("must provide a name for the hotel.");
        if (string.IsNullOrEmpty(location))
            throw new ArgumentException("must provide a location for the hotel.");
        if (capacity < 10 || capacity > 1000)
            throw new ArgumentException("capacity must be between 10 and 1000.");

        _id = Helpers.GenerateId(10);
        _name = name;
        _location = location;
        _type = type;
        _capacity = capacity;
    }
    public Hotel(
        string name,
        string location,
        List<HotelRoom> rooms,
        HotelType type = HotelType.Economy,
        int capacity = 50
    ) : this(name, location, type, capacity)
    {
        ArgumentNullException.ThrowIfNull(rooms);
        if (rooms.Count < 5 || rooms.Count > 1000)
            throw new ArgumentException("number of rooms cannot exceed the hotel capacity.");

        foreach (var room in rooms)
        {
            if (_rooms.ContainsKey(room.RoomNumber))
                throw new ArgumentException($"duplicate room found: {room.RoomNumber}");
            _rooms[room.RoomNumber] = room;
        }
    }
    #endregion

    #region override methods for string representation, hash code and equality
    public override string ToString() =>
        $"{Name} is a [{Type}] hotel located in ({Location}) can accommodate ({Capacity}) persons in ({Rooms.Count}) rooms.";
    public override int GetHashCode() =>
        HashCode.Combine(Id, Name, Location, Type, Capacity);
    public override bool Equals(object? obj) =>
        obj is Hotel otherHotel &&
        Id == otherHotel.Id && Name == otherHotel.Name &&
        Location == otherHotel.Location && Type == otherHotel.Type &&
        Capacity == otherHotel.Capacity;
    #endregion

    #region operator overloads for equality and comparison based on hotel capacity
    public static bool operator ==(Hotel left, Hotel right) => left.Equals(right);
    public static bool operator !=(Hotel left, Hotel right) => !left.Equals(right);
    public static bool operator >(Hotel left, Hotel right) => left.Capacity > right.Capacity;
    public static bool operator <(Hotel left, Hotel right) => left.Capacity < right.Capacity;
    public static bool operator >=(Hotel left, Hotel right) => left.Capacity >= right.Capacity;
    public static bool operator <=(Hotel left, Hotel right) => left.Capacity <= right.Capacity;
    #endregion

    #region implementation of ICloneable
    public object Clone() => new Hotel(
        name: Name, location: Location, type: Type, capacity: Capacity, rooms: [..Rooms]
    );
    #endregion

    #region public indexers to get and set hotel rooms
    public HotelRoom this[int RoomNumber]
    {
        get
        {
            if (RoomNumber < 1 || RoomNumber > 9999)
                throw new ArgumentOutOfRangeException(nameof(RoomNumber), $"room number must be between 1 and 9999.");
            return Rooms[RoomNumber];
        }
        set
        {
            if (RoomNumber < 1 || RoomNumber > 9999)
                throw new ArgumentOutOfRangeException(nameof(RoomNumber), $"room number must be between 1 and 9999.");
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            _rooms[value.RoomNumber] = value;
        }
    }
    #endregion

    #region public operations for check-in, check-out, update booking and change room
    // and calculate guest score and tier based on their history at the hotel
    // method to calculate the guest score
    public int CalculateGuestScore(int nights, double spent)
    {
        if (!_rankingWeights.TryGetValue(Type.ToString(), out RankingWeights? weights))
            throw new KeyNotFoundException($"ranking weights for hotel type ({Type}) not found.");

        return (nights > 0 && spent > 0.0)
            ? (nights * weights.NightWeight) + (int)(spent / weights.SpentDivider)
            : 0;
    }
    // method to determine the guest tier based on the calculated score
    public static HotelTier CalculateGuestTier(int score) => score switch
    {
        >= 500 => HotelTier.Platinum,
        >= 250 => HotelTier.Gold,
        >= 100 => HotelTier.Silver,
        _ => HotelTier.Bronze
    };

    // helper method to calculate existed guest score and tier based on their bookings at once
    // and update the guest information in the hotel system
    public void CalculateGuestScoreAndTier(string guestId)
    {
        guestId = guestId.Trim();
        if(string.IsNullOrEmpty(guestId))
            throw new ArgumentException("guest id cannot be null or empty.", nameof(guestId));

        if (!_guests.TryGetValue(guestId, out HotelGuest? existingGuest))
            throw new KeyNotFoundException($"guest with id ({guestId}) not found.");

        var guestBookings = from booking in Bookings
                            where booking.Guest.Id == guestId
                            select booking;

        (int Nights, double Spent) = guestBookings.Any()
            ? (guestBookings.Sum(b => b.Nights), guestBookings.Sum(b => b.Spent))
            : (0, 0.0);
        int score = CalculateGuestScore(Nights, Spent);
        HotelTier tier = CalculateGuestTier(score);
        _guests[guestId] = existingGuest with
        {
            TotalNights = Nights,
            TotalSpent = Spent,
            Score = score,
            Tier = tier
        };
    }

    // method to add the guest to the hotel system for the first time they check-in
    // or update their existing information if they have stayed before
    // and gradually update the guest score and tier when check-in or update booking
    // and update the guest information in the hotel system
    public void UpdateGuestScoreAndTier(HotelBooking booking)
    {
        ArgumentNullException.ThrowIfNull(booking, nameof(booking));
        ArgumentNullException.ThrowIfNull(booking.Guest, nameof(booking.Guest));

        int score, totalNights;
        double totalSpent;
        HotelTier tier;
        if(!_guests.TryGetValue(booking.Guest.Id, out HotelGuest? existingGuest))
        {
            score = CalculateGuestScore(booking.Nights, booking.Spent);
            tier = CalculateGuestTier(score);
            var newGuest = new HotelGuest(
                Guest: booking.Guest,
                TotalNights: booking.Nights,
                TotalSpent: booking.Spent,
                Score: score,
                Tier: tier
            );
            _guests[booking.Guest.Id] = newGuest;
        }
        else
        {
            totalNights = existingGuest.TotalNights + booking.Nights;
            totalSpent = existingGuest.TotalSpent + booking.Spent;
            score = CalculateGuestScore(totalNights, totalSpent);
            tier = CalculateGuestTier(score);
            _guests[booking.Guest.Id] = existingGuest with
            {
                TotalNights = totalNights,
                TotalSpent = totalSpent,
                Score = score,
                Tier = tier
            };
        }
    }

    // method to check-in a guest to a room with validations for
    // room availability, check-in date, amount spent and booking conflicts
    public string CheckInGuest(
        Guest guest,
        int roomNumber,
        string? checkedInDateString = null,
        double? spent = null,
        int nights = 1
    )
    {
        ArgumentNullException.ThrowIfNull(guest, nameof(guest));

        if (nights < 1 || nights > 30)
            throw new ArgumentOutOfRangeException(nameof(nights), $"number of nights must be between 1 and 30.");

        DateTime checkedInDate = DateTime.Now;
        if(checkedInDateString is not null && !DateTime.TryParse(checkedInDateString, out checkedInDate))
            throw new ArgumentException("must provide a valid check-in date.", nameof(checkedInDateString));

        if (roomNumber < 1 || roomNumber > 9999)
            throw new ArgumentOutOfRangeException(nameof(roomNumber), $"room number must be between 1 and 9999.");
        if (!_rooms.TryGetValue(roomNumber, out HotelRoom? room))
            throw new KeyNotFoundException($"room with number ({roomNumber}) not found.");
        if (!room.IsEmpty)
            throw new InvalidOperationException($"room with number ({roomNumber}) is not empty.");

        double minSpent = room.PricePerNight * nights;
        if (spent is not null && spent < minSpent)
            throw new ArgumentException($"spent must be at least ({minSpent}).", nameof(spent));

        var newBooking = new HotelBooking(
            Id: Helpers.GenerateId(10),
            Guest: guest,
            RoomNumber: room.RoomNumber,
            Nights: nights,
            Spent: spent ?? minSpent,
            CheckInDate: checkedInDate
        );
        _rooms[room.RoomNumber] = room with { IsEmpty = false };
        _bookings[newBooking.Id] = newBooking;
        UpdateGuestScoreAndTier(newBooking);
        return newBooking.Id;
    }

    // method to update guest to add more nights and spent amount for an existing booking
    // with validations for booking existence, number of nights and amount spent
    public void UpdateGuestBooking(
        HotelBooking existedBooking,
        int nights,
        double spent
    )
    {
        ArgumentNullException.ThrowIfNull(existedBooking, nameof(existedBooking));
        if (!_bookings.ContainsKey(existedBooking.Id))
            throw new KeyNotFoundException($"booking with id ({existedBooking.Id}) not found.");

        if (nights < 1 || nights > 30)
            throw new ArgumentOutOfRangeException(nameof(nights), $"number of nights must be between 1 and 30.");
        if (spent < 0)
            throw new ArgumentException($"spent must be at least ({spent}).", nameof(spent));

        foreach(var (date, booking) in ExpiredBookings)
            if(existedBooking.Id == booking.Id)
                throw new InvalidOperationException($"Can't update ({existedBooking.Id}) an expired booking, please try check-in instead!");

        var updatedBooking = existedBooking with
        {
            Nights = existedBooking.Nights + nights,
            Spent = existedBooking.Spent + spent
        };
        _bookings[existedBooking.Id] = updatedBooking;
        UpdateGuestScoreAndTier(updatedBooking);
    }

    // method to change the guest room with validations for
    // booking existence, room availability and check-in date
    public void ChangeGuestRoom(
        HotelBooking existedBooking,
        int newRoomNumber
    )
    {
        ArgumentNullException.ThrowIfNull(existedBooking, nameof(existedBooking));
        if (!_bookings.ContainsKey(existedBooking.Id))
            throw new KeyNotFoundException($"booking with id ({existedBooking.Id}) not found.");

        if (newRoomNumber < 1 || newRoomNumber > 9999)
            throw new ArgumentOutOfRangeException(nameof(newRoomNumber), $"room number must be between 1 and 9999.");
        if (!_rooms.TryGetValue(newRoomNumber, out HotelRoom? newRoom))
            throw new KeyNotFoundException($"room with number ({newRoomNumber}) not found.");
        if (!newRoom.IsEmpty)
            throw new InvalidOperationException($"room with number ({newRoomNumber}) is not empty.");

        var oldRoom = _rooms[existedBooking.RoomNumber];
        _rooms[oldRoom.RoomNumber] = oldRoom with { IsEmpty = true };
        _rooms[newRoom.RoomNumber] = newRoom with { IsEmpty = false };

        _bookings[existedBooking.Id] = existedBooking with { RoomNumber = newRoom.RoomNumber };
    }

    // method to check-out a guest from the hotel
    // with validations for booking existence and check-out date
    public void CheckOutGuest(
        HotelBooking existedBooking,
        string checkOutDateString
    )
    {
        ArgumentNullException.ThrowIfNull(existedBooking, nameof(existedBooking));
        if (!_bookings.ContainsKey(existedBooking.Id))
            throw new KeyNotFoundException($"booking with id ({existedBooking.Id}) not found.");

        checkOutDateString = checkOutDateString.Trim();
        if (string.IsNullOrEmpty(checkOutDateString))
            throw new ArgumentException("must provide a valid check-out date.", nameof(checkOutDateString));
        if (!DateTime.TryParse(checkOutDateString, out DateTime checkOutDate))
            throw new ArgumentException("invalid check-out date format.", nameof(checkOutDateString));
        if (checkOutDate < existedBooking.CheckInDate)
            throw new ArgumentException("check-out date cannot be before check-in date.", nameof(checkOutDate));

        var room = _rooms[existedBooking.RoomNumber];
        _rooms[room.RoomNumber] = room with { IsEmpty = true };

        _bookings[existedBooking.Id] = existedBooking with
        {
            CheckOutDate = checkOutDate,
            Status = CheckedStatus.CheckedOut
        };
    }
    #endregion

    #region Helper Methods for Searching and Filtering Guests
    public IEnumerable<HotelGuestHistory> GetGuestsByName(string name) =>
        from guest in GuestsHistory
        where guest.Guest.FullName.Contains(name, StringComparison.OrdinalIgnoreCase)
        select guest;

    public IEnumerable<HotelGuestHistory> GetGuestsByTier(string tier) =>
        from guest in GuestsHistory
        where guest.Tier == Enum.Parse<HotelTier>(tier, ignoreCase: true)
        select guest;

    public IEnumerable<HotelGuestHistory> GetGuestsByScoreRange(int minScore, int maxScore) =>
        from guest in GuestsHistory
        where guest.Score >= minScore && guest.Score <= maxScore
        select guest;

    public IEnumerable<HotelGuestHistory> GetGuestsBySpentRange(double minSpent, double maxSpent) =>
        from guest in GuestsHistory
        where guest.TotalSpent >= minSpent && guest.TotalSpent <= maxSpent
        select guest;

    public IEnumerable<HotelGuestHistory> GetGuestsByNightsRange(int minNights, int maxNights) =>
        from guest in GuestsHistory
        where guest.TotalNights >= minNights && guest.TotalNights <= maxNights
        select guest;

    public IEnumerable<HotelGuestHistory> GetTopScoredGuests(int limit = 5) => (
        from guest in GuestsHistory
        orderby guest.Score descending
        select guest
    ).Take(limit);

    public IEnumerable<HotelGuestHistory> GetTopSpentGuests(int limit = 5) => (
        from guest in GuestsHistory
        orderby guest.TotalSpent descending
        select guest
    ).Take(limit);

    public IEnumerable<HotelGuestHistory> GetTopStayedGuests(int limit = 5) => (
        from guest in GuestsHistory
        orderby guest.TotalNights descending
        select guest
    ).Take(limit);

    public HotelGuest? GetCurrentGuestInRoom(int roomNumber) => (
        from booking in Bookings
        where booking.RoomNumber == roomNumber
        let guest = booking.Guest
        from hotelGuest in Guests
        where guest.Id == hotelGuest.Guest.Id
        select hotelGuest
    ).FirstOrDefault();
    #endregion

}
