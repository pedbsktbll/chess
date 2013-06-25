using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;

namespace Chess
{
    class Player : IComparable<Player>
    {
		public static List<String> columns = new List<String>();

		private int id;
		private int rank;
		private String name;
		private String office;
		private String team;
		private List<String> weeks;
		private List<int> playersBeaten;
 		private List<int> playersLost;
 		private List<int> playersTied;
		private double score;
		private Player nextMatchup;
		private List<String> rowValues;
//		private List<Player> preferredPlayers;

		public double beatenPlayersRecord;

		public bool played(Player other, bool includeTeam = true);

		public Player( XmlTextReader reader, int id )
        {
			weeks = new List<String>();
			rowValues = new List<String>();
			playersBeaten = new List<int>();
 			playersLost = new List<int>();
 			playersTied = new List<int>();
			score = 0;
			beatenPlayersRecord = 0;
			nextMatchup = null;
//			preferredPlayers = new List<Player>();

			this.id = id;

			// Either assume Rank, Name, Team or use columns (preferred):
			if( columns.Count > 0 )
			{
				for( int i = 0; i < columns.Count; i++ )
				{
					for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
					string value = reader.Value;
					rowValues.Add(value);

					if( columns[i].ToLower().Equals("rank") )
						rank = Int32.Parse(value);
					else if( columns[i].ToLower().Equals("name") )
						name = reader.Value;
					else if( columns[i].ToLower().Equals("office") )
						office = reader.Value;
					else if( columns[i].ToLower().Equals("team") )
						team = reader.Value;
					else if( columns[i].ToLower().Equals("id") )
						this.id = Int32.Parse(reader.Value);
					else if( columns[i].ToLower().StartsWith("round") )
						addToScore(value.ToLower());
				}
			}
			else
			{
				// Assume first entry is Rank
				for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
				rank = Int32.Parse(reader.Value);

				// IF second entry is a number, assume it's the player's ID-- To support the older version
				for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
				int temp;
				if( Int32.TryParse(reader.Value, out temp) )
				{
					this.id = temp;
					for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
				}
				// OTHERWISE, assume second entry is name
				name = reader.Value;

				// Assume third entry is Team
				for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
				team = reader.Value;

				// Total Points is ignored:
				for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;

				// For each round:
				for( reader.Read(); !reader.Name.Equals("Row"); reader.Read() )
				{
					if( reader.NodeType != XmlNodeType.Text )
						continue;
					addToScore(reader.Value.ToLower());
				}
			}
		}

		// Assumes "week" is ToLower()
		public void addToScore(String week)
		{
			//Win, Lose, or tie!
			if( week.StartsWith("w") )
			{
				score++;
				if( week.Length > 1 )
					playersBeaten.Add(Int32.Parse(week.Substring(1)));
			}
			else if( week.StartsWith("l") )
			{
				if( week.Length > 1 )
					playersLost.Add(Int32.Parse(week.Substring(1)));
			}
			else if( week.StartsWith("t") )
			{
				score += 0.5;
				if( week.Length > 1 )
					playersTied.Add(Int32.Parse(week.Substring(1)));
			}
			else if( week.Equals("bye") )
				score += 0.5;
			// If not one of the above, then this data must have been invalid
			else
				return;

			weeks.Add(week);
		}

		public bool played(Player other, bool includeTeam /*= true*/)
		{
			if( other == null )
				return false;
			if( includeTeam && team != null && other.team != null && team.ToLower().Equals(other.team.ToLower()) )
				return true;
			foreach( int i in playersBeaten )
				if( i == other.rank ) return true;
			foreach( int i in playersLost )
				if( i == other.rank ) return true;
			foreach( int i in playersTied )
				if( i == other.rank ) return true;
// 			foreach (String s in weeks)
// 				if (s.Substring(1).Equals(other.rank.ToString())) return true;
			return false;
		}

		// Sorts players by id
		public static Comparison<Player> idCompare = delegate( Player object1, Player object2 )
		{
			return object1.id.CompareTo( object2.id );
		};

		// Compare scores, then versus record, then beaten player's record (Buchholz chess rating), then current rank
		public int CompareTo( Player p )
		{
			// Score:
			if( score > p.score )
				return -1;
			else if( p.score > score )
				return 1;
			else
			{
				// Versus record:
				foreach( int i in this.playersBeaten )
					if( p.id == i )
						return -1;
				foreach( int i in p.playersBeaten )
					if( this.id == i )
						return 1;

				// players beaten:
				if( beatenPlayersRecord > p.beatenPlayersRecord )
					return -1;
				else if( p.beatenPlayersRecord > beatenPlayersRecord )
					return 1;
				else
				{
					// Current rank:
					if( rank < p.rank )
						return -1;
					else if( p.rank < rank )
						return 1;
					else
						return 0;
				}
			}
		}

		public double getScore()
		{
			return score;
		}
		public List<int> getBeatenPlayers()
		{
			return playersBeaten;
		}

		public void setNextMatchup( Player p )
		{
			nextMatchup = p;
		}

		public void setRank( int r )
		{
			rank = r;
		}

		public Player getNextMatchup()
		{
			return nextMatchup;
		}
/*		public void addPreferred( Player p )
		{
			preferredPlayers.Add( p );
		}*/

		public int getNumWeeks()
		{
			return weeks.Count;
		}

		public int getID()
		{
			return id;
		}

		public List<string> getWeeks()
		{
			return weeks;
		}

		public string ToString()
		{
			return "#" + rank + " " + name;
		}

		public string writeXML()
		{
			string retString = "<Row>\r\n\t<Cell><Data ss:Type=\"Number\">" + rank + "</Data></Cell>\r\n"+
			"\t<Cell><Data ss:Type=\"String\">" + name + "</Data></Cell>\r\n" +
			"\t<Cell><Data ss:Type=\"Number\">" + score + "</Data></Cell>\r\n";

			foreach (String s in weeks)
				retString += "\t<Cell><Data ss:Type=\"String\">" + s + "</Data></Cell>\r\n";

			retString+="</Row>\r\n";
			return retString;
		}

		public void writeXML(XmlTextWriter writer, string ss)
		{
			writer.WriteStartElement("Row");

			// Either assume Rank, Name, Team or use columns (preferred):
			if( columns.Count > 0 )
			{
				int roundNum = 0;
				for( int i = 0; i < columns.Count; i++ )
				{
					if( columns[i].ToLower().Equals("rank") )
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "Number");
						writer.WriteValue(rank);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
					else if( columns[i].ToLower().Equals("name") )
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "String");
						writer.WriteValue(name);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
					else if( columns[i].ToLower().Equals("office") )
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "String");
						writer.WriteValue(office);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
					else if( columns[i].ToLower().Equals("team") )
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "String");
						writer.WriteValue(team);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
					else if( columns[i].ToLower().Equals("id") )
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "Number");
						writer.WriteValue(id);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
					else if( columns[i].ToLower().StartsWith("round") )
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "String");
						writer.WriteValue(weeks[roundNum++]);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
					else
					{
						writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
						writer.WriteAttributeString("ss", "Type", ss, "String");
						writer.WriteValue(rowValues[i]);
						writer.WriteEndElement(); writer.WriteEndElement();
					}
				}
			}
			else
			{
				// Rank
				writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
				writer.WriteAttributeString("ss", "Type", ss, "Number");
				writer.WriteValue(rank);
				writer.WriteEndElement(); writer.WriteEndElement();

				// Name
				writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
				writer.WriteAttributeString("ss", "Type", ss, "String");
				writer.WriteValue(name);
				writer.WriteEndElement(); writer.WriteEndElement();

				// Team
				writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
				writer.WriteAttributeString("ss", "Type", ss, "String");
				writer.WriteValue(team);
				writer.WriteEndElement(); writer.WriteEndElement();

				// Score
				writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
				writer.WriteAttributeString("ss", "Type", ss, "Number");
				writer.WriteValue(score);
				writer.WriteEndElement(); writer.WriteEndElement();

				foreach( String s in weeks )
				{
					writer.WriteStartElement("Cell"); writer.WriteStartElement("Data");
					writer.WriteAttributeString("ss", "Type", ss, "String");
					writer.WriteValue(s);
					writer.WriteEndElement(); writer.WriteEndElement();
				}
			}

			writer.WriteEndElement(); // END Row
		}

//		private void writeXMLCell(XmlTextWriter writer, string ss, string varType, )
    }
}
