using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DRedactor
{
    public class LineWrapper : LinesVisual3D
    {
        public string Name { get; set; }

        public LineWrapper(string name) => Name = name;
    }
}
