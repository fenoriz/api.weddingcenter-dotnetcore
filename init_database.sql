USE master;
GO

ALTER DATABASE [qltc] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE IF EXISTS qltc;
CREATE DATABASE [qltc];
GO

USE qltc;
GO

DROP TABLE IF EXISTS [dbo].[Users]
Create table Users (
    Id int IDENTITY(1,1),
    Username VARCHAR(25) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Phone VARCHAR(15) NOT NULL UNIQUE,
    FirstName NVARCHAR(25) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    IdentityNumber VARCHAR(20),
	CreatedDate DATETIME DEFAULT GETDATE(),
    Role VARCHAR(10),

    CONSTRAINT Pk_UserId
    PRIMARY KEY (Id)
);

SET IDENTITY_INSERT [dbo].[Users] ON;
GO

INSERT INTO Users(Id, Username, Password, Phone, FirstName, LastName, IdentityNumber, Role) VALUES 
(1, 'admin', '202cb962ac59075b964b07152d234b70', '0987654321', N'Ad', N'Vui tính', '1234567890', 'ADMIN'),
(2, 'tiennguyen', '202cb962ac59075b964b07152d234b70', '0000000006', N'Tiến', N'Nguyễn', '0123124773', 'EMPLOYEE'),
(3, 'ngocmy', '202cb962ac59075b964b07152d234b70', '0000000001', N'Mỹ', N'Ngọc', '0938392837', 'EMPLOYEE'),
(4, 'dangnguyen', '202cb962ac59075b964b07152d234b70', '0000000002', N'Đăng', N'Nguyễn', '0928291282', 'USER'),
(5, 'user1', '202cb962ac59075b964b07152d234b70', '0000000003', N'User', N'1', '0000032000', 'USER'),
(6, 'user2', '202cb962ac59075b964b07152d234b70', '0000000004', N'User', N'2', '0203400000', 'USER'),
(7, 'user3', '202cb962ac59075b964b07152d234b70', '0000000005', N'User', N'3', '0000400000', 'USER'),
(8, 'user4', '202cb962ac59075b964b07152d234b70', '0000000010', N'User', N'4', '0000004500', 'USER'),
(9, 'user5', '202cb962ac59075b964b07152d234b70', '0000000008', N'User', N'5', '0000004070', 'USER'),
(10, 'guestdefault', '202cb962ac59075b964b07152d234b70', '0000000007', N'Khách', N'Vãng Lai', '0000000000', 'USER');

SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

DROP TABLE IF EXISTS [dbo].[Employees]
Create table Employees (
    Id int IDENTITY(1, 1),
    DateOfBirth DATETIME NOT NULL,
    Picture Image,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UserId int NOT NULL,

    CONSTRAINT Pk_EmployeeId
    PRIMARY KEY (Id),
    CONSTRAINT FK_User
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

INSERT INTO Employees(DateOfBirth, Picture, UserId) VALUES ('2001-01-01 00:00:00', NULL, 1), 
('2002-01-01 00:00:00', NULL, 2), 
('2000-01-01 00:00:00', NULL, 3); 

DROP TABLE IF EXISTS [dbo].[Halls]
Create table Halls (
    Id int IDENTITY(1, 1),
    Name NVARCHAR(70) UNIQUE NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    TableNumber int NOT NULL,
    GuestUpTo int NOT NULL,
    Price decimal NOT NULL,
    Discount decimal DEFAULT 0,
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT Pk_HallId
    PRIMARY KEY (Id)
);

INSERT INTO Halls VALUES (N'Sảnh nhỏ', N'Chưa có mô tả', 20, 200, 20000000, 0, 1),
(N'Sảnh trung', N'Chưa có mô tả', 30, 340, 30000000, 0, 1),
(N'Sảnh tầm cỡ', N'Chưa có mô tả', 37, 400, 38000000, 0, 1),
(N'Sảnh đại', N'Chưa có mô tả', 50, 500, 49900000, 0, 1);


DROP TABLE IF EXISTS [dbo].[Weddings]
Create table Weddings (
    Id int IDENTITY(1, 1),
    CustomerId int NOT NULL,
    StaffId int,
    Description NVARCHAR(MAX) NOT NULL,
    TableNumber int NOT NULL,
    GuestNumber int NOT NULL,
    Deposit decimal DEFAULT 0,
    DepositVia NVARCHAR(100),
    DepositReceiptNo VARCHAR(255),
    PaidDate DATE, --- when charge all fee
    ReceiptNo VARCHAR(255),
    PaidVia VARCHAR(100),
    CelebrityDate DATETIME NULL,
    HallId int NOT NULL,
    HallPrice decimal NOT NULL,
    HallDiscount decimal NOT NULL, 
    CreatedDate DATETIME, -- when charge deposit

    CONSTRAINT Pk_WeddingId
    PRIMARY KEY (Id),
    CONSTRAINT Fk_HallId
    FOREIGN KEY (HallId) REFERENCES Halls(Id),
	CONSTRAINT Fk_CustomerId
	FOREIGN KEY (CustomerId) REFERENCES Users(Id),
	CONSTRAINT Fk_StaffId
	FOREIGN KEY (StaffId) REFERENCES Users(Id),
);


SET IDENTITY_INSERT [dbo].[Weddings] ON;
GO

 INSERT INTO Weddings (Id, CustomerId, StaffId, Description, TableNumber, GuestNumber, Deposit, DepositVia, DepositReceiptNo, PaidDate
	, ReceiptNo, PaidVia, CelebrityDate, HallId, HallPrice, HallDiscount, CreatedDate) VALUES 
 (1, 4, 1, N'', 20, 190, 30000000, N'Cash', NULL, '2023-07-26 09:00:00', '37y7sdf7823r0dsfh023', 'Momo', '2023-07-25 11:00:00', 1, 20000000, 0, '2023-07-05 12:00:00'),
 (2, 5, 2, N'', 25, 230, 30000000, N'Cash', NULL, '2023-07-25 09:00:00', '83h8d723ijs0df939002', 'Momo', '2023-07-23 11:00:00', 2, 30000000, 0, '2023-07-04 15:00:00'),
 (3, 8, 3, N'', 20, 190, 30000000, N'Cash', NULL, '2023-07-30 09:00:00', '578gdhjsdhsgddgsg323', 'VNPAY', '2023-07-28 11:00:00', 1, 20000000, 0, '2023-07-01 08:00:00'),
 (4, 9, 1, N'', 45, 240, 50000000, N'Cash', NULL, '2023-08-18 09:00:00', 'dh54ufdfh34tghghfhgd', '9Pay', '2023-08-15 11:00:00', 4, 20000000, 0, '2023-08-01 16:00:00'),
 (5, 4, 1, N'', 20, 190, 35000000, N'Momo', '345345fgfsghrffshsh', '2023-09-12 09:00:00', '38gc682390hfi0320-jc', 'VNPAY', '2023-09-10 11:00:00', 1, 20000000, 0, '2023-08-23 09:00:00'),
 (6, 6, 2, N'', 20, 190, 30000000, N'Cash', NULL, '2023-09-15 09:00:00', 'fgdjj3234y65475sssd', 'Momo', '2023-09-14 11:00:00', 1, 20000000, 0, '2023-09-05 12:00:00'),
 (7, 7, 1, N'', 34, 320, 43000000, N'Cash', NULL, NULL, NULL, NULL, '2023-09-20 11:00:00', 3, 38000000, 0, '2023-09-10 12:00:00'),
 (8, 5, NULL, N'', 20, 190, 0, NULL, NULL, NULL, NULL, NULL, '2023-09-25 11:00:00', 1, 20000000, 0, NULL),
 (9, 9, NULL, N'', 20, 180, 0, NULL, NULL, NULL, NULL, NULL, '2023-10-29 11:00:00', 1, 20000000, 0, NULL);
 
 GO


SET IDENTITY_INSERT [dbo].[Weddings] OFF;
GO

DROP TABLE IF EXISTS [dbo].[Dishes]
Create table Dishes (
    Id int IDENTITY(1, 1),
    Name NVARCHAR(70) UNIQUE NOT NULL,
    Description NVARCHAR(255),
    Price decimal NOT NULL,
    Discount float DEFAULT 0,
    Image Image,
    IsActive BIT DEFAULT 1,

    CONSTRAINT Pk_DishId
    PRIMARY KEY (Id)
);

INSERT INTO Dishes VALUES (N'Khai vị miền quê', N'Chưa có mô tả', 450000, 0, NULL, 1),
(N'Lẩu gà lá é', N'Chưa có mô tả', 470000, 0, NULL, 1),
(N'Xôi gà nướng vỉ', N'Chưa có mô tả', 430000, 0, NULL, 1),
(N'Cơm xá lợi phúc', N'Chưa có mô tả', 410000, 0, NULL, 1),
(N'Súp cua hải sản', N'Chưa có mô tả', 490000, 0, NULL, 1),
(N'Lẩu cù lao', N'Chưa có mô tả', 440000, 0, NULL, 1),
(N'Bò nhúng me', N'Chưa có mô tả', 490000, 0, NULL, 1),
(N'Cơm chiên dương châu', N'Chưa có mô tả', 400000, 0, NULL, 1),
(N'Cá nướng bánh trán cuộn', N'Chưa có mô tả', 430000, 0, NULL, 1),
(N'Bò mỡ chài', N'Chưa có mô tả', 550000, 0, NULL, 1),
(N'Tôm hấp bia', N'Chưa có mô tả', 520000, 0, NULL, 1),
(N'Gà sốt mắm nguyên con', N'Chưa có mô tả', 430000, 0, NULL, 1);

DROP TABLE IF EXISTS [dbo].[_DishInWedding]
Create table _DishInWedding (
    Id int IDENTITY(1,1),
    WeddingId int NOT NULL,
    DishId int NOT NULL,
    DishPrice decimal NOT NULL,
    DishDiscount float NOT NULL,
    Quantity int DEFAULT 1,

    CONSTRAINT Pk_DishInWeddingId
    PRIMARY KEY (Id),
    CONSTRAINT Fk_Wedding
    FOREIGN KEY (WeddingId) REFERENCES Weddings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Dish
    FOREIGN KEY (DishId) REFERENCES Dishes(Id)
);

GO

INSERT INTO _DishInWedding VALUES
(1, 1, 450000, 0, 20),
(1, 2, 470000, 0, 20),
(1, 3, 430000, 0, 20),
(1, 4, 410000, 0.05, 20),
(1, 5, 490000, 0, 20),
(2, 1, 450000, 0, 25),
(2, 3, 430000, 0, 25),
(2, 4, 410000, 0, 25),
(2, 5, 490000, 0, 25),
(2, 6, 400000, 0, 25),
(3, 1, 450000, 0.03, 25),
(3, 3, 430000, 0, 25),
(3, 4, 410000, 0, 25),
(3, 5, 490000, 0, 25),
(3, 6, 400000, 0, 25),
(4, 1, 450000, 0, 45),
(4, 3, 430000, 0.02, 45),
(4, 4, 410000, 0, 45),
(4, 5, 490000, 0, 45),
(4, 6, 400000, 0, 45),
(5, 1, 450000, 0, 20),
(5, 2, 470000, 0, 20),
(5, 8, 400000, 0, 20),
(5, 9, 430000, 0, 20),
(5, 10, 550000, 0, 20),
(6, 1, 450000, 0, 20),
(6, 2, 470000, 0, 20),
(6, 8, 400000, 0, 20),
(6, 9, 430000, 0, 20),
(6, 10, 550000, 0, 20),
(7, 1, 450000, 0, 34),
(7, 2, 470000, 0, 34),
(7, 8, 400000, 0, 34),
(7, 9, 430000, 0.02, 34),
(7, 10, 550000, 0.1, 34),
(8, 1, 450000, 0, 20),
(8, 2, 470000, 0, 20),
(8, 8, 400000, 0, 20),
(8, 9, 430000, 0.02, 20),
(8, 10, 550000, 0.1, 20),
(9, 6, 450000, 0, 20),
(9, 7, 470000, 0, 20),
(9, 5, 480000, 0, 20),
(9, 2, 460000, 0, 20),
(9, 10, 490000, 0, 20);

GO
------END

CREATE PROCEDURE GetDishInPriceRange(@FromPrice decimal, @ToPrice decimal)
AS
BEGIN
	IF (@FromPrice IS NULL OR @FromPrice < 0) SET @FromPrice = 0
	IF (@ToPrice IS NULL OR @ToPrice <= 0)
		SELECT * FROM Dishes WHERE Price > @FromPrice
	ELSE
		SELECT * FROM Dishes WHERE Price > @FromPrice AND Price < @ToPrice
END
GO

CREATE PROCEDURE GetUserByRole(@Role VARCHAR(20))
AS
BEGIN
	IF (@Role IS NULL)
		RETURN
	SELECT * FROM Users WHERE Role = @Role
END
GO

CREATE PROCEDURE GetRoles
AS
BEGIN
	SELECT Role FROM Users GROUP BY Role;
END
GO

CREATE PROCEDURE Top5EmployeesOrder(@Month int, @Year int)
AS
BEGIN
	IF (@Year IS NULL)
		SET @Year = YEAR(GETDATE())
	IF (@Month IS NULL)
		SET @Month = MONTH(GETDATE())
	
	SELECT * FROM Users WHERE Id IN (SELECT TOP 5 StaffId FROM (SELECT Id, StaffId, (HallPrice - HallDiscount) as HallPrice FROM Weddings WHERE YEAR(CreatedDate) = @Year AND MONTH(CreatedDate) = @Month) as w
		JOIN (SELECT WeddingId, SUM((1 - DishDiscount) * DishPrice * Quantity) as DishSum FROM _DishInWedding GROUP BY WeddingId) as diw 
		ON diw.WeddingId = w.Id GROUP BY StaffId)
END
GO

CREATE PROCEDURE FindPendingWeddingInDays(@DayLeft int)
AS
BEGIN
	SET @DayLeft = ABS(@DayLeft)
	SELECT * FROM Weddings WHERE CelebrityDate > CURRENT_TIMESTAMP AND CelebrityDate <= DATEADD(DAY, @DayLeft, GETDATE()) ORDER BY CelebrityDate DESC
END
GO

------- USER's FUNCTION --------

CREATE FUNCTION CalculateTotalOfWedding(@WeddingId int)
RETURNS DECIMAL
BEGIN
	DECLARE @Total DECIMAL
	IF NOT EXISTS (SELECT Id FROM Weddings WHERE Id = @WeddingId)
		RETURN 0
	SELECT @Total = (HallSum + DishSum) FROM (SELECT Id, (HallPrice - HallDiscount) AS HallSum FROM Weddings WHERE Id = @WeddingId) as w
	JOIN (SELECT WeddingId, SUM(DishPrice * Quantity * (1 - DishDiscount)) AS DishSum FROM _DishInWedding WHERE WeddingId = @WeddingId GROUP BY WeddingId) as diw 
	ON w.Id = diw.WeddingId
	RETURN @Total
END
GO

CREATE FUNCTION WeddingRevenueStatistics(@Year int, @Month int)
RETURNS @Result TABLE (Month int, Sum Decimal) 
BEGIN
	IF (@Year IS NULL)
		SET @Year = YEAR(GETDATE())
	
	IF (@Month IS NULL) -- Get stats in year
			INSERT INTO @Result SELECT MONTH(CreatedDate) as Month, SUM(DishSum + HallPrice) AS Sum
			FROM (SELECT Id, (HallPrice - HallDiscount) as HallPrice, CreatedDate FROM Weddings WHERE YEAR(CreatedDate) = @Year) as w
				JOIN (SELECT WeddingId, SUM(Quantity * DishPrice * (1 - DishDiscount)) as DishSum FROM _DishInWedding GROUP BY WeddingId) AS diw 
				ON w.Id = diw.WeddingId GROUP BY MONTH(CreatedDate)
	ELSE -- Get stats in month of year
		INSERT INTO @Result SELECT MONTH(CreatedDate) as Month, SUM(DishSum + HallPrice) AS Sum 
		FROM (SELECT Id, (HallPrice - HallDiscount) as HallPrice, CreatedDate FROM Weddings WHERE YEAR(CreatedDate) = @Year AND MONTH(CreatedDate) = @Month) as w
			JOIN (SELECT WeddingId, SUM(Quantity * DishPrice * (1 - DishDiscount)) as DishSum FROM _DishInWedding GROUP BY WeddingId) AS diw 
			ON w.Id = diw.WeddingId GROUP BY MONTH(CreatedDate)
	RETURN
END
GO

CREATE FUNCTION GetWeddingRevenueStatsOfMonth(@Month int, @Year int)
RETURNS @Result TABLE (Day int, Revenue decimal)
BEGIN
	IF (@Year IS NULL)
		SET @Year = YEAR(GETDATE())
	IF (@Month IS NULL)
		SET @Month = MONTH(GETDATE())

	INSERT INTO @Result SELECT DAY(CreatedDate) as Day, SUM(DishSum + HallPrice) AS Revenue 
		FROM (SELECT Id, (HallPrice - HallDiscount) as HallPrice, CreatedDate FROM Weddings WHERE YEAR(CreatedDate) = @Year AND MONTH(CreatedDate) = @Month) as w
			JOIN (SELECT WeddingId, SUM(Quantity * DishPrice * (1 - DishDiscount)) as DishSum FROM _DishInWedding GROUP BY WeddingId) AS diw 
			ON w.Id = diw.WeddingId GROUP BY DAY(CreatedDate)

	RETURN
END
GO