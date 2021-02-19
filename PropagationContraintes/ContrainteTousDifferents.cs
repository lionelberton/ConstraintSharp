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
            ContrainteGlobaleDelegate del = new ContrainteGlobaleDelegate(VerifierCohérenceArcGlobale);
            foreach (EnsembleDiscret ed in X)
            {
                ed.contraintesGlobales.Add(del);
            }
        }

        private  void CréerLeGrapheVariableValeur()
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
        public bool VerifierCohérenceArcGlobale(Dictionary<EnsembleDiscret,List<int>> changements)
        {
            //if (_graphe == null)
            //{
                CréerLeGrapheVariableValeur();
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
            TransformerEnGrapheOrienté(gvv);
            //AfficherLeGrapheOrientéAvecCouplageMaximum(gvv);
            MarquerLesCheminsPartantDunSommetLibre(gvv);
            MarquerLesCyclesPairs(gvv);
            //AfficherLesComposantesConnexes(gvv);
            SupprimerLesArcsNonMarqués(gvv);
            //foreach (KeyValuePair<EnsembleDiscret, List<int>> kvp in SupprimerLesArcsNonMarqués(gvv))
            //{
            //    foreach (ContrainteGlobaleDelegate del in kvp.Key.contraintesGlobales)
            //    {
            //        if (!del(null)) return false;
            //    }
            //}

            //test de cohérence.
            CréerLeGrapheVariableValeur();
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
                foreach (ArcCouplant ac in new List<ArcCouplant>( gvv.variables[kvp.Key].variablesCouplées))
                {
                    if (kvp.Value.Contains(ac.valeur.v))
                    {
                        gvv.variables[kvp.Key].variablesCouplées.Remove(ac);
                        gvv.valeurs[ac.valeur.v].variablesCouplées.Remove(ac);
                        if (gvv.valeurs[ac.valeur.v].variablesCouplées.Count == 0)
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
                if (!nx.couplées)
                {
                    foreach (ArcCouplant ac in nx.variablesCouplées)
                    {
                        if (!ac.valeur.couplées)
                        {
                            ac.valeur.couplées = true;
                            ac.couplant = true;
                            nx.couplées = true;
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
                
                if (!nx.couplées)
                {
                    RAZTrémaux(gvv);
                    int numéroDeTrémaux=0;
                    if (!ExploreNoeud(nx,ref numéroDeTrémaux))
                    {
                        return false;
                    }
                    nx.couplées = true;
                }
            }
            return true;
        }
        private bool ExploreNoeud(Noeud n,ref int numéroDeTrémaux)
        {
            numéroDeTrémaux++;
            n.numéroDeTrémaux = numéroDeTrémaux;
            if (n is NoeudX)
            {
                
                foreach (ArcCouplant ac in n.variablesCouplées)
                {
                    if (ac.couplant == false && !ac.marqué)
                    {
                        if (true)//(ac.valeur.numéroDeTrémaux == 0)
                        {
                            ac.marqué = true;
                            if (ExploreNoeud(ac.valeur,ref numéroDeTrémaux))
                            {
                                ac.marqué = false;
                                ac.couplant = true;
                                ac.valeur.couplées = true;
                                return true;
                            }
                            ac.marqué = false;
                        }
                    }
                }
            }
            else
            {
                if (n.couplées == false)
                {
                    return true;
                }
                foreach (ArcCouplant ac in n.variablesCouplées)
                {
                    if (ac.couplant == true && !ac.marqué)
                    {
                        if (true)//(ac.variable.numéroDeTrémaux == 0)
                        {
                            ac.marqué = true;
                            if (ExploreNoeud(ac.variable,ref numéroDeTrémaux))
                            {
                                ac.marqué = false;
                                ac.couplant = false;
                                n.couplées = false;
                                return true;
                            }
                            ac.marqué = false;
                        }
                    }
                }
            }
            return false;
        }
        private void RAZTrémaux(GrapheVariableValeur gvv)
        {
            foreach (Noeud n in EnumérateurDeNoeud(gvv))
            {
                n.numéroDeTrémaux = 0;
                n.pointDAttache = 0;
                n.numéroComposanteConnexe = 0;
            }
        }
        private void TransformerEnGrapheOrienté(GrapheVariableValeur gvv)
        {
            foreach (NoeudX nx in gvv.variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCouplées)
                {
                    if (ac.couplant)
                    {
                        nx.suivants.Add(ac);
                    }
                }
            }
            foreach (Noeud n in gvv.valeurs.Values)
            {
                foreach (ArcCouplant ac in n.variablesCouplées)
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
            int numéroDeTrémaux = 0;
            int numéroLibreDeComposanteConnexe = 0;
            RAZTrémaux(gvv);
            foreach (Noeud nx in gvv.variables.Values)
            {
                if (nx.variablesCouplées.Count > 1)
                {
                    if (nx.numéroComposanteConnexe == 0 && nx.numéroDeTrémaux == 0)
                    {
                        //AfficherLeGrapheOrientéAvecCouplageMaximum(gvv); 
                        NumerotationDeTrémaux(nx, ref numéroDeTrémaux, ref numéroLibreDeComposanteConnexe);
                        
                    }
                }
            }
        }

        private void NumerotationDeTrémaux(Noeud n, ref int numéroDeTrémaux, ref int numéroLibreDeComposanteConnexe)
        {
            //Console.Out.Write(n);
            numéroDeTrémaux++;
            n.numéroDeTrémaux = numéroDeTrémaux;
            n.pointDAttache = numéroDeTrémaux;
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
                if (suivant.numéroDeTrémaux == 0)
                {
                    NumerotationDeTrémaux(suivant, ref numéroDeTrémaux, ref numéroLibreDeComposanteConnexe);
                    n.pointDAttache = Math.Min(n.pointDAttache, suivant.pointDAttache);
                }
                else if (suivant.numéroComposanteConnexe == 0)
                {
                    n.pointDAttache = Math.Min(n.pointDAttache, suivant.numéroDeTrémaux);
                }
            }
            if (n.pointDAttache == n.numéroDeTrémaux)
            {
                numéroLibreDeComposanteConnexe++;
                MarquerLaComposanteConnexe(n, numéroLibreDeComposanteConnexe);
            }
            //Console.Out.Write("*");
        }
        private void MarquerLaComposanteConnexe(Noeud n, int numéroLibreDeComposanteConnexe)
        {
            //Console.Out.Write(n +"<-"+numéroLibreDeComposanteConnexe +")");
            n.numéroComposanteConnexe = numéroLibreDeComposanteConnexe;
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
                if (suivant.numéroComposanteConnexe == n.numéroComposanteConnexe)
                {
                    ac.marqué = true;
                }
                if (suivant.numéroDeTrémaux > n.numéroDeTrémaux && suivant.numéroComposanteConnexe == 0)
                {
                    ac.marqué = true;
                    MarquerLaComposanteConnexe(suivant, numéroLibreDeComposanteConnexe);
                }
            }
            //Console.Out.Write("<*");
        }

        private void MarquerLeCycle(Stack<ArcCouplant> cycle)
        {
            foreach (ArcCouplant ac in cycle)
            {
                ac.marqué = true;
            }
        }
        private void MarquerLesCheminsPartantDunSommetLibre(GrapheVariableValeur gvv)
        {
            foreach (Noeud nv in gvv.valeurs.Values)
            {
                if (nv.couplées == false)
                {
                    RechercheEnProfondeurDepuisUnSommetLibre(nv);
                }
            }
        }
        private void RechercheEnProfondeurDepuisUnSommetLibre(Noeud n)
        {
            foreach (ArcCouplant ac in n.suivants)
            {
                if (!ac.marqué)
                {
                    ac.marqué = true;
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

        private Dictionary<EnsembleDiscret, List<int>> SupprimerLesArcsNonMarqués(GrapheVariableValeur gvv)
        {
            Dictionary<EnsembleDiscret, List<int>> ensemblesModifiés = new Dictionary<EnsembleDiscret, List<int>>();
            foreach (NoeudX nx in gvv.variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCouplées)
                {
                    if (!ac.marqué && !ac.couplant)
                    {
                        //Console.Out.WriteLine(ac.variable.ed.position + " -- " + ac.valeur.v);
                        nx.ed._elements.Remove(ac.valeur.v);
                        if (!ensemblesModifiés.ContainsKey(nx.ed))
                        {
                            ensemblesModifiés.Add(nx.ed, new List<int>());
                        }
                        ensemblesModifiés[nx.ed].Add(ac.valeur.v);
                    }
                }
            }
            return ensemblesModifiés;
        }

        private void AfficherLeGrapheOrientéAvecCouplageMaximum(GrapheVariableValeur gvv)
        {
            foreach (NoeudX nx in gvv.variables.Values)
            {
                Console.Out.Write(nx.ed.position + " -- ");
                foreach (ArcCouplant ac in nx.variablesCouplées)
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
                 
                foreach (Noeud n in EnumérateurDeNoeud(gvv))
                {
                    if (n.numéroComposanteConnexe == composanteConnexe)
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
            foreach (Noeud n in EnumérateurDeNoeud(gvv))
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
                    if (suivant.numéroComposanteConnexe == n.numéroComposanteConnexe && n.numéroComposanteConnexe>0)
                    {
                        if (ac.marqué == false)
                        {
                            Console.Out.WriteLine("Erreur: " + n + suivant + "<-"+n.numéroComposanteConnexe);
                            Console.In.Read();
                        }
                    }
                }
            }
            Console.Out.WriteLine("----- comonconn");
        }
        private IEnumerable<Noeud> EnumérateurDeNoeud(GrapheVariableValeur gvv)
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
            nv.variablesCouplées.Add(ac);
            nx.variablesCouplées.Add(ac);
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
        public List<ArcCouplant> variablesCouplées;
        public List<ArcCouplant> suivants;
        public bool couplées = false;
        public int numéroDeTrémaux = 0;
        public int pointDAttache = 0;
        public int numéroComposanteConnexe = 0;
        
        public Noeud()
        {
            //this.v = v;
            variablesCouplées = new List<ArcCouplant>();
            suivants = new List<ArcCouplant>();
        }
    }
    class ArcCouplant
    {
        public NoeudX variable;
        public NoeudV valeur;
        public bool couplant = false;
        public bool marqué = false;
        public bool déjàPassé = false;
        public ArcCouplant(NoeudX nx, NoeudV nv)
        {
            variable = nx;
            valeur = nv;
        }
    }
}
