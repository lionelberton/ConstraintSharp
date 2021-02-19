using System;
using System.Collections.Generic;
using System.Text;

namespace PropagationContraintes
{
    class ContrainteTousDifferents
    {
        private List<EnsembleDiscret> _X;
        private GrapheVariableValeur _graphe;
        public ContrainteTousDifferents(List<EnsembleDiscret> X)
        {
            _X = X;
            ContrainteGlobaleDelegate del = new ContrainteGlobaleDelegate(VerifierCoh�renceArcGlobale);
            foreach (EnsembleDiscret ed in X)
            {
                ed.contraintesGlobales.Add(del);
            }
        }

        private  void Cr�erLeGrapheVariableValeur()
        {
            GrapheVariableValeur gvv = new GrapheVariableValeur();
            foreach (EnsembleDiscret ed in _X)
            {
                if (ed.Instantiated == false)
                {
                    gvv.AjouterVariable(ed);
                    foreach (int i in ed._elements.Keys)
                    {
                        gvv.AjouterValeur(ed, i);
                    }
                }
            }
            _graphe = gvv;
        }
        public bool VerifierCoh�renceArcGlobale(Dictionary<EnsembleDiscret,List<int>> changements)
        {
            //if (_graphe == null)
            //{
                Cr�erLeGrapheVariableValeur();
            //}
            GrapheVariableValeur gvv = _graphe;
            if (changements != null)
            {
                AppliquerLesChangementsSurLeGraphe(gvv, changements);
            }
            if (gvv.variables.Count > gvv.valeurs.Count) return false;
            if (!RechercheDuCouplageMaximal(gvv))
            {
                if (!RechercheCouplageMaximumAPartirDuCouplageMaximal(gvv)) return false;
            }
            TransformerEnGrapheOrient�(gvv);
            //AfficherLeGrapheOrient�AvecCouplageMaximum(gvv);
            MarquerLesCheminsPartantDunSommetLibre(gvv);
            MarquerLesCyclesPairs(gvv);
            //AfficherLesComposantesConnexes(gvv);
            SupprimerLesArcsNonMarqu�s(gvv);
            //foreach (KeyValuePair<EnsembleDiscret, List<int>> kvp in SupprimerLesArcsNonMarqu�s(gvv))
            //{
            //    foreach (ContrainteGlobaleDelegate del in kvp.Key.contraintesGlobales)
            //    {
            //        if (!del(null)) return false;
            //    }
            //}

            //test de coh�rence.
            Cr�erLeGrapheVariableValeur();
            gvv = _graphe;
            if (gvv.variables.Count > gvv.valeurs.Count)
            {
                Console.Out.WriteLine("error");
            }
            if (!RechercheDuCouplageMaximal(gvv))
            {
                if (!RechercheCouplageMaximumAPartirDuCouplageMaximal(gvv))
                {
                    Console.Out.WriteLine("error");
                };
            }
            return true;
        }
        private void AppliquerLesChangementsSurLeGraphe(GrapheVariableValeur gvv, Dictionary<EnsembleDiscret, List<int>> changements)
        {
            foreach (KeyValuePair<EnsembleDiscret, List<int>> kvp in changements)
            {
                foreach (ArcCouplant ac in new List<ArcCouplant>( gvv.variables[kvp.Key].variablesCoupl�es))
                {
                    if (kvp.Value.Contains(ac.valeur.v))
                    {
                        gvv.variables[kvp.Key].variablesCoupl�es.Remove(ac);
                        gvv.valeurs[ac.valeur.v].variablesCoupl�es.Remove(ac);
                        if (gvv.valeurs[ac.valeur.v].variablesCoupl�es.Count == 0)
                        {
                            gvv.valeurs.Remove(ac.valeur.v);
                        }
                    }
                }
            }
        }

        private bool RechercheDuCouplageMaximal(GrapheVariableValeur gvv)
        {
            int n = 0;
            foreach (Noeud nx in gvv.variables.Values)
            {
                if (!nx.coupl�es)
                {
                    foreach (ArcCouplant ac in nx.variablesCoupl�es)
                    {
                        if (!ac.valeur.coupl�es)
                        {
                            ac.valeur.coupl�es = true;
                            ac.couplant = true;
                            nx.coupl�es = true;
                            n++;
                            break;
                        }
                    }
                }
                else
                {
                    n++;
                }
            }
            if (n == gvv.variables.Count) return true;
            return false;//couplage non maximal
        }
        private bool RechercheCouplageMaximumAPartirDuCouplageMaximal(GrapheVariableValeur gvv)
        {
            
            foreach (Noeud nx in gvv.variables.Values)
            {
                
                if (!nx.coupl�es)
                {
                    RAZTr�maux(gvv);
                    int num�roDeTr�maux=0;
                    if (!ExploreNoeud(nx,ref num�roDeTr�maux))
                    {
                        return false;
                    }
                    nx.coupl�es = true;
                }
            }
            return true;
        }
        private bool ExploreNoeud(Noeud n,ref int num�roDeTr�maux)
        {
            num�roDeTr�maux++;
            n.num�roDeTr�maux = num�roDeTr�maux;
            if (n is NoeudX)
            {
                
                foreach (ArcCouplant ac in n.variablesCoupl�es)
                {
                    if (ac.couplant == false && !ac.marqu�)
                    {
                        if (true)//(ac.valeur.num�roDeTr�maux == 0)
                        {
                            ac.marqu� = true;
                            if (ExploreNoeud(ac.valeur,ref num�roDeTr�maux))
                            {
                                ac.marqu� = false;
                                ac.couplant = true;
                                ac.valeur.coupl�es = true;
                                return true;
                            }
                            ac.marqu� = false;
                        }
                    }
                }
            }
            else
            {
                if (n.coupl�es == false)
                {
                    return true;
                }
                foreach (ArcCouplant ac in n.variablesCoupl�es)
                {
                    if (ac.couplant == true && !ac.marqu�)
                    {
                        if (true)//(ac.variable.num�roDeTr�maux == 0)
                        {
                            ac.marqu� = true;
                            if (ExploreNoeud(ac.variable,ref num�roDeTr�maux))
                            {
                                ac.marqu� = false;
                                ac.couplant = false;
                                n.coupl�es = false;
                                return true;
                            }
                            ac.marqu� = false;
                        }
                    }
                }
            }
            return false;
        }
        private void RAZTr�maux(GrapheVariableValeur gvv)
        {
            foreach (Noeud n in Enum�rateurDeNoeud(gvv))
            {
                n.num�roDeTr�maux = 0;
                n.pointDAttache = 0;
                n.num�roComposanteConnexe = 0;
            }
        }
        private void TransformerEnGrapheOrient�(GrapheVariableValeur gvv)
        {
            foreach (NoeudX nx in gvv.variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCoupl�es)
                {
                    if (ac.couplant)
                    {
                        nx.suivants.Add(ac);
                    }
                }
            }
            foreach (Noeud n in gvv.valeurs.Values)
            {
                foreach (ArcCouplant ac in n.variablesCoupl�es)
                {
                    if (!ac.couplant)
                    {
                        n.suivants.Add(ac);
                    }
                }
            }
        }
        private void MarquerLesCyclesPairs(GrapheVariableValeur gvv)
        {
            int num�roDeTr�maux = 0;
            int num�roLibreDeComposanteConnexe = 0;
            RAZTr�maux(gvv);
            foreach (Noeud nx in gvv.variables.Values)
            {
                if (nx.variablesCoupl�es.Count > 1)
                {
                    if (nx.num�roComposanteConnexe == 0 && nx.num�roDeTr�maux == 0)
                    {
                        //AfficherLeGrapheOrient�AvecCouplageMaximum(gvv); 
                        NumerotationDeTr�maux(nx, ref num�roDeTr�maux, ref num�roLibreDeComposanteConnexe);
                        
                    }
                }
            }
        }

        private void NumerotationDeTr�maux(Noeud n, ref int num�roDeTr�maux, ref int num�roLibreDeComposanteConnexe)
        {
            //Console.Out.Write(n);
            num�roDeTr�maux++;
            n.num�roDeTr�maux = num�roDeTr�maux;
            n.pointDAttache = num�roDeTr�maux;
            foreach (ArcCouplant ac in n.suivants)
            {
                Noeud suivant;
                if (n is NoeudX)
                {
                    suivant = ac.valeur;
                }
                else
                {
                    suivant = ac.variable;
                }
                if (suivant.num�roDeTr�maux == 0)
                {
                    NumerotationDeTr�maux(suivant, ref num�roDeTr�maux, ref num�roLibreDeComposanteConnexe);
                    n.pointDAttache = Math.Min(n.pointDAttache, suivant.pointDAttache);
                }
                else if (suivant.num�roComposanteConnexe == 0)
                {
                    n.pointDAttache = Math.Min(n.pointDAttache, suivant.num�roDeTr�maux);
                }
            }
            if (n.pointDAttache == n.num�roDeTr�maux)
            {
                num�roLibreDeComposanteConnexe++;
                MarquerLaComposanteConnexe(n, num�roLibreDeComposanteConnexe);
            }
            //Console.Out.Write("*");
        }
        private void MarquerLaComposanteConnexe(Noeud n, int num�roLibreDeComposanteConnexe)
        {
            //Console.Out.Write(n +"<-"+num�roLibreDeComposanteConnexe +")");
            n.num�roComposanteConnexe = num�roLibreDeComposanteConnexe;
            Noeud suivant;
            foreach (ArcCouplant ac in n.suivants)
            {
                if (n is NoeudX)
                {
                    suivant = ac.valeur;
                }
                else
                {
                    suivant = ac.variable;
                }
                if (suivant.num�roComposanteConnexe == n.num�roComposanteConnexe)
                {
                    ac.marqu� = true;
                }
                if (suivant.num�roDeTr�maux > n.num�roDeTr�maux && suivant.num�roComposanteConnexe == 0)
                {
                    ac.marqu� = true;
                    MarquerLaComposanteConnexe(suivant, num�roLibreDeComposanteConnexe);
                }
            }
            //Console.Out.Write("<*");
        }

        private void MarquerLeCycle(Stack<ArcCouplant> cycle)
        {
            foreach (ArcCouplant ac in cycle)
            {
                ac.marqu� = true;
            }
        }
        private void MarquerLesCheminsPartantDunSommetLibre(GrapheVariableValeur gvv)
        {
            foreach (Noeud nv in gvv.valeurs.Values)
            {
                if (nv.coupl�es == false)
                {
                    RechercheEnProfondeurDepuisUnSommetLibre(nv);
                }
            }
        }
        private void RechercheEnProfondeurDepuisUnSommetLibre(Noeud n)
        {
            foreach (ArcCouplant ac in n.suivants)
            {
                if (!ac.marqu�)
                {
                    ac.marqu� = true;
                    Noeud suivant;
                    if (n is NoeudX)
                    {
                        suivant = ac.valeur;
                    }
                    else
                    {
                        suivant = ac.variable;
                    }
                    RechercheEnProfondeurDepuisUnSommetLibre(suivant);
                }
            }
        }

        private Dictionary<EnsembleDiscret, List<int>> SupprimerLesArcsNonMarqu�s(GrapheVariableValeur gvv)
        {
            Dictionary<EnsembleDiscret, List<int>> ensemblesModifi�s = new Dictionary<EnsembleDiscret, List<int>>();
            foreach (NoeudX nx in gvv.variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCoupl�es)
                {
                    if (!ac.marqu� && !ac.couplant)
                    {
                        //Console.Out.WriteLine(ac.variable.ed.position + " -- " + ac.valeur.v);
                        nx.ed._elements.Remove(ac.valeur.v);
                        if (!ensemblesModifi�s.ContainsKey(nx.ed))
                        {
                            ensemblesModifi�s.Add(nx.ed, new List<int>());
                        }
                        ensemblesModifi�s[nx.ed].Add(ac.valeur.v);
                    }
                }
            }
            return ensemblesModifi�s;
        }

        private void AfficherLeGrapheOrient�AvecCouplageMaximum(GrapheVariableValeur gvv)
        {
            foreach (NoeudX nx in gvv.variables.Values)
            {
                Console.Out.Write(nx.ed.position + " -- ");
                foreach (ArcCouplant ac in nx.variablesCoupl�es)
                {
                    Console.Out.Write(ac.valeur.v.ToString().PadRight(3));
                    if (ac.couplant)
                    {
                        Console.Out.Write("<couplant) ");
                    }
                }
                Console.Out.WriteLine();
            }
        }

        private void AfficherLesComposantesConnexes(GrapheVariableValeur gvv)
        {

            int composanteConnexe = 1;
            bool resteComposante = true;
            while (resteComposante)
            {
                resteComposante = false;
                 
                foreach (Noeud n in Enum�rateurDeNoeud(gvv))
                {
                    if (n.num�roComposanteConnexe == composanteConnexe)
                    {
                        
                        if (n is NoeudX)
                        {
                            NoeudX nx = (NoeudX)n;
                            Console.Out.Write(" [" + nx.ed.position + "] ");
                        }
                        else
                        {
                            NoeudV nv = (NoeudV)n;
                            Console.Out.Write(nv.v.ToString().PadRight(3));
                        }
                        resteComposante = true;
                    }
                }
                Console.Out.Write(" Composante " + composanteConnexe);
                Console.Out.WriteLine();
                composanteConnexe++;
            }
            foreach (Noeud n in Enum�rateurDeNoeud(gvv))
            {
                foreach (ArcCouplant ac in n.suivants)
                {
                    Noeud suivant;
                    if (n is NoeudX)
                    {
                        suivant = ac.valeur;
                    }
                    else
                    {
                        suivant = ac.variable;
                    }
                    if (suivant.num�roComposanteConnexe == n.num�roComposanteConnexe && n.num�roComposanteConnexe>0)
                    {
                        if (ac.marqu� == false)
                        {
                            Console.Out.WriteLine("Erreur: " + n + suivant + "<-"+n.num�roComposanteConnexe);
                            Console.In.Read();
                        }
                    }
                }
            }
            Console.Out.WriteLine("----- comonconn");
        }
        private IEnumerable<Noeud> Enum�rateurDeNoeud(GrapheVariableValeur gvv)
        {
            foreach (NoeudX nx in gvv.variables.Values)
            {
                yield return (Noeud)nx;
            }
            foreach (NoeudV nv in gvv.valeurs.Values)
            {
                yield return (Noeud)nv;
            }
        }
    }


    class GrapheVariableValeur
    {
        public Dictionary<EnsembleDiscret, NoeudX> variables;
        public Dictionary<int, NoeudV> valeurs;

        public GrapheVariableValeur()
        {
            variables = new Dictionary<EnsembleDiscret, NoeudX>();
            valeurs = new Dictionary<int, NoeudV>();
        }

        public void AjouterVariable(EnsembleDiscret ed)
        {
            variables.Add(ed, new NoeudX(ed));
        }
        public void AjouterValeur(EnsembleDiscret ed, int i)
        {
            NoeudX nx = variables[ed];
            NoeudV nv;
            if (valeurs.ContainsKey(i))
            {
                nv = valeurs[i];
            }
            else
            {
                nv = new NoeudV(i);
                valeurs.Add(i, nv);
            }
            ArcCouplant ac = new ArcCouplant(nx, nv);
            nv.variablesCoupl�es.Add(ac);
            nx.variablesCoupl�es.Add(ac);
        }
    }

    class NoeudX : Noeud
    {
        public EnsembleDiscret ed;

        public NoeudX(EnsembleDiscret e)
            : base()
        {
            ed = e;
        }
        public override string ToString()
        {
            return " [" +ed.position+"] ";
        }
    }
    class NoeudV : Noeud
    {
        public int v;

        public NoeudV(int i)
            : base()
        {
            this.v = i;
        }
        public override string ToString()
        {
            return " " +v.ToString()+" ";
        }
    }
    class Noeud
    {
        //int v;
        public List<ArcCouplant> variablesCoupl�es;
        public List<ArcCouplant> suivants;
        public bool coupl�es = false;
        public int num�roDeTr�maux = 0;
        public int pointDAttache = 0;
        public int num�roComposanteConnexe = 0;
        
        public Noeud()
        {
            //this.v = v;
            variablesCoupl�es = new List<ArcCouplant>();
            suivants = new List<ArcCouplant>();
        }
    }
    class ArcCouplant
    {
        public NoeudX variable;
        public NoeudV valeur;
        public bool couplant = false;
        public bool marqu� = false;
        public bool d�j�Pass� = false;
        public ArcCouplant(NoeudX nx, NoeudV nv)
        {
            variable = nx;
            valeur = nv;
        }
    }
}
