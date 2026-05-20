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
        Student tahaStudent = new("Taha", "Ahmed", "male", "1994-08-14");
        Student kareemStudent = new("Kareem", "Ali", "male", "1994-08-14");

        Student ayaStudent = new("Aya", "Ali", "female", "2003-01-01");
        Student rababStudent = new("Rabab", "Sayed", "female", "1991-10-01");
        Student solimanStudent = new("Soliman", "Zakaria", "male", "1999-05-11");
        Student mohamedStudent = new("Mohamed", "Fawzy", "male", "1987-03-11");
        Student omarStudent = new("Omar", "Salah", "male", "1992-08-14");
        Student halaStudent = new("Hala", "Mohammed", "female", "1992-08-14");
        Student nadaStudent = new("Nada", "Yousry", "female", "1992-08-14");

        // enroll students to courses
        cSharpCourse.EnrollStudents([ramyStudent, adelStudent, gaberStudent, emanStudent, yassenStudent, tahaStudent, kareemStudent]);
        javaCourse.EnrollStudents([ayaStudent, rababStudent, solimanStudent, mohamedStudent, omarStudent, halaStudent, nadaStudent]);
        pythonCourse.EnrollStudents([ayaStudent, ramyStudent, gaberStudent, mohamedStudent, yassenStudent, kareemStudent, nadaStudent]);
        javascriptCourse.EnrollStudents([ramyStudent, rababStudent, yassenStudent, adelStudent, tahaStudent, halaStudent, kareemStudent]);
        nodeJsCourse.EnrollStudents([adelStudent, emanStudent, solimanStudent, tahaStudent, omarStudent, halaStudent, nadaStudent]);

        try
        {
            // assign scores to students in courses
            cSharpCourse.AssignStudentScore(ramyStudent.Id, 50.0);
            cSharpCourse.AssignStudentScore(adelStudent.Id, 70.0);
            cSharpCourse.AssignStudentScore(gaberStudent.Id, 80.0);
            cSharpCourse.AssignStudentScore(emanStudent.Id, 90.0);
            cSharpCourse.AssignStudentScore(yassenStudent.Id, 100.0);
            cSharpCourse.AssignStudentScore(tahaStudent.Id, 100.0);
            cSharpCourse.AssignStudentScore(kareemStudent.Id, 100.0);

            javaCourse.AssignStudentScore(ayaStudent.Id, 50.0);
            javaCourse.AssignStudentScore(rababStudent.Id, 70.0);
            javaCourse.AssignStudentScore(solimanStudent.Id, 80.0);
            javaCourse.AssignStudentScore(mohamedStudent.Id, 90.0);
            javaCourse.AssignStudentScore(omarStudent.Id, 100.0);
            javaCourse.AssignStudentScore(halaStudent.Id, 100.0);
            javaCourse.AssignStudentScore(nadaStudent.Id, 100.0);

            pythonCourse.AssignStudentScore(ayaStudent.Id, 50.0);
            pythonCourse.AssignStudentScore(ramyStudent.Id, 70.0);
            pythonCourse.AssignStudentScore(gaberStudent.Id, 80.0);
            pythonCourse.AssignStudentScore(mohamedStudent.Id, 90.0);
            pythonCourse.AssignStudentScore(yassenStudent.Id, 100.0);
            pythonCourse.AssignStudentScore(kareemStudent.Id, 100.0);
            pythonCourse.AssignStudentScore(nadaStudent.Id, 100.0);

            javascriptCourse.AssignStudentScore(ramyStudent.Id, 50.0);
            javascriptCourse.AssignStudentScore(rababStudent.Id, 70.0);
            javascriptCourse.AssignStudentScore(yassenStudent.Id, 80.0);
            javascriptCourse.AssignStudentScore(adelStudent.Id, 90.0);
            javascriptCourse.AssignStudentScore(tahaStudent.Id, 100.0);
            javascriptCourse.AssignStudentScore(halaStudent.Id, 100.0);
            javascriptCourse.AssignStudentScore(kareemStudent.Id, 100.0);

            nodeJsCourse.AssignStudentScore(adelStudent.Id, 50.0);
            nodeJsCourse.AssignStudentScore(emanStudent.Id, 70.0);
            nodeJsCourse.AssignStudentScore(solimanStudent.Id, 80.0);
            nodeJsCourse.AssignStudentScore(tahaStudent.Id, 90.0);
            nodeJsCourse.AssignStudentScore(omarStudent.Id, 100.0);
            nodeJsCourse.AssignStudentScore(halaStudent.Id, 100.0);
            nodeJsCourse.AssignStudentScore(nadaStudent.Id, 100.0);

            // mark students as completed courses
            cSharpCourse.MarkStudentCompleted(ramyStudent.Id);
            cSharpCourse.MarkStudentCompleted(adelStudent.Id);
            cSharpCourse.MarkStudentCompleted(gaberStudent.Id);
            cSharpCourse.MarkStudentCompleted(tahaStudent.Id);
            cSharpCourse.MarkStudentCompleted(kareemStudent.Id);

            javaCourse.MarkStudentCompleted(ayaStudent.Id);
            javaCourse.MarkStudentCompleted(rababStudent.Id);
            javaCourse.MarkStudentCompleted(solimanStudent.Id);
            javaCourse.MarkStudentCompleted(halaStudent.Id);
            javaCourse.MarkStudentCompleted(nadaStudent.Id);

            pythonCourse.MarkStudentCompleted(ayaStudent.Id);
            pythonCourse.MarkStudentCompleted(ramyStudent.Id);
            pythonCourse.MarkStudentCompleted(gaberStudent.Id);
            pythonCourse.MarkStudentCompleted(mohamedStudent.Id);
            pythonCourse.MarkStudentCompleted(nadaStudent.Id);

            javascriptCourse.MarkStudentCompleted(ramyStudent.Id);
            javascriptCourse.MarkStudentCompleted(rababStudent.Id);
            javascriptCourse.MarkStudentCompleted(yassenStudent.Id);
            javascriptCourse.MarkStudentCompleted(adelStudent.Id);
            javascriptCourse.MarkStudentCompleted(tahaStudent.Id);
            javascriptCourse.MarkStudentCompleted(halaStudent.Id);

            nodeJsCourse.MarkStudentCompleted(adelStudent.Id);
            nodeJsCourse.MarkStudentCompleted(emanStudent.Id);
            nodeJsCourse.MarkStudentCompleted(tahaStudent.Id);
            nodeJsCourse.MarkStudentCompleted(omarStudent.Id);
            nodeJsCourse.MarkStudentCompleted(halaStudent.Id);

            // exclude students from courses
            cSharpCourse.ExcludeStudent(emanStudent);
            cSharpCourse.ExcludeStudent(yassenStudent);

            javaCourse.ExcludeStudent(mohamedStudent);
            javaCourse.ExcludeStudent(omarStudent);

            pythonCourse.ExcludeStudent(yassenStudent);
            pythonCourse.ExcludeStudent(kareemStudent);

            javascriptCourse.ExcludeStudent(kareemStudent);

            nodeJsCourse.ExcludeStudent(solimanStudent);
            nodeJsCourse.ExcludeStudent(nadaStudent);

            // fill the students courses info
            ramyStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 50.0);
            adelStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 70.0);
            gaberStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 80.0);
            tahaStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 100.0);
            kareemStudent.CompleteCourse(cSharpCourse.Id, cSharpCourse.Title, 100.0);

            ayaStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 50.0);
            rababStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 70.0);
            solimanStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 80.0);
            halaStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 100.0);
            nadaStudent.CompleteCourse(javaCourse.Id, javaCourse.Title, 100.0);

            ayaStudent.CompleteCourse(pythonCourse.Id, pythonCourse.Title, 50.0);
            ramyStudent.CompleteCourse(pythonCourse.Id, pythonCourse.Title, 70.0);
            gaberStudent.CompleteCourse(pythonCourse.Id, pythonCourse.Title, 80.0);
            mohamedStudent.CompleteCourse(pythonCourse.Id, pythonCourse.Title, 100.0);
            nadaStudent.CompleteCourse(pythonCourse.Id, pythonCourse.Title, 100.0);

            adelStudent.CompleteCourse(javascriptCourse.Id, javascriptCourse.Title, 50.0);
            rababStudent.CompleteCourse(javascriptCourse.Id, javascriptCourse.Title, 70.0);
            yassenStudent.CompleteCourse(javascriptCourse.Id, javascriptCourse.Title, 80.0);
            tahaStudent.CompleteCourse(javascriptCourse.Id, javascriptCourse.Title, 100.0);
            halaStudent.CompleteCourse(javascriptCourse.Id, javascriptCourse.Title, 100.0);

            adelStudent.CompleteCourse(nodeJsCourse.Id, nodeJsCourse.Title, 77.0);
            emanStudent.CompleteCourse(nodeJsCourse.Id, nodeJsCourse.Title, 50.0);
            tahaStudent.CompleteCourse(nodeJsCourse.Id, nodeJsCourse.Title, 80.0);
            omarStudent.CompleteCourse(nodeJsCourse.Id, nodeJsCourse.Title, 100.0);
            halaStudent.CompleteCourse(nodeJsCourse.Id, nodeJsCourse.Title, 100.0);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

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

        Console.WriteLine("--------------------------");
        Console.WriteLine("Yassen Student:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(yassenStudent);
        Console.WriteLine($"({yassenStudent.CompletedCourseCount}) Completed Courses:");
        Console.WriteLine($"[{string.Join(", ", yassenStudent.CompletedCoursesInfo)}]");

        Console.WriteLine("--------------------------");
        Console.WriteLine("Nada Student:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(nadaStudent);
        Console.WriteLine($"({nadaStudent.CompletedCourseCount}) Completed Courses:");
        Console.WriteLine($"[{string.Join(", ", nadaStudent.CompletedCoursesInfo)}]");

        Console.WriteLine("--------------------------");
        Console.WriteLine("Adel Student:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(adelStudent);
        Console.WriteLine($"({adelStudent.CompletedCourseCount}) Completed Courses:");
        Console.WriteLine($"[{string.Join(", ", adelStudent.CompletedCoursesInfo)}]");

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

        Console.WriteLine("--------------------------");
        Console.WriteLine("Python Course:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(pythonCourse);
        Console.WriteLine($"({pythonCourse.EnrolledStudentsCount}) Enrolled Students:");
        Console.WriteLine($"[{string.Join(", ", pythonCourse.EnrolledStudentsInfo)}]");
        Console.WriteLine($"Max Score: {pythonCourse.MaxScore}");
        Console.WriteLine($"Min Score: {pythonCourse.MinScore}");
        Console.WriteLine($"Average Score: {pythonCourse.AverageScore:F2}");

        Console.WriteLine("--------------------------");
        Console.WriteLine("JavaScript Course:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(javascriptCourse);
        Console.WriteLine($"({javascriptCourse.EnrolledStudentsCount}) Enrolled Students:");
        Console.WriteLine($"[{string.Join(", ", javascriptCourse.EnrolledStudentsInfo)}]");
        Console.WriteLine($"Max Score: {javascriptCourse.MaxScore}");
        Console.WriteLine($"Min Score: {javascriptCourse.MinScore}");
        Console.WriteLine($"Average Score: {javascriptCourse.AverageScore:F2}");

        Console.WriteLine("--------------------------");
        Console.WriteLine("NodeJS Course:");
        Console.WriteLine("--------------------------");
        Console.WriteLine(nodeJsCourse);
        Console.WriteLine($"({nodeJsCourse.EnrolledStudentsCount}) Enrolled Students:");
        Console.WriteLine($"[{string.Join(", ", nodeJsCourse.EnrolledStudentsInfo)}]");
        Console.WriteLine($"Max Score: {nodeJsCourse.MaxScore}");
        Console.WriteLine($"Min Score: {nodeJsCourse.MinScore}");
        Console.WriteLine($"Average Score: {nodeJsCourse.AverageScore:F2}");

    }
}
