/*
// Resume Builder App to demonstrate [the favor of composition over multiple inheritance]
// ______________________________________________________________________________________

Why composition beats multiple inheritance here
===============================================
    The problem multiple inheritance would create:
    if Resume tried to inherit from PersonalInfo, ContactInfo, Qualification, and JobExperience simultaneously,
    it would carry all of their properties as part of its own identity
    and a resume would be a person, be a contact, be one job.
    That is semantically wrong. A resume has all of those things,
    which is exactly what composition models.

The five design decisions worth understanding
===============================================
// 1. IResumeSection as the contract so each section implements the interface
    and can be rendered in a consistent way.
    Resume.Render() never imports PersonalInfo or JobExperience
    it loops over IResumeSection and calls Render(), Polymorphism handles the rest.
    This means you can add a SkillsSection, LanguagesSection,
    or CertificationsSection record — implementing the same interface
    and the resume renders it automatically with zero changes to Resume.
// 2. Each section class (PersonalInfoSection, ContactInfoSection, etc.)
    is focused on a single responsibility and encapsulates its own data and behavior.
    This makes the code easier to maintain and extend
    and with respect to section that represents a list of items like JobExperienceSection,
    the section manages its own collection of job experiences,
    allowing for adding, removing, and rendering them without affecting other sections.
// 3. IsEmpty in the contract allows Resume to determine if a section has content to render,
    so we can skips blank sections cleanly. Without this,
    every render method would need null checks scattered everywhere.
    Pushing the emptiness concern into the section itself keeps Resume free of that logic.
// 4. Each section class respects immutability (Value Object),
    using private readonly fields sets only in the constructor and exposing getter only properties.
    so Resume doesn't need to worry about accidentally modifying its sections.
// 5. Fluent builder as the construction API.
    The builder accumulates sections and validates required fields before calling Build().
    A caller can never produce a Resume with a missing required section,
    so the guard is at the construction site, not scattered across every consumer.
*/
using Bogus;

namespace ResumesBuilder;

public static class ResumeManager
{
    private static readonly Faker _faker = new();
    private static PersonalSection PersonalInfo
    {
        get
        {
            var gender = _faker.Person.Gender;
            return new(
                fullName: _faker.Name.FullName(gender),
                birthDate: _faker.Date
                    .BetweenDateOnly(DateOnly.Parse("1988-01-01"), DateOnly.Parse("2004-12-31"))
                    .ToString("yyyy-MM-dd"),
                gender: gender.ToString().ToLower(),
                nationality: _faker.Address.Country().ToLower(),
                nationalIdNumber: _faker.Phone.PhoneNumber("##############"),
                maritalStatus: _faker.PickRandom<MaritalStatus>()
            );
        }
    }
    private static ContactSection ContactInfo => new(
        email: _faker.Internet.Email(),
        address: _faker.Address.FullAddress(),
        phoneNumber: _faker.Phone.PhoneNumber("###########"),
        linkedInProfileUrl: _faker.Internet.Url()
    );
    private static Qualification Qualification => new(
        degree: _faker.PickRandom<SchoolDegree>().ToString(),
        institution: _faker.Company.CompanyName(),
        graduationYear: _faker.Random.Int(2008, 2025),
        grade: _faker.Random.Double(17.0, 97.99)
    );
    private static QualificationsSection Qualifications => new(
        [Qualification, Qualification, Qualification]
    );
    private static Skill Skill => new(
        title: _faker.Lorem.Word(),
        description: _faker.Lorem.Sentence(),
        proficiencyLevel: _faker.Random.Double(2.4, 9.5)
    );
    private static SkillsSection Skills => new(
        [Skill, Skill, Skill]
    );
    private static JobExperience JobExperience => new(
        jobTitle: _faker.Name.JobTitle(),
        companyName: _faker.Company.CompanyName(),
        achievements: _faker.Lorem.Sentences(_faker.Random.Int(3, 7)),
        startDate: _faker.Date
            .BetweenDateOnly(DateOnly.Parse("2022-01-01"), DateOnly.Parse("2023-12-31"))
            .ToString("yyyy-MM-dd"),
        endDate: _faker.PickRandom(_faker.Date
            .BetweenDateOnly(DateOnly.Parse("2022-01-01"), DateOnly.Parse("2023-12-31"))
            .ToString("yyyy-MM-dd"),
            null
        )
    );
    private static JobExperiencesSection JobExperiences => new(
        [JobExperience, JobExperience, JobExperience]
    );
    private static Resume BuildResume()
    {
        return new ResumeBuilder()
            .WithPersonalInfo(PersonalInfo)
            .WithContactInfo(ContactInfo)
            .WithSkills(Skills)
            .WithQualifications(Qualifications)
            .WithExperiences(JobExperiences)
        .Build(
            title: _faker.Name.FullName(),
            description: _faker.Lorem.Sentence()
        );
    }

    public static void Run()
    {
        Helpers.PrintHeader("Start of Resumes Builder App");
        try
        {
            var resume01 = BuildResume();
            var resume02 = BuildResume();
            var resume03 = BuildResume();
            Helpers.PrintSuccess("3 random Resumes are generated and built successfully!");
            Console.WriteLine("------------------------------");
            foreach (var resume in (List<Resume>)[resume01, resume02, resume03])
            {
                Console.ForegroundColor = Helpers.GetRandomConsoleColor();
                Console.WriteLine(resume);
                Console.WriteLine("------------------------------");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintError($"An error occurred: {ex.Message}");
        }
        finally
        {
            Helpers.PrintFooter("End of Resumes Builder App");
        }
    }
}
