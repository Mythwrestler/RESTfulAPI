using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [RouteAttribute("api/authorcollections")]
    public class AuthorCollectionsController: Controller
    {
        private ILibraryRepository _libaryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
         _libaryRepository = libraryRepository;   
        }

        [HttpPostAttribute]
        public IActionResult CreateAuthorCollection([FromBodyAttribute] IEnumerable<AuthorForCreateDto> authorcollection)
        {
            if(authorcollection == null) return BadRequest();

            var newAuthors = Mapper.Map<IEnumerable<Author>>(authorcollection);

            foreach (var author in newAuthors)
            {
                _libaryRepository.AddAuthor(author);
            }
            if(!_libaryRepository.Save())
            {
                throw new Exception("Creating author collection failed.");
            }
            
            return Ok();

        }


    }
}