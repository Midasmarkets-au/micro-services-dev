-- Demo Data for IB Center 代理中心 Related Tables
-- This script creates sample data for testing IB functionality

-- Prerequite: Create demo users in auth."_User" table
INSERT INTO auth."_User" (
    "Id", 
    "Uid", 
    "PartyId", 
    "TenantId", 
    "ReferrerPartyId", 
    "NativeName", 
    "FirstName", 
    "LastName", 
    "Language", 
    "Avatar", 
    "TimeZone", 
    "ReferCode", 
    "CountryCode", 
    "Currency", 
    "CCC", 
    "Birthday", 
    "Gender", 
    "Status", 
    "Citizen", 
    "Address", 
    "IdType", 
    "IdNumber", 
    "IdIssuer", 
    "IdIssuedOn", 
    "IdExpireOn", 
    "CreatedOn", 
    "UpdatedOn", 
    "LastLoginOn", 
    "RegisteredIp", 
    "LastLoginIp", 
    "ReferPath",
    "UserName", 
    "NormalizedUserName", 
    "Email", 
    "NormalizedEmail", 
    "EmailConfirmed", 
    "PasswordHash", 
    "SecurityStamp", 
    "ConcurrencyStamp", 
    "PhoneNumber", 
    "PhoneNumberConfirmed", 
    "TwoFactorEnabled", 
    "LockoutEnd", 
    "LockoutEnabled", 
    "AccessFailedCount"
) VALUES
-- IB Agent
(85000001, 85000001, 85000001, 1, 0, 'Demo IB Agent', 'Demo', 'IB Agent', 'en', '', 'UTC', 'RAA001A3B4C', 'US', 'USD', 'USD', 
 '1990-01-01', 1, 0, 'US', '123 Demo Street, Demo City, Demo State 12345', 1, 'ID123456789', 'Demo Gov', 
 '2020-01-01', '2030-01-01', '2024-01-01 00:00:00', '2024-01-01 00:00:00', '2024-01-01 00:00:00', 
 '192.168.1.1', '192.168.1.1', '',
 'demo_ib_agent', 'DEMO_IB_AGENT', 'demo.ib.agent@example.com', 'DEMO.IB.AGENT@EXAMPLE.COM', true, 
 'AQAAAAEAACcQAAAAEKlgzJxOcGYxjYJtOYfIGzMpKQVnmvQWmhtCOvNfgVFrwXZ1nqWZPJLmJlEE7LDtYA==', 
 'DEMO_SECURITY_STAMP_001', 'demo-concurrency-001', '+1234567890', true, false, null, true, 0),

-- Client 1
(85000002, 85000002, 85000002, 1, 85000001, 'Demo Client 1', 'Demo', 'Client 1', 'en', '', 'UTC', 'RAC001D5E6F', 'US', 'USD', 'USD', 
 '1985-05-15', 0, 0, 'US', '456 Demo Avenue, Demo City, Demo State 12345', 1, 'ID987654321', 'Demo Gov', 
 '2020-01-01', '2030-01-01', '2024-01-02 00:00:00', '2024-01-02 00:00:00', '2024-01-02 00:00:00', 
 '192.168.1.2', '192.168.1.2', '85000001',
 'demo_client_1', 'DEMO_CLIENT_1', 'demo.client1@example.com', 'DEMO.CLIENT1@EXAMPLE.COM', true, 
 'AQAAAAEAACcQAAAAEKlgzJxOcGYxjYJtOYfIGzMpKQVnmvQWmhtCOvNfgVFrwXZ1nqWZPJLmJlEE7LDtYA==', 
 'DEMO_SECURITY_STAMP_002', 'demo-concurrency-002', '+1234567891', true, false, null, true, 0),

-- Client 2
(85000003, 85000003, 85000003, 1, 85000001, 'Demo Client 2', 'Demo', 'Client 2', 'en', '', 'UTC', 'RAC002G7H8I', 'US', 'USD', 'USD', 
 '1992-08-20', 1, 0, 'US', '789 Demo Boulevard, Demo City, Demo State 12345', 1, 'ID567890123', 'Demo Gov', 
 '2020-01-01', '2030-01-01', '2024-01-03 00:00:00', '2024-01-03 00:00:00', '2024-01-03 00:00:00', 
 '192.168.1.3', '192.168.1.3', '85000001',
 'demo_client_2', 'DEMO_CLIENT_2', 'demo.client2@example.com', 'DEMO.CLIENT2@EXAMPLE.COM', true, 
 'AQAAAAEAACcQAAAAEKlgzJxOcGYxjYJtOYfIGzMpKQVnmvQWmhtCOvNfgVFrwXZ1nqWZPJLmJlEE7LDtYA==', 
 'DEMO_SECURITY_STAMP_003', 'demo-concurrency-003', '+1234567892', true, false, null, true, 0),

-- Client 3
(85000004, 85000004, 85000004, 1, 85000001, 'Demo Client 3', 'Demo', 'Client 3', 'en', '', 'UTC', 'RAC003J9K0L', 'US', 'USD', 'USD', 
 '1988-12-10', 0, 0, 'US', '321 Demo Road, Demo City, Demo State 12345', 1, 'ID234567890', 'Demo Gov', 
 '2020-01-01', '2030-01-01', '2024-01-04 00:00:00', '2024-01-04 00:00:00', '2024-01-04 00:00:00', 
 '192.168.1.4', '192.168.1.4', '85000001',
 'demo_client_3', 'DEMO_CLIENT_3', 'demo.client3@example.com', 'DEMO.CLIENT3@EXAMPLE.COM', true, 
 'AQAAAAEAACcQAAAAEKlgzJxOcGYxjYJtOYfIGzMpKQVnmvQWmhtCOvNfgVFrwXZ1nqWZPJLmJlEE7LDtYA==', 
 'DEMO_SECURITY_STAMP_004', 'demo-concurrency-004', '+1234567893', true, false, null, true, 0),

-- Client 4
(85000005, 85000005, 85000005, 1, 85000001, 'Demo Client 4', 'Demo', 'Client 4', 'en', '', 'UTC', 'RAC004M1N2O', 'US', 'USD', 'USD', 
 '1995-03-25', 1, 0, 'US', '654 Demo Lane, Demo City, Demo State 12345', 1, 'ID345678901', 'Demo Gov', 
 '2020-01-01', '2030-01-01', '2024-01-05 00:00:00', '2024-01-05 00:00:00', '2024-01-05 00:00:00', 
 '192.168.1.5', '192.168.1.5', '85000001',
 'demo_client_4', 'DEMO_CLIENT_4', 'demo.client4@example.com', 'DEMO.CLIENT4@EXAMPLE.COM', true, 
 'AQAAAAEAACcQAAAAEKlgzJxOcGYxjYJtOYfIGzMpKQVnmvQWmhtCOvNfgVFrwXZ1nqWZPJLmJlEE7LDtYA==', 
 'DEMO_SECURITY_STAMP_005', 'demo-concurrency-005', '+1234567894', true, false, null, true, 0);

-- ==========================================
-- 3. ASSIGN ROLES TO USERS IN AUTH."_UserRole"
-- ==========================================
INSERT INTO auth."_UserRole" ("UserId", "RoleId") VALUES
-- IB Agent gets IB role
(85000001, 300),
-- All clients get Client role
(85000002, 400),
(85000003, 400),
(85000004, 400),
(85000005, 400);

-- Now start tenant database
-- First, let's create some basic Party and Account records
-- Note: Adjust IDs based on your existing data

-- ==========================================
-- 1. PARTY DATA (core schema)
-- ==========================================
INSERT INTO core."_Party" ("Id", "SiteId", "Code", "Name", "Note", "Email", "FirstName", "LastName", "Language", "CountryCode", "PhoneNumber", "Birthday", "Gender", "Status", "Uid", "CreatedOn", "UpdatedOn", "SearchText") VALUES 
(85000001, 1, '5f39f81a-1', 'Demo IB Agent', 'IB Agent Account', 'demo.ib.agent@example.com', 'Demo', 'Agent', 'en', 'US', '+1234567890', '1985-05-15', 1, 1, 85000001, '2024-01-01 00:00:00', '2024-01-01 00:00:00', 'Demo IB Agent demo.ib.agent@example.com 85000001'),
(85000002, 1, '5f39f81a-2', 'Demo Client 1', 'Client Account 1', 'demo.client1@example.com', 'Demo', 'Client1', 'en', 'US', '+1234567891', '1990-03-20', 0, 1, 85000002, '2024-01-02 00:00:00', '2024-01-02 00:00:00', 'Demo Client 1 demo.client1@example.com 85000002'),
(85000003, 1, '5f39f81a-3', 'Demo Client 2', 'Client Account 2', 'demo.client2@example.com', 'Demo', 'Client2', 'en', 'US', '+1234567892', '1988-07-10', 1, 1, 85000003, '2024-01-03 00:00:00', '2024-01-03 00:00:00', 'Demo Client 2 demo.client2@example.com 85000003'),
(85000004, 1, '5f39f81a-4', 'Demo Client 3', 'Client Account 3', 'demo.client3@example.com', 'Demo', 'Client3', 'en', 'US', '+1234567893', '1992-11-25', 0, 1, 85000004, '2024-01-04 00:00:00', '2024-01-04 00:00:00', 'Demo Client 3 demo.client3@example.com 85000004'),
(85000005, 1, '5f39f81a-5', 'Demo Client 4', 'Client Account 4', 'demo.client4@example.com', 'Demo', 'Client4', 'en', 'US', '+1234567894', '1987-09-05', 1, 1, 85000005, '2024-01-05 00:00:00', '2024-01-05 00:00:00', 'Demo Client 4 demo.client4@example.com 85000005');

-- ==========================================
-- 2. ACCOUNT DATA (trd schema)
-- ==========================================
INSERT INTO trd."_Account" ("Id", "Uid", "PartyId", "Role", "Type", "ServiceId", "FundType", "CurrencyId", "Level", "HasLevelRule", "AccountNumber", "AgentAccountId", "ReferrerAccountId", "HasTradeAccount", "Status", "CreatedOn", "UpdatedOn", "Name", "Code", "Group", "ReferCode", "ReferPath", "SearchText", "Permission", "IsClosed", "SiteId") VALUES 
-- IB Agent Account
(20001, 85000001, 85000001, 110, 4, 30, 1, 840, 1, 0, 0, NULL, NULL, false, 0, '2024-01-01 00:00:00', '2024-01-01 00:00:00', 'Demo IB Agent', 'DIA001', 'AGENT', 'RAA001', '.85000001', 'Demo IB Agent demo.ib.agent@example.com DIA001', '11111', 0, 1),
-- Client Accounts under IB
(20002, 85000002, 85000002, 100, 4, 30, 1, 840, 2, 0, 0, NULL, 20001, false, 0, '2024-01-02 00:00:00', '2024-01-02 00:00:00', 'Demo Client 1', 'DC001', 'CLIENT', 'RAC001', '.85000001.85000002', 'Demo Client 1 demo.client1@example.com DC001', '11111', 0, 1),
(20003, 85000003, 85000003, 100, 4, 30, 1, 840, 2, 0, 0, NULL, 20001, false, 0, '2024-01-03 00:00:00', '2024-01-03 00:00:00', 'Demo Client 2', 'DC002', 'CLIENT', 'RAC002', '.85000001.85000003', 'Demo Client 2 demo.client2@example.com DC002', '11111', 0, 1),
(20004, 85000004, 85000004, 100, 4, 30, 1, 840, 2, 0, 0, NULL, 20001, false, 0, '2024-01-04 00:00:00', '2024-01-04 00:00:00', 'Demo Client 3', 'DC003', 'CLIENT', 'RAC003', '.85000001.85000004', 'Demo Client 3 demo.client3@example.com DC003', '11111', 0, 1),
(20005, 85000005, 85000005, 100, 4, 30, 1, 840, 2, 0, 0, NULL, 20001, false, 0, '2024-01-05 00:00:00', '2024-01-05 00:00:00', 'Demo Client 4', 'DC004', 'CLIENT', 'RAC004', '.85000001.85000005', 'Demo Client 4 demo.client4@example.com DC004', '11111', 0, 1);

-- ==========================================
-- 3. REFERRAL CODE DATA
-- ==========================================
INSERT INTO core."_ReferralCode" ("Id", "Code", "Name", "PartyId", "AccountId", "ServiceType", "Summary", "CreatedOn", "UpdatedOn") VALUES 
(2001, 'RAA001A3B4C', 'Demo IB Agent Referral Code', 85000001, 20001, 1, '{"name":"Demo IB Agent Referral Code","language":"en","siteId":1}', '2024-01-01 00:00:00', '2024-01-01 00:00:00'),
(2002, 'RAC001D5E6F', 'Demo Client Referral Code 1', 85000002, 20002, 0, '{"name":"Demo Client Referral Code 1","language":"en","siteId":1}', '2024-01-02 00:00:00', '2024-01-02 00:00:00'),
(2003, 'RAC002G7H8I', 'Demo Client Referral Code 2', 85000003, 20003, 0, '{"name":"Demo Client Referral Code 2","language":"en","siteId":1}', '2024-01-03 00:00:00', '2024-01-03 00:00:00'),
(2004, 'RAC003J9K0L', 'Demo Client Referral Code 3', 85000004, 20004, 0, '{"name":"Demo Client Referral Code 3","language":"en","siteId":1}', '2024-01-04 00:00:00', '2024-01-04 00:00:00'),
(2005, 'RAC004M1N2O', 'Demo Client Referral Code 4', 85000005, 20005, 0, '{"name":"Demo Client Referral Code 4","language":"en","siteId":1}', '2024-01-05 00:00:00', '2024-01-05 00:00:00');

-- ==========================================
-- 4. REFERRAL DATA (for /ib/new-customers endpoint)
-- ==========================================
INSERT INTO core."_Referral" ("Id", "RowId", "ReferralCodeId", "ReferrerPartyId", "ReferredPartyId", "CreatedOn", "Code", "Module") VALUES 
(2001, 85000002, 2001, 85000001, 85000002, '2024-01-02 00:00:00', 'RAA001A3B4C', 'User'),
(2002, 85000003, 2001, 85000001, 85000003, '2024-01-03 00:00:00', 'RAA001A3B4C', 'User'),
(2003, 85000004, 2001, 85000001, 85000004, '2024-01-04 00:00:00', 'RAA001A3B4C', 'User'),
(2004, 85000005, 2001, 85000001, 85000005, '2024-01-05 00:00:00', 'RAA001A3B4C', 'User');

-- ==========================================
-- 5. PAYMENT SERVICE DATA
-- ==========================================

-- Insert PaymentService records
INSERT INTO acct."_PaymentService" ("Id", "Platform", "CurrencyId", "Sequence", "IsActivated", "CanDeposit", "CanWithdraw", "InitialValue", "MinValue", "MaxValue", "Name", "Description", "CategoryName", "Configuration", "IsHighDollarEnabled", "CommentCode", "IsAutoDepositEnabled") VALUES 
(1, 1, 840, 1, 1, 1, 1, 0, 10000, 1000000, 'Bank Transfer', 'Bank wire transfer', 'Banking', '{"type":"bank_transfer","fees":{"fixed":0,"percentage":0}}', 1, 'BANK', 0),
(2, 1, 840, 2, 1, 1, 1, 0, 5000, 500000, 'Credit Card', 'Credit card payment', 'Card', '{"type":"credit_card","fees":{"fixed":250,"percentage":2.5}}', 0, 'CARD', 0),
(3, 1, 840, 3, 1, 1, 0, 0, 10000, 2000000, 'Cryptocurrency', 'Bitcoin payment', 'Crypto', '{"type":"crypto","fees":{"fixed":0,"percentage":1}}', 1, 'CRYPTO', 0),
(
    10093,                                    -- Same ID as PaymentMethod to match foreign key
    420,                                      -- ChinaPay platform type from PaymentPlatformTypes enum
    156,                                      -- CNY currency (Chinese Yuan)
    1,                                        -- Sequence
    1,                                        -- IsActivated (enabled)
    1,                                        -- CanDeposit (enabled)
    0,                                        -- CanWithdraw (disabled for P2P deposit-only)
    0,                                        -- InitialValue
    10000,                                    -- MinValue (100 CNY minimum)
    1000000,                                  -- MaxValue (10,000 CNY maximum)
    'ChinaPay (P2P)',                        -- Name
    'Union Pay ChinaPay P2P payment service', -- Description
    'P2P',                                   -- CategoryName
    '{"type":"chinapay","mode":"p2p","platform":"unionpay"}', -- Configuration JSON
    1,                                        -- IsHighDollarEnabled
    'CPAY',                                   -- CommentCode
    0                                         -- IsAutoDepositEnabled
)
-- ON CONFLICT ("Id") DO NOTHING;

-- Insert PaymentMethod records  
INSERT INTO acct."_PaymentMethod" ("Id", "Platform", "MethodType", "CurrencyId", "Percentage", "InitialValue", "MinValue", "MaxValue", "Name", "Configuration", "CommentCode", "IsHighDollarEnabled", "IsAutoDepositEnabled", "Group", "Logo", "Note", "Status", "CreatedOn", "UpdatedOn", "OperatorPartyId", "AvailableCurrencies") VALUES 
(1, 1, 'deposit', 840, 100, 0, 10000, 1000000, 'Bank Transfer', '{"type":"bank_transfer"}', 'BANK', 1, 0, 'Banking', '', 'Bank wire transfer method', 1, NOW(), NOW(), 85000001, '[840]'),
(2, 1, 'deposit', 840, 100, 0, 5000, 500000, 'Credit Card', '{"type":"credit_card"}', 'CARD', 0, 0, 'Card', '', 'Credit card payment method', 1, NOW(), NOW(), 85000001, '[840]'),
(3, 1, 'deposit', 840, 100, 0, 10000, 2000000, 'Cryptocurrency', '{"type":"crypto"}', 'CRYPTO', 1, 0, 'Crypto', '', 'Cryptocurrency payment method', 1, NOW(), NOW(), 85000001, '[840]');
-- ON CONFLICT ("Id") DO NOTHING;

-- ==========================================
-- 6. REBATE AGENT RULE DATA (trd."_RebateAgentRule")
-- ==========================================
INSERT INTO trd."_RebateAgentRule" ("Id", "ParentId", "AgentAccountId", "CreatedOn", "UpdatedOn", "Schema", "LevelSetting") VALUES 
(2001, NULL, 20001, '2024-01-01 00:00:00', '2024-01-01 00:00:00', 
'[{"AccountType":4,"Items":[{"CategoryId":1,"Rate":0.8,"Pips":8e-5,"Commission":8},{"CategoryId":2,"Rate":0.9,"Pips":0.009,"Commission":9},{"CategoryId":3,"Rate":0.85,"Pips":0.085,"Commission":8.5}]}]',
'{"distributionType":1,"allowedAccounts":[{"accountType":4,"optionName":"Standard","pips":8e-5,"commission":8,"items":[{"categoryId":1,"rate":0.8,"pips":8e-5,"commission":8},{"categoryId":2,"rate":0.9,"pips":0.009,"commission":9},{"categoryId":3,"rate":0.85,"pips":0.085,"commission":8.5}]}],"percentageSetting":{},"isRoot":true}'),

-- Additional RebateAgentRule for AgentAccountId 10018
(2002, NULL, 10018, '2024-01-01 00:00:00', '2024-01-01 00:00:00', 
'[{"AccountType":4,"Items":[{"CategoryId":1,"Rate":0.8,"Pips":8e-5,"Commission":8},{"CategoryId":2,"Rate":0.9,"Pips":0.009,"Commission":9},{"CategoryId":3,"Rate":0.85,"Pips":0.085,"Commission":8.5}]}]',
'{"distributionType":1,"allowedAccounts":[{"accountType":4,"optionName":"Standard","pips":8e-5,"commission":8,"items":[{"categoryId":1,"rate":0.8,"pips":8e-5,"commission":8},{"categoryId":2,"rate":0.9,"pips":0.009,"commission":9},{"categoryId":3,"rate":0.85,"pips":0.085,"commission":8.5}]}],"percentageSetting":{},"isRoot":true}');

-- ==========================================
-- 7. DEPOSIT DATA (acct."_Deposit")
-- ==========================================
-- First, create Matter records for deposits (required for IdNavigation foreign key)
INSERT INTO core."_Matter" ("Id", "Type", "PostedOn", "StateId", "StatedOn") VALUES 
(25001, 300, '2024-01-06 10:00:00', 350, '2024-01-06 10:05:00'),
(25002, 300, '2024-01-07 11:00:00', 350, '2024-01-07 11:05:00'),
(25003, 300, '2024-01-08 12:00:00', 345, '2024-01-08 12:05:00'),
(25004, 300, '2024-01-09 13:00:00', 350, '2024-01-09 13:05:00'),
(25005, 300, '2024-01-10 14:00:00', 345, '2024-01-10 14:05:00');

-- Create Payment records for deposits
INSERT INTO acct."_Payment" ("Id", "PartyId", "LedgerSide", "PaymentServiceId", "CurrencyId", "Amount", "CreatedOn", "UpdatedOn", "Status", "Number", "ReferenceNumber", "CallbackBody") VALUES 
(25001, 85000002, 1, 1, 840, 100000, '2024-01-06 10:00:00', '2024-01-06 10:05:00', 1, 'PAY001-25001', 'DEP001', '{"method":"bank_transfer","bank":"Chase Bank"}'),
(25002, 85000003, 1, 1, 840, 250000, '2024-01-07 11:00:00', '2024-01-07 11:05:00', 1, 'PAY002-25002', 'DEP002', '{"method":"bank_transfer","bank":"Bank of America"}'),
(25003, 85000004, 1, 2, 840, 500000, '2024-01-08 12:00:00', '2024-01-08 12:05:00', 1, 'PAY003-25003', 'DEP003', '{"method":"credit_card","card":"****1234"}'),
(25004, 85000005, 1, 1, 840, 150000, '2024-01-09 13:00:00', '2024-01-09 13:05:00', 1, 'PAY004-25004', 'DEP004', '{"method":"bank_transfer","bank":"Wells Fargo"}'),
(25005, 85000002, 1, 3, 840, 300000, '2024-01-10 14:00:00', '2024-01-10 14:05:00', 1, 'PAY005-25005', 'DEP005', '{"method":"crypto","wallet":"Bitcoin"}');

-- Insert Deposit records
INSERT INTO acct."_Deposit" ("Id", "Type", "PartyId", "PaymentId", "FundType", "CurrencyId", "Amount", "TargetAccountId", "ReferenceNumber") VALUES 
(25001, 1, 85000002, 25001, 1, 840, 100000, 20002, 'DEP001'),
(25002, 1, 85000003, 25002, 1, 840, 250000, 20003, 'DEP002'),
(25003, 1, 85000004, 25003, 1, 840, 500000, 20004, 'DEP003'),
(25004, 1, 85000005, 25004, 1, 840, 150000, 20005, 'DEP004'),
(25005, 1, 85000002, 25005, 1, 840, 300000, 20002, 'DEP005');

-- ==========================================
-- 8. WITHDRAWAL DATA (acct."_Withdrawal")
-- ==========================================
-- Create Matter records for withdrawals
INSERT INTO core."_Matter" ("Id", "Type", "PostedOn", "StateId", "StatedOn") VALUES 
(26001, 400, '2024-01-11 15:00:00', 450, '2024-01-11 15:05:00'),
(26002, 400, '2024-01-12 16:00:00', 450, '2024-01-12 16:05:00'),
(26003, 400, '2024-01-13 17:00:00', 450, '2024-01-13 17:05:00'),
(26004, 400, '2024-01-14 18:00:00', 450, '2024-01-14 18:05:00');

-- Create Payment records for withdrawals
INSERT INTO acct."_Payment" ("Id", "PartyId", "LedgerSide", "PaymentServiceId", "CurrencyId", "Amount", "CreatedOn", "UpdatedOn", "Status", "Number", "ReferenceNumber", "CallbackBody") VALUES 
(26001, 85000002, -1, 1, 840, 50000, '2024-01-11 15:00:00', '2024-01-11 15:05:00', 1, 'WPAY001-26001', 'WTH001', '{"method":"bank_transfer","bank":"Chase Bank"}'),
(26002, 85000003, -1, 1, 840, 100000, '2024-01-12 16:00:00', '2024-01-12 16:05:00', 1, 'WPAY002-26002', 'WTH002', '{"method":"bank_transfer","bank":"Bank of America"}'),
(26003, 85000004, -1, 2, 840, 75000, '2024-01-13 17:00:00', '2024-01-13 17:05:00', 1, 'WPAY003-26003', 'WTH003', '{"method":"credit_card","card":"****1234"}'),
(26004, 85000005, -1, 1, 840, 80000, '2024-01-14 18:00:00', '2024-01-14 18:05:00', 1, 'WPAY004-26004', 'WTH004', '{"method":"bank_transfer","bank":"Wells Fargo"}');

-- Insert Withdrawal records
INSERT INTO acct."_Withdrawal" ("Id", "PartyId", "PaymentId", "FundType", "CurrencyId", "Amount", "ReferenceNumber", "SourceAccountId", "ExchangeRate", "ApprovedOn") VALUES 
(26001, 85000002, 26001, 1, 840, 50000, 'WTH001', 20002, 1.0, '2024-01-11 15:05:00'),
(26002, 85000003, 26002, 1, 840, 100000, 'WTH002', 20003, 1.0, '2024-01-12 16:05:00'),
(26003, 85000004, 26003, 1, 840, 75000, 'WTH003', 20004, 1.0, '2024-01-13 17:05:00'),
(26004, 85000005, 26004, 1, 840, 80000, 'WTH004', 20005, 1.0, '2024-01-14 18:05:00');

-- ==========================================
-- 9. REBATE DATA (acct."_Rebate")
-- ==========================================
-- Create Matter records for rebates
INSERT INTO core."_Matter" ("Id", "Type", "PostedOn", "StateId", "StatedOn") VALUES 
(27001, 500, '2024-01-15 09:00:00', 550, '2024-01-15 09:05:00'),
(27002, 500, '2024-01-16 10:00:00', 550, '2024-01-16 10:05:00'),
(27003, 500, '2024-01-17 11:00:00', 510, '2024-01-17 11:05:00'),
(27004, 500, '2024-01-18 12:00:00', 550, '2024-01-18 12:05:00'),
(27005, 500, '2024-01-19 13:00:00', 550, '2024-01-19 13:05:00');

-- First, create TradeService records (required for TradeRebate)
-- INSERT INTO trd."_TradeService" ("Id", "Name", "Description", "Status", "CreatedOn", "UpdatedOn") VALUES 
--(30, 'MT5 Trading Service', 'MetaTrader 5 trading platform', 1, NOW(), NOW())
--ON CONFLICT ("Id") DO NOTHING;

-- Create TradeRebate records with correct schema
INSERT INTO trd."_TradeRebate" ("Id", "AccountId", "TradeServiceId", "Ticket", "AccountNumber", "CurrencyId", "Volume", "Status", "RuleType", "CreatedOn", "UpdatedOn", "ClosedOn", "OpenedOn", "TimeStamp", "Action", "Symbol", "DealId", "ClosePrice", "Commission", "OpenPrice", "Profit", "Swaps", "ReferPath", "Reason") VALUES 
(27001, 20002, 30, 100001, 85000002, 840, 100, 1, 1, '2024-01-15 09:00:00', '2024-01-15 09:05:00', '2024-01-15 09:05:00', '2024-01-15 09:00:00', 1705310400000, 1, 'EURUSD', 100001, 1.0970, 8.0, 1.0950, 200.0, 0.0, '.85000001.85000002', 0),
(27002, 20003, 30, 100002, 85000003, 840, 50, 1, 1, '2024-01-16 10:00:00', '2024-01-16 10:05:00', '2024-01-16 10:05:00', '2024-01-16 10:00:00', 1705396800000, 1, 'GBPUSD', 100002, 1.2770, 4.0, 1.2750, 100.0, 0.0, '.85000001.85000003', 0),
(27003, 20004, 30, 100003, 85000004, 840, 200, 0, 1, '2024-01-17 11:00:00', '2024-01-17 11:05:00', '2024-01-17 11:05:00', '2024-01-17 11:00:00', 1705483200000, 1, 'USDJPY', 100003, 150.30, 16.0, 150.50, -400.0, 0.0, '.85000001.85000004', 0),
(27004, 20005, 30, 100004, 85000005, 840, 150, 1, 1, '2024-01-18 12:00:00', '2024-01-18 12:05:00', '2024-01-18 12:05:00', '2024-01-18 12:00:00', 1705569600000, 1, 'AUDUSD', 100004, 0.6670, 12.0, 0.6650, 300.0, 0.0, '.85000001.85000005', 0),
(27005, 20002, 30, 100005, 85000002, 840, 100, 1, 1, '2024-01-19 13:00:00', '2024-01-19 13:05:00', '2024-01-19 13:05:00', '2024-01-19 13:00:00', 1705656000000, 1, 'USDCAD', 100005, 1.3430, 8.0, 1.3450, 200.0, 0.0, '.85000001.85000002', 0);

-- Insert Rebate records
INSERT INTO trd."_Rebate" ("Id", "PartyId", "AccountId", "FundType", "CurrencyId", "Amount", "TradeRebateId", "HoldUntilOn", "Information") VALUES 
(27001, 85000001, 20001, 1, 840, 16000, 27001, NULL, '{"type":"agent_rebate","ticket":100001,"symbol":"EURUSD","volume":100}'),
(27002, 85000001, 20001, 1, 840, 8000, 27002, NULL, '{"type":"agent_rebate","ticket":100002,"symbol":"GBPUSD","volume":50}'),
(27003, 85000001, 20001, 1, 840, 32000, 27003, '2024-01-20 11:05:00', '{"type":"agent_rebate","ticket":100003,"symbol":"USDJPY","volume":200,"on_hold":true}'),
(27004, 85000001, 20001, 1, 840, 24000, 27004, NULL, '{"type":"agent_rebate","ticket":100004,"symbol":"AUDUSD","volume":150}'),
(27005, 85000001, 20001, 1, 840, 16000, 27005, NULL, '{"type":"agent_rebate","ticket":100005,"symbol":"USDCAD","volume":100}');

-- ==========================================
-- 10. TRADE ACCOUNT DATA (for completeness)
-- ==========================================
INSERT INTO trd."_TradeAccount" ("Id", "ServiceId", "AccountNumber", "CurrencyId", "LastSyncedOn", "CreatedOn", "UpdatedOn", "RebateBaseSchemaId") VALUES 
(20001, 30, 85000001, 840, '2024-01-01 00:05:00', '2024-01-01 00:00:00', '2024-01-01 00:00:00', NULL),
(20002, 30, 85000002, 840, '2024-01-02 00:05:00', '2024-01-02 00:00:00', '2024-01-02 00:00:00', NULL),
(20003, 30, 85000003, 840, '2024-01-03 00:05:00', '2024-01-03 00:00:00', '2024-01-03 00:00:00', NULL),
(20004, 30, 85000004, 840, '2024-01-04 00:05:00', '2024-01-04 00:00:00', '2024-01-04 00:00:00', NULL),
(20005, 30, 85000005, 840, '2024-01-05 00:05:00', '2024-01-05 00:00:00', '2024-01-05 00:00:00', NULL);

-- ==========================================
-- 11. REFERENCE DATA (Currency, States, etc.)
-- ==========================================
-- These might already exist, but including for completeness

-- Currency (USD = 840)
-- INSERT INTO core."_Currency" ("Id", "Name", "Code", "Symbol", "DecimalPlace") VALUES 
-- (840, 'US Dollar', 'USD', '$', 2) 
-- ON CONFLICT ("Id") DO NOTHING;

-- States for Deposits
-- INSERT INTO core."_State" ("Id", "Name", "Type", "Actions") VALUES 
-- (350, 'DepositCompleted', 'Deposit', '[]'),
-- (345, 'DepositTenantApproved', 'Deposit', '[]')
-- ON CONFLICT ("Id") DO NOTHING;

-- States for Withdrawals
-- INSERT INTO core."_State" ("Id", "Name", "Type", "Actions") VALUES 
-- (450, 'WithdrawalCompleted', 'Withdrawal', '[]')
-- ON CONFLICT ("Id") DO NOTHING;

-- States for Rebates
-- INSERT INTO core."_State" ("Id", "Name", "Type", "Actions") VALUES 
-- (550, 'RebateCompleted', 'Rebate', '[]'),
-- (510, 'RebateOnHold', 'Rebate', '[]')
-- ON CONFLICT ("Id") DO NOTHING;

-- ==========================================
-- SUMMARY
-- ==========================================
-- This script creates:
-- 1. 1 Demo IB Agent (UID: 85000001) with 4 Client accounts under them
-- 2. Referral codes and referral relationships
-- 3. 1 RebateAgentRule for the IB
-- 4. 5 Deposit records (some completed, some approved)
-- 5. 4 Withdrawal records (all completed)
-- 6. 5 Rebate records (1 on hold, 4 completed)
-- 7. 5 TradeRebate records linked to the rebates
-- 8. 5 TradeAccount records for MT5 accounts
-- 9. Supporting reference data (PaymentService, PaymentMethod)

-- To test the endpoints, use:
-- GET /api/v1/ib/85000001/account?page=1&size=15&sortField=createdOn&sortFlag=true&relativeLevel=1
-- GET /api/v1/ib/85000001/referral/user-history?page=1&size=10&IsUnverified=true
-- GET /api/v1/ib/85000001/rebate?page=1&size=20
-- GET /api/v1/ib/85000001/deposit?page=1&size=25&StateIds=350&StateIds=345
-- GET /api/v1/ib/85000001/withdrawal?page=1&size=25&isClosed=false&StateIds=450
-- GET /api/v1/ib/85000001/rebate-rule/detail 