using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage.BipartiteGraph
{
    public class Noeud
    {
        //int v;
        public List<ArcCouplant> variablesCouplées;
        public List<ArcCouplant> suivants;
        public bool couplées = false;
        public int numéroDeTrémaux = 0;
        public int pointDAttache = 0;
        public int numéroComposanteConnexe = 0;

        public Noeud()
        {
            //this.v = v;
            variablesCouplées = new List<ArcCouplant>();
            suivants = new List<ArcCouplant>();
        }
    }
}
