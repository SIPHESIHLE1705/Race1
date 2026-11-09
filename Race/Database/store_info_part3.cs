using Race.Models;
using System.Data;
using System.Data.SqlClient;

namespace Race.Database
{
    // PART 3 additions: read-only queries that join tables so the MVC app
    // never has to replicate business logic or touch the database itself.
    public partial class store_info
    {
        // Status is derived, so no database change is needed:
        // Completed = a result has been recorded.
        // Pending = the event date has passed but no result has been recorded.
        // Confirmed = the participant is enrolled and the event is still to come.
        private const string StatusSql = @"CASE
                WHEN R.resultID IS NOT NULL THEN 'Completed'
                WHEN EV.eventDate < CAST(GETDATE() AS DATE) THEN 'Pending'
                ELSE 'Confirmed' END";

        // EVENTS OWNED BY ONE ORGANISER (with enrolment counts)

        // Retrieves all events created by a specific organiser.
        public List<EventSummary> getOrganiserEvents(int organiserID)
        {
            // Creates a list to store the organiser's events.
            List<EventSummary> list = new List<EventSummary>();
            try
            {
                // Establishes a connection to the RaceDay database.
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    // Retrieves event details and counts the number of enrolments for each event.
                    // Only events belonging to the specified organiser are returned.
                    string query = @"SELECT E.eventID, E.eventName, E.eventDescription,
                        E.eventLocation, E.eventDistance, E.eventDate, E.eventType, E.organiserID,
                        (SELECT COUNT(*) FROM Enrolments EN WHERE EN.eventID = E.eventID) AS enrolmentCount
                        FROM Events E
                        WHERE E.organiserID = @organiserID
                        ORDER BY E.eventDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the organiser ID as a SQL parameter.
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;

                        // Opens the database connection and executes the query.
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Reads each event returned by the query.
                            while (reader.Read())
                            {
                                // Converts the database values into an EventSummary object.
                                list.Add(new EventSummary
                                {
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDescription = reader["eventDescription"].ToString()!,
                                    eventLocation = reader["eventLocation"].ToString()!,
                                    eventDistance = Convert.ToDecimal(reader["eventDistance"]),
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    eventType = reader["eventType"].ToString()!,
                                    organiserID = Convert.ToInt32(reader["organiserID"]),
                                    enrolmentCount = Convert.ToInt32(reader["enrolmentCount"])
                                });
                            }
                        }
                    }
                }
            }
            // Stores the SQL error message if a database error occurs.
            catch (SqlException ex) { message = ex.Message; }

            // Returns the list of events retrieved from the database.
            return list;
        }

        // EVERYONE ENROLLED IN ONE EVENT

        // Retrieves all participants enrolled in a specific event.
        public List<EventEnrolment> getEventEnrolments(int eventID)
        {
            // Creates a list to store the event's enrolment records.
            List<EventEnrolment> list = new List<EventEnrolment>();
            try
            {
                // Establishes a connection to the RaceDay database.
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    // Retrieves enrolment, participant, category and result details.
                    // INNER JOIN connects related records, while LEFT JOIN allows
                    // participants without recorded results to be included.
                    string query = @"SELECT EN.enrolmentID, EN.enrolmentDate, EN.userID,
                        U.fullName, U.emailAddress, EN.categoryID, C.categoryName,
                        R.resultID, R.finishingTime, R.finishingPosition,
                        " + StatusSql + @" AS status
                        FROM Enrolments EN
                        INNER JOIN Users U ON EN.userID = U.userID
                        INNER JOIN Categories C ON EN.categoryID = C.categoryID
                        INNER JOIN Events EV ON EN.eventID = EV.eventID
                        LEFT JOIN Results R ON R.enrolmentID = EN.enrolmentID
                        WHERE EN.eventID = @eventID
                        ORDER BY U.fullName";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the event ID as a SQL parameter.
                        cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;

                        // Opens the database connection and executes the query.
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Reads each participant enrolled in the event.
                            while (reader.Read())
                            {
                                // Converts the database values into an EventEnrolment object.
                                list.Add(new EventEnrolment
                                {
                                    enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                                    enrolmentDate = Convert.ToDateTime(reader["enrolmentDate"]),
                                    userID = Convert.ToInt32(reader["userID"]),
                                    participantName = reader["fullName"].ToString()!,
                                    participantEmail = reader["emailAddress"].ToString()!,
                                    categoryID = Convert.ToInt32(reader["categoryID"]),
                                    categoryName = reader["categoryName"].ToString()!,
                                    status = reader["status"].ToString()!,

                                    // Checks whether a result exists before converting nullable values.
                                    resultID = reader["resultID"] == DBNull.Value
                                        ? (int?)null : Convert.ToInt32(reader["resultID"]),
                                    finishingTime = reader["finishingTime"] == DBNull.Value
                                        ? (TimeSpan?)null : (TimeSpan)reader["finishingTime"],
                                    finishingPosition = reader["finishingPosition"] == DBNull.Value
                                        ? (int?)null : Convert.ToInt32(reader["finishingPosition"])
                                });
                            }
                        }
                    }
                }
            }
            // Stores the SQL error message if a database error occurs.
            catch (SqlException ex) { message = ex.Message; }

            // Returns the list of event enrolments.
            return list;
        }

        // PARTICIPANT'S ENROLMENTS WITH NAMES AND STATUS

        // Retrieves all enrolments belonging to a specific participant.
        public List<MyEnrolment> getMyEnrolmentDetails(int userID)
        {
            // Creates a list to store the participant's enrolments.
            List<MyEnrolment> list = new List<MyEnrolment>();
            try
            {
                // Establishes a connection to the RaceDay database.
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    // Retrieves enrolment details together with the related event and category information.
                    // LEFT JOIN checks whether a result has been recorded for each enrolment.
                    string query = @"SELECT EN.enrolmentID, EN.enrolmentDate, EV.eventID,
                        EV.eventName, EV.eventDate, EV.eventLocation, EV.eventType,
                        EN.categoryID, C.categoryName,
                        " + StatusSql + @" AS status
                        FROM Enrolments EN
                        INNER JOIN Events EV ON EN.eventID = EV.eventID
                        INNER JOIN Categories C ON EN.categoryID = C.categoryID
                        LEFT JOIN Results R ON R.enrolmentID = EN.enrolmentID
                        WHERE EN.userID = @userID
                        ORDER BY EV.eventDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the participant's user ID as a SQL parameter.
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                        // Opens the database connection and executes the query.
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Reads each enrolment belonging to the participant.
                            while (reader.Read())
                            {
                                // Converts the database values into a MyEnrolment object.
                                list.Add(new MyEnrolment
                                {
                                    enrolmentID = Convert.ToInt32(reader["enrolmentID"]),
                                    enrolmentDate = Convert.ToDateTime(reader["enrolmentDate"]),
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    eventLocation = reader["eventLocation"].ToString()!,
                                    eventType = reader["eventType"].ToString()!,
                                    categoryID = Convert.ToInt32(reader["categoryID"]),
                                    categoryName = reader["categoryName"].ToString()!,
                                    status = reader["status"].ToString()!
                                });
                            }
                        }
                    }
                }
            }
            // Stores the SQL error message if a database error occurs.
            catch (SqlException ex) { message = ex.Message; }

            // Returns the participant's enrolment list.
            return list;
        }

        // PARTICIPANT'S RACE HISTORY (position + total finishers in that event)

        // Retrieves the results recorded for a specific participant.
        public List<MyResult> getMyResults(int userID)
        {
            // Creates a list to store the participant's race results.
            List<MyResult> list = new List<MyResult>();
            try
            {
                // Establishes a connection to the RaceDay database.
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    // Retrieves the participant's results, including event and category details.
                    // A subquery counts the total number of participants who have recorded
                    // results for the same event.
                    string query = @"SELECT R.resultID, EV.eventID, EV.eventName, EV.eventDate,
                        C.categoryName, R.finishingTime, R.finishingPosition,
                        (SELECT COUNT(*) FROM Results R2
                            INNER JOIN Enrolments EN2 ON R2.enrolmentID = EN2.enrolmentID
                            WHERE EN2.eventID = EN.eventID) AS totalFinishers
                        FROM Results R
                        INNER JOIN Enrolments EN ON R.enrolmentID = EN.enrolmentID
                        INNER JOIN Events EV ON EN.eventID = EV.eventID
                        INNER JOIN Categories C ON EN.categoryID = C.categoryID
                        WHERE EN.userID = @userID
                        ORDER BY EV.eventDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Supplies the participant's user ID as a SQL parameter.
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

                        // Opens the database connection and executes the query.
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Reads each result belonging to the participant.
                            while (reader.Read())
                            {
                                // Converts the database values into a MyResult object.
                                list.Add(new MyResult
                                {
                                    resultID = Convert.ToInt32(reader["resultID"]),
                                    eventID = Convert.ToInt32(reader["eventID"]),
                                    eventName = reader["eventName"].ToString()!,
                                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                                    categoryName = reader["categoryName"].ToString()!,
                                    finishingTime = (TimeSpan)reader["finishingTime"],
                                    finishingPosition = Convert.ToInt32(reader["finishingPosition"]),
                                    totalFinishers = Convert.ToInt32(reader["totalFinishers"])
                                });
                            }
                        }
                    }
                }
            }
            // Stores the SQL error message if a database error occurs.
            catch (SqlException ex) { message = ex.Message; }

            // Returns the participant's race results.
            return list;
        }
    }
}
