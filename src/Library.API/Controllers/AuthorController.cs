
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

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }


        [HttpGetAttribute]
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors();
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors);
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