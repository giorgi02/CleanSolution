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
    public class PositionsController : ControllerBase
    {
        // GET: api/<PositionsController>
        [HttpGet]
        public IEnumerable<GetPositionDto> Get()
        {
            return null;
        }

        // GET api/<PositionsController>/5
        [HttpGet("{id}")]
        public GetPositionDto Get(int id)
        {
            return null;
        }

        // POST api/<PositionsController>
        [HttpPost]
        public void Post([FromBody] SetPositionDto value)
        {
        }

        // PUT api/<PositionsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] SetPositionDto value)
        {
        }

        // DELETE api/<PositionsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
