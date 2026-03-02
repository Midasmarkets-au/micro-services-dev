drop view if exists trade_view_2023;
create definer = snps@`%` view trade_view_2023 as
    select
        o.`Deal`                                                        AS `TICKET`,
        o.`Login`                                                       AS `LOGIN`,
        o.`Symbol`                                                      AS `SYMBOL`,
        o.`Digits`                                                      AS `DIGITS`,
        o.`Action`                                                      AS `CMD`,
        o.`Volume`                                                      AS `VOLUME`,
        o.`Time`                                                        AS `OPEN_TIME`,
        o.`Price`                                                       AS `OPEN_PRICE`,
        COALESCE(c.`PriceSL`, o.`PriceSL`, CAST(0 as DECIMAL(8,2)))     AS `SL`,
        COALESCE(c.`PriceTP`, o.`PriceTP`, CAST(0 as DECIMAL(8,2)))     AS `TP`,
        COALESCE(c.`Time`, CAST('1970-01-01 00:00:00' as datetime))     AS `CLOSE_TIME`,
        CAST('1970-01-01 00:00:00' as datetime)                         AS `EXPIRATION`,
        COALESCE( o.`Reason`, c.`Reason`)                               AS `REASON`,
        CAST(0 as DECIMAL(8,2))                                         AS `CONV_RATE1`,
        CAST(0 as DECIMAL(8,2))                                         AS `CONV_RATE2`,
        COALESCE(c.`Commission`, CAST(0 as DECIMAL(8,2)))               AS `COMMISSION`,
        COALESCE(c.`Dealer`, 0)                                         AS `COMMISSION_AGENT`,
        CAST(0 as DECIMAL(8,2))                                         AS `SWAPS`,
        COALESCE(c.`Price`, CAST(0 as DECIMAL(8,2)))                    AS `CLOSE_PRICE`,
        COALESCE(c.`Profit`, o.`Profit`, CAST(0 as DECIMAL(8,2)))       AS `PROFIT`,
        COALESCE(c.`Fee`, CAST(0 as DECIMAL(8,2)))                      AS `TAXES`,
        CONTACT(o.`Comment`, '|', c.`Comment`)                          AS `COMMENT`,
        o.`PositionID`                                                  AS `INTERNAL_ID`,
        o.`RateMargin`                                                  AS `MARGIN_RATE`,
        COALESCE(c.`Timestamp`, o.`Timestamp`, 0)                       AS `TIMESTAMP`,
        CAST(0 as DECIMAL(8,2))                                         AS `MAGIC`,
        CAST(0 as DECIMAL(8,2))                                         AS `GW_VOLUME`,
        CAST(0 as DECIMAL(8,2))                                         AS `GW_OPEN_PRICE`,
        CAST(0 as DECIMAL(8,2))                                         AS `GW_CLOSE_PRICE`,
        COALESCE(c.`Time`, o.`Time`)                                    AS `MODIFY_TIME`
    from `mt5_deals_2023`      as o
    left join `mt5_deals_2023` as c
        on o.`PositionID` = c.`PositionID`
            and c.`PositionID`   > 0
            and c.`VolumeClosed` > 0
    where o.`VolumeClosed` = 0
    ;
