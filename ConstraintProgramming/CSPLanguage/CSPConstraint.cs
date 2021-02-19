using System;
using System.Collections.Generic;
using System.Text;

namespace ConstraintProgramming
{
    public abstract class CSPConstraint
    {
        public string Name { get; set; }

        public CSPConstraint(string name)
        {
            Name = name;
        }
        public abstract bool AppliquerLaContrainte(out Dictionary<CSPVariable,List<object>> changements);

    }
}
