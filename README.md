This mod requires [BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain).

# Description
Creates a CSV file that contains every combination of `BaseBrain` and `WildSpawnType`. For each combination, the CSV file contains the following:
* `BaseBrain` GClass name
* Brain layer name
* Brain layer GClass name
* Layer priority
* Layer index

# Usage
When enabled, the analysis will be performed on the first bot that spawns. Its `BaseBrain` will be changed to have every combination of `BaseBrain` and `WildSpawnType` to create the CSV file. After this happens, you must exit the raid because the bot will be "broken". The CSV file will be created in a "log" folder where the plugin DLL is located. 

Options to enable the analysis and enable debug logging are available in the F12 menu. 
