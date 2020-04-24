using cw3.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace cw3.DAL
{
    public class StudentDb : Controller,IDdService
    {
        private static IEnumerable<Models.Student> _students;
        private const string MyDatabase = "Data Source=db-mssql;Initial Catalog=s19278;Integrated Security=True;";
        private int nameid = 0;
        static StudentDb()
        {

        }
        public IConfiguration Configuration { get; set; }
        public StudentDb(IConfiguration configuration)
        {
            Configuration = configuration;
        }



        public IEnumerable<Student> GetStudents()
        {
            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand())
                {
                    _students = Enumerable.Empty<Student>();
                    command.Connection = client;
                    command.CommandText = "SELECT IndexNumber , FirstName,LastName , BirthDate,Semester , Name FROM Student,Enrollment,Studies where Student.IdEnrollment= Enrollment.IdEnrollment and Enrollment.idstudy=Studies.idstudy";

                    client.Open();
                    var qr = command.ExecuteReader();
                    List<Models.Student> st = new List<Models.Student>();
                    while (qr.Read())
                    {

                        var stud = new Student();
                        stud.IndexNumber = qr["IndexNumber"].ToString();
                        stud.FirstName = qr["FirstName"].ToString();
                        stud.LastName = qr["LastName"].ToString();
                        stud.BirthDate = DateTime.Parse(qr["BirthDate"].ToString());
                        stud.Semester = int.Parse(qr["Semester"].ToString());
                        stud.Name = qr["Name"].ToString();
                        st.Add(stud);

                    }

                    _students = st;


                }

                return _students;
            }
           
        
           
           
        }
        public IActionResult GetStudents(string id)
        {

            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand())
                {
                    _students = Enumerable.Empty<Student>();
                    command.Connection = client;
                    command.CommandText = "SELECT IndexNumber , FirstName,LastName , BirthDate,Semester , Name FROM Student,Enrollment,Studies where Student.IdEnrollment= Enrollment.IdEnrollment and Enrollment.idstudy=Studies.idstudy and Student.IndexNumber=@ind ";
                    command.Parameters.AddWithValue("ind", id);
                    client.Open();
                    var qr = command.ExecuteReader();

                    while (qr.Read())
                    {

                        var stud = new Student();
                        stud.IndexNumber = qr["IndexNumber"].ToString();
                        stud.FirstName = qr["FirstName"].ToString();
                        stud.LastName = qr["LastName"].ToString();
                        stud.BirthDate = DateTime.Parse(qr["BirthDate"].ToString());
                        stud.Semester = int.Parse(qr["Semester"].ToString());
                        stud.Name = qr["Name"].ToString();
                        return StatusCode(201, stud); ;
                    }




                }




                return NotFound();

            }
        }
        public IActionResult AddStudent(Models.EnrollStudClass enroll)
        {
            var st = new Models.Student();
            st.IndexNumber = enroll.IndexNumber;
            st.FirstName = enroll.FirstName;
            st.LastName = enroll.LastName;
            st.BirthDate = DateTime.Parse(enroll.BirthDate);
            
            using (var client = new SqlConnection(MyDatabase))
                using (var command = new SqlCommand())
                 {
                    command.Connection = client;
                    var enrollment = new Enrollment();
                    client.Open();
                    SqlTransaction tran = client.BeginTransaction();
                    try
                    {
                    
                    command.CommandText = "SELECT  Name FROM Studies where Name = @stud";
                    command.Parameters.AddWithValue("stud", enroll.Studies);
                    command.Parameters.AddWithValue("Fname", enroll.FirstName);
                    command.Parameters.AddWithValue("Lname", enroll.LastName);
                    command.Parameters.AddWithValue("Bdate", enroll.BirthDate);
                    command.Parameters.AddWithValue("id", enroll.IndexNumber);
                    command.Transaction = tran;
                    SqlDataReader qr = command.ExecuteReader();
                        while (!qr.Read())
                        {
                            tran.Rollback();
                            return NotFound(400 + " - Nie ma takich studiów");

                        }
                        qr.Close();
                        command.CommandText = "SELECT * FROM Enrollment inner join studies on Studies.IdStudy = Enrollment.IdStudy where Studies.Name = @stud and semester = 1";
                        qr = command.ExecuteReader();
                        while (!qr.HasRows)
                        {
                            qr.Close();
                            command.CommandText = "INSERT into enrollment(idEnrollment,semester,idstudy,startdate)values((select Max(idEnrollment)+1 from enrollment),1,(select idstudy from studies where name=@stud),GETDATE())";
                            command.ExecuteNonQuery();
                        
                        tran.Commit();
                        }
                        qr.Close();
                    command.CommandText = "select * from student where IndexNumber=@id";
                    
                    qr = command.ExecuteReader();
                        if (!qr.HasRows)
                        {
                            qr.Close();
                            command.CommandText = "insert into Student(IndexNumber,FirstName,LastName,BirthDate,IdEnrollment)values(@id,@Fname,@Lname, @Bdate,(select idEnrollment from enrollment inner join studies on enrollment.IdStudy=studies.IdStudy where studies.Name=@stud and semester=1))";
                            command.ExecuteNonQuery();
                       
                        }
                        else
                        {
                            tran.Rollback();
                            return NotFound("Id nie jest unikalne");
                        }
                         qr.Close();
                        

                        command.CommandText = "select IdEnrollment,StartDate,Semester from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where name=@stud and semester= 1";
                        qr = command.ExecuteReader();
                        if (qr.Read())
                        {
                        
                        enrollment.idEnrollment = qr["idEnrollment"].ToString();
                        enrollment.Semester = qr["Semester"].ToString();
                        enrollment.StartDate = qr["StartDate"].ToString();
                        }
                        qr.Close();
                    tran.Commit();
                }
                    catch(SqlException ex){
                        tran.Rollback();
                    }




                return StatusCode(201, enrollment);
            }


            
            
        }

        public IActionResult Promote(PromoteModel enroll)
        {
            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand())
                {
                    var enrollment = new Enrollment();
                    command.Connection = client;
                    command.CommandText = "SELECT * from Enrollment inner join studies on Studies.idstudy = Enrollment.idstudy where Semester = @sem and Studies.name = @stud";   
                    command.Parameters.AddWithValue("stud", enroll.Studies);
                    command.Parameters.AddWithValue("sem", enroll.Semester);
                    client.Open();
                    var qr = command.ExecuteReader();
                    if (qr.HasRows)
                    {
                        qr.Close();
                        command.CommandText = "exec Promote @stud, @sem";
                        command.ExecuteNonQuery();
                        qr.Close();
                        command.CommandText = "select IdEnrollment, StartDate, Semester from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where  semester= @sem + 1 and name=@stud";
                        qr = command.ExecuteReader();
                        if (qr.Read())
                        {

                            enrollment.idEnrollment = qr["idEnrollment"].ToString();
                            enrollment.Semester = qr["Semester"].ToString();
                            enrollment.StartDate = qr["StartDate"].ToString();
                        }
                        qr.Close();
                        return StatusCode(201, enrollment);
                    }
                    else
                    {
                        return NotFound();
                    }

                    


                }


            }
        }

        public bool CheckIndex(string index)
        {
            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand())
                {
                   
                    command.Connection = client;
                    command.CommandText = "SELECT IndexNumber FROM Student where Student.IndexNumber=@ind ";
                    command.Parameters.AddWithValue("ind", index);
                    client.Open();
                    var qr = command.ExecuteReader();

                    while (qr.Read())
                    {

                        return true ;
                    }




                }

                return false;

            }
        }

        public IActionResult Singin(Singin singin)
        {
            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand())
                {

                    command.Connection = client;
                    command.CommandText = "SELECT * FROM Student where Student.IndexNumber=@ind ";
                    command.Parameters.AddWithValue("ind", singin.Login);
                    command.Parameters.AddWithValue("pass", singin.Haslo);
                    client.Open();
                    var qr = command.ExecuteReader();

                    while (qr.Read())
                    {
                        var pass = qr["Password"].ToString();
                        var salt = qr["Salt"].ToString();
                        if (HashedPass(singin.Haslo, salt) == pass)
                        {
                            qr.Close();
                            var refreshToken = Guid.NewGuid();
                            command.CommandText = "UPDATE Student SET Refreshtkn =@tkn where Student.IndexNumber=@ind ";
                            command.Parameters.AddWithValue("tkn", refreshToken);
                            
                            command.ExecuteNonQuery();
                            nameid++;
                            var claims = new[]{
                            new Claim(ClaimTypes.NameIdentifier, ""+nameid),
                            new Claim(ClaimTypes.Name, singin.Login),
                            new Claim(ClaimTypes.Role, ""),
                            new Claim(ClaimTypes.Role, "student")
                            };

                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                            var token = new JwtSecurityToken
                        (
                            issuer: "Gakko",
                            audience: "Students",
                            claims: claims,
                            expires: DateTime.Now.AddSeconds(10),
                            signingCredentials: creds
                        );
                            Console.WriteLine(refreshToken);
                            return Ok(new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(token),
                                refreshToken
                            });

                        }
                    }



                }


            }
            return Unauthorized();
            
        }

        public IActionResult Refresh(string rtoken)
        {
            using (var client = new SqlConnection(MyDatabase))
            {
                using (var command = new SqlCommand())
                {

                    command.Connection = client;
                    command.CommandText = "SELECT * FROM Student where Student.Refreshtkn =@token ";
                    command.Parameters.AddWithValue("token",rtoken);
                    
                    client.Open();
                    var qr = command.ExecuteReader();

                    while (qr.Read())
                    {
                        var id = qr["IndexNumber"].ToString();
                        qr.Close();
                        var refreshToken = Guid.NewGuid();
                        command.CommandText = "UPDATE Student SET Refreshtkn =@tkn where Student.IndexNumber=@ind ";
                        command.Parameters.AddWithValue("tkn", refreshToken);
                        command.Parameters.AddWithValue("ind", id);
                        command.ExecuteNonQuery();
                        nameid++;
                        var claims = new[]{
                            new Claim(ClaimTypes.NameIdentifier, ""+nameid),
                            new Claim(ClaimTypes.Name, id),
                            new Claim(ClaimTypes.Role, ""),
                            new Claim(ClaimTypes.Role, "student")
                            };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken
                    (
                        issuer: "Gakko",
                        audience: "Students",
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(10),
                        signingCredentials: creds
                    );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            refreshToken
                        });

                    }




                }


            }
            return Unauthorized();

        }
        public static string HashedPass(string haslo, string salt)
        {
            var haslohash = KeyDerivation.Pbkdf2(
                                password: haslo,
                                salt: Encoding.UTF8.GetBytes(salt),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 20000,
                                numBytesRequested: 256 / 8);
            return Convert.ToBase64String(haslohash);

        }
    }
   
}

