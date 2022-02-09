using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashCards
{
    internal class Card
    {
        public int ID {get; set;}
        public string Word {get; set;}
        public string Translate {get; set;}
        public int Try { get; set;}
        public int Show { get; set; }
    }
}
