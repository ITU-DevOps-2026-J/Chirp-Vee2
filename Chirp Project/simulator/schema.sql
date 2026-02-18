DROP TABLE IF EXISTS Authors;
CREATE TABLE Authors (
                           "AuthorId" INTEGER NOT NULL CONSTRAINT "PK_Authors" PRIMARY KEY AUTOINCREMENT,
                           "Name" TEXT NOT NULL,
                           "Email" TEXT NOT NULL,
                           "Follows" TEXT NOT NULL,
                           "CheepLikes" TEXT NOT NULL
);

DROP TABLE IF EXISTS Latests;
CREATE TABLE Latests (
                           "LatestEntryId" INTEGER NOT NULL CONSTRAINT "PK_Latests" PRIMARY KEY AUTOINCREMENT,
                           "LatestCommandId" INTEGER NOT NULL,
                           "UpdatedDate" TEXT NOT NULL,
                           "CreatedDate" TEXT NOT NULL
);


DROP TABLE IF EXISTS Cheeps;
CREATE TABLE Cheeps (
                          "CheepId" INTEGER NOT NULL CONSTRAINT "PK_Cheeps" PRIMARY KEY AUTOINCREMENT,
                          "Text" TEXT NOT NULL,
                          "TimeStamp" TEXT NOT NULL,
                          "AuthorId" INTEGER NOT NULL,
                          "PeopleLikes" TEXT NOT NULL,
                          CONSTRAINT "FK_Cheeps_Authors_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Authors" ("AuthorId") ON DELETE CASCADE
);
