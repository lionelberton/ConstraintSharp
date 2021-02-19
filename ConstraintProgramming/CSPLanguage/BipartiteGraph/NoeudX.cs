using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage.BipartiteGraph
{
    public class NoeudX<T> : Noeud
    {
        public T ed;

        public NoeudX(T e)
            : base()
        {
            ed = e;
        }
        public override string ToString()
        {
            return " [" + ed.ToString() + "] ";
        }
    }
}
