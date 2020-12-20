using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;


namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly IUnitOfWork unit;
        public PositionsController(IUnitOfWork unit)
        {
            this.unit = unit;
        }

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
