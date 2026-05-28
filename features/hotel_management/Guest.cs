using System.Text.Json.Serialization;

namespace HotelManagement;

public class Guest
{
    private readonly string _id;
    private readonly string _firstName;
    private readonly string _lastName;
    private readonly string _gender;
    private readonly string _birthDate;
    private readonly string _mobileNumber;
    private readonly int _age;
    private readonly string _generation;

    public string Id => _id;
    public string FirstName => _firstName;
    public string LastName => _lastName;
    public string Gender => _gender;
    public string BirthDate => _birthDate;
    public string MobileNumber => _mobileNumber;
    public string Generation => _generation;
    public int Age => _age;

    [JsonIgnore]
    public string FullName => $"{_firstName} {_lastName}";

    private int CalculateAge()
    {
        var today = DateTime.Today;
        var birthDate = DateTime.Parse(_birthDate);
        var age = today.Year - DateTime.Parse(_birthDate).Year;
        if(today < birthDate.AddYears(age))
            age--;
        return age;
    }
    private string CalculateGeneration() =>
        DateTime.Parse(_birthDate).Year switch {
            < 1940 => "Silent",
            < 1965 => "Baby Boomers",
            < 1980 => "Gen X",
            < 1997 => "Gen Y",
            < 2013 => "Gen Z",
            < 2026 => "Gen Alpha",
            < 2040 => "Gen Beta",
            _ => "Unknown"
        };

    [JsonConstructor]
    public Guest(
        string firstName,
        string lastName,
        string birthDate,
        string mobileNumber,
        string gender = "male",
        string id = "",
        string generation = "",
        int age = 0
    )
    {
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        birthDate = birthDate.Trim();
        mobileNumber = mobileNumber.Trim();
        gender = gender.Trim().ToLower();

        ArgumentNullException.ThrowIfNull(firstName, nameof(firstName));
        ArgumentNullException.ThrowIfNull(lastName, nameof(lastName));
        ArgumentNullException.ThrowIfNull(birthDate, nameof(birthDate));
        ArgumentNullException.ThrowIfNull(mobileNumber, nameof(mobileNumber));
        ArgumentNullException.ThrowIfNull(gender, nameof(gender));

        if (!DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("must provide a valid birth date.");
        if (mobileNumber.Length < 11)
            throw new ArgumentException("must provide a valid mobile number.");
        if (gender != "male" && gender != "female")
            throw new ArgumentException("Gender must be either 'male' or 'female'.");

        _id = string.IsNullOrEmpty(id)
            ? Helpers.GenerateId(8)
            : id;
        _firstName = firstName;
        _lastName = lastName;
        _birthDate = birthDate;
        _mobileNumber = mobileNumber;
        _gender = gender;
        _generation = string.IsNullOrEmpty(generation)
            ? CalculateGeneration()
            : generation;
        _age = age == 0
            ? CalculateAge()
            : age;
    }

    public override string ToString() =>
        $"{FullName} is a {Gender} guest, born on ({BirthDate}), has ({Age}) years old and belongs to the {Generation} generation.";
    public override int GetHashCode() =>
        HashCode.Combine(Id, FullName, Gender, BirthDate, MobileNumber);
    public override bool Equals(object? obj)
    {
        if (obj is Guest otherGuest)
            return Id == otherGuest.Id && FullName == otherGuest.FullName &&
                Gender == otherGuest.Gender && BirthDate == otherGuest.BirthDate &&
                MobileNumber == otherGuest.MobileNumber;
        return false;
    }

    public static bool operator ==(Guest left, Guest right) => left.Equals(right);
    public static bool operator !=(Guest left, Guest right) => !left.Equals(right);
    public static bool operator >(Guest left, Guest right) => left.Age > right.Age;
    public static bool operator <(Guest left, Guest right) => left.Age < right.Age;
    public static bool operator >=(Guest left, Guest right) => left.Age >= right.Age;
    public static bool operator <=(Guest left, Guest right) => left.Age <= right.Age;

}
