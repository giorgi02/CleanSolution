using CleanSolution.Core.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;


namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator mediator;

        public EmployeesController(IMediator mediator) =>
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet]
        public IEnumerable<GetEmployeeDto> Get()
        {
            return null;
        }

        [HttpGet("{id}")]
        public GetEmployeeDto Get(int id)
        {
            return null;
        }

        [HttpPost]
        public void Post([FromBody] SetEmployeeDto value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] SetEmployeeDto value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
