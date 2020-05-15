using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{   [Authorize]
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private DAL.IDdService _dbService;
        public StudentsController(DAL.IDdService dbService)
        {
            _dbService = dbService;
        }
        [HttpGet("list")]
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }
        [HttpGet("{Index}")]
        public IActionResult GetStudents(string index)
        {
            
            return Ok(_dbService.GetStudents(index));
        }
        [HttpPost]
        public IActionResult CreateStudent(Models.Student student)
        {
            
            return Ok(student);
        }
        
        [HttpPut("update")]
        public IActionResult updateStudent(Models2.Student student)
        {
       
            return Ok(_dbService.updateStudent(student));
        }
        [HttpDelete("delete/{id}")]
        public IActionResult deleteStudent(String id)
        {

            return Ok(_dbService.deleteStudent(id));
        }
    }
}
