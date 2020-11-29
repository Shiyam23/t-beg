using System.Collections.Generic;

namespace GraphModel {

    public class State {

        static public State[] states;
        public string name {get; set;}
        public bool isStartState {get; set;}
        public bool isFinalState {get; set;}

    }

    public class Link {
        
        static public Link[] links;
        public string name {get; set;}
        public State source {get; set;}
        public State target {get; set;}
        public string value {get; set;}
    }
}