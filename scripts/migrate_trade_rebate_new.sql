-- 在每个租户库中执行
-- BCRTrade Rust 实现的校验表，与原 trd."_TradeRebate" 并存，用于对比验证

CREATE TABLE IF NOT EXISTS trd."_TradeRebateNew" (
    "Id"             BIGSERIAL        PRIMARY KEY,
    "AccountId"      BIGINT,
    "TradeServiceId" INT              NOT NULL,
    "Ticket"         BIGINT           NOT NULL,
    "AccountNumber"  BIGINT           NOT NULL,
    "CurrencyId"     INT              NOT NULL DEFAULT -1,
    "Volume"         INT              NOT NULL DEFAULT 0,
    "Status"         INT              NOT NULL DEFAULT 0,
    "RuleType"       INT              NOT NULL DEFAULT 199,
    "CreatedOn"      TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    "UpdatedOn"      TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    "ClosedOn"       TIMESTAMPTZ      NOT NULL,
    "OpenedOn"       TIMESTAMPTZ      NOT NULL,
    "TimeStamp"      BIGINT           NOT NULL DEFAULT 0,
    "Action"         INT              NOT NULL DEFAULT 0,
    "DealId"         BIGINT           NOT NULL DEFAULT 0,
    "Symbol"         VARCHAR(32)      NOT NULL DEFAULT '',
    "ReferPath"      VARCHAR(512)     NOT NULL DEFAULT '',
    "Commission"     DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Swaps"          DOUBLE PRECISION NOT NULL DEFAULT 0,
    "OpenPrice"      DOUBLE PRECISION NOT NULL DEFAULT 0,
    "ClosePrice"     DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Profit"         DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Reason"         INT              NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX__TradeRebateNew_Ticket_ServiceId"
    ON trd."_TradeRebateNew" ("Ticket", "TradeServiceId");

CREATE INDEX IF NOT EXISTS "IX__TradeRebateNew_AccountNumber"
    ON trd."_TradeRebateNew" ("AccountNumber");
