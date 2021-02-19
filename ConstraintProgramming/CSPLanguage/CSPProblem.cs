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
        public int NombreDeRetourArrière = 0;

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
                if (!AppliquerCohérenceArc(kvp.Value))
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
        private bool AppliquerCohérenceArc(List<CSPConstraint> dels)
        {
            foreach (CSPConstraint del in dels)
            {
                Dictionary<CSPVariable,List<object>> chg;
                if (!del.AppliquerLaContrainte(out chg)) //établit la cohérence d'arc: vrai =au moins une solution, faux=aucune solution possible.
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
                            if (!AppliquerCohérenceArc(kvp.Value))
                            {
                                return false;
                            }
                            ed.BinaryConstraints.Remove(kvp.Key);
                            kvp.Key.BinaryConstraints.Remove(ed);
                        }
                    }

                }
            }
            if (!AppliquerCohérenceArc(ContraintesGlobales)) return false;

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
        public virtual bool CohérenceDeLaSolution()
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
            NombreDeRetourArrière = 0;
            if (!PreSolve()) return;//on fait un présolve pour vérifier la cohérence initiale du système.
            var iterators = new Stack<IEnumerator<object>>();
            int avancementMaximum = 0;
            HashSet<CSPVariable> variablesLibres = new HashSet<CSPVariable>(Variables);
            Stack<CSPVariable> variablesFixees = new Stack<CSPVariable>();
            CSPVariable variableChoisie = null;
            while (true)
            {
                //on choisit une nouvelle variable à instancier suivant une heuristique.
                variableChoisie = ChoisirUneVariable(variablesLibres);
                var iter = variableChoisie.Items.GetEnumerator();
                variableChoisie.Instantiated = true;

                while (true)
                {
                    while (!iter.MoveNext()) //itérateur au bout du rouleau, on revient en arrière.
                    {
                        if (iterators.Count > 0)//il reste des itérateurs?
                        {// on revient en arrière.
                            variableChoisie.Instantiated = false;
                            variablesLibres.Add(variableChoisie);
                            variableChoisie = variablesFixees.Pop();
                            iter = iterators.Pop();
                            BackTrack();
                            NombreDeRetourArrière++;
                        }
                        else //on a fini d'explorer tout l'espace. Il n'y a plus rien à faire.
                        {
                            return;
                        }
                    }
                    // on choisit un élément.
                    variableChoisie.ElementChoisi = iter.Current;
                    //on vérifie la cohérence d'arc. Si on vient de fixer la dernière variable alors toutes les contraintes sont forcément cohérentes.
                    if (variablesLibres.Count > 0 && AnticipationPourContraintesBinaires(variableChoisie) && AppliquerCohérenceArc(variableChoisie.ContraintesGlobales))
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
                    } //on a trouvé une incohérence, revenir en arrière et faire un autre choix.
                    else
                    {
                        if (variablesLibres.Count == 0)//c'était la dernière variable?
                        {
                            //on a donc trouvé une solution.
                            OnSolutionFound(Variables);
                        }
                        if (CurrentBacktrack != null)
                        {
                            CurrentBacktrack();
                            CurrentBacktrack = null;
                        }
                        NombreDeRetourArrière++;
                    }

                }
            }
        }

    }
}
