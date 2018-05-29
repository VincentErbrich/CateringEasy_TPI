-- MySQL Script generated by MySQL Workbench
-- 05/22/18 13:40:07
-- Model: New Model    Version: 1.0
-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Schema cateasy_bd
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema cateasy_bd
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `cateasy_bd` DEFAULT CHARACTER SET utf8 ;
USE `cateasy_bd` ;

-- -----------------------------------------------------
-- Table `cateasy_bd`.`Table`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `cateasy_bd`.`Table` (
  `IDTable` INT NOT NULL,
  PRIMARY KEY (`IDTable`));


-- -----------------------------------------------------
-- Table `cateasy_bd`.`Order`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `cateasy_bd`.`Order` (
  `IDOrder` INT NOT NULL,
  `FIDTable` INT NOT NULL,
  `Completed` TINYINT(1) NOT NULL DEFAULT 0,
  `Paid` TINYINT(1) NOT NULL DEFAULT 0,
  `Started` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`IDOrder`),
  INDEX `fk_Order_Table_idx` (`FIDTable` ASC),
  CONSTRAINT `fk_Order_Table`
    FOREIGN KEY (`FIDTable`)
    REFERENCES `cateasy_bd`.`Table` (`IDTable`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);


-- -----------------------------------------------------
-- Table `cateasy_bd`.`MenuItem`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `cateasy_bd`.`MenuItem` (
  `IDMenuItem` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(50) NOT NULL,
  `Price` FLOAT UNSIGNED NOT NULL,
  `Image` BLOB NULL,
  `Remaining` SMALLINT UNSIGNED NOT NULL,
  `IsDrink` TINYINT(1) NOT NULL,
  PRIMARY KEY (`IDMenuItem`));


-- -----------------------------------------------------
-- Table `cateasy_bd`.`Settings`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `cateasy_bd`.`Settings` (
  `Password` VARCHAR(255) NOT NULL DEFAULT 'admin',
  `Currency` VARCHAR(6) NOT NULL DEFAULT 'CHF');


-- -----------------------------------------------------
-- Table `cateasy_bd`.`Order_MItems`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `cateasy_bd`.`Order_MItems` (
  `FIDMenuItem` INT NOT NULL,
  `FIDOrder` INT NOT NULL,
  `Completed` TINYINT(1) NOT NULL DEFAULT 0,
  INDEX `fk_Order_MItems_MenuItem1_idx` (`FIDMenuItem` ASC),
  INDEX `fk_Order_MItems_Order1_idx` (`FIDOrder` ASC),
  CONSTRAINT `fk_Order_MItems_MenuItem1`
    FOREIGN KEY (`FIDMenuItem`)
    REFERENCES `cateasy_bd`.`MenuItem` (`IDMenuItem`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Order_MItems_Order1`
    FOREIGN KEY (`FIDOrder`)
    REFERENCES `cateasy_bd`.`Order` (`IDOrder`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
