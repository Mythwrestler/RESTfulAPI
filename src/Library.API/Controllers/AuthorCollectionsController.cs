using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libaryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libaryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBodyAttribute] IEnumerable<AuthorForCreationDto> authorcollection)
        {
            if (authorcollection == null) return BadRequest();

            var newAuthors = Mapper.Map<IEnumerable<Author>>(authorcollection);

            foreach (var author in newAuthors)
            {
                _libaryRepository.AddAuthor(author);
            }
            if (!_libaryRepository.Save())
            {
                throw new Exception("Creating author collection failed.");
            }


            var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(newAuthors);
            var idsToReturn = String.Join(",",authorCollectionToReturn.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorCollection",
                                  new { ids = idsToReturn },
                                  authorCollectionToReturn
                                 );

        }


        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _libaryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }
    }
}