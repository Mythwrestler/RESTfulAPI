using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using AutoMapper;
using Library.API.Models;
using Library.API.Entities;
using Microsoft.AspNetCore.JsonPatch;

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
            if (!_libraryRepository.AuthorExists(authorId)) return NotFound();

            var booksFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var books = Mapper.Map<IEnumerable<BookDto>>(booksFromRepo);

            return Ok(books);
        }

        [HttpGet("{bookId}", Name = "BookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId)) return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookFromRepo == null) return NotFound();

            var book = Mapper.Map<BookDto>(bookFromRepo);
            return Ok(book);
        }


        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null) return BadRequest();
            if (!_libraryRepository.AuthorExists(authorId)) return NotFound();

            var newBook = Mapper.Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, newBook);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating book for author {authorId} failed on Save.");
            }

            var bookReturn = Mapper.Map<BookDto>(newBook);

            return CreatedAtRoute(
                "BookForAuthor",
                 new { authorId = newBook.AuthorId, bookId = newBook.Id },
                 bookReturn
            );
        }

        [HttpDelete("{bookId}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId)) return NotFound();
            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookFromRepo == null) return NotFound();

            _libraryRepository.DeleteBook(bookFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {bookId} for author {authorId} failed on save");
            }

            return NoContent();

        }

        [HttpPut("{bookId}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid bookId, [FromBody] BookForUpdateDTO book)
        {
            if (book == null) return NotFound();
            if (!_libraryRepository.AuthorExists(authorId)) return NotFound();
            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);


            if (bookFromRepo == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = bookId;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {bookId} for author {authorId} failed on save");
                }

                var newBook = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute(
                    "BookForAuthor",
                     new { authorId = newBook.AuthorId, bookId = newBook.Id },
                     newBook
                );


            }


            Mapper.Map(book, bookFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookFromRepo);


            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {bookId} for author {authorId} failed on save");
            }

            return NoContent();

        }


        [HttpPatch("{bookId}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookForUpdateDTO> patchDoc)
        {
			if (patchDoc == null) return NotFound();
			if (!_libraryRepository.AuthorExists(authorId)) return NotFound();
			var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);
			if (bookFromRepo == null)
            {
                var bookDto = new BookForUpdateDTO();
                patchDoc.ApplyTo(bookDto);
                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = bookId;

                _libraryRepository.AddBookForAuthor(authorId,bookToAdd);

				if (!_libraryRepository.Save())
				{
					throw new Exception($"Upserting book {bookId} for author {authorId} failed on save");
				}

				var newBook = Mapper.Map<BookDto>(bookToAdd);

				return CreatedAtRoute(
					"BookForAuthor",
					 new { authorId = newBook.AuthorId, bookId = newBook.Id },
					 newBook
				);

            }

            var bookToPatch = Mapper.Map<BookForUpdateDTO>(bookFromRepo);

            patchDoc.ApplyTo(bookToPatch);

            // add validation

            Mapper.Map(bookToPatch, bookFromRepo);


            _libraryRepository.UpdateBookForAuthor(bookFromRepo);


            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {bookId} for author {authorId} failed on save");
            }

            return NoContent();


        }

    }
}
