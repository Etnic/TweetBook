﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweetBook.Contracts.v1.Requests
{
    public class UpdatePostRequest
    {
       public Guid Id { get; set; }

        public string Name { get; set; }
    }
}