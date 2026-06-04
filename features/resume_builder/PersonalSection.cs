using System.Text.Json.Serialization;

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
    [JsonConstructor]
    public PersonalSection(
        string fullName,
        string birthDate,
        string gender,
        string nationality,
        string nationalIdNumber,
        string maritalStatus
    )
    {
        if(string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("must provide a full name.");
        if (string.IsNullOrWhiteSpace(birthDate))
            throw new ArgumentException("must provide a birth date.");
        if (string.IsNullOrWhiteSpace(gender))
            throw new ArgumentException("must provide a gender.");
        if (string.IsNullOrWhiteSpace(nationality))
            throw new ArgumentException("must provide a nationality.");
        if (string.IsNullOrWhiteSpace(nationalIdNumber))
            throw new ArgumentException("must provide a national ID number.");
        if (string.IsNullOrWhiteSpace(maritalStatus))
            throw new ArgumentException("must provide a marital status.");

        if (!DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("must provide a valid birth date.");
        if (gender != "male" && gender != "female")
            throw new ArgumentException("Gender must be either 'male' or 'female'.");
        if (!Enum.TryParse(maritalStatus, out MaritalStatus parsedMaritalStatus))
            throw new ArgumentException("Invalid marital status.");

        _fullName = fullName;
        _birthDate = birthDate;
        _gender = gender;
        _nationality = nationality;
        _nationalIdNumber = nationalIdNumber;
        _maritalStatus = parsedMaritalStatus;
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
