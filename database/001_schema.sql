/*
    FinTrack — Portfolio & Transactions Tracker
    001_schema.sql — database + tables

    Conventions follow parking_db: MySQL 8, InnoDB, utf8mb4_0900_ai_ci,
    snake_case, AUTO_INCREMENT PKs, created_at/updated_at timestamps,
    explicit foreign-key constraints with indexes.

    Run this ONCE against your MySQL server. It creates the fintrack_db
    database and all tables. Safe to re-run (drops tables first).
*/

CREATE DATABASE IF NOT EXISTS `fintrack_db`
    DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

USE `fintrack_db`;

/* Drop in child -> parent order so foreign keys don't block the drop */
DROP TABLE IF EXISTS `transactions`;
DROP TABLE IF EXISTS `portfolios`;
DROP TABLE IF EXISTS `analysts`;

/* ------------------------------------------------------------------ */
/*  analysts — the application users (investment operations analysts)  */
/*  Passwords are stored ONLY as BCrypt hashes, never in plain text.   */
/* ------------------------------------------------------------------ */
CREATE TABLE `analysts` (
    `id`            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `name`          VARCHAR(150)    NOT NULL,
    `email`         VARCHAR(150)    NOT NULL,
    `password_hash` VARCHAR(255)    NOT NULL,
    `is_active`     TINYINT(1)      NOT NULL DEFAULT 1,
    `created_at`    DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `last_login_at` DATETIME        NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_analysts_email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------ */
/*  portfolios — one row per client portfolio, owned by an analyst.    */
/*  base_currency drives how money is formatted in the UI.             */
/* ------------------------------------------------------------------ */
CREATE TABLE `portfolios` (
    `id`            BIGINT UNSIGNED   NOT NULL AUTO_INCREMENT,
    `analyst_id`    BIGINT UNSIGNED   NOT NULL,
    `name`          VARCHAR(150)      NOT NULL,
    `client_name`   VARCHAR(150)      NOT NULL,
    `base_currency` ENUM('KES','USD') NOT NULL DEFAULT 'KES',
    `created_at`    DATETIME          NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`    DATETIME          NOT NULL DEFAULT CURRENT_TIMESTAMP
                                      ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_portfolios_analyst` (`analyst_id`),
    CONSTRAINT `fk_portfolios_analyst`
        FOREIGN KEY (`analyst_id`) REFERENCES `analysts` (`id`)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------ */
/*  transactions — buy/sell trades recorded against a portfolio.       */
/*  quantity/unit_price use DECIMAL(18,4) so fractional units and      */
/*  precise prices are kept exactly (never floating-point money).      */
/* ------------------------------------------------------------------ */
CREATE TABLE `transactions` (
    `id`           BIGINT UNSIGNED    NOT NULL AUTO_INCREMENT,
    `portfolio_id` BIGINT UNSIGNED    NOT NULL,
    `instrument`   VARCHAR(30)        NOT NULL,          -- e.g. SCOM, KCB, EQTY, T-BILL-91
    `type`         ENUM('BUY','SELL') NOT NULL,
    `quantity`     DECIMAL(18,4)      NOT NULL,
    `unit_price`   DECIMAL(18,4)      NOT NULL,
    `trade_date`   DATE               NOT NULL,
    `created_at`   DATETIME           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_transactions_portfolio`  (`portfolio_id`),
    KEY `idx_transactions_instrument` (`portfolio_id`, `instrument`),
    CONSTRAINT `fk_transactions_portfolio`
        FOREIGN KEY (`portfolio_id`) REFERENCES `portfolios` (`id`)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
