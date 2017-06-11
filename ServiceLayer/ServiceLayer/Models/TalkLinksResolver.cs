using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;
using ServiceLayer.Controllers;
using System.Collections.Generic;

namespace ServiceLayer.Models
{
    public class TalkLinksResolver : IValueResolver<Talk, TalkModel, ICollection<LinkModel>>
    {
        private IHttpContextAccessor _httpContextAccessor;

        public TalkLinksResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public ICollection<LinkModel> Resolve(Talk source, TalkModel destination, ICollection<LinkModel> destMember, ResolutionContext context)
        {
            var url = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            object values = new { moniker = source.Speaker.Camp.Moniker, speakerId = source.Speaker.Id, id = source.Id };

            return new List<LinkModel>()
            {
                new LinkModel()
                {
                    Rel = "Self",
                    Href = url.Link("GetTalk", values)
                },
                new LinkModel()
                {
                    Rel = "Update",
                    Href = url.Link("UpdateTalk", values),
                    Verb = "PUT"
                },
                new LinkModel()
                {
                    Rel = "Speaker",
                    Href = url.Link("SpeakerGet", new { moniker = source.Speaker.Camp.Moniker, id = source.Speaker.Id })
                }
             };
        }
    }
}

