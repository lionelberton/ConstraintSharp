using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming
{
    public class CSPVariableInteger: CSPVariable
    {
        protected CSPVariableInteger(string name):base(name)
        {

        }

        public CSPVariableInteger(string name,int debut, int fin)
            : base(name)
        {
            foreach (int i in Enumerable.Range(debut, fin - debut + 1))
            {
                Domaine.Add(i);
            }
        }

        public override CSPVariable Clone()
        {
            CSPVariableInteger ed=new CSPVariableInteger(Name);
            base.Clone(ed);
            return ed;
        }
    }
}
