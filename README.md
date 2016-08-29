Author
-----
Hussein Bakri

Program Name
-----------
libOMVMobilityModels 1.0

License
-------
This program is licensed under GNU GPL v3 License - you are free to distribute, change, enhance and include any of the code of this application in your tools.
I only expect adequate attribution of this work. The attribution should include the title of the program, the author and the site or the document where the program is taken from.

Description
-----------
A C# application that allows you to load  Non-Player Characters (NPC bots) as many as you want into OpenSim or Second Life using 6 mobility models (you can expand on them if required). It reads their accounts information from a CSV file. 

libOMVMobilityModels uses LibOpenMetaverse Release 0.9.3 that can be found: https://github.com/openmetaversefoundation/libopenmetaverse
LibOpenMetaverse is a C# client side library for accessing OpenSim and Second Life.

This application is the result of complete days of work. LibOpenMetaverse is not well documented as you will probably figure out so this application will be a starting point for you to expand on, change and enhance. This program can be improved in many ways (it is not perfect and never meant to be).

In summary, the application allows you to load  Non-Player Characters (NPC bots) as many as you want. It reads their accounts information from a CSV file. I have a Linux bash shell script (see https://github.com/HusseinBakri/CreateXUserAccountsForOpenSim) that allows you to create as many NPC accunts for OpenSim as you want and save their info into a CSV file which could be fetched easly in this application without you to worry about anything. 
I will write a similar script for MS Windows users either in PowerShell or Batch. I will see how it can be done.
This application has 6 mobility models  which you can change the duration [by default 3 minutes (180000 milliseconds)] - Yaw is the change is the avatar gaze direction

Available Mobility models: [1] StayingStillWithoutYawFor3minutes , [2] StayingStillWithYawFor3minutes , [3] RandomWalkFor3minutes
[4] RandomRunFor3minutes , [5] RandomFlyFor3minutes , [6] RandomTeleportFor3minutes

The application also load one NPC statbot (this account should be created in OpenSim). Check NPCForStats.cs to see what could be done. This NPC capture simulator QoS (Quality of Service) metrics like FPS etc... every 5 seconds and then save all these metrics into one CSV file so that it could be fetched to a statistical tool like R, SPSS etc... 

Enjoy!

Technicalities
-------------
This application is built in Visual Studio Comunity version 2015. I did not tried it yet on MonoDevelop but probably it will work. 

Important notes before you compile and run this application
----------------------------------------------------------
Please make sure you add the following references before you can run this program : [In Visual Studio Comunity 2015, right click on References and Choose Add References]

1) For OpenMetaverse libraries to work, kindly add the following references as dlls files (add them in VS Comunity 2015 references):
OpenMetaverse.dll, OpenMetaverseTypes.dll, OpenMetaverse.StructuredData.dll, OpenMetaverse.Utilities.dll, OpenMetaverse.Rendering.Linden.dll

2) Don't forget to copy the following dlls into the bin folder of this VS project: openjpeg-dotnet.dll and openjpeg-dotnet-x86_64.dll They are required for the avatars appearance otherwise they will appear as clouds.

3)All above dlls are obtained from the libopenmetaverse source folder "only after" libopenmetaverse being build adequately depending on your target OS

4) Add also System.Windows.Forms ( in VS 2015 references) -- I have used the FolderBrowserDialog() to make the app not dependant on OS for opening the CSV file - it is uncommented in the code so that it works in Mono framework (On Linux and Mac OS).

5) One last important thing: (without this step - the NPC avatars will apear naked after being baked see: https://github.com/openmetaversefoundation/libopenmetaverse/releases

Solution:
Go to the libopenmetaverse source folder and after you have build it adequately depending on the OS, you should go the bin folder there and copy "openmetaverse_data" folder which contains the clothes of avatars among other things (.tga files needed like  glove_lenth_alpha.tga and shirt_collar_alpha.tga etc..)
Paste this folder in the bin directory of "this" project (whether on VS 2015 or MonoDevelop), merge and copy and replace existing files, becuase you might already see a folder of the same name in this project.

Enjoy!
 
TODO
-----
1)I will create later the same application with IronPython for Python lovers out there (including me)! allowing you to create Python NPC bots!

2) Solve problem of System.Windows.Forms for Mono framework.

3)An extension to this is to give this statistic NPC a mobility of its own

4) Retrieve Client side specific QoS (similar to the ones that we can get from SL Viewer) - it is known that the server sends stats about itself to the client periodically.
 
 In other words, TO BE ABLE to save or log the statistics given by the Second Life viewer (The ones available in the Advanced Menu, under Performance Tools) into text files or CSV files etc...so they could be fetched to a statistical tool - does libOpenMetaverse provide them and to which extent?

