-- Migration 04: Drop stale foreign key constraints on trd."_SalesRebate"
--
-- Context:
--   The scheduler service writes SalesRebate records using trade_rebate_k8s IDs
--   (the new partitioned table). The old FK "_SalesRebate_TradeRebateId_fkey"
--   points to trd."_TradeRebate" (legacy table), causing insert failures.
--
--   "_SalesRebate_WalletAdjustId_fkey" is also removed because the column
--   defaults to 0, which does not exist in trd."_WalletAdjust", causing
--   every insert to fail.
--
-- Applied manually on: 2026-05-27 (portal_tenant_bvi)
-- Must be re-applied on any new tenant database.

ALTER TABLE trd."_SalesRebate"
  DROP CONSTRAINT IF EXISTS "_SalesRebate_TradeRebateId_fkey";

ALTER TABLE trd."_SalesRebate"
  DROP CONSTRAINT IF EXISTS "_SalesRebate_WalletAdjustId_fkey";

-- wallet_transaction_k8s: matter_id was wrongly constrained to core.matter_k8s(id),
-- but sales_rebate release uses sales_rebate_k8s.id (sequential int) as matter_id.
ALTER TABLE acct.wallet_transaction_k8s
  DROP CONSTRAINT IF EXISTS fk_wallet_transaction_k8s_matter_id;
