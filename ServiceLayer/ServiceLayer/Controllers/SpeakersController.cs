﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using ServiceLayer.Filters;
using ServiceLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class SpeakersController : BaseController
    {
        protected ICampRepository _repository;
        protected ILogger<SpeakersController> _logger;
        protected IMapper _mapper;
        protected UserManager<CampUser> _userMgr;

        public SpeakersController(ICampRepository repository,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userMgr)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userMgr = userMgr;
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? _repository.GetSpeakersByMonikerWithTalks(moniker) : _repository.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet]
        [MapToApiVersion("1.1")]
        public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? _repository.GetSpeakersByMonikerWithTalks(moniker) : _repository.GetSpeakersByMoniker(moniker);

            return Ok(new { count = speakers.Count(), results = _mapper.Map<IEnumerable<SpeakerModel>>(speakers) });
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks ? _repository.GetSpeakerWithTalks(id) : _repository.GetSpeaker(id);
            if (speaker == null) return NotFound();

            if (speaker.Camp.Moniker != moniker)
                return BadRequest("Speaker not in specified class");

            return Ok(speaker);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody]SpeakerModel model)
        {
            try
            {
                var camp = _repository.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest("Could not find camp");

                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                var campUser = await _userMgr.FindByNameAsync(this.User.Identity.Name);

                if (campUser != null)
                {
                    speaker.User = campUser;

                    _repository.Add(speaker);

                    if (await _repository.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
                        return Created(url, _mapper.Map<SpeakerModel>(speaker));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while adding a speaker: {ex}");
            }

            return BadRequest("Could not add a new speaker");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker,
            int id,
            [FromBody]SpeakerModel model)
        {
            try
            {
                var speaker = _repository.GetSpeaker(id);
                if (speaker == null) return NotFound();

                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and camp do not match");
                if (speaker.User.UserName == this.User.Identity.Name) return Forbid();

                _mapper.Map(model, speaker);

                if (await _repository.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while updating speaker: {ex}");
            }

            return BadRequest("Could not update speaker.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = _repository.GetSpeaker(id);
                if (speaker == null) return NotFound();

                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and camp do not match");
                if (speaker.User.UserName == this.User.Identity.Name) return Forbid();

                _repository.Delete(speaker);

                if (await _repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while deleting speaker: {ex}");
            }

            return BadRequest("Could not delete speaker.");
        }
    }
}
