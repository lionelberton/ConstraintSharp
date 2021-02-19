using System;
using System.Collections.Generic;
using System.Text;

namespace PropagationContraintes
{
    public class Constraint
    {
        int e1, e2;
        public Constraint(int i, int j)
        {
            e1 = i;
            e2 = j;
        }
        public bool AppliquerContrainteDifférents(int vi, EnsembleDiscret edToFilter)
        {
            //plutôt que d'énumérer les élements, de tester s'ils sont différents puis de les supprimer,
            //on supprime directement l'élément qui ne doit pas y être.
            edToFilter._elements.Remove(vi);
            return edToFilter._elements.Count>0;
        }

    }
}
