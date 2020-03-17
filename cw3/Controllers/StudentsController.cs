using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{   [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {[HttpGet("{id}")]
        public string GetStudents(int id) 
        {

            return $"Chce info o:"+id;
        }
    }
}