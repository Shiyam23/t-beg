

using Microsoft.Msagl.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace TBeg
{

    /// <summary>
    /// this file contains the generic model and connection to the game
    /// Models maintain the whole game-flow and interact with the GUI
    /// </summary>
    /// <typeparam name="IModel"></typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ModelHandler_UpdateGraphView<IModel>(IModel sender, ModelEvent_UpdateGraphView e);
    public delegate void ModelHandler_InfoFileOp<IModel>(IModel sender, ModelEvent_InfoFileOp e);
    public delegate void ModelHandler_InfoStep1<IModel>(IModel sender, ModelEvent_InfoStep e);

    // The ModelEventArgs class which is derived from th EventArgs class to 
    // be passed on to the controller when the value is changed
    public class ModelEvent_UpdateGraphView : EventArgs
    {
        public string name;
        public Graph graph;
        public bool over;
        public int Flow_step;
        public bool UserisSpoiler;

        public ModelEvent_UpdateGraphView(string name,  bool consistent, Graph graph )
        {
            this.name = name;       
            this.over = consistent;
            this.graph = graph;
        }
        public ModelEvent_UpdateGraphView(string name, Graph graph, bool over, int flow)
        {
            this.name = name;
            this.graph = graph;
            this.over = over;
            this.Flow_step = flow;
        }

        public ModelEvent_UpdateGraphView(string name, Graph graph, bool over, int flow, bool UserisSpoiler)
        {
            this.name = name;
            this.graph = graph;
            this.over = over;
            this.Flow_step = flow;
            this.UserisSpoiler = UserisSpoiler;
        }
    }

    public class ModelEvent_InfoStep : EventArgs
    {
        public string name;
        public List<int> predicate_1;
        public List<int> predicate_2;
        public int selection;
        public int y_;
        public bool UserisSpoiler;
        public string x;
        public string y;


        public ModelEvent_InfoStep(string name)
        {
            this.name = name;
        }

        public ModelEvent_InfoStep(bool UserisSpoiler)
        {
            this.UserisSpoiler = UserisSpoiler;
        }

        public ModelEvent_InfoStep(bool UserisSpoiler, string x, string y)
        {
            this.UserisSpoiler = UserisSpoiler;
            this.x = x;
            this.y = y;
        }

        public ModelEvent_InfoStep(bool UserisSpoiler, int sel, List<int> predicate, string x, string y)
        {
            this.UserisSpoiler = UserisSpoiler;
            this.x = x;
            this.y = y;
            this.predicate_1 = predicate;
            this.selection = sel;
        }

        public ModelEvent_InfoStep(bool UserisSpoiler, int sel, int y_, List<int> predicate_1, List<int> predicate_2, string x, string y)
        {
            this.UserisSpoiler = UserisSpoiler;
            this.x = x;
            this.y = y;
            this.predicate_1 = predicate_1;
            this.predicate_2 = predicate_2;
            this.selection = sel;
            this.y_ = y_;
        }
        public ModelEvent_InfoStep(int selection, bool UserisSpoiler)
        {
            this.selection = selection;
            this.UserisSpoiler = UserisSpoiler;
        }

        public ModelEvent_InfoStep(string name, List<int> predicate, bool UserisSpoiler, int selection)
        {
            this.name = name;
            this.predicate_1 = predicate;
            this.selection = selection;
            this.UserisSpoiler = UserisSpoiler;
        }
    }


    // GUI save and loading communication
    public class ModelEvent_InfoFileOp : EventArgs
    {
        public string name;
        public string infotext; //save or load ok
        public string[,] contentTS; //loaded content from file
        public string optional;


        public ModelEvent_InfoFileOp(string name, string infotext)
        {
            this.name = name;
            this.infotext = infotext;
        }

        public ModelEvent_InfoFileOp(string name,  string[,] contentTS, string optional)
        {
            this.name = name;
            this.contentTS = contentTS;
            this.optional = optional;
        }

    }

    // The interface which the form/view must implement so that, the event will be
    // fired when a value is changed in the model.
    public interface IModelObserver
    {
        void FileInfoToUser(IModel model, ModelEvent_InfoFileOp e);
        void UpdateToGraphView(IModel model, ModelEvent_UpdateGraphView e);
        void UpdateGraph(IModel model, ModelEvent_UpdateGraphView e);
        void UpdateGraphName(IModel model, ModelEvent_UpdateGraphView e);
        void InfoTextToUser(IModel model, ModelEvent_UpdateGraphView e);
        void GraphIsConsistentWithGame(IModel model, ModelEvent_UpdateGraphView e);
        void InfoStep0(IModel model, ModelEvent_InfoStep e);
        void InfoStep1(IModel model, ModelEvent_InfoStep e);
        void InfoStep2(IModel model, ModelEvent_InfoStep e);
        void InfoStep3(IModel model, ModelEvent_InfoStep e);
        void InfoStep4(IModel model, ModelEvent_InfoStep e);

        void ReturnToPanel(IModel model, ModelEvent_InfoStep e);

        //StepBack stuff
        void StepBack2(IModel model, ModelEvent_InfoStep e);
        void StepBack3(IModel model, ModelEvent_InfoStep e);
        void StepBack4(IModel model, ModelEvent_InfoStep e);
    }


    public interface IModel
    {
        void Attach(IModelObserver imo);
        void InitGame(string name, string functor, string[] initPair, bool spoiler);
        void CheckConsistenceOfGameGraph(string name, string functor);
        void StepBack(string name, string functor);
        // should be called after "enter matrix"
        void InitandSaveMatrix(string name, string functor, string[] alphabet, List<int> states, string[] content, string optional);
        // should be called after "switch to graph-view":
        void InitandSaveGame(string name, string functor, string[] alphabet, List<int> states, string[] content, string optional);
        bool CheckMatrixName(string name);

        void SendStepSwitchCase(string name, List<int> predicate, int slection);


        void UpdateGraphView(string name, Graph graph);
        void UpdateGraph(string name, Graph graph);
        void SaveToCSV(string name, string functor, string[] content, string optional);
        void LoadFromCSV(string name, string functor, string[] content, int col, int row, string optional);
        void ExitGame(string name, string functor);
        void ResetGraph(string name, string functor, Graph graph, bool backToInit);
    }

    class Model<T> : IModel where T : new()
    {

        // events
        [field: NonSerialized]
        // FileOp info events
        public event ModelHandler_InfoFileOp<IModel> FileInfoToUser;

        //game events:
        public event ModelHandler_UpdateGraphView<IModel> update_GraphView;
        public event ModelHandler_UpdateGraphView<IModel> update_Graph;
        public event ModelHandler_UpdateGraphView<IModel> GraphNameUpdate;
        public event ModelHandler_UpdateGraphView<IModel> InfoToUser;
        public event ModelHandler_UpdateGraphView<IModel> GraphIsConsistentWithGame;
        public event ModelHandler_InfoStep1<IModel> returnToStartPanel;

        // step events to inform Overview Game-steps:
        public event ModelHandler_InfoStep1<IModel> infoStep0;
        public event ModelHandler_InfoStep1<IModel> infoStep1;
        public event ModelHandler_InfoStep1<IModel> infoStep2;
        public event ModelHandler_InfoStep1<IModel> infoStep3;
        public event ModelHandler_InfoStep1<IModel> infoStep4;


        //      
        public event ModelHandler_InfoStep1<IModel> backStep2;
        public event ModelHandler_InfoStep1<IModel> backStep3;
        public event ModelHandler_InfoStep1<IModel> backStep4;

        [field: NonSerialized]
        Thread GameThread;

        Hashtable datamodelGames;
        Hashtable datamodelMatrix;

        //One Step Back datastructure MaximumSize:20
        private Stack stepBack;
        private Stack stepBackGraph;
        private int MaximumSize = 20;

        Graph CurrentGraph;
        Object CurrentGame;
        long FLOW_Step = 1;// control game flow
        long play;


        public Model()
        {
            // datamanagment update: alllows loading, saving 
            datamodelGames = new Hashtable();
            datamodelMatrix = new Hashtable();
            stepBack        = new Stack();
            stepBackGraph   = new Stack();
            //names = new List<String>();

        }


        public void Attach(IModelObserver imo)
        {
            FileInfoToUser += new ModelHandler_InfoFileOp<IModel>(imo.FileInfoToUser);
            update_GraphView += new ModelHandler_UpdateGraphView<IModel>(imo.UpdateToGraphView);
            update_Graph += new ModelHandler_UpdateGraphView<IModel>(imo.UpdateGraph);
            GraphNameUpdate+= new ModelHandler_UpdateGraphView<IModel>(imo.UpdateGraphName);
            InfoToUser += new ModelHandler_UpdateGraphView<IModel>(imo.InfoTextToUser);
            GraphIsConsistentWithGame += new ModelHandler_UpdateGraphView<IModel>(imo.GraphIsConsistentWithGame);
            infoStep0 += new ModelHandler_InfoStep1<IModel>(imo.InfoStep0);
            infoStep1 += new ModelHandler_InfoStep1<IModel>(imo.InfoStep1);
            infoStep2 += new ModelHandler_InfoStep1<IModel>(imo.InfoStep2);
            infoStep3 += new ModelHandler_InfoStep1<IModel>(imo.InfoStep3);
            infoStep4 += new ModelHandler_InfoStep1<IModel>(imo.InfoStep4);
            returnToStartPanel += new ModelHandler_InfoStep1<IModel>(imo.ReturnToPanel);
            //Backstepp stuff
            backStep2 += new ModelHandler_InfoStep1<IModel>(imo.StepBack2);
            backStep3 += new ModelHandler_InfoStep1<IModel>(imo.StepBack3);
            backStep4 += new ModelHandler_InfoStep1<IModel>(imo.StepBack4);
        }

        public bool CheckMatrixName(string name)
        {
            if (datamodelMatrix.ContainsKey(name)) return true;
            else return false;
        }


  
        // Just for the game graph, not the real Game-Instance
        public void InitandSaveGame(string name, string functor, string[] alphabet, List<int> states, string[] content, string optional)
        {
            // check if already initialized
            Object transitionSystem;
            if (CheckMatrixName(name)) transitionSystem = datamodelMatrix[name];
            else   // if not init
            {
                //TODO check optional Input
                InitandSaveMatrix(name, functor, alphabet, states, content, optional);
                     
            }
            try
            {
                transitionSystem = datamodelMatrix[name];
                // update View, create GAME-Board:
                // transform to graph:

                Graph graph = new Graph();
                ExtractGraphfromTS(transitionSystem, ref graph, states, alphabet);
                /*
                for (int i = 0; i < states.Count; i++)
                {
                    Node node_i = new Node((i + 1).ToString());
                    //Update size of the label? GUI
                    node_i.Attr.Shape = Shape.Circle;
                    node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;

                    graph.AddNode(node_i);
                }


                // Distinguish between this weighted automata template and a more general one:
                // encapsulation of this into functor: TODO  GetGraph (graph) method
                // Update to more then one alphabet
                // init functor
                T F = new T();
                MethodInfo method_functor = F.GetType().GetMethod("GetGraph");
                try
                {
                    method_functor.Invoke(F, new Object[] { graph});
                }
                catch
                {
                    //hence method is not implemented, just simple semiring based transition system
                    for (int i = 0; i < states.Count; i++)
                    {
                        int columns = states.Count * alphabet.Count();
                        for (int j = 0; j < columns; j++)
                        {
                            MethodInfo method = transitionSystem.GetType().GetMethod("EqualToZero");
                            bool value = (bool)method.Invoke(transitionSystem, new Object[] { j, i });          
                            if (!value)
                            {
                                int state = (j + 1) / alphabet.Length;
                                if ((j + 1) % alphabet.Length != 0) state++;
                                int text = j % alphabet.Length;
                                //  update to input value of ts
                                string attr = "";
                                method = transitionSystem.GetType().GetMethod("EqualToOne");
                                value = (bool)method.Invoke(transitionSystem, new Object[] { j, i });
                                if (!value)
                                {
                                    method = transitionSystem.GetType().GetMethod("get");
                                    var val = method.Invoke(transitionSystem, new Object[] { j, i });
                                    attr = "," + (dynamic)val.ToString();
                                }

                                //handle also the case of no lable transition systems:
                                string edgeLabel = " ";
                                if (!alphabet[0].Equals("NL")) edgeLabel = alphabet[text];

                                graph.AddEdge((i + 1).ToString(), edgeLabel + attr, state.ToString());
                            }
                        }
                    }
                }
                foreach(Edge edge in graph.Edges)
                {
                    edge.Label.FontSize = 5;
                }
               */
                CurrentGraph = graph;
                UpdateGraphView(name, graph);

                PropertyInfo InitGraph = transitionSystem.GetType().GetProperty("InitGraph1");
                //Reference check?? Deep copy better:
                InitGraph.SetValue(transitionSystem, DeepCopyGraph(graph));

            }
            catch (Exception e)
            {
                throw new Exception("Error in graph construction"+ Environment.NewLine +e.Message);
            }

        }

        private void ExtractGraphfromTS(Object transitionSystem, ref Graph graph, List<int> states, string[] alphabet)
        {
            for (int i = 0; i < states.Count; i++)
            {
                Node node_i = new Node((i + 1).ToString());
                //Update size of the label? GUI
                node_i.Attr.Shape = Shape.Circle;
                node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;

                graph.AddNode(node_i);
            }


            // Distinguish between this weighted automata template and a more general one:
            // Update to more then one alphabet
            // init functor
            T F = new T();
            MethodInfo method_functor = F.GetType().GetMethod("GetGraph");
            try
            {
                method_functor.Invoke(F, new Object[] { graph });
            }
            catch
            {
                //hence method is not implemented, just simple semiring based transition system
                for (int i = 0; i < states.Count; i++)
                {
                    int columns = states.Count * alphabet.Count();
                    for (int j = 0; j < columns; j++)
                    {
                        MethodInfo method = transitionSystem.GetType().GetMethod("EqualToZero");
                        bool value = (bool)method.Invoke(transitionSystem, new Object[] { j, i });
                        if (!value)
                        {
                            int state = (j + 1) / alphabet.Length;
                            if ((j + 1) % alphabet.Length != 0) state++;
                            int text = j % alphabet.Length;
                            //  update to input value of ts
                            string attr = "";
                            method = transitionSystem.GetType().GetMethod("EqualToOne");
                            value = (bool)method.Invoke(transitionSystem, new Object[] { j, i });
                            if (!value)
                            {
                                method = transitionSystem.GetType().GetMethod("get");
                                var val = method.Invoke(transitionSystem, new Object[] { j, i });
                                attr = "," + (dynamic)val.ToString();
                            }

                            //handle also the case of no lable transition systems:
                            string edgeLabel = " ";
                            if (!alphabet[0].Equals("NL")) edgeLabel = alphabet[text];

                            graph.AddEdge((i + 1).ToString(), edgeLabel + attr, state.ToString());
                        }
                    }
                }
            }
            foreach (Edge edge in graph.Edges)
            {
                edge.Label.FontSize = 5;
            }

        }

        private Graph DeepCopyGraph(Graph graph)
        {
            Graph clone = new Graph();

            clone.Attr = graph.Attr;
          
            foreach (Node node in graph.Nodes)
            {
                Node node_i = new Node(node.Id);
                //Update size of the label? GUI
                node_i.Label = node.Label;
                node_i.Attr = node.Attr;
        
                node_i.Attr.Shape = Shape.Circle;
                node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;

              clone.AddNode(node_i);
            }

            foreach (Edge edge in graph.Edges)
            {
                clone.AddEdge(edge.Source, edge.Label.Text, edge.Target);
            }
            return clone;
        }

        public void InitandSaveMatrix(string name, string functor, string[] alphabet, List<int> states, string[] content, string optional)
        {
            //init functor
            T Functor = new T();

            Type[] types = Functor.GetType().GetGenericArguments();

            // init matrix and save
            try
            {
                MethodInfo method = Functor.GetType().GetMethod("InitMatrix");
                //todo: T1 and T2 from Functor<T1,T2> is not  known, but in types:
                Object transitionSystem = method.Invoke(Functor, new Object[] { content, alphabet, states, optional});
                // what if already contained?
                datamodelMatrix.Add(name, transitionSystem);
            }

       
            catch (ArgumentException e)
            {
                throw e;
            }
            //update view

        }

        public void SaveToCSV(string name, string functor, string[] content, string optional)
        {
            //init functor
            T Functor = new T();

            Type[] types = Functor.GetType().GetGenericArguments();
            // call save method of functor
            try
            {
                //SaveTransitionSystem(string filePath, string [] userinput);
                MethodInfo method = Functor.GetType().GetMethod("SaveTransitionSystem");
                //todo: T1 and T2 from Functor<T1,T2> is not  known, but in types:
                Object info = method.Invoke(Functor, new Object[] { name, content, optional });
                string infotext = (string)info;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void LoadFromCSV(string name, string functor, string[] content, int col, int row, string optional)
        {
            //init functor
            T Functor = new T();

            Type[] types = Functor.GetType().GetGenericArguments();
            // call save method of functor
            try
            {
                //SaveTransitionSystem(string filePath, string [] userinput);
                MethodInfo method = Functor.GetType().GetMethod("LoadTransitionSystemContent");
                //todo: T1 and T2 from Functor<T1,T2> is not  known, but in types:
                Object tsInfo = method.Invoke(Functor, new Object[] { content[2],col,row, optional });
                string [,] UIInfo = (string[,])tsInfo;

                //send feedback to the UI
                SendInfoFileOp(name,UIInfo,optional);
            }
            catch (Exception e)
            {
                //TODO continue with error message to GUI
            }
        }

        public void SendInfoFileOp(string name, string [,] content, string optional)
        {
            FileInfoToUser.Invoke(this, new ModelEvent_InfoFileOp(name, content,optional));
        }





        public void InitGame(string name, string functor, string[] initPair, bool spoiler)
        {

            // Iinit Game Instance
            if (CheckMatrixName(name))
            {
                try
                {
                    Object transitionSystem = datamodelMatrix[name];
                    T Functor = new T();
                    Type[] typeArgs = new Type[2];
                    FieldInfo fi_1 = Functor.GetType().GetField("BranchingDataType", BindingFlags.Instance | BindingFlags.NonPublic);
                    typeArgs[0] = (Type)fi_1.GetValue(Functor);
                    FieldInfo fi_2 = Functor.GetType().GetField("matrixValueType", BindingFlags.Instance | BindingFlags.NonPublic);
                    typeArgs[1] = (Type)fi_2.GetValue(Functor);


                    int x = Int32.Parse(initPair[0]);
                    int y = Int32.Parse(initPair[1]);

                    //Start GAME
                    Type generic = typeof(Game<,>);
                    Type constructed = generic.MakeGenericType(typeArgs);
                    Object game = Activator.CreateInstance(constructed, transitionSystem, name, x - 1, y - 1, spoiler);

                    if (!datamodelGames.ContainsKey(name)) datamodelGames.Add(name, game);
                    else
                    {
                        datamodelGames.Remove(name);
                        datamodelGames.Add(name, game); //switch to fresh intialized game!
                    }

                    //clear the game-graph if necessary
                    //reset coloring of the graph before new round
                    for (int j = 0; j < CurrentGraph.NodeCount; j++)
                    {
                        Node node = CurrentGraph.FindNode((j + 1).ToString());
                        node.Attr.FillColor = Color.White;
                        node.Attr.Color = Color.Gray;
                    }
                    UpdateGraph(name, CurrentGraph);

                    //extract the alphabet:
                    Type ts = transitionSystem.GetType();
                    PropertyInfo get = ts.GetProperty("Alphabet");
                    string[] alphabet = (string[])get.GetValue(transitionSystem, null);
                    //GO ON with game-flow  in separate thread:
                    FLOW_Step = 1;
                    // send event to GUI to initialize the Step-Overview:
                    PropertyInfo UserIsspoiler = game.GetType().GetProperty("Spoiler");
                    bool IsSpoiler = (bool)UserIsspoiler.GetValue(game, null);

                    InitGame(IsSpoiler, initPair[0], initPair[1]);


                    GameThread = new Thread(() => GameGoOn(name, 1, alphabet));
                    GameThread.IsBackground = true;
                    GameThread.Start();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                
            }
            else
            {
                //GUI-Info for the User
                throw new Exception("There exists no transition system under the name : " + name + " for the selected functor " + functor + ".");
            }

        }

        //TODO: More TestingView of Controller Model FLOW
        public void StepBack(string name, string functor)
        {
            if (stepBack.Count > 2)
            {
                //set to top of the gameStep stack
                CurrentGame = stepBack.Pop();
                CurrentGame = stepBack.Pop();
                CurrentGame = stepBack.Pop(); //state before the last step of the user
                //set to top of the gameStep stack
                Graph g = (Graph)stepBackGraph.Pop();
                g = (Graph)stepBackGraph.Pop();
                g = (Graph)stepBackGraph.Pop(); //state before the last step of the user
                // threadsafety
                Interlocked.Decrement(ref FLOW_Step);
                Interlocked.Decrement(ref FLOW_Step);
                //send update to GUI-GamePanel
                // switch case:
                long caseSwitch = Interlocked.Read(ref FLOW_Step);
                if (caseSwitch <= 0)
                {
                    caseSwitch = caseSwitch + 4;
                    while (Interlocked.Read(ref FLOW_Step) < caseSwitch)
                        Interlocked.Increment(ref FLOW_Step);

                }
                // prepare reflection stuff
                Type type = CurrentGame.GetType();
 
                // User is spoiler
                PropertyInfo spoiler = type.GetProperty("Spoiler");
                bool userisSpoiler = (bool)spoiler.GetValue(CurrentGame, null);

                //prepare the graph
                for (int i = 0; i < CurrentGraph.NodeCount; i++)
                {

                    CurrentGraph.FindNode((i + 1).ToString()).Attr.Shape = g.FindNode((i + 1).ToString()).Attr.Shape;
                    CurrentGraph.FindNode((i + 1).ToString()).Attr.Color = g.FindNode((i + 1).ToString()).Attr.Color;
                    CurrentGraph.FindNode((i + 1).ToString()).Attr.FillColor = g.FindNode((i + 1).ToString()).Attr.FillColor;
                }


                //update to situation according to step caseSwitch
                switch (caseSwitch)
                {
                    case 1:
                        // Handling of: If init of game , not from round before!
                        // could be controlled via the stack-size: if 1 then 0
                        try
                        {
                            stepBack = SaveStepBack(4, CurrentGame, name);
                            stepBackGraph = SaveStepBackGraph(4, name);
                        }
                        catch
                        {
                            stepBack = SaveStepBack(0, CurrentGame, name);
                            stepBackGraph = SaveStepBackGraph(0, name);
                        }
                        //inform GUI get the initial states of this round
                        PropertyInfo X = type.GetProperty("X");
                        int x = (int)X.GetValue(CurrentGame, null);
                        PropertyInfo Y = type.GetProperty("Y");
                        int y = (int)Y.GetValue(CurrentGame, null);
                        InitGame(userisSpoiler, (x + 1).ToString(), (y + 1).ToString());
                        //update graph
                        UpdateGraph(name, CurrentGraph);
                        break;
                    case 2:
                        stepBack = SaveStepBack(1, CurrentGame, name);
                        stepBackGraph = SaveStepBackGraph(1, name);
                        PropertyInfo S_SP = type.GetProperty("S_sp");
                        x = (int)S_SP.GetValue(CurrentGame, null);
                        PropertyInfo P1 = type.GetProperty("P1");
                        List<int> p1 = (List<int>)P1.GetValue(CurrentGame, null);
                        //clear game-panel wrt: from step 1 -> step 2
                        InitGameStepBack2(userisSpoiler);
                        //value infostring Fpalpha(x)
                        string value;
                        MethodInfo method = type.GetMethod("Fpalpha_xToString");
                        try
                        {
                            value = (string)method.Invoke(CurrentGame, new Object[] { x, p1 });
                        }
                        catch
                        {
                            value = "Unkown";
                        }
                      
                        UpdateStep1(x + 1, p1, userisSpoiler, value);
                        //update graph
                        UpdateGraph(name, CurrentGraph);
                        break;
                    case 3:
                        stepBack = SaveStepBack(2, CurrentGame, name);
                        stepBackGraph = SaveStepBackGraph(2, name);
                        //reconstruct initial situation
                        X = type.GetProperty("X");
                        x = (int)X.GetValue(CurrentGame, null);
                        Y = type.GetProperty("Y");
                        y = (int)Y.GetValue(CurrentGame, null);
                        //InitGame(userisSpoiler, (x + 1).ToString(), (y + 1).ToString());
                        //setup of step 1 & 2
                        S_SP = type.GetProperty("S_sp");
                        int x_ = (int)S_SP.GetValue(CurrentGame, null);
                        P1 = type.GetProperty("P1");
                        p1 = (List<int>)P1.GetValue(CurrentGame, null);
                        //clear game-panel wrt: from step 2 -> step 3                     
                        //UpdateStep1(x + 1, p1, userisSpoiler);
                        InitGameStepBack3(userisSpoiler, x_ + 1, p1, (x + 1).ToString(), (y + 1).ToString());
                        PropertyInfo T_dup = type.GetProperty("T_dup");
                        y = (int)T_dup.GetValue(CurrentGame, null);

                        PropertyInfo P2 = type.GetProperty("P2");
                        List<int> p2 = (List<int>)P2.GetValue(CurrentGame, null);

                        UpdateStep2(y, p2, userisSpoiler);

                        //update graph
                        UpdateGraph(name, CurrentGraph);
                        break;
                    case 4:
                        stepBack = SaveStepBack(3, CurrentGame, name);
                        stepBackGraph = SaveStepBackGraph(3, name);
                        //reconstruct initial situation wrt selection:                
                        PropertyInfo Sel = type.GetProperty("Selection");
                        int selection = (int)Sel.GetValue(CurrentGame, null);
                        if (selection == 0)
                        {
                            X = type.GetProperty("X_prime");
                            x = (int)X.GetValue(CurrentGame, null);
                            Y = type.GetProperty("Y");
                            y = (int)Y.GetValue(CurrentGame, null);
                        }
                        else
                        {
                            X = type.GetProperty("X");
                            x = (int)X.GetValue(CurrentGame, null);
                            Y = type.GetProperty("Y_prime");
                            y = (int)Y.GetValue(CurrentGame, null);
                        }
      
                        //InitGame(userisSpoiler, (x + 1).ToString(), (y + 1).ToString());
                        //setup of step 1 & 2
                        S_SP = type.GetProperty("S_sp");
                        x_ = (int)S_SP.GetValue(CurrentGame, null);
                        P1 = type.GetProperty("P1");
                        p1 = (List<int>)P1.GetValue(CurrentGame, null);
                        //clear game-panel wrt: from step 2 -> step 3                     


                        T_dup = type.GetProperty("T_dup");
                        int y_ = (int)T_dup.GetValue(CurrentGame, null);

                        P2 = type.GetProperty("P2");
                        p2 = (List<int>)P2.GetValue(CurrentGame, null);
                        InitGameStepBack4(userisSpoiler, x_ + 1, y_ + 1, p1, p2, (x + 1).ToString(), (y + 1).ToString());

                        //update to last step before user step wrt selection
                        if (selection == 0)
                        {
                            X = type.GetProperty("X");
                            x = (int)X.GetValue(CurrentGame, null);
                        }
                        else
                        {
                            X = type.GetProperty("Y");
                            x = (int)X.GetValue(CurrentGame, null);
                        }
                      

        
                        List<int> p = new List<int>();
                        p.Add(x);
                        UpdateStep3(selection, p, false);
                        //update graph
                        UpdateGraph(name, CurrentGraph);
                        break;
                    default:
                        break;
                }

            }

        }

       

        private Stack SaveStepBack(int step, Object game, string name)
        {
            // deepcopy game-object ??
            T Functor = new T();
            Object transitionSystem = datamodelMatrix[name];
            Type[] typeArgs = new Type[2];
            FieldInfo fi_1 = Functor.GetType().GetField("BranchingDataType", BindingFlags.Instance | BindingFlags.NonPublic);
            typeArgs[0] = (Type)fi_1.GetValue(Functor);
            FieldInfo fi_2 = Functor.GetType().GetField("matrixValueType", BindingFlags.Instance | BindingFlags.NonPublic);
            typeArgs[1] = (Type)fi_2.GetValue(Functor);

            Type generic = typeof(Game<,>);
            Type constructed = generic.MakeGenericType(typeArgs);
            Type type = game.GetType();

            //deep copy of game values  
            PropertyInfo init_X = type.GetProperty("Init_X");
            int x_ini = (int)init_X.GetValue(game, null);
            PropertyInfo init_Y = type.GetProperty("Init_Y");
            int y_ini = (int)init_Y.GetValue(game, null);
            PropertyInfo X = type.GetProperty("X");
            int x = (int)X.GetValue(game, null);
            PropertyInfo Y = type.GetProperty("Y");
            int y = (int)Y.GetValue(game, null);
            PropertyInfo X_ = type.GetProperty("X_prime");
            int x_ = (int)X_.GetValue(game, null);
            PropertyInfo Y_ = type.GetProperty("Y_prime");
            int y_ = (int)Y_.GetValue(game, null);
            //add selction of spoiler
            PropertyInfo sel = type.GetProperty("Selection");
            int selec = (int)sel.GetValue(game, null);
            PropertyInfo Sp = type.GetProperty("Spoiler");
            bool spoiler = (bool)Sp.GetValue(game, null);
            Object gameDeepCopy = Activator.CreateInstance(constructed, transitionSystem, name, x, y, spoiler);
            //set p1 and p2? spoiler move or duplicator move as deepy copies
            PropertyInfo S_SP = type.GetProperty("S_sp");
            int S_sp = (int)S_SP.GetValue(game, null);
            PropertyInfo T_DUP = type.GetProperty("T_dup");
            int T_dup = (int)T_DUP.GetValue(game, null);
            PropertyInfo P1 = type.GetProperty("P1");
            List<int> p1 = (List<int>)P1.GetValue(game, null);
            PropertyInfo P2 = type.GetProperty("P2");
            List<int> p2 = (List<int>)P2.GetValue(game, null);
            object[] arguments = new object[] { step, S_sp, T_dup, p1, p2, x_, y_ ,x_ini, y_ini, selec};
            MethodInfo method = type.GetMethod("DeepCopyClone");
            method.Invoke(gameDeepCopy, arguments);


            //test if MaximumSize reached
            if (stepBack.Count == MaximumSize)
            {
                //copy current stack to other in reversed order and pop first step        
                Stack reverse = new Stack();
                while (stepBack.Count != 0)
                {
                    reverse.Push(stepBack.Pop());
                }
                stepBack.Clear();
                reverse.Pop();
                while (reverse.Count != 0)
                {
                    stepBack.Push(reverse.Pop());
                }
                stepBack.Push(gameDeepCopy);
                return stepBack;
            }
            else
            {
                //copy back to stepBack Stack and push the current made step on top
                stepBack.Push(gameDeepCopy);
                return stepBack;
            }
        }

        private Stack SaveStepBackGraph(int step, string name)
        {
            // deepcopy graphobject ??
            Graph deepGraphCopy = new Graph();

            //deep copy of game values        


            for (int i = 0; i < CurrentGraph.NodeCount; i++)
            {
                Node node_i = new Node((i + 1).ToString());
                //Update size of the label? GUI
                node_i.Attr.Shape = CurrentGraph.FindNode((i + 1).ToString()).Attr.Shape;
                node_i.Attr.Color = CurrentGraph.FindNode((i + 1).ToString()).Attr.Color;
                node_i.Attr.FillColor = CurrentGraph.FindNode((i + 1).ToString()).Attr.FillColor;
                deepGraphCopy.AddNode(node_i);
            }
            foreach (Edge i in CurrentGraph.Edges)
            {
                deepGraphCopy.AddEdge(i.Source, i.LabelText, i.Target);
            }


            //test if MaximumSize reached
            if (stepBackGraph.Count == MaximumSize)
            {
                //copy current stack to other in reversed order and pop first step        
                Stack reverse = new Stack();
                while (stepBackGraph.Count != 0)
                {
                    reverse.Push(stepBackGraph.Pop());
                }
                stepBackGraph.Clear();
                reverse.Pop();
                while (reverse.Count != 0)
                {
                    stepBackGraph.Push(reverse.Pop());
                }
                stepBackGraph.Push(deepGraphCopy);
                return stepBackGraph;
            }
            else
            {
                //copy back to stepBack Stack and push the current made step on top
                stepBackGraph.Push(deepGraphCopy);
                return stepBackGraph;
            }
        }


        private string GetDistinguishingFormula(Type type)
        {
            PropertyInfo Xt = type.GetProperty("Init_X");
            PropertyInfo Yt = type.GetProperty("Init_Y");
            int xt = (int)Xt.GetValue(CurrentGame, null);
            int yt = (int)Yt.GetValue(CurrentGame, null);
            MethodInfo method = type.GetMethod("GetDistinguishingFormula");
            object[] argumentst = new object[] { xt, yt };
            string formula = (string)method.Invoke(CurrentGame, argumentst);

            //TODO: this throws exception
            List<string> lines = new List<string>();//WrapText(formula, 600, new System.Drawing.Font("Calibri", 11.0f));


            formula = "";
            foreach (var item in lines)
            {
                formula = formula + item + Environment.NewLine;
            }
            return formula;
        }



        /// <summary>
        /// To adapt the formula formatting
        /// Taken from https://stackoverflow.com/questions/3961278/word-wrap-a-string-in-multiple-lines 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pixels"></param>
        /// <param name="font"></param>
        /// <returns></returns>

        //TODO: this throws exception
        /* private List<string> WrapText(string text, double pixels, System.Drawing.Font font)
        {
            string[] originalLines = text.Split(new string[] { " " },
                StringSplitOptions.None);

            List<string> wrappedLines = new List<string>();

            StringBuilder actualLine = new StringBuilder();
            double actualWidth = 0;

            foreach (var item in originalLines)
            {
                int w = TextRenderer.MeasureText(item + " ", font).Width;
                actualWidth += w;

                if (actualWidth > pixels)
                {
                    wrappedLines.Add(actualLine.ToString());
                    actualLine.Clear();
                    actualWidth = w;
                }

                actualLine.Append(item + " ");
            }

            if (actualLine.Length > 0)
                wrappedLines.Add(actualLine.ToString());

            return wrappedLines;
        } */

        public void GameGoOn(string GameName, long step, string[] alphabet)
        {
            play = 1;
            CurrentGame = datamodelGames[GameName];

            //save inital situation ?? need to create a new ref object?
            stepBack = SaveStepBack(0, CurrentGame, GameName);
            stepBackGraph = SaveStepBackGraph(1, GameName);
            Type type = CurrentGame.GetType();
            MethodInfo method;

            // Distinguish between the 4 game steps:
            // game continues until: exit or one of the players wins the game
            while (Interlocked.Read(ref play) == 1)
            {
                try
                {
                    PropertyInfo spoiler;
                    if (Interlocked.Read(ref FLOW_Step) == 1)
                    {
                        spoiler = type.GetProperty("Spoiler");
                        if ((bool)spoiler.GetValue(CurrentGame, null))
                        {

                            //send message Info to User: PC is waiting for move
                            while (1 == Interlocked.Read(ref FLOW_Step))
                            {
                                Thread.Sleep(2000);
                                //send info to user:
                            }

                            //After user-move only go on if move of user has been valid:
                            if (Interlocked.Read(ref FLOW_Step) == 2)
                            {
                                Interlocked.Increment(ref step);
                                //save this step
                                stepBack = SaveStepBack(1, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(1, GameName);
                                //inform GUI:
                                UpdateGraph(GameName, CurrentGraph);
                            }
                        }
                        else //PC is Spoiler
                        {
                            int s = 0; ;
                            List<int> p1 = new List<int>();
                            object[] arguments = new object[] { s, p1, alphabet.Count() };
                            method = type.GetMethod("Step1_PCIsSpoiler");
                            int selection = (int)method.Invoke(CurrentGame, arguments);
                            // update the nodes in the graph:
                            s = (int)arguments[0];
                            p1 = (List<int>)arguments[1];

                            Node lila = CurrentGraph.FindNode((s + 1).ToString());
                            lila.Attr.Color = Color.BlueViolet;
                            for (int j = 0; j < p1.Count(); j++)
                            {
                                Node node = CurrentGraph.FindNode((p1[j] + 1).ToString());
                                node.Attr.FillColor = Color.Violet;
                            }
                            // mark the state for the duplicator:
                            PropertyInfo prop = type.GetProperty("T_dup");
                            int t = (int)prop.GetValue(CurrentGame, null);
                            Node tu = CurrentGraph.FindNode((t + 1).ToString());
                            tu.Attr.Color = Color.SkyBlue;
                            //
                            Interlocked.Increment(ref FLOW_Step);
                            Interlocked.Increment(ref step);

                            //save this step
                            stepBack = SaveStepBack(1, CurrentGame, GameName);
                            stepBackGraph = SaveStepBackGraph(1, GameName);
                            //inform GUI:
                            UpdateGraph(GameName, CurrentGraph);
                            //value infostring Fpalpha(x)
                            method = type.GetMethod("Fpalpha_xToString");
                            string value;
                            try
                            {
                                value = (string)method.Invoke(CurrentGame, new Object[] { s, p1 });
                            }
                            catch
                            {
                                value = "Unkown";
                            }
                            //check if game.X==s, then index=0
                            UpdateStep1(s + 1, p1, false, value);

                        }

                    }// if step1

                    if (Interlocked.Read(ref FLOW_Step) == 2)
                    {
                        //PC is defender:
                        spoiler = type.GetProperty("Spoiler");
                        if ((bool)spoiler.GetValue(CurrentGame, null))
                        {
                            PropertyInfo prop = type.GetProperty("T_dup");
                            int t = (int)prop.GetValue(CurrentGame, null);
                            List<int> p2 = new List<int>();
                            method = type.GetMethod("Step2_PCIsDefender");
                            method.Invoke(CurrentGame, new Object[] { p2, alphabet.Count() });
                            // update the nodes in the graph:
                            Node tu = CurrentGraph.FindNode((t + 1).ToString());
                            tu.Attr.Color = Color.SkyBlue;
                            for (int j = 0; j < p2.Count(); j++)
                            {
                                Node node = CurrentGraph.FindNode((p2[j] + 1).ToString());
                                node.Attr.FillColor = Color.Turquoise;
                            }

                            //inform GUI:
                            UpdateGraph(GameName, CurrentGraph);
                            method = type.GetMethod("Step2ByUser");
                            bool check = (bool)method.Invoke(CurrentGame, new Object[] { p2 });
                            if (check)
                            {
                                Interlocked.Increment(ref FLOW_Step);
                                Interlocked.Increment(ref step);
                                //save this step
                                stepBack = SaveStepBack(2, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(2, GameName);
                                UpdateStep2(t, p2, true);
                            }
                            else
                            {
                                //Defender has lost the game!
                                Interlocked.Decrement(ref play);
                                method = type.GetMethod("areBisimilar");
                                check = (bool)method.Invoke(CurrentGame, null);
                                if (!check)
                                {
                                    //integration into GUI, Test generation of formulas:
                                    string formula = GetDistinguishingFormula(type);
                                    InfoTextToUser("Duplicator has lost the game, because condition of step 2 is not satisfied. In addition the states are not bisimilar." + Environment.NewLine + "The distinguishing formula is: " + Environment.NewLine + formula, true, true);
                                }
                                else InfoTextToUser("Duplicator has lost the game, because condition of step 2 is not satisfied. But Duplicator has a winning strategy.", true, true);
                            }
                        }
                        else //User is Defender
                        {
                            //send message Info to User: PC is waiting for move
                            while (2 == Interlocked.Read(ref FLOW_Step))
                            {
                                Thread.Sleep(2000);
                                //send info to user:
                            }
                            //After user-move only go on if move of user has been valid:
                            if (Interlocked.Read(ref FLOW_Step) == 3)
                            {
                                Interlocked.Increment(ref step);
                                //save this step
                                stepBack = SaveStepBack(2, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(2, GameName);
                                UpdateGraph(GameName, CurrentGraph);
                            }


                        }
                    }//if step2
                    if (Interlocked.Read(ref FLOW_Step) == 3)
                    {
                        //PC is Attacker:
                        spoiler = type.GetProperty("Spoiler");
                        if (!(bool)spoiler.GetValue(CurrentGame, null))
                        {
                            int selection = 0; ;
                            object[] arguments = new object[] { selection };
                            method = type.GetMethod("Step3_PCIsSpoiler");
                            int x_prime = (int)method.Invoke(CurrentGame, arguments);

                            if (x_prime >= 0)
                            {
                                selection = (int)arguments[0];
                                //call update step3
                                List<int> p = new List<int>();
                                p.Add(x_prime);
                                // update the nodes in the graph:
                                Node tu = CurrentGraph.FindNode((x_prime + 1).ToString());
                                tu.Attr.Color = Color.BlueViolet;
                                tu.Attr.FillColor = Color.White;
                                //update previous game init
                                PropertyInfo X = type.GetProperty("S_sp");
                                int x = (int)X.GetValue(CurrentGame, null);
                                Node tu_old = CurrentGraph.FindNode((x + 1).ToString());
                                tu_old.Attr.Color = Color.Gray;
                                //tu_old.Attr.FillColor = Color.White;
                                PropertyInfo Y = type.GetProperty("T_dup");
                                int y = (int)Y.GetValue(CurrentGame, null);
                                tu_old = CurrentGraph.FindNode((y + 1).ToString());
                                tu_old.Attr.Color = Color.Gray;
                                //tu_old.Attr.FillColor = Color.White;
                                //for (int j = 0; j < CurrentGraph.NodeCount; j++)
                                //{
                                //    Node node = CurrentGraph.FindNode((j + 1).ToString());
                                //    node.Attr.FillColor = Color.White;
                                //}

                                Interlocked.Increment(ref FLOW_Step);
                                Interlocked.Increment(ref step);
                                //update x in game instance
                                //save this step
                                stepBack = SaveStepBack(3, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(3, GameName);
                                UpdateGraph(GameName, CurrentGraph);
                                UpdateStep3(selection, p, false);
                            }
                            else
                            {
                                //Spoiler/PC has lost
                                Interlocked.Decrement(ref play);
                                method = type.GetMethod("areBisimilar");
                                bool check = (bool)method.Invoke(CurrentGame, null);
                                if (!check)
                                {
                                    //integration into GUI, Test generation of formulas:
                                    string formula = GetDistinguishingFormula(type);
                                    InfoTextToUser("Spoiler has lost the game, because condition of step 3 is not satisfied. In addition the states are not bisimilar." + Environment.NewLine + "The distinguishing formula is: " + Environment.NewLine + formula, true, false);
                                }
                                else InfoTextToUser("Spoiler has lost the game, because condition of step 3 is not satisfied. And Duplicator has a winning strategy.", true, false);

                            }
                        }
                        else //User is Attacker
                        {
                            //send message Info to User: PC is waiting for move
                            while (3 == Interlocked.Read(ref FLOW_Step))
                            {
                                Thread.Sleep(2000);
                                //send info to user:
                            }
                            //After user-move only go on if move of user has been valid:
                            if (Interlocked.Read(ref FLOW_Step) == 4)
                            {
                                Interlocked.Increment(ref step);
                                //save this step
                                stepBack = SaveStepBack(3, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(3, GameName);
                                UpdateGraph(GameName, CurrentGraph);
                            }
                        }

                    }//if step3
                    if (Interlocked.Read(ref FLOW_Step) == 4)
                    {
                        //PC is defender:
                        spoiler = type.GetProperty("Spoiler");
                        if ((bool)spoiler.GetValue(CurrentGame, null))
                        {
                            method = type.GetMethod("Step4_PCIsDefender");
                            int y_prime = (int)method.Invoke(CurrentGame, null);
                            // update the nodes in the graph:

                            if (y_prime >= 0)
                            {
                                Node tu = CurrentGraph.FindNode((y_prime + 1).ToString());
                                tu.Attr.Color = Color.SkyBlue;
                                //get the old Y_prime value
                                PropertyInfo Y = type.GetProperty("Y_prime");
                                int y = (int)Y.GetValue(CurrentGame, null);
                                Node tu_old = CurrentGraph.FindNode((y + 1).ToString());
                                tu_old.Attr.Color = Color.Gray;
                                //reset coloring of the graph before new round
                                for (int j = 0; j < CurrentGraph.NodeCount; j++)
                                {
                                    Node node = CurrentGraph.FindNode((j + 1).ToString());
                                    node.Attr.FillColor = Color.White;
                                    node.Attr.Color = Color.Gray;
                                }

                                while (Interlocked.Read(ref FLOW_Step) > 1)
                                    Interlocked.Decrement(ref FLOW_Step);
                                // 
                                while (Interlocked.Read(ref step) > 1)
                                    Interlocked.Decrement(ref step);
                                //update y in game instance
                                UpdateStep4(y_prime, true);
                                //save this step
                                stepBack = SaveStepBack(4, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(4, GameName);
                                //Update Game-step overview:
                                PropertyInfo X = type.GetProperty("X");
                                int x = (int)X.GetValue(CurrentGame, null);
                                Y = type.GetProperty("Y");
                                y = (int)Y.GetValue(CurrentGame, null);
                                InitGame(true, (x + 1).ToString(), (y + 1).ToString());

                            }
                            else
                            {
                                //Defender has lost the game!
                                Interlocked.Decrement(ref play);
                                method = type.GetMethod("areBisimilar");
                                bool check = (bool)method.Invoke(CurrentGame, null);
                                if (!check)
                                {
                                    //integration into GUI, Test generation of formulas:
                                    string formula = GetDistinguishingFormula(type);
                                    InfoTextToUser("Duplicator has lost the game, because condition of step 4 is not satisfied. In addition the states are not bisimilar." + Environment.NewLine + "The distinguishing formula is: " + Environment.NewLine + formula, true, true);
                                }
                                else InfoTextToUser("Duplicator has lost the game, because condition of step 4 is not satisfied. But Duplicator has a winning strategy.", true, true);
                            }
                            //inform GUI:
                            UpdateGraph(GameName, CurrentGraph);
                        }
                        else //User is Defender
                        {
                            //send message Info to User: PC is waiting for move
                            while (4 == Interlocked.Read(ref FLOW_Step))
                            {
                                Thread.Sleep(2000);
                                //send info to user:
                            }
                            //After user-move only go on if move of user has been valid:
                            if (Interlocked.Read(ref FLOW_Step) == 1)
                            {
                                while (Interlocked.Read(ref step) > 1)
                                    Interlocked.Decrement(ref step);
                                //save this step
                                stepBack = SaveStepBack(4, CurrentGame, GameName);
                                stepBackGraph = SaveStepBackGraph(4, GameName);
                                UpdateGraph(GameName, CurrentGraph);
                            }

                        }
                    }
                }
                catch
                {
                    Interlocked.Decrement(ref play); // Game thread should finish
                    InfoTextToUser("Please check your game configurations. Maybe you have an unvalid state selection?", true, true);
                }
            }//while play not interrupted or finished

        }



        public void UpdateGraphView(string name, Graph graph)
        {
            int step = (int)Interlocked.Read(ref FLOW_Step);
            update_GraphView.Invoke(this, new ModelEvent_UpdateGraphView(name, graph, true, step));
        }

        public void UpdateGraph(string name, Graph graph)
        {
            int step = (int)Interlocked.Read(ref FLOW_Step);
            update_Graph.Invoke(this, new ModelEvent_UpdateGraphView(name, graph, true, step));
        }

        private void InfoTextToUser(string info, bool over, bool UserisSpoiler)
        {
            int step = (int)Interlocked.Read(ref FLOW_Step);

            if (over)//game over, flags has to be adapted
            {
                //User has lost the game!
                while (Interlocked.Read(ref FLOW_Step) > 0)
                    Interlocked.Decrement(ref FLOW_Step);
            }
         
            InfoToUser.Invoke(this, new ModelEvent_UpdateGraphView(info, CurrentGraph, over, step, UserisSpoiler));
        }

        private void ReturnToStartPanel(string info)
        {
            returnToStartPanel.Invoke(this, new ModelEvent_InfoStep(info));
        }

        //called at initialization of game instance
        // and after Step 4: TODO: check?
        private void InitGame(bool UserIsSpoiler, string x, string y)
        {
            infoStep0.Invoke(this, new ModelEvent_InfoStep(UserIsSpoiler,x,y));
        }


        //clear everything insted step 1 of the game
        private void InitGameStepBack2(bool UserIsSpoiler)
        {
            backStep2.Invoke(this, new ModelEvent_InfoStep(UserIsSpoiler));
        }

        private void InitGameStepBack3(bool UserIsSpoiler, int x_, List<int> p1, string x_init, string y_init)
        {
            backStep3.Invoke(this, new ModelEvent_InfoStep(UserIsSpoiler, x_, p1, x_init, y_init));
        }

        private void InitGameStepBack4(bool UserIsSpoiler, int x_, int y_, List<int> p1, List<int> p2, string x_init, string y_init)
        {
            backStep4.Invoke(this, new ModelEvent_InfoStep(UserIsSpoiler, x_, y_, p1, p2, x_init, y_init));
        }

        private void UpdateStep1(int s, List<int> p1, bool spoiler, string value)
        {
            infoStep1.Invoke(this, new ModelEvent_InfoStep(value, p1, spoiler, s));
        }

        private void UpdateStep2(int t, List<int> p2, bool spoiler)
        {
            infoStep2.Invoke(this, new ModelEvent_InfoStep("", p2, spoiler, t));
        }


        private void UpdateStep3(int selection, List<int> x_prime, bool spoiler)
        {
            infoStep3.Invoke(this, new ModelEvent_InfoStep("", x_prime, spoiler, selection));
        }

        private void UpdateStep4(int y_prime, bool spoiler)
        {
            infoStep4.Invoke(this, new ModelEvent_InfoStep(y_prime, spoiler));
        }

        public void SendStep1(int s, List<int> p1)
        {
            // get current game instance
            Type type = CurrentGame.GetType();
            PropertyInfo spoiler = type.GetProperty("Spoiler");
            if ((bool)spoiler.GetValue(CurrentGame, null))
            {
                // set Move of Defender by User
                MethodInfo method = type.GetMethod("Step1ByUser");
                bool check = (bool)method.Invoke(CurrentGame, new Object[] { s, p1 });
                //Update of Flow_Step
                if (check)
                {
                    //value infostring Fpalpha(x)
                    method = type.GetMethod("Fpalpha_xToString");
                    string value;
                    try
                    {
                        value = (string)method.Invoke(CurrentGame, new Object[] {s-1, p1 });
                    }
                    catch
                    {
                        value = "Unkown";
                    }
                    UpdateStep1(s, p1, true, value);
                    Interlocked.Increment(ref FLOW_Step);
                    //graph update:
                    Node lila = CurrentGraph.FindNode((s ).ToString());
                    lila.Attr.Color = Color.BlueViolet;
                    for (int j = 0; j < p1.Count(); j++)
                    {
                        Node node = CurrentGraph.FindNode((p1[j]+1 ).ToString());
                        node.Attr.FillColor = Color.Violet;
                    }
                    // mark the state for the duplicator:
                    PropertyInfo prop = type.GetProperty("T_dup");
                    int t = (int)prop.GetValue(CurrentGame, null);
                    Node tu = CurrentGraph.FindNode((t + 1).ToString());
                    tu.Attr.Color = Color.SkyBlue;
                }
                else
                {
                    InfoTextToUser("Please, ckeck your move.", false, true);
                }
            }
        }

        public void SendStep2(List<int> predicate)
        {
            // get current game instance
            Type type = CurrentGame.GetType();
            PropertyInfo spoiler = type.GetProperty("Spoiler");
            if (!(bool)spoiler.GetValue(CurrentGame, null))
            {
                // set Move of Defender by User
                MethodInfo method = type.GetMethod("Step2ByUser");
                bool check = (bool)method.Invoke(CurrentGame, new Object[] { predicate });
                //Update of Flow_Step
                //check of condition 2??
                if (check)
                {
                    PropertyInfo prop = type.GetProperty("T_dup");
                    int t = (int)prop.GetValue(CurrentGame, null);
                    Node tu = CurrentGraph.FindNode((t + 1).ToString());
                    tu.Attr.Color = Color.SkyBlue;
                    for (int j = 0; j < predicate.Count(); j++)
                    {
                        Node node = CurrentGraph.FindNode((predicate[j] + 1).ToString());
                        node.Attr.FillColor = Color.Turquoise;
                    }
                    //Update GUI
                    UpdateStep2(t, predicate, true);
         
                    //Update FLOW
                    Interlocked.Increment(ref FLOW_Step); //valid move of the user and game can continue
                }
                else
                {
                    //User has lost the game!
                    Interlocked.Decrement(ref play);  // Decrement to return to GameFlow While, but then with Interrupt FLAG

                    // Here inside decrement of FLAG: Flow_step
                    // ADD Textinfo about bisimilarity of the current states!
                    method = type.GetMethod("areBisimilar");
                    check = (bool)method.Invoke(CurrentGame, null);
                    if (!check)
                    {
                        //integration into GUI, Test generation of formulas:
                        string formula = GetDistinguishingFormula(type);
                        InfoTextToUser("Duplicator has lost the game, because condition of step 2 is not satisfied. In addition the states are not bisimilar." + Environment.NewLine + "The distinguishing formula is: " + Environment.NewLine + formula, true, false);
                    }
                    else InfoTextToUser("Duplicator has lost the game, because condition of step 2 is not satisfied. But Duplicator has a winning strategy.", true, false);

                }

            }

        }

        public void SendStep3(List<int> x_prime, int selection)
        {
            // get current game instance
            Type type = CurrentGame.GetType();
            PropertyInfo spoiler = type.GetProperty("Spoiler");
            if ((bool)spoiler.GetValue(CurrentGame, null))
            {
                PropertyInfo X = type.GetProperty("X");
                int x = (int)X.GetValue(CurrentGame, null);
                PropertyInfo Y = type.GetProperty("Y");
                int y = (int)Y.GetValue(CurrentGame, null);
                // set the user move wrt predicate
                if (selection == 0)
                {
                    PropertyInfo prop = type.GetProperty("X");
                    prop.SetValue(CurrentGame, x_prime[0]);
                }
                if (selection == 1)
                {
                    PropertyInfo prop = type.GetProperty("Y");
                    prop.SetValue(CurrentGame, x_prime[0]);
                }

                // check if x_prime (set to game.x) is incl. in chosen predicate
                MethodInfo method = type.GetMethod("CheckUserMove3");
                bool check = (bool)method.Invoke(CurrentGame, new Object[] { selection });
                // if true: update GUI
                if (check)
                {
                    //update previous game init (in graph)
                    if (selection == 0)
                    {
                        X = type.GetProperty("X_prime");
                        X.SetValue(CurrentGame, x);
                        Node tu_old = CurrentGraph.FindNode((x + 1).ToString());
                        tu_old.Attr.Color = Color.Gray;
                        //tu_old.Attr.FillColor = Color.White;
                    }
                    else
                    {
                        Y= type.GetProperty("Y_prime");
                        Y.SetValue(CurrentGame, y);
                        Node tu_old2 = CurrentGraph.FindNode((y + 1).ToString());
                        tu_old2.Attr.Color = Color.Gray;
                        //tu_old2.Attr.FillColor = Color.White;
                    }               
                    UpdateStep3(selection, x_prime, true);
                    Interlocked.Increment(ref FLOW_Step);
                }
                else
                {
                    InfoTextToUser("Your chosen state is not included in your predicate.", false, true);
                }
            }

        }

        public void SendStep4(int y_prime)
        {
            // get current game instance
            Type type = CurrentGame.GetType();
            PropertyInfo prop_selection = type.GetProperty("Selection");
            int selection = (int)prop_selection.GetValue(CurrentGame, null);
            // set the user move wrt predicate:
            PropertyInfo X = type.GetProperty("X");
            int old_x = (int)X.GetValue(CurrentGame, null);
            PropertyInfo Y = type.GetProperty("Y");
            int old_y = (int)Y.GetValue(CurrentGame, null);
            // set the user move wrt predicate
            if (selection == 0)
            {
                PropertyInfo prop_y = type.GetProperty("Y");
                prop_y.SetValue(CurrentGame, y_prime);//selection by user
                prop_y = type.GetProperty("Y_prime");
                prop_y.SetValue(CurrentGame, old_y);
            }
            if (selection == 1)
            {
                PropertyInfo prop_x = type.GetProperty("X");
                prop_x.SetValue(CurrentGame, y_prime);//selection by user
                prop_x = type.GetProperty("X_prime");
                prop_x.SetValue(CurrentGame, old_x);
            }
      
            // check if x_prime is incl. in chosen predicate
            MethodInfo method = type.GetMethod("CheckUserMove4");
            bool check = (bool)method.Invoke(CurrentGame, null);
            // if true: update GUI
            if (check)
            {
                //reset coloring of the graph before new round
                for (int j = 0; j < CurrentGraph.NodeCount; j++)
                {
                    Node node = CurrentGraph.FindNode((j + 1).ToString());
                    node.Attr.FillColor = Color.White;
                    node.Attr.Color = Color.Gray;
                }
                //TODO: reset all the textboxes and check boxes
                UpdateStep4(y_prime, false);
                while (Interlocked.Read(ref FLOW_Step) > 1)
                    Interlocked.Decrement(ref FLOW_Step);


                //Update Game-step overview:
                X = type.GetProperty("X");
                int x = (int)X.GetValue(CurrentGame, null);
                Y = type.GetProperty("Y");
                int y = (int)Y.GetValue(CurrentGame, null);
                InitGame(true, (x + 1).ToString(), (y + 1).ToString());
            }
            else
            {
                InfoTextToUser("Your chosen state is not included in your predicate.", false, true);
            }
        }

        public void SendStepSwitchCase(string name, List<int> selection_1, int selection)
        {
            // correct game FLOW check:
            bool check = false;
            // get current game instance
            Type type = CurrentGame.GetType();
            PropertyInfo spoiler_info = type.GetProperty("Spoiler");
            bool spoiler = ((bool)spoiler_info.GetValue(CurrentGame, null));
            if (Interlocked.Read(ref FLOW_Step) == 1 && spoiler) check = true;
            if (Interlocked.Read(ref FLOW_Step) == 3 && spoiler) check = true;
            if (Interlocked.Read(ref FLOW_Step) == 2 && !spoiler) check = true;
            if (Interlocked.Read(ref FLOW_Step) == 4 && !spoiler) check = true;

            if (check)
            {
                switch (Interlocked.Read(ref FLOW_Step))
                {
                    case 1:
                        SendStep1(selection, selection_1);
                        break;
                    case 2:
                        SendStep2(selection_1); // wrong moves just count! TODO: Update to more usability for the user
                        break;
                    case 3:
                        int x_prime;
                        List<int> p_i;
                        PropertyInfo prop = type.GetProperty("P1");
                        if (selection == 1) prop = type.GetProperty("P2");
                        p_i = (List<int>)prop.GetValue(CurrentGame, null);

                        try
                        {
                            x_prime = selection_1[0];
                        }
                        catch
                        {
                            if (p_i.Count == 0)
                            {
                                Interlocked.Decrement(ref play);
                                InfoTextToUser("Spoiler has lost the game, because condition of step 3 is not satisfied.", true, true);
                            }
                            else InfoTextToUser("Please ckeck your move.", false, true); //USer maybe forgets to select, but there has been options
                        }

                        if (p_i.Count == 0)
                        {
                            Interlocked.Decrement(ref play);
                            InfoTextToUser("Spoiler has lost the game, because condition of step 3 is not satisfied.", true, true);
                        }
                        else
                            SendStep3(selection_1, selection);
                        break;
                    case 4:
                        List<int> p_j;
                        type = CurrentGame.GetType();
                        prop = type.GetProperty("P1");
                        if (selection == 1) prop = type.GetProperty("P2");
                        p_j = (List<int>)prop.GetValue(CurrentGame, null);
                        if (p_j.Count == 0)
                        {
                            Interlocked.Decrement(ref play);
                            InfoTextToUser("Duplicator has lost the game, because condition of step 4 is not satisfied.", true, false);
                        }
                        else
                        {
                            if (selection_1.Count == 0)
                            {
                                InfoTextToUser("Please ckeck your move.", false, false);
                            }
                            else
                            {
                                SendStep4(selection_1[0]);
                            }

                        }
                        break;
                    default:
                        Console.WriteLine("");
                        break;
                }
            }


        }

        public void ExitGame(string name, string functor)
        {
            // stop game changing step and play
            int step = (int)Interlocked.Read(ref FLOW_Step);
            //User wants to stop the game
            while (Interlocked.Read(ref FLOW_Step) > 0)
                Interlocked.Decrement(ref FLOW_Step);
            Interlocked.Decrement(ref play); // Game thread should finish

            // clear graph vizu:
            //reset coloring of the graph before new round
            for (int j = 0; j < CurrentGraph.NodeCount; j++)
            {
                Node node = CurrentGraph.FindNode((j + 1).ToString());
                node.Attr.FillColor = Color.White;
                node.Attr.Color = Color.Gray;
            }
            //clean the stacks:
            stepBackGraph.Clear();
            stepBack.Clear();
        }

        private void KeepTheIdsConsistentToGame(int states, ref Graph graph)
        {
            // first search for the non used ids
            List<int> notUsedId = new List<int>();
            for (int i = 0; i < states; i++)
            {
                Node node = graph.FindNode((i + 1).ToString());
                if (node == null) notUsedId.Add(i);
            }
            //only adapt graph, if id's are not consistent to game!
            if (notUsedId.Count > 0)
            {
            // then rename the wrong one to the non used.
            Dictionary<string, int> consistentID = new Dictionary<string, int>();
            int nextNonUsed = 0;
            Graph gameGraph = new Graph();
            foreach (Node n in graph.Nodes)
            {
                try
                {
                    int test = Int32.Parse(n.Id);
                    Node node_i = new Node(n.Id);
                    node_i.LabelText = n.Id.ToString() + "." + n.LabelText;
                    //clean up the label from redundance:
                        while (node_i.LabelText.Contains(n.Id.ToString() + "."+ n.Id.ToString() + "."))
                        {
                            node_i.LabelText = node_i.LabelText.Replace(n.Id.ToString() + "." + n.Id.ToString() + ".", n.Id.ToString() + ".");
                        }
                    node_i.Attr.Shape = Shape.Circle;
                    node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;
                    gameGraph.AddNode(node_i);
                }
                catch
                {
                    string replace = n.Id;
                    consistentID.Add(n.Id, notUsedId[nextNonUsed] + 1);
                    string newID = (notUsedId[nextNonUsed] + 1).ToString();

                    Node node_i = new Node(newID);
                    node_i.LabelText = newID + "." + n.LabelText;
                    //clean up the label from redundance:
                       while (node_i.LabelText.Contains(n.Id.ToString() + "." + n.Id.ToString() + "."))
                        {
                            node_i.LabelText = node_i.LabelText.Replace(n.Id.ToString() + "." + n.Id.ToString() + ".", n.Id.ToString() + ".");
                        }
                        node_i.Attr.Shape = Shape.Circle;
                    node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;
                    gameGraph.AddNode(node_i);
                    nextNonUsed++;
                }
                   
                }



            foreach (Edge edge in graph.Edges)
            {
                string source = edge.Source;
                string target = edge.Target;



                if (consistentID.ContainsKey(source))
                {
                    if (consistentID.ContainsKey(target))
                    {
                        gameGraph.AddEdge(consistentID[source].ToString(), edge.LabelText, consistentID[target].ToString());

                    }
                    else
                    {
                        gameGraph.AddEdge(consistentID[source].ToString(), edge.LabelText, target);
                    }

                }
                else if (consistentID.ContainsKey(target))
                {
                    gameGraph.AddEdge(source, edge.LabelText, consistentID[target].ToString());
                }

                if (!consistentID.ContainsKey(source) && !consistentID.ContainsKey(target))
                {
                    gameGraph.AddEdge(source, edge.LabelText, target);
                }
            }

            foreach (Edge edge in graph.Edges)
            {
                edge.Label.FontSize = 5;
            }

           


                graph = gameGraph;
            }
        }

        private void UseAllIds(int states, ref Graph graph)
        {
            //get the biggest used id:
            int biggestUsedId = states;
            foreach(Node n in graph.Nodes)
            {
                int id = Int32.Parse(n.Id);
                if (biggestUsedId < id) biggestUsedId = id;
            }

            // first search for the non used ids
            List<int> notUsedId = new List<int>();
            for (int i = 0; i < states; i++)
            {
                Node node = graph.FindNode((i + 1).ToString());
                if (node == null) notUsedId.Add(i);
            }
            //only adapt graph, if id's are not consistent to game!
            if (notUsedId.Count > 0)
            {
                Dictionary<string, string> markedNonUsed = new Dictionary<string, string>();
                Dictionary<string, string> mappingInfo = new Dictionary<string, string>();
                Graph gameGraph = new Graph();
                for (int i = 0; i < states; i++)
                {
                    Node n = graph.FindNode((i + 1).ToString());
                    if (n == null || markedNonUsed.ContainsKey((i + 1).ToString())) //or set to unused by dict
                    {
                        //i is not used as id
                        Node node_i = new Node((i + 1).ToString());
                        //now remove from unused:
                        if (n != null) markedNonUsed.Remove((i + 1).ToString());
   ;                        // get next used node-id and mark as non used
                        for (int j = i + 1; j < biggestUsedId; j++)
                        {
                            Node node = graph.FindNode((j + 1).ToString());//get node j+1 and rename by i+1
                            if (node != null && !mappingInfo.ContainsKey((j + 1).ToString()))//else go on with search of next used id
                            {
                                markedNonUsed.Add((j + 1).ToString(), (i + 1).ToString());//j+1 node is now i+1 and therefore j+1 is available again as node id
                                //for next step:
                                mappingInfo.Add((j + 1).ToString(), (i + 1).ToString());
                                //remember old ID in Label (use in edge construction)
                                j = biggestUsedId;//stop searching here, one is enough
                                string j_Lable = node.LabelText;// get the "real" labeltext and just replace id
                                int indexOf = j_Lable.IndexOf(".");
                                j_Lable=j_Lable.Substring(indexOf + 1, j_Lable.Length - (indexOf+1));
                                node_i.LabelText = (i + 1).ToString() + "." + j_Lable;
                                //clean up label from redundance:
                                while (node_i.LabelText.Contains((i + 1).ToString() + "." + (i + 1).ToString() + "."))
                                {
                                    node_i.LabelText = node_i.LabelText.Replace((i + 1).ToString() + "." + (i + 1).ToString() + ".", (i + 1).ToString() + ".");
                                }
                                node_i.Attr.Shape = Shape.Circle;
                                node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;
                                gameGraph.AddNode(node_i);
                            }
                        }

                    }
                    else
                    {
                        Node node_i = new Node(n.Id);
                        node_i.LabelText = n.Id.ToString() + "." + n.LabelText;
                        //clean up the label from redundance:
                        while (node_i.LabelText.Contains(n.Id.ToString() + "." + n.Id.ToString() + "."))
                        {
                            node_i.LabelText = node_i.LabelText.Replace(n.Id.ToString() + "." + n.Id.ToString() + ".", n.Id.ToString() + ".");
                        }
                        node_i.Attr.Shape = Shape.Circle;
                        node_i.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;
                        gameGraph.AddNode(node_i);
                    }
                }

                //construct the edges:
                foreach (Edge edge in graph.Edges)
                {
                    string source = edge.Source;
                    string target = edge.Target;

                    //source-id has been replaced
                    if (mappingInfo.ContainsKey(source))
                    {
                        //and target-id has been replaced
                        if (mappingInfo.ContainsKey(target))
                        {
                            gameGraph.AddEdge(mappingInfo[source], edge.LabelText, mappingInfo[target]);
                        }
                        else
                        {
                            gameGraph.AddEdge(mappingInfo[source], edge.LabelText, target);
                        }
                    }

                    //only target-id has been replaced
                    else if (mappingInfo.ContainsKey(target))
                    {
                        gameGraph.AddEdge(source, edge.LabelText, mappingInfo[target]);
                    }


                    //this node-ids have never been replaced
                    if (!mappingInfo.ContainsKey(source) && !mappingInfo.ContainsKey(target))
                    {
                        gameGraph.AddEdge(source, edge.LabelText, target);
                    }

                }


                foreach (Edge edge in graph.Edges)
                {
                    edge.Label.FontSize = 5;
                }

                graph = gameGraph;
            }
        }

        public void ResetGraph(string name, string functor, Graph graph, bool backToInit)
        {
            // extraxt transitionsystem from graph
            int states = graph.Nodes.Count() ;
            // Adapt node-ids to game concept:
            KeepTheIdsConsistentToGame(states, ref graph);
            // Adapt smaller graph to enable content-info
            UseAllIds(states, ref graph);

            List<string> alphabet=new List<string>();
            //call getTSFrom Graph method or switch to standard
                 T F = new T();
            MethodInfo method_functor = F.GetType().GetMethod(" GetTSfromGraph");
            try
            {
                //non standard edge-label
                Object transitionSystem = method_functor.Invoke(F, new Object[] { graph, states });
                try
                {
                    datamodelMatrix.Add(name, transitionSystem);
                }
                catch
                {
                    //TODO infrom gui about error
                }

            }
            catch (Exception exe)
            {
                
                //extract content matrix
                List<string[]> contentInfo = new List<string[]>();
                //standard transformation based on matrix representation label and weight "a, weight"
                //ONE wrt Functor  : 1 based on F.matrixValueType
                try
                {
                    
                    FieldInfo One = F.GetType().GetField("matrixValueType", BindingFlags.NonPublic | BindingFlags.Instance);
                    //FieldInfo [] One = F.GetType().GetFields();
                    MemberInfo[] O = F.GetType().GetMember("matrixValueType", BindingFlags.NonPublic | BindingFlags.Instance);
                  
                    Type one = (Type) One.GetValue(F);
                    // generic check, waht is One in this context:
                    //IF an edge exists, then this is the "greatest" label info wrt to weigted systems
                    string string1 = "1"; 
                    if (! ((one == typeof(int)) || (one == typeof(uint)) || (one == typeof(double))))
                    {
                        try
                        {
                           string1 = (string)((dynamic)one.GetMethod("One").Invoke(null, null)).ToString();
                        }
                        catch (Exception e1)
                        {
                            throw (new Exception("One for type " + one.ToString() + " not defined! Define function One() in " + one.ToString() + " that yields the one of said semiring."));
                        };
                    }

                    //Zero wrt Functor : 0, which means: NO edge as label info
                    string string0 = "0";
                    if (!((one == typeof(int)) || (one == typeof(uint)) || (one == typeof(double))))
                    {
                        try
                        {
                            string0 = (string)((dynamic)one.GetMethod("Zero").Invoke(null, null)).ToString();
                        }
                        catch (Exception e0)
                        {
                            throw (new Exception("Zero for type " + one.ToString() + " not defined! Define function One() in " + one.ToString() + " that yields the zero of said semiring."));
                        };
                    }

                    foreach (Edge edge in graph.Edges)
                    {
                        string label = edge.LabelText;
                        string weight =  string1;
                        // old for working out the rest with NFA or DX+1- Functor: string weight = "1";
                        if (label.Contains(","))
                        {
                            string[] lab = label.Split(',');
                            label = lab[0];
                            weight = lab[1]; //get the real weight from graph
                        }
                        string source = edge.Source;
                        string target = edge.Target;
                        if (!alphabet.Contains(label)) alphabet.Add(label);//create the alphabet
                        if(alphabet.Contains("NL") && alphabet.Count > 1)
                        {
                            alphabet.Remove("NL");//NL is reserved for no label
                            label = "nl";
                            int a = 0;
                            while (alphabet.Contains(label)){
                                label = "nl" + a.ToString();
                                a++;
                            }
                            alphabet.Add(label);
                        }
                        contentInfo.Add(new string[4] { source, target, label, weight });
                    }
                    //create object based on prepared content:
                    //If Standard, then should be implemented!
                    try
                    {
                        Type generic = Type.GetType("TBeg." + functor);
                        Object obj = Activator.CreateInstance(generic, null);
                        MethodInfo method = generic.GetMethod("ReturnRowCount");
                        // no termination supported yet
                        int rowCount = (int)method.Invoke(obj, new object[] { alphabet.ToArray(), states, false });
                        string[] content = new string[rowCount * states];
                        //Zero element wrt functor:
                        for (int i = 0; i < rowCount * states; i++)
                        {
                            content[i] = string0; //specified by the weigted system BY USER
                        }

                        int alpha = alphabet.Count;
                        for (int i = 0; i < contentInfo.Count; i++)
                        {
                            int source = Int32.Parse(contentInfo[i][0]) - 1;
                            int target = Int32.Parse(contentInfo[i][1]) - 1;
                            int label_int = alphabet.IndexOf(contentInfo[i][2]);
                            // Int32.Parse(content[i * (alphabet.Length * states.Count) + j]);
                            content[source * alpha * states + ((target) * alpha + label_int)] = contentInfo[i][3];
                        }

                        List<int> statesList = new List<int>();
                        for (int i = 0; i < states; i++)
                        {
                            statesList.Add(i);
                        }


                        bool unsaved = true;
                        int id = 0;
                        while (unsaved)
                        {
                            unsaved = false;

                            try
                            {

                                string nameGraphUpdate = name + id.ToString();
                                InitandSaveMatrix(nameGraphUpdate, functor, alphabet.ToArray(), statesList, content, "");
                                //save  csv
                                string[] contentCSV;
                                contentCSV = new string[3];
                                contentCSV[0] = states.ToString();
                                contentCSV[1] = String.Join(",", alphabet.ToArray());
                                contentCSV[2] = String.Join(",", content);

                                //get the transition system
                                Object transitionSystem = datamodelMatrix[nameGraphUpdate];

                                //only use this if back to init graph
                                if (backToInit)
                                {
                                    graph = new Graph();
                                    ExtractGraphfromTS(transitionSystem, ref graph, statesList, alphabet.ToArray());
                                }


                                CurrentGraph = graph;

                                UpdateGraph_Name(nameGraphUpdate, graph);
                                UpdateGraphView(nameGraphUpdate, graph);

                                PropertyInfo InitGraph = transitionSystem.GetType().GetProperty("InitGraph1");
                                //Deep copy better:
                                InitGraph.SetValue(transitionSystem, DeepCopyGraph(graph));

                                //TODO: this throws exception
                                //SaveToCSV(Application.StartupPath + "\\Transitionsystems\\" + functor + "\\" + nameGraphUpdate + ".ts", functor, contentCSV, "");

                                //in case name is already used:
                                id = id + 1;
                            }


                            catch (ArgumentException ae)
                            {
                                unsaved = true;
                                id = id + 1;
                            }


                        }

                    }
                    catch (Exception e)
                    {
                        if (e.InnerException != null)
                        {   // infrom gui about error
                            if (e.InnerException.Message.Contains("Invalid input: e.g. for the state "))
                            {
                                throw new ArgumentException(e.InnerException.Message);
                            }
                        }
                        else throw e;
                    }


                }
                catch (Exception e)
                {
                    // infrom gui about error
                    if(e.InnerException != null)
                    {
                        if (e.InnerException.Message.Contains("Invalid input: e.g. for the state "))
                        {
                            throw new ArgumentException(e.InnerException.Message);
                        }
                    }
                 
                    else throw e;

                }       

                // Info to GUI if it woked well and set GrpahEditorfile to current-name!
            }
        }//end

        private void UpdateGraph_Name(string nameGraphUpdate, Graph graph)
        {
            GraphNameUpdate.Invoke(this, new  ModelEvent_UpdateGraphView(nameGraphUpdate, graph, false, 0));
        }

        public void CheckConsistenceOfGameGraph(string name, string functor)
        {
            bool graphsareConsistent;
            //get the currentGraph and the InitGraph of the transitionsystem
            if (CheckMatrixName(name)) {
                Object transitionSystem = datamodelMatrix[name];
                PropertyInfo InitGraphProp = transitionSystem.GetType().GetProperty("InitGraph1");
                //Is this only a reference check?? 
                Graph InitGraph = (Graph)InitGraphProp.GetValue(transitionSystem);
                //graphsareConsistent = CurrentGraph.Equals(InitGraph);
                graphsareConsistent = GraphsAreEqual(CurrentGraph, InitGraph);
            //check for equality
            GraphIsConsistentWithGame.Invoke(this, new ModelEvent_UpdateGraphView(name, graphsareConsistent, InitGraph));
            }

        }

        private bool GraphsAreEqual(Graph gr1, Graph gr2)
        {
            if (gr1.NodeCount != gr2.NodeCount) return false;
            if (gr1.EdgeCount != gr2.EdgeCount) return false;

            //node check: ID and LabelText
            foreach (Node node_gr1 in gr1.Nodes)
            {
                Node node_gr2 = gr2.FindNode(node_gr1.Id);
                if (node_gr2 == null) return false;
                if (!node_gr1.LabelText.Equals(node_gr2.LabelText)) return false;
            }
            foreach (Node node_gr2 in gr2.Nodes)
            {
                Node node_gr1 = gr1.FindNode(node_gr2.Id);
                if (node_gr1 == null) return false;
                if (!node_gr2.LabelText.Equals(node_gr1.LabelText)) return false;
            }

            bool ret = false;
            // Edge Check: Source-Target IDs and Label
            foreach (Edge edge_gr1 in gr1.Edges)
            {
                ret = false;
                // find the same edge in gr2:
                foreach (Edge edge_gr2 in gr2.Edges)
                {
                    if (edge_gr1.Source.Equals(edge_gr2.Source) && edge_gr1.Target.Equals(edge_gr2.Target) && edge_gr1.LabelText.Equals(edge_gr2.LabelText)) ret = true; 
                }
                if (!ret) return false;
            }
            //check for the reverse relation:
            foreach (Edge edge_gr1 in gr2.Edges)
            {
                ret = false;
                // find the same edge in gr2:
                foreach (Edge edge_gr2 in gr1.Edges)
                {
                    if (edge_gr1.Source.Equals(edge_gr2.Source) && edge_gr1.Target.Equals(edge_gr2.Target) && edge_gr1.LabelText.Equals(edge_gr2.LabelText)) ret = true;
                }
                if (!ret) return false;
            }

            return true;
        }
    }
}
