using System.Text.Json.Serialization;

namespace ResumesBuilder;

public sealed class Skill
{
    private readonly string _title;
    private readonly string _description;
    private readonly double _proficiencyLevel;
    public string Title => _title;
    public string Description => _description;
    public double ProficiencyLevel => _proficiencyLevel;
    [JsonConstructor]
    public Skill(
        string title,
        string description,
        double proficiencyLevel
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
        if (proficiencyLevel < 0.0 || proficiencyLevel > 10.0)
            throw new ArgumentOutOfRangeException(nameof(proficiencyLevel), "Proficiency level must be between 0 and 10.");

        _title = title;
        _description = description;
        _proficiencyLevel = proficiencyLevel;
    }

    public override string ToString() =>
        $"""
        Title: {Title}
        Description: {Description}
        Proficiency Level: {ProficiencyLevel * 100}%
        """;
    public override int GetHashCode() =>
        HashCode.Combine(Title, Description, ProficiencyLevel);
    public override bool Equals(object? obj)
    {
        if (obj is not Skill other) return false;
        return Title == other.Title &&
            Description == other.Description &&
            ProficiencyLevel == other.ProficiencyLevel;
    }
}

public sealed class SkillsSection : IResumeSection
{
    private readonly List<Skill> _skills;
    public IReadOnlyList<Skill> Skills => _skills.AsReadOnly();
    [JsonConstructor]
    public SkillsSection(IReadOnlyList<Skill> skills)
    {
        ArgumentNullException.ThrowIfNull(skills, nameof(skills));
        _skills = [..skills];
    }
    public void AddSkill(Skill skill)
    {
        ArgumentNullException.ThrowIfNull(skill, nameof(skill));
        _skills.Add(skill);
    }
    public void RemoveSkill(Skill skill)
    {
        ArgumentNullException.ThrowIfNull(skill, nameof(skill));
        _skills.Remove(skill);
    }

    [JsonIgnore]
    public int SkillsCount => _skills.Count;
    [JsonIgnore]
    public Skill? MostProficientSkill => (
        from skill in _skills
        orderby skill.ProficiencyLevel descending
        select skill
    ).FirstOrDefault();
    [JsonIgnore]
    public Skill? LeastProficientSkill => (
        from skill in _skills
        orderby skill.ProficiencyLevel ascending
        select skill
    ).FirstOrDefault();

    public string Title => "Skills";
    public bool IsEmpty => _skills.Count == 0;

    public string Render()
    {
        if (IsEmpty) return "No skills added yet.";
        string headerDivider = new('-', 30);
        var skills = from skill in Skills select skill.ToString();
        return string.Join($"\n{headerDivider}\n", skills);
    }
    public override string ToString() => Render();
}
