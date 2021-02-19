using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConstraintProgramming;

namespace PropagationContraintes
{
    public class ProblemeGolfeurs:CSPProblem
    {
        public ProblemeGolfeurs()
        {
            int nGolfeur = 32;
            int nSemaine = 10;

            for (int i = 1; i <= nSemaine;i++ )
            {
                var row = new List<CSPVariable>();
                for (int j = 1; j <= nGolfeur; j++)
                {
                    row.Add(new CSPVariableInteger("s" + i.ToString() + ",g" + j.ToString(), 1, nGolfeur));
                }
                ContraintesGlobales.Add(new CSPContrainteTousDifferents(row, "s" + i.ToString()));

            }
        }
    }
}
