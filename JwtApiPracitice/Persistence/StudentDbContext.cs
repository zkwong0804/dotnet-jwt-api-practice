using JwtApiPracitice.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtApiPracitice.Persistence
{
    public class StudentDbContext : IdentityDbContext<StudentUser, StudentRole, Guid>
    {
        public StudentDbContext(DbContextOptions opt) : base(opt) { }
    }
}
