using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;


namespace WindowsFormsApplication1
{
    //Class Event:  to keep the data of an event, incluindy its title, date, start time, end time, and content
    class Event
    {
        string connStr = "server=localhost;user=root;database=csc340_db;port=3306;password=;SslMode=none;";
        string eventTitle;  //event title
        string eventDate;  //date when the event shall happen
        int eventStartTime;  //the start time of the event
        int eventEndTime;  //the end time of the event
        string eventContent;  //the description of the event


        //defalult constructor
        public Event()
        {
        }


        //constructor with all the data needed for construction a new event
        public Event(string s1, string s2, int s3, int s4, string s5)
        {
            eventTitle = s1;
            eventDate = s2;
            eventStartTime = s3;
            eventEndTime = s4;
            eventContent = s5;
        }


        //to check if the specified start time of an event is later than the specified end time
        //if yes, a time conflict hasb been dectected, and the function returns fales
        //otherwise, it returs true
        public bool checkStartEndTimesConflict(int startTimeIndex, int endTimeIndex)
        {
            if (endTimeIndex < startTimeIndex)
            {
                MessageBox.Show("The start time is later than the end time.  Please pick another end time.");
                return false;
            }
            return true;
        }


        //to check if an event has any time conflict with any other events on the same day
        //if yes, the function returns the first event that is conflicted with the specified event
        //otherwise, return a null value (which means the specified event is good to be saved or changed
        public Event checkEventConflict()
        {
                     
            ArrayList eventList = new ArrayList();  //an event list that will keep all the conflicted events
            DataTable myTable = new DataTable();  //for retriving conflicted events from datanase
            //string connStr = "server=localhost;user=root;database=csc340_db;port=3306;password=;SslMode=none;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                //retrieve all the conflicted events from the database
                string sql = "SELECT * FROM changEventTable WHERE date=@myDate AND ((startTime >= @myStartTime AND startTime <= @myEndTime) OR (endTime >= @myStartTime AND endTime <= @myEndTime)) ORDER BY startTime ASC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@myDate", eventDate);
                cmd.Parameters.AddWithValue("@myStartTime", eventStartTime);
                cmd.Parameters.AddWithValue("@myEndTime", eventEndTime);
                MySqlDataAdapter myAdapter = new MySqlDataAdapter(cmd);
                myAdapter.Fill(myTable);
                Console.WriteLine("Table is ready.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            //convert the retrieved data to Event objects and save them in the event list, if any
            foreach (DataRow row in myTable.Rows)
            {
                Event newEvent = new Event();
                newEvent.eventTitle = row["title"].ToString();
                newEvent.eventDate = row["date"].ToString();
                newEvent.eventStartTime = Int32.Parse(row["startTime"].ToString());
                newEvent.eventEndTime = Int32.Parse(row["endTime"].ToString());
                newEvent.eventContent = row["content"].ToString();
                eventList.Add(newEvent);
            }
            // if the event list is NOT empty, it means that at least one conflicted event exists, then return the first conflicted event
            if (eventList.Count > 0)
            {
                Console.WriteLine("The list is NOT empty!");
                return (Event) eventList[0];
            }
            else  //otherwise, no conflicted event exists; return a null value at the end of the function
            {
                Console.WriteLine("The list is EMPTY!");
            }
            return null;
        }
        

        //to save a revised event to the database
        //the idea is to keep two versions of the event: the original version (oldEvent) and the revised version (newEvent, which is "this" event)
        //delete the old event first, and check if any conflicted events with the new event exist
        //if no, save the new event to the database, and return a true value
        //if yes, save the old (original) event back to the database and return a false value
        public bool saveRevisedEvent(Event oldEvent)
        {
            oldEvent.deleteEvent();  //delete the original version of the modified event from the database
            //try to save the modified event to the database
            if (saveNewEvent() == false)  //cannot save the modified event to the database because of conflicted events
            {
                oldEvent.saveNewEvent();  //save the original version of the modified event back to the database
                return false;  //return false to indicate the failure of changing the event
            }
            return true;  //the modified event has been saved to the database successfully
        }


        //to save a new event to the database
        public bool saveNewEvent()
        {
            Event conflictEvent = checkEventConflict();  //check if any confliced event exists
            //if any conflicted event exists
            if (conflictEvent != null)
            {
                MessageBox.Show("There is a time conflict between this event and Event: " + conflictEvent.getTitle());  //display an error message with the conflicted event
                return false;  //return false to indicate the failure of svaing the event
            }

            //If no conflicted event exists, do the follows
            
            //prepare an SQL query to save the event
            //string connStr = "server=localhost;user=root;database=csc340_db;port=3306;password=;SslMode=none;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                string sql = "INSERT INTO changEventTable(date, title, startTime, endTime, content) VALUE(@date, @title, @startTime, @endTime, @content)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@date", eventDate);
                cmd.Parameters.AddWithValue("@title", eventTitle);
                cmd.Parameters.AddWithValue("@startTime", eventStartTime);
                cmd.Parameters.AddWithValue("@endTime", eventEndTime);
                cmd.Parameters.AddWithValue("@content", eventContent);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            return true;  //return true to indicate the success of saving the event to the database
        }


        //to delete a selected event from the database
        public void deleteEvent()
        {
            //prepare an SQL query to delete the event from the database
            //string connStr = "server=localhost;user=root;database=csc340_db;port=3306;password=;SslMode=none;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                string sql = "DELETE FROM changEventTable WHERE date=@date AND title=@title AND startTime=@startTime AND endTime=@endTime AND content=@content";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@date", eventDate);
                cmd.Parameters.AddWithValue("@title", eventTitle);
                cmd.Parameters.AddWithValue("@startTime", eventStartTime);
                cmd.Parameters.AddWithValue("@endTime", eventEndTime);
                cmd.Parameters.AddWithValue("@content", eventContent);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
        }


        //to retrieve the date of the event
        public string getDate()
        {
            return eventDate;
        }


        //to retrieve the title of the event
        public string getTitle()
        {
            return eventTitle;
        }


        //to retrieve the start time of the event
        public int getStartTime()
        {
            return eventStartTime;
        }


        //to retrieve the end time of the event
        public int getEndTime()
        {
            return eventEndTime;
        }


        //to retrieve the description of the event
        public string getContent()
        {
            return eventContent;
        }
        
        
        //to retrieve a sorted list of events on the same date based on their start time
        public ArrayList getEventList(string dateString)
        {
            ArrayList eventList = new ArrayList();  //a list to save the events
            //prepare an SQL query to retrieve all the events on the same, specified date
            DataTable myTable = new DataTable();
            //string connStr = "server=localhost;user=root;database=csc340_db;port=3306;password=;SslMode=none;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                string sql = "SELECT * FROM changEventTable WHERE date=@myDate ORDER BY startTime ASC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@myDate", dateString);
                MySqlDataAdapter myAdapter = new MySqlDataAdapter(cmd);
                myAdapter.Fill(myTable);
                Console.WriteLine("Table is ready.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            //convert the retrieved data to events and save them to the list
            foreach (DataRow row in myTable.Rows)
            {
                Event newEvent = new Event();
                newEvent.eventTitle = row["title"].ToString();
                newEvent.eventDate = row["date"].ToString();
                newEvent.eventStartTime = Int32.Parse(row["startTime"].ToString());
                newEvent.eventEndTime = Int32.Parse(row["endTime"].ToString());
                newEvent.eventContent = row["content"].ToString();
                eventList.Add(newEvent);
            }
            return eventList;  //return the event list
        }


        ////to retrieve a sorted list of events in the same month based on their start time
        public ArrayList getMonthlyEventList(string dateString)
        {
            ArrayList eventList = new ArrayList();  //a list to save the events
            //prepare an SQL query to retrieve all the events in the same, specified month
            DataTable myTable = new DataTable();
            //string connStr = "server=localhost;user=root;database=csc340_db;port=3306;password=;SslMode=none;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                string sql = "SELECT * FROM changEventTable WHERE date LIKE @myDate ORDER BY date ASC, startTime ASC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@myDate", "%"+dateString+"%");
                MySqlDataAdapter myAdapter = new MySqlDataAdapter(cmd);
                myAdapter.Fill(myTable);
                Console.WriteLine("Table is ready.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            //convert the retrieved data to events and save them to the list
            foreach (DataRow row in myTable.Rows)
            {
                Event newEvent = new Event();
                newEvent.eventTitle = row["title"].ToString();
                newEvent.eventDate = row["date"].ToString();
                newEvent.eventStartTime = Int32.Parse(row["startTime"].ToString());
                newEvent.eventEndTime = Int32.Parse(row["endTime"].ToString());
                newEvent.eventContent = row["content"].ToString();
                eventList.Add(newEvent);
            }
            return eventList;  //return the event list
        }
    }
}
