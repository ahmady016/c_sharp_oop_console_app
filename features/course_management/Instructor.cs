namespace CourseManagement;

public class Instructor : Person
{
    private readonly List<Course> _courses = [];
    public IReadOnlyList<Course> Courses => _courses;

    public void AssignCourse(Course course) => _courses.Add(course);
    public void AssignCourses(List<Course> courses) => _courses.AddRange(courses);
    public void UnassignCourse(Course course) => _courses.Remove(course);
    public void UnassignCourses(List<Course> courses) => _courses.RemoveAll(courses.Contains);

    public Instructor(string firstName, string lastName) : base(firstName, lastName) { }
    public Instructor(
        string firstName,
        string lastName,
        string gender = "male",
        string birthDate = "2000-01-01") : base(firstName, lastName, gender, birthDate) { }
}
