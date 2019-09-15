using System;

namespace NzbWishlist.Azure.Models
{
    public class WishResultViewModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        public DateTime PubDate { get; set; }

        public string DetailsUrl { get; set; }

        public string PreviewUrl { get; set; }

        public string NzbUrl { get; set; }
    }
}