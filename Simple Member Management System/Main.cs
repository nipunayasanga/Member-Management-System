using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace Simple_Member_Management_System
{
    public partial class Main : Form
    {
        // Declare necessary variables for SQLite operations
        SQLiteConnection conn;
        SQLiteCommand cmd;
        SQLiteDataAdapter adapter;
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        int id; // To hold the ID of the selected record
        bool isDoubleClick = false; // To check if a row is double-clicked for editing
        String connectString; // Connection string for SQLite database

        public Main()
        {
            InitializeComponent();
            // Set the connection string to the database file located in the application's startup path
            connectString = @"Data Source=" + Application.StartupPath + @"\Database\db_mem.db;version=3";
            GenerateDatabase(); // Generate the database if it doesn't exist
        }

        // Method to handle the Add button click event
        private void Add(object sender, EventArgs e)
        {
            // Check if all required fields are filled
            if (txt_firstname.Text == "" || txt_lastname.Text == "" || txt_address.Text == "" || txt_age.Text == "" || cbox_gender.SelectedIndex == -1)
            {
                MessageBox.Show("Required Field!");
            }
            else
            {
                try
                {
                    conn = new SQLiteConnection(connectString); // Establish connection
                    cmd = new SQLiteCommand();
                    // SQL command to insert a new member record
                    cmd.CommandText = @"INSERT INTO member (firstname, lastname, address, age, gender) VALUES(@firstname, @lastname, @address, @age, @gender)";
                    cmd.Connection = conn;
                    // Add parameters to the SQL command
                    cmd.Parameters.Add(new SQLiteParameter("@firstname", txt_firstname.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@lastname", txt_lastname.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@address", txt_address.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@age", txt_age.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@gender", cbox_gender.SelectedItem.ToString()));
                    conn.Open();

                    int i = cmd.ExecuteNonQuery(); // Execute the command

                    if (i == 1)
                    {
                        MessageBox.Show("Successfully Created!");
                        // Clear the input fields
                        txt_firstname.Text = "";
                        txt_lastname.Text = "";
                        txt_address.Text = "";
                        txt_age.Text = "";
                        cbox_gender.SelectedItem = null;
                        ReadData(); // Refresh the data grid view
                        dataGridView1.ClearSelection();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Show error message
                }
            }
        }

        // Method to generate the database if it doesn't exist
        private void GenerateDatabase()
        {
            String path = Application.StartupPath + @"\Database\db_mem.db"; // Database file path
            if (!File.Exists(path))
            {
                conn = new SQLiteConnection(connectString); // Establish connection
                conn.Open();
                // SQL command to create a new table
                string sql = "CREATE TABLE member (ID INTEGER PRIMARY KEY AUTOINCREMENT, firstname TEXT, lastname TEXT, address TEXT, age TEXT, gender TEXT)";
                cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery(); // Execute the command
                conn.Close();
            }
        }

        // Method to read data from the database and display it in the data grid view
        private void ReadData()
        {
            try
            {
                conn = new SQLiteConnection(connectString); // Establish connection
                conn.Open();
                cmd = new SQLiteCommand();
                String sql = "SELECT * FROM member"; // SQL command to select all records
                adapter = new SQLiteDataAdapter(sql, conn);
                ds.Reset(); // Reset the dataset
                adapter.Fill(ds); // Fill the dataset with data from the database
                dt = ds.Tables[0]; // Assign the first table to the datatable
                dataGridView1.DataSource = dt; // Bind the datatable to the data grid view
                conn.Close();
                // Customize the data grid view columns
                dataGridView1.Columns[1].HeaderText = "Firstname";
                dataGridView1.Columns[2].HeaderText = "Lastname";
                dataGridView1.Columns[3].HeaderText = "Address";
                dataGridView1.Columns[4].HeaderText = "Age";
                dataGridView1.Columns[5].HeaderText = "Gender";
                dataGridView1.Columns[0].Visible = false; // Hide the ID column
                dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Set selection mode
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Show error message
            }
        }

        // Method to handle the form load event
        private void Main_Load(object sender, EventArgs e)
        {
            ReadData(); // Load data when the form loads
        }

        // Method to handle the cell double-click event in the data grid view for editing
        private void Edit(object sender, DataGridViewCellEventArgs e)
        {
            // Get the ID of the selected row
            id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
            // Populate the input fields with the selected row's data
            txt_firstname.Text = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            txt_lastname.Text = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            txt_address.Text = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
            txt_age.Text = dataGridView1.SelectedRows[0].Cells[4].Value.ToString();
            cbox_gender.SelectedIndex = cbox_gender.FindStringExact(dataGridView1.SelectedRows[0].Cells[5].Value.ToString());
            isDoubleClick = true; // Set the flag for editing
        }

        // Method to handle the cell click event in the data grid view for deletion
        private void GetIdToDelete(object sender, DataGridViewCellEventArgs e)
        {
            id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value); // Get the ID of the selected row
            isDoubleClick = false; // Reset the flag
            // Clear the input fields
            txt_firstname.Text = "";
            txt_lastname.Text = "";
            txt_address.Text = "";
            txt_age.Text = "";
            cbox_gender.SelectedItem = null;
        }

        // Method to handle the Update button click event
        private void Update(object sender, EventArgs e)
        {
            if (isDoubleClick)
            {
                try
                {
                    conn.Open();
                    cmd = new SQLiteCommand();
                    // SQL command to update the selected member record
                    cmd.CommandText = @"UPDATE member set firstname=@firstname, lastname=@lastname, address=@address, age=@age, gender=@gender WHERE ID='" + id + "'";
                    cmd.Connection = conn;
                    // Add parameters to the SQL command
                    cmd.Parameters.AddWithValue("@firstname", txt_firstname.Text);
                    cmd.Parameters.AddWithValue("@lastname", txt_lastname.Text);
                    cmd.Parameters.AddWithValue("@address", txt_address.Text);
                    cmd.Parameters.AddWithValue("@age", txt_age.Text);
                    cmd.Parameters.AddWithValue("@gender", cbox_gender.SelectedItem.ToString());

                    int i = cmd.ExecuteNonQuery(); // Execute the command

                    if (i == 1)
                    {
                        MessageBox.Show("Successfully Updated!");
                        // Clear the input fields
                        txt_firstname.Text = "";
                        txt_lastname.Text = "";
                        txt_address.Text = "";
                        txt_age.Text = "";
                        cbox_gender.SelectedItem = null;
                        ReadData(); // Refresh the data grid view
                        id = 0;
                        dataGridView1.ClearSelection();
                        dataGridView1.CurrentCell = null;
                        isDoubleClick = false; // Reset the flag
                    }

                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Show error message
                }
            }
        }

        // Method to handle the Delete button click event
        private void Delete(object sender, EventArgs e)
        {
            // Show confirmation dialog
            DialogResult dialogResult = MessageBox.Show("Do you want to delete this record?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    conn = new SQLiteConnection(connectString); // Establish connection
                    conn.Open();
                    cmd = new SQLiteCommand();
                    // SQL command to delete the selected member record
                    cmd.CommandText = @"DELETE FROM member WHERE ID='" + id + "'";
                    cmd.Connection = conn;
                    int i = cmd.ExecuteNonQuery(); // Execute the command
                    if (i == 1)
                    {
                        MessageBox.Show("Successfully Deleted!");
                        id = 0;
                        dataGridView1.ClearSelection();
                        dataGridView1.CurrentCell = null;
                        ReadData(); // Refresh the data grid view
                        dataGridView1.ClearSelection();
                        dataGridView1.CurrentCell = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Show error message
                }
            }
        }

        // Method to handle the Clear button click event
        private void Clear(object sender, EventArgs e)
        {
            // Reset input fields and selections
            id = 0;
            txt_firstname.Text = "";
            txt_lastname.Text = "";
            txt_address.Text = "";
            txt_age.Text = "";
            cbox_gender.SelectedItem = null;
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
            isDoubleClick = false; // Reset the flag
        }

        // Event handler for text changed event of the first name textbox (empty for now)
        private void txt_firstname_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
