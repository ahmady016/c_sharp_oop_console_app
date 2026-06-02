namespace ResumesBuilder;

public sealed class ResumeBuilder
{
    private PersonalSection? _personalInfo;
    private ContactSection? _contactInfo;
    private QualificationsSection? _qualifications;
    private JobExperiencesSection? _experiences;
    private SkillsSection? _skills;

    public ResumeBuilder WithPersonalInfo(PersonalSection personalInfo)
    {
        _personalInfo = personalInfo;
        return this;
    }
    public ResumeBuilder WithContactInfo(ContactSection contactInfo)
    {
        _contactInfo = contactInfo;
        return this;
    }
    public ResumeBuilder WithQualifications(QualificationsSection qualifications)
    {
        _qualifications = qualifications;
        return this;
    }
    public ResumeBuilder WithExperiences(JobExperiencesSection experiences)
    {
        _experiences = experiences;
        return this;
    }
    public ResumeBuilder WithSkills(SkillsSection skills)
    {
        _skills = skills;
        return this;
    }
    public ResumeBuilder AddQualification(Qualification qualification)
    {
        _qualifications ??= new QualificationsSection([]);
        _qualifications.AddQualification(qualification);
        return this;
    }
    public ResumeBuilder RemoveQualification(Qualification qualification)
    {
        _qualifications?.RemoveQualification(qualification);
        return this;
    }
    public ResumeBuilder AddJobExperience(JobExperience experience)
    {
        _experiences ??= new JobExperiencesSection([]);
        _experiences.AddJobExperience(experience);
        return this;
    }
    public ResumeBuilder RemoveJobExperience(JobExperience experience)
    {
        _experiences?.RemoveJobExperience(experience);
        return this;
    }
    public ResumeBuilder AddSkill(Skill skill)
    {
        _skills ??= new SkillsSection([]);
        _skills.AddSkill(skill);
        return this;
    }
    public ResumeBuilder RemoveSkill(Skill skill)
    {
        _skills?.RemoveSkill(skill);
        return this;
    }

    public Resume Build(
        string title,
        string description
    )
    {
        if(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Title and description cannot be null or empty.");

        ArgumentNullException.ThrowIfNull(_personalInfo, nameof(_personalInfo));
        ArgumentNullException.ThrowIfNull(_contactInfo, nameof(_contactInfo));
        ArgumentNullException.ThrowIfNull(_qualifications, nameof(_qualifications));
        ArgumentNullException.ThrowIfNull(_experiences, nameof(_experiences));
        ArgumentNullException.ThrowIfNull(_skills, nameof(_skills));

        return new Resume(
            title: title,
            description: description,
            personalInfo: _personalInfo,
            contactInfo: _contactInfo,
            qualifications: _qualifications,
            experiences: _experiences,
            skills: _skills
        );
    }

}
