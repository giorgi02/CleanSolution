using CleanSolution.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanSolution.Core.Application.DTOs
{
    public class GetEmployeeDto
    {
        public Guid Id { get; set; }
        public string PrivateNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public GetPositionDto Position { get; set; }
    }
}
