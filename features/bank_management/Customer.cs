using System.Text.Json.Serialization;

namespace BankManagement;

public enum Gender : byte { Male = 1, Female = 2 }

// ═════════════════════════════════════════════════════════════════════════════════════
//  CUSTOMER — a person who is a customer of the bank
// ═════════════════════════════════════════════════════════════════════════════════════
public sealed class Customer : ICloneable
{
    // ── encapsulated state (private fields) ───────────────────────────────────────────────
    private readonly string _id;
    private readonly string _firstName;
    private readonly string _lastName;
    private readonly Gender _gender;
    private readonly string _mobileNumber;
    private readonly string _nationalId;
    private readonly string _email;
    private readonly string _birthDate;
    private readonly string _generation;
    private readonly int _age;

    // ── public Identity (getter properties) ───────────────────────────────────────────────
    public string Id => _id;
    public string FirstName => _firstName;
    public string LastName => _lastName;
    public string Gender => _gender.ToString();
    public string MobileNumber => _mobileNumber;
    public string NationalId => _nationalId;
    public string Email => _email;
    public string BirthDate => _birthDate;
    public string Generation => _generation;
    public int Age => _age;

    [JsonIgnore]
    public string FullName => $"{_firstName} {_lastName}";

    // ── internal business logic (methods) ───────────────────────────────────────────────
    private int CalculateAge()
    {
        var today = DateTime.Today;
        var birthDate = DateTime.Parse(_birthDate);
        var age = today.Year - DateTime.Parse(_birthDate).Year;
        if(today < birthDate.AddYears(age))
            age--;
        return age;
    }
    private string CalculateGeneration() => DateTime.Parse(_birthDate).Year switch {
        < 1940 => "Silent",
        < 1965 => "Baby Boomers",
        < 1980 => "Gen X",
        < 1997 => "Gen Y",
        < 2013 => "Gen Z",
        < 2026 => "Gen Alpha",
        < 2040 => "Gen Beta",
        _ => "Unknown"
    };

    // ── public factory constructor ──────────────────────────────────────────────────────
    [JsonConstructor]
    public Customer(
        string firstName,
        string lastName,
        string birthDate,
        string mobileNumber,
        string nationalId,
        string email,
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
        nationalId = nationalId.Trim();
        email = email.Trim();
        gender = gender.Trim().ToLower();

        if(string.IsNullOrEmpty(firstName))
            throw new ArgumentException("must provide a valid first name.", nameof(firstName));
        if(string.IsNullOrEmpty(lastName))
            throw new ArgumentException("must provide a valid last name.", nameof(lastName));
        if(string.IsNullOrEmpty(birthDate) || !DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("must provide a valid birth date.", nameof(birthDate));
        if (!Helpers.IsValidMobileNumber(mobileNumber))
            throw new ArgumentException("Invalid phone number.", nameof(mobileNumber));
        if (string.IsNullOrWhiteSpace(nationalId))
            throw new ArgumentException("must provide a valid national id.", nameof(nationalId));
        if (!Helpers.IsValidEmail(email))
            throw new ArgumentException("Invalid email.", nameof(email));
        if (!Enum.TryParse(gender, ignoreCase: true, out Gender parsedGender))
            throw new ArgumentException("must provide a valid gender.", nameof(gender));

        _id = string.IsNullOrEmpty(id) ? Helpers.GenerateDigitsOnlyId(11) : id;
        _firstName = firstName;
        _lastName = lastName;
        _birthDate = birthDate;
        _mobileNumber = mobileNumber;
        _nationalId = nationalId;
        _email = email;
        _gender = parsedGender;
        _generation = string.IsNullOrEmpty(generation) ? CalculateGeneration() : generation;
        _age = age == 0 ? CalculateAge() : age;
    }

    // ─── ICloneable implementation ───────────────────────────────────────────────────────────
    public object Clone() => new Customer(
        id: Id,
        firstName: FirstName,
        lastName: LastName,
        birthDate: BirthDate,
        mobileNumber: MobileNumber,
        nationalId: NationalId,
        email: Email,
        gender: Gender,
        generation: Generation,
        age: Age
    );

    // ── overrides base object methods ────────────────────────────────────────────────────
    public override string ToString() =>
        $"{FullName} is a {Gender} person, born on {BirthDate}, and is {Age} years old, and is in the {Generation} generation.";
    public override bool Equals(object? obj) =>
        obj is Customer customer &&
        Id == customer.Id &&
        FirstName == customer.FirstName &&
        LastName == customer.LastName &&
        BirthDate == customer.BirthDate &&
        MobileNumber == customer.MobileNumber &&
        NationalId == customer.NationalId &&
        Email == customer.Email && Gender == customer.Gender;
    public override int GetHashCode() =>
        HashCode.Combine(Id, FirstName, LastName, BirthDate, MobileNumber, NationalId, Email, Gender);

    // ── overrides equality and comparison operators ───────────────────────────────────────
    public static bool operator ==(Customer left, Customer right) => left.NationalId == right.NationalId;
    public static bool operator !=(Customer left, Customer right) => left.NationalId != right.NationalId;
    public static bool operator >(Customer left, Customer right) => left.Age > right.Age;
    public static bool operator <(Customer left, Customer right) => left.Age < right.Age;
    public static bool operator >=(Customer left, Customer right) => left.Age >= right.Age;
    public static bool operator <=(Customer left, Customer right) => left.Age <= right.Age;

}
