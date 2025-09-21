using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksClient.Models
{
    public class Book
    {
        public string ISBN { get; set; } = "";
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string? Description { get; set; }
    }
}
