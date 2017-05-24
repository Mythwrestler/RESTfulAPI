using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using AutoMapper;
using Library.API.Models;
using Library.API.Entities;

namespace Library.API.Controllers
{
    [RouteAttribute("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGetAttribute]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if(!_libraryRepository.AuthorExists(authorId)) return NotFound();

            var booksFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var books = Mapper.Map<IEnumerable<BookDto>>(booksFromRepo);
            
            return Ok(books);
        }

        [HttpGet("{bookId}")]
        public IActionResult Get(Guid authorId, Guid bookId)
        {
            if(!_libraryRepository.AuthorExists(authorId)) return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);
            if(bookFromRepo == null) return NotFound();

            var book = Mapper.Map<BookDto>(bookFromRepo);
            return Ok(book);
        }



    }
}