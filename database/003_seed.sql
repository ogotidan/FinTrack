USE `fintrack_db`;

DELETE FROM `transactions`;
DELETE FROM `portfolios`;
DELETE FROM `analysts`;

INSERT INTO `analysts` (`id`,`name`,`email`,`password_hash`,`is_active`,`created_at`,`last_login_at`) VALUES
(1,'Demo Analyst','analyst@fintrack.co.ke','$2a$12$Xl.T34YfdfVC1Gy/ix/r4OEnACDN3HGi553M0w./QQDPTL5btJzxi',1,'2026-11-25 13:17:51',NULL);

INSERT INTO `portfolios` (`id`,`analyst_id`,`name`,`client_name`,`base_currency`,`created_at`,`updated_at`) VALUES
(1,1,'NSE Growth Portfolio','Acme Capital Ltd','KES','2026-11-25 13:17:51','2026-11-25 13:17:51'),
(2,1,'Income & Fixed Portfolio','Mwangi Family Trust','KES','2026-11-25 13:17:51','2026-11-25 13:17:51'),
(3,1,'Diaspora USD Fund','Ochieng Holdings','USD','2026-11-25 13:17:51','2026-11-25 13:17:51');

INSERT INTO `transactions` (`id`,`portfolio_id`,`instrument`,`type`,`quantity`,`unit_price`,`trade_date`,`created_at`) VALUES
(1,1,'SCOM','BUY',1000.0000,15.5000,'2026-10-01','2026-11-25 13:17:51'),
(2,1,'SCOM','BUY',500.0000,16.2000,'2026-10-15','2026-11-25 13:17:51'),
(3,1,'KCB','BUY',800.0000,38.0000,'2026-10-05','2026-11-25 13:17:51'),
(4,1,'EQTY','BUY',600.0000,45.5000,'2026-10-20','2026-11-25 13:17:51'),
(5,1,'SCOM','SELL',300.0000,17.0000,'2026-11-01','2026-11-25 13:17:51'),
(6,1,'SCOM','BUY',100.0000,15.7500,'2026-11-20','2026-11-25 13:17:51'),
(7,2,'KCB','BUY',1200.0000,37.5000,'2026-09-28','2026-11-25 13:17:51'),
(8,2,'EQTY','BUY',1000.0000,44.0000,'2026-10-03','2026-11-25 13:17:51'),
(9,2,'T-BILL-91','BUY',50000.0000,1.0000,'2026-10-10','2026-11-25 13:17:51'),
(10,2,'KCB','SELL',200.0000,39.0000,'2026-11-05','2026-11-25 13:17:51'),
(11,3,'SCOM','BUY',2000.0000,0.1200,'2026-10-08','2026-11-25 13:17:51'),
(12,3,'EQTY','BUY',800.0000,0.3500,'2026-10-18','2026-11-25 13:17:51');
