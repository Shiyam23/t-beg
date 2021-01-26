using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GraphModel;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;

namespace TBeg
{
    //check type for alphabet??
    class Powerset : IFunctor<List<List<int>>,int>
    {


        // Fields:
        Type matrixValueType;
        Type BranchingDataType;

        List<List<List<int>>> _elements_of_FX; // all Elements in FX captured by the transitions system; alphaX(x_1),alphaX(x_2)...


        // Constructor:
        public Powerset()
        {
            matrixValueType = typeof(int);
            BranchingDataType = typeof(List<List<int>>);
        }

        //maybe not used
        public Powerset(List<int> states, Matrix<List<List<int>>, int> transitionsystem, string [] alphabet)
        {
            //_elements_of_FX = ReturnFX(states, transitionsystem, alphabet);
            matrixValueType = typeof(int);
        }
        public List<List<List<int>>> elements_of_FX
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

        Type IFunctor<List<List<int>>, int>.matrixValueType
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

        Type IFunctor<List<List<int>>, int>.BranchingDataType
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
        
        /// <summary>
        /// Used during game:
        /// 2. Step Condition Check of the Duplicator: Fp(alpha(x)) smaller or equal Fp(alpha(y)) </summary>
        /// <param name="predicate"> the move of spoiler p_1</param>
        /// <param name="t1"> Fp(alpha(x))</param>
        /// <param name="t2"> Fp(alpha(y))</param>
        public bool CheckCondition2(List<int> predicate_1, List<int> predicate_2, int state_i,  int state_j, int states, Matrix<List<List<int>>, int> transitionsystem, string[] alphabet)
        {

            
            List<List<int>> t1 = ReturnAlpha_X(state_i, states, transitionsystem, transitionsystem.Alphabet);
            List<List<int>> t2 = ReturnAlpha_X(state_j, states, transitionsystem, transitionsystem.Alphabet);

            Func<List<List<int>>, List<List<int>>> f1 = ReturnFp(predicate_1);
            Func<List<List<int>>, List<List<int>>> f2 = ReturnFp(predicate_2);
            
        
            List <List<int>> Fp1_t1 =f1(t1);
            List<List<int>> Fp2_t2 = f2(t2);
            

            /*
            //TEST: very good memory behaviour ;)
            List<List<int>> Fp1_t1 = GetElemtentF2_MemoryTest(predicate_1, t1);
            List<List<int>> Fp2_t2 = GetElemtentF2_MemoryTest(predicate_2, t2);
            */

            int alphabets = Fp2_t2.Count;
            bool ret = false;

            if (Fp1_t1.Count == 0 && Fp2_t2.Count == 0) return true;

            else
            {
                // has to satisfy this for all alphabets:
                for (int i = 0; i < alphabets; i++)
                {
                    ret = false;
                    if (Fp2_t2[i].All(e => Fp1_t1[i].Contains(e)) && Fp1_t1[i].All(e => Fp2_t2[i].Contains(e))) ret = true; // both are equal
                    if (Fp2_t2[i].Contains(1) && Fp2_t2[i].Count == 1 && Fp1_t1[i].Count != 0) ret = true;//{0,1} <= {1}
                    if ((Fp1_t1[i].Contains(0) && Fp1_t1[i].Count == 1) && (Fp2_t2[i].Count != 0)) ret = true;//{0} <= {0,1} and  {0} <= {1}

                    if (!ret) i = alphabets;
                }

            }
     
            return ret;
        }
      
        
        public Func<List<List<int>>, List<List<int>>> ReturnFp(List<int> predicate)
        {
            return returnFP(predicate);
        }
        

        //HOF syntax:
        Func<List<int>, Func<List<List<int>>, List<List<int>>>> returnFP = predicate => new Func<List<List<int>>, List<List<int>>>(alphaX => GetElemtentF2_MemoryTest(predicate, alphaX));

        /// <summary>
        /// Computation of an element in F2,  concretly Fp(alpha(x)) (without any memroy leaks ;))
        /// </summary>
        /// <param name="p"></param>
        /// <param name="alphaX"></param>
        /// <returns></returns>
        private static List<List<int>> GetElemtentF2_MemoryTest(List<int> p, List<List<int>> alphaX)
        {
            List<List<int>> F2 = new List<List<int>>();
            if (alphaX.Count == 0) return F2; //empty list

            // for each alphabet-symbol:
            for (int i = 0; i < alphaX.Count; i++)
            {
                List<int> item = new List<int>();

                if (alphaX[i].Count != 0)
                {
                    for(int t =0; t< alphaX[i].Count; t++)
                    {
                        if (p.Contains(alphaX[i][t]))
                        {
                            if (!item.Contains(1)) item.Add(1);
                        }
                        else if (!item.Contains(0)) item.Add(0);
                    }             
                }//if end
                item.TrimExcess();
                F2.Add(item);
            }
            return F2;
        }

        /// <summary>
        /// Computation of an element in F2,  concretly Fp(alpha(x))
        /// </summary>
        /// <param name="p"></param>
        /// <param name="alphaX"></param>
        /// <returns></returns>
        private static List<List<int>> GetElemtentF2(List<int> p, List<List<int>> alphaX)
        {
            List<List<int>> F2 = new List<List<int>>();
            if (alphaX.Count == 0) return F2; //empty list

            // for each alphabet-symbol:
            for (int i=0; i< alphaX.Count; i++)
            {
                List<int> item = new List<int>();
               
                if (alphaX[i].Count != 0) 
                {
                    if (alphaX[i].All(e => p.Contains(e)))
                    {
                        item.Add(1);//{1}                     
                    }
                    else
                    {
                        IEnumerable<int> res = p.AsQueryable().Intersect(alphaX[i]);// since this method is called quite often, a memory leak is produced for bigger transition systems
                        if (res.Count() != 0)
                        {
                            item.Add(0);
                            item.Add(1);//{0,1}
                        }
                        else
                        {

                            item.Add(0);//{0}
                        }
                    }
                }//if end
                item.TrimExcess();
                F2.Add(item);
            }
            return F2;
        }


       /// <summary>
       /// Computation of alpha(x)
       /// </summary>
       /// <param name="state"></param>
       /// <param name="states"></param>
       /// <param name="transitionsystem"></param>
       /// <param name="alphabet"></param>
       /// <returns></returns>
        public List<List<int>> ReturnAlpha_X( int state,int states, Matrix<List<List<int>>, int> transitionsystem, string [] alphabet)
        {
           List<List<int>> alphaX = new List<List<int>>();
     
                for (int a = 0; a < alphabet.Count(); a++)
                {
                    List<int> temp = new List<int>();
                    for (int j = 0; j < states; j++)
                    {
                        int row = j * alphabet.Count();
                    // old with bug: if(transitionsystem.get((row+a)*(states-1),state)==1)temp.Add(j);
                    if (transitionsystem.get((row+a),state)==1)temp.Add(j);
                    }
                alphaX.Add(temp);
                }       
            return alphaX;
        }

        // Future TODO: Check represent coalgebra with Func???
        public Func<int,List<List<int>>> returnFX(List<int> states, Matrix<List<List<int>>, int> transitionsystem, string[] alphabet)
        {
            throw new NotImplementedException();
        }




        // GUI Helping methods:

        public List<string> GetRowHeadings(string [] alphabet, List<int> states, bool termination)
        {
            List<string> rowHeadings = new List<string>();
            for (int i = 0; i < states.Count; ++i)
            {
                for (int j = 0; j < alphabet.Length; ++j)
                {
                    rowHeadings.Add("(" + alphabet[j] + "," + (i+1) + ")");
                }

            }
            if (termination) rowHeadings.Add("{\u2022}");

            return rowHeadings;
        }

 

    
      public int ReturnRowCount(string[] alphabet, int states, bool termination)
        {
            if (termination) return alphabet.Length * states+ 1;
            else return alphabet.Length * states;
        }

      public Matrix <List<List<int>>, int> InitMatrixStandard(string[] content, string[] alphabet, List<int> states)
        {
            Matrix<List<List<int>>, int> transitionSystem = new Matrix<List<List<int>>, int>(alphabet.Length * states.Count, states.Count, this);
            for (int i=0;i< states.Count; i++ )
            {
                for (int j = 0; j< alphabet.Length * states.Count; j++)
                {
                    try
                    {
                        int c = Int32.Parse(content[i * (alphabet.Length * states.Count) + j]);
                        transitionSystem.set(j, i, c);
                    }
                    catch {
                        throw new ArgumentException("Invalid input: e.g. for the state " + (i+1).ToString() + "! Please validate your inputs!");
                    }
                  
                }
            }
            //_elements_of_FX = ReturnFX(states, transitionSystem, alphabet);
            transitionSystem.functor = this;
            transitionSystem.Alphabet=alphabet;
            return transitionSystem;
        }

        //Save and Load of transition systems from csv files
        public string SaveTransitionSystem(string filePath, string [] content, string optional)
        {
            // Exeption handling of input should happen directly in the model or GUI method
            if (content.Length==3)
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
            string[,] contentTS = new string[col,row];
            string[] ts = content.Split(',');
            for(int i=0; i< col;i++)
            {
                for(int j=0; j< row;j++)
                {
                    contentTS[i, j] = ts[i * row + j];
                }
            }
            return contentTS;
        }

        //For random strategy:
        /// <summary>
        /// Used during game:
        /// 2. Step Condition Check of the Duplicator: Fp(alpha(x)) smaller  Fp(alpha(y)) </summary>
        /// <param name="predicate"> the move of spoiler p_1</param>
        /// <param name="t1"> Fp(alpha(x))</param>
        /// <param name="t2"> Fp(alpha(y))</param>
        public bool getRandomPredicate_HelperMethod(List<int> predicate_1, List<int> predicate_2, List<List<int>> t1, List<List<int>> t2)
        {
            List<List<int>> Fp1_t1 = ReturnFp(predicate_1)(t1);
            List<List<int>> Fp2_t2 = ReturnFp(predicate_2)(t2);
            if (Fp1_t1.Count == 0 && Fp2_t2.Count == 0) return false;
            int alphabets = Fp2_t2.Count;
            bool ret = false;

            if (Fp1_t1.Count == 0 && Fp2_t2.Count == 0) return false;

            else
            {
                // has to satisfy this for all alphabets:
                for (int i = 0; i < alphabets; i++)
                {
                    if (Fp2_t2[i].Contains(1) && Fp1_t1[i].Count != 0) ret = true;//{0,1} <= {1}
                    if ((Fp1_t1[i].Contains(0) && Fp1_t1[i].Count == 1) && (Fp2_t2[i].Count != 0)) ret = true;//{0} <= {0,1} and  {0} <= {1}
                }

            }
            return ret;
        }

        /// <summary>
        /// Returns all the states "somehow" reachable from "state"
        /// </summary>
        /// <param name="row">The row index of the entry to be set.</param>
        /// <param name="column">The column index of the entry to be set.</param>
        /// <param name="value">The value the entry gets.</param>
        public List<int> ReturnDirectSuccesors(int state, Matrix<List<List<int>>, int> ts, int states, int alphabet)
        {
            List<int> succesors = new List<int>();
            int countState = 0;
            for (int i = 0; i < ts.rows(); i++)
            {
                for(int j=0; j< alphabet; j++)
                {
                    if (ts.get(i+j, state) == 1) succesors.Add(countState);
                }
                i = i + (alphabet - 1);
                countState++;
            }

            return succesors;
        }


        //standard transformation captured by MVC
        public void GetGraph(ref Microsoft.Msagl.Drawing.Graph g)
        {
            throw new NotImplementedException();
        }

        //standard transformation captured by MVC
        public Matrix<List<List<int>>, int> GetTSfromGraph(Microsoft.Msagl.Drawing.Graph g, int states)
        {
            throw new NotImplementedException();
        }

        public string Fpalpha_xToString(int state, List<int> predicate, Matrix<List<List<int>>, int> ts)
        {
            string ret = "{";
            List<List<int>> t = ReturnAlpha_X(state, ts.columns(), ts, ts.Alphabet);
            List<List<int>> Fp_t = ReturnFp(predicate)(t);

            int alphabets = Fp_t.Count;
         

                // has to satisfy this for all alphabets:

                for (int i = 0; i < alphabets; i++)
                {
                    if (i != 0 && !ret.Substring(ret.Length-1).Equals("{"))
                    {
                        if (Fp_t[i].Contains(0) && Fp_t[i].Contains(1)) ret = ret + ",(" + ts.Alphabet[i].ToString() + ",0)" + "," + "(" + ts.Alphabet[i].ToString() + ",1)";
                        if (Fp_t[i].Contains(0) && !Fp_t[i].Contains(1)) ret = ret + ",(" + ts.Alphabet[i].ToString() + ",0)";
                        if (Fp_t[i].Contains(1) && !Fp_t[i].Contains(0)) ret = ret + ",(" + ts.Alphabet[i].ToString() + ",1)";
                    }
                    else
                    {
                        if (Fp_t[i].Contains(0) && Fp_t[i].Contains(1)) ret = ret + "(" + ts.Alphabet[i].ToString() + ",0)" + "," + "(" + ts.Alphabet[i].ToString() + ",1)";
                        if (Fp_t[i].Contains(0) && !Fp_t[i].Contains(1)) ret = ret + "(" + ts.Alphabet[i].ToString() + ",0)";
                        if (Fp_t[i].Contains(1) && !Fp_t[i].Contains(0)) ret = ret + "(" + ts.Alphabet[i].ToString() + ",1)";
                    }
                }

            ret = ret + "}";

            return ret;
        }

        public Matrix<List<List<int>>, int> InitMatrix(string[] content, string[] alphabet, List<int> states, string optional)
        {
            return InitMatrixStandard(content, alphabet, states);
        }

        public string[][] GetValidator() {
            
            // First array: State [<Regex>, <ErrorMessage>, <Description>]
            // Second array: Link [<Regex>, <ErrorMessage>, <Description>]
            return new string[][] {
                new string[]{"^$", "No value allowed", "No value expected here!"},
                new string[]{"^$", "No value allowed", "No value expected here!"}
            };
        }

        public String GetValue(string rowhead, int state, GraphModel.Graph graph) {

            Link[] links = graph.links;
            int length = links.Length;
            int sourceState = state;
            string rawInput = rowhead.Substring(1).Remove(rowhead.Length-2);
            string[] inputs = rawInput.Split(',');
            int targetState = (int)Char.GetNumericValue(inputs[1],0);
            string label = inputs[0];
            for (int i = 0; i < length; i++)
            {   
                if (
                    links[i].source.name == sourceState 
                    && links[i].target.name == targetState
                    && links[i].name == label
                    ) {
                    return "1"; 
                }
            }
            return "0";
        }
    }
}
