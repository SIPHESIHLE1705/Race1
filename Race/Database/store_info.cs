using Race.Models;
using System.Data;
using System.Data.SqlClient;

namespace Race.Database
{
    public partial class store_info
    {
        // Stores the connection string used to connect to the RaceDay database.
        public string connection_string =
            @"Data source =(localdb)\api_both;database=RaceDay";

        // Stores database error messages when SQL operations fail.
        public string message = string.Empty;

        // REGISTER USER

        // Registers a new user in the database.
        public string createUser(
            string fullname, string email,
            string role, string password)
        {
            try
            {
                // Establishes a connection to the database.
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    con.Open();

                    // Checks whether the email address is already registered.
                    string check = @"SELECT COUNT(*)
                                     FROM Users
                                     WHERE emailAddress = @email";

                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        // Supplies the email address as a SQL parameter.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        // Stops registration if the email already exists.
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return "EMAIL_EXISTS";
                    }

                    // Hashes the password before storing it in the database.
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    // Inserts the new user's details into the Users table.
                    string query = @"INSERT INTO Users
                        (fullName, emailAddress, [password], userRole)
                        VALUES (@name, @email, @password, @role)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Adds the user's full name.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                            .Value = fullname;

                        // Adds the user's email address.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        // Adds the hashed password.
                        cmd.Parameters.Add("@password", SqlDbType.VarChar, 255)
                            .Value = hashedPassword;

                        // Adds the user's selected role.
                        cmd.Parameters.Add("@role", SqlDbType.VarChar, 20)
                            .Value = role;

                        // Executes the INSERT statement and returns the result.
                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            // Handles database errors during registration.
            catch (SqlException ex)
            {
                message = ex.Message;
                return "DATABASE_ERROR";
            }
        }

        // GET USER BY EMAIL

        // Retrieves a user's details using their email address.
        // The password is included so that login authentication can be performed.
        public User? getUserByEmail(string email)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the user whose email matches the supplied email.
                    string query = @"SELECT userID, fullName,
                        emailAddress, [password], userRole
                        FROM Users
                        WHERE emailAddress = @email";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the email address as a SQL parameter.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        con.Open();

                        // Executes the query and reads the matching user.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Maps the database values to a User object.
                                return new User
                                {
                                    userID = Convert.ToInt32(reader["userID"]),
                                    fullName = reader["fullName"].ToString()!,
                                    emailAddress = reader["emailAddress"].ToString()!,
                                    password = reader["password"].ToString()!,
                                    userRole = reader["userRole"].ToString()!
                                };
                            }
                        }
                    }
                }
            }
            // Stores the database error if the query fails.
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns null when no matching user is found or an SQL error occurs.
            return null;
        }

        // GET USER BY ID

        // Retrieves a user's profile details using their user ID.
        public User? getUserByID(int userID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the profile information for the specified user.
                    string query = @"SELECT userID, fullName,
                        emailAddress, userRole
                        FROM Users
                        WHERE userID = @userID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the user ID as a SQL parameter.
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Maps the retrieved values to a User object.
                                return new User
                                {
                                    userID = Convert.ToInt32(reader["userID"]),
                                    fullName = reader["fullName"].ToString()!,
                                    emailAddress = reader["emailAddress"].ToString()!,
                                    userRole = reader["userRole"].ToString()!
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns null if the user cannot be found.
            return null;
        }

        // CREATE EVENT

        // Creates a new event and links it to the organiser who created it.
        public bool createEvent(
            CreateEventRequest request, int organiserID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Inserts the event details into the Events table.
                    string query = @"INSERT INTO Events
                        (eventName, eventDescription, eventLocation,
                         eventDistance, eventDate, eventType, organiserID)
                        VALUES
                        (@name, @description, @location, @distance,
                         @date, @type, @organiserID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Adds the event name.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                            .Value = request.eventName;

                        // Adds the event description.
                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 500)
                            .Value = request.eventDescription;

                        // Adds the event location.
                        cmd.Parameters.Add("@location", SqlDbType.VarChar, 100)
                            .Value = request.eventLocation;

                        // Adds the event distance with its decimal precision and scale.
                        SqlParameter distance =
                            cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8;
                        distance.Scale = 2;
                        distance.Value = request.eventDistance;

                        // Stores the event date without its time component.
                        cmd.Parameters.Add("@date", SqlDbType.Date)
                            .Value = request.eventDate.Date;

                        // Adds the event type.
                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 20)
                            .Value = request.eventType;

                        // Links the event to its organiser.
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int)
                            .Value = organiserID;

                        // Opens the connection and executes the INSERT statement.
                        con.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return false;
            }
        }

        // GET ALL EVENTS

        // Retrieves all events stored in the database.
        public List<Event> getEvents()
        {
            // Creates a list to store the retrieved events.
            List<Event> events = new List<Event>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects all event details and orders them by event date.
                    string query = @"SELECT eventID, eventName,
                        eventDescription, eventLocation, eventDistance,
                        eventDate, eventType, organiserID
                        FROM Events
                        ORDER BY eventDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();

                        // Executes the query and reads the returned events.
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Converts each database record into an Event object.
                                events.Add(new Event
                                {
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDescription = reader["eventDescription"].ToString()!,
                                    eventLocation = reader["eventLocation"].ToString()!,
                                    eventDistance = Convert.ToDecimal(reader["eventDistance"]),
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    eventType = reader["eventType"].ToString()!,
                                    organiserID = Convert.ToInt32(reader["organiserID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns the list of events.
            return events;
        }

        // GET EVENT BY ID

        // Retrieves one event using its unique event ID.
        public Event? getEventByID(int eventID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the event matching the supplied ID.
                    string query = @"SELECT eventID, eventName,
                        eventDescription, eventLocation, eventDistance,
                        eventDate, eventType, organiserID
                        FROM Events
                        WHERE eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID as a SQL parameter.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Maps the database record to an Event object.
                                return new Event
                                {
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDescription = reader["eventDescription"].ToString()!,
                                    eventLocation = reader["eventLocation"].ToString()!,
                                    eventDistance = Convert.ToDecimal(reader["eventDistance"]),
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    eventType = reader["eventType"].ToString()!,
                                    organiserID = Convert.ToInt32(reader["organiserID"])
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns null if the event is not found.
            return null;
        }

        // CHECK EVENT OWNERSHIP

        // Checks whether an event belongs to the specified organiser.
        public bool organiserOwnsEvent(int eventID, int organiserID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Counts records matching both the event ID and organiser ID.
                    string query = @"SELECT COUNT(*)
                        FROM Events
                        WHERE eventID = @eventID
                        AND organiserID = @organiserID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Supplies the organiser ID.
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int)
                            .Value = organiserID;

                        con.Open();

                        // Returns true if a matching event belongs to the organiser.
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return false;
            }
        }

        // CREATE CATEGORY

        // Creates a category and links it to the specified event.
        public bool createCategory(
            CreateCategoryRequest request, int eventID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Inserts the category details into the Categories table.
                    string query = @"INSERT INTO Categories
                        (categoryName, categoryAge, categoryDistance,
                         startTime, eventID)
                        VALUES
                        (@name, @age, @distance, @startTime, @eventID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Adds the category name.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 50)
                            .Value = request.categoryName;

                        // Adds the category age.
                        cmd.Parameters.Add("@age", SqlDbType.Int)
                            .Value = request.categoryAge;

                        // Adds the category distance with decimal precision.
                        SqlParameter distance =
                            cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8;
                        distance.Scale = 2;
                        distance.Value = request.categoryDistance;

                        // Adds the category starting time.
                        cmd.Parameters.Add("@startTime", SqlDbType.Time)
                            .Value = request.startTime;

                        // Links the category to its event.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Executes the INSERT statement.
                        con.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return false;
            }
        }

        // GET CATEGORIES BY EVENT

        // Retrieves all categories belonging to a specific event.
        public List<Category> getCategories(int eventID)
        {
            // Creates a list to store the categories.
            List<Category> categories = new List<Category>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects categories linked to the specified event.
                    string query = @"SELECT categoryID, categoryName,
                        categoryAge, categoryDistance, startTime, eventID
                        FROM Categories
                        WHERE eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Converts each database record into a Category object.
                                categories.Add(new Category
                                {
                                    categoryID = Convert.ToInt32(reader["categoryID"]),
                                    categoryName = reader["categoryName"].ToString()!,
                                    categoryAge = Convert.ToInt32(reader["categoryAge"]),
                                    categoryDistance = Convert.ToDecimal(reader["categoryDistance"]),
                                    startTime = (TimeSpan)reader["startTime"],
                                    eventID = Convert.ToInt32(reader["eventID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns the category list.
            return categories;
        }

        // CREATE ENROLMENT

        // Enrols a participant in a selected event category.
        public string createEnrolment(
            int userID, int eventID, int categoryID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    con.Open();

                    // Checks that the selected category belongs to the selected event.
                    string check = @"SELECT COUNT(*)
                        FROM Categories
                        WHERE categoryID = @categoryID
                        AND eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        // Supplies the selected category ID.
                        cmd.Parameters.Add("@categoryID", SqlDbType.Int)
                            .Value = categoryID;

                        // Supplies the selected event ID.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Stops enrolment if the category does not belong to the event.
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            return "CATEGORY_NOT_FOUND";
                    }

                    // Inserts the participant's enrolment details.
                    string query = @"INSERT INTO Enrolments
                        (enrolmentDate, userID, eventID, categoryID)
                        VALUES (@date, @userID, @eventID, @categoryID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Records the current date as the enrolment date.
                        cmd.Parameters.Add("@date", SqlDbType.Date)
                            .Value = DateTime.Today;

                        // Links the enrolment to the participant.
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        // Links the enrolment to the event.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        // Links the enrolment to the selected category.
                        cmd.Parameters.Add("@categoryID", SqlDbType.Int)
                            .Value = categoryID;

                        // Executes the INSERT statement and returns the status.
                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                // Identifies SQL Server errors caused by duplicate records.
                if (ex.Number == 2601 || ex.Number == 2627)
                    return "ALREADY_ENROLLED";

                return "DATABASE_ERROR";
            }
        }

        // GET PARTICIPANT ENROLMENTS

        // Retrieves all enrolments belonging to a particular participant.
        public List<Enrolment> getEnrolments(int userID)
        {
            // Creates a list to store the participant's enrolments.
            List<Enrolment> enrolments = new List<Enrolment>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects enrolments belonging to the supplied user ID.
                    string query = @"SELECT enrolmentID, enrolmentDate,
                        userID, eventID, categoryID
                        FROM Enrolments
                        WHERE userID = @userID
                        ORDER BY enrolmentDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the participant's user ID.
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Maps each database record to an Enrolment object.
                                enrolments.Add(new Enrolment
                                {
                                    enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                                    enrolmentDate = Convert.ToDateTime(reader["enrolmentDate"]),
                                    userID = Convert.ToInt32(reader["userID"]),
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    categoryID = Convert.ToInt32(reader["categoryID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns the participant's enrolments.
            return enrolments;
        }

        // GET EVENT ID FOR ENROLMENT

        // Retrieves the event ID connected to a particular enrolment.
        public int? getEventIDForEnrolment(int enrolmentID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects the event ID using the enrolment ID.
                    string query = @"SELECT eventID
                        FROM Enrolments
                        WHERE enrolmentID = @enrolmentID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the enrolment ID.
                        cmd.Parameters.Add("@enrolmentID", SqlDbType.Int)
                            .Value = enrolmentID;

                        con.Open();

                        // ExecuteScalar retrieves the first column of the first returned row.
                        object? value = cmd.ExecuteScalar();

                        // Returns null if no matching event ID exists.
                        if (value == null || value == DBNull.Value)
                            return null;

                        // Converts the returned value into an integer.
                        return Convert.ToInt32(value);
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return null;
            }
        }

        // CREATE RESULT

        // Records a participant's finishing time and position.
        public string createResult(CreateResultRequest request)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Inserts the result details into the Results table.
                    string query = @"INSERT INTO Results
                        (finishingTime, finishingPosition, enrolmentID)
                        VALUES (@time, @position, @enrolmentID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Adds the participant's finishing time.
                        cmd.Parameters.Add("@time", SqlDbType.Time)
                            .Value = request.finishingTime;

                        // Adds the participant's finishing position.
                        cmd.Parameters.Add("@position", SqlDbType.Int)
                            .Value = request.finishingPosition;

                        // Links the result to the participant's enrolment.
                        cmd.Parameters.Add("@enrolmentID", SqlDbType.Int)
                            .Value = request.enrolmentID;

                        con.Open();

                        // Executes the INSERT statement and returns the result.
                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                // Identifies duplicate result errors.
                if (ex.Number == 2601 || ex.Number == 2627)
                    return "RESULT_EXISTS";

                return "DATABASE_ERROR";
            }
        }

        // GET ALL RESULTS

        // Retrieves all recorded results from the database.
        public List<Result> getResults()
        {
            // Creates a list to store the results.
            List<Result> results = new List<Result>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    // Selects all results and orders them by finishing position.
                    string query = @"SELECT resultID, finishingTime,
                        finishingPosition, enrolmentID
                        FROM Results
                        ORDER BY finishingPosition";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Maps each database record to a Result object.
                                results.Add(new Result
                                {
                                    resultID = Convert.ToInt32(reader["resultID"]),
                                    finishingTime = (TimeSpan)reader["finishingTime"],
                                    finishingPosition = Convert.ToInt32(reader["finishingPosition"]),
                                    enrolmentID = Convert.ToInt32(reader["enrolmentID"])
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            // Returns the list of results.
            return results;
        }

        // UPDATE USER PROFILE

        // Updates a participant's or organiser's name and email address.
        public string updateUser(int userID, string fullName, string emailAddress)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    con.Open();

                    // Checks whether another user already uses the new email address.
                    string check = @"SELECT COUNT(*) FROM Users
                                     WHERE emailAddress = @email AND userID <> @userID";
                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        // Supplies the new email address and current user's ID.
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = emailAddress;
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                        // Stops the update if another account uses the email address.
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return "EMAIL_EXISTS";
                    }

                    // Updates the user's name and email address.
                    string query = @"UPDATE Users SET fullName = @name, emailAddress = @email
                                     WHERE userID = @userID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the updated profile information.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = fullName;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = emailAddress;
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                        // Returns SUCCESS if a record was updated, otherwise NOT_FOUND.
                        return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                    }
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // UPDATE EVENT

        // Updates an existing event.
        // The controller is responsible for checking that the organiser owns the event.
        public string updateEvent(int eventID, int organiserID, CreateEventRequest request)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    // Updates the event details only when both IDs match.
                    string query = @"UPDATE Events SET eventName=@name, eventDescription=@description,
                        eventLocation=@location, eventDistance=@distance, eventDate=@date, eventType=@type
                        WHERE eventID=@eventID AND organiserID=@organiserID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the updated event details.
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = request.eventName;
                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 500).Value = request.eventDescription;
                        cmd.Parameters.Add("@location", SqlDbType.VarChar, 100).Value = request.eventLocation;

                        // Supplies the event distance with decimal precision.
                        SqlParameter distance = cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8; distance.Scale = 2; distance.Value = request.eventDistance;

                        cmd.Parameters.Add("@date", SqlDbType.Date).Value = request.eventDate.Date;
                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 20).Value = request.eventType;

                        // Supplies the event and organiser IDs.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;

                        con.Open();

                        // Returns SUCCESS if the event was updated.
                        return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                    }
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // DELETE EVENT

        // Deletes an event belonging to the specified organiser.
        // SQL Server prevents deletion when dependent records exist.
        public string deleteEvent(int eventID, int organiserID)
        {
            try
            {
                // Creates the database connection and DELETE command.
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Events WHERE eventID=@eventID AND organiserID=@organiserID", con))
                {
                    // Supplies the event ID and organiser ID.
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;

                    con.Open();

                    // Deletes the matching event and checks whether a row was affected.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                // SQL error 547 indicates a foreign key or other dependency restriction.
                return ex.Number == 547 ? "HAS_DEPENDENCIES" : "DATABASE_ERROR";
            }
        }

        // UPDATE CATEGORY

        // Updates the details of a category belonging to a particular event.
        public string updateCategory(int eventID, int categoryID, CreateCategoryRequest request)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                // Updates the category only when both event and category IDs match.
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Categories SET categoryName=@name,
                    categoryAge=@age, categoryDistance=@distance, startTime=@startTime
                    WHERE eventID=@eventID AND categoryID=@categoryID", con))
                {
                    // Supplies the updated category details.
                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = request.categoryName;
                    cmd.Parameters.Add("@age", SqlDbType.Int).Value = request.categoryAge;

                    // Supplies the category distance with decimal precision.
                    SqlParameter distance = cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                    distance.Precision = 8; distance.Scale = 2; distance.Value = request.categoryDistance;

                    cmd.Parameters.Add("@startTime", SqlDbType.Time).Value = request.startTime;
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;

                    con.Open();

                    // Returns SUCCESS if the category was updated.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                // Identifies duplicate category errors.
                if (ex.Number == 2601 || ex.Number == 2627) return "DUPLICATE_CATEGORY";

                return "DATABASE_ERROR";
            }
        }

        // DELETE CATEGORY

        // Deletes a category belonging to the specified event.
        public string deleteCategory(int eventID, int categoryID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Categories WHERE eventID=@eventID AND categoryID=@categoryID", con))
                {
                    // Supplies the event and category IDs.
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;

                    con.Open();

                    // Executes the DELETE statement.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                // SQL error 547 indicates that enrolments depend on this category.
                return ex.Number == 547 ? "HAS_ENROLMENTS" : "DATABASE_ERROR";
            }
        }

        // DELETE PARTICIPANT ENROLMENT

        // Deletes an enrolment belonging to a specific participant.
        // An enrolment cannot be deleted while it has an associated result.
        public string deleteEnrolment(int enrolmentID, int userID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM Enrolments
                    WHERE enrolmentID=@enrolmentID AND userID=@userID
                    AND NOT EXISTS (SELECT 1 FROM Results WHERE Results.enrolmentID=Enrolments.enrolmentID)", con))
                {
                    // Supplies the enrolment ID and participant ID.
                    cmd.Parameters.Add("@enrolmentID", SqlDbType.Int).Value = enrolmentID;
                    cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                    con.Open();

                    // Deletes the enrolment only if it belongs to the participant
                    // and does not have an associated result.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND_OR_HAS_RESULT";
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // UPDATE RESULT

        // Updates a participant's finishing time and finishing position.
        // The controller is responsible for verifying event ownership.
        public string updateResult(int resultID, TimeSpan finishingTime, int finishingPosition)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Results
                    SET finishingTime=@time, finishingPosition=@position
                    WHERE resultID=@resultID", con))
                {
                    // Supplies the updated result details and result ID.
                    cmd.Parameters.Add("@time", SqlDbType.Time).Value = finishingTime;
                    cmd.Parameters.Add("@position", SqlDbType.Int).Value = finishingPosition;
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;

                    con.Open();

                    // Returns SUCCESS if the result was updated.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // GET EVENT ID FOR RESULT

        // Retrieves the event ID associated with a particular result.
        public int? getEventIDForResult(int resultID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"SELECT E.eventID
                    FROM Results R
                    INNER JOIN Enrolments EN ON R.enrolmentID=EN.enrolmentID
                    INNER JOIN Events E ON EN.eventID=E.eventID
                    WHERE R.resultID=@resultID", con))
                {
                    // Supplies the result ID.
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;

                    con.Open();

                    // Retrieves the event ID connected to the result.
                    object? value = cmd.ExecuteScalar();

                    // Returns null if no matching result or event is found.
                    return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
                }
            }
            catch (SqlException ex) { message = ex.Message; return null; }
        }

        // DELETE RESULT

        // Deletes a result using its unique result ID.
        public string deleteResult(int resultID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Results WHERE resultID=@resultID", con))
                {
                    // Supplies the result ID.
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;

                    con.Open();

                    // Executes the DELETE statement and returns the operation status.
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

    }
}
