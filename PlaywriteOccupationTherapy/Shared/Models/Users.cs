using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaywriteOccupationTherapy.Shared.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public Guid Salt { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsSuperAdmin { get; set; }
        public bool? IsGeneralUser { get; set; }
    }
}
