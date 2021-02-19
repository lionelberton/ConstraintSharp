using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Linq;

namespace ConstraintProgramming
{
    public delegate bool BinConsDelegate(object vi, CSPVariable edToFilter);
    public delegate bool ContrainteGlobaleDelegate(CSPProblem E, Dictionary<CSPVariable, List<int>> changements);
    public delegate void HandleSolution(CSPProblem sender, List<CSPVariable> tuple);
    public delegate CSPVariable ChooseVariable(HashSet<CSPVariable> variablesLibres);

    public class CSPProblem
    {
        public int NombreDeChoix = 0;
        public int NombreDeRetourArri�re = 0;

        public List<CSPVariable> Variables { get; protected set; }
        public List<CSPConstraint> ContraintesGlobales { get; protected set; }

        private Stack<ThreadStart> _backtrack;
        public ThreadStart CurrentBacktrack;
        public ChooseVariable ChoisirUneVariable;

        public CSPProblem()
        {
            Variables = new List<CSPVariable>();
            _backtrack = new Stack<ThreadStart>();
            ContraintesGlobales = new List<CSPConstraint>();
            ChoisirUneVariable = delegate(HashSet<CSPVariable> h)
            {
                CSPVariable min = h.First();
                foreach (var ed in h)
                {
                    min = (ed.Domaine.Count < min.Domaine.Count ? ed : min);
                }
                h.Remove(min);
                return min;
            };
        }

        public CSPProblem Clone()
        {
            CSPProblem e = new CSPProblem();
            e.Variables = new List<CSPVariable>(Variables.Select(v => v.Clone()));
            return e;
        }

        //pour contraintes binaires
        // en utilisant la double liaison entre les ensembles de contraintes.
        public bool AnticipationPourContraintesBinaires(CSPVariable variableFixee)
        {
            bool status = true;
            List<CSPVariable> toPop = new List<CSPVariable>();
            foreach (KeyValuePair<CSPVariable, List<CSPConstraint>> kvp in variableFixee.BinaryConstraints)
            {
                CSPVariable variableLibre = kvp.Key;
                if (variableLibre.Instantiated) continue;

                variableLibre.Push();
                toPop.Add(variableLibre);
                if (!AppliquerCoh�renceArc(kvp.Value))
                {
                    status = false;
                    break;
                }
                variableLibre.BinaryConstraints.Remove(variableFixee);
            }

            CurrentBacktrack += delegate()
            {
                foreach (CSPVariable edToPop in toPop)
                {
                    edToPop.Pop();
                }
            };
            return status;
        }
        //renvoie vrai s'il peut exister une solution possible, faux sinon=aucune solution possible.
        private bool AppliquerCoh�renceArc(List<CSPConstraint> dels)
        {
            foreach (CSPConstraint del in dels)
            {
                Dictionary<CSPVariable,List<object>> chg;
                if (!del.AppliquerLaContrainte(out chg)) //�tablit la coh�rence d'arc: vrai =au moins une solution, faux=aucune solution possible.
                {
                    AddToPopDelegate(chg);
                    return false;
                }
                AddToPopDelegate(chg);
            }
            return true;
        }
        private void AddToPopDelegate(Dictionary<CSPVariable, List<object>> changements)
        {
            CurrentBacktrack += delegate()
            {
                foreach (CSPVariable ed in changements.Keys)
                {
                    ed.Pop();
                }
            };
        }

        public bool PreSolve()
        {
            bool repasser = true;
            while (repasser == true)
            {
                repasser = false;
                foreach (CSPVariable ed in Variables)
                {
                    foreach (KeyValuePair<CSPVariable, List<CSPConstraint>> kvp in new Dictionary<CSPVariable, List<CSPConstraint>>(ed.BinaryConstraints))
                    {
                        if (kvp.Key.Domaine.Count == 1)
                        {
                            if (!AppliquerCoh�renceArc(kvp.Value))
                            {
                                return false;
                            }
                            ed.BinaryConstraints.Remove(kvp.Key);
                            kvp.Key.BinaryConstraints.Remove(ed);
                        }
                    }

                }
            }
            if (!AppliquerCoh�renceArc(ContraintesGlobales)) return false;

            return true;
        }

        protected void BackTrack()
        {
            _backtrack.Pop()();
        }

        public int Dimension
        {
            get
            {
                return Variables.Count;
            }
        }

        public virtual bool ComparerALaReference()
        {
            return true;
        }
        public virtual bool Coh�renceDeLaSolution()
        {
            return true;
        }

        public event HandleSolution SolutionFound;
        public void OnSolutionFound(List<CSPVariable> tuple)
        {
            if (SolutionFound != null)
            {
                SolutionFound(this, tuple);
            }
        }
        public void ParcourirEspace()
        {
            NombreDeChoix = 0;
            NombreDeRetourArri�re = 0;
            if (!PreSolve()) return;//on fait un pr�solve pour v�rifier la coh�rence initiale du syst�me.
            var iterators = new Stack<IEnumerator<object>>();
            int avancementMaximum = 0;
            HashSet<CSPVariable> variablesLibres = new HashSet<CSPVariable>(Variables);
            Stack<CSPVariable> variablesFixees = new Stack<CSPVariable>();
            CSPVariable variableChoisie = null;
            while (true)
            {
                //on choisit une nouvelle variable � instancier suivant une heuristique.
                variableChoisie = ChoisirUneVariable(variablesLibres);
                var iter = variableChoisie.Items.GetEnumerator();
                variableChoisie.Instantiated = true;

                while (true)
                {
                    while (!iter.MoveNext()) //it�rateur au bout du rouleau, on revient en arri�re.
                    {
                        if (iterators.Count > 0)//il reste des it�rateurs?
                        {// on revient en arri�re.
                            variableChoisie.Instantiated = false;
                            variablesLibres.Add(variableChoisie);
                            variableChoisie = variablesFixees.Pop();
                            iter = iterators.Pop();
                            BackTrack();
                            NombreDeRetourArri�re++;
                        }
                        else //on a fini d'explorer tout l'espace. Il n'y a plus rien � faire.
                        {
                            return;
                        }
                    }
                    // on choisit un �l�ment.
                    variableChoisie.ElementChoisi = iter.Current;
                    //on v�rifie la coh�rence d'arc. Si on vient de fixer la derni�re variable alors toutes les contraintes sont forc�ment coh�rentes.
                    if (variablesLibres.Count > 0 && AnticipationPourContraintesBinaires(variableChoisie) && AppliquerCoh�renceArc(variableChoisie.ContraintesGlobales))
                    {
                        _backtrack.Push(CurrentBacktrack);
                        CurrentBacktrack = null;
                        iterators.Push(iter);
                        variablesFixees.Push(variableChoisie);
                        if (variablesFixees.Count > avancementMaximum)
                        {
                            avancementMaximum = variablesFixees.Count;
                            Console.Out.WriteLine(avancementMaximum);
                        }
                        NombreDeChoix++;
                        break;
                    } //on a trouv� une incoh�rence, revenir en arri�re et faire un autre choix.
                    else
                    {
                        if (variablesLibres.Count == 0)//c'�tait la derni�re variable?
                        {
                            //on a donc trouv� une solution.
                            OnSolutionFound(Variables);
                        }
                        if (CurrentBacktrack != null)
                        {
                            CurrentBacktrack();
                            CurrentBacktrack = null;
                        }
                        NombreDeRetourArri�re++;
                    }

                }
            }
        }

    }
}
