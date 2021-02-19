using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage.BipartiteGraph
{
    public class ArcCouplant
    {
        public Noeud variable;
        public Noeud valeur;
        public bool couplant = false;
        public bool marqué = false;
        public bool déjàPassé = false;
        public ArcCouplant(Noeud nx, Noeud nv)
        {
            variable = nx;
            valeur = nv;
        }
    }
}
