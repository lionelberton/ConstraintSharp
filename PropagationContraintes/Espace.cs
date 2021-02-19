using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace PropagationContraintes
{
    public delegate bool BinConsDelegate(int vi, EnsembleDiscret edToFilter);
    public delegate bool ContrainteGlobaleDelegate(Dictionary<EnsembleDiscret,List<int>> changements);

    public class Espace
    {
        public List<EnsembleDiscret> _ensembles;
        public List<EnsembleDiscret> _ensemblesTriés;
        public List<ContrainteGlobaleDelegate> contraintesGlobales;
        private Stack<ThreadStart> _backtrack;

        public Espace()
        {
            _ensembles = new List<EnsembleDiscret>();
            _ensemblesTriés = new List<EnsembleDiscret>();
            _backtrack = new Stack<ThreadStart>();
            contraintesGlobales = new List<ContrainteGlobaleDelegate>();
        }

        public void InitialiserLesEnsemblesTriés()
        {
            _ensemblesTriés = new List<EnsembleDiscret>(_ensembles);
        }

        public void AddConstraint(int i, int j, BinConsDelegate del)
        {
            _ensembles[j].AjouterContrainte(_ensembles[i], del);
            _ensembles[i].AjouterContrainte(_ensembles[j], del);

        }

        public Espace Clone()
        {
            Espace e = new Espace();
            e._ensembles = new List<EnsembleDiscret>();
            foreach (EnsembleDiscret ed in _ensembles)
            {
                e._ensembles.Add(ed.Clone());
            }
            e._ensemblesTriés = new List<EnsembleDiscret>(e._ensembles);
            return e;
        }

        //pour contraintes binaires
        // en utilisant la double liaison entre les ensembles de contraintes.
        public bool ForwardCheck2(int i, int vi)
        {
            List<EnsembleDiscret> toPop = new List<EnsembleDiscret>();
            foreach (KeyValuePair<EnsembleDiscret, List<BinConsDelegate>> kvp in _ensemblesTriés[i].binaryConstraints)
            {
                int k = 0;
                EnsembleDiscret ed = kvp.Key;
                if (ed.Instantiated) continue;

                ed.Push();
                toPop.Add(ed);
                if (!CheckElement2(kvp.Value, vi, ed))
                {
                    foreach (EnsembleDiscret edToPop in toPop)
                    {
                        edToPop.Pop();
                    }
                    return false;
                }
                ed.binaryConstraints.Remove(_ensemblesTriés[i]);
            }

            _backtrack.Push(delegate()
            {
                foreach (EnsembleDiscret edToPop in toPop)
                {
                    edToPop.Pop();
                }
            });
            return true;
        }

        private bool CheckElement2(List<BinConsDelegate> dels, int vi, EnsembleDiscret edToFilter)
        {
            foreach (BinConsDelegate del in dels)
            {
                if (!del(vi, edToFilter))
                {
                    return false;
                }
            }
            return true;
        }

        public bool PreSolve()
        {
            bool repasser = true;
            while (repasser == true)
            {
                repasser = false;
                foreach (EnsembleDiscret ed in _ensembles)
                {
                    foreach (KeyValuePair<EnsembleDiscret, List<BinConsDelegate>> kvp in new Dictionary<EnsembleDiscret, List<BinConsDelegate>>(ed.binaryConstraints))
                    {
                        if (kvp.Key._elements.Count == 1)
                        {
                            if (!CheckElement2(kvp.Value, kvp.Key.GetFirstElement(), ed))
                            {
                                return false;
                            }
                            ed.binaryConstraints.Remove(kvp.Key);
                            kvp.Key.binaryConstraints.Remove(ed);
                        }
                    }

                }
            }
            foreach (ContrainteGlobaleDelegate del in contraintesGlobales)
            {
                if (!del(null)) return false;
            }
            return true;
        }

        public bool AppliquerLesContraintesGlobales(EnsembleDiscret ed)
        {
            foreach (ContrainteGlobaleDelegate del in ed.contraintesGlobales)
            {
                if (!del(null)) return false;
            }
            return true;
        }

        public void BackTrack()
        {
            _backtrack.Pop()();
        }

        public int[] ReorderTuple(Stack<int> t)
        {
            int[] tupleEnOrdre = new int[t.Count];
            int[] tuple = t.ToArray();
            for (int i = 0; i < t.Count; i++)
            {
                tupleEnOrdre[_ensemblesTriés[t.Count - i - 1].position] = tuple[i];
            }
            return tupleEnOrdre;

        }

        public int Dimension
        {
            get
            {
                return _ensembles.Count;
            }
        }

        public bool ComparerALaReference(Espace e)
        {
            for (int i = 0; i < e._ensembles.Count;i++ )
            {
                if (!this._ensembles[i]._elements.ContainsKey(e._ensembles[i].GetFirstElement()))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool ComparerALaReference()
        {
            return true;
        }
        public virtual bool CohérenceDeLaSolution()
        {
            return true;
        }

    }
}
