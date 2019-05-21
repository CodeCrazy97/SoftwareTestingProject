using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        ArrayList eventList = new ArrayList();  //a local list to ssave events of a specified date or month to be referred by all the controls in the form
        string selectedDate = "";  //the date selected by the user
        int selectedEventIndex = -1;  //the indexing number of the selected event in the event list
        bool monthlyEventMode = false;  //whether the display is to show all the events in the same month or just in a particular day


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //initialize the default values of variables used in the system when the form is called and loaded
            DateTime thisDay = DateTime.Today;  //get today's date
            string myString = thisDay.ToShortDateString();  //get rid of time, just keep the date information
            eventList.Clear();  //empty the event list at the beginning
            myString = convertDateFormat(myString);  //change the date's format to the format that is used for date data in MySQL database  mm/dd/yyyy
            selectedDate = myString;  //set the selected date to today's date
            textBox2.Text = myString;  //display today's date in the form at the beginning

            //get the list of events for today
            Event aEvent = new Event();
            eventList = aEvent.getEventList(myString);  //retrieve a list of today's events from the database
            listBox1.Items.Clear();
            //list and display the list of events in the form
            foreach (Event nextEvent in eventList)
            {
                string eventIndex = findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                listBox1.Items.Add(eventIndex);
            }
            //display the frst event of the list in the form
            if (eventList.Count >= 1)
            {
                Event thisEvent = eventList[0] as Event;
                textBox1.Text = thisEvent.getTitle();
                textBox2.Text = thisEvent.getDate();
                comboBox1.SelectedIndex = thisEvent.getStartTime();
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = thisEvent.getEndTime();
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = thisEvent.getContent();
            }
            
        }


        //to convert the format mm/dd/yyyy of a date in Visual Studio to the format yyyy-mm-dd that is used in MySQL database
        private string convertDateFormat(string dateString)
        {
            string myString = dateString;
            string mm;
            string dd;
            eventList.Clear();
            //get mm
            if (myString[1] != '/')  //if month is 10, 11, or 12
            {
                mm = myString.Substring(0, 2);
                myString = myString.Substring(3);
            }
            else  //if month is 1, 2, ...8, 9
            {
                mm = "0" + myString.Substring(0, 1);
                myString = myString.Substring(2);
            }
            //get dd
            if (myString[1] != '/')  //if date is 10, 11, 12, ..., 30, 31
            {
                dd = myString.Substring(0, 2);
                myString = myString.Substring(3);
            }
            else  //if the date is 1, 2, ..., 8, 9
            {
                dd = "0" + myString.Substring(0, 1);
                myString = myString.Substring(2);
            }
            //form the date in the format of yyyy-mm-dd
            myString = myString + "-" + mm + "-" + dd;
            return myString;
        }


        //to display a list of events on a selected date chosen from the monthCalendar control by the user
        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            //change the selected date's format to yyyy-mm-dd
            monthlyEventMode = false;  //just display the events on the selected date, not in the whole month
            string myString = monthCalendar1.SelectionRange.Start.ToShortDateString();
            eventList.Clear();
            myString = convertDateFormat(myString);
            selectedDate = myString;
            textBox2.Text = myString;
            //retrieve the list of events on the selected date from the database
            Event aEvent = new Event();
            eventList = aEvent.getEventList(myString);
            //display the list of events in the form with the listBox control
            listBox1.Items.Clear();
            foreach (Event nextEvent in eventList)
            {
                string eventIndex = findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                listBox1.Items.Add(eventIndex);
            }
            //dosplay the first event of the list in the form with repevant controls
            if (eventList.Count == 0)  //if no events on the selected date, display nothing
            {
                textBox1.Text = "";
                textBox2.Text = "";
                comboBox1.SelectedIndex = 0;
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = 0;
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = "";

                selectedEventIndex = -1;

            }
            else  //otherwise, display the first event in the list
            {
                Event thisEvent = eventList[0] as Event;
                textBox1.Text = thisEvent.getTitle();
                textBox2.Text = thisEvent.getDate();
                comboBox1.SelectedIndex = thisEvent.getStartTime();
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = thisEvent.getEndTime();
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = thisEvent.getContent();

                selectedEventIndex = 0;
            }
        }


        //to display the selected event from the list in the form with relevant controls
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            selectedEventIndex = selectedIndex;
            if (selectedIndex >= 0)  //make sure that some event has been selected
            {
                Event thisEvent = eventList[selectedIndex] as Event;
                textBox1.Text = thisEvent.getTitle();
                textBox2.Text = thisEvent.getDate();
                comboBox1.SelectedIndex = thisEvent.getStartTime();
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = thisEvent.getEndTime();
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = thisEvent.getContent();
            }
        }


        //to display all the events of the selected month
        private void button5_Click(object sender, EventArgs e)
        {
            monthlyEventMode = true;  //change the mode to display the monthly events
            string selectedMonth = selectedDate.Substring(0, 7);  //read the selected month
            //retrieve the monthly events from the data and save them to the event list
            eventList.Clear();
            Event aEvent = new Event();
            eventList = aEvent.getMonthlyEventList(selectedMonth);
            //display the monthly events in the form with the eventList control
            listBox1.Items.Clear();
            foreach (Event nextEvent in eventList)
            {
                string eventIndex = nextEvent.getDate().Substring(5) + "    " + findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                listBox1.Items.Add(eventIndex);
            }
            //display the first monthly event in the form with relevant controls
            if (eventList.Count == 0)  //if there is no event in the selected month, then display nothing
            {
                textBox1.Text = "";
                textBox2.Text = "";
                comboBox1.SelectedIndex = 0;
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = 0;
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = "";
            }
            else  //otherwise, display the first monthly event
            {
                Event thisEvent = eventList[0] as Event;
                textBox1.Text = thisEvent.getTitle();
                textBox2.Text = thisEvent.getDate();
                comboBox1.SelectedIndex = thisEvent.getStartTime();
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = thisEvent.getEndTime();
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = thisEvent.getContent();
            }
        }


        //to create and save a new event to the database
        private void button6_Click(object sender, EventArgs e)
        {
            //create a new event
            Event aNewEvent = new Event(textBox1.Text, textBox2.Text, comboBox1.SelectedIndex, comboBox2.SelectedIndex, richTextBox1.Text);
            //save the event to the database and check if it is successful
            bool successfulSave = aNewEvent.saveNewEvent();
            if (successfulSave == false)  //if the action, saving the event to the database, fails, do nothing
            {
                return;
            }
            //if the action of saving the new event to the database is successful, set the form to its original setting
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = false;
            button7.Visible = false;

            textBox1.ReadOnly = true;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            richTextBox1.ReadOnly = true;

            //retrieve the event lists from the database again, including the NEW event just created
            eventList.Clear();
            Event aEvent = new Event();
            eventList = aEvent.getEventList(selectedDate);
            //display the event list in the form with the list box control
            listBox1.Items.Clear();
            foreach (Event nextEvent in eventList)
            {
                string eventIndex = findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                listBox1.Items.Add(eventIndex);
                if (textBox1.Text == nextEvent.getTitle() && comboBox1.SelectedIndex == nextEvent.getStartTime() && comboBox2.SelectedIndex == nextEvent.getEndTime())
                {
                    selectedEventIndex = listBox1.Items.Count - 1;
                }
            }
        }


        //to set the form to the mode for creating a new event on the selected date
        private void button3_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = true;
            button7.Visible = true;

            textBox1.ReadOnly = false;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            richTextBox1.ReadOnly = false;

            textBox1.Text = "";
            textBox2.Text = selectedDate;
            comboBox1.SelectedIndex = 0;
            comboBox1.Text = comboBox1.SelectedItem.ToString();
            comboBox2.SelectedIndex = 0;
            comboBox2.Text = comboBox2.SelectedItem.ToString();
            richTextBox1.Text = "";

        }


        //to ignore any changes and set the form back to its original setting
        private void button7_Click(object sender, EventArgs e)
        {
            button1_Click_1(sender, e);
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;

            textBox1.ReadOnly = true;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            richTextBox1.ReadOnly = true;
        }


        //to set the form to the mode for deleting a selected event
        private void button4_Click(object sender, EventArgs e)
        {
            if (eventList.Count == 0)  //if no selected event, which mean no selected event and the event list is empty, then do nothing
                return;
            button1_Click_1(sender, e);
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button7.Visible = true;
            button8.Visible = true;
        }


        //to reload the original selected event to the form, including the list of selected events to be displayed with the listBox control
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (eventList.Count == 0)  //if the event list contains nothing, which means there is not event on the selected date, then simply do nothing (nothing to reload)
                return;

            eventList.Clear();
            Event aEvent = new Event();
            //check to load the events for the selected date or for the selected month (daily events or monthly events)
            if (monthlyEventMode == false)
            {
                eventList = aEvent.getEventList(selectedDate);
            }
            else
            {
                string selectedMonth = selectedDate.Substring(0, 7);
                eventList = aEvent.getMonthlyEventList(selectedMonth);
            }

            //retrieve a list of events (daily or month;ly) from the database, and display them in the form with the listBox contrl
            listBox1.Items.Clear();
            foreach (Event nextEvent in eventList)
            {
                string eventIndex;
                if (monthlyEventMode == false)
                    eventIndex = findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                else
                    eventIndex = nextEvent.getDate().Substring(5) + "    " + findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                listBox1.Items.Add(eventIndex);
                if (textBox1.Text == nextEvent.getTitle() && comboBox1.SelectedIndex == nextEvent.getStartTime() && comboBox2.SelectedIndex == nextEvent.getEndTime())
                {
                    selectedEventIndex = listBox1.Items.Count - 1;
                    Console.WriteLine("index number is " + selectedEventIndex);
                }
            }
            //reload the selected event from the event list, and display it in the form with relevant controls
            if (selectedEventIndex >= 0)
            {
                Console.WriteLine("before: " + selectedEventIndex + " Event count: " + eventList.Count);
                Console.WriteLine("Here is selectedEventIndex: " + selectedEventIndex);
                Event thisEvent = eventList[selectedEventIndex] as Event;
                Console.WriteLine("after: " + selectedEventIndex);
                textBox1.Text = thisEvent.getTitle();
                textBox2.Text = thisEvent.getDate();
                comboBox1.SelectedIndex = thisEvent.getStartTime();
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = thisEvent.getEndTime();
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = thisEvent.getContent();
            }
        }


        //to make sure that the user wish to delete the selected event displayed in the form with relevant controls, and set the form to its original setting
        private void button8_Click(object sender, EventArgs e)
        {
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button7.Visible = false;
            button8.Visible = false;

            if (eventList.Count > 0 && textBox1.Text != "") //if a valid event was selected for deletion
            {
                //delete the selected event from the datbase
                Event aDeletedEvent = new Event(textBox1.Text, textBox2.Text, comboBox1.SelectedIndex, comboBox2.SelectedIndex, richTextBox1.Text);
                aDeletedEvent.deleteEvent();
                eventList.Clear();
                Event aEvent = new Event();
                //retrteiev an event list from the database, which should not include the deleted event (it's been deleted)
                if (monthlyEventMode == false)
                {
                    eventList = aEvent.getEventList(selectedDate);
                }
                else
                {
                    string selectedMonth = selectedDate.Substring(0, 7);
                    eventList = aEvent.getMonthlyEventList(selectedMonth);
                }
                //display the event list in the form with the listBox control
                listBox1.Items.Clear();
                foreach (Event nextEvent in eventList)
                {
                    string eventIndex;
                    if (monthlyEventMode == false)
                        eventIndex = findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                    else
                        eventIndex = nextEvent.getDate().Substring(5) + "    " + findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                    listBox1.Items.Add(eventIndex);
                    if (textBox1.Text == nextEvent.getTitle() && comboBox1.SelectedIndex == nextEvent.getStartTime() && comboBox2.SelectedIndex == nextEvent.getEndTime())
                    {
                        selectedEventIndex = listBox1.Items.Count - 1;
                        Console.WriteLine("index number is " + selectedEventIndex);
                    }
                }
                /*
                //displat the first event in the list in the form with relevant controls
                if (selectedEventIndex >= 0)  //if the event list contains at least one event, then display the first event
                {
                    Event thisEvent = eventList[selectedEventIndex] as Event;
                    textBox1.Text = thisEvent.getTitle();
                    textBox2.Text = thisEvent.getDate();
                    comboBox1.SelectedIndex = thisEvent.getStartTime();
                    comboBox1.Text = comboBox1.SelectedItem.ToString();
                    comboBox2.SelectedIndex = thisEvent.getEndTime();
                    comboBox2.Text = comboBox2.SelectedItem.ToString();
                    richTextBox1.Text = thisEvent.getContent();
                }
                 * */
                //displat the first event in the list in the form with relevant controls
                if (eventList.Count == 0)  //if the list contains no event, which means no events on the selected date or in the selected month, then display nothing
                {
                    textBox1.Text = "";
                    textBox2.Text = "";
                    comboBox1.SelectedIndex = 0;
                    comboBox1.Text = comboBox1.SelectedItem.ToString();
                    comboBox2.SelectedIndex = 0;
                    comboBox2.Text = comboBox2.SelectedItem.ToString();
                    richTextBox1.Text = "";
                }
                else  //if the event list contains at least one event, then display the first event
                {
                    Event thisEvent = eventList[0] as Event;
                    textBox1.Text = thisEvent.getTitle();
                    textBox2.Text = thisEvent.getDate();
                    comboBox1.SelectedIndex = thisEvent.getStartTime();
                    comboBox1.Text = comboBox1.SelectedItem.ToString();
                    comboBox2.SelectedIndex = thisEvent.getEndTime();
                    comboBox2.Text = comboBox2.SelectedItem.ToString();
                    richTextBox1.Text = thisEvent.getContent();
                }
            }
        }


        //to set the form to the mode for modifying a selected event
        private void button2_Click(object sender, EventArgs e)
        {
            //button1_Click_1(sender, e);
            if (eventList.Count == 0)  //if no selected event, which mean no selected event and the event list is empty, then do nothing
                return;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button7.Visible = true;
            button9.Visible = true;

            textBox1.ReadOnly = false;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            richTextBox1.ReadOnly = false;
        }


        //to save the selected event to the database after the user has modified it, and set the form back to its original setting
        private void button9_Click(object sender, EventArgs e)
        {
            Event oldEvent = eventList[selectedEventIndex] as Event; //keep the origianl version (copy) of the selected event
            Event newEvent = new Event(textBox1.Text, textBox2.Text, comboBox1.SelectedIndex, comboBox2.SelectedIndex, richTextBox1.Text);  //save the new (modified) version of the selected event to the database by deleting the original copy and creating a new copy of the event
            //check the the saving event been successful
            if (newEvent.saveRevisedEvent(oldEvent) == true)  //if the saving is successful, set the form to its original setting
            {
                button2.Visible = true;
                button3.Visible = true;
                button4.Visible = true;
                button5.Visible = true;
                button7.Visible = false;
                button9.Visible = false;

                textBox1.ReadOnly = true;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                richTextBox1.ReadOnly = true;

                button1_Click_1(sender, e);  //reload the new event list with the modified event
            }
        }


        //to detect what time has been selected by the user and set it to the start time of the event
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //also change the end time of the selected event to the same time as the start time, to save the user to pick a valid end time
            comboBox2.SelectedIndex = comboBox1.SelectedIndex;
            comboBox2.Text = comboBox2.SelectedItem.ToString();
        }


        //convet the selected time to a readable time format in hh:mm format
        private string findSelectedTime(int timeIndex)
        {
            int hh = timeIndex / 2;
            int mm = timeIndex % 2 * 30;
            string timeString = "";
            string ampm = "";
            string mmS = "";
            if (hh < 12)
            {
                ampm = "am";
            }
            else
            {
                hh = hh - 12;
                if (hh == 0)
                    hh = 12;
                ampm = "pm";
            }
            if (mm == 0)
                mmS = "00";
            else
                mmS = "30";
            if (hh < 10)
                timeString = "0";
            timeString = timeString + hh.ToString() + ":" + mmS + " " + ampm;
            return timeString;
            
        }


        //to detect what time has been selected by the user as the end time of the event, and check if the selected end time is earlier than the start time
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Event aEvent = new Event();
            //check if the selected end time is earlier than the start time, if yes, an error happens
            if (aEvent.checkStartEndTimesConflict(comboBox1.SelectedIndex, comboBox2.SelectedIndex) == false)
            {
                comboBox2.SelectedIndex = comboBox1.SelectedIndex;
                comboBox2.Text = comboBox2.SelectedItem.ToString();
            }
        }


        //Similar to the monthCalendar1_DateSelected function; check that function instead
        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            monthlyEventMode = false;
            string myString = monthCalendar1.SelectionRange.Start.ToShortDateString();
            eventList.Clear();
            myString = convertDateFormat(myString);

            selectedDate = myString;

            textBox1.Text = myString;
            Event aEvent = new Event();
            eventList = aEvent.getEventList(myString);
            listBox1.Items.Clear();
            foreach (Event nextEvent in eventList)
            {
                string eventIndex = findSelectedTime(nextEvent.getStartTime()) + "    " + findSelectedTime(nextEvent.getEndTime()) + "    " + nextEvent.getTitle();
                listBox1.Items.Add(eventIndex);
            }
            if (eventList.Count == 0)
            {
                textBox1.Text = "";
                textBox2.Text = "";
                comboBox1.SelectedIndex = 0;
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = 0;
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = "";

                selectedEventIndex = -1;

            }
            else
            {
                Event thisEvent = eventList[0] as Event;
                textBox1.Text = thisEvent.getTitle();
                textBox2.Text = thisEvent.getDate();
                comboBox1.SelectedIndex = thisEvent.getStartTime();
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                comboBox2.SelectedIndex = thisEvent.getEndTime();
                comboBox2.Text = comboBox2.SelectedItem.ToString();
                richTextBox1.Text = thisEvent.getContent();

                selectedEventIndex = 0;
            }
        }
 
    }
}
