using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TBeg;
using Hubs;
using Microsoft.AspNetCore.SignalR;

namespace tbeg_server
{
    public class Program {
        public static void Main(string[] args) {

            var host = CreateHostBuilder(args).Build();
            var hubContext = host.Services.GetService(typeof(IHubContext<MessageHub>));
            TBeg.TBeg.setHubContext((IHubContext<MessageHub>)hubContext);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        public static void UpdateFunctorList(ref Dictionary<String, IModel> models, string filename) {

            string[] config_Functor = System.IO.File.ReadAllLines(filename);
            foreach (string line in config_Functor)
            {
                if (line.Length != 0)
                {
                    if (!models.Keys.Contains(line))
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
                                //Object obj_data = Activator.CreateInstance(obj.GetType().GetField("elements_of_FX").GetType(), null);
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
        }
    }
}
