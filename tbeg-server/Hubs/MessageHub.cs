using Microsoft.AspNetCore.SignalR;  
using System.Threading.Tasks;  
using System;

namespace Hubs  
{  
    public class MessageHub : Hub  
    {  
        public async Task sum(int a, int b)  {   
            
            Console.WriteLine($"Addiere {a} und {b}");
            await Clients.Caller.SendAsync("Result", a + b);  
        }  
    }  
}  