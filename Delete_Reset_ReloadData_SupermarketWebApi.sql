USE SupermarketWebApi;
DELETE FROM Supermarkets;
GO
DElETE FROM Products;
GO
DELETE FROM StaffMembers;
GO
DElETE FROM SupermarketStocks;
GO

DBCC CHECKIDENT (Supermarkets, RESEED, 0)
GO
DBCC CHECKIDENT (Products, RESEED, 0)
GO
DBCC CHECKIDENT (StaffMembers, RESEED, 0)
GO
DBCC CHECKIDENT (SupermarketStocks, RESEED, 0)
GO

INSERT INTO 
Supermarkets(Location, NumberOfStaff)
VALUES
('Sydney', 202), 
('Brisbane', 178),
('Melbourne', 223),
('Gold Coast', 165),
('Adelaide', 149),
('Perth', 187);
SELECT * FROM Supermarkets

INSERT INTO 
Products(Name, Price)
VALUES
('Snickers', 1.70), 
('Mars Bar', 1.60),
('Picnic', 1.75),
('Turkish Delight', 2.05),
('Kit Kat', 1.45),
('Hershey Bar', 1.65), 
('Crunch', 1.20),
('Twix', 1.95),
('Crunchie', 2.25),
('Areo', 2.45),
('Bounty', 0.70), 
('Galaxy', 1.10),
('Lion Bar', 2.75),
('Twirl', 1.05),
('Curly Wurly', 0.45),
('Milky Way', 0.95);
SELECT * FROM Products

INSERT INTO 
StaffMembers(Name, PhoneNumber, Address, SupermarketId)
VALUES
('John', '0413749843', '1 Main St', 1), 
('Mike', '0412349856', '2 Main St', 1),
('Phil', '0404284573', '3 Main St', 1),
('Trish', '0402294691', '4 Main St', 2),
('Amy', '0417629998', '5 Main St', 2),
('James', '0413123456', '6 Main St', 2), 
('Robert', '0412234567', '7 Main St', 3),
('Michael', '0404345678', '8 Main St', 3),
('William', '0402456789', '9 Main St', 3),
('David', '0417567890', '10 Main St', 3),
('Ian', '0413098765', '11 Main St', 3), 
('Richard', '0412987654', '12 Main St', 3),
('Joseph', '0404876543', '13 Main St', 3),
('Thomas', '0402765432', '14 Main St', 3),
('Charles', '0417654321', '15 Main St', 4),
('Daniel', '0413934787', '16 Main St', 4), 
('Mark', '0412439857', '17 Main St', 5),
('Paul', '0404190328', '18 Main St', 5),
('Andrew', '0402348579', '19 Main St', 6),
('George', '0417983257', '20 Main St', 6),
('Sue', '0426112278', '21 Main St', 3);
SELECT * FROM StaffMembers

INSERT INTO
SupermarketStocks(SupermarketId, ProductId, NumberInStock)
VALUES
(1,1,245),
(1,2,574),
(1,11,874),
(1,15,257),
(1,16,25),
(2,1,179),
(2,2,268),
(2,3,947),
(2,4,22),
(2,6,477),
(3,1,889),
(3,2,764),
(3,3,257),
(3,6,249),
(3,7,215),
(3,8,745),
(3,10,249),
(3,11,285),
(3,12,235),
(3,13,595),
(3,14,5),
(3,16,335),
(4,3,233),
(4,7,271),
(4,9,27),
(4,12,212),
(5,13,455),
(5,15,969),
(5,16,727),
(5,2,663),
(6,1,579),
(6,4,462),
(6,8,333),
(6,12,911),
(6,14,808);
SELECT * FROM SupermarketStocks