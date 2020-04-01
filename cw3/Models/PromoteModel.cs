using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.Models
{
    public class PromoteModel
    {
        [Required]
        public string Studies { get; set; }
        [Required]
        public string Semester { get; set; }
    }
}
