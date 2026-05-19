namespace CourseManagement;
// # build an online course management program using classes:
// ----------------------------------------------------------
// admin can track courses, instructors, students and their grades
// it allow admin to see course details
// and assign an instructor to a course
// and enroll students to a course
// and assign a score to a student in a course
// and calculate the average scores of enrolled students in a course
// and mark a student as completed a course
// ###########################################################
public static class CourseManager
{
    public static void Run()
    {
        Console.WriteLine("-----------------------");
        Console.WriteLine("Welcome to Course Manager App");
        Console.WriteLine("-----------------------");

        // create some instructors
        Instructor omar = new("Omar", "Salah", "male", "1999-01-01");
        Instructor ahmed = new("Ahmed", "Ali", "male", "1988-10-01");
        Instructor sayed = new("Sayed", "Gaber", "male", "1965-05-11");
        Instructor mohamed = new("Mohamed", "Fawzy", "male", "1977-03-11");
        Instructor Osama = new("Osama", "Hessen", "male", "1992-08-14");

        // create some courses
        Course cSharpCourse = new("C# Programming", "Learn C# Programming from scratch", 72);
        Course javaCourse = new("Java Programming", "Learn Java Programming from scratch", 90);
        Course pythonCourse = new("Python Programming", "Learn Python Programming from scratch", 60);
        Course javascriptCourse = new("JavaScript Programming", "Learn JavaScript Programming from scratch", 48);
        Course nodeJsCourse = new("Node.js Programming", "Learn Node.js Programming from scratch", 60);

        // assign instructors to courses
        cSharpCourse.AssignInstructor(omar);
        javaCourse.AssignInstructor(ahmed);
        pythonCourse.AssignInstructor(sayed);
        javascriptCourse.AssignInstructor(mohamed);
        nodeJsCourse.AssignInstructor(Osama);

        // create some students
        Student ramyStudent = new("Ramy", "Ali", "male", "2000-01-01");
        Student adelStudent = new("Adel", "Zaki", "male", "1997-10-01");
        Student gaberStudent = new("Gaber", "Sayed", "male", "2001-05-11");
        Student emanStudent = new("Eman", "Mohamed", "female", "2004-03-11");
        Student yassenStudent = new("Yassen", "Hessen", "male", "1994-08-14");
        Student ayaStudent = new("Aya", "Ali", "female", "2003-01-01");
        Student rababStudent = new("Rabab", "Sayed", "female", "1991-10-01");
        Student solimanStudent = new("Soliman", "Zakaria", "male", "1999-05-11");
        Student mohamedStudent = new("Mohamed", "Fawzy", "male", "1987-03-11");
        Student omarStudent = new("Omar", "Salah", "male", "1992-08-14");

        // enroll students to courses
        cSharpCourse.EnrollStudents([ramyStudent, adelStudent, gaberStudent, emanStudent, yassenStudent]);
        javaCourse.EnrollStudents([ayaStudent, rababStudent, solimanStudent, mohamedStudent, omarStudent]);

        // assign scores to students in courses
        cSharpCourse.AssignStudentScore(ramyStudent.Id, 50.0);
        cSharpCourse.AssignStudentScore(adelStudent.Id, 70.0);
        cSharpCourse.AssignStudentScore(gaberStudent.Id, 80.0);
        cSharpCourse.AssignStudentScore(emanStudent.Id, 90.0);
        cSharpCourse.AssignStudentScore(yassenStudent.Id, 100.0);

        javaCourse.AssignStudentScore(ayaStudent.Id, 50.0);
        javaCourse.AssignStudentScore(rababStudent.Id, 70.0);
        javaCourse.AssignStudentScore(solimanStudent.Id, 80.0);
        javaCourse.AssignStudentScore(mohamedStudent.Id, 90.0);
        javaCourse.AssignStudentScore(omarStudent.Id, 100.0);

        // mark students as completed courses
        cSharpCourse.MarkStudentCompleted(ramyStudent.Id);
        cSharpCourse.MarkStudentCompleted(adelStudent.Id);
        cSharpCourse.MarkStudentCompleted(gaberStudent.Id);

        javaCourse.MarkStudentCompleted(ayaStudent.Id);
        javaCourse.MarkStudentCompleted(rababStudent.Id);
        javaCourse.MarkStudentCompleted(solimanStudent.Id);

        // exclude students from courses
        cSharpCourse.ExcludeStudent(emanStudent);
        cSharpCourse.ExcludeStudent(yassenStudent);

        javaCourse.ExcludeStudent(mohamedStudent);
        javaCourse.ExcludeStudent(omarStudent);

        // fill the students courses info
        ramyStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 50.0);
        adelStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 70.0);
        gaberStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 80.0);

        ayaStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 50.0);
        rababStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 70.0);
        solimanStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 80.0);

        // print student info
        Console.WriteLine("--------------------------");
        Console.WriteLine("Ramy Student:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(ramyStudent);
        Console.WriteLine($"({ramyStudent.CompletedCourseCount}) Completed Courses:");
        Console.WriteLine($"[{string.Join(", ", ramyStudent.CompletedCoursesInfo)}]");

        Console.WriteLine("--------------------------");
        Console.WriteLine("Rabab Student:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(rababStudent);
        Console.WriteLine($"({rababStudent.CompletedCourseCount}) Completed Courses:");
        Console.WriteLine($"[{string.Join(", ", rababStudent.CompletedCoursesInfo)}]");

        // print course details
        Console.WriteLine("--------------------------");
        Console.WriteLine("C Sharp Course:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(cSharpCourse);
        Console.WriteLine($"({cSharpCourse.EnrolledStudentsCount}) Enrolled Students:");
        Console.WriteLine($"[{string.Join(", ", cSharpCourse.EnrolledStudentsInfo)}]");
        Console.WriteLine($"Max Score: {cSharpCourse.MaxScore}");
        Console.WriteLine($"Min Score: {cSharpCourse.MinScore}");
        Console.WriteLine($"Average Score: {cSharpCourse.AverageScore:F2}");

        Console.WriteLine("--------------------------");
        Console.WriteLine("Java Course:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(javaCourse);
        Console.WriteLine($"({javaCourse.EnrolledStudentsCount}) Enrolled Students:");
        Console.WriteLine($"[{string.Join(", ", javaCourse.EnrolledStudentsInfo)}]");
        Console.WriteLine($"Max Score: {javaCourse.MaxScore}");
        Console.WriteLine($"Min Score: {javaCourse.MinScore}");
        Console.WriteLine($"Average Score: {javaCourse.AverageScore:F2}");

    }
}
