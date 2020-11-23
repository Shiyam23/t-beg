using System;
using System.Collections.Generic;

using Microsoft.Msagl.Drawing;

namespace TBeg
{

    public interface IFunctor<T, TValue>
    {
        Type matrixValueType
        {
            get;
            set;
        }
        Type BranchingDataType
        {
            get;
            set;
        }
        // Elements of FX according to the given transition system:
        List<T> elements_of_FX
        {
            get;
            set;
        }

        
        // Generic template for the functor-Interface here to capture Powerset and (DX+1)^A    
        // checkcondition
        // ReturnDirectSuccessors //make the use optional with try catch in Game.cs
        // GetRowHeaders
        //

        // Methods used during Bisimulation and strategy computation
      
        Matrix<T, TValue> InitMatrix(string[] content, string[] alphabet, List<int> states, string optional);
        //bool CheckCondition2(List<int> predicate, int state_i, int state_j, int states, Matrix<T, TValue> transitionsystem, string[] alphabet);
        bool CheckCondition2(List<int> p1, List<int> p2, int state_i,  int state_j, int states, Matrix<T, TValue> transitionsystem, string[] alphabet);//Fp(t1) <=^F Fp(t2)

       

        //bool getRandomPredicate_HelperMethod(List<int> predicate_1, List<int> predicate_2, T t1, T t2);
        List<int> ReturnDirectSuccesors(int state, Matrix<T, TValue> ts, int states, int alphabet);


        // Methods needed to create the transition system
 
        int ReturnRowCount(string[] alphabet, int states, bool termination);// this hardly depends on the branching-type of the system
        // According to GUI
        List<string> GetRowHeadings(string[] alphabet, List<int> states, bool termination);

        //Fp alpha(x) ToString
        string Fpalpha_xToString(int state, List<int> predicate, Matrix<T, TValue> transitionsystem);

        //Get graph
        void GetGraph(ref Graph g);

        //TODO: From graph to initialization panel 
        Matrix<T, TValue>  GetTSfromGraph(Graph g, int states);

        //Save ts to csv file
        string SaveTransitionSystem(string filePath, string[] userinput, string optional);

        //Load ts from csv file
        string[,] LoadTransitionSystemContent(string content, int col, int row, string optional);

    }

}
