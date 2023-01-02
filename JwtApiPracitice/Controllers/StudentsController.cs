using JwtApiPracitice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtApiPracitice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        [HttpGet("josuke")]
        public IActionResult GetJosuke()
        {
            return Ok(new Student() { Name = "Hishigata Josuke" });
        }

        [Authorize]
        [HttpGet("giorno")]
        public IActionResult GetGiorno()
        {
            return Ok(new Student() { Name = "Giorno Giovvana" });
        }

        [Authorize(Roles = "JOESTAR")]
        [HttpGet("joseph")]
        public IActionResult GetJotaro()
        {
            return Ok(new Student() { Name = "Joseph Joestar" });
        }
    }
}
