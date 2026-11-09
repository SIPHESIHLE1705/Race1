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
        //   Completed = a result has been recorded
        //   Pending   = event date has passed but no result yet
        //   Confirmed = enrolled and the event is still to come
        private const string StatusSql = @"CASE
                WHEN R.resultID IS NOT NULL THEN 'Completed'
                WHEN EV.eventDate < CAST(GETDATE() AS DATE) THEN 'Pending'
                ELSE 'Confirmed' END";

        // EVENTS OWNED BY ONE ORGANISER (with enrolment counts)
        public List<EventSummary> getOrganiserEvents(int organiserID)
        {
            List<EventSummary> list = new List<EventSummary>();
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
                    string query = @"SELECT E.eventID, E.eventName, E.eventDescription,
                        E.eventLocation, E.eventDistance, E.eventDate, E.eventType, E.organiserID,
                        (SELECT COUNT(*) FROM Enrolments EN WHERE EN.eventID = E.eventID) AS enrolmentCount
                        FROM Events E
                        WHERE E.organiserID = @organiserID
                        ORDER BY E.eventDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@organiserID", SqlDbType.Int).Value = organiserID;
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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
            catch (SqlException ex) { message = ex.Message; }
            return list;
        }

        // EVERYONE ENROLLED IN ONE EVENT
        public List<EventEnrolment> getEventEnrolments(int eventID)
        {
            List<EventEnrolment> list = new List<EventEnrolment>();
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
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
                        cmd.Parameters.Add("@eventID", SqlDbType.Int).Value = eventID;
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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
            catch (SqlException ex) { message = ex.Message; }
            return list;
        }

        // PARTICIPANT'S ENROLMENTS WITH NAMES AND STATUS
        public List<MyEnrolment> getMyEnrolmentDetails(int userID)
        {
            List<MyEnrolment> list = new List<MyEnrolment>();
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
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
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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
            catch (SqlException ex) { message = ex.Message; }
            return list;
        }

        // PARTICIPANT'S RACE HISTORY (position + total finishers in that event)
        public List<MyResult> getMyResults(int userID)
        {
            List<MyResult> list = new List<MyResult>();
            try
            {
                using (SqlConnection con = new SqlConnection(connection_string))
                {
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
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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
            catch (SqlException ex) { message = ex.Message; }
            return list;
        }
    }
}
