using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using G = GraphModel;

namespace TBeg
{
    public interface IController
    {
        Dictionary<string, IModel> Models
        {
            get;
            set;
        }
    }
    class Controller : IController
    {

        IView view;
        Dictionary<String, IModel> models;

        public Dictionary<string, IModel> Models
        {
            get
            {
                return models;
            }

            set
            {
                models = value;
            }
        }


        // The controller which implements the IController interface ties the view and model 
        // together and attaches the view via the IModelInterface and addes the event
        // handler to the view_changed function. The view ties the controller to itself.
        public Controller(IView v, Dictionary<String, IModel> models)
        {
            view = v;
            this.Models = models;
            view.SetController(this);

            foreach (string s in models.Keys)
            {
                models[s].Attach((IModelObserver)view);
            }

            view.InitandSaveMatrix += new ViewHandler_Matrix<IView>(this.InitandSaveMatrix);
            view.CheckMatrixName += new ViewHandler_MatrixName<IView>(this.CheckMatrixName);
            view.InitandSaveGame += new ViewHandler_Matrix<IView>(this.InitandSaveGame);
            view.InitGame+= new ViewHandler_Game<IView>(this.InitGame);
            view.StepBack += new ViewHandler_Game<IView>(this.StepBack);
            view.SendStep += new ViewEvent_GameStepUser<IView>(this.SendStep);
            view.SaveToCSV += new ViewHandler_Matrix<IView>(this.SaveToCSV);
            view.LoadFromCSV += new ViewHandler_Matrix<IView>(this.LoadFromCSV);
            view.ExitGame += new ViewHandler_Game<IView>(this.ExitGame);
            view.ResetGraph += new ViewEvent_GameGraph<IView>(this.ResetGraph);
            view.CheckConsistenceOfGameGraph += new ViewHandler_Game<IView>(this.CheckConsistenceOfGameGraph);
            view.GetValidator += new ViewHandler_Validator<IView>(this.GetValidator);
            view.AddGraph += new ViewHandler_Graph<IView>(this.AddGraph);
        }

        private void CheckConsistenceOfGameGraph(IView sender, ViewEvent_Game e)
        {
                getTypeModel(e.functor).CheckConsistenceOfGameGraph(e.name, e.functor);
        }

        private void ResetGraph(IView sender, ViewEvent_GameGraph e)
        {
                getTypeModel(e.functor).ResetGraph(e.name, e.functor, e.graph, e.BackToInit);
        }

        private void ExitGame(IView sender, ViewEvent_Game e)
        {
                getTypeModel(e.functor).ExitGame(e.name, e.functor);
        }

        private void StepBack(IView sender, ViewEvent_Game e)
        {
                getTypeModel(e.functor).StepBack(e.name, e.functor);
        }

        private void LoadFromCSV(IView sender, ViewEvent_Matrix e)
        {
                getTypeModel(e.functor).LoadFromCSV(e.name, e.functor, e.content, e.col, e.row, e.optional);
        }

        private void SaveToCSV(IView sender, ViewEvent_Matrix e)
        {
                getTypeModel(e.functor).SaveToCSV(e.name, e.functor,  e.content, e.optional);
        }

        private void SendStep(IView sender, ViewEvent_GameStepUser e)
        {
                getTypeModel(e.functor).SendStepSwitchCase(e.name, e.p2,e.selection);
            
        }

        private void InitGame(IView sender, ViewEvent_Game e)
        {
                getTypeModel(e.functor).InitGame(e.name, e.functor, e.initPair,e.spoiler);   
        }

        private void InitandSaveGame(IView sender, ViewEvent_Matrix e)
        {
                getTypeModel(e.functor).InitandSaveGame(e.name, e.functor, e.alphabet, e.states, e.content,e.optional);
        }

        private bool CheckMatrixName(IView sender, ViewEvent_Matrix e)
        {
            bool ret = false; ;
            try
            {
               ret= getTypeModel(e.functor).CheckMatrixName(e.name);
            }
            catch
            {
                return false;
            }
            return ret;
        }

        /// <summary>
        /// get the datamodel by functor-name
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IModel getTypeModel(String t)
        {
            return Models[t];
        }

        private void InitandSaveMatrix(IView sender, ViewEvent_Matrix e)
        {
                getTypeModel(e.functor).InitandSaveMatrix(e.name,e.functor,e.alphabet,e.states,e.content,e.optional);
        }

        private void GetValidator(string functor)
        {
                getTypeModel(functor).GetValidator();
        }

        private void AddGraph(G::Graph graph, string functor)
        {
                getTypeModel(functor).AddGraph(graph, functor);
        }
    }
}
