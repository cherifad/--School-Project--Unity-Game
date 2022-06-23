V2.5 Map Generator

Disclaimer: This is NOT a random map generator, it is a tool to convert bitmaps into three dimensional dungeon-like maps for games.

Usage:

create a bit map file in your favourite image editor you can select both height and width as needed. Paint the whole image in black, black will be the void color on your map. to add walls use pure Red Color. Walless floor will be white, for outside corners use pure Green Color, to add Collumns you'll need pure Blue, diagonal walls use Magenta for a Second level Ceiling floor use Cyan.

save your bitmap WITHOUT ANY compression and import it to unity with read and write enabled.

open the MapGen scene and add the desired map texture to Map on MapGenerator game object.

by default only 1 sample of each possible tile type assigned to the generator, you may want to modify those. you can add multiple prefabs to each tile type, the generator will choose one of them at random for each instance of that type. Make sure all your tiles have the same size and set that size in the TileDimension option in MapGen Script.
Trans is the parent transform your map will be allocated. That helps export your maps for better tunning and re-usage between scenes.
Version 2.0 adds a Cluster build option. With cluster mode instead of building a dungeon tile by tile you can build it room by room. Cluster mode assigns diferent room types on each of the 7 now supported colors so you can draw your bitmap as you please. be aware though it does not support tile rotation, since it does not know how to proper connect room exits.
It is highly recommended for cluster mode to make rooms as modulated as possible in order to minimize possible tile assembly errors.

when satisfied, press Build Object.

ChangeLog V2.5 new ceiling floor prefab and rotation added