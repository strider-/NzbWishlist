using System;

namespace NzbWishlist.Azure.Models
{
    public class CartEntryViewModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime PublishDate { get; set; }

        public string DetailsUrl { get; set; }

        public string GrabUrl { get; set; }

        public string Category { get; set; }
    }
}
