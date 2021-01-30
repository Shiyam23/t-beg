using System;
using System.Collections.Generic;

namespace TBeg
{
    
    class DFA : IFunctor<List<int>,int>
    {

        Type matrixValueType;
        Type BranchingDataType;
        List<List<int>> _elements_of_FX;

        Type IFunctor<List<int>,int>.matrixValueType 
        { 
            get => matrixValueType; 
            set => throw new NotImplementedException(); 
        }
        Type IFunctor<List<int>, int>.BranchingDataType 
        { 
            get => BranchingDataType; 
            set => throw new NotImplementedException(); 
        }
        public List<List<int>> elements_of_FX 
        { 
            get => _elements_of_FX; 
            set => throw new NotImplementedException(); 
        }


        public DFA()
        {
            matrixValueType = typeof(int);
            BranchingDataType = typeof(List<int>);
        }

        public DFA(List<int> states, Matrix<List<int>, int> transitionsystem, string [] alphabet)
        {
            matrixValueType = typeof(int);
        }

        public bool CheckCondition2(List<int> p1, List<int> p2, int state_i, int state_j, int states, Matrix<List<int>, int> transitionsystem, string[] alphabet)
        {
            List<int> t1 = returnAlpha(state_i, states, transitionsystem, alphabet);
            List<int> t2 = returnAlpha(state_j, states, transitionsystem, alphabet);

            Func<List<int>, List<int>> f1 = ReturnFp(p1);
            Func<List<int>, List<int>> f2 = ReturnFp(p2);

            List<int> Fp1_t1 = f1(t1);
            List<int> Fp2_t2 = f2(t2);

            if (Fp1_t1.Count == 0 && Fp2_t2.Count == 0) return true;
            int count = Fp1_t1.Count;
            if (Fp1_t1[count - 1] != Fp2_t2[count - 1]) return false;
            for (int i = 0; i < count - 1; i++) {
                if (Fp1_t1[i] > Fp2_t2[i]) return false;
            }
            return true;
        }

        public Func<List<int>, List<int>> ReturnFp(List<int> predicate)
        {
            return returnFP(predicate);
        }

        Func<List<int>, Func<List<int>, List<int>>> returnFP = predicate => new Func<List<int>, List<int>>(alphaX => GetElementF2(predicate, alphaX));

        private static List<int> GetElementF2(List<int> p, List<int> alphaX)
        {
            List<int> F2 = new List<int>();
            for(int i = 0; i < alphaX.Count - 1; i++) {
                if (p.Contains(alphaX[i])) F2.Add(1);
                else F2.Add(0);
            }
            F2.Add(alphaX[alphaX.Count-1]);
            return F2;
        }

        private List<int> returnAlpha(int state, int states, Matrix<List<int>, int> transitionsystem, string[] alphabet) {

            List<int> alphaX = new List<int>();
            for(int i = 0; i < alphabet.Length; i++) {
                alphaX.Add(transitionsystem.get(i,state)-1);
            }
            alphaX.Add(transitionsystem.get(alphabet.Length,state));
            return alphaX;
        }

        public string Fpalpha_xToString(int state, List<int> predicate, Matrix<List<int>, int> transitionsystem)
        {
            List<int> t = returnAlpha(state, transitionsystem.columns(), transitionsystem, transitionsystem.Alphabet);
            List<int> Fp_t = ReturnFp(predicate)(t);
            string ret = "{";
            for(int i = 0; i < Fp_t.Count - 1; i++) {
                ret += "(" + transitionsystem.Alphabet[i] + ", " + Fp_t[i] + ")" + (i == Fp_t.Count - 2 ? "}" : ",");
            }
            return ret;
        }



        public void GetGraph(ref Microsoft.Msagl.Drawing.Graph g)
        {
            return;
        }

        public List<string> GetRowHeadings(string[] alphabet, List<int> states, bool termination)
        {
            List<string> rowHeadings = new List<string>();
            for (int i = 0; i < alphabet.Length; ++i)
            {
                rowHeadings.Add(alphabet[i]);
            }
            rowHeadings.Add("•");
            return rowHeadings;
        }

        public Matrix<List<int>, int> GetTSfromGraph(Microsoft.Msagl.Drawing.Graph g, int states)
        {
            throw new NotImplementedException();
        }

        public string[][] GetValidator()
        {
            // First array: State [<Regex>, <ErrorMessage>, <Description>]
            // Second array: Link [<Regex>, <ErrorMessage>, <Description>]
            return new string[][] {
                new string []{"^[0,1]$", "Only value 0 or 1 is allowed", "Write 1 here if this state is an accepting state, otherwise 0!"},
                new string []{"^$", "No value allowed here", "No value expected here!"}
            };
        }

        public string GetValue(string rowhead, int state, GraphModel.Graph graph)
        {   
            if (rowhead.Equals("•")) return graph.states[state - 1].value.ToString();
            GraphModel.Link[] allLinks = graph.links;
            GraphModel.Link selectedLink = null;
            for (int i = 0; i < graph.links.Length; i++) {
                if (allLinks[i].name.Equals(rowhead) && allLinks[i].source.name == state) {
                    selectedLink = allLinks[i];
                    break;
                }
            }
            try {
                return selectedLink.target.name.ToString();
            } catch (NullReferenceException) {
                throw new ArgumentException($"Missing link {rowhead} from state {state}");
            }
        }

        public Matrix<List<int>, int> InitMatrix(string[] content, string[] alphabet, List<int> states, string optional)
        {
            Matrix<List<int>, int> transitionSystem = new Matrix<List<int>, int>(alphabet.Length + 1, states.Count, this);
            for (int i=0;i< states.Count; i++ )
            {
                for (int j = 0; j < alphabet.Length + 1; j++)
                {
                    try
                    {
                        int c = Int32.Parse(content[i * (alphabet.Length + 1) + j]);
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
            transitionSystem.print();
            return transitionSystem;
        }

        public string[,] LoadTransitionSystemContent(string content, int col, int row, string optional)
        {
            throw new NotImplementedException();
        }

        public List<int> ReturnDirectSuccesors(int state, Matrix<List<int>, int> ts, int states, int alphabet)
        {
            Console.WriteLine($"Successors of {state}...");
            List<int> stateList = new List<int>();
            for (int i = 0; i < alphabet; i++) {
                Console.Write((ts.get(i,state)) + ",");
                stateList.Add(ts.get(i,state)-1);
            }
            Console.WriteLine("");
            return stateList;
        }

        public int ReturnRowCount(string[] alphabet, int states, bool termination)
        {
            return alphabet.Length * states+ 1;
        }

        public string SaveTransitionSystem(string filePath, string[] userinput, string optional)
        {
            throw new NotImplementedException();
        }
    }

}