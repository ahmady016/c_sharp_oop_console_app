namespace CourseManagement;
public abstract class Person
{
    private readonly string _id;
    private readonly string _firstName;
    private readonly string _lastName;
    private readonly string _gender;
    private readonly string _birthDate;

    public string Id => _id;
    public string FirstName => _firstName;
    public string LastName => _lastName;
    public string Gender => _gender;
    public string BirthDate => _birthDate;
    public string FullName => $"{FirstName} {LastName}";
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

    public Person(
        string firstName,
        string lastName,
        string gender = "male",
        string birthDate = "2000-01-01"
    )
    {
        if(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            throw new ArgumentException("Invalid first or last name.");
        if(string.IsNullOrEmpty(gender) || (gender != "male" && gender != "female"))
            throw new ArgumentException("Invalid gender.");
        if(!DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("Invalid birth date.");

        _id = Helpers.GenerateId();
        _firstName = firstName;
        _lastName = lastName;
        _gender = gender;
        _birthDate = birthDate;
    }

    public override string ToString() =>
        $"{FullName} is a {Gender} student, born on {BirthDate}, and is {Age} years old.";

    public override int GetHashCode() =>
        HashCode.Combine(FullName, Gender, BirthDate);

    public override bool Equals(object? obj) =>
        obj is Person person &&
        person.FullName == FullName &&
        person.Gender == Gender &&
        person.BirthDate == BirthDate;

    public static bool operator == (Person left, Person right) => left.Equals(right);
    public static bool operator != (Person left, Person right) => !left.Equals(right);

    public static bool operator < (Person left, Person right) => left.Age < right.Age;
    public static bool operator > (Person left, Person right) => left.Age > right.Age;

    public static bool operator <= (Person left, Person right) => left.Age <= right.Age;
    public static bool operator >= (Person left, Person right) => left.Age >= right.Age;

}