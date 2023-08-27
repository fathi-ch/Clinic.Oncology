BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS "Patients"
(
    "Id"                   INTEGER,
    "FirstName"            TEXT NOT NULL,
    "LastName"             TEXT NOT NULL,
    "BirthDate"            DATE NOT NULL DEFAULT CURRENT_DATE,
    "NextAppointment"      DATE,
    "Weight"               REAL,
    "Height"               REAL,
    "Gender"               TEXT NOT NULL,
    "Mobile"               TEXT NOT NULL,
    "SocialSecurityNumber" TEXT,
    "Referral"             TEXT,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Visits"
(
    "Id"          INTEGER,
    "PatientId"   INTEGER NOT NULL,
    "StartTime"   Date    NOT NULL,
    "EndTime"     Date    NOT NULL,
    "Price"       REAL,
    "Description" TEXT,
    "VisitType"   TEXT,
    "Status"      TEXT,
    FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id"),
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Documents"
(
    "Id"           INTEGER,
    "VisitId"      INTEGER NOT NULL,
    "Name"         Date    NOT NULL,
    "DocumentType" Date    NOT NULL,
    FOREIGN KEY ("VisitId") REFERENCES "Visits" ("Id"),
    PRIMARY KEY ("Id")
);

COMMIT;
