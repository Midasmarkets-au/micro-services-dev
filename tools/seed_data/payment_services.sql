-- PaymentService INSERT script to fix withdrawal constraint violations
-- These records will allow PaymentMethod IDs 150, 151, 154 to be used as PaymentServiceId

-- Insert PaymentService records with matching IDs
INSERT INTO acct."_PaymentService" (
    "Id", 
    "Platform", 
    "CurrencyId", 
    "Sequence", 
    "IsActivated", 
    "CanDeposit", 
    "CanWithdraw", 
    "InitialValue", 
    "MinValue", 
    "MaxValue", 
    "Name", 
    "Description", 
    "CategoryName", 
    "Configuration", 
    "IsHighDollarEnabled", 
    "CommentCode", 
    "IsAutoDepositEnabled"
) VALUES 

-- PaymentMethod ID 150: Wire (International) Withdrawal
(150, 100, 840, 1, 1, 1, 1, 0, 10000, 1000000, 
 'Wire (International) Service', 
 'International wire transfer withdrawal service', 
 'Banking', 
 '{"type":"wire_international","fees":{"fixed":0,"percentage":0},"processing_time":"1-3 business days"}', 
 1, 'WIRE', 0),

-- PaymentMethod ID 151: UnionPay Withdrawal (using Platform 100 = Wire, as UnionPay is wire-based)
(151, 100, 840, 2, 1, 1, 1, 0, 10000, 1000000, 
 'UnionPay Wire Service', 
 'UnionPay wire transfer withdrawal service', 
 'Banking', 
 '{"type":"unionpay_wire","fees":{"fixed":0,"percentage":0},"processing_time":"instant","network":"unionpay"}', 
 1, 'UPAY', 0),

-- PaymentMethod ID 152: UnionPay Withdrawal (using Platform 100 = Wire, as UnionPay is wire-based)
(152, 100, 841, 2, 1, 1, 1, 0, 10000, 1000000, 
 'USDT (USC) Service', 
 'USDT (USC) withdrawal service', 
 'Crypto', 
 '{"type":"crypto","protocol":"USC","token":"USDT","network":"TRON","confirmations_required":1,"wallet_type":"hot","fees":{"fixed":0,"percentage":0.5},"limits":{"daily_withdraw":5000000}}', 
 1, 'UPAY', 0),

-- PaymentMethod ID 154: USDT (TRC20) Withdrawal
(154, 100, 840, 3, 1, 1, 1, 0, 10000, 5000000, 
 'USDT (TRC20) Service', 
 'USDT TRC20 withdrawal service', 
 'Crypto', 
 '{"type":"crypto","protocol":"TRC20","token":"USDT","network":"TRON","confirmations_required":1,"wallet_type":"hot","fees":{"fixed":0,"percentage":0.5},"limits":{"daily_withdraw":50000}}', 
 1, 'USDT', 0);