using CleanSolution.Core.Domain.Enums;
using System;

namespace $safeprojectname$.DTOs
{
    public class SetEmployeeDto
    {
        public string PrivateNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public SetPositionDto Position { get; set; }
    }
}
