using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage
{
    /// <summary>
    /// V1-V2!=delta
    /// </summary>
    public class CSPContrainteDiffAvecDelta:CSPContrainteDiff
    {
        public int Delta { get; private set; }
        public CSPContrainteDiffAvecDelta(string name, CSPVariable v1, CSPVariable v2, int delta) :
            base(name, v1, v2)
        {
            Delta = delta;
        }

        public override bool AppliquerLaContrainte(out Dictionary<CSPVariable, List<object>> changements)
        {
            //plutôt que d'énumérer les élements, de tester s'ils sont différents puis de les supprimer,
            //on supprime directement l'élément qui ne doit pas y être.
            changements = new Dictionary<CSPVariable, List<object>>();
            if (Variable1.Instantiated && !Variable2.Instantiated)
            {
                Variable2.Push();
                int tabou = (int)(Variable1.ElementChoisi) - Delta;
                Variable2.Domaine.Remove(tabou);
                changements.Add(Variable2, new List<object>() {tabou });
                return Variable2.Domaine.Count > 0;
            }
            else if (Variable2.Instantiated && !Variable1.Instantiated)
            {
                Variable1.Push();
                int tabou = (int)(Variable2.ElementChoisi) + Delta;
                Variable1.Domaine.Remove(tabou);
                changements.Add(Variable1, new List<object>() { tabou });
                return Variable1.Domaine.Count > 0;
            }
            return false;
        }
    }
}
