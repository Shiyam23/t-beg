﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Hubs;
using G = GraphModel;

namespace TBeg
{

    
//delegates for event handling in c#:
    public delegate void ViewHandler_Matrix<IView>(IView sender, ViewEvent_Matrix e);
    public delegate bool ViewHandler_MatrixName<IView>(IView sender, ViewEvent_Matrix e);
    public delegate void ViewHandler_Game<IView>(IView sender, ViewEvent_Game e);
    public delegate void ViewEvent_GameStepUser<IView>(IView sender, ViewEvent_GameStepUser e);
    public delegate void ViewEvent_GameGraph<IView>(IView sender, ViewEvent_GameGraph e);
    public delegate void ViewHandler_Validator<IView>(string functor);
    public delegate void ViewHandler_Graph<IView>(G::Graph graph, string functor);

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

    public partial class TBeg : IView, IModelObserver
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
        public event ViewHandler_Validator<IView> GetValidator;
        public event ViewHandler_Graph<IView> AddGraph;

        
        //Eventhandling
        IController Controller;

        //more usability 
        private bool swap = false;
        string connectionId;
        static IHubContext<MessageHub> context;

        public IController Controller1
        {
            get{ return Controller; }

            set{ Controller = value; }
        }

        public TBeg(string id)
        {
            this.connectionId = id;
        }

        public static void setHubContext(IHubContext<MessageHub> context) {
            TBeg.context = context;
        }

        // MVC event handling implementations:
        void IView.SetController(IController controller)
        {
            Controller = controller;
        }

        public string[] getFunctors(string filename)
        {

            //Update Model to configfile -dll types
            Dictionary<String, IModel> models = this.Controller1.Models;
            return models.Keys.ToArray<string>();
        }

        public void getValidator(string functor) {

            GetValidator.Invoke(functor);
        }

        public void initMatrix(G::Graph graph, string functor) {
            AddGraph.Invoke(graph, functor);
        }

        public void SendValidator(string[] stateValidator, string[] linkValidator) {

            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("Validator", stateValidator, linkValidator);

        }

        public void InitGameView() {
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InitGameView");
        }

        public void SendErrorMessage(string errorMsg) {
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("Error", errorMsg);
        }

        public void StartGame(string functor, string[] initialPair, bool spoiler){
            InitGame.Invoke(this, new ViewEvent_Game("test", functor, initialPair, spoiler));
        }

        public void SendStepToGame(string functor, int selection, int[] states) {
            SendStep.Invoke(this, new ViewEvent_GameStepUser(functor, "test", new List<int>(states), selection));
        }

        public void SendStepBackToGame(string functor){
            StepBack.Invoke(this, new ViewEvent_Game("test", functor));
        }
        public void SendResetToGame(string functor){
            Console.WriteLine("Game reset");
            ExitGame.Invoke(this, new ViewEvent_Game("test", functor));
        }




        public void FileInfoToUser(IModel model, ModelEvent_InfoFileOp e) {}
        public void UpdateToGraphView(IModel model, ModelEvent_UpdateGraphView e) {}
        public void UpdateGraph(IModel model, ModelEvent_UpdateGraphView e) {}
        public void UpdateGraphName(IModel model, ModelEvent_UpdateGraphView e){}
        public void InfoTextToUser(IModel model, ModelEvent_UpdateGraphView e){
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoText", e.name, e.over, e.Flow_step, e.UserisSpoiler);
            Console.WriteLine(e);
        }
        public void GraphIsConsistentWithGame(IModel model, ModelEvent_UpdateGraphView e){}
        public void InfoStep0(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoStep", e.name, pred1, e.selection, e.UserisSpoiler, e.x, e.y, 0);
            Console.WriteLine("InfoStep 0:");
            Console.WriteLine(e);
        }
        public void InfoStep1(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoStep", e.name, pred1, e.selection, e.UserisSpoiler, e.x, e.y, 1);
            Console.WriteLine("InfoStep 1:");
            Console.WriteLine(e);
        }
        public void InfoStep2(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoStep", e.name, pred1, e.selection, e.UserisSpoiler, e.x, e.y, 2);
            Console.WriteLine("InfoStep 2:");
            Console.WriteLine(e);
        }
        public void InfoStep3(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoStep", e.name, pred1, e.selection, e.UserisSpoiler, e.x, e.y, 3);
            Console.WriteLine("InfoStep 3:");
            Console.WriteLine(e);
        }
        public void InfoStep4(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoStep", e.name, pred1, e.selection, e.UserisSpoiler, e.x, e.y, 4);
            Console.WriteLine("InfoStep 4:");
            Console.WriteLine(e);
        
        }

        public void ReturnToPanel(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("InfoStep1", e.name, pred1, e.selection, e.UserisSpoiler, e.x, e.y, -1);
            Console.WriteLine(e);
        }

        //StepBack stuff
        public void StepBack2(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray(); 
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("StepBack", e.name, pred1, null, e.selection, e.UserisSpoiler, e.x, e.y, -2);
            Console.WriteLine("StepBack 2:");
            Console.WriteLine(e);
        }
        public void StepBack3(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray();
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("StepBack", e.name, pred1, null, e.selection, e.UserisSpoiler, e.x, e.y, -3);
            Console.WriteLine("StepBack 3:");
            Console.WriteLine(e);
        }
        public void StepBack4(IModel model, ModelEvent_InfoStep e){
            int[] pred1 = e.predicate_1?.ToArray();
            int[] pred2 = e.predicate_2?.ToArray();
            Console.WriteLine("pred 2 is: " + pred2[0]);
            TBeg.context.Clients.Client(this.connectionId)
            .SendAsync("StepBack", e.name, pred1, pred2, e.selection, e.UserisSpoiler, e.x, e.y, -4);
            Console.WriteLine("StepBack 4:");
            Console.WriteLine(e);
        }
    }

    

    public interface ITBeg {
        abstract string[] getFunctors(string filename);
        abstract Guid GetID();
    }



}//end namesapce
