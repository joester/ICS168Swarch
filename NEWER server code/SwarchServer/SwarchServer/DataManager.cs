using System;
using System.Data.SQLite;

namespace SwarchServer
{
    public class DataManager
    {
        public static SQLiteConnection swarchDatabase;


        public DataManager()
        {
            createSwarchDatabase();
            connectToDatabase();
			//createTables ();
            clearTable();
            //fillPlayerTable();
            printTable();
        }

        // Creates an empty database

        void createSwarchDatabase()
        {
            try
            {

                SQLiteConnection.CreateFile("SwarchDatabase.sqlite");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Creates a connection with our database

        void connectToDatabase()
        {
            swarchDatabase = new SQLiteConnection("Data Source=SwarchDatabase.db;Version=3;");
            swarchDatabase.Open();
        }

		// Create two tables

		// a table named 'playerInfo' with two columns: nae (a string of max 20 characters) and password(a string of max 50 characters)
        // a table named 'highscores' with two columns: name (a string of max 20 characters) and score (an int)

        void createTables()
        {
            string sql = "create table playerInfo (name varchar(20), password varchar(50))";
            SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
            command.ExecuteNonQuery();
      		sql = "create table highScores (name varchar(20), score int)";
            command = new SQLiteCommand (sql, swarchDatabase);
            command.ExecuteNonQuery ();
        }

		// Clear the tables

        public void clearTable()
        {
            string sql = "DELETE FROM playerInfo";
            SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
            SQLiteDataReader reader = command.ExecuteReader();
        }

		// print the tables to the console

        public void printTable()
        {
            string sql = "select * from playerInfo";
            SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tpassword: " + reader["password"]);
        }

		// check to see if a value exist in the table

		public bool existsInTable(String name)
        {
            string sql = "SELECT count(*) FROM playerInfo WHERE name=:Name";
            SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
            command.Parameters.AddWithValue(":Name", name);
            int count = Convert.ToInt32(command.ExecuteScalar());

            return (count != 0);
        }

		// get the user's password from the playerInfo database

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

		// insert a player into the playerInfo table

        public void insertIntoPlayer(string name, string password)
        {
            string sql = "select * from playerInfo where name =:name";
            SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
            command.Parameters.AddWithValue(":name", name);
            SQLiteDataReader dataReader = command.ExecuteReader();

            // if the name is in the database

            if (dataReader.Read())
            {
                Console.WriteLine("found: " + name);
                if (password.Equals(dataReader["password"]))
                {
                    Console.WriteLine("you have entered the correct password");
                }
                else
                {
                    Console.WriteLine("Invalid passowrd");
                }
            }
            else
            {
                sql = "insert into playerInfo (name, password) values(@param1, @param2)";
                command = new SQLiteCommand(sql, swarchDatabase);
                command.Parameters.Add(new SQLiteParameter("@param1", name));
                command.Parameters.Add(new SQLiteParameter("@param2", password));
                command.ExecuteNonQuery();

            }

        }

		public void insertIntoHighScores(string name, int score)
		{
			string sql = "insert into highScores (name, score) values(@param1, @param2)";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			command.Parameters.Add(new SQLiteParameter("@param1", name));
			command.Parameters.Add(new SQLiteParameter("@param2", score));
			command.ExecuteNonQuery();
		}

		public void printHighTable()
		{
			string sql = "select * from highScores";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			SQLiteDataReader reader = command.ExecuteReader();
			while (reader.Read())
				Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);
		}

		public void updateHighScores(string name, int score)
		{
			string sql = "UPDATE highScores SET score=@newScore WHERE name=@name";
			SQLiteCommand command = new SQLiteCommand(sql, swarchDatabase);
			command.Parameters.AddWithValue("@name", name);
			command.Parameters.AddWithValue("@newScore", score);
			try{
				command.ExecuteNonQuery();
			}catch(Exception fail){
				Console.WriteLine (fail.Message);
			}


			// if the name is in the database



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

