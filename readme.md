# Oil Barons Map Tool #

## Overview ##
**Oil Barons Map Tool** is an application to help manage the map when playing the classic C64 game [Oil Barons](http://en.wikipedia.org/wiki/Oil_Barons).  The original game came with a physical jigsaw map that shows which plots belong to which parcels and the plot's terrain type.  The game itself has an internal map with each terrain type, but it lacks the parcel information; therefore, playing the game "correctly" cannot be done without the physical map.

Application info:

1. The game has a settings file in JSON (see below) that allows some minor customization
2. Even though the C64 game supports up to 8 players, this tool only supports up to 4 plus Auction and Government Reserve markers
3. The name and colors for each player can be customized
4. The board state can be saved and reloaded at the next play

### Game Settings File ###

	{
		"useOverlay":false,
		"overlayAlpha":0.0,
		"windowLeft":7,
		"windowTop":0,
		"windowWidth":1285,
		"windowHeight":738,
		"windowState":0
	}

### Parcel File Sample ###

	[
		{
			"id":0,
			"plotIds":[0,50,100],
			"ownedBy":-1
		},
		{
			"id":1,
			"plotIds":[1,51,101,102],
			"ownedBy":-1
		},

		...
	]

### Plot File Sample ###

	[
		[
			{
				"id":0,
				"loc":"0, 0",
				"terrain":11,
				"hasTopBorder":true,
				"hasLeftBorder":true,
				"hasBottomBorder":false,
				"hasRightBorder":true,
				"neighbors":[50],
				"parentParcelId":0,
				"ownedBy":-1,
				"surveyPercentage":0,
				"isSurveyed":false,
				"isDrilled":false,
				"isDry":false,
				"isActive":false,
				"isDepleted":false,
				"isGusher":false
			},
			{
				"id":50,
				"loc":"0, 1",
				"terrain":11,
				"hasTopBorder":false,
				"hasLeftBorder":true,
				"hasBottomBorder":false,
				"hasRightBorder":true,
				"neighbors":[100,0],
				"parentParcelId":0,
				"ownedBy":-1,
				"surveyPercentage":0,
				"isSurveyed":false,
				"isDrilled":false,
				"isDry":false,
				"isActive":false,
				"isDepleted":false,
				"isGusher":false
			}
				
			...
		]
	]

## Development Requirements ##
Visual Studio 2010, C#, .NET Framework 4.0.

## TODO (not in any particular order ;) ) ##
* Add more code comments
* Add SignalR support (?) to help when playing Vice64 over Internet
* Refactor player structure to allow up to 8 players + 2 system

## License ##

Released under the [MIT License](http://www.opensource.org/licenses/mit-license.php)

Copyright (c) 2012 Doug Thompson

Permission is hereby granted, free of charge, to any person obtaining a
copy of this software and associated documentation files (the
"Software"),to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
