
public class Person
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _gender = "male";
    private string _birthDate = $"{DateTime.Now:yyy-mm-dd}";
    private float _height = 70;
    private float _weight = 1.5f;
    private float _bankBalance = 0.0f;

    public string BirthDate => _birthDate;
    public string Gender => _gender;
    public float Height {
        get => _height;
        set {
            if(value <= 0 && value > 250)
                throw new ArgumentException("Invalid height.");
            _height = value;
        }
    }
    public float Weight {
        get => _weight;
        set {
            if(value <= 0 && value > 150)
                throw new ArgumentException("Invalid weight.");
            _weight = value;
        }
    }
    public float BankBalance {
        get => _bankBalance;
        set {
            if(value < 0 && value > float.MaxValue)
                throw new ArgumentException("Invalid bank balance.");
            _bankBalance = value;
        }
    }

    public string FullName => $"{_firstName} {_lastName}";
    public int Age {
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
    public float BmiRate => _weight / (_height / 100 * (_height / 100));
    public string BmiStatus => BmiRate switch {
        < 18.5f => "Underweight",
        < 25 => "Normal",
        < 30 => "Overweight",
        _ => "Obese"
    };

    public Person(string firstName, string lastName)
    {
        if(string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            throw new ArgumentException("Invalid first or last name.");

        _firstName = firstName;
        _lastName = lastName;
    }

    public Person(
        string firstName,
        string lastName,
        string gender,
        string birthDate,
        float height,
        float weight,
        float bankBalance
    ) : this(firstName, lastName)
    {
        if(string.IsNullOrEmpty(gender) || (gender != "male" && gender != "female"))
            throw new ArgumentException("Invalid gender.");
        if(!DateTime.TryParse(birthDate, out _))
            throw new ArgumentException("Invalid birth date.");

        _gender = gender;
        _birthDate = birthDate;

        Height = height;
        Weight = weight;
        BankBalance = bankBalance;
    }

    public override string ToString() =>
        $"{FullName} is a {Gender} person, born on {BirthDate}, and is {Age} years old, and is in the {Generation} generation, with a BMI of {BmiRate} and is {BmiStatus}.";

    public override int GetHashCode() =>
        HashCode.Combine(FullName, Gender, BirthDate, Height, Weight);

    public override bool Equals(object? obj) =>
        obj is Person person &&
        person.FullName == FullName &&
        person.Gender == Gender &&
        person.BirthDate == BirthDate &&
        person.Height == Height &&
        person.Weight == Weight;

    public static bool operator == (Person left, Person right) => left.Equals(right);
    public static bool operator != (Person left, Person right) => !(left == right);

    public static bool operator < (Person left, Person right) => left.Age < right.Age;
    public static bool operator > (Person left, Person right) => left.Age > right.Age;

    public static bool operator <= (Person left, Person right) => left.Age <= right.Age;
    public static bool operator >= (Person left, Person right) => left.Age >= right.Age;

    public static Person operator + (Person left, Person right) => new(
            firstName: left.FullName.Split(' ')[0],
            lastName: left.FullName.Split(' ')[1],
            gender: left.Gender,
            birthDate: left.BirthDate,
            height: left.Height + right.Height,
            weight: left.Weight + right.Weight,
            bankBalance: left.BankBalance + right.BankBalance
        );
    public static Person operator - (Person left, Person right) => new(
            firstName: left.FullName.Split(' ')[0],
            lastName: left.FullName.Split(' ')[1],
            gender: left.Gender,
            birthDate: left.BirthDate,
            height: left.Height - right.Height,
            weight: left.Weight - right.Weight,
            bankBalance: left.BankBalance - right.BankBalance
        );
    public static Person operator * (Person left, Person right) => new(
            firstName: left.FullName.Split(' ')[0],
            lastName: left.FullName.Split(' ')[1],
            gender: left.Gender,
            birthDate: left.BirthDate,
            height: left.Height * right.Height,
            weight: left.Weight * right.Weight,
            bankBalance: left.BankBalance * right.BankBalance
        );
    public static Person operator / (Person left, Person right) => new(
            firstName: left.FullName.Split(' ')[0],
            lastName: left.FullName.Split(' ')[1],
            gender: left.Gender,
            birthDate: left.BirthDate,
            height: left.Height / right.Height,
            weight: left.Weight / right.Weight,
            bankBalance: left.BankBalance / right.BankBalance
        );
    public static Person operator % (Person left, Person right) => new(
            firstName: left.FullName.Split(' ')[0],
            lastName: left.FullName.Split(' ')[1],
            gender: left.Gender,
            birthDate: left.BirthDate,
            height: left.Height % right.Height,
            weight: left.Weight % right.Weight,
            bankBalance: left.BankBalance % right.BankBalance
        );

}

public static class PersonTest
{
    public static void Run()
    {
        Person omar = new(
            firstName: "Omar",
            lastName: "Salah",
            gender: "male",
            birthDate: "1999-01-01",
            height: 1.8f,
            weight: 80,
            bankBalance: 5000
        );
        Person ahmed = new(
            firstName: "Ahmed",
            lastName: "Ali",
            gender: "male",
            birthDate: "1988-10-01",
            height: 1.77f,
            weight: 88,
            bankBalance: 15000
        );
        Person sayed = new(
            firstName: "Sayed",
            lastName: "Gaber",
            gender: "male",
            birthDate: "1955-05-11",
            height: 1.71f,
            weight: 97,
            bankBalance: 25000
        );
        List<Person> people = [omar, ahmed, sayed];

        Console.WriteLine("--------------------");
        Console.WriteLine($"{people.Count} Persons List:");
        Console.WriteLine("--------------------");
        foreach (Person person in people)
            Console.WriteLine(person);

        Console.WriteLine("--------------------");
        Console.WriteLine("Does omar and sayed is equal? " + (omar == sayed));
        Console.WriteLine("Does ahmed and sayed is equal? " + (ahmed == sayed));
        Console.WriteLine("Does ahmed and omar is equal? " + (ahmed == omar));
        Console.WriteLine("--------------------");

        Console.WriteLine($"The total bank balance is {(omar + ahmed + sayed).BankBalance}");
        Console.WriteLine($"The max bank balance is {people.Max(person => person.BankBalance)}");
        Console.WriteLine($"The min bank balance is {people.Min(person => person.BankBalance)}");
        Console.WriteLine($"The average bank balance is {people.Average(person => person.BankBalance):F2}");

        Console.WriteLine("--------------------");
        Console.WriteLine($"The total wight is {(omar + ahmed + sayed).Weight}");
        Console.WriteLine($"The max wight is {people.Max(person => person.Weight)}");
        Console.WriteLine($"The min wight is {people.Min(person => person.Weight)}");
        Console.WriteLine($"The average wight is {people.Average(person => person.Weight):F2}");
        Console.WriteLine("--------------------");
    }
}
