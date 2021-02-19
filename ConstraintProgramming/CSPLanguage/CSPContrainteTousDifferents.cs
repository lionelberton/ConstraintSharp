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
            CréerLeGrapheVariableValeur();
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
            gvv.TransformerEnGrapheOrienté();
            //AfficherLeGrapheOrientéAvecCouplageMaximum(gvv);
            gvv.MarquerLesCheminsPartantDunSommetLibre();
            gvv.MarquerLesCyclesPairs();
            //AfficherLesComposantesConnexes(gvv);
            changements = SupprimerLesArcsNonMarqués(gvv);
            //foreach (KeyValuePair<EnsembleDiscret, List<int>> kvp in SupprimerLesArcsNonMarqués(gvv))
            //{
            //    foreach (ContrainteGlobaleDelegate del in kvp.Key.contraintesGlobales)
            //    {
            //        if (!del(null)) return false;
            //    }
            //}

            //test de cohérence.
            //CréerLeGrapheVariableValeur();
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


        private void CréerLeGrapheVariableValeur()
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
        //        foreach (ArcCouplant ac in new List<ArcCouplant>( gvv.variables[kvp.Key].variablesCouplées))
        //        {
        //            if (kvp.Value.Contains(ac.valeur.v))
        //            {
        //                gvv.variables[kvp.Key].variablesCouplées.Remove(ac);
        //                gvv.valeurs[ac.valeur.v].variablesCouplées.Remove(ac);
        //                if (gvv.valeurs[ac.valeur.v].variablesCouplées.Count == 0)
        //                {
        //                    gvv.valeurs.Remove(ac.valeur.v);
        //                }
        //            }
        //        }
        //    }
        //}

        private Dictionary<CSPVariable, List<object>> SupprimerLesArcsNonMarqués( GrapheVariableValeur<CSPVariable, object> gvv)
        {
            var ensemblesModifiés = new Dictionary<CSPVariable, List<object>>();
            foreach (NoeudX<CSPVariable> nx in gvv.variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCouplées)
                {
                    if (!ac.marqué && !ac.couplant)
                    {
                        //Console.Out.WriteLine(ac.variable.ed.position + " -- " + ac.valeur.v);
                        if (!ensemblesModifiés.ContainsKey(nx.ed))
                        {
                            nx.ed.Push();
                            ensemblesModifiés.Add(nx.ed, new List<object>());
                        }
                        nx.ed.Domaine.Remove(((NoeudV<object>)ac.valeur).v);
                        ensemblesModifiés[nx.ed].Add(((NoeudV<object>)ac.valeur).v);
                    }
                }
            }
            //Console.WriteLine(ensemblesModifiés.Count +" arcs supprimés.");
            return ensemblesModifiés;
        }

        private void AfficherLeGrapheOrientéAvecCouplageMaximum(GrapheVariableValeur<CSPVariable, object> gvv)
        {
            foreach (NoeudX<CSPVariable> nx in gvv.variables.Values)
            {
                Console.Out.Write(nx.ed.ToString() + " -- ");
                foreach (ArcCouplant ac in nx.variablesCouplées)
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

                foreach (Noeud n in gvv.EnumérateurDeNoeud())
                {
                    if (n.numéroComposanteConnexe == composanteConnexe)
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
            foreach (Noeud n in gvv.EnumérateurDeNoeud())
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
                    if (suivant.numéroComposanteConnexe == n.numéroComposanteConnexe && n.numéroComposanteConnexe > 0)
                    {
                        if (ac.marqué == false)
                        {
                            Console.Out.WriteLine("Erreur: " + n + suivant + "<-" + n.numéroComposanteConnexe);
                            Console.In.Read();
                        }
                    }
                }
            }
            Console.Out.WriteLine("----- comonconn");
        }

    }
}
