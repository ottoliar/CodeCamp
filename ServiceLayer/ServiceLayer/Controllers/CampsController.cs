using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using ServiceLayer.Filters;
using ServiceLayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Controllers
{
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
    {
        private ICampRepository _repo;
        private ILogger<CampsController> _logger;
        private IMapper _mapper;

        public CampsController(ICampRepository repo, ILogger<CampsController> logger, IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [Route("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();
            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;
                if (includeSpeakers)
                {
                    camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                }
                else
                {
                    camp = _repo.GetCampByMoniker(moniker);
                }

                if (camp == null)
                {
                    return NotFound($"Camp {moniker} was not found");
                }

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
                return NotFound();
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CampModel model)
        {
            try
            {
                _logger.LogInformation("Creating a new code camp.");

                var camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);
                if (await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { moniker = camp.Moniker });
                    return Created(newUri, _mapper.Map<CampModel>(camp));
                }
                else
                {
                    _logger.LogWarning("Error saving code camp to database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while saving Camp: {ex}");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody]Camp model)
        {
            try
            {
                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with an ID of {moniker}");
                }

                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }
            }
            catch (Exception ex)
            {

            }

            return BadRequest("Couldn't update camp.");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find Camp with ID of {moniker}");
                }

                _repo.Delete(oldCamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
            }

            return BadRequest("Could not delete Camp");
        }
    }
}
