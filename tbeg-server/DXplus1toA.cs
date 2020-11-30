using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBeg
{
    class DXplus1toA : IFunctor<Func<string,Func<int,double>>, double>
    {
        
        // Fields:
        Type matrixValueType;
        Type BranchingDataType;

        List<Func<string, Func<int, double>>> _elements_of_FX; // all Elements in FX captured by the transitions system; alphaX(x_1),alphaX(x_2)...

        // Constructor:
        public DXplus1toA()
        {
            matrixValueType = typeof(double);
            BranchingDataType = typeof(Func<string,Func<int,double>>);
        }

        //maybe not used
        public DXplus1toA(List<int> states, Matrix<Func<string, Func<int, double>>, double> transitionsystem, string[] alphabet)
        {
            //_elements_of_FX = ReturnFX(states, transitionsystem, alphabet);
            matrixValueType = typeof(double);
        }
        public List<Func<string, Func<int, double>>> elements_of_FX
        {
            get
            {
                return _elements_of_FX;
            }

            set
            {
                _elements_of_FX= value;
            }
        }

        Type IFunctor<Func<string, Func<int, double>>, double>.matrixValueType
        {
            get
            {
                return matrixValueType;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        Type IFunctor<Func<string, Func<int, double>>, double>.BranchingDataType
        {
            get
            {
                return BranchingDataType;
            }

            set
            {
                throw new NotImplementedException();
            }
        }


        //methods used inside the game and winning strategy computation:
        //TODO optional method with given predicate liftings by the user, or just used inside implementation of the concrete functor
        /// <summary>
        /// Used during game:
        /// 2. Step Condition Check of the Duplicator: Fp(alpha(x)) smaller or equal Fp(alpha(y)) </summary>
        /// <param name="predicate"> the move of spoiler p_1</param>
        /// <param name="t1"> Fp(alpha(x))</param>
        /// <param name="t2"> Fp(alpha(y))</param>
        public bool CheckCondition2(List<int> predicate_1, List<int> predicate_2, int state_i, int state_j, int states, Matrix<Func<string, Func<int, double>>, double> transitionsystem, string[] alphabet)
        {
            Func<string, Func<int, double>> t1 = ReturnAlpha_X(state_i, states, transitionsystem, transitionsystem.Alphabet);
            Func<string, Func<int, double>> t2 = ReturnAlpha_X(state_j, states, transitionsystem, transitionsystem.Alphabet);

            Func<string, Func<int, double>> Fp1_t1 = ReturnFP(predicate_1,states)(t1);
            Func<string, Func<int, double>> Fp2_t2 = ReturnFP(predicate_2,states)(t2);

            bool ret = true;

            for (int i = 0; i < alphabet.Count(); i++)
            {
                double valueOne = Fp1_t1(alphabet[i])(1);
                double valueTwo = Fp2_t2(alphabet[i])(1);
                //check for both are termination otherwise not comparable based on the lifted order
                if ((valueTwo == -1) && (valueOne > -1)) return false;
                if ((valueOne == -1) && (valueTwo > -1)) return false;
                //both are not termination
                if (valueTwo < valueOne) return false;
            }

            return ret;
        }

        /*
        /// <summary>
        /// Used during computation of winning strategy:
        /// 2. Step Condition Check of the Duplicator: Fp(alpha(x)) smaller or equal Fp(alpha(y)) </summary>
        /// <param name="predicate"> the move of spoiler p_1</param>
        /// <param name="t1"> Fp(alpha(x))</param>
        /// <param name="t2"> Fp(alpha(y))</param>
        public bool CheckCondition2(List<int> predicate, int state_i, int state_j, int states, Matrix<Func<string, Func<int, double>>, double> transitionsystem, string[] alphabet)
        {
            Func<string, Func<int, double>> t1 = ReturnAlpha_X(state_i, states, transitionsystem, transitionsystem.Alphabet);
            Func<string, Func<int, double>> t2 = ReturnAlpha_X(state_j, states, transitionsystem, transitionsystem.Alphabet);

            Func<string, Func<int, double>> Fp1_t1 = ReturnFP(predicate,states)(t1);
            Func<string, Func<int, double>> Fp2_t2 = ReturnFP(predicate,states)(t2);

            bool ret = true;

            for (int i = 0; i < alphabet.Count(); i++ )
            {
                double valueOne = Fp1_t1(alphabet[i])(1);
                double valueTwo = Fp2_t2(alphabet[i])(1);
                //check for both are termination otherwise not comparable based on the lifted order
                if ((valueTwo == -1) && (valueOne > -1)) return false;
                if ((valueOne == -1) && (valueTwo > -1)) return false;
                //both are not termination
                if (valueTwo < valueOne) return false;
            }

            return ret;
        }
        */


        Func<List<int>,int, Func<Func<string, Func<int, double>>, Func<string, Func<int, double>>>> ReturnFP = (predicate,states) => new Func<Func<string, Func<int, double>>, Func<string, Func<int, double>>>(alphaX => GetElemtentF2(predicate, alphaX,states));

        private static Func<string, Func<int, double>> GetElemtentF2(List<int> p, Func<string, Func<int, double>> alphaX, int states)
        {
            Func<string, Func<int, double>> Fp = new Func<string, Func<int, double>>(alp => new Func<int,double>(state_intern => GetF2(alp,state_intern,p, alphaX, states)));
            return Fp;
        }

        private static double GetF2(string alp, int state_intern, List<int> p, Func<string, Func<int, double>> alphaX, int states)
        {
            List<int> notInp = new List<int>();
            for (int i = 0; i < states; i++)
            {
                if (!p.Contains(i)) notInp.Add(i);
            }
            double value =0;
         
                for (int i = 0; i < p.Count; i++)
                {
                    value = value + alphaX(alp)(p[i]);
                }

            double checkForTermination = 0;
                for (int i = 0; i < notInp.Count; i++)
                {
                    checkForTermination = checkForTermination + alphaX(alp)(notInp[i]);
                }
            if (value + checkForTermination < 1) return -1;
  
            return value;
        }

        // return type is an element in (DX+1)^A
        public Func<string, Func<int, double>> ReturnAlpha_X(int state, int states, Matrix<Func<string, Func<int, double>>, double> transitionsystem, string[] alphabet)
        {
            Func<string, Func<int, double>> alphaX = new Func<string, Func<int, double>>(alp => new Func<int,double> (state_intern => GetValue(state,alp,state_intern,transitionsystem, alphabet)));
            return alphaX;
        }


        //returnuning alpha(X) based on ts
        // transiton system as Func instead of Matrix would make things easier here (consistent types)
        private double GetValue(int state, string alp, int state_intern, Matrix<Func<string, Func<int, double>>, double> transitionsystem,string[] alphabet)
        {
            int states = transitionsystem.columns(); //number of states
            int var=0;
            for (int i=0; i< alphabet.Length;i++)
            {
                if (alphabet[i].Equals(alp)) var = i;
            }
            return transitionsystem.get(alphabet.Length * state_intern +var, state);
        }




        /// <summary>
        /// Return the 
        /// </summary>
        /// <param name="row">The row index of the entry to be set.</param>
        /// <param name="column">The column index of the entry to be set.</param>
        /// <param name="value">The value the entry gets.</param>
        public List<int> ReturnDirectSuccesors(int state, Matrix<Func<string, Func<int, double>>, double> ts, int states, int alphabet)
        {
            List<int> succesors = new List<int>();
            int countState = 0;
            for (int i = 0; i < ts.rows(); i++)
            {
                for (int j = 0; j < alphabet; j++)
                {
                    if (ts.get(i + j, state) > 0 && !succesors.Contains(countState)) succesors.Add(countState);
                }
                i = i + (alphabet - 1);
                countState++;
            }

            return succesors;
        }


        // GUI Helping methods:

        public List<string> GetRowHeadings(string[] alphabet, List<int> states, bool termination)
        {
            List<string> rowHeadings = new List<string>();
            for (int i = 0; i < states.Count; ++i)
            {
                for (int j = 0; j < alphabet.Length; ++j)
                {
                    rowHeadings.Add("(" + alphabet[j] + "," + (i + 1) + ")");
                }

            }
            if (termination) rowHeadings.Add("{\u2022}");

            return rowHeadings;
        }

        public int ReturnRowCount(string[] alphabet, int states, bool termination)
        {
            return alphabet.Length * states;
        }

        public Matrix<Func<string, Func<int, double>>, double> InitMatrixStandard(string[] content, string[] alphabet, List<int> states)
        {
            Matrix<Func<string, Func<int, double>>, double> transitionSystem = new Matrix<Func<string, Func<int, double>>, double>(alphabet.Length * states.Count, states.Count, this);
            for (int i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < alphabet.Length * states.Count; j++)
                {
                    try
                    {
                        double c = Double.Parse(content[i * (alphabet.Length * states.Count) + j]);
                        transitionSystem.set(j, i, c);
                    }
                    catch { }

                }
            }

            transitionSystem.functor = this;
            transitionSystem.Alphabet = alphabet;
            //TODO: Check for sum=1 or termination wrt to transition-label

            for (int i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < alphabet.Length; j++)
                {
                     double value=0;
                     for (int l = 0; l < states.Count; l++)
                     {
                         value = (double) value+  transitionSystem.get(l* alphabet.Length+j,i);
                     }
                     if (value != 0.0 && value != 1.0) throw new ArgumentException("Invalid input: e.g. for the state " + (i+1).ToString()+ "! Please validate your inputs!");

                }
            }

            return transitionSystem;
        }
        //Save and Load of transition systems from csv files
        public string SaveTransitionSystem(string filePath, string[] content, string optional)
        {
            //TODO: Exeption handling of input should happen directly in the model or GUI method
            if (content.Length == 3)
            {
                try
                {
                    string states = content[0];
                    string alphabetCSV = content[1];
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(states);
                    sb.AppendLine(alphabetCSV);
                    string transitionSystem = content[2];
                    sb.AppendLine(transitionSystem);
                    File.WriteAllText(filePath, sb.ToString());
                    return "ok";
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                return "Some Input is missing. Please check your Transitionsystem inputs.";
            }


        }



        public string[,] LoadTransitionSystemContent(string content, int col, int row, string optional)
        {
            string[,] contentTS = new string[col, row];
            string[] ts = content.Split(',');
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    contentTS[i, j] = ts[i * row + j];
                }
            }
            return contentTS;
        }

        //standard transformation captured by MVC
        public void GetGraph(ref Graph g)
        {
            throw new NotImplementedException();
        }

        //standard transformation captured by MVC
        public Matrix<Func<string, Func<int, double>>, double> GetTSfromGraph(Graph g, int states)
        {
            throw new NotImplementedException();
        }

        public string Fpalpha_xToString(int state, List<int> predicate, Matrix<Func<string, Func<int, double>>, double> ts)
        {
            string ret = "[";
            Func<string, Func<int, double>> t1 = ReturnAlpha_X(state, ts.columns(), ts, ts.Alphabet);
            Func<string, Func<int, double>> Fp_t1 = ReturnFP(predicate, ts.columns())(t1);
         
            for (int i = 0; i < ts.Alphabet.Count(); i++ )
            {
                double valueOne = Fp_t1(ts.Alphabet[i])(1);        
                      if ((valueOne == -1))       ret= ret + ts.Alphabet[i] + "-> t";
                      else ret= ret + ts.Alphabet[i] + "->"+ valueOne.ToString();
                if(i != ts.Alphabet.Count()-1)  ret= ret+",";
                 
            }
             
            ret = ret + "]";
            return ret;
        }

        public Matrix<Func<string, Func<int, double>>, double> InitMatrix(string[] content, string[] alphabet, List<int> states, string optional)
        {
            return InitMatrixStandard(content,  alphabet,  states);
        }

        public string[] GetValidator() {
            
            // First index: regex, Second index: Error message 
            return new string[] {"^(1|0|0\\.\\d{1,2})$", "Only decimals (0-1) with 2 decimal places!"};
        }
    }
}
