namespace ResumesBuilder;

public enum MaritalStatus : byte
{
    Single = 1,
    Married = 2,
    Divorced = 3,
    Widowed = 4
}

public sealed class PersonalSection : IResumeSection
{
    private readonly string _fullName;
    private readonly string _birthDate;
    private readonly string _gender;
    private readonly string _nationality;
    private readonly string _nationalIdNumber;
    private readonly MaritalStatus _maritalStatus;

    public string FullName => _fullName;
    public string BirthDate => _birthDate;
    public string Gender => _gender;
    public string Nationality => _nationality;
    public string NationalIdNumber => _nationalIdNumber;
    public string MaritalStatus => _maritalStatus.ToString();
    public int Age
    {
        get
        {
            var birthDate = DateTime.Parse(_birthDate);
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (today < birthDate.AddYears(age)) age--;
            return age;
        }
    }

    public PersonalSection(
        string fullName,
        string birthDate,
        string gender,
        string nationality,
        string nationalIdNumber,
        MaritalStatus maritalStatus
    )
    {
        ArgumentNullException.ThrowIfNull(fullName, nameof(fullName));
        ArgumentNullException.ThrowIfNull(birthDate, nameof(birthDate));
        ArgumentNullException.ThrowIfNull(gender, nameof(gender));
        ArgumentNullException.ThrowIfNull(nationality, nameof(nationality));
        ArgumentNullException.ThrowIfNull(nationalIdNumber, nameof(nationalIdNumber));

        if (!DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("must provide a valid birth date.");
        if (gender != "male" && gender != "female")
            throw new ArgumentException("Gender must be either 'male' or 'female'.");
        if (!Enum.IsDefined(maritalStatus))
            throw new ArgumentException("Invalid marital status.");

        _fullName = fullName;
        _birthDate = birthDate;
        _gender = gender;
        _nationality = nationality;
        _nationalIdNumber = nationalIdNumber;
        _maritalStatus = maritalStatus;
    }

    public string Title => "Personal Information";
    public bool IsEmpty => string.IsNullOrWhiteSpace(_fullName) ||
        string.IsNullOrWhiteSpace(_birthDate) ||
        string.IsNullOrWhiteSpace(_gender) ||
        string.IsNullOrWhiteSpace(_nationality) ||
        string.IsNullOrWhiteSpace(_nationalIdNumber) ||
        string.IsNullOrWhiteSpace(_maritalStatus.ToString());
    public string Render() =>
        $"""
        Full Name: {FullName}
        Birth Date: {BirthDate}
        Gender: {Gender}
        Nationality: {Nationality}
        National ID Number: {NationalIdNumber}
        Marital Status: {MaritalStatus}
        Age: {Age}
        """;
    public override string ToString() => Render();
    public override bool Equals(object? obj)
    {
        if (obj is not PersonalSection other) return false;
        return _fullName == other._fullName &&
            _birthDate == other._birthDate &&
            _gender == other._gender &&
            _nationality == other._nationality &&
            _nationalIdNumber == other._nationalIdNumber &&
            _maritalStatus == other._maritalStatus;
    }
    public override int GetHashCode() =>
        HashCode.Combine(_fullName, _birthDate, _gender, _nationality, _nationalIdNumber, _maritalStatus);
}
