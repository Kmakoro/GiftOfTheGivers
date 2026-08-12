-- Gift of the Givers Foundation - Azure SQL schema (first release)
-- Section B 2.2: core entities, attributes, primary and foreign keys.
-- ASP.NET Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, ...) are
-- created by EF Core migrations; only the domain tables are scripted here.

CREATE TABLE dbo.ReliefProjects (
    Id            INT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_ReliefProjects PRIMARY KEY,
    Name          NVARCHAR(150)      NOT NULL,
    Location      NVARCHAR(120)      NOT NULL,
    Description   NVARCHAR(1000)     NULL,
    StartDate     DATETIME2          NOT NULL,
    EndDate       DATETIME2          NULL,
    Status        INT                NOT NULL CONSTRAINT DF_ReliefProjects_Status DEFAULT (0),
    FundingGoal   DECIMAL(18,2)      NOT NULL CONSTRAINT DF_ReliefProjects_Goal DEFAULT (0)
);

CREATE TABLE dbo.ProjectUpdates (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProjectUpdates PRIMARY KEY,
    ReliefProjectId INT               NOT NULL,
    Title           NVARCHAR(200)     NOT NULL,
    Body            NVARCHAR(2000)    NOT NULL,
    PostedAt        DATETIME2         NOT NULL,
    PostedByUserId  NVARCHAR(450)     NULL,
    CONSTRAINT FK_ProjectUpdates_ReliefProjects FOREIGN KEY (ReliefProjectId)
        REFERENCES dbo.ReliefProjects (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProjectUpdates_Users FOREIGN KEY (PostedByUserId)
        REFERENCES dbo.AspNetUsers (Id)
);

CREATE TABLE dbo.Volunteers (
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Volunteers PRIMARY KEY,
    FullName     NVARCHAR(120)     NOT NULL,
    Email        NVARCHAR(200)     NOT NULL,
    PhoneNumber  NVARCHAR(30)      NULL,
    Skills       NVARCHAR(300)     NOT NULL,
    Availability NVARCHAR(120)     NOT NULL,
    City         NVARCHAR(120)     NULL,
    RegisteredAt DATETIME2         NOT NULL,
    UserId       NVARCHAR(450)     NULL,
    CONSTRAINT UQ_Volunteers_Email UNIQUE (Email),
    CONSTRAINT FK_Volunteers_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id)
);

CREATE TABLE dbo.VolunteerAssignments (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VolunteerAssignments PRIMARY KEY,
    VolunteerId     INT               NOT NULL,
    ReliefProjectId INT               NOT NULL,
    Role            NVARCHAR(100)     NULL,
    AssignedAt      DATETIME2         NOT NULL,
    CONSTRAINT FK_VolunteerAssignments_Volunteers FOREIGN KEY (VolunteerId)
        REFERENCES dbo.Volunteers (Id) ON DELETE CASCADE,
    CONSTRAINT FK_VolunteerAssignments_ReliefProjects FOREIGN KEY (ReliefProjectId)
        REFERENCES dbo.ReliefProjects (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_VolunteerAssignments UNIQUE (VolunteerId, ReliefProjectId)
);

CREATE TABLE dbo.Donations (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Donations PRIMARY KEY,
    Reference       NVARCHAR(40)      NOT NULL,
    Amount          DECIMAL(18,2)     NOT NULL,
    Currency        INT               NOT NULL CONSTRAINT DF_Donations_Currency DEFAULT (0), -- 0=ZAR 1=USD 2=EUR
    Frequency       INT               NOT NULL CONSTRAINT DF_Donations_Frequency DEFAULT (0), -- 0=OneTime 1=Monthly
    DonorName       NVARCHAR(120)     NOT NULL,
    DonorEmail      NVARCHAR(200)     NULL,
    IsAnonymous     BIT               NOT NULL CONSTRAINT DF_Donations_Anon DEFAULT (0),
    DonatedAt       DATETIME2         NOT NULL,
    UserId          NVARCHAR(450)     NULL,
    ReliefProjectId INT               NULL,
    CONSTRAINT UQ_Donations_Reference UNIQUE (Reference),
    CONSTRAINT CK_Donations_Amount CHECK (Amount > 0),
    CONSTRAINT FK_Donations_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id),
    CONSTRAINT FK_Donations_ReliefProjects FOREIGN KEY (ReliefProjectId)
        REFERENCES dbo.ReliefProjects (Id) ON DELETE SET NULL
);

CREATE TABLE dbo.TaxCertificates (
    Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaxCertificates PRIMARY KEY,
    DonationId        INT               NOT NULL,
    CertificateNumber NVARCHAR(40)      NOT NULL,
    IssuedAt          DATETIME2         NOT NULL,
    CONSTRAINT UQ_TaxCertificates_Donation UNIQUE (DonationId),
    CONSTRAINT UQ_TaxCertificates_Number UNIQUE (CertificateNumber),
    CONSTRAINT FK_TaxCertificates_Donations FOREIGN KEY (DonationId)
        REFERENCES dbo.Donations (Id) ON DELETE CASCADE
);

-- Section B 2.3: performance
CREATE INDEX IX_Donations_DonatedAt          ON dbo.Donations (DonatedAt) INCLUDE (Amount, Currency);
CREATE INDEX IX_Donations_Project_DonatedAt  ON dbo.Donations (ReliefProjectId, DonatedAt) INCLUDE (Amount);
CREATE INDEX IX_ProjectUpdates_Project_Posted ON dbo.ProjectUpdates (ReliefProjectId, PostedAt DESC);
CREATE INDEX IX_Volunteers_RegisteredAt      ON dbo.Volunteers (RegisteredAt);
CREATE INDEX IX_Volunteers_City              ON dbo.Volunteers (City);
