///<summary>
///Author: Hussein Bakri
///Program Name: libOMVMobilityModels 1.0
///This program uses libopenmetaverse Release 0.9.3 ( a C# client side library for accessing OpenSim and Second Life).
///It is the result of days of work.  lipopenmetaverse is not well documented as you will probably figure out so this app will be a starting point for you to expand on, change and enhance
///It is not perfect but do the job.
///This program is licensed under GNU GPL v3 License - you are free to distribute, change, enhance and include any of the code of this application in your tools.
///I only expect adequate attribution of this work. The attribution should include the title of the program, the author and the site or the document where the program is taken from.
///
/// In summary, the application allows you to load  Non-Player Characters (NPC bots) as many as you want.
/// It reads their accounts information from a CSV file. Please check my Github repository at the following URL:
///I have a Linux bash shell script there that allows you to create as many dump NPC accunts for OpenSim as you want and save their info into a CSV file which could be
///fetched easly in this app without you to worry about anything. I will write a similar script for MS Windows users either in PowerShell or Batch. I will see how it can be done.
///This app has 6 mobility models only which you can change the duration [by default 3 minutes (180000 milliseconds)] - Yaw is the change is the avatar gaze direction
///Available Mobility models: [1] StayingStillWithoutYawFor3minutes , [2] StayingStillWithYawFor3minutes , [3] RandomWalkFor3minutes
///[4] RandomRunFor3minutes , [5] RandomFlyFor3minutes , [6] RandomTeleportFor3minutes
///Enjoy!
///</summary>
///<remarks>
///Built in Visual Studio Comunity 2015. I did not tried it yet on MonoDevelop but probably it will work. 
///Please make sure you add the following references before you can run this program : [In Visual Studio Comunity 2015, right click on References and Choose Add References]
///For OpenMetaverse libraries to work, kindly add the following references as dlls files (add them in VS Comunity 2015 references):
///OpenMetaverse.dll, OpenMetaverseTypes.dll, OpenMetaverse.StructuredData.dll, OpenMetaverse.Utilities.dll, OpenMetaverse.Rendering.Linden.dll
///Don't forget to copy the following dlls into the bin folder of this VS project: openjpeg-dotnet.dll and openjpeg-dotnet-x86_64.dll::required for the avatars appearance otherwise they will appear as clouds
///All above dlls are obtained from the libopenmetaverse source folder "only after" libopenmetaverse being build adequately depending on your target OS
///Add also System.Windows.Forms ( in VS 2015 references) -- I have used the FolderBrowserDialog() to make the app not dependant on OS for opening the CSV file
///TODO: Solve problem of System.Windows.Forms on Mono Linux
/// One last important thing: (without this step - the NPC avatars will apear naked after being baked see https://github.com/openmetaversefoundation/libopenmetaverse/releases
///It seems in Release 0.8.1, the naked avatar bug is supposed to be solved but it is appearing again in release 0.9.3 (July 2016) used in this application
///Solution:
///Go to the libopenmetaverse source folder and after you have build it adequately depending on the OS, you should go the bin folder there and copy
///openmetaverse_data folder which contains the clothes of avatars among other things (.tga files needed like glove_lenth_alpha.tga and shirt_collar_alpha.tga etc..)
/// Paste this folder in the bin directory of "this" project (whether on VS 2015 or MonoDevelop), merge and copy and replace existing files, becuase you might already see
/// a folder of the same name in this project.
/// /// Enjoy!
/// 
/// I will create later the same application with IronPython for Python lovers out there (including me)! allowing you to create Python NPC bots!
///</remarks>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenMetaverse;
using System.Threading;
using System.IO;

namespace libOMVMobilityModels
{
    class NPCForStats                   //A bot that just collect statistics of the simulator and client and save them into a CSV file
    {                                   //NOT COMPLETE
        private GridClient Client;
        private int StatisticsDuration = 180000;           //default is 3 minutes
        private Random rnd = new Random();
        private String startRegionName = "Cathedral 1";


        public NPCForStats(string startRegion, string LoginServer, string username, string lastname, string password, int StatsDuration)
        {
            Client = new GridClient();
            startRegionName = startRegion;
            string startLocation = NetworkManager.StartLocation(startRegion, rnd.Next(1, 700), rnd.Next(1, 500), 30);
            Client.Settings.LOGIN_SERVER = LoginServer;
            bool loginFlag = Client.Network.Login(username, lastname, password, "Bot", startLocation, "1.0");
            StatisticsDuration = StatsDuration;
            
            Console.WriteLine("*****************************************************************");
            Console.WriteLine();

            //subscribe to the events needed
            if (loginFlag)
            {
                //Yay we made it! let us print out the message of the day
                Console.WriteLine("You have successfully logged into OpenSim\nThe message of the day is: {0}", Client.Network.LoginMessage);

            }
            else
            {
                //tell the use why the login failed
                Console.WriteLine("You were unable to log in into OpenSim\nCause is: {0}", Client.Network.LoginMessage);
            }

            ////Waiting 20 second for the bot
            //Thread.Sleep(20000);

            Client.Network.EventQueueRunning += Network_EventQueueRunning; //This is the handler where the logic of the NPC goes under 
            Client.Network.SimChanged += Network_SimChanged;
            Client.Network.SimConnected += Network_SimConnected;
            Client.Network.LoginProgress += Network_LoginProgress;
            //registering in the event (the event will print
            //Client.Grid.GridLayer += Grid_GridLayer;
        }

        

        private void Network_EventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
            Client.Self.Chat("Network_EventQueueRunning....", 0, ChatType.Normal);
            Console.WriteLine("Network_EventQueueRunning....");
            Console.WriteLine("Simulator Name: " + e.Simulator.Name);
            Console.WriteLine("Nb of Parcels in the Simulator: " + e.Simulator.Parcels.Count);
            Client.Self.Chat("NPC Activated", 0, ChatType.Normal);
            
            //Waiting 20 second for the bot
            Client.Self.Chat("Waiting 30 second before taking statistics from the simulator...", 0, ChatType.Normal);
            Console.WriteLine("Waiting 30 second before taking statistics from the simulator...");
            Thread.Sleep(35000);
            Console.WriteLine("***********************************************************************************");
            Console.WriteLine("Available Simulators .......................");
            Console.WriteLine("Available Simulators: " + Client.Network.Simulators.Count); //giving only 1 sim for the Cathedral world ?

            StringBuilder output = new StringBuilder();
            for (int i = 0; i < Client.Network.Simulators.Count; i++)
            {
                Simulator sim = Client.Network.Simulators[i];

                output.AppendLine(String.Format(
                    "[{0}] Dilation: {1} InBPS: {2} OutBPS: {3} ResentOut: {4}  ResentIn: {5}",
                    sim.ToString(), sim.Stats.Dilation, sim.Stats.IncomingBPS, sim.Stats.OutgoingBPS,
                    sim.Stats.ResentPackets, sim.Stats.ReceivedResends));
            }
            Console.WriteLine(output.ToString());

            Console.WriteLine("***********************************************************************************");
            Console.WriteLine("[BEGIN] TESTING AREA : REGION INFO....");
            Console.WriteLine("Client.Network.CurrentSim.ToString(): {0}", Client.Network.CurrentSim.ToString());
            Console.WriteLine("Client.Network.CurrentSim.ID.ToString(): {0}", Client.Network.CurrentSim.ID.ToString());
            Console.WriteLine("Client.Network.CurrentSim.Handle: {0}", Client.Network.CurrentSim.Handle.ToString());
            Console.WriteLine("Client.Network.CurrentSim.Access.ToString():" + Client.Network.CurrentSim.Access.ToString());
            Console.WriteLine("Client.Network.CurrentSim.Flags.ToString():" + Client.Network.CurrentSim.Flags.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainBase0.ToString():" + Client.Network.CurrentSim.TerrainBase0.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainBase1.ToString():" + Client.Network.CurrentSim.TerrainBase1.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainBase2.ToString():" + Client.Network.CurrentSim.TerrainBase2.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainBase3.ToString():" + Client.Network.CurrentSim.TerrainBase3.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainDetail0.ToString():" + Client.Network.CurrentSim.TerrainDetail0.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainDetail1.ToString():" + Client.Network.CurrentSim.TerrainDetail1.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainDetail2.ToString():" + Client.Network.CurrentSim.TerrainDetail2.ToString());
            Console.WriteLine("Client.Network.CurrentSim.TerrainDetail3.ToString():" + Client.Network.CurrentSim.TerrainDetail3.ToString());
            Console.WriteLine("Client.Network.CurrentSim.WaterHeight.ToString():" + Client.Network.CurrentSim.WaterHeight.ToString());
            Console.WriteLine("Client.Network.CurrentSim.ColoLocation:" + Client.Network.CurrentSim.ColoLocation);
            Console.WriteLine("Client.Network.CurrentSim.CPURatio.ToString():" + Client.Network.CurrentSim.CPURatio.ToString());
            Console.WriteLine("Client.Network.CurrentSim.CPUClass.ToString():" + Client.Network.CurrentSim.CPUClass.ToString());
            Console.WriteLine("Region SKU / Type:" + Client.Network.CurrentSim.ProductSku + " " + Client.Network.CurrentSim.ProductName);
            Console.WriteLine("[END] TESTING AREA....");
            Console.WriteLine("***********************************************************************************");
            
            //Console.WriteLine("Searching for a simulator and returning information about it....");
            //Console.WriteLine("***********************************************************************************");
            //string simName = string.Empty;
            //simName = Client.Network.CurrentSim.ToString();
            //GridRegion region;
            //if (Client.Grid.GetGridRegion(simName, GridLayerType.Objects, out region))
            //    Console.WriteLine(String.Format("{0}: handle={1} Region X,Y ({2},{3})", region.Name, region.RegionHandle, region.X, region.Y));
            //else
            //    Console.WriteLine("Lookup of " + simName + " failed");

            Thread.Sleep(20000);
            Console.WriteLine("***********************************************************************************");
            //Downloads all of the layer chunks for the grid object map - gridlayer
            //Console.WriteLine("Requesting Grid Map Layer...");
            //Client.Grid.RequestMapLayer(GridLayerType.Objects);
            
            //Downloads all visible information about the grid map
            //Console.WriteLine("Downloading all visible information about the grid map...");
            //Client.Grid.RequestMainlandSims(GridLayerType.Objects);


            Console.WriteLine(" Waitin 20 seconds before collecting stats the stat....");
 
            Thread.Sleep(20000);
            SimulatorStatistics();
            Console.WriteLine(" Waitin 20 seconds before loging stats bot out.....");

            Thread.Sleep(20000);

            Console.WriteLine("Logging out Stats bot...");
            Client.Network.Logout();

        }

        //private void Grid_GridLayer(object sender, GridLayerEventArgs e)
        //{
        //    Console.WriteLine(" I am  now Grid_GridLayer event handler ****....");
        //    Client.Self.Chat("I am  now Grid_GridLayer event handler....", 0, ChatType.Normal);
        //    Console.WriteLine("***********************************************************************************");
        //    Console.WriteLine("Layer({0}) Bottom: {1} Left: {2} Top: {3} Right: {4}", e.Layer.ImageID.ToString(), e.Layer.Bottom, e.Layer.Left, e.Layer.Top, e.Layer.Right);
        //    Console.WriteLine("***********************************************************************************");
        //    Console.WriteLine(" I will sleep 90 seconds....");
        //    Client.Self.Chat("I will sleep 90 seconds....", 0, ChatType.Normal);
        //    Thread.Sleep(90000);

        //    Console.WriteLine("Layer({0}) Bottom: {1} Left: {2} Top: {3} Right: {4}",
        //           e.Layer.ImageID.ToString(), e.Layer.Bottom, e.Layer.Left, e.Layer.Top, e.Layer.Right);
        //    Console.WriteLine("***********************************************************************************");
        //}

        private void Network_SimChanged(object sender, SimChangedEventArgs e)
        {
            Client.Self.Chat("Network_SimChanged Event", 0, ChatType.Normal);
        }

        private void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
            Client.Self.Chat("Network_SimConnected Event", 0, ChatType.Normal);
            Console.WriteLine("We are now in Network_SimConnected Event................");
        }

        private void Network_LoginProgress(object sender, LoginProgressEventArgs e)
        {
            Client.Self.Chat("LoginProgress Event", 0, ChatType.Normal);
            Console.WriteLine("We are now in LoginProgress Event................");
        }


        void SimulatorStatistics()
        {
            //This function print the statistics and save them into a CSV file called Stats.csv
            Console.WriteLine("Simulator statistics (comes from OpenSim server) shown below....");
            Console.WriteLine();
            int time = 0;

            //before your loop
            StringBuilder csv = new StringBuilder();
            string newLine = "";
            //in your loop
            //var first = reader[0].ToString();
            //var second = image.ToString();
            //var newLine = string.Format("{0},{1}{2}", first, second, Environment.NewLine);
            //csv.Append(newLine);

            //after your loop
            //File.WriteAllText(filePath, csv.ToString());
            int linecount = 0;
            //Uncomment the following 4 lines for writing the header of values into the CSV file
            Console.WriteLine("Writing the headers into the CSV file...");
            string headers = "InboxCount,FPS,PhysicsFPS,AgentUpdates,Objects,ActiveScripts,Agents,AgentTime,ScriptedObjects,FrameTime,NetTime,ImageTime,PhysicsTime,ChildAgents,ConnectTime,Dilation,IncomingBPS,INPPS,LastLag,LastPingID,LastPingSent,LSLIPS,MissedPings,OtherTime,OutgoingBPS,OUTPPS,PendingDownloads,PendingLocalUploads,PendingUploads,ReceivedPongs,ReceivedResends,RecvBytes,RecvPackets,ResentPackets,ResidentSize,ScriptTime,SentBytes,SentPackets,SentPings,UnackedBytes,VirtualSize";
            newLine = string.Format("{0}{1}", headers, Environment.NewLine);
            File.AppendAllText("Stats.csv", newLine.ToString());

            newLine = "";

            while (time <= StatisticsDuration)
            {
                newLine = "";
                Console.WriteLine("***********************************************************************************");
                Console.WriteLine("Packets in the queue: " + Client.Network.InboxCount);
                newLine +=  string.Format("{0},", Client.Network.InboxCount.ToString());
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.FPS: {0}", Client.Network.CurrentSim.Stats.FPS);
                newLine = newLine + string.Format("{0},", Client.Network.CurrentSim.Stats.FPS);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.PhysicsFPS: {0}", Client.Network.CurrentSim.Stats.PhysicsFPS);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.PhysicsFPS);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.AgentUpdates: {0}", Client.Network.CurrentSim.Stats.AgentUpdates);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.AgentUpdates);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.Objects: {0}", Client.Network.CurrentSim.Stats.Objects);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.Objects);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ActiveScripts: {0}", Client.Network.CurrentSim.Stats.ActiveScripts);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ActiveScripts); //6
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.Agents: {0}", Client.Network.CurrentSim.Stats.Agents);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.Agents);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.AgentTime: {0}", Client.Network.CurrentSim.Stats.AgentTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.AgentTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ScriptedObjects: {0}", Client.Network.CurrentSim.Stats.ScriptedObjects);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ScriptedObjects);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.FrameTime: {0}", Client.Network.CurrentSim.Stats.FrameTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.FrameTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.NetTime: {0}", Client.Network.CurrentSim.Stats.NetTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.NetTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ImageTime: {0}", Client.Network.CurrentSim.Stats.ImageTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ImageTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.PhysicsTime: {0}", Client.Network.CurrentSim.Stats.PhysicsTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.PhysicsTime);
                
                Console.WriteLine("Client.Network.CurrentSim.Stats.ChildAgents: {0}", Client.Network.CurrentSim.Stats.ChildAgents);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ChildAgents);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ConnectTime: {0}", Client.Network.CurrentSim.Stats.ConnectTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ConnectTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.Dilation (Shows time dilation for current sim): {0}", Client.Network.CurrentSim.Stats.Dilation);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.Dilation); //16
              
                Console.WriteLine("Client.Network.CurrentSim.Stats.IncomingBPS: {0}", Client.Network.CurrentSim.Stats.IncomingBPS);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.IncomingBPS);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.INPPS: {0}", Client.Network.CurrentSim.Stats.INPPS);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.INPPS);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.LastLag: {0}", Client.Network.CurrentSim.Stats.LastLag);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.LastLag);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.LastPingID: {0}", Client.Network.CurrentSim.Stats.LastPingID);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.LastPingID);
               
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.LastPingSent: {0}", Client.Network.CurrentSim.Stats.LastPingSent);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.LastPingSent);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.LSLIPS: {0}", Client.Network.CurrentSim.Stats.LSLIPS);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.LSLIPS);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.MissedPings: {0}", Client.Network.CurrentSim.Stats.MissedPings);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.MissedPings);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.OtherTime: {0}", Client.Network.CurrentSim.Stats.OtherTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.OtherTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.OutgoingBPS: {0}", Client.Network.CurrentSim.Stats.OutgoingBPS);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.OutgoingBPS);  //25
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.OUTPPS: {0}", Client.Network.CurrentSim.Stats.OUTPPS);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.OUTPPS);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.PendingDownloads: {0}", Client.Network.CurrentSim.Stats.PendingDownloads);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.PendingDownloads);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.PendingLocalUploads: {0}", Client.Network.CurrentSim.Stats.PendingLocalUploads);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.PendingLocalUploads);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.PendingUploads: {0}", Client.Network.CurrentSim.Stats.PendingUploads);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.PendingUploads);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ReceivedPongs: {0}", Client.Network.CurrentSim.Stats.ReceivedPongs);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ReceivedPongs);    //30
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ReceivedResends: {0}", Client.Network.CurrentSim.Stats.ReceivedResends);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ReceivedResends);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.RecvBytes: {0}", Client.Network.CurrentSim.Stats.RecvBytes);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.RecvBytes);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.RecvPackets: {0}", Client.Network.CurrentSim.Stats.RecvPackets);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.RecvPackets);
                
                Console.WriteLine("Client.Network.CurrentSim.Stats.ResentPackets: {0}", Client.Network.CurrentSim.Stats.ResentPackets);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ResentPackets);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ResidentSize: {0}", Client.Network.CurrentSim.Stats.ResidentSize);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ResidentSize);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.ScriptTime: {0}", Client.Network.CurrentSim.Stats.ScriptTime);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.ScriptTime);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.SentBytes: {0}", Client.Network.CurrentSim.Stats.SentBytes);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.SentBytes);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.SentPackets: {0}", Client.Network.CurrentSim.Stats.SentPackets);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.SentPackets);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.SentPings: {0}", Client.Network.CurrentSim.Stats.SentPings);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.SentPings);
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.UnackedBytes: {0}", Client.Network.CurrentSim.Stats.UnackedBytes);
                newLine += string.Format("{0},", Client.Network.CurrentSim.Stats.UnackedBytes); //40
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("Client.Network.CurrentSim.Stats.VirtualSize: {0}", Client.Network.CurrentSim.Stats.VirtualSize);
                newLine += string.Format("{0}{1}", Client.Network.CurrentSim.Stats.VirtualSize, Environment.NewLine); //41
                
                Console.WriteLine("[CHECK] newline so far: " + newLine);
                Console.WriteLine("***********************************************************************************");
                Console.WriteLine("Writing one line of stats to the CSV file now....");
               
                File.AppendAllText("Stats.csv", newLine.ToString());
                //var newLine = string.Format("{0},{1}{2}", first, second, Environment.NewLine);
                Thread.Sleep(5000);
                time += 5000;
                linecount++;
            }
            Console.WriteLine("Nb of lines written to the CSV file is: " + linecount);
            Client.Self.Chat("Finished taking  Simulator statistics ...", 0, ChatType.Normal);
            Client.Self.Chat("Moving to the client side statistics ...", 0, ChatType.Normal);
         }


    }
}
