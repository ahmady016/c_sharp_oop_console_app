namespace CourseManagement;

public record CompletedCourse(
    string Title,
    double Score,
    string Grade,
    DateOnly CompletedAt
);

public class Student : Person
{
    private readonly Dictionary<string, CompletedCourse> _completedCourses = [];
    public int CompletedCourseCount => _completedCourses.Count;
    public IReadOnlyDictionary<string, CompletedCourse> CompletedCourses => _completedCourses;

    public IReadOnlyList<string> CompletedCoursesInfo =>
    [..
        from course in _completedCourses
        select $"{course.Value.Title}, {course.Value.Score}, ({course.Value.Grade}), {course.Value.CompletedAt}"
    ];

    public void CompleteCourse(string courseId, string courseTitle, double score)
    {
        if(score < 0.0 || score > 100.0)
            throw new ArgumentException("Invalid score.");
        if(_completedCourses.ContainsKey(courseId))
            throw new ArgumentException($"{FullName} has already completed {courseTitle}.");

        string grade = score switch
        {
            >= 95 => "A+",
            >= 90 => "A",
            >= 85 => "A-",
            >= 80 => "B+",
            >= 76 => "B",
            >= 73 => "B-",
            >= 70 => "C+",
            >= 67 => "C",
            >= 65 => "C-",
            >= 62 => "D+",
            >= 60 => "D",
            >= 50 => "D-",
            _ => "F"
        };
        _completedCourses.Add(
            courseId,
            new CompletedCourse(courseTitle, score, grade, DateOnly.FromDateTime(DateTime.Now))
        );
    }

    public void UnCompleteCourse(string courseId)
    {
        if(!_completedCourses.ContainsKey(courseId))
            throw new ArgumentException($"Course with Id {courseId} has not been completed.");
        _completedCourses.Remove(courseId);
    }

    public Student(string firstName, string lastName) : base(firstName, lastName) { }
    public Student(
        string firstName,
        string lastName,
        string gender = "male",
        string birthDate = "2000-01-01"
    ) : base(firstName, lastName, gender, birthDate) { }
}
