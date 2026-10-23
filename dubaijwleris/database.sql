-- Dubai Jewel Manufacturing Database Setup Script
-- Database Name: hakam_barcode

CREATE DATABASE IF NOT EXISTS `hakam_barcode` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `hakam_barcode`;

-- --------------------------------------------------------
-- Table structure for table `roles`
-- --------------------------------------------------------
DROP TABLE IF EXISTS `users`;
DROP TABLE IF EXISTS `roles`;

CREATE TABLE `roles` (
  `Role_Id` INT NOT NULL AUTO_INCREMENT,
  `RoleName` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`Role_Id`),
  UNIQUE KEY `uk_rolename` (`RoleName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed data for `roles`
INSERT INTO `roles` (`Role_Id`, `RoleName`) VALUES
(1, 'Admin'),
(2, 'User');

-- --------------------------------------------------------
-- Table structure for table `users`
-- --------------------------------------------------------
CREATE TABLE `users` (
  `User_Id` VARCHAR(50) NOT NULL,
  `Username` VARCHAR(100) NOT NULL,
  `Role_Id` INT NOT NULL,
  `UserPassword` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`User_Id`),
  KEY `fk_users_roles` (`Role_Id`),
  CONSTRAINT `fk_users_roles` FOREIGN KEY (`Role_Id`) REFERENCES `roles` (`Role_Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed data for `users`
INSERT INTO `users` (`User_Id`, `Username`, `Role_Id`, `UserPassword`) VALUES
('1', 'Admin', 1, 'admin'),
('admin', 'Admin User', 1, 'admin'),
('2', 'Standard User', 2, 'user123');

-- --------------------------------------------------------
-- Table structure for table `products`
-- --------------------------------------------------------
DROP TABLE IF EXISTS `products`;

CREATE TABLE `products` (
  `ProductType` VARCHAR(100) NOT NULL,
  `Carat` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`ProductType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed data for `products`
INSERT INTO `products` (`ProductType`, `Carat`) VALUES
('Ring', '21'),
('Necklace', '22'),
('Bangle', '18'),
('Bracelet', '21'),
('Earrings', '18');

-- --------------------------------------------------------
-- Table structure for table `product_data`
-- --------------------------------------------------------
DROP TABLE IF EXISTS `product_data`;

CREATE TABLE `product_data` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `ProductType` VARCHAR(100) DEFAULT NULL,
  `Carat` VARCHAR(50) DEFAULT NULL,
  `Weight` VARCHAR(50) DEFAULT NULL,
  `BarcodeFormat` VARCHAR(100) DEFAULT NULL,
  `SerialNumber` INT DEFAULT NULL,
  `Date` DATE DEFAULT NULL,
  `Time` VARCHAR(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed data for `product_data`
INSERT INTO `product_data` (`ProductType`, `Carat`, `Weight`, `BarcodeFormat`, `SerialNumber`, `Date`, `Time`) VALUES
('Ring', '21', '5.20', 'Code128', 1001, CURDATE(), '10:30:00'),
('Necklace', '22', '12.50', 'Code128', 1002, CURDATE(), '11:15:00'),
('Bangle', '18', '8.75', 'Code128', 1003, CURDATE(), '14:20:00');
