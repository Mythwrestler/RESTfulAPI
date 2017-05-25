using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class AuthorForCreateDto 
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Genre { get; set; }

        public ICollection<BookForCreateDto> Books {get; set;} =
         new List<BookForCreateDto>();

    }
}