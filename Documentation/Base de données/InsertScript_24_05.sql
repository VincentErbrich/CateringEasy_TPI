INSERT INTO `cateasy_bd`.`Table` (`IDTable`) VALUES (1);
INSERT INTO `cateasy_bd`.`Table` (`IDTable`) VALUES (2);
INSERT INTO `cateasy_bd`.`Table` (`IDTable`) VALUES (3);
INSERT INTO `cateasy_bd`.`Table` (`IDTable`) VALUES (4);
INSERT INTO `cateasy_bd`.`Table` (`IDTable`) VALUES (5);

INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (1, 'Poulet', 20, NULL, 15, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (2, 'Frites', 12, NULL, 26, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (3, 'Boeuf', 30, NULL, 10, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (4, 'Porc', 15, NULL, 12, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (5, 'Spaghettis', 15, NULL, 36, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (6, 'Riz', 10, NULL, 100, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (7, 'Pain', 5, NULL, 50, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (8, 'Fromage', 8, NULL, 100, 0);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (9, 'Eau', 1, NULL, 9000, 1);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (10, 'Thé', 3, NULL, 57, 1);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (11, 'Thé froid', 2, NULL, 250, 1);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (12, 'Coca', 2, NULL, 300, 1);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (13, 'Soda orange', 3, NULL, 210, 1);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (14, 'Jus d\'orange', 4, NULL, 340, 1);
INSERT INTO `cateasy_bd`.`MenuItem` (`IDMenuItem`, `Name`, `Price`, `Image`, `Remaining`, `IsDrink`) VALUES (15, 'Jus de pomme', 4, NULL, 120, 1);


INSERT INTO `cateasy_bd`.`Order` (`IDOrder`, `FIDTable`, `Completed`, `Paid`) VALUES (1, 2, 0, 0);
INSERT INTO `cateasy_bd`.`Order` (`IDOrder`, `FIDTable`, `Completed`, `Paid`) VALUES (2, 1, 0, 0);

INSERT INTO `cateasy_bd`.`Order_MItems` (`FIDMenuItem`, `FIDOrder`, `Delivered`, `Paid`) VALUES (1, 1, 0, 0);
INSERT INTO `cateasy_bd`.`Order_MItems` (`FIDMenuItem`, `FIDOrder`, `Delivered`, `Paid`) VALUES (9, 1, 0, 0);
INSERT INTO `cateasy_bd`.`Order_MItems` (`FIDMenuItem`, `FIDOrder`, `Delivered`, `Paid`) VALUES (10, 1, 0, 0);
INSERT INTO `cateasy_bd`.`Order_MItems` (`FIDMenuItem`, `FIDOrder`, `Delivered`, `Paid`) VALUES (3, 2, 0, 0);
INSERT INTO `cateasy_bd`.`Order_MItems` (`FIDMenuItem`, `FIDOrder`, `Delivered`, `Paid`) VALUES (4, 2, 0, 0);
INSERT INTO `cateasy_bd`.`Order_MItems` (`FIDMenuItem`, `FIDOrder`, `Delivered`, `Paid`) VALUES (8, 2, 0, 0);
