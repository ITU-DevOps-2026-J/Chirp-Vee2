-- Roles table
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                                             "Id" TEXT PRIMARY KEY,
                                             "Name" TEXT,
                                             "NormalizedName" TEXT,
                                             "ConcurrencyStamp" TEXT
);

-- Users table
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
                                             "Id" TEXT PRIMARY KEY,
                                             "UserName" TEXT,
                                             "NormalizedUserName" TEXT,
                                             "Email" TEXT,
                                             "NormalizedEmail" TEXT,
                                             "EmailConfirmed" BOOLEAN NOT NULL,
                                             "PasswordHash" TEXT,
                                             "SecurityStamp" TEXT,
                                             "ConcurrencyStamp" TEXT,
                                             "PhoneNumber" TEXT,
                                             "PhoneNumberConfirmed" BOOLEAN NOT NULL,
                                             "TwoFactorEnabled" BOOLEAN NOT NULL,
                                             "LockoutEnd" TIMESTAMP,
                                             "LockoutEnabled" BOOLEAN NOT NULL,
                                             "AccessFailedCount" INTEGER NOT NULL
);

-- Authors table
CREATE TABLE IF NOT EXISTS "Authors" (
                                         "AuthorId" SERIAL PRIMARY KEY,
                                         "Name" TEXT NOT NULL,
                                         "Email" TEXT NOT NULL,
                                         "Follows" TEXT NOT NULL,
                                         "CheepLikes" TEXT NOT NULL
);

-- Latests table
CREATE TABLE IF NOT EXISTS "Latests" (
                                         "LatestEntryId" SERIAL PRIMARY KEY,
                                         "LatestCommandId" INTEGER NOT NULL,
                                         "UpdatedDate" TIMESTAMP NOT NULL,
                                         "CreatedDate" TIMESTAMP NOT NULL
);

-- Role claims
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
                                                  "Id" SERIAL PRIMARY KEY,
                                                  "RoleId" TEXT NOT NULL REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    "ClaimType" TEXT,
    "ClaimValue" TEXT
    );

-- User claims
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
                                                  "Id" SERIAL PRIMARY KEY,
                                                  "UserId" TEXT NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "ClaimType" TEXT,
    "ClaimValue" TEXT
    );

-- User logins
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
                                                  "LoginProvider" TEXT NOT NULL,
                                                  "ProviderKey" TEXT NOT NULL,
                                                  "ProviderDisplayName" TEXT,
                                                  "UserId" TEXT NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    PRIMARY KEY ("LoginProvider", "ProviderKey")
    );

-- User roles
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
                                                 "UserId" TEXT NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "RoleId" TEXT NOT NULL REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    PRIMARY KEY ("UserId", "RoleId")
    );

-- User tokens
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
                                                  "UserId" TEXT NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name")
    );

-- Cheeps
CREATE TABLE IF NOT EXISTS "Cheeps" (
                                        "CheepId" SERIAL PRIMARY KEY,
                                        "Text" TEXT NOT NULL,
                                        "TimeStamp" TIMESTAMP NOT NULL,
                                        "AuthorId" INTEGER NOT NULL REFERENCES "Authors" ("AuthorId") ON DELETE CASCADE,
    "PeopleLikes" TEXT NOT NULL
    );

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX IF NOT EXISTS "IX_Cheeps_AuthorId" ON "Cheeps" ("AuthorId");