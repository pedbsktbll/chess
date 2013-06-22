﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;

namespace Chess
{
    class Player : IComparable<Player>
    {
		private int id;
		private int rank;
		private String name;
		private String office;
		private List<String> weeks;
		private List<int> playersBeaten;
		private List<int> playersLost;
		private List<int> playersTied;
		private double score;
		public int beatenPlayersRecord;
		private Player nextMatchup;

//		private List<Player> preferredPlayers;

		public Player( XmlTextReader reader )
        {
			weeks = new List<String>();
			playersBeaten = new List<int>();
			playersLost = new List<int>();
			playersTied = new List<int>();
			score = 0;
			beatenPlayersRecord = 0;
			nextMatchup = null;
//			preferredPlayers = new List<Player>();

			for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
			rank = Int32.Parse( reader.Value );

			for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
			id = Int32.Parse( reader.Value );

			for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
			name = reader.Value;

			for( reader.Read(); reader.NodeType != XmlNodeType.Text; reader.Read() ) ;
			office = reader.Value;

			for( reader.Read(); !reader.Name.Equals( "Row" ); reader.Read() )
			{
				if( reader.NodeType != XmlNodeType.Text )
					continue;
				String week = reader.Value;
				weeks.Add( week );

				//Win, Lose, or tie!
				if( week.StartsWith( "W" ) )
				{
					score++;
					playersBeaten.Add( Int32.Parse( week.Substring( 1 ) ) );
				}
				else if( week.StartsWith( "L" ) )
					playersLost.Add( Int32.Parse( week.Substring( 1 ) ) );
				else if( week.StartsWith( "T" ) )
				{
					score += 0.5;
					playersTied.Add( Int32.Parse( week.Substring( 1 ) ) );
				}
				else if( week.Equals( "BYE" ) )
					score += 0.5;
			}
		}

		public bool played(Player other)
		{
			if( other == null )
				return false;
			foreach( String s in weeks )
				if( s.EndsWith(other.id.ToString()) ) return true;
			return false;
		}

/*		public int beatenPlayersRecord()
		{
			foreach( String s in weeks )
			{

			}
			return 0;
		}
*/
/*		int IComparer.Compare( object a, object b )
		{
			Player c1 = (Player)a;
			Player c2 = (Player)b;

			if( c1.wins < c2.wins )
				return 1;
			if( c1.wins > c2.wins )
				return -1;
			else
				return 0;
		}*/
		public static Comparison<Player> idCompare = delegate( Player object1, Player object2 )
		{
			return object1.id.CompareTo( object2.id );
		};

		public int CompareTo( Player p )
		{
//			Player p = (Player)other;
			if( score > p.score )
				return -1;
			else if( p.score > score )
				return 1;
			else
			{
//				return 0;
//				int thisRec = beatenPlayersRecord();
//				int otherRec = p.beatenPlayersRecord();
				if( beatenPlayersRecord > p.beatenPlayersRecord )
					return -1;
				else if( p.beatenPlayersRecord > beatenPlayersRecord )
					return 1;
				else
				{
					if( rank > p.rank )
						return -1;
					else if( p.rank > rank )
						return 1;
					else
						return 0;
				}
			}
		}

		public int getScore()
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

		public string ToString()
		{
			return "#" + rank + " " + name;
		}

		public string writeXML()
		{
			string retString = "<Row>\r\n\t<Cell><Data ss:Type=\"Number\">" + rank + "</Data></Cell>\r\n"+
			"\t<Cell><Data ss:Type=\"Number\">" + id + "</Data></Cell>\r\n" +
			"\t<Cell><Data ss:Type=\"String\">" + name + "</Data></Cell>\r\n" +
			"\t<Cell><Data ss:Type=\"String\">" + office + "</Data></Cell>\r\n";

			foreach( String s in weeks )
				retString+="\t<Cell><Data ss:Type=\"String\">" + s + "</Data></Cell>\r\n";
			retString+="</Row>\r\n";
			return retString;
		}
    }
}
