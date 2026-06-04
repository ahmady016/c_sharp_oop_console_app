using System.Text.Json.Serialization;

namespace ResumesBuilder;

public sealed class JobExperience
{
    private readonly string _jobTitle;
    private readonly string _companyName;
    private readonly string _startDate;
    private readonly string? _endDate;
    private readonly string _achievements;

    public string JobTitle => _jobTitle;
    public string CompanyName => _companyName;
    public string StartDate => _startDate;
    public string? EndDate => _endDate;
    public string Achievements => _achievements;
    public bool IsCurrent => _endDate == null;
    public string EmploymentPeriod => $"From ({_startDate}) To {_endDate ?? "Present"}";
    public int YearsOfExperience
    {
        get
        {
            var start = DateTime.Parse(_startDate);
            var end = _endDate != null ? DateTime.Parse(_endDate) : DateTime.Now;
            var duration = end - start;
            return (int)(duration.TotalDays / 365.25);
        }
    }
    public string Duration
    {
        get
        {
            var start = DateTime.Parse(_startDate);
            var end = _endDate != null ? DateTime.Parse(_endDate) : DateTime.Now;
            var duration = end - start;

            int months = (int)(duration.TotalDays / 30.4375);
            if(months < 12)
                return $"{months} {Helpers.GetPluralString("month", months)}";

            int years = months / 12;
            int remainingMonths = months % 12;
            return $"{years} {Helpers.GetPluralString("year", years)} " +
                $"{remainingMonths} {Helpers.GetPluralString("month", remainingMonths)}";
        }
    }
    [JsonConstructor]
    public JobExperience(
        string jobTitle,
        string companyName,
        string startDate,
        string? endDate,
        string achievements
    )
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title cannot be null or whitespace.", nameof(jobTitle));
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be null or whitespace.", nameof(companyName));
        if (string.IsNullOrWhiteSpace(startDate))
            throw new ArgumentException("Start date cannot be null or whitespace.", nameof(startDate));
        if (string.IsNullOrWhiteSpace(achievements))
            throw new ArgumentException("Achievements cannot be null or whitespace.", nameof(achievements));

        if (!DateTime.TryParse(startDate, out _))
            throw new ArgumentException("must provide a valid start date.");
        if (endDate != null && !DateTime.TryParse(endDate, out _))
            throw new ArgumentException("must provide a valid end date.");

        _jobTitle = jobTitle;
        _companyName = companyName;
        _startDate = startDate;
        _endDate = endDate;
        _achievements = achievements;
    }
    public override string ToString() =>
        $"""
        Job Title: {_jobTitle}
        Company Name: {_companyName}
        Employment Period: {EmploymentPeriod}
        Duration: {Duration}
        Achievements: {_achievements}
        """;
    public override bool Equals(object? obj)
    {
        if (obj is not JobExperience other) return false;
        return _jobTitle == other._jobTitle &&
            _companyName == other._companyName &&
            _startDate == other._startDate &&
            _endDate == other._endDate &&
            _achievements == other._achievements;
    }
    public override int GetHashCode() =>
        HashCode.Combine(_jobTitle, _companyName, _startDate, _endDate, _achievements);
}

public sealed class JobExperiencesSection : IResumeSection
{
    private readonly List<JobExperience> _jobExperiences;
    public IReadOnlyList<JobExperience> JobExperiences => _jobExperiences.AsReadOnly();
    [JsonConstructor]
    public JobExperiencesSection(IReadOnlyList<JobExperience> jobExperiences)
    {
        ArgumentNullException.ThrowIfNull(jobExperiences, nameof(jobExperiences));
        _jobExperiences = [..jobExperiences];
    }
    public void AddJobExperience(JobExperience jobExperience)
    {
        ArgumentNullException.ThrowIfNull(jobExperience, nameof(jobExperience));
        _jobExperiences.Add(jobExperience);
    }
    public void RemoveJobExperience(JobExperience jobExperience)
    {
        ArgumentNullException.ThrowIfNull(jobExperience, nameof(jobExperience));
        _jobExperiences.Remove(jobExperience);
    }

    [JsonIgnore]
    public int ExperiencesCount => _jobExperiences.Count;
    [JsonIgnore]
    public JobExperience? CurrentJob => (
        from job in JobExperiences
        where job.IsCurrent
        orderby DateTime.Parse(job.StartDate) descending
        select job
    ).FirstOrDefault();
    [JsonIgnore]
    public JobExperience? MostExperiencedJob => (
        from job in JobExperiences
        orderby job.YearsOfExperience descending
        select job
    ).FirstOrDefault();
    [JsonIgnore]
    public int TotalYearsOfExperience => JobExperiences.Sum(job => job.YearsOfExperience);

    public string Title => "Job Experiences";
    public bool IsEmpty => _jobExperiences.Count == 0;
    public string Render()
    {
        if (IsEmpty) return "No job experiences listed.";
        string headerDivider = new('-', 30);
        var sortedExperiences = from job in _jobExperiences
                                let endDate = job.EndDate != null ? DateTime.Parse(job.EndDate) : DateTime.Now
                                orderby endDate descending
                                select job.ToString();
        return string.Join($"\n{headerDivider}\n", sortedExperiences);
    }
    public override string ToString() => Render();
}