using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConstraintProgramming;
using ConstraintProgramming.CSPLanguage;

namespace PropagationContraintes
{
    public class ProblemeNReines:CSPProblem
    {
        public ProblemeNReines()
        {
            int nReines = 8;

            var reines = new List<CSPVariable>();

            for (int i = 1; i <= nReines; i++)
            {
                reines.Add(new CSPVariableInteger("r" + i.ToString(), 1, nReines));
            }
            ContraintesGlobales.Add(new CSPContrainteTousDifferents(reines, "toutesdiff."));
            Variables.AddRange(reines);

            var q = new Queue<CSPVariable>(reines);
            while (q.Count > 0)
            {
                var r = q.Dequeue();
                int delta = 1;
                foreach (var diagr in q)
                {
                    new CSPContrainteDiffAvecDelta(r.Name + "," + diagr.Name, r, diagr, delta);
                    new CSPContrainteDiffAvecDelta(r.Name + "," + diagr.Name, r, diagr, -delta);
                    delta++;
                }
            }
        }
    }
}
