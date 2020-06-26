using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class BooksViewModel
    {
        public Book Book { get; set; }
        public Author Author { get; set; }
        public IEnumerable<Genrer> Genrers { get; set; }
        public List<BookGenrer> BookGenrers { get; set; }
        public IEnumerable<Author> Authors { get; set; }
        public IEnumerable<Publisher> Publishers { get; set; }
        public IEnumerable<SpecialTag> SpecialTags { get; set; }
        public List<Book> Books { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public SpecialTag specialtag { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Book> BooksSeen { get; set; }
        public List<Book> BestSeller { get; set; }
        public List<Book> SameGenrer { get; set; }
        public List<Banner> Banners { get; set; }
        public List<Book> Top10BestSeller { get; set; }
        public List<Author> Top5Author { get; set; }
        public List<Book> HighRate { get; set; }
        public List<BookSellPerMonth> BookSellPerMonths { get; set; }
        public double DoanhThu { get; set; }
        public List<PotentialCustomerViewModel> PotentialCustomerViewModels { get; set; }
        public List<ReplyReviewViewModel> ReplyReviewViewModels { get; set; }
        public List<GenrerSelectedForBookViewModel> GenrerSelectedForBookViewModels { get; set; }
        public List<BookGenrersViewModel> BookGenrersViewModels { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
