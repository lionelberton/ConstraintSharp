using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstraintProgramming.CSPLanguage.BipartiteGraph
{
    public class GrapheVariableValeur<Tvar, Tval>
    {
        public Dictionary<Tvar, NoeudX<Tvar>> variables;
        public Dictionary<Tval, NoeudV<Tval>> valeurs;

        public GrapheVariableValeur()
        {
            variables = new Dictionary<Tvar, NoeudX<Tvar>>();
            valeurs = new Dictionary<Tval, NoeudV<Tval>>();
        }

        public NoeudX<Tvar> AjouterVariable(Tvar ed)
        {
            NoeudX<Tvar> nv=new NoeudX<Tvar>(ed);
            variables.Add(ed, nv);
            return nv;
        }
        public void AjouterValeur(NoeudX<Tvar> nx, Tval i)
        {
            NoeudV<Tval> nv;
            if (valeurs.ContainsKey(i))
            {
                nv = valeurs[i];
            }
            else
            {
                nv = new NoeudV<Tval>(i);
                valeurs.Add(i, nv);
            }
            ArcCouplant ac = new ArcCouplant(nx, nv);
            nv.variablesCouplées.Add(ac);
            nx.variablesCouplées.Add(ac);
        }

        public bool RechercheDuCouplageMaximal()
        {
            int n = 0;
            foreach (Noeud nx in variables.Values)
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
            if (n == variables.Count) return true;
            return false;//couplage non maximal
        }
        public bool RechercheCouplageMaximumAPartirDuCouplageMaximal()
        {

            foreach (Noeud nx in variables.Values)
            {

                if (!nx.couplées)
                {
                    RAZTrémaux();
                    int numéroDeTrémaux = 0;
                    if (!ExploreNoeud(nx, ref numéroDeTrémaux))
                    {
                        return false;
                    }
                    nx.couplées = true;
                }
            }
            return true;
        }
        private bool ExploreNoeud(Noeud n, ref int numéroDeTrémaux)
        {
            numéroDeTrémaux++;
            n.numéroDeTrémaux = numéroDeTrémaux;
            if (n is NoeudX<Tvar>)
            {

                foreach (ArcCouplant ac in n.variablesCouplées)
                {
                    if (ac.couplant == false && !ac.marqué)
                    {
                        if (true)//(ac.valeur.numéroDeTrémaux == 0)
                        {
                            ac.marqué = true;
                            if (ExploreNoeud(ac.valeur, ref numéroDeTrémaux))
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
                            if (ExploreNoeud(ac.variable, ref numéroDeTrémaux))
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
        private void RAZTrémaux()
        {
            foreach (Noeud n in EnumérateurDeNoeud())
            {
                n.numéroDeTrémaux = 0;
                n.pointDAttache = 0;
                n.numéroComposanteConnexe = 0;
            }
        }
        public  void TransformerEnGrapheOrienté()
        {
            foreach (NoeudX<Tvar> nx in variables.Values)
            {
                foreach (ArcCouplant ac in nx.variablesCouplées)
                {
                    if (ac.couplant)
                    {
                        nx.suivants.Add(ac);
                    }
                }
            }
            foreach (Noeud n in valeurs.Values)
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
        public void MarquerLesCyclesPairs()
        {
            int numéroDeTrémaux = 0;
            int numéroLibreDeComposanteConnexe = 0;
            RAZTrémaux();
            foreach (Noeud nx in variables.Values)
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
                if (n is NoeudX<Tvar>)
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
                if (n is NoeudX<Tvar>)
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
        public void MarquerLesCheminsPartantDunSommetLibre()
        {
            foreach (Noeud nv in valeurs.Values)
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
                    if (n is NoeudX<Tvar>)
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
        public IEnumerable<Noeud> EnumérateurDeNoeud()
        {
            foreach (NoeudX<Tvar> nx in variables.Values)
            {
                yield return (Noeud)nx;
            }
            foreach (NoeudV<Tval> nv in valeurs.Values)
            {
                yield return (Noeud)nv;
            }
        }
    }
}
