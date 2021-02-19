using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage
{
    public class CSPContrainteDiff : CSPConstraint
    {
        protected CSPVariable Variable1 { get; set; }
        protected CSPVariable Variable2 { get; set; }

        public CSPContrainteDiff(string name, CSPVariable v1, CSPVariable v2)
            : base(name)
        {
            Variable1 = v1;
            Variable2 = v2;
            Variable1.AjouterContrainte(Variable2, this);
            Variable2.AjouterContrainte(Variable1, this);
        }
        public override bool AppliquerLaContrainte(out Dictionary<CSPVariable, List<object>> changements)
        {
            //plutôt que d'énumérer les élements, de tester s'ils sont différents puis de les supprimer,
            //on supprime directement l'élément qui ne doit pas y être.
            changements = new Dictionary<CSPVariable, List<object>>();
            if (Variable1.Instantiated && !Variable2.Instantiated)
            {
                Variable2.Push();
                Variable2.Domaine.Remove(Variable1.ElementChoisi);
                changements.Add(Variable2, new List<object>() { Variable1.ElementChoisi });
                return Variable2.Domaine.Count > 0;
            }
            else if (Variable2.Instantiated && !Variable1.Instantiated)
            {
                Variable1.Push();
                Variable1.Domaine.Remove(Variable2.ElementChoisi);
                changements.Add(Variable1, new List<object>() { Variable2.ElementChoisi });
                return Variable1.Domaine.Count > 0;
            }
            else if (Variable1.Instantiated && Variable2.Instantiated)
            {
                return Variable1.ElementChoisi != Variable2.ElementChoisi;
            }
            else
            {
                if (Variable1.Domaine.Count == 1)
                {
                    Variable2.Domaine.Remove(Variable1.Domaine.First());
                    return Variable2.Domaine.Count > 0;
                }
                else if (Variable2.Domaine.Count == 1)
                {
                    Variable1.Domaine.Remove(Variable2.Domaine.First());
                    return Variable1.Domaine.Count > 0;
                }
            }
            return false;
        }
    }
}
