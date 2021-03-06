﻿using NzbWishlist.Core.Models;
using System.Collections.Generic;

namespace NzbWishlist.Azure.Models
{
    internal class SearchProviderContext
    {
        public Provider Provider { get; set; }

        public IEnumerable<Wish> Wishes { get; set; }
    }
}
