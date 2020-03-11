using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortZilla
{
    class ZillaLabel
    {
        private string path;
        private int mapping;

        public ZillaLabel()
        {
            this.Path = null;
            this.Mapping = -1;
        }

        public ZillaLabel(string path, int mapping)
        {
            this.Path = path;
            this.Mapping = mapping;
        }

        public string Path { get => path; set => path = value; }
        public int Mapping { get => mapping; set => mapping = value; }
    }
}
