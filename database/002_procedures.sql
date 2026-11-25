USE `fintrack_db`;

DROP PROCEDURE IF EXISTS `sp_register_analyst`;
DELIMITER $$
CREATE PROCEDURE `sp_register_analyst`(
    OUT p_out_id        BIGINT UNSIGNED,
    IN  p_name          VARCHAR(150),
    IN  p_email         VARCHAR(150),
    IN  p_password_hash VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM analysts
               WHERE email = p_email COLLATE utf8mb4_0900_ai_ci) THEN
        SET p_out_id = 0;
    ELSE
        INSERT INTO analysts (name, email, password_hash)
        VALUES (p_name, p_email, p_password_hash);
        SET p_out_id = LAST_INSERT_ID();
    END IF;
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_validate_login`;
DELIMITER $$
CREATE PROCEDURE `sp_validate_login`(IN p_email VARCHAR(150))
BEGIN
    SELECT id, name, email, password_hash, is_active
    FROM analysts
    WHERE email = p_email COLLATE utf8mb4_0900_ai_ci
    LIMIT 1;
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_update_analyst_last_login`;
DELIMITER $$
CREATE PROCEDURE `sp_update_analyst_last_login`(IN p_analyst_id BIGINT UNSIGNED)
BEGIN
    UPDATE analysts SET last_login_at = NOW() WHERE id = p_analyst_id;
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_add_portfolio`;
DELIMITER $$
CREATE PROCEDURE `sp_add_portfolio`(
    OUT p_out_id       BIGINT UNSIGNED,
    IN  p_analyst_id   BIGINT UNSIGNED,
    IN  p_name         VARCHAR(150),
    IN  p_client_name  VARCHAR(150),
    IN  p_base_currency VARCHAR(3)
)
BEGIN
    INSERT INTO portfolios (analyst_id, name, client_name, base_currency)
    VALUES (p_analyst_id, p_name, p_client_name, p_base_currency);
    SET p_out_id = LAST_INSERT_ID();
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_get_portfolios`;
DELIMITER $$
CREATE PROCEDURE `sp_get_portfolios`(IN p_analyst_id BIGINT UNSIGNED)
BEGIN
    SELECT
        p.id, p.name, p.client_name, p.base_currency,
        p.created_at, p.updated_at,
        (SELECT COUNT(*) FROM transactions t WHERE t.portfolio_id = p.id)
            AS transaction_count
    FROM portfolios p
    WHERE p.analyst_id = p_analyst_id
    ORDER BY p.created_at DESC;
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_get_portfolio_by_id`;
DELIMITER $$
CREATE PROCEDURE `sp_get_portfolio_by_id`(
    IN p_id         BIGINT UNSIGNED,
    IN p_analyst_id BIGINT UNSIGNED
)
BEGIN
    SELECT
        p.id, p.name, p.client_name, p.base_currency,
        p.created_at, p.updated_at
    FROM portfolios p
    WHERE p.id = p_id AND p.analyst_id = p_analyst_id
    LIMIT 1;
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_update_portfolio`;
DELIMITER $$
CREATE PROCEDURE `sp_update_portfolio`(
    OUT p_out_id        BIGINT UNSIGNED,
    IN  p_id            BIGINT UNSIGNED,
    IN  p_analyst_id    BIGINT UNSIGNED,
    IN  p_name          VARCHAR(150),
    IN  p_client_name   VARCHAR(150),
    IN  p_base_currency VARCHAR(3)
)
BEGIN
    UPDATE portfolios
    SET name          = p_name,
        client_name   = p_client_name,
        base_currency = p_base_currency,
        updated_at    = NOW()
    WHERE id = p_id AND analyst_id = p_analyst_id;
    SET p_out_id = ROW_COUNT();
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_delete_portfolio`;
DELIMITER $$
CREATE PROCEDURE `sp_delete_portfolio`(
    OUT p_out_id     BIGINT UNSIGNED,
    IN  p_id         BIGINT UNSIGNED,
    IN  p_analyst_id BIGINT UNSIGNED
)
BEGIN
    DELETE FROM portfolios
    WHERE id = p_id AND analyst_id = p_analyst_id;
    SET p_out_id = ROW_COUNT();
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_add_transaction`;
DELIMITER $$
CREATE PROCEDURE `sp_add_transaction`(
    OUT p_out_id      BIGINT UNSIGNED,
    IN  p_portfolio_id BIGINT UNSIGNED,
    IN  p_instrument  VARCHAR(30),
    IN  p_type        VARCHAR(4),
    IN  p_quantity    DECIMAL(18,4),
    IN  p_unit_price  DECIMAL(18,4),
    IN  p_trade_date  DATE
)
BEGIN
    DECLARE v_held DECIMAL(18,4) DEFAULT 0;

    IF p_quantity <= 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Quantity must be greater than zero';
    END IF;

    IF p_unit_price < 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Unit price cannot be negative';
    END IF;

    IF p_type = 'SELL' THEN
        SELECT COALESCE(SUM(CASE WHEN type = 'BUY'  THEN quantity
                                 WHEN type = 'SELL' THEN -quantity
                                 ELSE 0 END), 0)
        INTO v_held
        FROM transactions
        WHERE portfolio_id = p_portfolio_id
          AND instrument   = p_instrument COLLATE utf8mb4_0900_ai_ci;

        IF p_quantity > v_held THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Cannot sell more units than are currently held';
        END IF;
    END IF;

    INSERT INTO transactions
        (portfolio_id, instrument, type, quantity, unit_price, trade_date)
    VALUES
        (p_portfolio_id, p_instrument, p_type, p_quantity, p_unit_price, p_trade_date);

    SET p_out_id = LAST_INSERT_ID();
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_get_transactions`;
DELIMITER $$
CREATE PROCEDURE `sp_get_transactions`(IN p_portfolio_id BIGINT UNSIGNED)
BEGIN
    SELECT
        id, portfolio_id, instrument, type,
        quantity, unit_price,
        ROUND(quantity * unit_price, 2) AS total_amount,
        trade_date, created_at
    FROM transactions
    WHERE portfolio_id = p_portfolio_id
    ORDER BY trade_date DESC, id DESC;
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_delete_transaction`;
DELIMITER $$
CREATE PROCEDURE `sp_delete_transaction`(
    OUT p_out_id     BIGINT UNSIGNED,
    IN  p_id         BIGINT UNSIGNED,
    IN  p_analyst_id BIGINT UNSIGNED
)
BEGIN
    DELETE t FROM transactions t
    JOIN portfolios p ON p.id = t.portfolio_id
    WHERE t.id = p_id AND p.analyst_id = p_analyst_id;
    SET p_out_id = ROW_COUNT();
END $$
DELIMITER ;

DROP PROCEDURE IF EXISTS `sp_get_portfolio_summary`;
DELIMITER $$
CREATE PROCEDURE `sp_get_portfolio_summary`(IN p_portfolio_id BIGINT UNSIGNED)
BEGIN
    SELECT
        x.instrument,
        x.net_quantity,
        ROUND(x.average_cost, 4)                  AS average_cost,
        ROUND(x.net_quantity * x.average_cost, 2) AS total_invested,
        ROUND(x.latest_price, 4)                  AS latest_price,
        ROUND(x.net_quantity * x.latest_price, 2) AS total_value
    FROM (
        SELECT
            t.instrument,
            SUM(CASE WHEN t.type = 'BUY'  THEN t.quantity ELSE 0 END)
          - SUM(CASE WHEN t.type = 'SELL' THEN t.quantity ELSE 0 END) AS net_quantity,
            CASE
                WHEN SUM(CASE WHEN t.type = 'BUY' THEN t.quantity ELSE 0 END) > 0
                THEN SUM(CASE WHEN t.type = 'BUY' THEN t.quantity * t.unit_price ELSE 0 END)
                   / SUM(CASE WHEN t.type = 'BUY' THEN t.quantity ELSE 0 END)
                ELSE 0
            END AS average_cost,
            (SELECT t2.unit_price
             FROM transactions t2
             WHERE t2.portfolio_id = p_portfolio_id
               AND t2.instrument   = t.instrument
             ORDER BY t2.trade_date DESC, t2.id DESC
             LIMIT 1) AS latest_price
        FROM transactions t
        WHERE t.portfolio_id = p_portfolio_id
        GROUP BY t.instrument
    ) x
    WHERE x.net_quantity > 0
    ORDER BY x.instrument ASC;
END $$
DELIMITER ;
