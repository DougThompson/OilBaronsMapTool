/*
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
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.Drawing.Imaging;
using System.Resources;
using System.Text.RegularExpressions;

namespace OilBaronsMapTool
{
	public partial class frmMap : Form
	{

#region Structures
		struct GameSettings
		{
			public bool useOverlay;
			public float overlayAlpha;
			public int windowLeft;
			public int windowTop;
			public int windowWidth;
			public int windowHeight;
			public int windowState;
		}
		struct GridPlot
		{
			public int id;
			public Point loc;
			public Terrain terrain;
			public bool hasTopBorder;
			public bool hasLeftBorder;
			public bool hasBottomBorder;
			public bool hasRightBorder;
			public List<int> neighbors;
			public int parentParcelId;
			public int ownedBy;

			public int surveyPercentage;
			public bool isSurveyed;
			public bool isDrilled;
			public bool isDry;
			public bool isActive;
			public bool isDepleted;
			public bool isGusher;
		}

		struct Parcel
		{
			public int id;
			public List<int> plotIds;
			public int ownedBy;
		}

		struct Player
		{
			public string playerName;
			public Color outline;
			public int colorIndex;
			//public List<Parcel> parcelsOwned;
			//public List<GridPlot> surveyed;
			//public List<GridPlot> drilled;
			//public List<GridPlot> activeWells;
			//public List<GridPlot> deadWells;
			//public List<GridPlot> gushers;
		}
#endregion

		GridPlot[,] map = new GridPlot[50, 40];
		Parcel[] mapParcels;
		Player[] players = new Player[6];

		Bitmap bmpBaseMap = new Bitmap("baseMap.png");
		Bitmap bmpActive = new Bitmap("active.png");
		Bitmap bmpDry = new Bitmap("dry.png");
		Bitmap bmpDepleted = new Bitmap("depleted.png");
		Bitmap bmpGusher = new Bitmap("gusher.png");
		Bitmap bmpSurveyed = new Bitmap("surveyed.png");
		Bitmap bmpWhite = new Bitmap("white.png");
		Bitmap bmpCloneMap;
		Bitmap bmpPlayerColors;

		int side = 16;
		int padding = 1;
		int offset = 20;
		int fontFudgeStart = -1;
		int fontFudgeEnd = 3;
		int lastX = 0;
		int lastY = 0;
        bool surveyFormDisplayed = false;

		Pen parcelOutline = new Pen(Color.LightGray, 3);
		Pen gridLine = new Pen(Color.White, 1);

		public enum Terrain { Desert, Plains, Brushland, IcePack, Lake, Coastline, Forest, Mountains, Jungle, Swamp, Offshore, Arctic, City, Unknown }

        public Color[] playerColors = new Color[] { Color.Blue, Color.Yellow, Color.Magenta, Color.Orange, Color.Green, Color.Purple, Color.White, Color.Cyan, Color.Red };

		GameSettings gameSettings;

		public frmMap()
		{
			InitializeComponent();
		}

		private void frmMap_Load(object sender, EventArgs e)
		{
			try
            {
                #region Parsing Routines
                // Use the functions below to parse the default images and files to build the
                // reference files.

                //this.Show();
				//Application.DoEvents();
				//parseKey();
				//parseMaps();
				//parseParcels();
                //mapSetup();
                #endregion

                readMapData();
				setupPlayers();
				loadBoardStatus();
				updateMap();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				throw;
			}
		}

        private void frmMap_FormClosing(object sender, FormClosingEventArgs e)
		{
            // Save the board status
			saveBoardStatus();

            // Save the game setting and window state
            gameSettings.windowHeight = this.Height;
            gameSettings.windowLeft = this.Left;
            gameSettings.windowTop = this.Top;
            gameSettings.windowWidth = this.Width;
            gameSettings.windowState = (int)this.WindowState;

            // Serialize to JSON 
            string jsonString = JsonConvert.SerializeObject(gameSettings, Formatting.None);
			File.WriteAllText("gameSettings.json", jsonString);
		}

		private void picMap_MouseClick(object sender, MouseEventArgs e)
		{
            // Not currently used, originally used for development and debugging

            //int x = (e.X - offset) / 17;
            //int y = (e.Y - offset) / 17;

            //if (isValidPlot(e.X, e.Y))
            //{
            //    lblCurrentLocation.Text = String.Format("( {0}, {1} ), {2}{3}", x + 1, y + 1, map[x, y].terrain.ToString(), map[x, y].surveyPercentage <= 0 ? "" : String.Format(", {0}%", map[x, y].surveyPercentage.ToString()));
            //    picMap.Image = bmpCloneMap;
            //}
		}

        private void picMap_DoubleClick(object sender, EventArgs e)
        {
            // Open the survey form via the survey context menu click event
            surveyFormDisplayed = true;
            mnuSurveyed_Click(null, null);
            surveyFormDisplayed = false;
        }

		private void picMap_MouseMove(object sender, MouseEventArgs e)
		{
            // Update the map highlight as the user mouses over the plots
            lastX = e.X;
            lastY = e.Y;

            drawHighlight();
		}

		private void picMap_MouseLeave(object sender, EventArgs e)
		{
            // Clear any highlights and update the location text

            if (!mnuMap.Visible & !surveyFormDisplayed)
            {
                picMap.Image = bmpCloneMap;
                lblCurrentLocation.Text = "( -, - )";
            }
		}

		private void List_SelectedIndexChanged(object sender, EventArgs e)
		{
            // Generic list selected index handler
            // When an item is selected in a list

			ListBox list = sender as ListBox;
            
            // Build regex to grab the x and y coordinates
            // Assume string is correct
            // TODO: add error checking in case string malformed
            Regex regex = new Regex(@".*\(([0-9]+), ([0-9]+)\)");
            MatchCollection mc = regex.Matches(list.Text);
            int x = int.Parse(mc[0].Groups[1].Value) - 1;
            int y = int.Parse(mc[0].Groups[2].Value) - 1;

            // Grab the clone image and calculate the offset to highlight
            // the plot with a goldenrod frame
			picMap.Image = bmpCloneMap;
			Bitmap newMap = bmpCloneMap.Clone() as Bitmap;
			using (Graphics gr = Graphics.FromImage(newMap))
			{
				x = x * side + x * padding + offset;
				y = y * side + y * padding + offset;

				gr.DrawRectangle(new Pen(Color.Goldenrod, 3), x, y, 17, 17);
			}
			picMap.Image = newMap;
		}

        private void mnuMap_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            // Clean up after the context menu closes
            picMap.Image = bmpCloneMap;
            lblCurrentLocation.Text = "( -, - )";
        }

        private void mnuClearPlayer_Click(object sender, EventArgs e)
        {
            // Convert the click to a plot then clear the plot ownership

            int x = (lastX - offset) / 17;
            int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
            {
                assignPlotToPlayer(-1, x, y);
            }
        }

		private void mnuClearStatus_Click(object sender, EventArgs e)
		{
            // Convert the click to a plot and then clear the plot status

            int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
				clearPlotStatus("", x, y);
				updateMap();
				//autoSave();
			}
		}

		private void mnuSurveyed_Click(object sender, EventArgs e)
		{
            // Convert the click to a cell and then set the cell survey percentage

            int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

            // Check for a valid plot
			if (isValidPlot(lastX, lastY))
			{
                // Draw highlight, open survey form and populate which cell to be surveyed
                drawHighlight();
				frmSurveryPercentage frm = new frmSurveryPercentage();
				frm.surveyPercentage = map[x, y].surveyPercentage;
                frm.x = x + 1;
                frm.y = y + 1;
                frm.curTerrain = (this.Controls.Find("pictureBox" + ((int)map[x, y].terrain + 1).ToString("00"), true)[0] as PictureBox).Image as Bitmap;
                frm.terrainName = map[x, y].terrain.ToString();
                frm.playerName = map[x, y].ownedBy >= 0 ? players[map[x, y].ownedBy].playerName : "No one";
				frm.ShowDialog();

                // If the form is closed with "OK" then update the map status
				if (frm.DialogResult == System.Windows.Forms.DialogResult.OK)
				{
                    map[x, y].surveyPercentage = frm.surveyPercentage;
					map[x, y].isSurveyed = true;
                    clearPlotStatus("surveyed", x, y);
                    updateMap();

                    // Debug info
					//string item = getListItemName(x, y, true);
                }
				frm.Dispose();
            }
		}

		private void mnuDryHole_Click(object sender, EventArgs e)
		{
            // Convert the click to a plot and then set the status as "dry"

            int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
				map[x, y].isDry = true;
				clearPlotStatus("dry", x, y);
				updateMap();
			}
		}

		private void mnuActiveWell_Click(object sender, EventArgs e)
		{
            // Convert the click to a plot and then set the status as "active"

            int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
				map[x, y].isActive = true;
				clearPlotStatus("active", x, y);
				updateMap();
			}
		}

		private void mnuDepleted_Click(object sender, EventArgs e)
		{
            // Convert the click to a plot and then set the status as "depleted"

            int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
				map[x, y].isDepleted = true;
				clearPlotStatus("depleted", x, y);
				updateMap();
			}
		}

		private void mnuGusher_Click(object sender, EventArgs e)
		{
            // Convert the click to a plot and then set the status as "gusher"

            int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
				map[x, y].isGusher = true;
				clearPlotStatus("gusher", x, y);
				updateMap();
			}
		}

		private void cbxPlayers_SelectedIndexChanged(object sender, EventArgs e)
		{
            // Handle the switch of the current player

			int player = cbxPlayers.SelectedIndex - 1;
			txtPlayerName.Text = cbxPlayers.Text;

			if (player >= 0) //& player < 4)
			{
				txtPlayerName.Enabled = (player < 4);
				picPlayerColor.Enabled = (player < 4);
				setPlayerColor(players[player].colorIndex);
			}
			else
			{
				txtPlayerName.Enabled = false;
				picPlayerColor.Enabled = false;
				updateLists();
			}
		}
		
		private void PlayerName_TextChanged(object sender, EventArgs e)
        {
            // Handle the player name change and update the tool strip menu, too

			int player = cbxPlayers.SelectedIndex - 1;
			if (player >= 0 & player < 4)
			{
				players[player].playerName = txtPlayerName.Text;

				ToolStripMenuItem menuItem = new ToolStripMenuItem();
				if (player == 0)
					menuItem = mnuAddToPlayer1;
				if (player == 1)
					menuItem = mnuAddToPlayer2;
				if (player == 2)
					menuItem = mnuAddToPlayer3;
				if (player == 3)
					menuItem = mnuAddToPlayer4;

                // Remove the old name, insert in the new name
				menuItem.Text = players[player].playerName;
				cbxPlayers.Items.RemoveAt(player + 1);
				cbxPlayers.Items.Insert(player + 1, players[player].playerName);
				cbxPlayers.SelectedIndex = player + 1;
			}
        }

		private void PlayerColor_MouseClick(object sender, MouseEventArgs e)
		{
            // Handle chaning a player's color

			int player = cbxPlayers.SelectedIndex - 1;
			players[player].colorIndex = e.X / 16;
			players[player].outline = playerColors[players[player].colorIndex];
			setPlayerColor(players[player].colorIndex);
			updateMap();
		}
        
        private void AddToPlayer_Click(object sender, EventArgs e)
		{
            // Assign a plot and the parcel to a player
			int player = getPlayerNumber(((ToolStripMenuItem)sender).Name);

			int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
				assignPlotToPlayer(player, x, y);
			}
		}

		private bool isValidPlot(int x, int y)
		{
            // Make sure that the calculated plot is within range

			int tempX = (lastX - offset) / 17;
			int tempY = (lastY - offset) / 17;

			if ((tempX >= 0 & tempX <= 49 & (x - offset >= 0)) & (tempY >= 0 & tempY <= 39 & (y - offset >= 0)))
				return true;
			else
				return false;
		}

		private GridPlot getMapPlotFromId(int id)
		{
            // Get a map plot from an ID to calc the x, y coords

			int x = id % 50;
			int y = id / 50;

			return map[x, y];
		}

		private int getPlayerNumber(string controlName)
		{
            //Handle grabbing the player number

			int player = -1;
			for (int i = 10; i >= 1; i--)
			{
				if (controlName.Contains(i.ToString()))
				{
					player = i - 1;
					break;
				}
			}
			return player;
		}

		private Parcel getParcelFromPlotId(int plotId)
		{
            // Find the parcel that includes the plot id
			Parcel parcel = (from par in mapParcels
						where par.plotIds.Contains(plotId)
						select par).FirstOrDefault();

			if (parcel.plotIds == null)
				parcel.ownedBy = -1;

			return parcel;
		}

		private Parcel[] getParcelsByOwner(int player)
		{
            // Get all parcels owned by a player

			Parcel[] parcels = (from par in mapParcels
						where par.ownedBy == player
						select par).ToArray();

			return parcels;
		}

		private GridPlot[] getPlotsByOwner(int player)
		{
            // Get all plots owned by a player

            // First grab all parcels owned by a player
            // Then add each plot to the list and return as an array
			Parcel[] parcels = (from par in mapParcels
						  where par.ownedBy == player
						  select par).ToArray();

			List<GridPlot> plots = new List<GridPlot>();
			foreach (Parcel parcel in parcels)
			{
				foreach (int plotId in parcel.plotIds)
				{
					plots.Add(getMapPlotFromId(plotId));
				}
			}

			return plots.ToArray();
		}

		private string getListItemName(int x, int y, bool includePercentage)
        {
            // Given the coordinates, populate the list with the proper text

            GridPlot plot = map[x, y];
            int player = 0;

            // Get the player that owns the parcel from the given plot
			player = getParcelFromPlotId(plot.id).ownedBy + 1;

            string playerName = player == 0 ? "No one" : cbxPlayers.Items[player].ToString();
            string item = "";
            
            if (includePercentage)
                item = String.Format("{0} - ({1}, {2}), {3}%", playerName, x + 1, y + 1, plot.surveyPercentage);
            else
                item = String.Format("{0} - ({1}, {2})", playerName, x + 1, y + 1);

            return item;
        }

		private void clearPlotStatus(string keep, int x, int y)
		{
            // Clear a status

			if (map[x, y].isSurveyed & !keep.Equals("surveyed", StringComparison.InvariantCultureIgnoreCase))
			{
				map[x, y].isSurveyed = false;
			}
			if (map[x, y].isActive & !keep.Equals("active", StringComparison.InvariantCultureIgnoreCase))
			{
				map[x, y].isActive = false;
			}
			if (map[x, y].isDepleted & !keep.Equals("depleted", StringComparison.InvariantCultureIgnoreCase))
			{
				map[x, y].isDepleted = false;
			}
			if (map[x, y].isDry & !keep.Equals("dry", StringComparison.InvariantCultureIgnoreCase))
			{
				map[x, y].isDry = false;
			}
			if (map[x, y].isGusher & !keep.Equals("gusher", StringComparison.InvariantCultureIgnoreCase))
			{
				map[x, y].isGusher = false;
			}
			
			map[x, y].isDrilled = map[x, y].isActive | map[x, y].isDepleted | map[x, y].isDry | map[x, y].isGusher;
			updateLists();
		}

		private void assignPlotToPlayer(int player, int x, int y)
		{
            // Assign a plot to a player

			try
			{
                // First grab the parcel that contains the plot, and set the owner
				Parcel parcel = getParcelFromPlotId(map[x, y].id); //mapParcels[i];
				parcel.ownedBy = player;
				mapParcels[parcel.id] = parcel;

                // Next, loop through each plot in the parcel and set the owner
				foreach (int plotId in parcel.plotIds)
				{
					x = plotId % 50;
					y = plotId / 50;
					map[x, y].ownedBy = player;
				}

                // Update the map to show the change in ownership
				updateMap();
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		private void setupPlayers()
		{
            // Setup the players
			cbxPlayers.SelectedIndex = 0;
			setPlayerColorMap();

			for (int i = 0; i < 4; i++)
			{
				players[i].playerName = "Player " + (i + 1).ToString();
				players[i].outline = playerColors[i];
				players[i].colorIndex = i;
			}

			//Auction
			players[4].outline = playerColors[7];
			players[4].colorIndex = 7;

			//Government Reserve
			players[5].outline = playerColors[8];
			players[5].colorIndex = 8;
		}

		private void setPlayerColorMap()
		{
            // Draw the player color map to allow user to switch colors for each player

			int xOffset = 0;
			int color = 0;

			if (bmpPlayerColors == null)
			{
				System.Drawing.Bitmap colorSelection = new System.Drawing.Bitmap(160, 16);
				using (Graphics gr = Graphics.FromImage(colorSelection))
				{
                    // Increment the x-offset and color for each player

					SolidBrush myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);

					xOffset += 16;
					color++;
					myBrush = new System.Drawing.SolidBrush(playerColors[color]);
					gr.FillRectangle(myBrush, xOffset, 0, 16, 16);
				}
				bmpPlayerColors = colorSelection.Clone() as Bitmap;
			}

			picPlayerColor.Image = bmpPlayerColors.Clone() as Bitmap;
		}

		private void setPlayerColor(int color)
		{
            // Set the player color
			setPlayerColorMap();
			Bitmap cloneColors = picPlayerColor.Image.Clone() as Bitmap;
			using (Graphics gr = Graphics.FromImage(cloneColors))
			{
				gr.DrawRectangle(new Pen(Color.Black, 3), 1 + color * 16, 1, 13, 13);
				picPlayerColor.Image = cloneColors;
			}

			updateMap();
		}

		private void readMapData()
		{
            // Read the JSON map and parcel data
            // This reads the base parcel/plot relationships and any saved data

			map = JsonConvert.DeserializeObject<GridPlot[,]>(File.ReadAllText("map.json"));
			mapParcels = JsonConvert.DeserializeObject<Parcel[]>(File.ReadAllText("parcel.json"));

			for (int i = 0; i < mapParcels.Count(); i++)
			{
				mapParcels[i].ownedBy = -1;
			}
		}

		private void saveBoardStatus()
		{
            // Ask the user to save the board status
			string jsonString = "";

			if (MessageBox.Show("Do you want to save the current board status?", "Save Current Board?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
			{
                // If no, then delete all previously saved files
                if (File.Exists("curPlayerParcels.json"))
					File.Delete("curPlayerParcels.json");

				if (File.Exists("curMapPlots.json"))
					File.Delete("curMapPlots.json");

				if (File.Exists("curPlayers.json"))
					File.Delete("curPlayers.json");
			}
			else
			{
                // If yes, then serialize data to JSON and save the files
                jsonString = JsonConvert.SerializeObject(mapParcels, Formatting.None);
				File.WriteAllText("curPlayerParcels.json", jsonString);

				jsonString = JsonConvert.SerializeObject(map, Formatting.None);
				File.WriteAllText("curMapPlots.json", jsonString);

				jsonString = JsonConvert.SerializeObject(players, Formatting.None);
				File.WriteAllText("curPlayers.json", jsonString);
			}
		}

		private void loadBoardStatus()
		{
            // Load the board status

            // Check for a gameSettings.json and read data if it exists
            // If it exists, reset window location, size, and state
            if (File.Exists("gameSettings.json"))
            {
                gameSettings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText("gameSettings.json"));
                this.Left = gameSettings.windowLeft;
                this.Top = gameSettings.windowTop;
                this.Width = Math.Min(Screen.FromControl(this).WorkingArea.Width, gameSettings.windowWidth);
                this.Height = Math.Min(Screen.FromControl(this).WorkingArea.Height, gameSettings.windowHeight);
                this.WindowState = gameSettings.windowState == 2 ? FormWindowState.Maximized : FormWindowState.Normal;
            }
            else
            {
                gameSettings.useOverlay = true;
                gameSettings.overlayAlpha = 0.5f;
            }

            // If any previously saved files, parse the data and update game state
			if (File.Exists("curPlayerParcels.json")
				|| File.Exists("curMapPlots.json")
				|| File.Exists("curPlayers.json")
			)
			{
				if (MessageBox.Show("Do you want to load the last saved board status?", "Load Last Saved Board?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
				{
					if (File.Exists("curPlayerParcels.json"))
					{
						mapParcels = JsonConvert.DeserializeObject<Parcel[]>(File.ReadAllText("curPlayerParcels.json"));
					}

					if (File.Exists("curMapPlots.json"))
					{
						map = JsonConvert.DeserializeObject<GridPlot[,]>(File.ReadAllText("curMapPlots.json"));
					}

					if (File.Exists("curPlayers.json"))
					{
						players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText("curPlayers.json"));
						
						for (int i = 0; i < 4; i++)
						{
							ToolStripMenuItem menuItem = new ToolStripMenuItem();
							if (i == 0)
								menuItem = mnuAddToPlayer1;
							if (i == 1)
								menuItem = mnuAddToPlayer2;
							if (i == 2)
								menuItem = mnuAddToPlayer3;
							if (i == 3)
								menuItem = mnuAddToPlayer4;

							menuItem.Text = players[i].playerName;
							cbxPlayers.Items.RemoveAt(i + 1);
							cbxPlayers.Items.Insert(i + 1, players[i].playerName);
						}
					}

					updateMap();
				}
			}
		}

		private void updateMap()
		{
            // Update the map graphics

			Bitmap tempMap = bmpBaseMap.Clone() as Bitmap;
			using (Graphics gr = Graphics.FromImage(tempMap))
			{
				updateMapStatuses(gr);
				updateParcels(gr);
			}
			picMap.Image = tempMap;
			bmpCloneMap = tempMap.Clone() as Bitmap;

			updateLists();
		}

		private void updateMapStatuses(Graphics gr)
		{
            // Use the passed graphics object to overlay the statuses

			Bitmap newMap = bmpBaseMap.Clone() as Bitmap;
			for (int y = 0; y < 40; y++)
			{
				for (int x = 0; x < 50; x++)
				{
					if (map[x, y].isSurveyed)
					{
						gr.DrawImage(bmpSurveyed, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
					}
					if (map[x, y].isDry)
					{
						gr.DrawImage(bmpDry, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
					}
					if (map[x, y].isActive)
					{
						gr.DrawImage(bmpActive, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
					}
					if (map[x, y].isGusher)
					{
						gr.DrawImage(bmpGusher, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
					}
					if (map[x, y].isDepleted)
					{
						gr.DrawImage(bmpDepleted, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
					}
				}
			}
		}

		private void updateParcels(Graphics gr)
		{
            // For each un-owned parcel, update the graphics based on the plot 
			foreach (var parcel in mapParcels)
			{
				if (parcel.ownedBy < 0)
				{
					foreach (int plotId in parcel.plotIds)
					{
                        // Get plot details and draw the border
                        // If using an overlay, then also draw the overlay
						GridPlot plot = getMapPlotFromId(plotId);
						int x = plot.loc.X;
						int y = plot.loc.Y;

						if (gameSettings.useOverlay)
						{
							ColorMatrix cm = new ColorMatrix();
							cm.Matrix33 = gameSettings.overlayAlpha;
							ImageAttributes ia = new ImageAttributes();
							ia.SetColorMatrix(cm);
							gr.DrawImage(bmpWhite, new Rectangle(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, ia);
							//gr.DrawImage(bmpWhite, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
						}

                        // If the plot has any borders, draw them
						if (plot.hasTopBorder)
							gr.DrawLine(parcelOutline, x * side + x * padding + offset + fontFudgeStart, y * side + y * padding + offset, x * side + x * padding + side + offset + fontFudgeEnd, y * side + y * padding + offset);
						if (plot.hasBottomBorder)
							gr.DrawLine(parcelOutline, x * side + x * padding + offset + fontFudgeStart, y * side + (y + 1) * padding + side + offset, x * side + x * padding + side + offset + fontFudgeEnd, y * side + (y + 1) * padding + side + offset);
						if (plot.hasLeftBorder)
							gr.DrawLine(parcelOutline, x * side + x * padding + offset, y * side + y * padding + offset + fontFudgeStart, x * side + x * padding + offset, y * side + y * padding + side + offset + fontFudgeEnd);
						if (plot.hasRightBorder)
							gr.DrawLine(parcelOutline, x * side + side + (x + 1) * padding + offset, y * side + y * padding + offset + fontFudgeStart, x * side + side + (x + 1) * padding + offset, y * side + y * padding + side + offset + fontFudgeEnd);
					}
				}
			}

            // For each owned parcel, update the graphics and draw each border
            // that exists for each plot
			foreach (var parcel in mapParcels)
			{
				if (parcel.ownedBy >= 0)
				{
					foreach (int plotId in parcel.plotIds)
					{
						GridPlot plot = getMapPlotFromId(plotId);
						int x = plot.loc.X;
						int y = plot.loc.Y;
                        // Only draw the appropriate borders
						if (plot.hasTopBorder)
							gr.DrawLine(new Pen(players[parcel.ownedBy].outline, 3), x * side + x * padding + offset + fontFudgeStart, y * side + y * padding + offset, x * side + x * padding + side + offset + fontFudgeEnd, y * side + y * padding + offset);
						if (plot.hasBottomBorder)
							gr.DrawLine(new Pen(players[parcel.ownedBy].outline, 3), x * side + x * padding + offset + fontFudgeStart, y * side + (y + 1) * padding + side + offset, x * side + x * padding + side + offset + fontFudgeEnd, y * side + (y + 1) * padding + side + offset);
						if (plot.hasLeftBorder)
							gr.DrawLine(new Pen(players[parcel.ownedBy].outline, 3), x * side + x * padding + offset, y * side + y * padding + offset + fontFudgeStart, x * side + x * padding + offset, y * side + y * padding + side + offset + fontFudgeEnd);
						if (plot.hasRightBorder)
							gr.DrawLine(new Pen(players[parcel.ownedBy].outline, 3), x * side + side + (x + 1) * padding + offset, y * side + y * padding + offset + fontFudgeStart, x * side + side + (x + 1) * padding + offset, y * side + y * padding + side + offset + fontFudgeEnd);
					}
				}
			}
		}

		private void updateLists()
		{
            // Filter the list based on the selected player;
            // if none selected, then display all
			int player = cbxPlayers.SelectedIndex - 1;
			List<string> tempSurveyed = new List<string>();
			List<string> tempDead = new List<string>();
			List<string> tempProducing = new List<string>();

			for (int y = 0; y < 40; y++)
			{
				for (int x = 0; x < 50; x++)
				{
					string item = getListItemName(x, y, true);
					if (map[x, y].isSurveyed)
					{
						if (map[x, y].ownedBy == player | player == -1)
							tempSurveyed.Add(item);

					}
					if (map[x, y].isActive | map[x, y].isGusher)
					{
						if (map[x, y].ownedBy == player | player == -1)
							tempProducing.Add(item);

					}
					if (map[x, y].isDry | map[x, y].isDepleted)
					{
						if (map[x, y].ownedBy == player | player == -1)
							tempDead.Add(item);

					}
				}
			}

            // Sort the data and add to the lists
			lstSurveyed.Items.Clear();
			tempSurveyed.Sort();
			lstSurveyed.Items.AddRange(tempSurveyed.ToArray());

			lstDead.Items.Clear();
			tempDead.Sort();
			lstDead.Items.AddRange(tempDead.ToArray());

			lstProducing.Items.Clear();
			tempProducing.Sort();
			lstProducing.Items.AddRange(tempProducing.ToArray());
		}

		private void drawHighlight()
		{
            // Based on the last x and y position, draw the outline highlight on the map
			int x = (lastX - offset) / 17;
			int y = (lastY - offset) / 17;

			if (isValidPlot(lastX, lastY))
			{
                // The point is valid, so update the info and clone the bitmaps
                // Next, draw the rectangle
				lblCurrentLocation.Text = String.Format("( {0}, {1} ), {2}{3}", x + 1, y + 1, map[x, y].terrain.ToString(), map[x, y].surveyPercentage <= 0 ? "" : String.Format(", {0}%", map[x, y].surveyPercentage.ToString()));

				picMap.Image = bmpCloneMap;
				Bitmap newMap = bmpCloneMap.Clone() as Bitmap;
				using (Graphics gr = Graphics.FromImage(newMap))
				{
					x = x * side + x * padding + offset;
					y = y * side + y * padding + offset;

                    // Draw the four map-edge highlights
					gr.DrawRectangle(new Pen(Color.White, 3), 0, y, 17, 17);
					gr.DrawRectangle(new Pen(Color.White, 3), picMap.Image.Width - 21, y, 17, 17);
					gr.DrawRectangle(new Pen(Color.White, 3), x - 1, 1, 17, 17);
					gr.DrawRectangle(new Pen(Color.White, 3), x - 1, picMap.Image.Height - 21, 17, 17);

                    // Draw the current
					gr.DrawRectangle(new Pen(Color.Red, 3), x, y, 17, 17);
				}
				picMap.Image = newMap;
			}
			else
			{
				picMap.Image = bmpCloneMap;
			}
		}

#region Building Maps

        /// <summary>
        /// These functions are the rough tools used to parse the parcel map and 
        /// the terrain maps.  Then builds the JSON files.
        /// </summary>

		List<int> alreadyVisited = new List<int>();
        public string[] TerrainHashes = new string[] {"SRF2okrH7CXEnFlKcrElLvJdSsY=","hUCGRmr9q1H8wUJdIxV12J5yOSE=","6GhXq7hhwVOOisvskX19TVXbHus=",
                        "rmikAyOQ3DKzTuZXOTS4QIzmEWA=","+n6PdCHcO+mdpfqcPfefMfwJ70Y=","bbI/0vdPA7Urk9hcn4aS33lcST4=","a1yNlBxMgcct5YubgWKWz5WOiHU=",
                        "2R1OCanztqAymhvu8tn7Iwi2yHg=","QKuYrs8E+15mlFVnijEGy95Aj9Q=","SLmUJUd06E4hcHRW2xkN21t5ocA=","KxuqQ5YzHgqr3gnl7avIvVZj3SE=",
                        "O0jCalSuD7sc7JL4RQNhgTD7cJE=","OTCFUYIyoqWdTKsPhGdqSFfkHuo="
        };

        private void mapSetup()
		{
			Bitmap mapBoard = new System.Drawing.Bitmap((50 * (side + padding)) + 2 * offset + 2, (40 * (side + padding)) + 2 * offset + 2);
			using (Graphics gr = Graphics.FromImage(mapBoard))
			{
				gr.Clear(Color.FromArgb(255, 33, 33, 33));

				gr.DrawRectangle(new Pen(Color.Black), offset, offset, 50 * (side + padding), 40 * (side + padding));

				for (int y = 0; y < 40; y++)
				{
					for (int x = 0; x < 50; x++)
					{
						Bitmap bmp = new Bitmap(((int)map[x, y].terrain).ToString() + "_Terrain.png");
						gr.DrawImage(bmp, new Point(x * side + (x + 1) * padding + offset, y * side + (y + 1) * padding + offset));
						bmp.Dispose();
					}
				}

				for (int x = 0; x < 50; x++)
				{
					using (Font myFont = new Font("Arial", 8, FontStyle.Bold))
					{
						StringFormat format = new StringFormat();
						format.Alignment = StringAlignment.Center;
						gr.DrawString((x + 1).ToString(), myFont, Brushes.White, new PointF(x * side + x * padding + offset + 8, 6), format);
						gr.DrawString((x + 1).ToString(), myFont, Brushes.White, new PointF(x * side + x * padding + offset + 8, 702), format);
					}
				}

				for (int y = 0; y < 40; y++)
				{
					using (Font myFont = new Font("Arial", 8, FontStyle.Bold))
					{
						StringFormat format = new StringFormat();
						format.Alignment = StringAlignment.Far;
						gr.DrawString((y + 1).ToString(), myFont, Brushes.White, new PointF(18, y * side + y * padding + offset + 2), format);
						format.Alignment = StringAlignment.Near;
						gr.DrawString((y + 1).ToString(), myFont, Brushes.White, new PointF(872, y * side + y * padding + offset + 2), format);
					}
				}

				//for (int y = 0; y < 40; y++)
				//{
				//    for (int x = 0; x < 50; x++)
				//    {
				//        if (map[x, y].hasTopBorder)
				//            gr.DrawLine(parcelOutline, x * side + x * padding + offset + fontFudgeStart, y * side + y * padding + offset, x * side + x * padding + side + offset + fontFudgeEnd, y * side + y * padding + offset);
				//        if (map[x, y].hasBottomBorder)
				//            gr.DrawLine(parcelOutline, x * side + x * padding + offset + fontFudgeStart, y * side + (y + 1) * padding + side + offset, x * side + x * padding + side + offset + fontFudgeEnd, y * side + (y + 1) * padding + side + offset);
				//        if (map[x, y].hasLeftBorder)
				//            gr.DrawLine(parcelOutline, x * side + x * padding + offset, y * side + y * padding + offset + fontFudgeStart, x * side + x * padding + offset, y * side + y * padding + side + offset + fontFudgeEnd);
				//        if (map[x, y].hasRightBorder)
				//            gr.DrawLine(parcelOutline, x * side + side + (x + 1) * padding + offset, y * side + y * padding + offset + fontFudgeStart, x * side + side + (x + 1) * padding + offset, y * side + y * padding + side + offset + fontFudgeEnd);
				//    }
				//}

				gr.DrawImage(mapBoard, 0, 0);
			}

			picMap.Image = null;
			picMap.Image = mapBoard;
			bmpCloneMap = mapBoard.Clone() as Bitmap;

			//mapBoard.Save("baseMap.png", System.Drawing.Imaging.ImageFormat.png);
		}

		private void parseKey()
		{
			//string terrainHashes = "";
			Bitmap myBitmap = new Bitmap(@"Oil Barons Map\C64\1.png");
			int startX = 472, startY = 184, side = 16;

			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
			{
				for (int x = 0; x < 1; x++)
				{
					for (int y = 0; y < 13; y++)
					{
						Rectangle cloneRect = new Rectangle(startX + x * side, startY + y * side, side, side);
						System.Drawing.Imaging.PixelFormat format = myBitmap.PixelFormat;
						Bitmap cloneBitmap = myBitmap.Clone(cloneRect, format);

						picMap.Image = cloneBitmap;
						cloneBitmap.Save(y.ToString() + "_Terrain.png", System.Drawing.Imaging.ImageFormat.Png);
						//System.Threading.Thread.Sleep(1000);
						//Application.DoEvents();

						//ImageConverter converter = new ImageConverter();
						//var byteArray = (byte[])converter.ConvertTo(cloneBitmap, typeof(byte[]));
						//terrainHashes += String.Format("\"{0}\",{1}", Convert.ToBase64String(sha1.ComputeHash(byteArray)), Environment.NewLine);
					}
				}
			}
		}

		private void parseMaps()
		{
			for (int y = 0; y < 40; y++)
			{
				for (int x = 0; x < 50; x++)
				{
					map[x, y].terrain = Terrain.Unknown;
					map[x, y].id = x + y * 50;
				}
			}

			string hash;
			ImageConverter converter = new ImageConverter();

			//130,178 - 156,204
			for (int m = 1; m <= 20; m++)
			{
				Bitmap myBitmap = new Bitmap(String.Format(@"Oil Barons Map\C64\{0}.png", m));
				int startX = 136, startY = 184, side = 16, offset = 16;
				int yOffset = m <= 5 ? 0 : m <= 10 ? 10 : m <= 15 ? 20 : 30;
				int xOffset = 0;

				if (m == 1 | m == 6 | m == 11 | m == 16)
					xOffset = 0;
				if (m == 2 | m == 7 | m == 12 | m == 17)
					xOffset = 10;
				if (m == 3 | m == 8 | m == 13 | m == 18)
					xOffset = 20;
				if (m == 4 | m == 9 | m == 14 | m == 19)
					xOffset = 30;
				if (m == 5 | m == 10 | m == 15 | m == 20)
					xOffset = 40;

				using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
				{
					for (int y = 0; y < 10; y++)
					{
						for (int x = 0; x < 10; x++)
						{
							Rectangle cloneRect = new Rectangle(startX + x * offset + x * side, startY + y * offset + y * side, side, side);
							System.Drawing.Imaging.PixelFormat format = myBitmap.PixelFormat;
							Bitmap cloneBitmap = myBitmap.Clone(cloneRect, format);

							//if (m >= 1)
							//{
							//    pictureBox1.Image = cloneBitmap;
							//    //System.Threading.Thread.Sleep(1000);
							//    Application.DoEvents();
							//}

							var byteArray = (byte[])converter.ConvertTo(cloneBitmap, typeof(byte[]));
							hash = Convert.ToBase64String(sha1.ComputeHash(byteArray));

							bool found = false;
							for (int i = 0; i < TerrainHashes.Count(); i++)
							{
								if (hash == TerrainHashes[i])
								{
									map[x + xOffset, y + yOffset].terrain = (Terrain)i;
									//map[x + xOffset, y + yOffset].c64 = cloneBitmap;
									found = true;
									break;
								}
							}

							if (!found)
							{
								map[x + xOffset, y + yOffset].terrain = Terrain.Unknown;
							}
						}
					}
				}

				myBitmap.Dispose();
			}
		}

		private void parseParcels()
		{
			int startX = 0;
			int startY = 0;
			int side = 44;

			List<Parcel> parcels = new List<Parcel>();
			List<string> visited = new List<string>();

			using (Bitmap bmp = new Bitmap(System.Drawing.Image.FromFile(@"Oil Barons Map\Parcels2.png", true)))
			{
				for (int y = 0; y < 40; y++)
				{
					for (int x = 0; x < 50; x++)
					{
						int curX, curY;

						curX = startX + x * side;
						curY = startY + y * side + side / 2;
						Color leftSide = bmp.GetPixel(curX, curY);

						curX = startX + x * side + side - 1;
						curY = startY + y * side + side / 2;
						Color rightSide = bmp.GetPixel(curX, curY);

						curX = startX + x * side + side / 2;
						curY = startY + y * side;
						Color topSide = bmp.GetPixel(curX, curY);

						curX = startX + x * side + side / 2;
						curY = startY + y * side + side - 1;
						Color bottomSide = bmp.GetPixel(curX, curY);

						//map[x, y].id = id;
						map[x, y].loc = new Point(x, y);
						map[x, y].hasTopBorder = Color.FromArgb(255, 0, 0, 0) == topSide;
						map[x, y].hasLeftBorder = Color.FromArgb(255, 0, 0, 0) == leftSide;
						map[x, y].hasBottomBorder = Color.FromArgb(255, 0, 0, 0) == bottomSide;
						map[x, y].hasRightBorder = Color.FromArgb(255, 0, 0, 0) == rightSide;

						if (map[x, y].neighbors == null)
							map[x, y].neighbors = new List<int>();

						if (!map[x, y].hasBottomBorder)
							map[x, y].neighbors.Add(map[x, y + 1].id);
						if (!map[x, y].hasTopBorder)
							map[x, y].neighbors.Add(map[x, y - 1].id);
						if (!map[x, y].hasLeftBorder)
							map[x, y].neighbors.Add(map[x - 1, y].id);
						if (!map[x, y].hasRightBorder)
							map[x, y].neighbors.Add(map[x + 1, y].id);
					}
				}
			}

			for (int y = 0; y < 40; y++)
			{
				for (int x = 0; x < 50; x++)
				{
					if (parcels.Count == 0)
					{
						Parcel newParcel = new Parcel();
						newParcel.id = parcels.Count();
						newParcel.plotIds = new List<int>();
						newParcel.plotIds.Add(map[x, y].id);
						alreadyVisited.Add(map[x, y].id);
						addToParcel(newParcel, map[x, y].neighbors);
						parcels.Add(newParcel);
					}
					else
					{
						bool belongs = false;
						for (int i = 0; i < parcels.Count; i++)
						{
							Parcel parcel = parcels[i];
							if (belongsInCollection(parcel.plotIds, map[x, y].id))
							{
								belongs = true;
								alreadyVisited.Add(map[x, y].id);
								addToParcel(parcel, map[x, y].neighbors);
								break;
							}
						}

						if (!belongs)
						{
							belongs = false;
							//Check if any neighbor belongs in any parcel

							int j;
							for (j = 0; j < parcels.Count; j++)
							{
								foreach (var neighbor in map[x, y].neighbors)
								{
									belongs = belongsInCollection(parcels[j].plotIds, neighbor);
									if (belongs)
										break;
								}
							}

							if (!belongs)
							{
								Parcel newParcel = new Parcel();
								newParcel.id = parcels.Count();
								newParcel.plotIds = new List<int>();
								newParcel.plotIds.Add(map[x, y].id);

								alreadyVisited.Add(map[x, y].id);
								addToParcel(newParcel, map[x, y].neighbors);

								parcels.Add(newParcel);
							}
							else
							{
								foreach (var plot in map[x, y].neighbors)
								{
									if (!belongsInCollection(parcels[j].plotIds, plot))
									{
										parcels[j].plotIds.Add(plot);
									}
								}
							}
						}
					}
				}
			}

			int parcelId = 0;
			for (int i = 0; i < parcels.Count(); i++)
			{
				Parcel parcel = parcels[i];
				parcel.id = parcelId;
				parcel.ownedBy = -1;
				parcels[i] = parcel;

				foreach (int plotId in parcel.plotIds)
				{
					int x = plotId % 50;
					int y = plotId / 50;
					map[x, y].parentParcelId = parcelId;
					map[x, y].ownedBy = -1;
				}

				parcelId++;
			}

			string jsonMap = JsonConvert.SerializeObject(map, Formatting.None);
			File.WriteAllText("map.json", jsonMap);

			string jsonParcels = JsonConvert.SerializeObject(parcels.ToArray(), Formatting.None);
			File.WriteAllText("parcel.json", jsonParcels);

			drawTestParcels();
		}

		private void addToParcel(Parcel parcel, List<int> neighbors)
		{
			foreach (int neighbor in neighbors)
			{
				if (!alreadyVisited.Contains(neighbor))
				{
					if (!belongsInCollection(parcel.plotIds, neighbor))
					{
						parcel.plotIds.Add(neighbor);
						alreadyVisited.Add(neighbor);
					}
					GridPlot plot = getMapPlotFromId(neighbor);
					if (plot.neighbors != null)
					{
						if (plot.neighbors.Count > 0)
						{
							addToParcel(parcel, plot.neighbors);
						}
					}
				}
			}
		}

		private void drawTestParcels()
		{
			Bitmap test = new System.Drawing.Bitmap(1000, 800);
			using (Graphics gr = Graphics.FromImage(test))
			{
				gr.Clear(Color.White);

				Pen blackPen = new Pen(Color.Orange, 3);

				side = 16;
				for (int y = 0; y < 40; y++)
				{
					for (int x = 0; x < 50; x++)
					{
						//gr.DrawImage(map[x, y].c64, new Point(x * side, y * side));
						if (map[x, y].hasTopBorder)
							gr.DrawLine(blackPen, x * side, y * side, x * side + side, y * side);
						if (map[x, y].hasBottomBorder)
							gr.DrawLine(blackPen, x * side, y * side + side, x * side + side, y * side + side);
						if (map[x, y].hasLeftBorder)
							gr.DrawLine(blackPen, x * side, y * side, x * side, y * side + side);
						if (map[x, y].hasRightBorder)
							gr.DrawLine(blackPen, x * side + side, y * side, x * side + side, y * side + side);
					}
				}

				gr.DrawImage(test, 0, 0);
			}
			//gr.Dispose();

			picMap.Image = test;
		}

		private bool belongsInCollection(List<int> plotIds, int plotId)
		{
			bool belongs = false;
			belongs = plotIds.Contains(plotId);

			//foreach (var item in plots)
			//{
			//    if (item.id == plot.id)
			//    {
			//        belongs = true;
			//        break;
			//    }
			//}

			return belongs;
		}
#endregion

	}
}
