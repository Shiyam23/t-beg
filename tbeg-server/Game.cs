using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Threading.Tasks;

namespace TBeg
{

    /// <summary>
    /// 
    /// compute equvalence classes, hence all pairs assigned with 1 in the matrix
    /// store state and according equivalence class
    /// 
    /// checkCondition2 sould return true if both conditions are fine, otherwise also the corresponding (x,y,p) information
    /// hence return (bool, int , int, List<int>) universal methode, usable for computation and during the game
    /// 
    /// Interaction with the player, just use random moves for first prototype.
    /// S starts with a move and waits for the input by the user.
    /// 
    /// check Condition works with functions/operators of the Functor F. 
    /// 
    /// 
    /// </summary>
    class Game<T, TS> : IDisposable
    {
        //Memory stuff:
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        private IFunctor<T, TS> functor;
        private Matrix<T, TS> ts; //transition system
        private int states;
        private bool spoiler;
        private string name;
        //public Dictionary<int, List<int>[]> winningMovesSpoiler = new Dictionary<int, List<int>[]>();//contains the winning moves of the Spoiler
        public Dictionary<Tuple, List<List<int>>> winningMovesSpoiler;// = new Dictionary<Tuple, List<List<int>>>();//contains the winning moves of the Spoiler

        int[] bisimilarPairs;

        int init_X;
        int init_Y;

        //for the game flow:
        int FLOW_STEP = 0;
        int x;
        int y;
        List<int> p1;
        List<int> p2;
        int s_sp; //selection step 1
        int t_dup;//selection step 2
        int x_prime;
        int y_prime;
        int selection;

        Random random;


        public int FLOW_STEP1
        {
            get
            {
                return FLOW_STEP;
            }

            set
            {
                FLOW_STEP = value;
            }
        }

        public int X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }

        public int[] BisimilarPairs
        {
            get
            {
                return bisimilarPairs;
            }

            set
            {
                bisimilarPairs = value;
            }
        }

        public bool Spoiler
        {
            get
            {
                return spoiler;
            }

            set
            {
                spoiler = value;
            }
        }

        public int S_sp
        {
            get
            {
                return s_sp;
            }

            set
            {
                s_sp = value;
            }
        }

        public int T_dup
        {
            get
            {
                return t_dup;
            }

            set
            {
                t_dup = value;
            }
        }

        public List<int> P1
        {
            get
            {
                return p1;
            }

            set
            {
                p1 = value;
            }
        }

        public List<int> P2
        {
            get
            {
                return p2;
            }

            set
            {
                p2 = value;
            }
        }

        public int X_prime
        {
            get
            {
                return x_prime;
            }

            set
            {
                x_prime = value;
            }
        }

        public int Y_prime
        {
            get
            {
                return y_prime;
            }

            set
            {
                y_prime = value;
            }
        }

        public int Selection
        {
            get
            {
                return selection;
            }

            set
            {
                selection = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public Matrix<T, TS> Ts
        {
            get
            {
                return ts;
            }

            set
            {
                ts = value;
            }
        }

        public int Init_X
        {
            get
            {
                return init_X;
            }

            set
            {
                init_X = value;
            }
        }

        public int Init_Y
        {
            get
            {
                return init_Y;
            }

            set
            {
                init_Y = value;
            }
        }

        public Game(Matrix<T, TS> ts, string name, int x, int y, bool spoiler)
        {
            this.Init_X = x;
            this.Init_Y = y;   
            this.functor = ts.functor;
            this.ts = ts; // one ref in datamodel!
            this.name = name;
            this.states = ts.columns();
            this.Spoiler = spoiler;
            this.X = x;
            this.Y = y;
            // Bisimulation has to be computed!
            winningMovesSpoiler = new Dictionary<Tuple, List<List<int>>>();
            BisimilarPairs = computeBisimulation(ref winningMovesSpoiler);
            this.random = new Random(DateTime.Now.Millisecond); // initialized once for the game
        }



        //deep copy method for step back:
        public void DeepCopyClone(int step, int s, int t, List<int> p1, List<int> p2, int x_prime, int y_prime, int init_X, int init_Y, int selection)
        {
            this.Init_X = init_X;
            this.Init_Y = init_Y;
            this.Y = y;
            this.X = x;
            this.Selection = selection;
            this.x_prime = x_prime;
            this.y_prime = y_prime;
            this.s_sp = s;
            this.t_dup = t;
            // deep copies:
            if (step >= 1)
            {
                this.p1 = new List<int>();
                foreach (var elt in p1)
                {
                    this.p1.Add((int)elt);
                }
            }
            if (step >= 2)
            {
                this.p2 = new List<int>();
                foreach (var elt in p2)
                {
                    this.p2.Add((int)elt);
                }
            }
        }


        //Apply game-strategy:

        /// <summary>This method returns the correct state and predicate for the spoiler
        /// in Step 1 of the game
        /// TODO: info selection of the tuple!
        /// </summary>
        public int Step1_PCIsSpoiler(ref int s, ref List<int> p1, int alphabet)
        {
            //has strategy:
            // return alwas the first move, which was added
            Tuple key_1 = new Tuple(this.x, this.y); // no negation
            Tuple key_2 = new Tuple(this.y, this.x); // add negation
            int selection = 0;
            //
            if (BisimilarPairs[x * states + y] == 0) {
                if (winningMovesSpoiler.ContainsKey(key_1))
                {
                    p1 = winningMovesSpoiler[key_1][0];
                    this.p1 = p1;
                    this.s_sp = x;
                    s = x;
                    this.t_dup = y;
                }

                else  //(winningMovesSpoiler.ContainsKey(key_2))
                {
                    p1 = winningMovesSpoiler[key_2][0];
                    this.p1 = p1;
                    this.s_sp = y;
                    s = y;
                    selection = 1;
                    this.t_dup = x;
                }
            } else
            {
                //has no strategy: random choice x,y
                s = random.Next(2);
                // return p1 based on alpha(s)
                selection = 1;
                this.s_sp = y;
                this.t_dup = x;
                if (s == 0)
                {
                    this.s_sp = x;
                    this.t_dup = y;
                    selection = 0;
                }
                s = this.s_sp;
                try
                {
                    p1 = functor.ReturnDirectSuccesors(s, ts, states, alphabet);
                }
                catch
                {
                    //TODO: some generic procedure based on alpha(_)?
                    for (int i = 0; i < states; i++)
                    {
                        p1.Add(i);
                    }
                    P1 = p1;
                }
             
                this.p1 = p1;
            }

            return selection;
        }

        public bool Step1ByUser(int s, List<int> p1)
        {
            if (s - 1 == X)
            {
                S_sp = s - 1;
                T_dup = Y;
                P1 = p1;
                return true;
            }
            if (s - 1 == Y)
            {
                S_sp = s - 1;
                T_dup = X;
                P1 = p1;
                return true;
            }
            return false;
        }

        public bool Step2ByUser(List<int> p2)
        {

            //encapsulation of ReturnAlpha_X ??
            if (functor.CheckCondition2(P1, p2, S_sp, T_dup, states, ts, ts.Alphabet))
            {
                P2 = p2;
                return true;
            }
            else return false;
        }



        /// <summary>This method returns the correct state and predicate for the Defender
        /// in Step 2 of the game
        /// </summary>
        public void Step2_PCIsDefender(ref List<int> p2, int alphabet)
        {
            //has strategy: construct p2 based on p1
            // bisimulation is already computed
            if (BisimilarPairs[t_dup * states + s_sp] == 1)
            {
                for (int i = 0; i < P1.Count; i++)
                {
                    if (!p2.Contains(i)) p2.Add(P1[i]);
                    //add all other bisimilar states according to p1[i]
                    for (int j = 0; j < states; j++)
                    {
                        //add all other bisimilar states of s, but only once
                        if (!p2.Contains(j)) if (BisimilarPairs[j * states + P1[i]] == 1) p2.Add(j);
                    }
                }
                this.p2 = p2;
            }
            else
            {
                //Dup has no strategy
                //check if User has made a mistake: means s and p1 is not contained in the winning strategy List
                List<List<int>> strategies;
                bool mistake = true;
                try
                {
                    strategies = winningMovesSpoiler[new Tuple(s_sp, t_dup)];
                    foreach (var predicate in strategies)
                    {
                        //means the User does not take a valid strategy, hence could be wrong (eg. if p1 is greater)
                        if (P1 == predicate)
                        {
                            mistake = false;
                        }
                    }
                }
                catch
                {
                    //winning strategy is based on two partitions, which is not captured by our Algorithm, therefore negation is necessary!
                }
      
                if (mistake)
                {
                    //take all the bisimilar states s.t condition 2 is still satisfied
                    // In that case the User has made a mistake:
                    GetTheBisimilarIntersection(ref p1, ref p2, s_sp);//s_sp not as context
                    // check if really a mistake has happened:
                    // Note that also p1={} is captured here!
                    if (!functor.CheckCondition2(P1, p2, S_sp, T_dup, states, ts, ts.Alphabet))
                        mistake = false;
                }
                if (!mistake)
                {
                    // just needed, if any bug in the winning strategy computation
                    p2.Clear();
                    //if User did not make a mistake, no strategy available:
                    try
                    {
                        P2 = functor.ReturnDirectSuccesors(t_dup, ts, states, alphabet);
                    }         
                    catch
                    {
                        //TODO: some generic procedure based on alpha(_)?
                        for (int i=0;i<states; i++)
                        {
                            p2.Add(i);
                        }
                        P2 = p2;
                    }
                    foreach (int state in P2)
                    {
                        p2.Add(state);
                    }
                }
            }
            P2 = p2;
        }


        /// <summary>This method returns the correct state and predicate for the Defender
        /// in Step 3 of the game
        /// no check if a strategy exits, hence User could have made a mistake
        /// update of bool
        /// </summary>
        public int Step3_PCIsSpoiler(ref int selection)
        {
            List<int> step3;
            // PC is Spoiler: Goal find the state, which is non-bisimilar to all the other
            int nonBisimlarState;
            selection = 0;

            bool found = FindTheOneNonBisim(p1, p2, out nonBisimlarState);
            if (found) step3 = p1;
            if (!found)
            {
                found = FindTheOneNonBisim(p2, p1, out nonBisimlarState);
                if (found)
                {
                    selection = 1;
                    step3 = p2;
                }
            }
            // IF no one could be found: random choice, hence no strategy available
            if (!found)
            {
                int i = 0;
                i = random.Next(2);
                step3 = p1;
                if (i == 1)
                {
                    step3 = p2;
                    selection = 1;
                }
                try
                {
                    i = random.Next(step3.Count);
                    nonBisimlarState = step3[i];
                }
                catch
                {
                    // predicate is empty
                    i = -1;
                    nonBisimlarState = -1;
                    selection = -1;
                }

            }
       
            this.Selection = selection; //chosen predicate
            // update wrt predicate
            if (selection == 0)
            {
                this.x_prime = this.x; //x'
                this.x = nonBisimlarState;
            }
            if (selection == 1)
            {
                this.y_prime = this.y; //x'
                this.y = nonBisimlarState;
            }

            return nonBisimlarState;
        }


        public bool CheckUserMove3(int selection)
        {
            this.Selection = selection;
            if (selection == 0)
            {
                return p1.Contains(x);
            } else
            {
                return p2.Contains(y);
            }
        }

        /// <summary>This method returns the correct state for the Defender
        /// in Step 4 of the game
        /// no check if a strategy exits, hence User could make an mistake
        /// update of bool
        /// </summary>
        public int Step4_PCIsDefender()
        {
            List<int> p3 = p2;
            // PC is Defender: take any bisimilar state to x' in p2_prime
            List<int> AllBisimilarStates = new List<int>();
         
            if (selection == 1)
            {
                p3 = p1;
                GetTheBisimilarStates(y, ref AllBisimilarStates);
            }
            else
            {
                GetTheBisimilarStates(x, ref AllBisimilarStates);
            }

            int BisimilarState;
     
            IEnumerable<int> intersect = p3.Intersect(AllBisimilarStates);
            if (intersect.Count() > 0)
            {
                int i = random.Next(intersect.Count());
                BisimilarState = intersect.ElementAt(i);
            }
            else
            {
                // IF no one could be found: random choice, hence no strategy available
                if (p3.Count > 0)
                {
                    int i = random.Next(p3.Count);
                    BisimilarState = p3[i];
                }
                else return -1; //if empty predicate D has lost

            }

            if (selection == 0)
            {
                this.y_prime = this.y; //y'
                this.y = BisimilarState;
            }
            if (selection == 1)
            {
                this.x_prime = this.x; //x'
                this.x = BisimilarState;
            }

            return BisimilarState;
        }

        public bool CheckUserMove4()
        {
            this.Selection = selection;
            if (selection == 0)
            {
                return p2.Contains(y);
            }
            else
            {
                return p1.Contains(x);
            }
        }


        //Helping method defender is PC
        private void GetTheBisimilarIntersection(ref List<int> p1, ref List<int> p2, int s)
        {
            for (int i = 0; i < p1.Count; i++)
            {
                p2.Add(p1[i]);
                //add all other bisimilar states of s
                for (int j = 0; j < states; j++)
                {
                    //add all other bisimilar states of p1[i], but only once!
                    if (!p2.Contains(j)) if (BisimilarPairs[j * states + p1[i]] == 1) p2.Add(j);
                }
            }
        }




        //Helping Method PC is Spoiler
        private bool FindTheOneNonBisim(List<int> predicate_i, List<int> predicate_j, out int nonBisim)
        {
            bool found = false;
            nonBisim = 0;
            int i = 0;
            while (!found && i < states)
            {
                if (predicate_i.Contains(i))
                {
                    List<int> BisimilarStates = new List<int>();
                    GetTheBisimilarStates(i, ref BisimilarStates);
                    //Check for Intersection: IF empty, move for Spoiler found!
                    IEnumerable<int> intersect = BisimilarStates.Intersect(predicate_j);
                    if (intersect.Count() == 0)
                    {
                        nonBisim = i;
                        found = true;
                    }
                }

                i++;
            }
            return found;
        }

        //Get a List of bisimilar states
        private void GetTheBisimilarStates(int state, ref List<int> BisimilarStates)
        {
            for (int j = 0; j < states; j++)
            {
                //add all other bisimilar states of s
                if (BisimilarPairs[j * states + state] == 1) BisimilarStates.Add(j);
            }
        }



        public int[] computeBisimulation(ref Dictionary<Tuple, List<List<int>>> winningMovesSpoiler)
        {
            //initial situation R_0
            int computationStep = 0;


            //datastructure for winning-strategy

            int[] bisimilarPairs = new int[states * states];
            for (int i = 0; i < bisimilarPairs.Length; i++) bisimilarPairs[i] = 1;
            // R_i+1
            int[] bisimilarPairsUpdate = new int[states * states];
            for (int i = 0; i < bisimilarPairs.Length; i++) bisimilarPairsUpdate[i] = 1;
            bool updateshappened;
            List<List<int>> equivalences;
            do
            {
                updateshappened = false;
                if(computationStep != 0)Array.Copy(bisimilarPairsUpdate, bisimilarPairs, bisimilarPairs.Length);

                equivalences = computeEquivalenceClasses(bisimilarPairs);//datastructure to avoid double computations
                for (int i = 0; i < states; i++)
                {
                    for (int j = i + 1; j < states; j++)
                    {
                        
                        if (bisimilarPairs[states * j + i] == 1)
                        {
                            List<List<int>> moves_of_Spoiler_xy = new List<List<int>>(); ;
                            List<List<int>> moves_of_Spoiler_yx = new List<List<int>>(); ;
                            for (int k = 0; k < equivalences.Count; k++)// for all equivalence-classes
                            {
                                List<int> predicate = equivalences[k];
                 
                                if (!functor.CheckCondition2(predicate, predicate, i, j, states, ts, ts.Alphabet))
                                {                                    
                                    bisimilarPairsUpdate[states * j + i] = 0;
                                    bisimilarPairsUpdate[states * i + j] = 0;
                                    moves_of_Spoiler_xy.Add(predicate);
                                    updateshappened = true;
                                }
                                if (!functor.CheckCondition2(predicate, predicate, j, i, states, ts, ts.Alphabet))
                                {
                                    
                                    bisimilarPairsUpdate[states * j + i] = 0;
                                    bisimilarPairsUpdate[states * i + j] = 0;
                                    moves_of_Spoiler_yx.Add(predicate);
                                    updateshappened = true;
                                }
                                
                                try
                                {
                                    if (moves_of_Spoiler_xy.Count > 0 && k == equivalences.Count - 1) winningMovesSpoiler.Add(new Tuple(i, j), moves_of_Spoiler_xy);
                                    if (moves_of_Spoiler_yx.Count > 0 && k == equivalences.Count - 1) winningMovesSpoiler.Add(new Tuple(j, i), moves_of_Spoiler_yx);
                                }
                                catch
                                {
                                    // KEY conflict:
                                    // should not happen, if correct implemented!
                                }
                                
                            }// if pair is in R_i, check if still in R_i+1

                        }// done for xy, yx for all equivalence-classes


                    }// end for: one (x,y),(y,x)
                }// end for: All (x,y),(y,x)
                ++computationStep;
                //} while (!RiEqualsRiPlusOne(bisimilarPairs, bisimilarPairsUpdate));//end while
            } while (updateshappened);//end while to avoid a runtime + n^2 at each iteration ;)
            bisimilarPairsUpdate = null;
            return bisimilarPairs;
        }

        private bool RiEqualsRiPlusOne(int[] Ri, int[] RiPlusOne)
        {
            //return Array.Equals(Ri, RiPlusOne);
            
            bool ret = true;
            for (int i = 0; i < Ri.Length; i++)
            {
                if (Ri[i] != RiPlusOne[i]) return false;
            }

            return ret;
            
        }

        private List<List<int>> computeEquivalenceClasses(int[] bisimilarPairs)
        {

            List<List<int>> equivalences = new List<List<int>>();
            for (int i = 0; i < states; i++)
            {
                for (int j = i; j < states; j++)
                {
                    List<int> currentList;

                    if (bisimilarPairs[i * states + j] == 1)
                    {
                        if (alreadyContainedinClass(i, equivalences, out currentList))//return class containing already i
                        {
                            if (!currentList.Contains(j)) currentList.Add(j);// automatic updated in equivalences!
                        }
                        if (alreadyContainedinClass(j, equivalences, out currentList))
                        {
                            if (!currentList.Contains(i)) currentList.Add(i);// automatic updated in equivalences!
                        }
                        else
                        {
                            currentList = new List<int>();
                            currentList.Add(i);
                            if (i != j) currentList.Add(j);
                            equivalences.Add(currentList);
                        }
                    }
                }//

            }
            return equivalences;
        }

        private bool alreadyContainedinClass(int i, List<List<int>> equivalences, out List<int> currentList)
        {
            bool alreadyContained = false;
            currentList = new List<int>();
            foreach (var item in equivalences)
            {
                if (item.Contains(i))
                {
                    currentList = item;
                    alreadyContained = true;
                }
            }
            return alreadyContained;
        }

        private bool returnEquivalenceClass(int[] bisimilarPairs, int i, int j)
        {
            return true;
        }


        //bisimilartiy check of two states:
        public bool areBisimilar ()
        {
            if (bisimilarPairs[states * this.y + this.x] == 1) return true;
            else return false;
        }

        //Generation of distinguishing formulas
        public string GetDistinguishingFormula(int x, int y)
        {
            int sizeOfConjunction = Int32.MaxValue;
            string phi = "";
            List<int> p1;
            List<int> XMinus_p1 = new List<int>();
            //has strategy:
            // return always the first move, which was added
            Tuple key_1 = new Tuple(x, y);
            Tuple key_2 = new Tuple(y, x);
            if (BisimilarPairs[x * states + y] == 0)
            {
                if (winningMovesSpoiler.ContainsKey(key_1))
                {
                    p1 = winningMovesSpoiler[key_1][0];//only the first splitter predicate what was found

                    //Generalization of Cleavelands algorithm to lambda -> cone modality:
                    phi = "[" + "\u2191" + functor.Fpalpha_xToString(x, p1, this.ts) + "]"+" ";

                }

                else //if(winningMovesSpoiler.ContainsKey(key_2))
                {
                    // formula satisfied by y and not x
                    p1 = winningMovesSpoiler[key_2][0];
                    // add negation!
                    phi = "-[" + "\u2191" + functor.Fpalpha_xToString(y, p1, this.ts) + "]"+" ";
                }

                //get the rest of states
                for (int i = 0; i < states; i++)
                {
                    if (!p1.Contains(i))
                    {
                        XMinus_p1.Add(i);
                    }
                }
                //termination condition of recursion!
                if (XMinus_p1.Count == 0) return phi;

                //starting with disjunction of conjunctions
                string formulaBasedOnP1 = "";//the smallest one
                for (int i = 0; i < p1.Count; i++)
                {
                    Dictionary<int, string> teta = new Dictionary<int, string>();
                    for (int j = 0; j < XMinus_p1.Count; j++)
                    {
                        string phi_ij = GetDistinguishingFormula(p1[i], XMinus_p1[j]);


                        if (!teta.Values.Contains(phi_ij))
                        {
                            teta.Add(XMinus_p1[j], phi_ij);
                        }// end add the new formula_x'_y'
                    }//end X minus p1


                    //TODO Future: more testing: remove unnecessary formulas from the conjunction and check if size is smaller:
                    List<int> p1Prime;
                    List<int> toRemove = new List<int>();
              
                    foreach (int key in teta.Keys.ToArray())
                    {
                        bool formulaNeeded = false;
                        for (int j = 0; j < XMinus_p1.Count; j++)
                        {

                            key_1 = new Tuple(p1[i], key);
                            key_2 = new Tuple(key, p1[i]);
                            //if the next bool vriables are both  set to true, than the formula is needed
                            bool jnotSatTeta; //this distinguishes j 
                            bool jSatAllOtherTeta; //hence all other formulas do not the job

                            if (winningMovesSpoiler.ContainsKey(key_1))
                            {
                                p1Prime = winningMovesSpoiler[key_1][0];
                                //check if j  satisfies teta_key
                                jnotSatTeta = !functor.CheckCondition2(p1Prime, p1Prime, p1[i], XMinus_p1[j], this.states, this.ts, this.ts.Alphabet);
                                //check if it satisfies all other teta_keys
                                jSatAllOtherTeta = CheckSatAllOtherTeta(XMinus_p1[j], teta, key, p1[i]);
                                if (jnotSatTeta && jSatAllOtherTeta) formulaNeeded = true;
                            }

                            else //if(winningMovesSpoiler.ContainsKey(key_2))
                            {
                                // formula satisfied by y and not x hence add negation
                                p1Prime = winningMovesSpoiler[key_2][0];
                                //check if j is satisfies teta_key //changed to key!
                                jnotSatTeta = functor.CheckCondition2(p1Prime, p1Prime, key, XMinus_p1[j], this.states, this.ts, this.ts.Alphabet);
                                //check if it satisfies all other teta_keys
                                jSatAllOtherTeta = CheckSatAllOtherTeta(XMinus_p1[j], teta, key, p1[i]);
                                if (jnotSatTeta && jSatAllOtherTeta) formulaNeeded = true;
                            }

                            if (formulaNeeded)
                            {
                                // check can be interrupted since it is already clear that set is not empty and formula needed
                                j = XMinus_p1.Count;
                            }

                        }//check for each y' not in p1

                        // if formula not needed: remove formula!
                        if (!formulaNeeded)
                        {
                            toRemove.Add(key);
                        }
                    }//check for each formula
                    for (int k = 0; k < toRemove.Count; k++)
                    {
                        teta.Remove(toRemove[k]);
                    }
                    //Testing: if last conjunction smaller, than set to this conjunction
                 
                    if (teta.Count < sizeOfConjunction)
                    {
                        formulaBasedOnP1 = "";
                        sizeOfConjunction = teta.Count;
                        //set to this conjunction
                        int index = 0;
                        foreach (int key in teta.Keys.ToArray())
                        {
                            if (index == 0)
                            {
                                formulaBasedOnP1 = formulaBasedOnP1 + "(" + teta[key]+" ";
                            }
                            if (index > 0 && index < teta.Keys.Count)
                            {
                                formulaBasedOnP1 = formulaBasedOnP1 + "&" + teta[key]+" ";
                            }
                            index++;
                        }
                        formulaBasedOnP1 = formulaBasedOnP1 + ")"; // the conjunction according to p1[i]
                    }//if size samller        
                }//end p1
              return phi + formulaBasedOnP1;
            }//if not bisimilar
            else return "No formula exists since (" + x.ToString() + "," + y.ToString() + ") are bisimilar.";
        }

        private bool  CheckSatAllOtherTeta(int y_prime,Dictionary<int,string> tetas, int key, int fromp1)
        {
            bool AllSat=true;
            foreach (int k in tetas.Keys.ToArray())
            {
                if (k != key && AllSat)//check all the other tetas for y_prime
                {
                    Tuple key_1 = new Tuple(fromp1, k);
                    Tuple key_2 = new Tuple(k, fromp1);
                    if (winningMovesSpoiler.ContainsKey(key_1))
                    {                
                        List<int> p1Prime = winningMovesSpoiler[key_1][0];
                        //check if j is satisfies teta_key
                        AllSat = functor.CheckCondition2(p1Prime, p1Prime, fromp1, y_prime, this.states, this.ts, this.ts.Alphabet);
                    }
                    else //if(winningMovesSpoiler.ContainsKey(key_2))
                    {
                        // formula satisfied by y and not x hence add negation
                        List<int>  p1Prime = winningMovesSpoiler[key_2][0];
                        //check if j is satisfies teta_key, but add negation
                        AllSat = !functor.CheckCondition2(p1Prime, p1Prime, k, y_prime, this.states, this.ts, this.ts.Alphabet);
                   
                    }
                }
            }
            return AllSat;
        }

        //TODO: maybe not needed
        private List<int> computeEquivalenceClassOf(int[] bisimilarPairs, int x)
        {
            List<int> p=new List<int>();
            for (int i = 0; i < bisimilarPairs.Length; i++)
            {
                if (bisimilarPairs[x * i + bisimilarPairs.Length] == 1) p.Add(i);
            }
            return p;
        }

        //GUI Helper Method

        public string Fpalpha_xToString(int state, List<int> predicate)
        {
            return "Your selection has to be at least: "+ functor.Fpalpha_xToString(state, predicate, ts);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }

            disposed = true;
        }
    }
}
