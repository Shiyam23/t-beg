using Microsoft.AspNetCore.SignalR;  
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;  
using System.Collections.Generic;
using System;
using TBeg;
using tbeg_server;

namespace Hubs  
{  
    public class MessageHub : Hub  
    {  
        private ITBeg _tbeg;
        public MessageHub(ITBeg tbeg) {
            _tbeg = tbeg;
        }

        public override Task OnConnectedAsync()
        {
            
            return base.OnConnectedAsync();
        }
        public async Task getFunctors()  {   
            
            Console.WriteLine($"Requesting functor list...");
            //TODO: only works if txt file is in dev folder (and not in bin folder)
            await Clients.Caller.SendAsync("Result", _tbeg.getFunctors("config_Functor.txt"));  
        } 

    }  
}  