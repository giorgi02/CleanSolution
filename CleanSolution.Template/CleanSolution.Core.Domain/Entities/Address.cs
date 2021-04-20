﻿using $safeprojectname$.Basics;
using System.Collections.Generic;

namespace $safeprojectname$.Entities
{
    public class Address : ValueObject
    {
        public string City { get; set; }
        public string Street { get; set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return this.City;
            yield return this.Street;
        }
    }
}