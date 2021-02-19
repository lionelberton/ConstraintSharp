using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using ConstraintProgramming.CSPLanguage;
using ConstraintProgramming.CSPLanguage.BipartiteGraph;

namespace ConstraintProgramming
{
    public class CSPContrainteTousDifferents : CSPConstraint
    {
        private List<CSPVariable> _X;
        private GrapheVariableValeur<CSPVariable, object> _graphe;
        public CSPContrainteTousDifferents(IEnumerable<CSPVariable> X, string name)
            : base(name)
        {
            _X = X.ToList();
            
            foreach (CSPVariable ed in X)
            {
                ed.ContraintesGlobales.Add(this);
            }
        }
        public override bool AppliquerLaContrainte(out Dictionary<CSPVariable, List<object>> changements)
        {
            changements = new Dictionary<CSPVariable, List<object>>();
            //Console.WriteLine(Name);
            //if (_graphe == null)
            //{
            Cr�erLeGrapheVariableValeur();
            //}
            var gvv = _graphe;
            //if (changements != null)
            //{
            //    //AppliquerLesChangementsSurLeGraphe(gvv, changements);
            //}
            if (gvv.variables.Count > gvv.valeurs.Count) return false;
            if (!gvv.RechercheDuCouplageMaximal())
            {
                if (!gvv.RechercheCouplageMaximumAPartirDuCouplageMaximal()) return false;
            }
            gvv.TransformerEnGrapheOrient�();
            //AfficherLeGrapheOrient�AvecCouplageMaximum(gvv);
            gvv.MarquerLesCheminsPartantDunSommetLibre();
            gvv.MarquerLesCyclesPairs();
            //AfficherLesComposantesConnexes(gvv);
            changements = SupprimerLesArcsNonMarqu�s(gvv);
            //foreach (KeyValuePair<EnsembleDiscret, List<int>> kvp in SupprimerLesArcsNonMarqu�s(gvv))
            //{
            //    foreach (ContrainteGlobaleDelegate del in kvp.Key.contraintesGlobales)
            //    {
            //        if (!del(null)) return false;
            //    }
            //}

            //test de coh�rence.
            //Cr�erLeGrapheVariableValeur();
            //gvv = _graphe;
            //if (gvv.variables.Count > gvv.valeurs.Count)
            //{
            //    Console.Out.WriteLine("error");
            //}
            //if (!RechercheDuCouplageMaximal(gvv))
            //{
            //    if (!RechercheCouplageMaximumAPartirDuCouplageMaximal(gvv))
            //    {
            //        Console.Out.WriteLine("error");
            //    };
            //}
            return true;
        }


        private void Cr�erLeGrapheVariableValeur()
        {
            var gvv = new GrapheVariableValeur<CSPVariable, object>();
            foreach (CSPVariable ed in _X)
            {
                var nx=gvv.AjouterVariable(ed);

                if (ed.Instantiated == false)
                {
                    foreach (object i in ed.Domaine)
                    {
                        gvv.AjouterValeur(nx, i);
                    }
                }
                else
                {
                    gvv.AjouterValeur(nx, ed.ElementChoisi);
                }
            }
            _graphe = gvv;
        }
        //private void AppliquerLesChangementsSurLeGraphe(GrapheVariableValeur<Tvar, Tval> gvv, Dictionary<EnsembleDiscret, List<int>> changements)
        //{
        //    foreach (KeyValuePair<EnsembleDiscret, List<int>> kvp in changements)
        //    {
        //        foreach (ArcCouplant ac in new List<ArcCouplant>( gvv.variables[kvp.Key].variablesCoupl�es))
        //        {
        //            if (kvp.Value.Contains(ac.valeur.v))
        //            {
        //                gvv.variables[kvp.Key].variablesCoupl�es.Remove(ac);
        //                gvv.valeurs[ac.valeur.v].variablesCoupl�es.Remove(ac);
        //                if (gvv.valeurs[ac.valeur.v].variablesCoupl�es.Count == 0)
        //                {
        //                    gvv.valeurs.Remove(ac.valeur.v);
        //                }
        //            }
        //        }
        //    }
        //}

        private Dictionary<CSPVariable, List<object>> SupprimerLesArcsNonMarqu�s( GrapheVariableValeur<CSPVariable, object> gvv)
        {
            var ensemblesModifi�s = new Dictionary<CSPVariable, List<object>>();
            foreach (NoeudX<CSPVariable> nx in gvv.variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCoupl�es)
                {
                    if (!ac.marqu� && !ac.couplant)
                    {
                        //Console.Out.WriteLine(ac.variable.ed.position + " -- " + ac.valeur.v);
                        if (!ensemblesModifi�s.ContainsKey(nx.ed))
                        {
                            nx.ed.Push();
                            ensemblesModifi�s.Add(nx.ed, new List<object>());
                        }
                        nx.ed.Domaine.Remove(((NoeudV<object>)ac.valeur).v);
                        ensemblesModifi�s[nx.ed].Add(((NoeudV<object>)ac.valeur).v);
                    }
                }
            }
            //Console.WriteLine(ensemblesModifi�s.Count +" arcs supprim�s.");
            return ensemblesModifi�s;
        }

        private void AfficherLeGrapheOrient�AvecCouplageMaximum(GrapheVariableValeur<CSPVariable, object> gvv)
        {
            foreach (NoeudX<CSPVariable> nx in gvv.variables.Values)
            {
                Console.Out.Write(nx.ed.ToString() + " -- ");
                foreach (ArcCouplant ac in nx.variablesCoupl�es)
                {
                    Console.Out.Write(ac.valeur.ToString().PadRight(3));
                    if (ac.couplant)
                    {
                        Console.Out.Write("<couplant) ");
                    }
                }
                Console.Out.WriteLine();
            }
        }

        private void AfficherLesComposantesConnexes(GrapheVariableValeur<CSPVariable, object> gvv)
        {

            int composanteConnexe = 1;
            bool resteComposante = true;
            while (resteComposante)
            {
                resteComposante = false;

                foreach (Noeud n in gvv.Enum�rateurDeNoeud())
                {
                    if (n.num�roComposanteConnexe == composanteConnexe)
                    {

                        if (n is NoeudX<CSPVariable>)
                        {
                            NoeudX<CSPVariable> nx = (NoeudX<CSPVariable>)n;
                            Console.Out.Write(" [" + nx.ed.ToString() + "] ");
                        }
                        else
                        {
                            NoeudV<object> nv = (NoeudV<object>)n;
                            Console.Out.Write(nv.v.ToString().PadRight(3));
                        }
                        resteComposante = true;
                    }
                }
                Console.Out.Write(" Composante " + composanteConnexe);
                Console.Out.WriteLine();
                composanteConnexe++;
            }
            foreach (Noeud n in gvv.Enum�rateurDeNoeud())
            {
                foreach (ArcCouplant ac in n.suivants)
                {
                    Noeud suivant;
                    if (n is NoeudX<CSPVariable>)
                    {
                        suivant = ac.valeur;
                    }
                    else
                    {
                        suivant = ac.variable;
                    }
                    if (suivant.num�roComposanteConnexe == n.num�roComposanteConnexe && n.num�roComposanteConnexe > 0)
                    {
                        if (ac.marqu� == false)
                        {
                            Console.Out.WriteLine("Erreur: " + n + suivant + "<-" + n.num�roComposanteConnexe);
                            Console.In.Read();
                        }
                    }
                }
            }
            Console.Out.WriteLine("----- comonconn");
        }

    }
}
