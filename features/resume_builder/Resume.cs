using System.Text.Json.Serialization;

namespace ResumesBuilder;

public sealed class Resume : IResumeSection
{
    private readonly string _title;
    private readonly string _description;
    private readonly PersonalSection _personalInfo;
    private readonly ContactSection _contactInfo;
    private readonly QualificationsSection _qualifications;
    private readonly JobExperiencesSection _experiences;
    private readonly SkillsSection _skills;

    public string Title => _title;
    public string Description => _description;
    public PersonalSection PersonalInfo => _personalInfo;
    public ContactSection ContactInfo => _contactInfo;
    public QualificationsSection Qualifications => _qualifications;
    public JobExperiencesSection Experiences => _experiences;
    public SkillsSection Skills => _skills;

    private IEnumerable<IResumeSection> Sections()
    {
        yield return _personalInfo;
        yield return _contactInfo;
        yield return _qualifications;
        yield return _experiences;
        yield return _skills;
    }
    [JsonConstructor]
    public Resume(
        string title,
        string description,
        PersonalSection personalInfo,
        ContactSection contactInfo,
        QualificationsSection qualifications,
        JobExperiencesSection experiences,
        SkillsSection skills
    )
    {
        _title = title;
        _description = description;
        _personalInfo = personalInfo;
        _contactInfo = contactInfo;
        _qualifications = qualifications;
        _experiences = experiences;
        _skills = skills;
    }

    public int QualificationsCount => _qualifications.QualificationsCount;
    public Qualification? LastQualification => _qualifications.LastQualification;

    public int SkillsCount => _skills.SkillsCount;
    public Skill? MostProficientSkill => _skills.MostProficientSkill;
    public Skill? LeastProficientSkill => _skills.LeastProficientSkill;

    public int ExperiencesCount => _experiences.ExperiencesCount;
    public int TotalYearsOfExperience => _experiences.TotalYearsOfExperience;
    public JobExperience? CurrentJob => _experiences.CurrentJob;
    public JobExperience? MostExperiencedJob => _experiences.MostExperiencedJob;

    public bool IsEmpty => string.IsNullOrEmpty(_title) ||
        string.IsNullOrEmpty(_description) ||
        Sections().All(section => section.IsEmpty);
    public string Render()
    {
        string headerDivider = new('-', 30);
        string sectionDivider = new('_', 50);

        if (IsEmpty)
            return "Opps, your resume is empty. Try fill in some information first.";

        var header = $"{Title.ToUpper()}\n{headerDivider}\n{Description}\n{headerDivider}";
        var sections = from section in Sections()
            where !section.IsEmpty
            let block = $"{section.Title.ToUpper()}\n{headerDivider}\n{section.Render()}"
            select block;
        string body = string.Join($"\n{sectionDivider}\n", sections);
        var footer = $"""
            Summary:
            {headerDivider}
            Total Skills: {SkillsCount}
            Total Qualifications: {QualificationsCount}
            Total Years of Experience: {TotalYearsOfExperience}
            Total Job Experiences: {ExperiencesCount}
            {headerDivider}
            Last Qualification:
            {headerDivider}
            {LastQualification}
            {headerDivider}
            Most Proficient Skill:
            {headerDivider}
            {MostProficientSkill}
            {headerDivider}
            Least Proficient Skill:
            {headerDivider}
            {LeastProficientSkill}
            {headerDivider}
            {(CurrentJob is not null
                ? $"Current Job:\n{headerDivider}\n{CurrentJob}"
                : "Current Job: None"
            )}
            {headerDivider}
            Most Experienced Job:
            {headerDivider}
            {MostExperiencedJob}
            {headerDivider}
        """;
        return $"{header}\n{body}\n{sectionDivider}\n{footer}";
    }
    public override string ToString() => Render();
}
