using cw3.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace cw3.DAL
{
    public class StudentDb : IDdService
    {
        private static IEnumerable<Models.Student> _students;
        private const string MyDatabase = "Data Source=db-mssql;Initial Catalog=s19278;Integrated Security=True";

        static StudentDb(){
            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand()) {

                    command.Connection=client;
                    command.CommandText = "SELECT IndexNumber , FirstName,LastName , BirthDate,Semester , Name FROM Student,Enrollment,Studies where Student.IdEnrollment= Enrollment.IdEnrollment and Enrollment.idstudy=Studies.idstudy";

                    client.Open();
                    var qr = command.ExecuteReader();
                    List<Models.Student> st = new List<Models.Student>();
                    while (qr.Read()) 
                    {

                        var stud = new Student();
                        stud.IndexNumber = int.Parse(qr["IndexNumber"].ToString());
                        stud.FirstName = qr["FirstName"].ToString();
                        stud.LastName = qr["LastName"].ToString();
                        stud.BirthDate = DateTime.Parse(qr["BirthDate"].ToString());
                        stud.Semester = int.Parse(qr["Semester"].ToString());
                        stud.Name = qr["Name"].ToString();
                        st.Add(stud);

                    }

                    _students = st;


                }


            }

        }
        
        
        
        public IEnumerable<Student> GetStudents()
        {
            return _students;
           
        }
        public Student GetStudents(int id)
        {
            List<Student> st = _students.ToList();
            var response = st.Find(r => r.IndexNumber == id);
            return response;

        }
    }
}
