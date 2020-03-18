
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.DAL
{
    public class MockDbService : IDdService
    {
        private static IEnumerable<Models.Student> _students;

        static MockDbService() {
            _students = new List<Models.Student>
            {
                new Models.Student{ IdStudent=1,FirstName="Mirek",LastName="Mirabelka",IndexNumber= $"S{new Random().Next(1, 20000)}"},
                new Models.Student{ IdStudent=2,FirstName="Janusz",LastName="Nosacz",IndexNumber= $"S{new Random().Next(1, 20000)}"},
                new Models.Student{ IdStudent=3,FirstName="Grażyna",LastName="Nosacz",IndexNumber= $"S{new Random().Next(1, 20000)}"},
                new Models.Student{ IdStudent=4,FirstName="Pioter",LastName="Podrost",IndexNumber= $"S{new Random().Next(1, 20000)}"},
                new Models.Student{ IdStudent=5,FirstName="Dmitrii",LastName="Uakari",IndexNumber= $"S{new Random().Next(1, 20000)}"},
                new Models.Student{ IdStudent=6,FirstName="Michał",LastName="Markiewicz",IndexNumber= $"S{new Random().Next(1, 20000)}"}
            };
        }
        public IEnumerable<Models.Student> GetStudents()
        {
            return _students;
        }
    }
}
