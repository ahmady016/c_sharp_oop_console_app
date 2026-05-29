// # ------------------------------------------------------------------------
// # add a hotel with name, location, capacity, rooms and type
// # the hotel types are [resort, hotel, boutique, economy]
// # add a new guest with first name, last name, phone number, birth year, gender
// # and calculate the guest's rank based on volume(total_nights) and value (total_spent)
// # and there is a ranking formula for each hotel type [resort, hotel, boutique, economy]
// # as follows:
// # resort: rank = total_nights * 5 + total_spent / 5
// # hotel: rank = total_nights * 25 + total_spent / 50
// # boutique: rank = total_nights * 15 + total_spent / 15
// # economy: rank = total_nights * 50 + total_spent / 100
// # and based on the guest's rank (Weight) he assigned a guest tier
// # the guest tiers are one of [bronze, silver, gold, platinum] as follows:
// # bronze: if the rank is less than 100
// # silver: if the rank is between 100 and 250
// # gold: if the rank is between 250 and 500
// # platinum: if the rank is more than 500
// # the system should add the guest to the guest list in each hotel
// # and calculate the the guest rank and tier
// # then finally sort the guests in each hotel by their rank
// # and the admin can view the guest list in each hotel with guest details
// # guest details are [name, phone number, rank, tier]
// # and the admin can view the guest list in each hotel by tier
// -----------------------------------------------------------------------------
using Bogus;

namespace HotelManagement;

// # build a Hotel Management System using oop approach that allows admin to:
public static class HotelManager
{
    private static readonly Faker _faker = new("en");
    private static readonly List<int> _roomNumbersRange = [.. Enumerable.Range(101, 1000)];
    private static readonly ValueTuple<string, string>[] _checkInDateRanges = [
        ("2024-01-01", "2024-05-31"),
        ("2024-07-01", "2024-12-31"),
        ("2025-01-01", "2025-05-31"),
        ("2025-07-01", "2025-12-31"),
        ("2026-01-01", "2026-04-30")
    ];
    private const string GUESTS_FILE_NAME = "guests.json";
    private const string RESORT_HOTEL_FILE_NAME = "paradise_resort.json";
    private const string CITY_HOTEL_FILE_NAME = "city_hotel.json";
    private const string BOUTIQUE_HOTEL_FILE_NAME = "boutique_hotel.json";
    private const string ECONOMY_HOTEL_FILE_NAME = "economy_hotel.json";
    private static readonly string DATA_DIRECTORY = Path.Combine(Helpers.SameDirectory(), "data");
    private static readonly string[] _filesPaths = [
        Path.Combine(DATA_DIRECTORY, GUESTS_FILE_NAME),
        Path.Combine(DATA_DIRECTORY, RESORT_HOTEL_FILE_NAME),
        Path.Combine(DATA_DIRECTORY, CITY_HOTEL_FILE_NAME),
        Path.Combine(DATA_DIRECTORY, BOUTIQUE_HOTEL_FILE_NAME),
        Path.Combine(DATA_DIRECTORY, ECONOMY_HOTEL_FILE_NAME)
    ];

    private static List<Guest> _guests = [];
    private static Hotel _resortHotel = default!;
    private static Hotel _cityHotel = default!;
    private static Hotel _boutiqueHotel = default!;
    private static Hotel _economyHotel = default!;
    private static IReadOnlyList<Hotel> _hotels = [];
    private static List<int> _roomNumbers = [];
    private static List<HotelRoom> _rooms = [];

    private static HotelTier GetRandomTier()
    {
        var randomTier = _faker.PickRandom<HotelTier>();
        while(randomTier == HotelTier.None)
            randomTier = _faker.PickRandom<HotelTier>();
        return randomTier;
    }
    private static Hotel GetResortHotel()
    {
        _roomNumbers = [..Helpers.PickSet(_roomNumbersRange, 200)];
        _rooms = [..
            from i in _roomNumbers
            select new HotelRoom(
                RoomNumber: i,
                PricePerNight: Math.Round(_faker.Random.Double(
                    Hotel.MinRoomCostPerNight[HotelType.Resort.ToString()],
                    Hotel.MaxRoomCostPerNight[HotelType.Resort.ToString()]
                ), 2),
                Type: _faker.PickRandom<HotelRoomType>(),
                IsEmpty: true
            )
        ];
        return new(
            name: "Paradise Resort",
            location: "Hawaii",
            type: HotelType.Resort,
            capacity: Convert.ToInt32(_rooms.Count * 1.75),
            rooms: _rooms
        );
    }
    private static Hotel GetCityHotel()
    {
        _roomNumbers = [..Helpers.PickSet(_roomNumbersRange, 100)];
        _rooms = [..
            from i in _roomNumbers
            select new HotelRoom(
                RoomNumber: i,
                PricePerNight: _faker.Random.Double(
                    Hotel.MinRoomCostPerNight[HotelType.Hotel.ToString()],
                    Hotel.MaxRoomCostPerNight[HotelType.Hotel.ToString()]
                ),
                Type: _faker.PickRandom<HotelRoomType>(),
                IsEmpty: true
            )
        ];
        return new(
            name: "City Hotel",
            location: "New York",
            type: HotelType.Hotel,
            capacity: Convert.ToInt32(_rooms.Count * 1.45),
            rooms: _rooms
        );
    }
    private static Hotel GetBoutiqueHotel()
    {
        _roomNumbers = [..Helpers.PickSet(_roomNumbersRange, 50)];
        _rooms = [..
            from i in _roomNumbers
            select new HotelRoom(
                RoomNumber: i,
                PricePerNight: _faker.Random.Double(
                    Hotel.MinRoomCostPerNight[HotelType.Boutique.ToString()],
                    Hotel.MaxRoomCostPerNight[HotelType.Boutique.ToString()]
                ),
                Type: _faker.PickRandom<HotelRoomType>(),
                IsEmpty: true
            )
        ];
        return new(
            name: "Boutique Hotel",
            location: "Paris",
            type: HotelType.Boutique,
            capacity: Convert.ToInt32(_rooms.Count * 1.25),
            rooms: _rooms
        );
    }
    private static Hotel GetEconomyHotel()
    {
        _roomNumbers = [..Helpers.PickSet(_roomNumbersRange, 25)];
        _rooms = [..
            from i in _roomNumbers
            select new HotelRoom(
                RoomNumber: i,
                PricePerNight: _faker.Random.Double(
                    Hotel.MinRoomCostPerNight[HotelType.Economy.ToString()],
                    Hotel.MaxRoomCostPerNight[HotelType.Economy.ToString()]
                ),
                Type: _faker.PickRandom<HotelRoomType>(),
                IsEmpty: true
            )
        ];
        return new(
            name: "Economy Hotel",
            location: "London",
            type: HotelType.Economy,
            capacity: Convert.ToInt32(_rooms.Count * 1.25),
            rooms: _rooms
        );
    }
    private static List<Guest> GetGuests(int count = 500) => [..
        from i in Enumerable.Range(1, count + 1)
        let gender = _faker.Person.Gender
        select new Guest(
            firstName: _faker.Name.FirstName(gender),
            lastName: _faker.Name.LastName(),
            birthDate: _faker.Date.Past(50, DateTime.Now.AddYears(-18)).ToString("yyyy-MM-dd"),
            mobileNumber: _faker.Phone.PhoneNumber("01#########"),
            gender: gender.ToString().ToLower()
        )
    ];

    // method to check-in a given count of randomly picked guests to the given hotel
    private static void CheckInGuests(
        Hotel hotel,
        ValueTuple<string, string> checkInDateRange,
        int numberOfGuests = 100
    )
    {
        foreach (var guest in Helpers.PickSet(_guests, numberOfGuests))
        {
            // find an empty room
            HotelRoom _room = hotel.AvailableRooms.Any()
                ? Helpers.PickOne(hotel.AvailableRooms)
                : throw new InvalidOperationException($"No empty rooms found in the {hotel.Name} right now.");
            // get a random number of nights
            int randomNights = _faker.Random.Int(1, 30);
            // calculate the total spent range
            double minSpent = _room.PricePerNight * randomNights;
            double maxSpent = minSpent + _faker.Random.Double(100, 1000);
            // get a random spent based on the calculated range
            double randomSpent = _faker.Random.Double(minSpent, maxSpent);
            string checkInDate = _faker.Date.BetweenDateOnly(
                DateOnly.Parse(checkInDateRange.Item1),
                DateOnly.Parse(checkInDateRange.Item2)
            ).ToString("yyyy-MM-dd");
            // check-in the guest
            hotel.CheckInGuest(
                guest: guest,
                roomNumber: _room.RoomNumber,
                checkedInDateString: checkInDate,
                nights: randomNights,
                spent: randomSpent
            );
        }
    }

    // method to check-out all guests which their booking nights are over in the given hotel
    private static void CheckOutExpiredBookings(Hotel hotel)
    {
        foreach ((string date, HotelBooking booking) in hotel.ExpiredBookings)
            hotel.CheckOutGuest(booking, date);
    }

    // method to check-in guests using the past check-in dates ranges
    // and random number of guests to each one of the 4 hotels
    // and check-out guests from each one of the 4 hotels
    private static void CheckInAndCheckOutGuests()
    {
        foreach ((string startDate, string endDate) in _checkInDateRanges)
        {
            CheckInGuests(
                hotel: _resortHotel,
                numberOfGuests: _faker.Random.Int(100, 200),
                checkInDateRange: (startDate, endDate)
            );
            CheckInGuests(
                hotel: _cityHotel,
                numberOfGuests: _faker.Random.Int(50, 100),
                checkInDateRange: (startDate, endDate)
            );
            CheckInGuests(
                hotel: _boutiqueHotel,
                numberOfGuests: _faker.Random.Int(25, 50),
                checkInDateRange: (startDate, endDate)
            );
            CheckInGuests(
                hotel: _economyHotel,
                numberOfGuests: _faker.Random.Int(10, 25),
                checkInDateRange: (startDate, endDate)
            );
            CheckOutExpiredBookings(_resortHotel);
            CheckOutExpiredBookings(_cityHotel);
            CheckOutExpiredBookings(_boutiqueHotel);
            CheckOutExpiredBookings(_economyHotel);
        }
    }

    // method to randomly choose range of existing bookings in the given hotel
    // and update existing booking to stay longer and spent more
    private static void UpdateBookings(Hotel hotel)
    {
        var existingBookings = Helpers.PickSet(hotel.Bookings, _faker.Random.Int(5, hotel.Bookings.Count));
        foreach (var booking in existingBookings)
        {
            var _room = hotel.Rooms.First(room => room.RoomNumber == booking.RoomNumber);
            int moreNights = _faker.Random.Int(1, 30);
            double moreSpent = _room.PricePerNight * moreNights + _faker.Random.Double(100, 500);
            hotel.UpdateGuestBooking(booking, moreNights, moreSpent);
        }
    }
    // method to check-in guests with presently dates ranges
    // with random number of guests to each one of the 4 hotels
    // and update their existing booking to stay longer and spent more
    private static void CheckInAndUpdateBookings()
    {
        CheckInGuests(
            hotel: _resortHotel,
            numberOfGuests: _faker.Random.Int(100, 200),
            checkInDateRange: ("2026-05-27", "2026-06-30")
        );
        UpdateBookings(_resortHotel);

        CheckInGuests(
            hotel: _cityHotel,
            numberOfGuests: _faker.Random.Int(50, 100),
            checkInDateRange: ("2026-05-27", "2026-06-30")
        );
        UpdateBookings(_cityHotel);

        CheckInGuests(
            hotel: _boutiqueHotel,
            numberOfGuests: _faker.Random.Int(25, 50),
            checkInDateRange: ("2026-05-27", "2026-06-30")
        );
        UpdateBookings(_boutiqueHotel);

        CheckInGuests(
            hotel: _economyHotel,
            numberOfGuests: _faker.Random.Int(10, 25),
            checkInDateRange: ("2026-05-27", "2026-06-30")
        );
        UpdateBookings(_economyHotel);
    }

    // method to check for guests and 4 hotels JSON files existence
    private static bool DataFilesExists()
    {
        foreach(string path in _filesPaths)
            if (!File.Exists(path))
                return false;
        return true;
    }

    // method to read all guests and hotels data from JSON files
    // and fill in the hotels and guests lists
    private static async Task ReadDataFromJsonFiles()
    {
        Console.WriteLine("Reading Guests Data from JSON File Starts");
        Console.WriteLine("-----------------------------------------");
        _guests = await Helpers.ReadFromJsonFileAsync<List<Guest>>(Path.Combine(DATA_DIRECTORY, GUESTS_FILE_NAME));
        Console.WriteLine("Reading Guests Data from JSON File Finished");
        Console.WriteLine("-----------------------------------------");

        Console.WriteLine("Reading Hotels Data from JSON Files Starts");
        Console.WriteLine("-----------------------------------------");
        _resortHotel = await Helpers.ReadFromJsonFileAsync<Hotel>(Path.Combine(DATA_DIRECTORY, RESORT_HOTEL_FILE_NAME));
        _cityHotel = await Helpers.ReadFromJsonFileAsync<Hotel>(Path.Combine(DATA_DIRECTORY, CITY_HOTEL_FILE_NAME));
        _boutiqueHotel = await Helpers.ReadFromJsonFileAsync<Hotel>(Path.Combine(DATA_DIRECTORY, BOUTIQUE_HOTEL_FILE_NAME));
        _economyHotel = await Helpers.ReadFromJsonFileAsync<Hotel>(Path.Combine(DATA_DIRECTORY, ECONOMY_HOTEL_FILE_NAME));
        Console.WriteLine("Reading Hotels Data from JSON Files Finished");
        Console.WriteLine("-----------------------------------------");

        _hotels = [_resortHotel, _cityHotel, _boutiqueHotel, _economyHotel];
    }

    // method to seed the hotels and guests data
    private static void SeedDataInMemory()
    {
        // fill each one of the 4 hotels with random rooms and prices
        Console.WriteLine("Seeding the 4 Hotel Started ...");
        Console.WriteLine("-----------------------------------------");
        _resortHotel = GetResortHotel();
        _cityHotel = GetCityHotel();
        _boutiqueHotel = GetBoutiqueHotel();
        _economyHotel = GetEconomyHotel();
        Console.WriteLine("Seeding the 4 Hotel Finished!");
        Console.WriteLine("-----------------------------------------");

        // generate a list of 1200 random guests to check-in to each one of the 4 hotels
        Console.WriteLine("Seeding the 1200 Guests Started ...");
        Console.WriteLine("-----------------------------------------");
        _guests = GetGuests(1200);
        Console.WriteLine("Seeding the 1200 Guests Finished!");
        Console.WriteLine("-----------------------------------------");

        try
        {
            // create a list from the 4 hotels
            _hotels = [_resortHotel, _cityHotel, _boutiqueHotel, _economyHotel];

            // check-in and check-out guests to each one of the 4 hotels
            Console.WriteLine("Testing Check-in and Check-out Guests Starts");
            CheckInAndCheckOutGuests();
            Console.WriteLine("Testing Check-in and Check-out Guests Finished");
            Console.WriteLine("-----------------------------------------");

            // check-in and update bookings to each one of the 4 hotels
            Console.WriteLine("Testing Presently Check-in and Update Bookings Starts");
            CheckInAndUpdateBookings();
            Console.WriteLine("Testing Presently Check-in and Update Bookings Finished");
            Console.WriteLine("-----------------------------------------");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // method to write Hotel Manager data to JSON files
    private static async Task WriteDataToJsonFiles()
    {
        string filePath = Path.Combine(DATA_DIRECTORY, GUESTS_FILE_NAME);
        Console.WriteLine("Writing Guests and Hotels Data to JSON Files Starts");
        Console.WriteLine("-----------------------------------------");
        await Helpers.WriteToJsonFileAsync(filePath, _guests);
        foreach (var hotel in _hotels)
        {
            filePath = Path.Combine(DATA_DIRECTORY, $"{hotel.Name.ToLower().Replace(' ', '_')}.json");
            await Helpers.WriteToJsonFileAsync(filePath, hotel);
        }
        Console.WriteLine("Writing Guests and Hotels Data to JSON Files Finished");
        Console.WriteLine("-----------------------------------------");
    }

    // method to print some details about the given hotel
    private static void DisplayHotelInfo(Hotel hotel)
    {
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"{hotel.Name} Details:");
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine(hotel);
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"({hotel.TotalGuests}) Total Guests, ({hotel.CurrentGuestsCount}) Current, ({hotel.PastGuestsCount}) Leaved.");
        Console.Write("Guests Count by Tier: ");
        foreach(var (tier, count) in hotel.TierGuestsCounter)
            Console.Write($"({count}) -> [{tier}] ");
        Console.WriteLine();
        Console.WriteLine($"({hotel.TotalRooms}) Total Rooms, ({hotel.AvailableRoomsCount}) Empty, ({hotel.OccupiedRoomsCount}) Occupied");
        Console.WriteLine($"({hotel.TotalBookings}) Total Bookings, ({hotel.CurrentBookingsCount}) Current, ({hotel.CompletedBookingsCount}) Completed, ({hotel.ExpiredBookingsCount}) Expired");
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"Average Guests Age: {hotel.AverageAge:F2}");
        Console.WriteLine($"Average Guests Score: {hotel.AverageScore:F2}");
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"Total Guests Spent: {hotel.TotalSpent:C2}");
        Console.WriteLine($"Average Guests Spent: {hotel.AverageSpent:C2}");
        Console.WriteLine($"Average Guests Nights: {hotel.AverageNights:F2}");
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"Top Scored Guests:\n[{string.Join(", ", hotel.GetTopScoredGuests(3))}]");
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"Top Spent Guests:\n[{string.Join(", ", hotel.GetTopSpentGuests(3))}]");
        Console.WriteLine($"-----------------------------------------");
        Console.WriteLine($"Top Stayed Guests:\n[{string.Join(", ", hotel.GetTopStayedGuests(3))}]");
        Console.WriteLine($"-----------------------------------------");
        string randomTier = GetRandomTier().ToString();
        List<HotelGuestHistory> tierGuests = [..hotel.GetGuestsByTier(randomTier)];
        if(tierGuests.Count > 0)
            Console.WriteLine($"({tierGuests.Count}) [{randomTier}] Tier Guests:\n[{string.Join(", ", tierGuests)}]");
        Console.WriteLine($"-----------------------------------------");
    }

    // method to run and test the hotel management system
    public static async Task Run()
    {
        Console.WriteLine("------------------------------");
        Console.WriteLine("Welcome to the Hotel Management System!");
        Console.WriteLine("------------------------------");
        try
        {
            if(DataFilesExists())
                await ReadDataFromJsonFiles();
            else
            {
                SeedDataInMemory();
                await WriteDataToJsonFiles();
            }

            Console.WriteLine("Testing Print Hotel Info Starts");
            Console.WriteLine("-----------------------------------------");
            foreach (var hotel in _hotels) DisplayHotelInfo(hotel);
            Console.WriteLine("Testing Print Hotel Info Finished");
            Console.WriteLine("-----------------------------------------");

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
