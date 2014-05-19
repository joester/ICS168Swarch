﻿using System;
using System.Data.SQLite;

namespace SwarchServer
{
	public class DataManager
	{
		public static SQLiteConnection swarchDatabase;


		public DataManager ()
		{
			createSwarchDatabase ();
			connectToDatabase ();
			//createTables ();
			printInfoTable ();
		}

		// Creates an empty database file
		void createSwarchDatabase()
		{
			try
			{

				SQLiteConnection.CreateFile("SwarchDatabase.sqlite");
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		// Creates a connection with our database file.
		void connectToDatabase()
		{
			swarchDatabase = new SQLiteConnection("Data Source=SwarchDatabase.db;Version=3;");
			swarchDatabase.Open();
		}

		// Creates a table named 'highscores' with two columns: name (a string of max 20 characters) and score (an int)
		void createTables()
		{
			string sql = "create table playerInfo (name varchar(20), password varchar(50))";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			command.ExecuteNonQuery();
			sql = "create table highScores (name varchar(20), score int)";
			command = new SQLiteCommand (sql, swarchDatabase);
			command.ExecuteNonQuery ();
		}

		public void clearTable()
		{
			string sql = "DELETE FROM playerInfo";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			SQLiteDataReader reader = command.ExecuteReader();
		}

		public void printInfoTable()
		{
			string sql = "select * from playerInfo";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			SQLiteDataReader reader = command.ExecuteReader();
			while (reader.Read())
				Console.WriteLine("Name: " + reader["name"] + "\tpassword: " + reader["password"]);
		}

		public bool existsInInfoTable(String name)
		{
			string sql = "SELECT count(*) FROM playerInfo WHERE name=:Name";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			command.Parameters.AddWithValue(":Name", name);
			int count = Convert.ToInt32(command.ExecuteScalar());

			return (count != 0);
		}

		public string getUserPassword(String name)
		{
			string sql = "select password from playerInfo where name=" + "'" + name + "'";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			SQLiteDataReader reader = command.ExecuteReader();
			return reader["password"].ToString();
		}

		public void getTableEntry(String name)
		{
			string sql = "select * from playerInfo where name=" + "'" + name + "'";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			SQLiteDataReader reader = command.ExecuteReader();
			Console.WriteLine(reader["name"] + " : " + reader["password"]);

		}

		public void insertIntoInfoTable(string name, string password)
		{
			string sql = "select * from playerInfo where name =:name";
			SQLiteCommand command = new SQLiteCommand (sql, swarchDatabase);
			command.Parameters.AddWithValue (":name", name);
			SQLiteDataReader dataReader = command.ExecuteReader ();

			// if the name is in the database

			if (dataReader.Read ()) {
				Console.WriteLine ("found: " + name);
				if (password.Equals(dataReader ["password"])) {
					Console.WriteLine ("you have entered the correct password");
				} else {
					Console.WriteLine ("Invalid passowrd");
				}
			} 
			else {
				sql = "insert into playerInfo (name, password) values(@param1, @param2)";
				command = new SQLiteCommand (sql, swarchDatabase);
				command.Parameters.Add (new SQLiteParameter ("@param1", name));
				command.Parameters.Add (new SQLiteParameter ("@param2", password));
				command.ExecuteNonQuery ();

			}

		}

		public void insertIntoHighscoresTable(string name, int weight)
		{
			string sql = "insert into highScores (name, score) values(@param1, @param2)";
			SQLiteCommand command = new SQLiteCommand (sql, swarchDatabase);
			command.Parameters.Add (new SQLiteParameter ("@param1", name));
			command.Parameters.Add (new SQLiteParameter ("@param2", weight));
			command.ExecuteNonQuery ();
			sql = "SELECT * FROM `tablename` ORDER BY `columnname`";
			command = new SQLiteCommand (sql, swarchDatabase);
			command.ExecuteNonQuery ();

		}



		// Inserts some values in the highscores table.
		// As you can see, there is quite some duplicate code here, we'll solve this in part two.
		void fillPlayerTable()
		{
			string sql = "insert into playerInfo  (name, password) values ('Jay', '39ec785d60a1b23bfda9944b9138bbcf')";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			command.ExecuteNonQuery();
			sql = "insert into playerInfo  (name, password) values ('Me', 3232 )";
			command = new SQLiteCommand(sql, swarchDatabase);
			command.ExecuteNonQuery();
			sql = "insert into playerInfo  (name, password) values ('Not me', 30011)";
			command = new SQLiteCommand(sql, swarchDatabase);
			command.ExecuteNonQuery();
		}

	}
}

