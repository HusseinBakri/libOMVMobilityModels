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
//using System.Windows.Forms;
//using System.Windows;


namespace libOMVMobilityModels
{
    
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Constructing a bot needs:
            //string startLocat, string LoginServer, string username, string lastname, string password, string MobilityModel, int MobilityModelDuration
            //Mobility models: StayingStillWithoutYawFor3minutes , StayingStillWithYawFor3minutes , RandomWalkFor3minutes
            //RandomRunFor3minutes , RandomFlyFor3minutes , RandomTeleportFor3minutes
            //duration is in milliseconds (default is 3 minutes = 180000 milliseconds)
            
            Console.WriteLine("Welcome to an NPC bot loader using 6 mobility models explained later...- you can add to the source code more if your want");
            Console.WriteLine("I need to ask you few questions before...");
            Console.WriteLine("What is the server handler? [PS: it should be given of the form:http://127.0.0.1:9000 , Press ENTER for default]");
            string InputServerHandler = Console.ReadLine();
            string ServerHandler = String.IsNullOrEmpty(InputServerHandler) ? "http://127.0.0.1:9000" : InputServerHandler;
            Console.WriteLine("********************************************************************************************************************");
            Console.WriteLine("This application requires a CSV file where you will store information about accounts you have already created in OpenSim for your NPC bots ");
            Console.WriteLine("Each Line in the CSV should be of the following format: [PS: the symbol | means OR; ServerIP:Port is of the form http://127.0.0.1:9000] ");
            Console.WriteLine("FirstName, LastName, Password, email");
            Console.WriteLine("This format can be generated easly by one of my bash script see my GitHub repository URL:");
            Console.WriteLine("First parameter above in the CSV is the starting location as a region name like Cathedral 1");
            Console.WriteLine("********************************************************************************************************************");
            Console.WriteLine("Please give us the CSV name with its absolute path: [PS: like the form C:\\test.csv");
            string CSVFilePath = Console.ReadLine();

            //Uncomment if you want to use System.Windows.Forms [need to do extra work for Mono (Linux,MacOS)] - no time for that
            //string CSVFilePath = "C:\\OpenSimUsers.csv";
            //OpenFileDialog fileSelectPopUp = new OpenFileDialog();
            //fileSelectPopUp.Title = "Select the CSV file of OpenSim NPC accounts...";
            //fileSelectPopUp.RestoreDirectory = true;
            //if (fileSelectPopUp.ShowDialog() == DialogResult.OK)
            //{
            //    CSVFilePath = fileSelectPopUp.FileName;
            //}

            Console.WriteLine("********************************************************************************************************************");
            Console.WriteLine("Now please choose mobility model  - write the exact name shown below, per example:  StayingStillWithoutYawFor3minutes ");
            Console.WriteLine("Available Mobility Models are: ***************************************************************************************");
            Console.WriteLine(" ** [1] StayingStillWithoutYawFor3minutes ** , ** [2] StayingStillWithYawFor3minutes ** , ** [3] RandomWalkFor3minutes **");
            Console.WriteLine(" ** [4] RandomRunFor3minutes ** , ** [5] RandomFlyFor3minutes ** , ** [6] RandomTeleportFor3minutes ** ");
            Console.WriteLine("ENTER for Default which is [3]RandomWalkFor3minutes...");
           string InputMobilityModel = Console.ReadLine();
           

            Dictionary<int, string> MobilityModelDictionary = new Dictionary<int, string>();
            MobilityModelDictionary.Add(1, "StayingStillWithoutYawFor3minutes");
            MobilityModelDictionary.Add(2, "StayingStillWithYawFor3minutes");
            MobilityModelDictionary.Add(3, "RandomWalkFor3minutes");
            MobilityModelDictionary.Add(4, "RandomRunFor3minutes");
            MobilityModelDictionary.Add(5, "RandomFlyFor3minutes");
            MobilityModelDictionary.Add(6, "RandomTeleportFor3minutes");

            string MobilityModel = String.IsNullOrEmpty(InputMobilityModel) ? "RandomWalkFor3minutes" : MobilityModelDictionary[Int32.Parse(InputMobilityModel)];

            Console.WriteLine("********************************************************************************************************************");
            Console.WriteLine("Now please enter your starting location (where NPCs spawn) as a region name: [PS: like the form Cathedral 1] ");
            Console.WriteLine("If more than one regions as starting locations, please write them in the form:(without spaces): Cathedral 1,Cathedral 2,Cathedral 4");
            string InputstartLocation = Console.ReadLine();
            Console.WriteLine("********************************************************************************************************************");

            Random rnd = new Random();
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            String[] StartLocationsArray = InputstartLocation.Split(delimiterChars);
            
            Console.WriteLine("Finally please enter the Mobility model duration in milliseconds [1 second is 1000 milliseconds]");
            Console.WriteLine("Press ENTER for default which is 3 minutes [180000 milliseconds]: ");
            string InputMobilityModelDuration = Console.ReadLine();
            int MobilityModelDuration = String.IsNullOrEmpty(InputMobilityModelDuration) ? 180000 : Int32.Parse(InputMobilityModelDuration);
            Console.WriteLine("********************************************************************************************************************");

            //Bot mybot = new Bot("Cathedral 1", "http://127.0.0.1:9000","Hussein1","Bakri1","123", "RandomWalkFor3minutes",180000);
            List <Bot> myBotList = new List<Bot>();
            try
            {
                var reader = new StreamReader(File.OpenRead(@CSVFilePath));
                int RandomChoice;

                while (!reader.EndOfStream)
                {
                    RandomChoice = rnd.Next(StartLocationsArray.Length);
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    myBotList.Add(new Bot(StartLocationsArray[RandomChoice], ServerHandler, values[0], values[1], values[2], MobilityModel, MobilityModelDuration));
                }

            }
            catch (IOException)
            {
                Console.WriteLine("IO ERROR: File Not Found or either file is unreadable.....Closing application");
                Console.WriteLine("Press any key to exist...");
                Console.ReadLine();
                Environment.Exit(1);
            }
            
            Console.WriteLine("!!!!...");
            Thread.Sleep(50000);
            //DateTime Created = DateTime.Now;
            Console.WriteLine("Starting the statistical bot now....");
            NPCForStats StatisticalBot = new NPCForStats(StartLocationsArray[0], ServerHandler, "statbot", "statbot", "123", MobilityModelDuration);
          //  DateTime Finished = DateTime.Now;
           // Console.WriteLine("Statistics bot up time: " + (Finished - Created)) ;

            //Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();
        }
  
        

   

    }
}
