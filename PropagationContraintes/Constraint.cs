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
        public bool AppliquerContrainteDiff�rents(int vi, EnsembleDiscret edToFilter)
        {
            //plut�t que d'�num�rer les �lements, de tester s'ils sont diff�rents puis de les supprimer,
            //on supprime directement l'�l�ment qui ne doit pas y �tre.
            edToFilter._elements.Remove(vi);
            return edToFilter._elements.Count>0;
        }

    }
}
