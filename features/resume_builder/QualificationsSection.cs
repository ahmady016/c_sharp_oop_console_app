namespace ResumesBuilder;

public sealed class Qualification
{
    private readonly string _degree;
    private readonly string _institution;
    private readonly double _grade;
    private readonly int _graduationYear;

    public string Degree => _degree;
    public string Institution => _institution;
    public string Grade => $"{_grade:F2}%";
    public int GraduationYear => _graduationYear;

    public Qualification(
        string degree,
        string institution,
        double grade,
        int graduationYear
    )
    {
        if (string.IsNullOrWhiteSpace(degree))
            throw new ArgumentException("Degree cannot be null or whitespace.", nameof(degree));
        if (string.IsNullOrWhiteSpace(institution))
            throw new ArgumentException("Institution cannot be null or whitespace.", nameof(institution));
        if (grade < 0 || grade > 100)
            throw new ArgumentException("Grade must be between 0 and 100.", nameof(grade));
        if (graduationYear < 1900 || graduationYear > DateTime.Now.Year + 10)
            throw new ArgumentException("Graduation year must be between 1900 and 10 years in the future.", nameof(graduationYear));

        _degree = degree.Trim();
        _institution = institution.Trim();
        _grade = grade;
        _graduationYear = graduationYear;
    }
    public override string ToString() =>
        $"""
        Degree: {Degree}
        Institution: {Institution}
        Grade: {Grade}
        Graduation Year: {GraduationYear}
        """;
    public override bool Equals(object? obj)    {
        if (obj is not Qualification other) return false;
        return Degree == other.Degree &&
            Institution == other.Institution &&
            Grade == other.Grade &&
            GraduationYear == other.GraduationYear;
    }
    public override int GetHashCode() =>
        HashCode.Combine(_degree, _institution, _grade, _graduationYear);
}

public sealed class QualificationsSection : IResumeSection
{
    private readonly List<Qualification> _qualifications;
    public IReadOnlyList<Qualification> Qualifications => _qualifications.AsReadOnly();

    public QualificationsSection(IEnumerable<Qualification> qualifications)
    {
        ArgumentNullException.ThrowIfNull(qualifications, nameof(qualifications));
        _qualifications = [..qualifications];
    }
    public void AddQualification(Qualification qualification)
    {
        ArgumentNullException.ThrowIfNull(qualification, nameof(qualification));
        _qualifications.Add(qualification);
    }
    public void RemoveQualification(Qualification qualification)
    {
        ArgumentNullException.ThrowIfNull(qualification, nameof(qualification));
        _qualifications.Remove(qualification);
    }

    public int QualificationsCount => _qualifications.Count;
    public Qualification? LastQualification => (
        from q in _qualifications
        orderby q.GraduationYear descending
        select q
    ).FirstOrDefault();
    public Qualification? MaxGradeQualification => (
        from q in _qualifications
        let grade = q.Grade.EndsWith('%') && double.TryParse(q.Grade.TrimEnd('%'), out var g)
            ? g : 0.0
        orderby grade descending
        select q
    ).FirstOrDefault();

    public string Title => "Qualifications And Education Background";
    public bool IsEmpty => _qualifications.Count == 0;
    public string Render()
    {
        if (IsEmpty) return "No qualifications listed.";
        string headerDivider = new('-', 30);
        var qualifications = from q in _qualifications select q.ToString();
        return string.Join($"\n{headerDivider}\n", qualifications);
    }
    public override string ToString() => Render();
}