using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.Models
{
	public class Singin
	{
		[Required]
		public string Login { get; set; }
		[Required]
		public string Haslo { get; set; }
	}
}
