using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{

    public interface IPropertyMappingService
    
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

    }



}