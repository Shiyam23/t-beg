using Microsoft.AspNetCore.SignalR;  
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;  
using System.Collections.Generic;
using System;
using TBeg;
using tbeg_server;
using GraphModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hubs  
{  
    public class MessageHub : Hub  
    {  

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected...");
            Context.Items.Add("tbeg", new TBeg.TBeg(Context.ConnectionId));
            Dictionary<String, IModel> models = new Dictionary<String, IModel>();
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string dir = System.IO.Path.GetDirectoryName(appPath);
            UpdateFunctorList(ref models, dir+"\\config_Functor.txt");
            TBeg.Controller cnt = new TBeg.Controller((TBeg.TBeg)Context.Items["tbeg"], models);
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Context.Items["tbeg"] = null;
            Console.WriteLine($"Client disconnected...");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task getFunctors()  {   
            
            Console.WriteLine($"Requesting functor list...");
            await Clients.Caller.SendAsync("Result", ((TBeg.TBeg) Context.Items["tbeg"]).getFunctors("\\config_Functor.txt"));  
        } 

        public Task getValidator(string functor) {

            Console.WriteLine($"Requesting validator for {functor}...");
            var gui = ((TBeg.TBeg) Context.Items["tbeg"]);
            gui.getValidator(functor);
            return Task.CompletedTask;
        }

        public Task graph(State[] states, Link[] links, string[] alphabet, string functor ) {
            
            ((TBeg.TBeg)Context.Items["tbeg"]).initMatrix(new Graph(states, links, alphabet), functor);
            return Task.CompletedTask;
        }


        public static void UpdateFunctorList(ref Dictionary<String, IModel> models, string filename)
        {

            string[] config_Functor = System.IO.File.ReadAllLines(filename);
            foreach (string line in config_Functor)
            {
                if (line.Length != 0)
                {
                    if (!models.ContainsKey(line))
                    {
                        string functor = line;
                        if (true)
                        {
                            try
                            {
                                Type generic = typeof(Model<>);
                                Type[] typeArgs = { Type.GetType("TBeg." + functor) };

                                Type constructed = generic.MakeGenericType(typeArgs);
                                Object obj = Activator.CreateInstance(constructed, null);
                                models.Add(functor, (IModel)obj);
                            }
                            catch
                            {
                                // no remove needed since should be available in Functor-Selection box in Input transition panel:
                                //System.Windows.Forms.MessageBox.Show("Please remove "+functor+" from functor_config file.");

                            }

                        }

                    }

                }

            }
        }//end configfile method
    }  
}  