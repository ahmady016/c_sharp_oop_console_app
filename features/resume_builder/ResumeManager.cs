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
namespace ResumesBuilder;

public static class ResumeManager
{
    private static Resume BuildResume()
    {
        return new ResumeBuilder()
            .WithPersonalInfo(new PersonalSection(
                fullName: "John Doe",
                birthDate: "1990-01-01",
                gender: "male",
                nationality: "United States",
                nationalIdNumber: "12345678901114",
                maritalStatus: MaritalStatus.Single
            ))
            .WithContactInfo(new ContactSection(
                email: "6dNlq@example.com",
                address: "123 Main St, Los Angeles, USA",
                phoneNumber: "123-456-7890",
                linkedInProfileUrl: "https://www.linkedin.com/in/johndoe256"
            ))
            .WithSkills(new SkillsSection(
                [
                    new Skill(
                        title: "C#",
                        description: "Proficient in C# programming language.",
                        proficiencyLevel: 7.3
                    ),
                    new Skill(
                        title: "ASP.NET Core",
                        description: "Experienced in ASP.NET Core development.",
                        proficiencyLevel: 4.5
                    ),
                    new Skill(
                        title: "SQL Server",
                        description: "Skilled in SQL Server database management.",
                        proficiencyLevel: 8.2
                    )
                ]
            ))
            .WithQualifications(new QualificationsSection(
                [new Qualification(
                    degree: "Bachelor of Science in Computer Science",
                    institution: "University of Example",
                    graduationYear: 2012,
                    grade: 75
                )]
            ))
            .WithExperiences(new JobExperiencesSection(
                [new JobExperience(
                    jobTitle: "Software Engineer",
                    companyName: "ABC Company",
                    startDate: "2012-06-01",
                    endDate: "2018-08-31",
                    achievements: $"""
                        "Developed and maintained web applications using C# and ASP.NET Core.",
                        "Collaborated with cross-functional teams to design and implement new features.",
                        "Optimized application performance, resulting in a 20% reduction in load times."
                    """
                )]
            ))
        .Build(
            title: "John Doe Resume",
            description: "Experienced software engineer with a strong background in C# and ASP.NET Core."
        );
    }
    public static void Run()
    {
        Helpers.PrintHeader("Start of Resumes Builder App");
        try
        {
            var resume = BuildResume();
            Helpers.PrintSuccess("John Doe Resume are built successfully!");
            Console.WriteLine();
            Console.WriteLine(resume.Render());
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
