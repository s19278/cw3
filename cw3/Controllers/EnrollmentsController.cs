using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [Authorize(Roles = "employee")]
    [ApiController]
    [Route("api/enrolments")]
    public class EnrollmentsController : ControllerBase
    {
        private DAL.IDdService _dbService;
        
        public EnrollmentsController(DAL.IDdService dbService)
        {
            _dbService = dbService;
        }
        [HttpPost]
        public IActionResult AddStudent(Models.EnrollStudClass enroll)
        {
            return _dbService.AddStudent(enroll);
        }
        [HttpPost("promotions")]
        public IActionResult Promote(Models.PromoteModel enroll)
        {
            return _dbService.Promote(enroll);
        }
    }
}