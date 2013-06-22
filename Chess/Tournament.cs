using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Chess
{
    class Tournament
    {
		private List<Player> players;

        public Tournament(String fileName)
        {
			players = new List<Player>();

			XmlTextReader reader = new XmlTextReader( fileName );
			bool firstRow = false;
            while( reader.Read() )
            {
				if( reader.NodeType != XmlNodeType.Element || !reader.Name.Equals( "Row" ) )
					continue;
				reader.Read();
				if( firstRow )
					players.Add( new Player( reader ) );
				else
				{
					for( ; !reader.Name.Equals( "Row" ); reader.Read() ) ;
					firstRow = true;
				}
            }
			reader.Close();

			players.Sort( Player.idCompare );
			foreach( Player p in players )
				foreach( int beatenPlayerID in p.getBeatenPlayers() )
					p.beatenPlayersRecord += players[beatenPlayerID-1].getNumWins();

			updateRankings();
		}

		private void updateRankings()
		{
			players.Sort();
			for( int i = 0; i < players.Count; i++ )
				players[i].setRank( i + 1 );
			return;
		}

		public void GenerateMatchups()
		{
//			updateRankings();
			List<List<Player>> mutExclLists = new List<List<Player>>();
			int currWinCt = -1;
			foreach( Player p in players )
			{
				if( p.getNumWins() != currWinCt )
				{
					mutExclLists.Add( new List<Player>() );
					currWinCt = p.getNumWins();
				}
				mutExclLists[mutExclLists.Count - 1].Add( p );
			}
/*
			for( int i = 0; i < mutExclLists.Count; i++ )
			{
				List<Player> list = mutExclLists[i];
				if( list.Count % 2 == 1 )
				{
					mutExclLists[i+1].Add(list[list.Count-1]);
					mutExclLists[i+1].Sort();
					list.RemoveAt(list.Count-1);
				}
				for( int j = 0, k = list.Count / 2 + 1; j < list.Count / 2 + 1; j++, k++ )
				{
					list[j].setNextMatchup( list[k] );
					list[k].setNextMatchup( list[j] );
				}
			}

			//Now let's fix the problem where players already played each other!
			for( int i = 0; i < mutExclLists.Count; i++ )
			{
				List<Player> list = mutExclLists[i];
				for( int j = 0, k = list.Count / 2 + 1; j < list.Count / 2 + 1; j++, k++ )
				{
					list[j].setNextMatchup( list[k] );
					list[k].setNextMatchup( list[j] );
				}
			}
*/
			for( int i = 0; i < mutExclLists.Count; i++ )
			{	
				List<Player> list = mutExclLists[i];
				for( int j = 0, k = list.Count / 2, incrementer = 1; list.Count > 1; )
				{
					//Preferred matchups
					if( j != k && !list[j].played( list[k] ) )
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
							if( !players[l].getNextMatchup().played( list[j] ) )
							{
								list[j].setNextMatchup( players[l].getNextMatchup() );
								list.RemoveAt( j );

								players[l].setNextMatchup( null );
								list.Add( players[l] );
							}
						}
					}
				}
				//Groping was uneven, let's move last member to next list.
				//Can't reach this in last list as long was we have even number of players.
				if( list.Count == 1 )
				{
					mutExclLists[i + 1].Add( list[0] );
					mutExclLists[i + 1].Sort();
					list.RemoveAt( 0 );
				}
			}
		}

		public void printMatchups(String dir)
		{
			System.IO.StreamWriter fileWriter = new System.IO.StreamWriter( dir + "\\newMatchups.txt" );
			fileWriter.WriteLine( "Player(WHITE) vs. Player(BLACK)" );
			List<Player> printed = new List<Player>();
			foreach( Player p in players )
			{
				if( printed.Contains( p ) )
					continue;
				string output = p.ToString() + " vs. " + p.getNextMatchup().ToString();
				fileWriter.WriteLine( output );
				printed.Add( p );
				printed.Add( p.getNextMatchup() );
			}
			fileWriter.Close();
		}

		public void printNewFile( String fileName )
		{
			int cols = 4 + players[0].getNumWeeks();
			int rows = players.Count + 1;
			System.IO.StreamWriter fileWriter = new System.IO.StreamWriter( fileName );
			fileWriter.Write( "<?xml version=\"1.0\"?>\r\n" +
							"<?mso-application progid=\"Excel.Sheet\"?>\r\n" +
							"<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n" +
							"xmlns:o=\"urn:schemas-microsoft-com:office:office\"\r\n" +
							"xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\r\n" +
							"xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\r\n" +
							"xmlns:html=\"http://www.w3.org/TR/REC-html40\">\r\n" +
							"<Worksheet ss:Name=\"AED Chess Tournamend\">\r\n" +
			"<Table ss:ExpandedColumnCount=\"" + cols + "\" ss:ExpandedRowCount=\"" + rows + "\">\r\n" +
//			"\" x:FullColumns=\"1\" x:FullRows=\"1\">\r\n" +
			   "\r\n<Row>\r\n" +
				"\t<Cell><Data ss:Type=\"String\">Rank</Data></Cell>\r\n" +
				"\t<Cell><Data ss:Type=\"String\">ID</Data></Cell>\r\n" +
				"\t<Cell><Data ss:Type=\"String\">Name</Data></Cell>\r\n" +
				"\t<Cell><Data ss:Type=\"String\">Office</Data></Cell>\r\n" +
				"\t<Cell><Data ss:Type=\"String\">Week 1</Data></Cell>\r\n" +
			   "</Row>\r\n" );

			foreach( Player p in players )
				fileWriter.Write(p.writeXML());

			fileWriter.Write( "\t\t</Table>\r\n\t</Worksheet>\r\n</Workbook>\r\n" );
			fileWriter.Close();
		}
    }
}
