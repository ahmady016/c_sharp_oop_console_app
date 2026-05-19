namespace CourseManagement;

public record CourseStudent(
    Student Student,
    double Score,
    bool Completed
);

public class Course
{
    private readonly string _id;
    private readonly string _title;
    private readonly string _description;
    private readonly int _totalHours;
    private readonly Dictionary<string, CourseStudent> _enrolledStudents = [];
    private Instructor? _instructor = null;

    public string Id => _id;
    public string Title => _title;
    public string Description => _description;
    public int TotalHours => _totalHours;
    public IReadOnlyDictionary<string, CourseStudent> EnrolledStudents => _enrolledStudents;
    public Instructor? Instructor => _instructor;

    public int EnrolledStudentsCount => _enrolledStudents.Count;

    public IReadOnlyList<string> EnrolledStudentsInfo =>
        [..
            from courseStudent in _enrolledStudents.Values
            select $"{courseStudent.Student.FullName} has {(courseStudent.Completed ? "completed" : "not completed")} the course with {courseStudent.Score} score"
        ];

    private IReadOnlyList<double> Scores =>
        [..
            from courseStudent in _enrolledStudents.Values
            where courseStudent.Completed
            select courseStudent.Score
        ];
    public double AverageScore => Scores.Average();
    public double MaxScore => Scores.Max();
    public double MinScore => Scores.Min();

    public Course(
        string title,
        string description,
        int totalHours = 24
    )
    {
        if(string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            throw new ArgumentException("Invalid title or description.");
        if(totalHours <= 12)
            throw new ArgumentException("Invalid total hours.");

        _id = Helpers.GenerateId();
        _title = title;
        _description = description;
        _totalHours = totalHours;
    }
    public void AssignInstructor(Instructor instructor) => _instructor = instructor;

    public void EnrollStudent(Student student)
    {
        if(_enrolledStudents.ContainsKey(student.Id))
            throw new ArgumentException($"student with Id {student.Id} is already enrolled in this course.");
        _enrolledStudents.Add(student.Id, new CourseStudent(student, 0.0, false));
    }
    public void EnrollStudents(List<Student> students) => students.ForEach(EnrollStudent);

    public void ExcludeStudent(Student student)
    {
        if(!_enrolledStudents.ContainsKey(student.Id))
            throw new ArgumentException($"student with Id {student.Id} is not enrolled in this course.");
        _enrolledStudents.Remove(student.Id);
    }
    public void ExcludeStudents(List<Student> students) => students.ForEach(ExcludeStudent);

    public void AssignStudentScore(string studentId, double score)
    {
        if(score < 0.0 || score > 100.0)
            throw new ArgumentException("Invalid student score.");

        if(!_enrolledStudents.TryGetValue(studentId, out CourseStudent? value))
            throw new ArgumentException($"student with Id {studentId} is not enrolled in this course.");

        _enrolledStudents[studentId] = value with { Score = score };
    }

    public void MarkStudentCompleted(string studentId)
    {
        if(!_enrolledStudents.TryGetValue(studentId, out CourseStudent? value))
            throw new ArgumentException($"student with Id {studentId} is not enrolled in this course.");

        _enrolledStudents[studentId] = value with { Completed = true };
    }

    public override string ToString() => $"{Title} - {Description} - {TotalHours} hours";
    public override int GetHashCode() => HashCode.Combine(Title, Description, TotalHours);
    public override bool Equals(object? obj)
    {
        if(obj is Course course)
            return course.Title == Title && course.Description == Description && course.TotalHours == TotalHours;
        return false;
    }

}
