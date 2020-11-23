using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//Graph libs
using Microsoft.Msagl.Drawing;
using Color = Microsoft.Msagl.Drawing.Color;
using Node = Microsoft.Msagl.Drawing.Node;
using Shape = Microsoft.Msagl.Drawing.Shape;
using System.Collections;
using System.Threading;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using tbeg_server;



namespace TBeg
{

    
//delegates for event handling in c#:
    public delegate void ViewHandler_SaveToCSV<IView>(IView sender, ViewEvent_Matrix e);
    public delegate void ViewHandler_Matrix<IView>(IView sender, ViewEvent_Matrix e);
    public delegate bool ViewHandler_MatrixName<IView>(IView sender, ViewEvent_Matrix e);
    public delegate void ViewHandler_Game<IView>(IView sender, ViewEvent_Game e);
    public delegate void ViewEvent_GameStepUser<IView>(IView sender, ViewEvent_GameStepUser e);
    public delegate void ViewEvent_GameGraph<IView>(IView sender, ViewEvent_GameGraph e);

    //Thread-Communication
    public delegate void UpdateGraphCallback();
    public delegate void InfoTextToUserCallback( int step, string info, bool UserIsSpoiler);


    //Thread-Communication Step0
    public delegate void UpdateInitSPCallback(string x, string y);
    public delegate void UpdateInitaCallback(bool IsSpoiler);
    public delegate void UpdateInitbCallback();
    public delegate void UpdateStep2InitCallback();
    public delegate void UpdateStep3aInitCallback();
    public delegate void UpdateStep3bInitCallback();
    public delegate void UpdateStep4InitCallback();
    public delegate void GOCallback();
    public delegate void OKCallback();
    public delegate void StepBackCallback();
   
    //Thread-Communication Step1
    public delegate void UpdateStep1aCallback(int s);
    public delegate void UpdateStep1bCallback(List<int> predicate);
    public delegate void UpdateStep1EnableCallback(string value);
    //Thread-Communication Step2
    public delegate void UpdateStep2Callback(List<int> predicate);
    public delegate void UpdateStep2EnableCallback();
    public delegate void UpdateStep2DisbaleCallback();
    //Thread-Communication Step3
    public delegate void UpdateStep3aCallback(int x_prime);
    public delegate void UpdateStep3bCallback(int predicate);
    public delegate void UpdateStep3EnableCallback();
    //Thread-Communication Step4
    public delegate void UpdateStep4Callback(int y_prime);
    public delegate void UpdateStep4EnableCallback();

    public partial class TBeg : IView, ITBeg
    {
        public event ViewHandler_Matrix<IView> InitandSaveMatrix;
        public event ViewHandler_Matrix<IView> SaveToCSV;
        public event ViewHandler_Matrix<IView> LoadFromCSV;
        public event ViewHandler_MatrixName<IView> CheckMatrixName;
        public event ViewHandler_Matrix<IView> InitandSaveGame;// initialization of the board
        public event ViewHandler_Game<IView> InitGame;// of the game-instance
        public event ViewHandler_Game<IView> CheckConsistenceOfGameGraph;// check if user did not change the graph view
        public event ViewHandler_Game<IView> StepBack;// step backwards in the current game flow
        public event ViewHandler_Game<IView> ExitGame;// stop the game and return to starting panel
        public event ViewEvent_GameStepUser<IView> SendStep;
        public event ViewEvent_GameGraph<IView> ResetGraph;

        
        //Eventhandling
        IController Controller;

        //more usability 
        private bool swap = false;
        Guid id;

        public IController Controller1
        {
            get
            {
                return Controller;
            }

            set
            {
                Controller = value;
            }
        }

        public TBeg()
        {
            id = Guid.NewGuid();
        }

        public Guid GetID()
        {
            return id;
        }

        // MVC event handling implementations:
        void IView.SetController(IController controller)
        {
            Controller = controller;
        }

        public string[] getFunctors(string filename)
        {
            string[] functors = System.IO.File.ReadAllLines(filename);

            //Update Model to configfile -dll types
            /* Dictionary<String, IModel> models = this.Controller1.Models;
            Program.UpdateFunctorList(ref models, filename);
            this.Controller1.Models = models; */
            return functors;
        }

    }

    public interface ITBeg {
        abstract string[] getFunctors(string filename);
        abstract Guid GetID();
    }



}//end namesapce
