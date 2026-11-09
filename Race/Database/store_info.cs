using Race.Models;
using System.Data;
using System.Data.SqlClient;
namespace Race.Database
{
    public partial class store_info
    {
        public string connection_string =
            @"Data source =(localdb)\api_both;database=RaceDay";

        public string message = string.Empty;

        // REGISTER USER
        public string createUser(
            string fullname, string email,
            string role, string password)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    con.Open();

                    string check = @"SELECT COUNT(*)
                                     FROM Users
                                     WHERE emailAddress = @email";

                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return "EMAIL_EXISTS";
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    string query = @"INSERT INTO Users
                        (fullName, emailAddress, [password], userRole)
                        VALUES (@name, @email, @password, @role)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                            .Value = fullname;

                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        cmd.Parameters.Add("@password", SqlDbType.VarChar, 255)
                            .Value = hashedPassword;

                        cmd.Parameters.Add("@role", SqlDbType.VarChar, 20)
                            .Value = role;

                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return "DATABASE_ERROR";
            }
        }

        // GET USER BY EMAIL
        public User? getUserByEmail(string email)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT userID, fullName,
                        emailAddress, [password], userRole
                        FROM Users
                        WHERE emailAddress = @email";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100)
                            .Value = email;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
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
            catch (SqlException ex)
            {
                message = ex.Message;
            }

            return null;
        }

        // GET USER BY ID
        public User? getUserByID(int userID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT userID, fullName,
                        emailAddress, userRole
                        FROM Users
                        WHERE userID = @userID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
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

            return null;
        }

        // CREATE EVENT
        public bool createEvent(
            CreateEventRequest request, int organiserID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"INSERT INTO Events
                        (eventName, eventDescription, eventLocation,
                         eventDistance, eventDate, eventType, organiserID)
                        VALUES
                        (@name, @description, @location, @distance,
                         @date, @type, @organiserID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                            .Value = request.eventName;

                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 500)
                            .Value = request.eventDescription;

                        cmd.Parameters.Add("@location", SqlDbType.VarChar, 100)
                            .Value = request.eventLocation;

                        SqlParameter distance =
                            cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8;
                        distance.Scale = 2;
                        distance.Value = request.eventDistance;

                        cmd.Parameters.Add("@date", SqlDbType.Date)
                            .Value = request.eventDate.Date;

                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 20)
                            .Value = request.eventType;

                        cmd.Parameters.Add("@organiserID", SqlDbType.Int)
                            .Value = organiserID;

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
        public List<Event> getEvents()
        {
            List<Event> events = new List<Event>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT eventID, eventName,
                        eventDescription, eventLocation, eventDistance,
                        eventDate, eventType, organiserID
                        FROM Events
                        ORDER BY eventDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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

            return events;
        }

        // GET EVENT BY ID
        public Event? getEventByID(int eventID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT eventID, eventName,
                        eventDescription, eventLocation, eventDistance,
                        eventDate, eventType, organiserID
                        FROM Events
                        WHERE eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
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

            return null;
        }

        // CHECK EVENT OWNERSHIP
        public bool organiserOwnsEvent(int eventID, int organiserID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT COUNT(*)
                        FROM Events
                        WHERE eventID = @eventID
                        AND organiserID = @organiserID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        cmd.Parameters.Add("@organiserID", SqlDbType.Int)
                            .Value = organiserID;

                        con.Open();
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
        public bool createCategory(
            CreateCategoryRequest request, int eventID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"INSERT INTO Categories
                        (categoryName, categoryAge, categoryDistance,
                         startTime, eventID)
                        VALUES
                        (@name, @age, @distance, @startTime, @eventID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 50)
                            .Value = request.categoryName;

                        cmd.Parameters.Add("@age", SqlDbType.Int)
                            .Value = request.categoryAge;

                        SqlParameter distance =
                            cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8;
                        distance.Scale = 2;
                        distance.Value = request.categoryDistance;

                        cmd.Parameters.Add("@startTime", SqlDbType.Time)
                            .Value = request.startTime;

                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

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
        public List<Category> getCategories(int eventID)
        {
            List<Category> categories = new List<Category>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT categoryID, categoryName,
                        categoryAge, categoryDistance, startTime, eventID
                        FROM Categories
                        WHERE eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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

            return categories;
        }

        // CREATE ENROLMENT
        public string createEnrolment(
            int userID, int eventID, int categoryID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    con.Open();

                    string check = @"SELECT COUNT(*)
                        FROM Categories
                        WHERE categoryID = @categoryID
                        AND eventID = @eventID";

                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        cmd.Parameters.Add("@categoryID", SqlDbType.Int)
                            .Value = categoryID;

                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                            return "CATEGORY_NOT_FOUND";
                    }

                    string query = @"INSERT INTO Enrolments
                        (enrolmentDate, userID, eventID, categoryID)
                        VALUES (@date, @userID, @eventID, @categoryID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@date", SqlDbType.Date)
                            .Value = DateTime.Today;

                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        cmd.Parameters.Add("@eventID", SqlDbType.Int)
                            .Value = eventID;

                        cmd.Parameters.Add("@categoryID", SqlDbType.Int)
                            .Value = categoryID;

                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                if (ex.Number == 2601 || ex.Number == 2627)
                    return "ALREADY_ENROLLED";

                return "DATABASE_ERROR";
            }
        }

        // GET PARTICIPANT ENROLMENTS
        public List<Enrolment> getEnrolments(int userID)
        {
            List<Enrolment> enrolments = new List<Enrolment>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT enrolmentID, enrolmentDate,
                        userID, eventID, categoryID
                        FROM Enrolments
                        WHERE userID = @userID
                        ORDER BY enrolmentDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@userID", SqlDbType.Int)
                            .Value = userID;

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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

            return enrolments;
        }

        // GET EVENT ID FOR ENROLMENT
        public int? getEventIDForEnrolment(int enrolmentID)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"SELECT eventID
                        FROM Enrolments
                        WHERE enrolmentID = @enrolmentID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@enrolmentID", SqlDbType.Int)
                            .Value = enrolmentID;

                        con.Open();

                        object? value = cmd.ExecuteScalar();

                        if (value == null || value == DBNull.Value)
                            return null;

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
        public string createResult(CreateResultRequest request)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
                    string query = @"INSERT INTO Results
                        (finishingTime, finishingPosition, enrolmentID)
                        VALUES (@time, @position, @enrolmentID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@time", SqlDbType.Time)
                            .Value = request.finishingTime;

                        cmd.Parameters.Add("@position", SqlDbType.Int)
                            .Value = request.finishingPosition;

                        cmd.Parameters.Add("@enrolmentID", SqlDbType.Int)
                            .Value = request.enrolmentID;

                        con.Open();

                        return cmd.ExecuteNonQuery() > 0
                            ? "SUCCESS" : "FAILED";
                    }
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;

                if (ex.Number == 2601 || ex.Number == 2627)
                    return "RESULT_EXISTS";

                return "DATABASE_ERROR";
            }
        }

        // GET ALL RESULTS
        public List<Result> getResults()
        {
            List<Result> results = new List<Result>();

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connection_string))
                {
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

            return results;
        }

        // UPDATE USER PROFILE
        public string updateUser(int userID, string fullName, string emailAddress)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    con.Open();

                    string check = @"SELECT COUNT(*) FROM Users
                                     WHERE emailAddress = @email AND userID <> @userID";
                    using (SqlCommand cmd = new SqlCommand(check, con))
                    {
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = emailAddress;
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return "EMAIL_EXISTS";
                    }

                    string query = @"UPDATE Users SET fullName = @name, emailAddress = @email
                                     WHERE userID = @userID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = fullName;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = emailAddress;
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                        return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                    }
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // UPDATE EVENT (owner must be checked by controller)
        public string updateEvent(int eventID, int organiserID, CreateEventRequest request)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    string query = @"UPDATE Events SET eventName=@name, eventDescription=@description,
                        eventLocation=@location, eventDistance=@distance, eventDate=@date, eventType=@type
                        WHERE eventID=@eventID AND organiserID=@organiserID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = request.eventName;
                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 500).Value = request.eventDescription;
                        cmd.Parameters.Add("@location", SqlDbType.VarChar, 100).Value = request.eventLocation;
                        SqlParameter distance = cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                        distance.Precision = 8; distance.Scale = 2; distance.Value = request.eventDistance;
                        cmd.Parameters.Add("@date", SqlDbType.Date).Value = request.eventDate.Date;
                        cmd.Parameters.Add("@type", SqlDbType.VarChar, 20).Value = request.eventType;
                        cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;
                        con.Open();
                        return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                    }
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // DELETE EVENT. SQL Server will reject deletion when dependent rows exist.
        public string deleteEvent(int eventID, int organiserID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Events WHERE eventID=@eventID AND organiserID=@organiserID", con))
                {
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return ex.Number == 547 ? "HAS_DEPENDENCIES" : "DATABASE_ERROR";
            }
        }

        // UPDATE CATEGORY
        public string updateCategory(int eventID, int categoryID, CreateCategoryRequest request)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Categories SET categoryName=@name,
                    categoryAge=@age, categoryDistance=@distance, startTime=@startTime
                    WHERE eventID=@eventID AND categoryID=@categoryID", con))
                {
                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = request.categoryName;
                    cmd.Parameters.Add("@age", SqlDbType.Int).Value = request.categoryAge;
                    SqlParameter distance = cmd.Parameters.Add("@distance", SqlDbType.Decimal);
                    distance.Precision = 8; distance.Scale = 2; distance.Value = request.categoryDistance;
                    cmd.Parameters.Add("@startTime", SqlDbType.Time).Value = request.startTime;
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                if (ex.Number == 2601 || ex.Number == 2627) return "DUPLICATE_CATEGORY";
                return "DATABASE_ERROR";
            }
        }

        // DELETE CATEGORY
        public string deleteCategory(int eventID, int categoryID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Categories WHERE eventID=@eventID AND categoryID=@categoryID", con))
                {
                    cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                    cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex)
            {
                message = ex.Message;
                return ex.Number == 547 ? "HAS_ENROLMENTS" : "DATABASE_ERROR";
            }
        }

        // DELETE PARTICIPANT ENROLMENT. A result must be removed first.
        public string deleteEnrolment(int enrolmentID, int userID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM Enrolments
                    WHERE enrolmentID=@enrolmentID AND userID=@userID
                    AND NOT EXISTS (SELECT 1 FROM Results WHERE Results.enrolmentID=Enrolments.enrolmentID)", con))
                {
                    cmd.Parameters.Add("@enrolmentID", SqlDbType.Int).Value = enrolmentID;
                    cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND_OR_HAS_RESULT";
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

        // UPDATE RESULT by resultID. Ownership is verified by controller.
        public string updateResult(int resultID, TimeSpan finishingTime, int finishingPosition)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Results
                    SET finishingTime=@time, finishingPosition=@position
                    WHERE resultID=@resultID", con))
                {
                    cmd.Parameters.Add("@time", SqlDbType.Time).Value = finishingTime;
                    cmd.Parameters.Add("@position", SqlDbType.Int).Value = finishingPosition;
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

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
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;
                    con.Open();
                    object? value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
                }
            }
            catch (SqlException ex) { message = ex.Message; return null; }
        }

        // DELETE RESULT
        public string deleteResult(int resultID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Results WHERE resultID=@resultID", con))
                {
                    cmd.Parameters.Add("@resultID", SqlDbType.Int).Value = resultID;
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0 ? "SUCCESS" : "NOT_FOUND";
                }
            }
            catch (SqlException ex) { message = ex.Message; return "DATABASE_ERROR"; }
        }

    }
}