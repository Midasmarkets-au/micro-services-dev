alter table trd."_ExchangeRate"
    alter column "BuyingRate" type numeric(16, 8) using "BuyingRate"::numeric(16, 8);

alter table trd."_ExchangeRate"
    alter column "SellingRate" type numeric(16, 8) using "SellingRate"::numeric(16, 8);

alter table trd."_ExchangeRate"
    alter column "AdjustRate" type numeric(16, 8) using "AdjustRate"::numeric(16, 8);
