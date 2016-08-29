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
///
/// One last important thing: (without this step - the NPC avatars will apear naked after being baked see https://github.com/openmetaversefoundation/libopenmetaverse/releases
///It seems in Release 0.8.1, the naked avatar bug is supposed to be solved but it is appearing again in release 0.9.3 (July 2016) used in this application
///Solution:
///Go to the libopenmetaverse source folder and after you have build it adequately depending on the OS, you should go the bin folder there and copy
///openmetaverse_data folder which contains the clothes of avatars among other things (.tga files needed like glove_lenth_alpha.tga and shirt_collar_alpha.tga etc..)
/// Paste this folder in the bin directory of "this" project (whether on VS 2015 or MonoDevelop), merge and copy and replace existing files, becuase you might already see
/// a folder of the same name in this project.
/// Enjoy!
///</remarks>


//Problems to solve
//----------------
//Make the mobility model span over all regions not only one by getting dynamically the size of the world
//try the application with all worlds (Timespan, etc...)


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenMetaverse;
using System.Threading;

namespace libOMVMobilityModels
{
    public class Bot
    {
        private static int MobilityDelay = 180000; //default is 3 minutes
        private GridClient Client;
        private Random rnd = new Random();
        private static string MobilityModel;
        private int NbOfParcels;
        

       public Bot(string startLocat, string LoginServer, string username, string lastname, string password, string Mobility, int MobilityModelDuration)
        {
            MobilityDelay = MobilityModelDuration;
            Client = new GridClient();
            string startLocation = NetworkManager.StartLocation(startLocat, rnd.Next(1, 700), rnd.Next(1, 500), 30);
            Client.Settings.LOGIN_SERVER = LoginServer;
            bool loginFlag = Client.Network.Login(username, lastname, password, "Bot", startLocation, "1.0");
            //Console.WriteLine("My UDP circuit code is: " + Client.Network.CircuitCode); 
            //Console.WriteLine("Client.Network.Connected: {0}", Client.Network.Connected);
            //Console.WriteLine("The simulator that the log-in avatar is currently occupying: {0}", Client.Network.CurrentSim);
            //Console.WriteLine("Client.Network.CurrentSim.Name: {0}", Client.Network.CurrentSim.Name);
            //Console.WriteLine("Client.Network.CurrentSim.RegionID: {0}", Client.Network.CurrentSim.RegionID);
            //Console.WriteLine("Client.Network.CurrentSim.SimOwner: {0}", Client.Network.CurrentSim.SimOwner);
            //Console.WriteLine("Client.Network.CurrentSim.SimVersion: {0}", Client.Network.CurrentSim.SimVersion);
            //Console.WriteLine("Some Statistics HOUHOU!!!");
            //Console.WriteLine("Client.Network.CurrentSim.Stats.FPS: {0}", Client.Network.CurrentSim.Stats.FPS);
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

            
            //registering events
            Client.Network.EventQueueRunning += Network_EventQueueRunning; //This is the handler where the logic of the NPC goes under
            Client.Network.SimChanged += Network_SimChanged;
            Client.Network.SimConnected += Network_SimConnected;
            Client.Self.ChatFromSimulator += Self_ChatFromSimulator; //This handler handle the code if someone chatted from Simulator
            Client.Network.LoginProgress += Network_LoginProgress;
            Client.Directory.DirLandReply += Directory_DirLandReply; // to count the nb of parcels and get their area
            

            MobilityModel = Mobility;

           
        }

        

        private void Network_EventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
            Client.Self.Chat("Check Land status....", 0, ChatType.Normal);
            Console.WriteLine("Check Land status....");
            Client.Directory.StartLandSearch(DirectoryManager.SearchTypeFlags.Any);
            Console.WriteLine("Simulator Name: " + e.Simulator.Name);
            Console.WriteLine("Nb of Parcels in the Simulator: " + e.Simulator.Parcels.Count);
            Client.Self.Chat("NPC Activated", 0, ChatType.Normal);
            if (e.Simulator == Client.Network.CurrentSim)
            {

                Client.Appearance.SetPreviousAppearance(true);

            }

            //Waiting 20 second for the bot
            Client.Self.Chat("Waiting 30 second before activating the mobility model...", 0, ChatType.Normal);
            Console.WriteLine("Waiting 30 seconds........");
            Thread.Sleep(30000);

            switch (MobilityModel)
            {
                case "StayingStillWithoutYawFor3minutes":
                    {
                        Console.WriteLine("Activating the StayingStillWithoutYawFor3minutes Mobility model");
                        StayingStillWithoutYawFor3minutes(MobilityDelay);
                        break;
                    }
                case "StayingStillWithYawFor3minutes":
                    {
                        Console.WriteLine("Activating the StayingStillWithYawFor3minutes Mobility model");
                        StayingStillWithYawFor3minutes(MobilityDelay);
                        break;
                    }
                case "RandomWalkFor3minutes":
                    {
                        Console.WriteLine("Activating the RandomWalkFor3minutes Mobility model");
                        RandomWalkFor3minutes(MobilityDelay);
                        break;
                    }
                case "RandomRunFor3minutes":
                    {
                        Console.WriteLine("Activating the RandomRunFor3minutes Mobility model");
                        RandomRunFor3minutes(MobilityDelay);
                        break;
                    }
                case "RandomFlyFor3minutes":
                    {
                        Console.WriteLine("Activating the RandomFlyFor3minutes Mobility model");
                        RandomFlyFor3minutes(MobilityDelay);
                        break;
                    }
                case "RandomTeleportFor3minutes":
                    {
                        Console.WriteLine("Activating the RandomTeleportFor3minutes Mobility model");
                        RandomTeleportFor3minutes(MobilityDelay);
                        break;
                    }
                default:
                    Console.WriteLine("There are no available Mobility models that match your choice...");
                    Console.WriteLine("Available Mobility Models are: ** StayingStillWithoutYawFor3minutes ** , ** StayingStillWithYawFor3minutes ** , ** RandomWalkFor3minutes ** , ** RandomRunFor3minutes ** , ** RandomFlyFor3minutes ** , ** RandomTeleportFor3minutes ** ");
                    break;
            }
            

            Thread.Sleep(2000);
            Console.WriteLine("Logging out the current bot...");
            Client.Network.Logout();

        }

        private void Directory_DirLandReply(object sender, DirLandReplyEventArgs e)
        {
            Client.Self.Chat("We are in Directory_DirLandReply Event", 0, ChatType.Normal);
            Console.WriteLine("We are now in Directory_DirLandReply Event");
            NbOfParcels = e.DirParcels.Count;
            Console.WriteLine("The number of parcels in this world is: " + NbOfParcels);
            foreach (DirectoryManager.DirectoryParcel parcel in e.DirParcels)
            {
                Console.WriteLine("Name: {0}\nPrice: {1}\nArea: {2}\n", parcel.Name, parcel.SalePrice.ToString(" '$L'#, #.00"), parcel.ActualArea.ToString("#,#"));
            }

         }

        private void Network_LoginProgress(object sender, LoginProgressEventArgs e)
        {
            Client.Self.Chat("LoginProgress Event", 0, ChatType.Normal);
            Console.WriteLine("We are now in LoginProgress Event................");
            //Client.Directory.StartLandSearch(DirectoryManager.SearchTypeFlags.Mainland);
        }

        private void Self_ChatFromSimulator(object sender, ChatEventArgs e)
        {
            //process the chat info (it is contained in "e") and the sender info is in sender
           
            if (e.Message == "How are you?")
            {
                Client.Self.Chat("You " + e.FromName + " is talking to me and I will answer ", 0, ChatType.Normal);
                Client.Self.Chat("I am fine Dude!", 0, ChatType.Normal);
                Client.Self.Chat("Your UUID is: " + e.SourceID, 0, ChatType.Normal);
            }
            else if (e.Message == "Bye")
            {
                Client.Self.Chat("Bye " + e.FromName + " !", 0, ChatType.Normal);
            }
        }

        private void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
            Client.Self.Chat("Network_SimConnected Event", 0, ChatType.Normal);
            Console.WriteLine("We are now in Network_SimConnected Event................");
            //Client.Directory.StartLandSearch(DirectoryManager.SearchTypeFlags.Mainland);
        }

        private void Network_SimChanged(object sender, SimChangedEventArgs e)
        {
            Client.Self.Chat("Network_SimChanged Event", 0, ChatType.Normal);
            if (e.PreviousSimulator != null)
            {
                Client.Appearance.SetPreviousAppearance(false);
            }
        }

        public void StayingStillWithoutYawFor3minutes(int delay)
        {
            //Avatar mobility model for Standing an avatar still in its spawn location for 3 minutes with yaw (no change in the lookat direction)
            //Yaw is the change in the look at direction which is also known as the gaze direction of the avatar
            int time = 0;
            Random rnd = new Random();
            Console.WriteLine("*** Standing still without Yaw *** ");
            Client.Self.Chat("*** Standing still without Yaw *** ", 0, ChatType.Normal);
            while (time <= delay)
            {
                OpenMetaverse.Vector3d mycurrentLookAt = Client.Self.LookAt;
                //Client.Self.Chat("*** My Current lookat vector: *** " + mycurrentLookAt, 0, ChatType.Normal);
                //just passing time but you can also use Client.Self.Stand() -> this send packets to server
                //better do it the way it is done here
                Thread.Sleep(1000);
                time += 1000;
            }
        }


        public void StayingStillWithYawFor3minutes(int delay)
        {
            //Avatar mobility model for Standing an avatar still in its spawn location for 3 minutes with yaw 
            //Yaw is the change in the look at direction which is also known as the gaze direction of the avatar
            int time = 0;
            Random rnd = new Random();
            Console.WriteLine("*** Standing still WITH Yaw *** ");
            Client.Self.Chat("*** Standing still WITH Yaw *** ", 0, ChatType.Normal);
            while (time <= delay)
            {
                OpenMetaverse.Vector3 target = new Vector3(rnd.Next(1, 700), rnd.Next(1, 500), rnd.Next(1, 254));
                Client.Self.Movement.TurnToward(target);
                Thread.Sleep(1000);
                time += 1000;
            }
        }

        public void RandomWalkFor3minutes(int delay)
        {
            //Avatar mobility model for walking an avatar for 3 minutes in random directions
            Random rnd = new Random();
            int time = 0;
            Client.Self.Movement.AlwaysRun = false;
            Client.Self.Movement.Fly = false;

            while (time <= delay)
            {
                //Console.WriteLine("*** calling RandomWalk() *** ");
                //Client.Self.Chat("*** calling RandomWalk() *** ", 0, ChatType.Normal);
                Client.Self.Movement.Fly = false;
                //Client.Self.AutoPilotLocal(rnd.Next(1, 254), rnd.Next(1, 254), 42); //for local coordinates only
                //Client.Self.AutoPilot(rnd.Next(1, 254), rnd.Next(1, 254), 42);        //for global coordinates

                Client.Self.AutoPilotLocal(rnd.Next(1, 700), rnd.Next(1, 500), 0); //put always z=0, otherwise no matter what you do the avatar will always fly
                
                Thread.Sleep(5000);
                time += 5000;
            }
            //Thread.Sleep(10000);
        }

        public void RandomRunFor3minutes(int delay)
        {
            //Avatar mobility model for Running an avatar for 3 minutes in random directions
            Random rnd = new Random();
            int time = 0;
            Client.Self.Movement.AlwaysRun = true;
            Client.Self.Movement.Fly = false;
            //Console.WriteLine("*** calling RandomRun() *** ");
            //Client.Self.Chat("*** calling RandomRun() *** ", 0, ChatType.Normal);
            while (time <= delay)
            {
                //TheClient.Self.AutoPilotLocal(rnd.Next(1, 254), rnd.Next(1, 254), 42);
                Client.Self.AutoPilotLocal(rnd.Next(1, 700), rnd.Next(1, 500), 0);              //put always z=0
                Thread.Sleep(5000);
                time += 5000;
            }
        }

   
        public void RandomFlyFor3minutes(int delay)
        {
            //Avatar mobility model for Flying an avatar for 3 minutes in random directions
            //TheClient.Self.Movement.AlwaysRun = false;
            Client.Self.Movement.AlwaysRun = false;
            Client.Self.Movement.Fly = true;

            Console.WriteLine("*** calling RandomFly() *** ");
            Client.Self.Chat("*** Flying Now! *** ", 0, ChatType.Normal);
            Random rnd = new Random();
            int time = 0;
            while (time <= delay)
            {

                //TheClient.Self.AutoPilotLocal(rnd.Next(1, 254), rnd.Next(1, 254), 103);
                Client.Self.AutoPilotLocal(rnd.Next(1, 700), rnd.Next(1, 500), 50);
                Thread.Sleep(5000);
                time += 5000;
            }
        }

        public void RandomTeleportFor3minutes(int delay)
        {
            //Avatar mobility model:
            //Standing still (no Yawing) for 30 seconds
            //The teleporting to a random location on the whole map
            //Then Standing still (no Yawing) for 30 seconds Then teleporting to a random location on the whole map etc... untill it fills 3 minutes
            Random rnd = new Random();
            int time = 0;
            while (time <= delay)
            {
                OpenMetaverse.Vector3 RandomTarget = new Vector3(rnd.Next(1, 700), rnd.Next(1, 500), rnd.Next(1, 254));
                StayingStillWithoutYawFor3minutes(30000);
                time += 30000;
                Console.WriteLine("*** Teleporting Now! *** ");
                Client.Self.Chat("*** Teleporting Now! *** ", 0, ChatType.Normal);
                Client.Self.Teleport("Cathedral 1", RandomTarget);
                Thread.Sleep(3000);
                time += 3000;
            }


        }

        public void RandomActions (int delay)
        {
            Client.Self.Chat("*** I will Jump now: *** ", 0, ChatType.Normal);
            Client.Self.Jump(true);     //Jumps or flies up

            Client.Self.Chat("*** I will sit on closest primitive now: *** ", 0, ChatType.Normal);
            //Attempt to sit on the closest prim
            //code taken from examples inside the lipopenmetaverse library
            //Other usefull things to do can be found https://github.com/openmetaversefoundation/libopenmetaverse/tree/master/Programs/examples/TestClient/Commands


            Primitive closest = null;
            double closestDistance = Double.MaxValue;

            Client.Network.CurrentSim.ObjectsPrimitives.ForEach(
                delegate (Primitive prim)
                {
                    float distance = Vector3.Distance(Client.Self.SimPosition, prim.Position);

                    if (closest == null || distance < closestDistance)
                    {
                        closest = prim;
                        closestDistance = distance;
                    }
                }
            );

            if (closest != null)
            {
                Client.Self.RequestSit(closest.ID, Vector3.Zero);
                Client.Self.Sit();

                Console.WriteLine( "Sat on " + closest.ID + " (" + closest.LocalID + "). Distance: " + closestDistance);
            }
            else
            {
                Console.WriteLine("Couldn't find a nearby prim to sit on");
            }




        }


    }



    }

