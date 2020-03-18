using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private  DAL.IDdService _dbService;

        public StudentsController(DAL.IDdService dbService) 
        {
            _dbService = dbService;
        }
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpPost]
        public IActionResult CreateStudent(Models.Student student)
        {
            student.IndexNumber = $"S{new Random().Next(1, 20000)}";
            return Ok(student);
        }
        [HttpPut("{id}")]
        public IActionResult updateStudent(int id)
        {
       
            return Ok("Aktualizacja studenta o id "+id+" dokonczona");
        }
        [HttpDelete("{id}")]
        public IActionResult deleteStudent(int id)
        {

            return Ok("Usuwanie studenta o id " + id + "zakończone");
        }
    }
}
