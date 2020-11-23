using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBeg
{
    public class ViewEvent_Matrix : EventArgs
    {
        public String name = " ";
        public String functor = " ";
        public string[] alphabet;
        public List<int> states;
        public string[] content;
        public string optional = "";
        public int col;
        public int row;

        public ViewEvent_Matrix(string name, string functor)
        {
            this.name = name;
            this.functor = functor;
        }

        public ViewEvent_Matrix(string name, string functor, string[] content)
        {
            this.name = name;
            this.functor = functor;
            this.content = content;
        }


        public ViewEvent_Matrix(string name, string functor, string[] content, string optional)
        {
            this.name = name;
            this.functor = functor;
            this.content = content;
            this.optional = optional;
        }


        public ViewEvent_Matrix(string name, string functor, string[] content, int col, int row, string optional)
        {
            this.name = name;
            this.functor = functor;
            this.content = content;
            this.col = col;
            this.row = row;
            this.optional = optional;
        }

        public ViewEvent_Matrix(string name, string functor, string[] alphabet, List<int> states, string[] content)
        {
            this.name = name;
            this.functor = functor;
            this.alphabet = alphabet;
            this.states = states;
            this.content = content;
        }

        public ViewEvent_Matrix(string name, string functor, string[] alphabet, List<int> states, string[] content, string optional)
        {
            this.name = name;
            this.functor = functor;
            this.alphabet = alphabet;
            this.states = states;
            this.content = content;
            this.optional = optional;
        }
    }

    public class ViewEvent_GameGraph : EventArgs
    {
        public String name = " ";
        public String functor = " ";
        public Graph graph;
        public bool BackToInit;


        public ViewEvent_GameGraph(string name, string functor, Graph graph, bool backToInit)
        {
            this.name = name;
            this.functor = functor;
            this.graph = graph;
            this.BackToInit = backToInit;
        }

    }



        public class ViewEvent_Game : EventArgs
    {
        public String name = " ";
        public String functor = " ";
        public string[] initPair;
        public bool spoiler; // either user is spoiler or PC, true if User!



        public ViewEvent_Game(string name, string functor)
        {
            this.name = name;
            this.functor = functor;
        }
        public ViewEvent_Game(string name, string functor, string[] initPair, bool spoiler)
        {
            this.name = name;
            this.functor = functor;
            this.initPair = initPair;
            this.spoiler = spoiler;

        }
    }

    public class ViewEvent_GameStepUser : EventArgs
    {
        public String name = " ";
        public String functor = " ";
        public int selection;
        public List<int> p2;

        public ViewEvent_GameStepUser(string functor, string name, List<int> p2, int selection)
        {
            this.functor = functor;
            this.name = name;
            this.p2 = p2;
            this.selection = selection;
        }


    }


}
