using cw3.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class StudentDbEntity : Controller, IDdService
    {
        private static IEnumerable<Models.Student> _students;
        private const string MyDatabase = "Data Source=db-mssql;Initial Catalog=s19278;Integrated Security=True;";
        private int nameid = 0;
        static StudentDbEntity()
        {

        }
        public IConfiguration Configuration { get; set; }
        public StudentDbEntity(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IActionResult GetStudents()
        {
            var db = new Models2.s19278Context();
            var st = db.Student.ToList();
            if (!st.Any())
            {
                return NotFound("W bazie nie ma studentów");
            }
            db.Student.Remove(st.FirstOrDefault());
            db.SaveChanges();

            return Ok(st);
        }

        public IActionResult GetStudents(string index)
        {
            throw new NotImplementedException();
        }

        public IActionResult AddStudent(EnrollStudClass enroll)
        {
            var db = new Models2.s19278Context();
            var st = db.Studies.Where(e => e.Name == enroll.Studies);

            if (!st.Any())
            {
                return NotFound("W bazie nie ma takich studiów");
            }
            var en = db.Enrollment.Join(db.Studies, k => k.IdStudy,
                                 e => e.IdStudy,
                                 (k, e) => new { Enrollment = k, Studies = e })
                                .Where(e => e.Enrollment.Semester == 1 && e.Studies.Name == enroll.Studies);
            if (!en.Any())
            {
                var enrolst = db.Studies.Where(e => e.Name == enroll.Studies).Select(e => e.IdStudy);
                db.Enrollment.Add(new Models2.Enrollment
                {
                    IdEnrollment = db.Enrollment.Max(e => e.IdEnrollment) + 1,
                    Semester = 1,
                    IdStudy = enrolst.FirstOrDefault(),
                    StartDate = DateTime.Now

                });    
            }
            var std = db.Student.Where(e => e.IndexNumber == enroll.IndexNumber);
            if (!std.Any())
            {
                db.Student.Add(new Models2.Student
                {
                    IndexNumber = enroll.IndexNumber,
                    FirstName = enroll.FirstName,
                    LastName = enroll.LastName,
                    BirthDate = DateTime.Parse(enroll.BirthDate),
                    IdEnrollment = db.Enrollment.Join(db.Studies, k => k.IdStudy,
                                 e => e.IdStudy,
                                 (k, e) => new { Enrollment = k, Studies = e })
                                .Where(e => e.Enrollment.Semester == 1 && e.Studies.Name == enroll.Studies).Select(e => e.Enrollment.IdEnrollment).FirstOrDefault()


                });
            }
            else {
                return NotFound("Id studenta nie jest unikalne");
            }
            var enrollment = db.Enrollment.Join(db.Studies, k => k.IdStudy,
                                 e => e.IdStudy,
                                 (k, e) => new { Enrollment = k, Studies = e })
                                .Where(e => e.Enrollment.Semester == 1 && e.Studies.Name == enroll.Studies).FirstOrDefault();
            db.SaveChanges();
            return StatusCode(201, enrollment);
        }

        public IActionResult Promote(PromoteModel enroll)
        {
            var db = new Models2.s19278Context();
            var st = db.Enrollment.Join(db.Studies, k => k.IdStudy,
                                 e => e.IdStudy,
                                 (k, e) => new { Enrollment = k, Studies = e })
                                .Where(e=>e.Enrollment.Semester==(Int32.Parse(enroll.Semester)) && e.Studies.Name==enroll.Studies);
            if (!st.Any())
            {
                return NotFound("W bazie nie ma takich studiów");
            }
            db.Database.ExecuteSqlRaw("exec Promote @stud, @sem" ,enroll.Studies ,enroll.Semester);
            
            st= db.Enrollment.Join(db.Studies, k => k.IdStudy,
                                 e => e.IdStudy,
                                 (k, e) => new { Enrollment = k, Studies = e })
                                .Where(e => e.Enrollment.Semester == (Int32.Parse(enroll.Semester))+1 && e.Studies.Name == enroll.Studies);

            return Ok(st);
        }

        public bool CheckIndex(string index)
        {
            throw new NotImplementedException();
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
                            expires: DateTime.Now.AddMinutes(10),
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
                    command.Parameters.AddWithValue("token", rtoken);

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

       

        public IActionResult deleteStudent(String id)
        {
            var db = new Models2.s19278Context();
            var st = db.Student.Where(e => e.IndexNumber == id);
            if (!st.Any())
            {
                return NotFound();
            }
            db.Student.Remove(st.FirstOrDefault());
            db.SaveChanges();

            return Ok();
        }

        public IActionResult updateStudent(Models2.Student student)
        {
            var db = new Models2.s19278Context();
           
            
            var st = db.Student.Where(e=>e.IndexNumber==student.IndexNumber);
            if (!st.Any()) {
                return NotFound();
            }
            if (student.LastName != null) {
                st.FirstOrDefault().LastName = student.LastName;
            }
            if (student.FirstName != null)
            {
                st.FirstOrDefault().FirstName = student.FirstName;
            }
            if (student.BirthDate != null)
            {
                st.FirstOrDefault().BirthDate = student.BirthDate;
            }
            if (student.IdEnrollment != 0)
            {
                st.FirstOrDefault().IdEnrollment = student.IdEnrollment;
            }
            db.SaveChanges();

            return Ok();
        }
    }
}
    