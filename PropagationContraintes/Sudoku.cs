using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ConstraintProgramming;

namespace PropagationContraintes
{
    public class Sudoku : CSPProblem
    {
        public CSPProblem reference;

        int _size;
        public Sudoku(int size)
            : base()
        {
            _size = size;
            for (int i = 0; i < size * size; i++)
            {
                Variables.Add(new CSPVariableInteger("e"+i.ToString(),1, size));
            }
            //InitialiserLesEnsemblesTriés();

            //for (int k = 1; k <= size; k++)
            //{
            //    for (int i = 1; i <= size; i++)
            //    {
            //        for (int j = i + 1; j <= size; j++)
            //        {
            //            int e1 = XYversLinéaire(i, k);
            //            int e2 = XYversLinéaire(j, k);
            //            AddConstraint(e1, e2, new BinConsDelegate(new Constraint<object>().AppliquerContrainteDifférents));
            //            e1 = XYversLinéaire(k, i);
            //            e2 = XYversLinéaire(k, j);
            //            AddConstraint(e1, e2, new BinConsDelegate(new Constraint<object>().AppliquerContrainteDifférents));
            //        }
            //    }
            //}
            //lignes
            for (int i = 1; i <= size; i++)
            {
                var row = new List<CSPVariable>();
                for (int j = 1; j <= size; j++)
                {
                    int e1 = XYversLinéaire(i, j);
                    row.Add( Variables[e1]);

                }
                ContraintesGlobales.Add(new CSPContrainteTousDifferents(row,"ligne"+i.ToString()));
            }
            //colonnes
            for (int i = 1; i <= size; i++)
            {
                var row = new List<CSPVariable>();
                for (int j = 1; j <= size; j++)
                {
                    int e1 = XYversLinéaire(j, i);
                    row.Add(Variables[e1]);

                }
                ContraintesGlobales.Add(new CSPContrainteTousDifferents(row,"col"+i.ToString()));
            }



            int ss = (int)Math.Sqrt(size);
            //for (int xc = 0; xc < ss; xc++)
            //{
            //    for (int yc = 0; yc < ss; yc++)
            //    {
            //        for (int i = 1 + xc * ss; i <= ss * (xc + 1); i++)
            //        {
            //            for (int j = 1 + yc * ss; j <= ss * (yc + 1); j++)
            //            {
            //                for (int m = 1 + xc * ss; m <= ss * (xc + 1); m++)
            //                {
            //                    for (int n = j + 1; n <= ss * (yc + 1); n++)
            //                    {
            //                        if (m != i)
            //                        {
            //                            int e1 = XYversLinéaire(i, j);
            //                            int e2 = XYversLinéaire(m, n);
            //                            //Console.Out.WriteLine(e1 + " ("+i+","+j+") : " + e2 + " ("+m+","+n+")");
            //                            AddConstraint(e1, e2, new BinConsDelegate(new Constraint<object>().AppliquerContrainteDifférents));
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            ss = (int)Math.Sqrt(size);
            for (int xc = 0; xc < ss; xc++)
            {
                for (int yc = 0; yc < ss; yc++)
                {
                    var tousdiff = new List<CSPVariable>();
                    for (int i = 1 + xc * ss; i <= ss * (xc + 1); i++)
                    {
                        for (int j = 1 + yc * ss; j <= ss * (yc + 1); j++)
                        {

                            int e1 = XYversLinéaire(i, j);
                            tousdiff.Add(Variables[e1]);

                        }

                    }
                    ContraintesGlobales.Add(new CSPContrainteTousDifferents(tousdiff,"carre"+xc.ToString()+","+yc.ToString()));
                }
            }
        }
        private int XYversLinéaire(int x, int y)
        {
            return (x - 1) * _size + (y - 1);
        }
        public CSPVariable GetVariable(int ligne, int col)
        {
            return Variables[XYversLinéaire(ligne, col)];
        }
        public void FixerCase(int i, int j, int v)
        {
            i--;
            j--;
            FixerEnsembleDiscret(i * _size + j, v);
        }
        private void FixerEnsembleDiscret(int position, int v)
        {
            Variables[position].Domaine.Clear();
            Variables[position].Domaine.Add(v);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">chaine de caractère contenant les valeurs connues séparées par des virgules.Les inconnues sont laissées vides.</param>
        public void ChargerUnProblème(string s)
        {
            string[] values = s.Split(new char[] { ',' });
            if (values.Length != Dimension)
            {
                Console.Out.WriteLine("Le problème à charger n'a pas la bonne taille:" + values.Length);
                return;
            }
            int i = 0;
            foreach (string e in values)
            {
                if (!string.IsNullOrEmpty(e))
                {
                    int j = int.Parse(e);
                    FixerEnsembleDiscret(i, j);
                }
                i++;
            }

        }
        public void ChargerUneSolutionRéférence(string s)
        {
            reference = ChargerEspaceReference(s);
            if (reference.Dimension != Dimension)
            {
                Console.Out.WriteLine("La référence n'a pas la bonne taille:" + reference.Dimension);
            }
        }
        private CSPProblem ChargerEspaceReference(string s)
        {
            CSPProblem E = new CSPProblem();
            string[] values = s.Split(new char[] { ',' });
            int i = 0;
            foreach (string e in values)
            {
                int j = int.Parse(e);
                E.Variables.Add(new CSPVariableInteger("e" + i.ToString(),j,j));
                i++;
            }
            return E;
        }
        public void PrintProb()
        {
            int i = 0, j = 0, k = 0, petiteTaille;
            petiteTaille = (int)Math.Sqrt(_size);
            foreach (CSPVariable ed in Variables)
            {
                i++;
                if (ed.Domaine.Count == 1)
                {
                    Console.Out.Write(ed.GetFirstElement().ToString().PadRight(3));
                }
                else
                {
                    Console.Out.Write(".  ");
                }
                if (i == petiteTaille)
                {
                    Console.Out.Write("|");
                    i = 0;
                    j++;
                    if (j == petiteTaille)
                    {
                        Console.Out.WriteLine();
                        j = 0;
                        k++;
                        if (k == petiteTaille)
                        {
                            Console.Out.WriteLine("-".PadRight(_size * 3, '-'));
                            k = 0;
                        }
                    }
                }
            }
        }
        public void PrintProblèmeEnLigne()
        {
            foreach (CSPVariable ed in Variables)
            {
                Console.Out.Write((ed.Domaine.Count == 1 ? ed.GetFirstElement().ToString() : "") + ",");
            }
            Console.Out.Write("\b \n");
        }
        public void PrintEspaceDeRecherche()
        {
            int i = 0;

            foreach (CSPVariable ed in Variables)
            {
                i++;
                if (ed.Domaine.Count == 1)
                {
                    Console.Out.Write(ed.GetFirstElement().ToString() + ",");
                }
                else
                {
                    foreach (int e in ed.Domaine)
                    {
                        Console.Out.Write(e + ";");
                    }
                }
                if (i == _size)
                {
                    Console.Out.Write("\n");
                    i = 0;

                }
            }
        }

        public override bool ComparerALaReference()
        {
            if (reference == null) return true;
            for (int i = 0; i < reference.Variables.Count; i++)
            {
                if (!(this.Variables[i].ElementChoisi.Equals(reference.Variables[i].GetFirstElement())))
                {
                    return false;
                }
            }
            return true;
        }
        public override bool CohérenceDeLaSolution()
        {
            int somme = 0;
            for (int i = 0; i < _size; i++)
            {
                somme = 0;
                for (int j = 0; j < _size; j++)
                {
                    CSPVariable ed=Variables[i*_size+j];
                    if (ed.Instantiated == false) return false;
                    somme += (int)ed.ElementChoisi;
                }
                if (somme != _size * (_size + 1) / 2) return false;

                somme = 0;
                for (int j = 0; j < _size; j++)
                {
                    CSPVariable ed = Variables[j * _size + i];
                    if (ed.Instantiated == false) return false;
                    somme += (int)ed.ElementChoisi;
                }
                if (somme != _size * (_size + 1) / 2) return false;
            }
            int petiteTaille=(int)Math.Sqrt(_size);
            for (int m = 0; m < petiteTaille; m++)
            {
                for (int n = 0; n < petiteTaille; n++)
                {
                    somme = 0;
                    for (int i = m * petiteTaille; i < (m + 1) * petiteTaille; i++)
                    {
                        for (int j = n * petiteTaille; j < (n + 1) * petiteTaille; j++)
                        {
                            CSPVariable ed = Variables[i * _size + j];
                            if (ed.Instantiated == false) return false;
                            somme +=(int) ed.ElementChoisi;
                        }
                    }
                    if (somme != _size * (_size + 1) / 2) return false;
                }
            }
            return true;
        }
    }
}
