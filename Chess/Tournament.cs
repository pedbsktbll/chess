using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;

namespace Chess
{
    class Tournament
    {
		private List<Player> players;

        public Tournament(String fileName)
        {
			players = new List<Player>();

			XmlTextReader reader = null;
			try
			{
				reader = new XmlTextReader(fileName);
				bool firstRow = false;
				int playerID = 1;
				while( reader.Read() )
				{
					if( reader.NodeType != XmlNodeType.Element || !reader.Name.Equals("Row") )
						continue;
					reader.Read();
					if( firstRow )
						players.Add(new Player(reader, playerID++));
					else
					{
						for( ; !reader.Name.Equals("Row"); reader.Read() )
						{
							if( reader.NodeType == XmlNodeType.Text )
								Player.columns.Add(reader.Value);
						}
						firstRow = true;
					}
				}
			}
			catch( System.Exception )
			{
				throw new Exception("Invalid Doc Format!");
			}
			finally
			{
				if( reader != null )
					reader.Close();
			}

			if( players.Count == 0 || players.Count % 2 != 0 )
				throw new Exception("Uneven number of players is unsupported!");
			if( players[0].getWeeks().Count >= players.Count - 1 )
				throw new Exception("Impossible to match players uniquely when number of rounds exceeds n - 1 players!");
			
			// Sums the wins of all beaten players
			players.Sort( Player.idCompare );
			foreach( Player p in players )
				foreach( int beatenPlayerID in p.getBeatenPlayers() )
					p.beatenPlayersRecord += players[beatenPlayerID - 1].getScore();

			updateRankings();
		}

		private void updateRankings()
		{
			int[] idToRank = new int[players.Count+1];

			// Sorts players by rank and updates their rank attribute
			players.Sort();
			for( int i = 0; i < players.Count; i++ )
			{
				idToRank[players[i].getID()] = i + 1;
				players[i].setRank(i + 1);
			}

			// Updates the rank of all players played to the new ranks
			foreach( Player p in players )
			{
				List<string> weeks = p.getWeeks();
				for( int i = 0; i < weeks.Count; i++ )
				{
					string str = weeks[i];
					int num;
					if( Int32.TryParse( str.Substring(1), out num ) )
						weeks[i] = str.Substring(0, 1) + idToRank[num];
				}
			}

			return;
		}

		public void GenerateMatchups()
		{
//			updateRankings();
			List<List<Player>> mutExclLists = new List<List<Player>>();
			double currWinCt = -1;
			// Creates lists of players for every unique score. IE) List for score of 10,9,8...0
			foreach( Player p in players )
			{
				if (p.getScore() != currWinCt)
				{
					mutExclLists.Add( new List<Player>() );
					currWinCt = p.getScore();
				}
				mutExclLists[mutExclLists.Count - 1].Add( p );
			}

			for( int i = 0; i < mutExclLists.Count; i++ )
			{	
				List<Player> list = mutExclLists[i];
				for( int j = 0, k = list.Count / 2, incrementer = 1; list.Count > 1; )
				{
					//Preferred matchups
					if( j != k && !list[j].played( list[k], true ) )
					{
						list[j].setNextMatchup( list[k] );
						list[k].setNextMatchup( list[j] );
						list.RemoveAt( k );
						list.RemoveAt( j );
						k = list.Count / 2;
						incrementer = 1;
						continue;
					}

					//Otherwise let's try the next guy:
					k+=incrementer;
					if( k < list.Count && k > j )
						continue;
					//We reached end of list, let's go back to half the list and go upwards:
					else if( k == list.Count )
					{
						k = list.Count / 2 - 1;
						incrementer = -1;
					}
					//We reached beginning of list, j cannot play any opponents in this grouping
					else if( i + 1 < mutExclLists.Count )
					{
						mutExclLists[i + 1].Add( list[j] );
						mutExclLists[i + 1].Sort();
						list.RemoveAt( j );
						k = list.Count / 2;
						incrementer = 1;
					}
					//This only executes if the final list contains all players who have played each other
					else
					{
						j = 0;
						k = list.Count / 2 + 1;
						incrementer = 1;
						//now what???
						for( int l = players.Count - 1; l >= 0; l-- )
						{
							if( list.Contains( players[l] ) )
								continue;
							if( !players[l].getNextMatchup().played( list[j], false ) )
							{
								list[j].setNextMatchup( players[l].getNextMatchup() );
								list.RemoveAt( j );

								players[l].setNextMatchup( null );
								list.Add( players[l] );
								list.Sort();
								j = 0;
								k = list.Count / 2;
								incrementer = 1;
								break;
							}
						}
					}
				}
				//Grouping was uneven, let's move last member to next list.
				//Can't reach this in last list as long was we have even number of players.
				if( list.Count == 1 )
				{
					mutExclLists[i + 1].Add( list[0] );
					mutExclLists[i + 1].Sort();
					list.RemoveAt( 0 );
				}
			}
		}

		public bool printMatchups(String dir)
		{
			System.IO.StreamWriter fileWriter = new System.IO.StreamWriter( dir + "\\newMatchups.txt" );
			fileWriter.WriteLine( "Player(WHITE) vs. Player(BLACK)" );
			Random rand = new Random((int)DateTime.Now.Ticks);

			HashSet<int> printed = new HashSet<int>();
			bool retVal = true;
			foreach( Player p in players )
			{
				if( printed.Contains( p.getID() ) )
					continue;
				int playAsWhite = rand.Next(2);// % 2;
				string output = (playAsWhite == 1 ? p.ToString() : p.getNextMatchup().ToString()) + " vs. " + (playAsWhite == 1 ? p.getNextMatchup().ToString() : p.ToString());
				fileWriter.WriteLine( output );
				retVal &= printed.Add( p.getID() );
				retVal &= printed.Add( p.getNextMatchup().getID() );
			}
			fileWriter.Close();
			return retVal && printed.Count == players.Count ? true : false;
		}

		public void printNewFile( String fileName )
		{
			int cols = Player.columns.Count;
			int rows = players.Count + 1;

			string ss = "urn:schemas-microsoft-com:office:spreadsheet";
			XmlTextWriter writer = new XmlTextWriter( fileName, System.Text.Encoding.Default );
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
			writer.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
			writer.WriteStartElement("Workbook");
			writer.WriteAttributeString("xmlns", "",	"http://www.w3.org/2000/xmlns/", "urn:schemas-microsoft-com:office:spreadsheet"); //writer.WriteRaw("\r\n");
			writer.WriteAttributeString("xmlns", "o",	"http://www.w3.org/2000/xmlns/", "urn:schemas-microsoft-com:office:office"); //writer.WriteRaw("\r\n");
			writer.WriteAttributeString("xmlns", "x",	"http://www.w3.org/2000/xmlns/", "urn:schemas-microsoft-com:office:excel"); //writer.WriteRaw("\r\n");
			writer.WriteAttributeString("xmlns", "ss", "http://www.w3.org/2000/xmlns/", ss);
			writer.WriteAttributeString("xmlns", "html","http://www.w3.org/2000/xmlns/", "http://www.w3.org/TR/REC-html40");
			
			writer.WriteStartElement("Worksheet");
			writer.WriteAttributeString("ss", "Name", ss, "AED Chess Tournament");
			writer.WriteStartElement("Table");
			writer.WriteAttributeString("ss", "ExpandedColumnCount", ss, cols + "");
			writer.WriteAttributeString("ss", "ExpandedRowcount", ss, rows + "");

			writer.WriteStartElement("Row");
			if( (Player.columns.FindIndex( x => x.Equals( "Rank", StringComparison.OrdinalIgnoreCase ) )) < 0 )
				Player.columns.Insert( 0, "Rank" );
			if( ( Player.columns.FindIndex( x => x.Equals( "Name", StringComparison.OrdinalIgnoreCase ) ) ) < 0 )
				Player.columns.Insert( 0, "Name" );
			if( ( Player.columns.FindIndex( x => x.Equals( "Team", StringComparison.OrdinalIgnoreCase ) ) ) < 0 )
				Player.columns.Insert( 0, "Team" );
			if( ( Player.columns.FindIndex( x => x.Equals( "Total Points", StringComparison.OrdinalIgnoreCase ) ) ) < 0 )
				Player.columns.Insert( 0, "Total Points" );

			foreach( String s in Player.columns )
			{
				writer.WriteStartElement( "Cell" );
				writer.WriteStartElement( "Data" );
				writer.WriteAttributeString( "ss", "Type", ss, "String" );
				writer.WriteString( s );
				writer.WriteEndElement();
				writer.WriteEndElement();
			}

			writer.WriteEndElement(); //END Row

			foreach( Player p in players )
 				p.writeXML(writer, ss);

			writer.WriteEndElement(); //END Table
			writer.WriteEndElement(); //END Worksheet
			writer.WriteEndElement(); //END Workbook
			writer.WriteEndDocument();//END Document
			writer.Close();
		}
    }
}
