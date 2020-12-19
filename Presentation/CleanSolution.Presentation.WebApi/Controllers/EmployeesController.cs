using CleanSolution.Core.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
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
