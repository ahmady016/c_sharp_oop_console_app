#region Resume Builder App Description
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
#endregion

using Bogus;

namespace ResumesBuilder;

public static class ResumeManager
{
    private static readonly Faker _faker = new();
    private static readonly string DATA_DIRECTORY = Path.Combine(Helpers.SameDirectory(), "data");
    private static Bogus.DataSets.Name.Gender _gender = default;
    private static string _fullName = default!;
    private static int ResumesCount => _faker.Random.Int(5, 10);
    private static List<Resume> _resumes = [];
    private static List<string> _resumesFilesPaths = [];
    private static PersonalSection PersonalInfo
    {
        get
        {
            _gender = _faker.Person.Gender;
            _fullName = _faker.Name.FullName(_gender);
            return new(
                fullName: _fullName,
                birthDate: _faker.Date
                    .BetweenDateOnly(DateOnly.Parse("1988-01-01"), DateOnly.Parse("2004-12-31"))
                    .ToString("yyyy-MM-dd"),
                gender: _gender.ToString().ToLower(),
                nationality: _faker.Address.Country().ToLower(),
                nationalIdNumber: _faker.Phone.PhoneNumber("##############"),
                maritalStatus: _faker.PickRandom<MaritalStatus>().ToString()
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
        grade: $"{_faker.Random.Double(17.0, 97.99)}%"
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
            title: _fullName,
            description: _faker.Lorem.Sentences(_faker.Random.Int(3, 5))
        );
    }

    private static async Task ReadResumesFromJsonFiles()
    {
        foreach (var filePath in _resumesFilesPaths)
            _resumes.Add(await Helpers.ReadFromJsonFileAsync<Resume>(filePath));
    }
    private static List<Resume> GenerateRandomResumes(int count = 3)
    {
        if(count < 1 || count > 100)
            throw new ArgumentOutOfRangeException(nameof(count));
        return [..from _ in Enumerable.Range(0, count) select BuildResume()];
    }
    private static async Task WriteResumesToJsonFiles(List<Resume> resumes)
    {
        foreach (var resume in resumes)
        {
            string resumeFileName = $"{resume.Title.ToLower().Replace(' ', '_')}.json";
            await Helpers.WriteToJsonFileAsync(
                filePath: Path.Combine(DATA_DIRECTORY, resumeFileName),
                data: resume
            );
        }
    }
    private static void PrintResumes(List<Resume> resumes)
    {
        foreach (var resume in resumes)
        {
            Console.ForegroundColor = Helpers.GetRandomConsoleColor();
            Console.WriteLine(resume);
            Console.WriteLine("------------------------------");
            Console.ResetColor();
        }
    }

    public static async Task Run()
    {
        Helpers.PrintHeader("Start of Resumes Builder App");
        try
        {
            _resumesFilesPaths = Helpers.GetJsonFilesPaths(DATA_DIRECTORY);
            var _resumesCount = _resumesFilesPaths.Count == 0
                ?  ResumesCount
                : _resumesFilesPaths.Count;
            if(_resumesFilesPaths.Count == 0)
            {
                Helpers.PrintSuccess($"({_resumesCount}) random Resumes are about to be generated!");
                _resumes = GenerateRandomResumes(_resumesCount);
                Helpers.PrintSuccess($"({_resumesCount}) random Resumes are generated successfully!");
                Console.WriteLine("------------------------------");

                Helpers.PrintSuccess($"({_resumesCount}) random Resumes are about to start writing to JSON files!");
                await WriteResumesToJsonFiles(_resumes);
                Helpers.PrintSuccess($"({_resumesCount}) random Resumes are written to JSON files successfully!");
                Console.WriteLine("------------------------------");
            }
            else
            {
                Helpers.PrintSuccess($"({_resumesCount}) random Resumes are about to be loaded from JSON files!");
                await ReadResumesFromJsonFiles();
                Helpers.PrintSuccess($"({_resumesCount}) random Resumes are loaded from JSON files successfully!");
                Console.WriteLine("------------------------------");
            }
            Helpers.PrintSuccess($"({_resumesCount}) random Resumes are about to be printed!");
            PrintResumes(_resumes);
            Helpers.PrintSuccess($"({_resumesCount}) random Resumes are printed successfully!");
            Console.WriteLine("------------------------------");
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
