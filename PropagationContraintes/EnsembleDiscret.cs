using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PropagationContraintes
{
    public class EnsembleDiscret
    {
        public List<ContrainteGlobaleDelegate> contraintesGlobales;
        public Dictionary<EnsembleDiscret,List<BinConsDelegate>> binaryConstraints;
        public Dictionary<int,int> _elements;

        public int position;
        private Stack<Dictionary<int,int>> _elementsSave;
        private Stack<Dictionary<EnsembleDiscret, List<BinConsDelegate>>> _constraintSave;
        public bool Instantiated=false;
        public int ElementChoisi;

        public void Push()
        {
            _elementsSave.Push(new Dictionary<int,int>(_elements));
            _constraintSave.Push(new Dictionary<EnsembleDiscret, List<BinConsDelegate>>(binaryConstraints));
        }
        public void Pop()
        {
            _elements = _elementsSave.Pop();
            binaryConstraints = _constraintSave.Pop();
        }

        public EnsembleDiscret(int posi)
        {
            position = posi;
            _elementsSave = new Stack<Dictionary<int,int>>();
            _constraintSave = new Stack<Dictionary<EnsembleDiscret, List<BinConsDelegate>>>();

            binaryConstraints = new Dictionary<EnsembleDiscret, List<BinConsDelegate>>();
            _elements = new Dictionary<int,int>();
            contraintesGlobales = new List<ContrainteGlobaleDelegate>();

        }
        public EnsembleDiscret(int debut, int fin,int position):this(position)
        {
            AjouterSequence(debut, fin);
        }
        public void AjouterSequence(int debut, int fin)
        {
            for (int i = debut; i <= fin; i++)
            {
                _elements.Add(i,i);
            }
        }

        public void AjouterContrainte(EnsembleDiscret i, BinConsDelegate del)
        {
            
            if (binaryConstraints.ContainsKey(i))
            {
                binaryConstraints[i].Add(del);
            }
            else
            {
                binaryConstraints.Add(i, new List<BinConsDelegate>());
                binaryConstraints[i].Add(del);
            }
        }

        public EnsembleDiscret Clone()
        {
            EnsembleDiscret ed = new EnsembleDiscret(position);
            ed._elements = new Dictionary<int,int>(this._elements);
            ed.binaryConstraints = new Dictionary<EnsembleDiscret,List<BinConsDelegate>>(binaryConstraints);
            return ed;
        }

        public int IndiceOrdre1
        {
            get
            {
                return binaryConstraints.Count;
            }
        }
        public int IndiceOrdre2(Espace e)
        {
            int io2=0;
            foreach (EnsembleDiscret i in binaryConstraints.Keys)
            {
                io2 += i.IndiceOrdre1;
            }
            return io2;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }
        public int GetFirstElement()
        {
            foreach (int i in _elements.Keys)
            {
                return i;
            }
            throw new Exception();
        }
    }
}
