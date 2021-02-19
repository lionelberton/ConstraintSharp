using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace ConstraintProgramming
{
    public class CSPVariable
    {
        public string Name { get; protected set; }

        public List<CSPConstraint> ContraintesGlobales { get; protected set; }
        public Dictionary<CSPVariable, List<CSPConstraint>> BinaryConstraints { get; protected set; }
        public HashSet<object> Domaine { get; protected set; }

        protected Stack<HashSet<object>> domainesSauve { get; set; }
        protected Stack<Dictionary<CSPVariable, List<CSPConstraint>>> constraintSave { get; set; }
        public bool Instantiated { get; set; }
        public object ElementChoisi { get; set; }

        public void Push()
        {
            domainesSauve.Push(new HashSet<object>(Domaine));
            constraintSave.Push(new Dictionary<CSPVariable, List<CSPConstraint>>(BinaryConstraints));
        }
        public void Pop()
        {
            Domaine = domainesSauve.Pop();
            BinaryConstraints = constraintSave.Pop();
        }

        public CSPVariable(string name)
        {
            Name = name;
            Instantiated = false;
            domainesSauve = new Stack<HashSet<object>>();
            constraintSave = new Stack<Dictionary<CSPVariable, List<CSPConstraint>>>();

            BinaryConstraints = new Dictionary<CSPVariable, List<CSPConstraint>>();
            Domaine = new HashSet<object>();
            ContraintesGlobales = new List<CSPConstraint>();

        }

        public void AjouterContrainte(CSPVariable i, CSPConstraint del)
        {

            if (BinaryConstraints.ContainsKey(i))
            {
                BinaryConstraints[i].Add(del);
            }
            else
            {
                BinaryConstraints.Add(i, new List<CSPConstraint>() { del });
            }
        }
        public virtual CSPVariable Clone()
        {
            CSPVariable ed = new CSPVariable(Name);
            Clone(ed);
            return ed;
        }
        protected virtual void Clone(CSPVariable cloneToFill)
        {
            cloneToFill.Domaine = new HashSet<object>(this.Domaine);
            cloneToFill.BinaryConstraints = new Dictionary<CSPVariable, List<CSPConstraint>>(BinaryConstraints);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public object GetFirstElement()
        {
            return Domaine.First();
        }

        public IEnumerable<object> Items
        {
            get
            {
                return Domaine;
            }
        }
    }
}
