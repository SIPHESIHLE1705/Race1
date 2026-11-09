CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- ==========================================
-- 1. USERS (Organiser and Participant share this table via userRole)
-- ==========================================
CREATE TABLE Users
(
    userID INT PRIMARY KEY IDENTITY(100,1),
    fullName VARCHAR(100) NOT NULL,
    emailAddress VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    userRole VARCHAR(20) NOT NULL,

    CONSTRAINT CK_Users_Role
        CHECK (userRole IN ('Organiser', 'Participant'))
);
GO

-- ==========================================
-- 2. EVENTS
-- ==========================================
CREATE TABLE Events
(
    eventID INT PRIMARY KEY IDENTITY(300,1),
    eventName VARCHAR(100) NOT NULL,
    eventDescription VARCHAR(500) NOT NULL,
    eventLocation VARCHAR(100) NOT NULL,
    eventDistance DECIMAL(8,2) NOT NULL,
    eventDate DATE NOT NULL,
    eventType VARCHAR(20) NOT NULL,
    organiserID INT NOT NULL,

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (organiserID)
        REFERENCES Users(userID),

    CONSTRAINT CK_Events_Distance
        CHECK (eventDistance > 0),

    CONSTRAINT CK_Events_Type
        CHECK (eventType IN ('Run', 'Walk', 'Cycle'))
);
GO

-- ==========================================
-- 3. CATEGORIES
-- ==========================================
CREATE TABLE Categories
(
    categoryID INT PRIMARY KEY IDENTITY(500,1),
    categoryName VARCHAR(50) NOT NULL,
    categoryAge INT NOT NULL,
    categoryDistance DECIMAL(8,2) NOT NULL,
    startTime TIME NOT NULL,
    eventID INT NOT NULL,

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (eventID)
        REFERENCES Events(eventID),

    CONSTRAINT CK_Categories_Age
        CHECK (categoryAge >= 0),

    CONSTRAINT CK_Categories_Distance
        CHECK (categoryDistance > 0),

    CONSTRAINT UQ_Category_Per_Event
        UNIQUE (eventID, categoryName, categoryDistance)
);
GO

-- ==========================================
-- 4. ENROLMENTS
-- Links the Participant (userID), the event (eventID)
-- and the selected category (categoryID).
-- ==========================================
CREATE TABLE Enrolments
(
    enrolmentID INT PRIMARY KEY IDENTITY(700,1),
    enrolmentDate DATE NOT NULL,
    userID INT NOT NULL,
    eventID INT NOT NULL,
    categoryID INT NOT NULL,

    CONSTRAINT FK_Enrolments_Users
        FOREIGN KEY (userID)
        REFERENCES Users(userID),

    CONSTRAINT FK_Enrolments_Events
        FOREIGN KEY (eventID)
        REFERENCES Events(eventID),

    CONSTRAINT FK_Enrolments_Categories
        FOREIGN KEY (categoryID)
        REFERENCES Categories(categoryID),

    CONSTRAINT UQ_User_Event
        UNIQUE (userID, eventID)
);
GO

-- ==========================================
-- 5. RESULTS
-- ==========================================
CREATE TABLE Results
(
    resultID INT PRIMARY KEY IDENTITY(900,1),
    finishingTime TIME NOT NULL,
    finishingPosition INT NOT NULL,
    enrolmentID INT NOT NULL UNIQUE,

    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (enrolmentID)
        REFERENCES Enrolments(enrolmentID),

    CONSTRAINT CK_Results_Position
        CHECK (finishingPosition > 0)
);
GO

-- ==========================================
-- SAMPLE DATA
-- 2 Organisers, 2 Participants, 3 Events,
-- 3 Categories, 3 Enrolments
-- ==========================================

-- Users: 100-101 organisers; 102-103 participants
INSERT INTO Users (fullName, emailAddress, passwordHash, userRole)
VALUES
    ('John Smith', 'john@email.com', '123', 'Organiser'),
    ('Sarah Jones', 'sarah@email.com', '456', 'Organiser'),
    ('Mike Brown', 'mike@email.com', '789', 'Participant'),
    ('Emily Davis', 'emily@email.com', '111', 'Participant');
GO

-- Events: 300-302
INSERT INTO Events
    (eventName, eventDescription, eventLocation, eventDistance, eventDate, eventType, organiserID)
VALUES
    ('City Run', 'Annual city running event', 'Cape Town', 10.00, '2026-09-20', 'Run', 100),
    ('Beach Half Marathon', 'Half marathon along the coast', 'Durban Beach', 21.10, '2026-10-04', 'Run', 101),
    ('Community Walk', 'Family friendly walking event', 'Johannesburg', 5.00, '2026-12-05', 'Walk', 100);
GO

-- Categories: 500-502 (one per event)
INSERT INTO Categories
    (categoryName, categoryAge, categoryDistance, startTime, eventID)
VALUES
    ('Senior 10K', 18, 10.00, '08:00:00', 300),
    ('Half Marathon', 18, 21.10, '07:00:00', 301),
    ('Family Walk 5K', 0, 5.00, '09:00:00', 302);
GO

-- Enrolments: 700-702 (participant, event, category)
INSERT INTO Enrolments (enrolmentDate, userID, eventID, categoryID)
VALUES
    ('2026-09-01', 102, 300, 500),
    ('2026-09-02', 103, 300, 500),
    ('2026-09-03', 103, 301, 501);
GO

-- Results: 900-902 (for the two events that have already taken place)
INSERT INTO Results (finishingTime, finishingPosition, enrolmentID)
VALUES
    ('00:48:35', 1, 700),
    ('00:52:10', 2, 701),
    ('01:45:25', 1, 702);
GO


select*from Users;