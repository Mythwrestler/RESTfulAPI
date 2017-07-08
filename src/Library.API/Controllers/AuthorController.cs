
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [RouteAttribute("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;

        public AuthorsController(ILibraryRepository libraryRepository,
                                IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }


        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorResourceParameters authorResourceParameters)
        {
            
            var authorsFromRepo = _libraryRepository.GetAuthors(authorResourceParameters);

            var previousPageLink = 
                authorsFromRepo.HasPrevious? 
                               GetAuthorsResourceUri(authorResourceParameters,ResourceUriType.PreviousPage) :
                               null;

			var nextPageLink =
				authorsFromRepo.HasNext ?
							   GetAuthorsResourceUri(authorResourceParameters, ResourceUriType.NextPage) :
							   null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
        }

        private string GetAuthorsResourceUri(
            AuthorResourceParameters authorResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                       new
                       {
                           searchQuery = authorResourceParameters.SearchQuery,
                           genre = authorResourceParameters.Genre, 
                           pageNumber = authorResourceParameters.PageNumber - 1,
                           pageSize = authorResourceParameters.PageSize
                       });
				case ResourceUriType.NextPage:
					return _urlHelper.Link("GetAuthors",
					   new
					   {
						   searchQuery = authorResourceParameters.SearchQuery,
						   genre = authorResourceParameters.Genre,
						   pageNumber = authorResourceParameters.PageNumber + 1,
						   pageSize = authorResourceParameters.PageSize
					   });
                default:
					return _urlHelper.Link("GetAuthors",
					   new
					   {
						   searchQuery = authorResourceParameters.SearchQuery,
						   genre = authorResourceParameters.Genre,
						   pageNumber = authorResourceParameters.PageNumber,
						   pageSize = authorResourceParameters.PageSize
					   });
                    
            }
                                

        }


        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null) return NotFound();
            var author = Mapper.Map<AuthorDto>(authorFromRepo);
            return Ok(author);
        }


        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if(!ModelState.IsValid) {
                return BadRequest();
            }

            if(author == null) return BadRequest();

            var newAuthor = Mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(newAuthor);            
            if(!_libraryRepository.Save()) 
            {
                throw new Exception("Creating an author failed on Save.");
            }

            var authorReturn = Mapper.Map<AuthorDto>(newAuthor);

            return CreatedAtRoute("GetAuthor", new { id = authorReturn.Id}, authorReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreate (Guid id)
        {
            if(_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if(authorFromRepo == null)
            {
                return NotFound();
            }

			_libraryRepository.DeleteAuthor(authorFromRepo);

			if (!_libraryRepository.Save())
			{
				throw new Exception($"Deleting author {id} failed on save");
			}

            return NoContent();

        }

    }
}