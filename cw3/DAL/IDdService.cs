using cw3.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.DAL
{
    public interface IDdService
    {

        public IEnumerable<Models.Student> GetStudents();
        public IActionResult GetStudents(string index);

        public IActionResult AddStudent(Models.EnrollStudClass enroll);
        public IActionResult Promote(PromoteModel enroll);
        public bool  CheckIndex(string index);
        IActionResult Singin(Singin singin);
		IActionResult Refresh(string token);
	}
}
