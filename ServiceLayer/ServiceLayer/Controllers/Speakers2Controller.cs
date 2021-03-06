﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using ServiceLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ApiVersion("2.0")]
    public class Speakers2Controller : SpeakersController
    {
        public Speakers2Controller(ICampRepository repository,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userMgr)
            : base(repository, logger, mapper, userMgr)
        {
        }

        public override IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? _repository.GetSpeakersByMonikerWithTalks(moniker)
                : _repository.GetSpeakersByMoniker(moniker);

            return Ok(new
            {
                currentTime = DateTime.UtcNow,
                count = speakers.Count(),
                results = _mapper.Map<IEnumerable<Speaker2Model>>(speakers)
            });
        }
    }
}
