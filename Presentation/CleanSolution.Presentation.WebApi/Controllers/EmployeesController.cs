using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;


namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator mediator;

        public EmployeesController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        // GET: api/<EmployeesController>
        [HttpGet]
        public IEnumerable<GetEmployeeDto> Get()
        {
            return null;
        }

        // GET api/<EmployeesController>/5
        [HttpGet("{id}")]
        public GetEmployeeDto Get(int id)
        {
            return null;
        }

        // POST api/<EmployeesController>
        [HttpPost]
        public void Post([FromBody] SetEmployeeDto value)
        {
        }

        // PUT api/<EmployeesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] SetEmployeeDto value)
        {
        }

        // DELETE api/<EmployeesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
