﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CleanSolution.Core.Domain.Entities
{
    public class Position
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Salary { get; set; }
    }
}
