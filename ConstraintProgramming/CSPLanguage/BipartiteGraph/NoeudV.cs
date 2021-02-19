using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage.BipartiteGraph
{
    public class NoeudV<T> : Noeud
    {
        public T v;

        public NoeudV(T i)
            : base()
        {
            this.v = i;
        }
        public override string ToString()
        {
            return " " + v.ToString() + " ";
        }
    }
}
