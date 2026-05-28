namespace HotelManagement;

public class Guest
{
    private readonly string _id;
    private readonly string _firstName;
    private readonly string _lastName;
    private readonly string _gender;
    private readonly string _birthDate;
    private readonly string _mobileNumber;

    public string Id => _id;
    public string FullName => $"{_firstName} {_lastName}";
    public string Gender => _gender;
    public string BirthDate => _birthDate;
    public string MobileNumber => _mobileNumber;
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var birthDate = DateTime.Parse(_birthDate);
            var age = today.Year - DateTime.Parse(_birthDate).Year;
            if(today < birthDate.AddYears(age))
                age--;
            return age;
        }
    }
    public string Generation =>
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

    public Guest(
        string firstName,
        string lastName,
        string birthDate,
        string mobileNumber,
        string gender = "male"
    )
    {
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        birthDate = birthDate.Trim();
        mobileNumber = mobileNumber.Trim();
        gender = gender.Trim().ToLower();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            throw new ArgumentException("must provide both first name and last name.");

        if (string.IsNullOrEmpty(birthDate) || !DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("must provide a valid birth date.");

        if (string.IsNullOrEmpty(mobileNumber) || mobileNumber.Length < 11)
            throw new ArgumentException("must provide a valid mobile number.");

        if (gender != "male" && gender != "female")
            throw new ArgumentException("Gender must be either 'male' or 'female'.");

        _id = Helpers.GenerateId(8);
        _firstName = firstName;
        _lastName = lastName;
        _mobileNumber = mobileNumber;
        _gender = gender;
        _birthDate = birthDate;
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
