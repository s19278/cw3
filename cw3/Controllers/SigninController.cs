using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/singin")]
    public class SinginController : ControllerBase
    {
        private DAL.IDdService _dbService;
        public SinginController(DAL.IDdService dbService)
        {
            _dbService = dbService;
        }
        [HttpPost]
        public IActionResult Singin(Models.Singin singin)
        {
            return _dbService.Singin(singin);
        }
        [HttpPost("refresh/{token}")]
        public IActionResult Refresh(string token)
        {
            return _dbService.Refresh(token);
        }

    }
}